// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Kubernetes.Core.Resources
{
    public interface IResourceKindElement
    {
        ElementMergeStrategy MergeStrategy { get; }

        public string MergeKey { get; }

        IResourceKindElement GetPropertyElementType(string name);

        IResourceKindElement GetCollectionElementType();
    }
}
