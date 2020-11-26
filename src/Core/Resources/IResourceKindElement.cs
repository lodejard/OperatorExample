// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Kubernetes.Core.Resources
{
    public interface IResourceKindElement
    {
        ElementMergeStrategy MergeStrategy { get; }

        IResourceKindElement GetProperty(string name);
    }
}
