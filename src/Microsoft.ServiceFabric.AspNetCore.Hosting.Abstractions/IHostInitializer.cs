// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Represents initialization that will be applied to a <see cref="IHostLifetime"/> when building an <see cref="IHost"/>.
    /// </summary>
    /// <remarks>
    /// see [Support multiple IHostLifetime in Host class], this class supports multiple instances.
    /// https://github.com/aspnet/Hosting/issues/1401.
    /// </remarks>
    public interface IHostInitializer
    {
        /// <summary>
        /// initialize asynchronous before <see cref="IHostLifetime"/> WaitForStartAsync.
        /// </summary>
        /// <param name="cancellationToken">the cancellation token to cancel the task.</param>
        /// <returns>The task to wait for finish.</returns>
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
