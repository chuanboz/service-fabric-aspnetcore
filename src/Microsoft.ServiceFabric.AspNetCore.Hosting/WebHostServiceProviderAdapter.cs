// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Fabric;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// The default implementation of adapter that create service instance in separate DI container.
    /// </summary>
    internal class WebHostServiceProviderAdapter : IServiceProviderAdapter
    {
        public WebHostServiceProviderAdapter()
        {
        }

        public IServiceProvider CreateReplicaServiceProvider(IHost host, ServiceContext context)
        {
            var hostServiceProvider = host.Services;

            // save the host configuration and logger factory to be used later in app during dependency registration in ConfigureServices.
            AppDomain.CurrentDomain.SetData(nameof(IServiceProvider), hostServiceProvider);
            AppDomain.CurrentDomain.SetData(nameof(IConfiguration), hostServiceProvider.GetRequiredService<IConfiguration>());
            AppDomain.CurrentDomain.SetData(nameof(ILoggerFactory), hostServiceProvider.GetRequiredService<ILoggerFactory>());

            var builder = CreateWebHostBuilder(host, context);
            var webHost = builder.Build();
            var options = webHost.Services.GetService<IOptions<ServiceFabricHostOptions>>();
            options.Value.WebHost = webHost;

            return webHost.Services;
        }

        private static IWebHostBuilder CreateWebHostBuilder(IHost host, ServiceContext context)
        {
            var hostServiceProvider = host.Services;

            // populate required service from DI.
            var hostBuilderContext = hostServiceProvider.GetRequiredService<HostBuilderContext>();

            var builder = hostServiceProvider.GetRequiredService<IWebHostBuilder>();

            // link the configuration from host and apply it first
            builder.ConfigureAppConfiguration((builderContext, config) => config.AddConfiguration(hostBuilderContext.Configuration));

            // link the service from the host and apply it first
            var hostServicesCollection = hostBuilderContext.Properties[ServiceFabricHost.HostServiceCollectionKey] as IServiceCollection;
            builder.ConfigureServices(sc =>
            {
                foreach (var service in hostServicesCollection)
                {
                    sc.Add(service);
                }

                // add the service context
                sc.AddSingleton(context);

                if (context is StatelessServiceContext statelessContext)
                {
                    sc.AddSingleton(statelessContext);
                }
                else
                {
                    sc.AddSingleton(context as StatefulServiceContext);
                }

                sc.AddSingleton(context.CodePackageActivationContext);
                sc.AddSingleton<StatelessCommunicationService>();
                sc.AddSingleton<StatelessService>(sp => sp.GetRequiredService<StatelessCommunicationService>());
                sc.AddSingleton(sp => sp.GetRequiredService<StatelessCommunicationService>().ServicePartition);
                sc.AddSingleton<ICommunicationListener, WebHostCommunicationListener>();
                sc.AddSingleton(builder); // add the builder itself to support test server which needs the builder
                sc.AddSingleton(host); // add host to control to stop the application
            });

            var startups = hostServiceProvider.GetServices<IHostingStartup>();
            foreach (var item in startups)
            {
                item.Configure(builder);
            }

            // use the same runtime instance for all service instances.
            builder.ConfigureServices(sc =>
            {
                sc.AddSingleton(hostServiceProvider.GetRequiredService<IStatelessServiceRuntime>());
                sc.AddSingleton(hostServiceProvider.GetRequiredService<IServiceProviderAdapter>());
            });

            return builder;
        }
    }
}
