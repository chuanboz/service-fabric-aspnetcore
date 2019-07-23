// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    using System.Collections.ObjectModel;
    using System.Fabric.Description;

    internal class MockConfigurationProperties : KeyedCollection<string, ConfigurationProperty>
    {
        internal static MockConfigurationProperties CreateDefault()
        {
            var parameters = new MockConfigurationProperties();
            var parameter = TestHelper.CreateInstanced<ConfigurationProperty>();
            parameter.Set("Name", "PropertyName1");
            parameter.Set("Value", "PropertyValue1");
            parameters.Add(parameter);

            return parameters;
        }

        protected override string GetKeyForItem(ConfigurationProperty item)
        {
            return item.Name;
        }
    }
}
