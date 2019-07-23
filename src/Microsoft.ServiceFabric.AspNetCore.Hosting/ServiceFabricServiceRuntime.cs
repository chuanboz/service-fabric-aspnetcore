// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class ServiceFabricServiceRuntime : IStatelessServiceRuntime
    {
        public Task RegisterServiceAsync(string serviceTypeName, Func<StatelessServiceContext, StatelessService> serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return ServiceRuntime.RegisterServiceAsync(serviceTypeName, serviceFactory, timeout, cancellationToken);
        }

        public Task RegisterServiceAsync(string serviceTypeName, Func<StatefulServiceContext, StatefulServiceBase> serviceFactory, TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            return ServiceRuntime.RegisterServiceAsync(serviceTypeName, serviceFactory, timeout, cancellationToken);
        }
    }
}
