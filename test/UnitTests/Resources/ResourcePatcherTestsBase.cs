// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Kubernetes.Core.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Resources
{
    [TestClass]
    public abstract class ResourcePatcherTestsBase
    {
        public virtual IResourceKindManager Manager { get; set; }

        public async Task RunStandardTest(StandardTestYaml testYaml)
        {
            await RunThreeWayMerge(testYaml);
            if (!testYaml.Patch.Any(operation => operation.Op == "remove"))
            {
                await RunApplyLiveOnlyMerge(testYaml);
            }
        }

        private async Task RunThreeWayMerge(StandardTestYaml testYaml)
        {
            // arrange
            IResourcePatcher patcher = new ResourcePatcher();

            // act
            var context = new CreateJsonPatchContext
            {
                ApplyResource = testYaml.Apply,
                LastAppliedResource = testYaml.LastApplied,
                LiveResource = testYaml.Live,
            };

            if (testYaml.ResourceKind != null)
            {
                context.ResourceKind = await Manager.GetResourceKindAsync(
                    testYaml.ResourceKind.ApiVersion,
                    testYaml.ResourceKind.Kind);
            }

            var patch = patcher.CreateJsonPatch(context);

            // assert
            var operations = new ResourceSerializers().Convert<PatchOperation[]>(patch);
            operations.ShouldBe(testYaml.Patch, ignoreOrder: true);
        }

        private async Task RunApplyLiveOnlyMerge(StandardTestYaml testYaml)
        {
            // arrange
            IResourcePatcher patcher = new ResourcePatcher();

            // act
            var context = new CreateJsonPatchContext
            {
                ApplyResource = testYaml.Apply,
                LiveResource = testYaml.Live,
            };

            if (testYaml.ResourceKind != null)
            {
                context.ResourceKind = await Manager.GetResourceKindAsync(
                    testYaml.ResourceKind.ApiVersion,
                    testYaml.ResourceKind.Kind);
            }

            var patch = patcher.CreateJsonPatch(context);

            // assert
            var operations = new ResourceSerializers().Convert<List<PatchOperation>>(patch);
            operations.ShouldBe(testYaml.Patch, ignoreOrder: true);
        }

        public class StandardTestYaml
        {
            public StandardTestResourceKind ResourceKind { get; set; }
            public JToken Apply { get; set; }
            public JToken Live { get; set; }
            public JToken LastApplied { get; set; }
            public List<PatchOperation> Patch { get; set; }
        }

        public class StandardTestResourceKind
        {
            public string ApiVersion { get; set; }
            public string Kind { get; set; }
        }

        public struct PatchOperation : IEquatable<PatchOperation>
        {
            private static readonly IEqualityComparer<JToken> _tokenEqualityComparer = new JTokenEqualityComparer();

            [JsonProperty("op")]
            public string Op { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("value")]
            public JToken Value { get; set; }

            public override bool Equals(object obj)
            {
                return obj is PatchOperation operation && Equals(operation);
            }

            public bool Equals(PatchOperation other)
            {
                return Op == other.Op &&
                       Path == other.Path &&
                       _tokenEqualityComparer.Equals(Value, other.Value);
            }

            public static bool operator ==(PatchOperation left, PatchOperation right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(PatchOperation left, PatchOperation right)
            {
                return !(left == right);
            }

            public override string ToString()
            {
                return $"{Op} {Path} {Value?.GetType()?.Name} {Value}";
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Op, Path, _tokenEqualityComparer.GetHashCode(Value));
            }
        }
    }
}
