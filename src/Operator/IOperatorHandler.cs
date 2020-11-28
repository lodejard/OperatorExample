// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Hosting;

namespace Microsoft.Kubernetes.Operator
{
    internal interface IOperatorHandler<TResource> : IHostedService
    {
    }
}
