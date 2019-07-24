// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.TestRuntime
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Fabric.Health;
    using System.IO;
    using System.Xml.Linq;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A test implementation of code package activation context that gets the data from IConfiguration from DI container.
    /// </summary>
    internal class TestCodePackageActivationContext : ICodePackageActivationContext
    {
        private readonly IConfiguration config;
        private readonly XElement manifest;

        private bool disposedValue = false; // To detect redundant calls

        public TestCodePackageActivationContext(IConfiguration config)
        {
            this.config = config;
            this.ApplicationName = config[nameof(this.ApplicationName)];
            this.ApplicationTypeName = config[nameof(this.ApplicationTypeName)];

            var manifestFile = "PackageRoot\\ServiceManifest.xml";

            if (File.Exists(manifestFile))
            {
                this.manifest = XElement.Load(manifestFile);
            }

            this.ServiceTypes = new TestServiceTypes(config, this.manifest);
            this.Endpoints = new TestEndPoints(config, this.manifest);
        }

#pragma warning disable CS0067
        public event EventHandler<PackageAddedEventArgs<CodePackage>> CodePackageAddedEvent;

        public event EventHandler<PackageModifiedEventArgs<CodePackage>> CodePackageModifiedEvent;

        public event EventHandler<PackageRemovedEventArgs<CodePackage>> CodePackageRemovedEvent;

        public event EventHandler<PackageAddedEventArgs<ConfigurationPackage>> ConfigurationPackageAddedEvent;

        public event EventHandler<PackageModifiedEventArgs<ConfigurationPackage>> ConfigurationPackageModifiedEvent;

        public event EventHandler<PackageRemovedEventArgs<ConfigurationPackage>> ConfigurationPackageRemovedEvent;

        public event EventHandler<PackageAddedEventArgs<DataPackage>> DataPackageAddedEvent;

        public event EventHandler<PackageModifiedEventArgs<DataPackage>> DataPackageModifiedEvent;

        public event EventHandler<PackageRemovedEventArgs<DataPackage>> DataPackageRemovedEvent;
#pragma warning restore CS0067

        /// <inheritdoc/>
        public string ApplicationName { get; set; }

        /// <inheritdoc/>
        public string ApplicationTypeName { get; set; }

        /// <inheritdoc/>
        public string CodePackageName { get; set; }

        public string CodePackageVersion { get; set; }

        public string ContextId { get; set; }

        public string LogDirectory { get; set; }

        public string TempDirectory { get; set; }

        public string WorkDirectory { get; set; }

        public KeyedCollection<string, ServiceTypeDescription> ServiceTypes { get; private set; }

        public KeyedCollection<string, EndpointResourceDescription> Endpoints { get; private set; }

        public ApplicationPrincipalsDescription GetApplicationPrincipals()
        {
            throw new NotImplementedException();
        }

        public IList<string> GetCodePackageNames()
        {
            return new List<string>() { this.CodePackageName };
        }

        public CodePackage GetCodePackageObject(string packageName)
        {
            return new TestCodePackage(null);
        }

        public IList<string> GetConfigurationPackageNames()
        {
            return new List<string>() { string.Empty };
        }

        public ConfigurationPackage GetConfigurationPackageObject(string packageName)
        {
            return MockConfigurationPackage.CreateDefaultPackage(packageName);
        }

        public IList<string> GetDataPackageNames()
        {
            return new List<string>() { string.Empty };
        }

        public DataPackage GetDataPackageObject(string packageName)
        {
            throw new NotImplementedException();
        }

        public EndpointResourceDescription GetEndpoint(string endpointName)
        {
            return this.Endpoints[endpointName];
        }

        public KeyedCollection<string, EndpointResourceDescription> GetEndpoints()
        {
            return this.Endpoints;
        }

        public KeyedCollection<string, ServiceGroupTypeDescription> GetServiceGroupTypes()
        {
            throw new NotImplementedException();
        }

        public string GetServiceManifestName()
        {
            return this.config["ServiceManifetName"];
        }

        public string GetServiceManifestVersion()
        {
            return this.config["ServiceManifetVersion"];
        }

        public KeyedCollection<string, ServiceTypeDescription> GetServiceTypes()
        {
            this.ThrowIfDisposed();
            return this.ServiceTypes;
        }

        public void ReportApplicationHealth(HealthInformation healthInformation)
        {
            throw new NotImplementedException();
        }

        public void ReportDeployedServicePackageHealth(HealthInformation healthInformation)
        {
            throw new NotImplementedException();
        }

        public void ReportDeployedApplicationHealth(HealthInformation healthInformation)
        {
            throw new NotImplementedException();
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public void ReportApplicationHealth(HealthInformation healthInfo, HealthReportSendOptions sendOptions)
        {
            throw new NotImplementedException();
        }

        public void ReportDeployedApplicationHealth(HealthInformation healthInfo, HealthReportSendOptions sendOptions)
        {
            throw new NotImplementedException();
        }

        public void ReportDeployedServicePackageHealth(HealthInformation healthInfo, HealthReportSendOptions sendOptions)
        {
            throw new NotImplementedException();
        }

        internal void ThrowIfDisposed()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(TestCodePackageActivationContext));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.disposedValue = true;
            }
        }
    }
}
