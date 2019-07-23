// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    /// <summary>
    /// generic implementation of CommunicationListener that register with ServiceFabricHost.
    /// </summary>
    /// <remarks>
    /// This is a bridge between IHost/ServiceFabricHost and WebHostBuilder, will register with Service Fabric for the service instance factory.
    /// </remarks>
    internal class WebHostCommunicationListener : ICommunicationListener
    {
        private readonly ILogger<WebHostCommunicationListener> logger;
        private readonly IApplicationLifetime lifetime;
        private IWebHost webHost = null;
        private bool configuredToUseUniqueServiceUrl = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostCommunicationListener"/> class.
        /// </summary>
        /// <param name="serviceContext">the service context.</param>
        /// <param name="logger">logger.</param>
        /// <param name="options">service host start options.</param>
        /// <param name="lifetime">the application lifetime.</param>
        public WebHostCommunicationListener(ServiceContext serviceContext, ILogger<WebHostCommunicationListener> logger, IOptions<ServiceFabricHostOptions> options, IApplicationLifetime lifetime)
        {
            this.ServiceContext = serviceContext;
            this.webHost = options.Value.WebHost;
            this.logger = logger;
            this.lifetime = lifetime;
        }

        /// <summary>
        /// Gets the context of the service for which this communication listener is being constructed.
        /// </summary>
        public ServiceContext ServiceContext { get; }

        /// <summary>
        /// Gets or sets UrlSuffix to be used.
        /// </summary>
        public string UrlSuffix
        {
            get; set;
        }

        public string PublishAddress { get; private set; }

        public void Abort()
        {
            var watch = Stopwatch.StartNew();
            Log.AbortStart(this.logger, null);

            try
            {
                if (this.webHost != null)
                {
                    this.webHost.Dispose();
                    this.webHost = null;
                }
            }
            catch (Exception ex)
            {
                Log.AbortFailed(this.logger, (long)watch.Elapsed.TotalSeconds, ex);
                throw;
            }
            finally
            {
                Log.AbortEnd(this.logger, (long)watch.Elapsed.TotalSeconds, null);
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            var watch = Stopwatch.StartNew();
            Log.CloseAsyncStart(this.logger, null);

            try
            {
                if (this.webHost != null)
                {
                    await this.webHost.StopAsync(cancellationToken);
                    this.webHost.Dispose();
                    this.webHost = null;
                }
            }
            catch (Exception ex)
            {
                Log.CloseAsyncFailed(this.logger, (long)watch.Elapsed.TotalSeconds, ex);
                throw;
            }
            finally
            {
                Log.CloseAsyncEnd(this.logger, (long)watch.Elapsed.TotalSeconds, null);
            }
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                // this will be trigger as the last step for webHost.StartAsync
                this.lifetime.ApplicationStarted.Register(() =>
                {
                    // AspNetCore 1.x returns http://+:port
                    // AspNetCore 2.0 returns http://[::]:port
                    var feature = this.webHost.ServerFeatures.Get<IServerAddressesFeature>();
                    var url = feature.Addresses.FirstOrDefault();

                    if (url == null)
                    {
                        throw new InvalidOperationException("No url found from asp.net core IServerAddressesFeature");
                    }

                    var publishAddress = this.ServiceContext.PublishAddress;

                    if (url.Contains("://+:"))
                    {
                        url = url.Replace("://+:", $"://{publishAddress}:");
                    }
                    else if (url.Contains("://[::]:"))
                    {
                        url = url.Replace("://[::]:", $"://{publishAddress}:");
                    }

                    // See https://github.com/aspnet/IISIntegration/blob/df88e322cc5e52db3dbce4060d5bc7db88edb8e4/src/Microsoft.AspNetCore.Server.IISIntegration/WebHostBuilderIISExtensions.cs#L19
                    var pathBase = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                    if (!string.IsNullOrEmpty(pathBase))
                    {
                        // Append the PathBase for Kestrel, otherwise it will only contains the Host/Port
                        url += pathBase;
                    }

                    // When returning url to naming service, add UrlSuffix to it.
                    // This UrlSuffix will be used by middleware to:
                    //    - drop calls not intended for the service and return 410.
                    //    - modify Path and PathBase in Microsoft.AspNetCore.Http.HttpRequest to be sent correctly to the service code.
                    this.PublishAddress = url.TrimEnd(new[] { '/' }) + this.UrlSuffix;

                    // refresh IServerAddressesFeature to store the final publish address.
                    if (!feature.Addresses.IsReadOnly)
                    {
                        feature.Addresses.Clear();
                        feature.Addresses.Add(this.PublishAddress);
                    }
                });

                Log.OpenAsyncStart(this.logger, null);
                await this.webHost.StartAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.OpenAsyncFailed(this.logger, (long)watch.Elapsed.TotalSeconds, ex);

                throw;
            }

            Log.OpenAsyncEnd(this.logger, this.PublishAddress, (long)watch.Elapsed.TotalSeconds, null);
            return this.PublishAddress;
        }

        /// <summary>
        /// Configures the listener to use UniqueServiceUrl by appending a urlSuffix PartitionId and ReplicaId.
        /// It helps in scenarios when ServiceA listening on a node on port X moves and another Service takes its place on the same node and starts using the same port X,
        /// The UniqueServiceUrl in conjunction with middleware rejects requests meant for serviceA arriving at ServiceB.
        /// Example:
        /// Service A is dynamically assigned port 30000 on node with IP 10.0.0.1, it listens on http://+:30000/ and reports to Naming service http://10.0.0.1:30000/serviceName-A/partitionId-A/replicaId-A
        /// Client resolves URL from NS: http://10.0.0.1:30000/serviceName-A/partitionId-A/replicaId-A and sends a request, Service A compares URL path segments to its own service name, partition ID, replica ID, finds they are equal, serves request.
        /// Now Service A moves to a different node and Service B comes up at the node with IP 10.0.0.1 and is dynamically assigned port 30000.
        /// Service B listens on: http://+:30000/ and reports to NS http://10.0.0.1:30000/serviceName-B/partitionId-B/replicaId-B, Client for Service a sends request to http://10.0.0.1:30000/serviceName-A/partitionId-A/replicaId-A
        /// Service B compares URL path segments to its own service name, partition ID, replica ID, finds they do not match, ends the request and responds with HTTP 410. Client receives 410 and re-resolves for service A.
        /// </summary>
        internal void ConfigureToUseUniqueServiceUrl()
        {
            if (!this.configuredToUseUniqueServiceUrl)
            {
                this.UrlSuffix = string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", this.ServiceContext.PartitionId, this.ServiceContext.ReplicaOrInstanceId);

                if (this.ServiceContext is StatefulServiceContext)
                {
                    // For stateful service, also append a Guid, Guid makes the url unique in scenarios for stateful services when Listener is
                    // created to support read on secondary and change role happens from Primary->Secondary for the replica.
                    this.UrlSuffix += "/" + Guid.NewGuid();
                }

                this.configuredToUseUniqueServiceUrl = true;
            }
        }

        private static class Log
        {
            public static readonly Action<ILogger, long, Exception> OpenAsyncFailed = LoggerMessage.Define<long>(
                LogLevel.Critical,
                new EventId(1001, nameof(OpenAsyncFailed)),
                "OpenCommunicationListenerAsync Failed after {ElapsedSeconds} seconds");

            public static readonly Action<ILogger, Exception> OpenAsyncStart = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1002, nameof(OpenAsyncStart)),
                "OpenSync started to call WebHost.StartAsync");

            public static readonly Action<ILogger, string, long, Exception> OpenAsyncEnd = LoggerMessage.Define<string, long>(
                LogLevel.Information,
                new EventId(1003, nameof(OpenAsyncEnd)),
                "PublishAddress:{PublishAddress}, OpenAsync end after {ElapsedSeconds} seconds");

            public static readonly Action<ILogger, long, Exception> CloseAsyncFailed = LoggerMessage.Define<long>(
                LogLevel.Error,
                new EventId(1004, nameof(CloseAsyncFailed)),
                "CloseAsync Failed after {ElapsedSeconds} seconds");

            public static readonly Action<ILogger, Exception> CloseAsyncStart = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1005, nameof(CloseAsyncStart)),
                "CloseSync started to call WebHost.StartAsync");

            public static readonly Action<ILogger, long, Exception> CloseAsyncEnd = LoggerMessage.Define<long>(
                LogLevel.Information,
                new EventId(1006, nameof(CloseAsyncEnd)),
                "CloseAsync end after {ElapsedSeconds} seconds");

            public static readonly Action<ILogger, long, Exception> AbortFailed = LoggerMessage.Define<long>(
                LogLevel.Error,
                new EventId(1007, nameof(AbortFailed)),
                "Abort Failed after {ElapsedSeconds} seconds");

            public static readonly Action<ILogger, Exception> AbortStart = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(1008, nameof(AbortStart)),
                "Abort started");

            public static readonly Action<ILogger, long, Exception> AbortEnd = LoggerMessage.Define<long>(
                LogLevel.Information,
                new EventId(1009, nameof(AbortEnd)),
                "Abort end after {ElapsedSeconds} seconds");
        }
    }
}
