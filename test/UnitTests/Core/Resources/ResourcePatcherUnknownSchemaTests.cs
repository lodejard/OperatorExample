// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Kubernetes.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Kubernetes.Core.Resources
{
    [TestClass]
    public class ResourcePatcherUnknownSchemaTests
    {
        [TestMethod]
        public void ObjectPropertyIsAddedWhenMissing()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void NestedPropertyIsAddedWhenMissing()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void TildaAndForwardSlashAreEscapedInPatchPaths()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void AdditionalPropertyIsAddedWhenMissing()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void PropertiesOfStringAreOnlyRemovedWhenPreviouslyAdded()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void PropertiesOfObjectAreOnlyRemovedWhenPreviouslyAdded()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void PropertiesOfNullAreOnlyRemovedWhenPreviouslyAdded()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void ArrayAreAddedAndRemovedEntirelyAsNeeded()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void ArraysAreReplacedEntirelyWhenDifferent()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }
        
        [TestMethod]
        public void MergingWhenApplyElementTypeHasChanged()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        [TestMethod]
        public void MergingWhenLiveElementTypeHasChanged()
        {
            RunStandardTest(TestYaml.LoadFromEmbeddedStream<StandardTestYaml>());
        }

        public void RunStandardTest(StandardTestYaml testYaml)
        {
            RunThreeWayMerge(testYaml);
            if (!testYaml.Patch.Any(operation => operation.Op == "remove"))
            {
                RunApplyLiveOnlyMerge(testYaml);
            }
        }

        private static void RunThreeWayMerge(StandardTestYaml testYaml)
        {
            // arrange
            IResourcePatcher patcher = new ResourcePatcher();

            // act
            var patch = patcher.CreateJsonPatch(new CreateJsonPatchContext
            {
                ApplyResource = testYaml.Apply,
                LastAppliedResource = testYaml.LastApplied,
                LiveResource = testYaml.Live,
            });

            // assert
            var operations = new ResourceSerializers().Convert<PatchOperation[]>(patch);
            operations.ShouldBe(testYaml.Patch, ignoreOrder: true);
        }

        private static void RunApplyLiveOnlyMerge(StandardTestYaml testYaml)
        {
            // arrange
            IResourcePatcher patcher = new ResourcePatcher();

            // act
            var patch = patcher.CreateJsonPatch(new CreateJsonPatchContext
            {
                ApplyResource = testYaml.Apply,
                LiveResource = testYaml.Live,
            });

            // assert
            var operations = new ResourceSerializers().Convert<List<PatchOperation>>(patch);
            operations.ShouldBe(testYaml.Patch, ignoreOrder: true);
        }

        public class StandardTestYaml
        {
            public JToken Apply { get; set; }
            public JToken Live { get; set; }
            public JToken LastApplied { get; set; }
            public List<PatchOperation> Patch { get; set; }
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
                return $"{Op} {Path} {Value?.GetType()} {Value}";
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Op, Path, _tokenEqualityComparer.GetHashCode(Value));
            }
        }
    }
}
