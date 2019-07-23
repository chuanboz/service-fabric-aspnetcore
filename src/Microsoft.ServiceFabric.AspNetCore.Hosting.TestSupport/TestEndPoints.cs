using System;
using System.Collections.ObjectModel;
using System.Fabric.Description;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    internal class TestEndPoints : KeyedCollection<string, EndpointResourceDescription>
    {
        private IConfiguration config;
        private XElement manifest;

        public TestEndPoints(IConfiguration config, XElement manifest)
        {
            this.config = config;
            this.manifest = manifest;

            this.config = config;

            if (manifest != null)
            {
                XNamespace ns = manifest.Name.Namespace;

                foreach (var item in manifest.Descendants(ns + "Endpoint"))
                {
                    // TODO, FIX THE KIND
                    var endpoint = new EndpointResourceDescription()
                    {
                        Name = item.Attribute(nameof(EndpointResourceDescription.Name)).Value,
                        EndpointType = EndpointType.Input, // item.Attribute(nameof(EndpointResourceDescription.EndpointType)).Value,
                        IpAddressOrFqdn = item.Attribute(nameof(EndpointResourceDescription.IpAddressOrFqdn))?.Value,
                        //Port = int.Parse(item.Attribute(nameof(EndpointResourceDescription.Port)).Value),
                        Protocol = (EndpointProtocol)Enum.Parse(typeof(EndpointProtocol), item.Attribute(nameof(EndpointResourceDescription.Protocol)).Value, true)
                    };

                    this.Add(endpoint);
                }
            }
        }

        protected override string GetKeyForItem(EndpointResourceDescription item)
        {
            return item.Name;
        }
    }
}