// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Options for <see cref="ServiceFabricHost"/>.
    /// </summary>
    public class ServiceFabricHostOptions : HostOptions
    {
        /// <summary>
        /// Gets the name of endpoint used by the communication listener to obtain the port on which to listen.
        /// </summary>
        public string EndpointName { get; } = "ServiceEndpoint";

        /// <summary>
        /// Gets the action to configure the WebHosts.
        /// </summary>
        public Action<IWebHostBuilder> ConfigureWebHost { get; internal set; }

        /// <summary>
        /// Gets the WebHost associated with the service instances.
        /// </summary>
        public IWebHost WebHost { get; internal set; }
    }
}
