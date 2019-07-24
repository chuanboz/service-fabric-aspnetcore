// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    using System;
    using System.Collections.Generic;
    using System.Fabric.Description;
    using System.Text;

    internal class TestServiceTypeDescription : ServiceTypeDescription
    {
        public TestServiceTypeDescription(ServiceDescriptionKind kind)
            : base(kind)
        {
        }
    }
}
