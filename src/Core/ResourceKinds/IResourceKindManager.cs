// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Core.Resources
{
    public interface IResourceKindManager
    {
        public Task<IResourceKind> GetResourceKindAsync(string apiVersion, string kind);
    }
}
