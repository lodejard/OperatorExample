// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Kubernetes.Testing.Models;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Testing
{
    [Route("api/{version}/{plural}")]
    public class ResourceApiController : Controller
    {
        private readonly ITestCluster _testCluster;

        public ResourceApiController(ITestCluster testCluster)
        {
            _testCluster = testCluster;
        }

        [FromRoute]
        public string Version { get; set; }

        [FromRoute]
        public string Plural { get; set; }


        [HttpGet]
        public async Task<object> ListAsync(ListParameters parameters)
        {
            var result = _testCluster.ListResources(string.Empty, Version, Plural, parameters);

            return new KubernetesList<ResourceObject>(
                apiVersion: Version,
                kind: "PodList",
                metadata: new V1ListMeta(
                    continueProperty: result.Continue,
                    remainingItemCount: null,
                    resourceVersion: result.ResourceVersion),
                items: result.Items);
        }
    }
}
