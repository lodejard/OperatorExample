// <copyright file="KubernetesClientOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Kubernetes.Helpers
{
    using k8s;

    /// <summary>
    /// Class KubernetesClientOptions.
    /// </summary>
    public class KubernetesClientOptions
    {
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public KubernetesClientConfiguration Configuration { get; set; }
    }
}
