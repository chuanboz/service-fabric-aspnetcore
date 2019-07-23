// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    internal class TestClusterInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClusterInfo"/> class.
        /// </summary>
        public TestClusterInfo()
        {
        }

        // key is the serviceName, value is the appName
        public IDictionary<string, IList<string>> Apps { get; set; } = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

        // key is ServiceName.AppName, value is the uri for this app/service
        public IDictionary<string, IList<Uri>> Endpoints { get; set; } = new Dictionary<string, IList<Uri>>(StringComparer.OrdinalIgnoreCase);

        public string RunTimePath
        {
            get
            {
                var path = Environment.GetEnvironmentVariable("E2ETestRunTimePath");
                if (string.IsNullOrEmpty(path))
                {
                    // use 5f to avoid conflict when multiple tests run in similar times
                    path = Path.Combine(AppContext.BaseDirectory, "E2ETestRuntime", DateTime.Now.ToString("HHmmss.fffff"));

                    Console.WriteLine($"Created E2ETestRunTimePath: {path}");
                    Environment.SetEnvironmentVariable("E2ETestRunTimePath", path);
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        internal void RegisterService(string applicationName, string serviceTypeName, Uri address)
        {
            var serviceName = serviceTypeName.EndsWith("Type", StringComparison.OrdinalIgnoreCase) ? serviceTypeName.Substring(0, serviceTypeName.Length - 4) : serviceTypeName;

            // first save app info
            if (!this.Apps.ContainsKey(serviceName))
            {
                this.Apps[serviceName] = new List<string>() { applicationName };
            }

            var key = $"{serviceName}.{applicationName}";
            if (!this.Endpoints.ContainsKey(key))
            {
                var service = new List<Uri>();
                service.Add(address);
                this.Endpoints[key] = service;
            }
        }

        internal void LoadChange()
        {
            // first load the apps
            foreach (var item in Directory.EnumerateFiles(this.RunTimePath, "*.apps.json", SearchOption.TopDirectoryOnly))
            {
                // save information to disk for E2E tests
                // serialize JSON to a string and then write string to a file
                var json = File.ReadAllText(item);
                var serviceName = new FileInfo(item).Name.Replace(".apps.json", string.Empty);
                var apps = JsonConvert.DeserializeObject<IList<string>>(json);

                if (!this.Apps.ContainsKey(serviceName))
                {
                    this.Apps.Add(serviceName, new List<string>());
                }

                var list = this.Apps[serviceName];
                foreach (var item2 in apps)
                {
                    list.Add(item2);
                }

                Console.WriteLine($"Load service {serviceName} apps from {item}:\n {json}");
            }

            // then load the endpoints
            foreach (var item in Directory.EnumerateFiles(this.RunTimePath, "*.endpoints.json", SearchOption.TopDirectoryOnly))
            {
                // save information to disk for E2E tests
                // serialize JSON to a string and then write string to a file
                var json = File.ReadAllText(item);
                var serviceNameWithApp = new FileInfo(item).Name.Replace(".endpoints.json", string.Empty);
                var address = JsonConvert.DeserializeObject<IList<Uri>>(json);

                if (!this.Endpoints.ContainsKey(serviceNameWithApp))
                {
                    this.Endpoints.Add(serviceNameWithApp, new List<Uri>());
                }

                var list = this.Endpoints[serviceNameWithApp];
                foreach (var item2 in address)
                {
                    list.Add(item2);
                }

                Console.WriteLine($"Load service {serviceNameWithApp} endpoints from {item}:\n {json}");
            }
        }

        internal void SaveChanges()
        {
            foreach (var item in this.Apps)
            {
                // save information to disk for E2E tests
                // serialize JSON to a string and then write string to a file
                var json = JsonConvert.SerializeObject(item.Value);
                var path = Path.Combine(this.RunTimePath, $"{item.Key}.apps.json");

                Console.WriteLine($"Save service {item.Key} apps to {path}:\n {json}");
                File.WriteAllText(path, json);
            }

            foreach (var item in this.Endpoints)
            {
                // save information to disk for E2E tests
                // serialize JSON to a string and then write string to a file
                var json = JsonConvert.SerializeObject(item.Value);
                var path = Path.Combine(this.RunTimePath, $"{item.Key}.endpoints.json");

                Console.WriteLine($"Save service {item.Key} endpoints to {path}:\n {json}");
                File.WriteAllText(path, json);
            }
        }
    }
}
