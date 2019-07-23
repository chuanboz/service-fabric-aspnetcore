// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// The interface for the service runtime to register service factory.
    /// </summary>
    public interface IStatelessServiceRuntime
    {
        /// <summary>
        /// Register given service type for a factory to create service instance.
        /// </summary>
        /// <param name="serviceTypeName">the type name of the service.</param>
        /// <param name="serviceFactory">the factory to create new service instance.</param>
        /// <param name="timeout">timeout to wait for the service startup.</param>
        /// <param name="cancellationToken">cancellation token.</param>
        /// <returns>the task.</returns>
        Task RegisterServiceAsync(string serviceTypeName, Func<StatelessServiceContext, StatelessService> serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default);
    }
}
