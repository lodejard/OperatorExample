﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using k8s.Models;
using Microsoft.Kubernetes.Controller.Informers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KubernetesControllerExtensions
    {
        public static IServiceCollection AddKubernetesControllerRuntime(this IServiceCollection services)
        {
            return services
                .AddKubernetesHelpers()
                .AddSingleton(typeof(IResourceInformer<>), typeof(ResourceInformer<>));
        }

        /// <summary>
        /// Registers the resource informer.
        /// </summary>
        /// <typeparam name="TResource">The type of the t related resource.</typeparam>
        /// <param name="services">The services.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection RegisterResourceInformer<TResource>(this IServiceCollection services)
            where TResource : IKubernetesObject<V1ObjectMeta>, new()
        {
            return services
                .RegisterHostedService<IResourceInformer<TResource>>();
        }
    }
}
