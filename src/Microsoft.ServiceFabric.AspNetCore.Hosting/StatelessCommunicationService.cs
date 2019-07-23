// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;

    /// <summary>
    /// internal class that act as a bridge between Service Fabric factory and IHost.
    /// </summary>
    public class StatelessCommunicationService : StatelessService
    {
        private readonly ICommunicationListener listener;
        private readonly ServiceFabricHostOptions options;
        private readonly ApplicationLifetime applicationLifetime;
        private readonly ILogger<StatelessCommunicationService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatelessCommunicationService"/> class.
        /// </summary>
        /// <param name="serviceContext">the service context.</param>
        /// <param name="listener">The communication listener.</param>
        /// <param name="applicationLifetime">The application lifetime.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The host options.</param>
        public StatelessCommunicationService(StatelessServiceContext serviceContext, ICommunicationListener listener, IApplicationLifetime applicationLifetime, ILogger<StatelessCommunicationService> logger, IOptions<ServiceFabricHostOptions> options)
            : base(serviceContext)
        {
            this.listener = listener ?? throw new ArgumentNullException(nameof(listener));
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.applicationLifetime = (applicationLifetime as ApplicationLifetime) ?? throw new ArgumentNullException(nameof(applicationLifetime));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal IStatelessServicePartition ServicePartition => this.Partition;

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext => this.listener),
            };
        }

        /// <inheritdoc/>
        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(new EventId(101, "OnOpenAsyncStart"), "StatelessCommunicationService OnOpenAsync start.");

            // invoke the base will register the communication listener
            await base.OnOpenAsync(cancellationToken);

            // Fire IApplicationLifetime.Started
            this.applicationLifetime.NotifyStarted();

            this.logger.LogInformation(new EventId(102, "OnOpenAsyncEnd"), "StatelessCommunicationService OnOpenAsync end.");
        }

        /// <inheritdoc/>
        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation(new EventId(103, "OnCloseAsyncStart"), "StatelessCommunicationService OnCloseAsync start with shutdown timeout: {ShutdownTimeout}.", this.options.ShutdownTimeout);
                await base.OnCloseAsync(cancellationToken);

                // use linked token to support both shutdown time out and cancellation
                using (var cts = new CancellationTokenSource(this.options.ShutdownTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                {
                    var token = linkedCts.Token;

                    // Trigger IApplicationLifetime.ApplicationStopping
                    this.applicationLifetime.StopApplication();

                    token.ThrowIfCancellationRequested();

                    // Fire IApplicationLifetime.Stopped
                    this.applicationLifetime.NotifyStopped();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical(new EventId(105, "OnCloseAsyncException"), "Hosting shutdown exception: {Exception}", ex);

                // throw;
            }
            finally
            {
                this.logger.LogInformation(new EventId(104, "OnCloseAsyncEnd"), "StatelessCommunicationService OnCloseAsync end.");
            }
        }
    }
}
