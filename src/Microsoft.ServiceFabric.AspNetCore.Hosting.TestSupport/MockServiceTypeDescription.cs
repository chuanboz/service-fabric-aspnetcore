using System;
using System.Collections.Generic;
using System.Fabric.Description;
using System.Text;

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    public class TestServiceTypeDescription : ServiceTypeDescription
    {
        public TestServiceTypeDescription(ServiceDescriptionKind kind) : base(kind)
        {
        }
    }
}
