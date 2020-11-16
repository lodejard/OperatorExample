﻿// <copyright file="ServiceCollectionKubernetesExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Linq;
    using k8s;
    using Microsoft.Extensions.Options;
    using Microsoft.Kubernetes.Helpers;
    using Microsoft.Kubernetes.Helpers.Resources;

    /// <summary>
    /// Class ServiceCollectionKubernetesExtensions.
    /// </summary>
    public static class KubernetesHelpersExtensions
    {
        /// <summary>
        /// Adds the kubernetes.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddKubernetesHelpers(this IServiceCollection services)
        {
            services = services
                .AddTransient<IResourceSerializers, ResourceSerializers>();

            if (!services.Any(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IKubernetes)))
            {
                services.Configure<KubernetesClientOptions>(options =>
                {
                    if (options.Configuration == null)
                    {
                        options.Configuration = KubernetesClientConfiguration.BuildDefaultConfig();
                    }
                });

                services.AddSingleton<IKubernetes>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<KubernetesClientOptions>>().Value;

                    return new Kubernetes(options.Configuration);
                });
            }

            return services;
        }
    }
}
