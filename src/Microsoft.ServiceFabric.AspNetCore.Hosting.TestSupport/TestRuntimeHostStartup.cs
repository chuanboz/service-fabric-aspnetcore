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
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.AspNetCore.Hosting;

    internal class TestRuntimeHostStartup : IHostStartup
    {
        public void Configure(IHostBuilder hostBuilder)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("activationContext.default.json", optional: true);
            configBuilder.AddJsonFile("activationContext.json", optional: true);
            configBuilder.AddEnvironmentVariables();

            var config = configBuilder.Build();

            var context = new TestCodePackageActivationContext(config);
            NodeContext nodeContext = new TestNodeContext(config);
            hostBuilder.UseServiceFabricRuntime(context, nodeContext);

            hostBuilder.ConfigureServices(sc => sc.AddSingleton<IStatelessServiceRuntime, TestServiceRuntime>());
        }
    }
}
