// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using k8s;
using Microsoft.Extensions.Options;
using Microsoft.Kubernetes.Core;
using Microsoft.Kubernetes.Core.Resources;
using Microsoft.Kubernetes.Core.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Class KubernetesCoreExtensions.
    /// </summary>
    public static class KubernetesCoreExtensions
    {
        /// <summary>
        /// Adds the kubernetes.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddKubernetesCore(this IServiceCollection services)
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

                    return new k8s.Kubernetes(options.Configuration);
                });
            }

            if (!services.Any(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IResourceKindManager)))
            {
                services.AddSingleton<IResourceKindManager, ResourceKindManager>();
            }

            return services;
        }
    }
}

namespace k8s
{
    public static class KubernetesHelpersExtensions
    {
        public static IAnyResourceKind AnyResourceKind(this IKubernetes client)
        {
            return new AnyResourceKind(client);
        }
    }
}
