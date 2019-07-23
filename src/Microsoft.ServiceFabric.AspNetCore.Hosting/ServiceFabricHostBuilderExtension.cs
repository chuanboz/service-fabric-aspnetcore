// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Fabric;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// public extensions for ServiceFabricHost as the public contract.
    /// </summary>
    public static class ServiceFabricHostBuilderExtension
    {
        /// <summary>
        /// Register ServiceFabricHost to the DI containers.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <returns>The host builder with ServiceFabricHost register as IHost.</returns>
        public static IHostBuilder UseServiceFabricHost(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseServiceProviderFactory(new DefaultServiceProviderFactory());

            // record the final host service collection to be used later to create service instance
            hostBuilder.ConfigureContainer<IServiceCollection>((hostBuilderContext, serviceCollection) =>
            {
                hostBuilderContext.Properties.Add(ServiceFabricHost.HostServiceCollectionKey, serviceCollection);
            });

            hostBuilder.ConfigureServices(s =>
            {
                // DO NOT use TryAddSingleton here as the intention is to replace the default implementation
                s.AddSingleton<IHost, ServiceFabricHost>();

                // register as transient because each service instance shall have different WebHostBuilder
                s.TryAddTransient<IWebHostBuilder, WebHostBuilder>();

                s.TryAddSingleton<IServiceProviderAdapter, WebHostServiceProviderAdapter>();

                // There's no way to to register multiple service types per definition. See https://github.com/aspnet/DependencyInjection/issues/360
                s.TryAddSingleton<Microsoft.AspNetCore.Hosting.IApplicationLifetime, Microsoft.AspNetCore.Hosting.Internal.ApplicationLifetime>();
                s.TryAddSingleton(sp => sp.GetRequiredService<Microsoft.AspNetCore.Hosting.IApplicationLifetime>() as Extensions.Hosting.IApplicationLifetime);
            });

            return hostBuilder;
        }

        /// <summary>
        /// Configure the WebHost for the service instance.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="configureWebHost">The configuration actions.</param>
        /// <returns>The host builder with WebHost action configured.</returns>
        public static IHostBuilder ConfigureWebHost(this IHostBuilder hostBuilder, Action<IWebHostBuilder> configureWebHost)
        {
            return hostBuilder.ConfigureServices((bulderContext, services) =>
            {
                services.AddSingleton<IHostingStartup>(implementationInstance: new WebHostStartup(configureWebHost));
                services.Configure<ServiceFabricHostOptions>(options => options.ConfigureWebHost = configureWebHost);
            });
        }

        /// <summary>
        /// Register the real service fabric service runtime.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <returns>The host builder with real service fabric runtime registered.</returns>
        public static IHostBuilder UseServiceFabricRuntime(this IHostBuilder hostBuilder)
        {
            ICodePackageActivationContext activationContext = FabricRuntime.GetActivationContext();

            // DO NOT dispose activation context as it will be saved to DI container and used later.
            // using (activationContext)
            {
                var nodeContext = FabricRuntime.GetNodeContext();
                return hostBuilder.UseServiceFabricRuntime(activationContext, nodeContext);
            }
        }

        /// <summary>
        /// Register the real service fabric service runtime.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="activationContext">the activation context.</param>
        /// <param name="nodeContext">the node context.</param>
        /// <returns>The host builder with real service fabric runtime registered.</returns>
        public static IHostBuilder UseServiceFabricRuntime(this IHostBuilder hostBuilder, ICodePackageActivationContext activationContext, NodeContext nodeContext)
        {
            var serviceTypes = activationContext.GetServiceTypes();
            if (serviceTypes.Count > 1)
            {
                throw new NotSupportedException($"Only 1 ServiceType supported but found {serviceTypes.Count} declared.");
            }

            if (nodeContext == null)
            {
                throw new ArgumentNullException(nameof(nodeContext));
            }

            hostBuilder.ConfigureServices(s =>
            {
                // use try add so that test implementation could override it before hand
                s.TryAddSingleton<IStatelessServiceRuntime, ServiceFabricServiceRuntime>();
                s.TryAddSingleton(activationContext);
                s.TryAddSingleton(nodeContext);
             });

            return hostBuilder;
        }

        internal class WebHostStartup : IHostingStartup
        {
            private readonly Action<IWebHostBuilder> configureWebHost;

            public WebHostStartup(Action<IWebHostBuilder> configureWebHost)
            {
                this.configureWebHost = configureWebHost;
            }

            public void Configure(IWebHostBuilder builder)
            {
                this.configureWebHost(builder);
            }
        }
    }
}
