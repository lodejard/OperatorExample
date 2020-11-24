// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Kubernetes.Testing.Models;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Testing
{
    [Route("apis/{group}/{version}/{plural}")]
    public class ResourceApiGroupController : Controller
    {
        private readonly ITestCluster _testCluster;

        public ResourceApiGroupController(ITestCluster testCluster)
        {
            _testCluster = testCluster;
        }

        [FromRoute]
        public string Group { get; set; }

        [FromRoute]
        public string Version { get; set; }

        [FromRoute]
        public string Plural { get; set; }

        [HttpGet]
        public async Task<object> ListAsync(ListParameters parameters)
        {
            var result = _testCluster.ListResources(Group, Version, Plural, parameters);

            return new KubernetesList<ResourceObject>(
                apiVersion: $"{Group}/{Version}",
                kind: "DeploymentList",
                metadata: new V1ListMeta(
                    continueProperty: result.Continue,
                    remainingItemCount: null,
                    resourceVersion: result.ResourceVersion),
                items: result.Items);
        }
    }
}
