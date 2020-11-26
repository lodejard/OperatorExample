// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Kubernetes.Core.Resources
{
    public interface IResourceKind
    {
        string ApiVersion { get;  }

        string Kind { get;  }

        IResourceKindElement Schema { get;  }
    }
}
