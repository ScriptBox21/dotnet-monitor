// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Tools.Monitor
{
    /// <summary>
    /// Used to generate Api Key for authentication. The first output is
    /// part of the Authorization header, and is the Base64 encoded key.
    /// The second output is a hex encoded string of the hash of the secret.
    /// </summary>
    internal sealed class GenerateApiKeyCommandHandler
    {
        public Task<int> GenerateApiKey(CancellationToken token, int keyLength, string hashAlgorithm, IConsole console)
        {
            if (!HashAlgorithmChecker.IsAllowedAlgorithm(hashAlgorithm))
            {
                console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture, 
                        Strings.ErrorMessage_ParameterNotAllowed, 
                        nameof(hashAlgorithm), 
                        hashAlgorithm));
                return Task.FromResult(1);
            }

            if (keyLength < ApiKeyAuthenticationHandler.ApiKeyByteMinLength || keyLength > ApiKeyAuthenticationHandler.ApiKeyByteMaxLength)
            {
                console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.ErrorMessage_ParameterNotAllowedByteRange,
                        nameof(hashAlgorithm),
                        hashAlgorithm,
                        ApiKeyAuthenticationHandler.ApiKeyByteMinLength,
                        ApiKeyAuthenticationHandler.ApiKeyByteMaxLength));
                return Task.FromResult(1);
            }

            GeneratedApiKey newKey = GeneratedApiKey.Create(keyLength, hashAlgorithm);

            console.Out.WriteLine(FormattableString.Invariant($"Authorization: {Monitoring.WebApi.AuthConstants.ApiKeySchema} {newKey.MonitorApiKey}"));
            console.Out.WriteLine(FormattableString.Invariant($"ApiKeyHash: {newKey.HashValue}"));
            console.Out.WriteLine(FormattableString.Invariant($"ApiKeyHashType: {newKey.HashAlgorithm}"));

            return Task.FromResult(0);
        }
    }
}
