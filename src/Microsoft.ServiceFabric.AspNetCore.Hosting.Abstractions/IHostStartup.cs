// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Represents platform specific configuration that will be applied to a <see cref="IHostBuilder"/> when building an <see cref="IHost"/>.
    /// </summary>
    /// <remarks>
    /// see [Add support for IHostingStartup/IHostStartup in generic host]
    /// https://github.com/aspnet/Hosting/issues/1403.
    /// </remarks>
    public interface IHostStartup
    {
        /// <summary>
        /// Configure the <see cref="IHostBuilder"/>.
        /// </summary>
        /// <remarks>
        /// Configure is intended to be called before user code, allowing a user to overwrite any changes made.
        /// </remarks>
        /// <param name="builder">the host builder to be extended by platform specific logic.</param>
        void Configure(IHostBuilder builder);
    }
}
