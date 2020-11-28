// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace BasicOperator.Models
{
    [KubernetesEntity(ApiVersion = KubeApiVersion, Group = KubeGroup, Kind = KubeKind, PluralName = "helloworlds")]
    public class V1alpha1HelloWorld : IKubernetesObject<V1ObjectMeta>, ISpec<V1alpha1HelloWorldSpec>, IStatus<V1alpha1HelloWorldStatus>
    {        
        public const string KubeApiVersion = "v1alpha1";
        public const string KubeGroup = "basic-operator.example.io";
        public const string KubeKind = "HelloWorld";

        /// <inheritdoc/>
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        /// <inheritdoc/>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <inheritdoc/>
        [JsonProperty("metadata")]
        public V1ObjectMeta Metadata { get; set; }

        [JsonProperty("spec")]
        public V1alpha1HelloWorldSpec Spec { get; set; }

        [JsonProperty("status")]
        public V1alpha1HelloWorldStatus Status { get; set; }
    }

    public class V1alpha1HelloWorldSpec
    {
    }

    public class V1alpha1HelloWorldStatus
    {
    }
}
