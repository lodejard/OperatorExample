// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Kubernetes.Core.Resources
{
    public sealed class DefaultResourceKind : IResourceKind
        {
            public static IResourceKind Instance { get; } = new DefaultResourceKind();

            public string ApiVersion => default;

            public string Kind => default;

            public IResourceKindElement Schema => DefaultResourceKindElement.Instance;
        }
}
