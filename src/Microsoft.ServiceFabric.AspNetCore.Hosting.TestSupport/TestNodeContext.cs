// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    using System.Fabric;
    using Microsoft.Extensions.Configuration;

    internal class TestNodeContext
    {
        private IConfiguration config;

        public TestNodeContext(IConfiguration config)
        {
            this.config = config;
        }

        public static implicit operator NodeContext(TestNodeContext package)
        {
            var config = package.config.GetSection(nameof(NodeContext));
            return new NodeContext(config[nameof(NodeContext.NodeName)], new NodeId(0, 0), 0, config[nameof(NodeContext.NodeType)], config[nameof(NodeContext.IPAddressOrFQDN)]);
        }
    }
}
