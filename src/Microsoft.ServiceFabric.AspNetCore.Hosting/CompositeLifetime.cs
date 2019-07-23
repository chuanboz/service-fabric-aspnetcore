// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Hosting.Internal;

    internal class CompositeLifetime : IHostLifetime
    {
        private readonly ConsoleLifetime consoleLifetime;
        private readonly IEnumerable<IHostInitializer> initializers;
        private readonly IApplicationLifetime applicationLifetime;

        public CompositeLifetime(ConsoleLifetime lifetime, IEnumerable<IHostInitializer> initializers, IApplicationLifetime applicationLifetime)
        {
            this.consoleLifetime = lifetime;
            this.initializers = initializers;
            this.applicationLifetime = applicationLifetime;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.applicationLifetime.StopApplication();
            await this.consoleLifetime.StopAsync(cancellationToken);
        }

        public async Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            // initialize first before calling lifetime
            foreach (var item in this.initializers)
            {
                await item.InitializeAsync(cancellationToken);
            }

            await this.consoleLifetime.WaitForStartAsync(cancellationToken);
        }
    }
}
