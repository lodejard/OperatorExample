// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Kubernetes.Core.Resources
{
    public sealed class DefaultResourceKindElement : IResourceKindElement
        {
            public static IResourceKindElement Instance { get; } = new DefaultResourceKindElement();

            public ElementMergeStrategy MergeStrategy => ElementMergeStrategy.Unknown;

            public IResourceKindElement GetProperty(string name) => Instance;
        }
}
