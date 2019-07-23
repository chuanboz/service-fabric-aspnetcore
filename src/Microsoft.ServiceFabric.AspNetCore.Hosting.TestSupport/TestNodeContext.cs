using Microsoft.Extensions.Configuration;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
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