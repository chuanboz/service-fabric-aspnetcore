// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    using System;
    using System.Fabric;
    using Microsoft.Extensions.Configuration;

    internal class TestCodePackage
    {
        private readonly IConfiguration config;

        public TestCodePackage(IConfiguration configuration)
        {
            this.config = configuration;
        }

        public static implicit operator CodePackage(TestCodePackage package)
        {
            var codePackage = TestHelper.CreateInstanced<CodePackage>();

            return codePackage;
        }
    }
}
