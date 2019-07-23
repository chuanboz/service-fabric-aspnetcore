// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    internal static class LoggerEventIds
    {
        public const int HostStarting = 1;
        public const int HostStarted = 2;
        public const int HostStopping = 3;
        public const int HostStopped = 4;
        public const int StoppedWithException = 5;
        public const int ApplicationStartupException = 6;
        public const int ApplicationStoppingException = 7;
        public const int ApplicationStoppedException = 8;
        public const int WaitForApplicationStart = 9;
    }
}
