// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Kubernetes.Controller.Informers;
using System.Collections.Generic;

namespace Microsoft.Kubernetes.Operator
{
    public class OperatorOptions
    {
        public List<IResourceInformer> Informers { get; } = new List<IResourceInformer>();

    }
}
