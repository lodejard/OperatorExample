// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using k8s.Models;
using System.Collections.Generic;

namespace Microsoft.Kubernetes.Operator
{
    public class ReconcileParameters<TResource>
        where TResource : class, IKubernetesObject<V1ObjectMeta>
    {
        public TResource Resource { get; set; }
        public IDictionary<GroupKindNamespacedName, IKubernetesObject<V1ObjectMeta>> RelatedResources { get; set; }
    }
}
