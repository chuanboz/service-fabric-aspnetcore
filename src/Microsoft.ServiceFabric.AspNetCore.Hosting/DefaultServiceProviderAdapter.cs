// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Fabric;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class DefaultServiceProviderAdapter : IServiceProviderAdapter
    {
        public IServiceProvider CreateReplicaServiceProvider(IHost host, ServiceContext context)
        {
            var hostServiceProvider = host.Services;
            IServiceCollection sc = new ServiceCollection();

            // populate required service from DI.
            var hostBuilderContext = hostServiceProvider.GetRequiredService<HostBuilderContext>();

            // link the configuration from host and apply it first
            sc.AddSingleton(hostBuilderContext.Configuration);

            // link the service from the host and apply it first
            var hostCollection = hostBuilderContext.Properties[ServiceFabricHost.HostServiceCollectionKey] as IServiceCollection;

            foreach (var service in hostCollection)
            {
                sc.Add(service);
            }

            // add the service context
            sc.AddSingleton(context);
            sc.AddSingleton(context.CodePackageActivationContext);
            sc.AddSingleton<StatelessService, StatelessCommunicationService>();

            return sc.BuildServiceProvider();
        }
    }
}
