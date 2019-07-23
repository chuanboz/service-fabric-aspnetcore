// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    using System.Collections.ObjectModel;
    using System.Fabric.Description;

    internal class MockConfigurationSections : KeyedCollection<string, ConfigurationSection>
    {
        public static MockConfigurationSections Default { get; } = CreateDefault();

        protected override string GetKeyForItem(ConfigurationSection item)
        {
            return item.Name;
        }

        private static MockConfigurationSections CreateDefault()
        {
            var sections = new MockConfigurationSections();
            var section = TestHelper.CreateInstanced<ConfigurationSection>();
            section.Set("Name", "SectionName1");
            section.Set(nameof(ConfigurationSection.Parameters), MockConfigurationProperties.CreateDefault());
            sections.Add(section);
            return sections;
        }
    }
}
