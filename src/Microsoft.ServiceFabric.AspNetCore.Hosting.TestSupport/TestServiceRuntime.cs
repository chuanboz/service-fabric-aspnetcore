// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.AspNetCore.TestRuntime;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class TestServiceRuntime : IStatelessServiceRuntime, IServiceProviderAdapter
    {
        private readonly StatelessServiceContext statelessContext;
        private readonly StatefulServiceContext statefulContext;
        private IServiceProvider replicaServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceRuntime"/> class.
        /// </summary>
        public TestServiceRuntime(IServiceProvider serviceProvider, StatelessServiceContext statelessContext, StatefulServiceContext statefulContext)
        {
            this.statelessContext = statelessContext;
            this.statefulContext = statefulContext;
            this.HostServices = serviceProvider;
        }

        /// <summary>
        /// Gets the service provider from the host.
        /// </summary>
        public IServiceProvider HostServices { get; }

        /// <summary>
        /// Gets the service provider from the service replica after service is started.
        /// </summary>
        public IServiceProvider ReplicaServices
        {
            get
            {
                if (this.replicaServices == null)
                {
                    throw new InvalidOperationException("Services replica is not started yet, please start the host via IHost.Start()");
                }

                return this.replicaServices;
            }
        }

        public TestClusterInfo ClusterInfo { get; set; } = new TestClusterInfo();

        async Task IStatelessServiceRuntime.RegisterServiceAsync(
            string serviceTypeName,
            Func<StatelessServiceContext, StatelessService> serviceFactory,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            if (serviceTypeName == null)
            {
                throw new ArgumentNullException(nameof(serviceTypeName));
            }

            var service = serviceFactory(this.statelessContext);

            var listeners = TestHelper.InvokeMember<IEnumerable<ServiceInstanceListener>>(service, "CreateServiceInstanceListeners");

            foreach (var item in listeners)
            {
                var com = item.CreateCommunicationListener(this.statelessContext);
                var address = await com.OpenAsync(cancellationToken);

                var task = TestHelper.InvokeMember<Task>(service, "OnOpenAsync", cancellationToken);

                await task;

                // append the final slash so it work for HttpSys that has sub prefix as service name
                // see https://stackoverflow.com/questions/20609118/httpclient-with-baseaddress
                address = address + "/";

                Console.WriteLine($"Service {serviceTypeName} started listening at {address}");

                // for test, register as the current hostname
                this.ClusterInfo.RegisterService(this.statelessContext.CodePackageActivationContext.ApplicationName, serviceTypeName, new Uri(address.Replace("*", Dns.GetHostName())));
            }

            this.ClusterInfo.SaveChanges();
        }

        /// <inheritdoc/>
        IServiceProvider IServiceProviderAdapter.CreateReplicaServiceProvider(IHost host, ServiceContext serviceContext)
        {
            var hostServiceProvider = host.Services;
            var adapter = hostServiceProvider.GetRequiredService<IServiceProviderAdapter>();
            this.replicaServices = adapter.CreateReplicaServiceProvider(host, serviceContext);
            return this.replicaServices;
        }
    }
}
