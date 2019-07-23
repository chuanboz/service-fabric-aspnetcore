// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    using System;
    using System.Fabric;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.AspNetCore.Hosting;

    /// <summary>
    /// Public extensions for test support.
    /// </summary>
    public static class TestRuntimeHostBuilderExtensions
    {
        /// <summary>
        /// Register with the test implementation of service runtime.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <returns>The host builder that has the test runtime.</returns>
        public static IHostBuilder UseServiceFabricTestRuntime(this IHostBuilder hostBuilder)
        {
            // for embeded file, no need to have base path
            var embedded = new EmbeddedFileProvider(typeof(TestRuntimeHostBuilderExtensions).Assembly);
            var file = embedded.GetFileInfo("Config.fabricSettings.default.json");

            var cb = new ConfigurationBuilder()
                .AddJsonFile(embedded, file.Name, optional: true, reloadOnChange: false)
                .AddJsonFile("hostSettings.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = cb.Build();
            var context = new TestCodePackageActivationContext(config);
            NodeContext nodeContext = new TestNodeContext(config);
            var statelessContext = new StatelessServiceContext(nodeContext, context, "serviceTypeName", new Uri("http://localhost"), null, default, default);
            var statefulContext = new StatefulServiceContext(nodeContext, context, "serviceTypeName", new Uri("http://localhost"), null, default, default);

            hostBuilder.UseServiceFabricRuntime(context, nodeContext);

            hostBuilder.ConfigureHostConfiguration(cb2 => cb2.AddConfiguration(config));

            hostBuilder.ConfigureServices((builderContext, sc) =>
            {
                sc.AddSingleton<TestServiceRuntime>();
                sc.AddSingleton<IStatelessServiceRuntime>(sp => sp.GetRequiredService<TestServiceRuntime>());
                sc.AddSingleton<IServiceProviderAdapter>(sp => sp.GetRequiredService<TestServiceRuntime>());
                sc.AddSingleton(statelessContext);
                sc.AddSingleton(statefulContext);
            });

            return hostBuilder;
        }
    }
}
