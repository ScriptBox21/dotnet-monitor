﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.WebApi
{
    internal sealed class ClientEndpointInfoSource : IEndpointInfoSourceInternal
    {
        public async Task<IEnumerable<IEndpointInfo>> GetEndpointInfoAsync(CancellationToken token)
        {
            var endpointInfoTasks = new List<Task<EndpointInfo>>();
            // Run the EndpointInfo creation parallel. The call to FromProcessId sends
            // a GetProcessInfo command to the runtime instance to get additional information.
            foreach (int pid in DiagnosticsClient.GetPublishedProcesses())
            {
                endpointInfoTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        return EndpointInfo.FromProcessId(pid);
                    }
                    // Catch when runtime instance shuts down while attepting to use the established diagnostic port connection.
                    catch (EndOfStreamException)
                    {
                        return null;
                    }
                    //Catch when the application is running a more privilaged socket than dotnet-monitor. For example, running a web app as administrator
                    //while running dotnet-monitor without elevation.
                    catch (UnauthorizedAccessException)
                    {
                        return null;
                    }
                    //Most errors from IpcTransport, such as a stale socket.
                    catch (ServerNotAvailableException)
                    {
                        return null;
                    }
                }, token));
            }

            await Task.WhenAll(endpointInfoTasks);

            return endpointInfoTasks.Where(t => t.Result != null).Select(t => t.Result);
        }
    }
}
