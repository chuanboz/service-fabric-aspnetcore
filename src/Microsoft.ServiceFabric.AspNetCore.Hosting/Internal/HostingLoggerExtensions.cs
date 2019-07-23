// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    using System;
    using System.Reflection;
    using Microsoft.Extensions.Logging;

    internal static class HostingLoggerExtensions
    {
        public static void ApplicationError(this ILogger logger, EventId eventId, string message, Exception exception)
        {
            if (exception is ReflectionTypeLoadException reflectionTypeLoadException)
            {
                foreach (var ex in reflectionTypeLoadException.LoaderExceptions)
                {
                    message = message + Environment.NewLine + ex.Message;
                }
            }

            logger.LogCritical(
                eventId: eventId,
                message: message,
                exception: exception);
        }

        public static void Starting(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                   eventId: LoggerEventIds.HostStarting,
                   message: "Hosting starting");
            }
        }

        public static void WaitForApplicationStart(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                   eventId: LoggerEventIds.WaitForApplicationStart,
                   message: "Waiting for application to finish starting");
            }
        }

        public static void Started(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    eventId: LoggerEventIds.HostStarted,
                    message: "Hosting started");
            }
        }

        public static void Stopping(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    eventId: LoggerEventIds.HostStopping,
                    message: "Hosting stopping");
            }
        }

        public static void Stopped(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    eventId: LoggerEventIds.HostStopped,
                    message: "Hosting stopped");
            }
        }
    }
}
