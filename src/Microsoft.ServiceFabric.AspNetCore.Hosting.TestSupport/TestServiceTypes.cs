using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Fabric.Description;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    public class TestServiceTypes : KeyedCollection<string, ServiceTypeDescription>
    {
        private IConfiguration config;

        public TestServiceTypes(IConfiguration config, XElement manifest)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            if (manifest != null)
            {
                XNamespace ns = manifest.Name.Namespace;

                var serviceTypes = manifest.Element(ns + "ServiceTypes") ?? throw new InvalidOperationException("ServiceTypes not found in service manifest");

                foreach (var item in serviceTypes.Elements())
                {
                    // TODO, FIX THE KIND
                    var desc = new TestServiceTypeDescription(ServiceDescriptionKind.Stateless)
                    {
                        ServiceTypeName = item.Attribute(nameof(TestServiceTypeDescription.ServiceTypeName)).Value
                    };

                    this.Add(desc);
                }
            }
            else if(config["ServiceTypeNames"] != null)
            {
                foreach (var item in config["ServiceTypeNames"].Split(','))
                {
                    // TODO, FIX THE KIND
                    var desc = new TestServiceTypeDescription(ServiceDescriptionKind.Stateless)
                    {
                        ServiceTypeName = item
                    };

                    this.Add(desc);
                }
            }
            else
            {
                throw new InvalidOperationException($"serviceManifest.xml not exists and ServiceTypes not found in IConfiguration: {AppContext.BaseDirectory}PackageRoot\\ServiceManifest.xml");
            }
        }

        protected override string GetKeyForItem(ServiceTypeDescription item)
        {
            return item.ServiceTypeName;
        }
    }
}
