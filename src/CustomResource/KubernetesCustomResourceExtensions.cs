// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Kubernetes.CustomResources;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KubernetesCustomResourceExtensions
    {
        public static IServiceCollection AddKubernetesCustomResources(this IServiceCollection services)
        {
            return services.AddTransient<ICustomResourceDefinitionGenerator, CustomResourceDefinitionGenerator>();
        }
    }
}
