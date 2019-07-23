// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Fabric;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Core interface to translate host service provider to service instance service provider.
    /// </summary>
    /// <remarks>
    /// Depends on specific implementation, the service instance could share the same DI container as the host,
    /// or have completed separated DI container.
    /// </remarks>
    public interface IServiceProviderAdapter
    {
        /// <summary>
        /// translate the host service provider to service replica (partition for statefull service or instance for stateless service) service provider.
        /// </summary>
        /// <param name="host">the generic host.</param>
        /// <param name="serviceContext">The service context used to create service replica.</param>
        /// <returns>the service provider used for service replica dependency resolution.</returns>
        IServiceProvider CreateReplicaServiceProvider(IHost host, ServiceContext serviceContext);
    }
}
