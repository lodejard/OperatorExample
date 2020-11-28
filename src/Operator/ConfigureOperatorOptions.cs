// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using k8s.Models;
using Microsoft.Extensions.Options;
using Microsoft.Kubernetes.Controller.Informers;
using System;

namespace Microsoft.Kubernetes.Operator
{
    public class ConfigureOperatorOptions<TOperatorResource, TRelatedResource> : IConfigureNamedOptions<OperatorOptions> where TRelatedResource : IKubernetesObject<V1ObjectMeta>, new()
    {
        private readonly IResourceInformer<TRelatedResource> _resourceInformer;

        public ConfigureOperatorOptions(IResourceInformer<TRelatedResource> resourceInformer)
        {
            _resourceInformer = resourceInformer;
        }

        public void Configure(string name, OperatorOptions options)
        {
            if (string.Equals(name, typeof(TOperatorResource).Name, StringComparison.Ordinal))
            {
                options.Informers.Add(_resourceInformer);
            }
        }

        public void Configure(OperatorOptions options)
        {
        }
    }
}
