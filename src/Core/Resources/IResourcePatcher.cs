// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.JsonPatch;

namespace Microsoft.Kubernetes.Core.Resources
{
    public interface IResourcePatcher
    {
        JsonPatchDocument CreateJsonPatch(CreateJsonPatchContext context);
    }

    public class CreateJsonPatchContext
    {
        public IResourceKind ResourceKind { get; set; }
        public object ApplyResource { get; set; }
        public object LastAppliedResource { get; set; }
        public object LiveResource { get; set; }
    }
}
