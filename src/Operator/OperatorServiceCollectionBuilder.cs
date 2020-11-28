// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using k8s.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Class OperatorServiceCollection.
    /// </summary>
    /// <typeparam name="TResource">The type of the t resource.</typeparam>
    public class OperatorServiceCollectionBuilder<TResource>
            where TResource : IKubernetesObject<V1ObjectMeta>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorServiceCollectionBuilder{TResource}" /> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public OperatorServiceCollectionBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// Gets or sets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceCollection Services { get; set; }

        /// <summary>
        /// Withes the related resource.
        /// </summary>
        /// <typeparam name="TRelatedResource">The type of the t related resource.</typeparam>
        /// <returns>OperatorServiceCollection&lt;TResource&gt;.</returns>
        public OperatorServiceCollectionBuilder<TResource> WithRelatedResource<TRelatedResource>()
            where TRelatedResource : IKubernetesObject<V1ObjectMeta>, new()
        {
            Services = Services.RegisterOperatorResourceInformer<TResource, TRelatedResource>();
            return this;
        }
    }
}
