// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    using System;
    using System.Fabric;
    using System.Fabric.Description;

    internal static class MockConfigurationPackage
    {
        internal static ConfigurationPackage CreateDefaultPackage(string packageName)
        {
            var package = TestHelper.CreateInstanced<ConfigurationPackage>();
            var settings = TestHelper.CreateInstanced<ConfigurationSettings>();
            var basePath = AppContext.BaseDirectory;
            package.Set("Settings", settings);
            package.Set("Path", $"{basePath}\\PackageRoot\\Config\\");

            var section = TestHelper.CreateInstanced<ConfigurationSection>();
            settings.Set(nameof(ConfigurationSettings.Sections), MockConfigurationSections.Default);

            return package;
        }
    }
}
