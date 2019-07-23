// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Fabric;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class ServiceFabricHost : IHost
    {
        internal const string HostServiceCollectionKey = "ServiceFabricHostServiceCollection";

        private readonly ILogger<ServiceFabricHost> logger;
        private readonly IHostLifetime hostLifetimes;
        private readonly ICodePackageActivationContext activationContext;
        private readonly string serviceTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFabricHost"/> class.
        /// </summary>
        /// <param name="services">the DI container.</param>
        /// <param name="logger">the Logger.</param>
        /// <param name="hostLifetime">the lifetime for host.</param>
        /// <param name="activationContext">the activation context.</param>
        public ServiceFabricHost(
            IServiceProvider services,
            ILogger<ServiceFabricHost> logger,
            IHostLifetime hostLifetime,
            ICodePackageActivationContext activationContext)
        {
            if (hostLifetime == null)
            {
                throw new ArgumentNullException(nameof(hostLifetime));
            }

            this.Services = services ?? throw new ArgumentNullException(nameof(services));

            var serviceTypes = activationContext.GetServiceTypes();
            if (serviceTypes.Count > 1)
            {
                throw new NotSupportedException($"Only 1 ServiceType supported but found {serviceTypes.Count} declared.");
            }

            this.activationContext = activationContext;
            this.serviceTypeName = serviceTypes.Single().ServiceTypeName;

            var context = services.GetRequiredService<HostBuilderContext>();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.hostLifetimes = hostLifetime;
        }

        /// <summary>
        /// Gets the application services DI container for the current active service instance.
        /// </summary>
        public IServiceProvider ApplicationServices { get; private set; }

        public IServiceProvider Services { get; }

        public IServiceCollection HostServiceCollection { get; }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation(new EventId(1, "HostStarting"), "ServiceFabricHost starting");

            try
            {
                // start the lifetime
                await this.hostLifetimes.WaitForStartAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var runtime = this.Services.GetRequiredService<IStatelessServiceRuntime>();

                this.logger.LogInformation(new EventId(2, "RegisterServiceAsync"), "ServiceFabricHost RegisterServiceAsync with runtime: {RuntimeType}", runtime.GetType().Name);

                // TODO, add Stateful service support with StatefulHostedService
                await runtime.RegisterServiceAsync(
                    this.serviceTypeName,
                    context =>
                        {
                            // create the DI container and use it to resolve the service replica.
                            var adapter = this.Services.GetRequiredService<IServiceProviderAdapter>();
                            var provider = adapter.CreateReplicaServiceProvider(this, context);
                            this.ApplicationServices = provider;
                            var service = provider.GetRequiredService<StatelessService>();

                            this.logger.LogInformation(new EventId(3, "ApplicationServicesCreated"), "ServiceFabricHost application service created from adapter: {ServiceProviderAdapter}", adapter.GetType().Name);

                            return service;
                        },
                    default,
                    cancellationToken);

                this.logger.LogInformation(new EventId(4, "WaitForApplicationStart"), "ServiceFabricHost Waiting for application to finish starting");

                var serviceProvider = this.ApplicationServices;

                // wait for the service to be started.
                while (serviceProvider == null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    serviceProvider = this.ApplicationServices;
                }

                // get the applicationLifetime to check service startup
                var applicationLifetime = serviceProvider.GetRequiredService<IApplicationLifetime>();

                cancellationToken.Register(
                    state =>
                {
                    this.logger.LogInformation(new EventId(5, "StopApplication"), "ServiceFabricHost: host cancelled, trigger to stop application");
                    ((IApplicationLifetime)state).StopApplication();
                },
                    applicationLifetime);

                var waitForStart = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                applicationLifetime.ApplicationStarted.Register(
                    obj =>
                {
                    var tcs = (TaskCompletionSource<object>)obj;
                    tcs.TrySetResult(null);
                    this.logger.LogInformation(new EventId(5, "ApplicationStarted"), "ServiceFabricHost: application started.");
                }, waitForStart);

                await waitForStart.Task;
                this.logger.LogInformation(new EventId(6, "HostStarted"), "ServiceFabricHost: host started.");
            }
            catch (Exception ex)
            {
                this.logger.LogCritical(new EventId(8, "HostStartException"), "ServiceFabricHost: StartAsync with exception: {Exception}", ex);
                throw;
            }
            finally
            {
                this.logger.LogInformation(new EventId(7, "HostStarted"), "ServiceFabricHost: end host start.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (this.ApplicationServices != null)
                {
                    var localAppServices = this.ApplicationServices;

                    var applicationLifetime = localAppServices.GetRequiredService<IApplicationLifetime>();
                    var listener = localAppServices.GetRequiredService<ICommunicationListener>();

                    applicationLifetime.StopApplication();
                    await listener.CloseAsync(cancellationToken);
                    this.logger.LogInformation(new EventId(9, "HostStopping"), "ServiceFabricHost: host stopping.");
                    await this.hostLifetimes.StopAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(new EventId(11, "HostStopException"), "ServiceFabricHost: StopAsync with exception: {Exception}", ex);

                // throw; // do not throw for StopAsync, otherwise upgrade or undeployment could get stuck
            }
            finally
            {
                this.logger.LogInformation(new EventId(10, "HostStopped"), "ServiceFabricHost: host stopped.");
            }
        }

        public void Dispose()
        {
            (this.Services as IDisposable)?.Dispose();
        }
    }
}
