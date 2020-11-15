// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Falcon.Kubernetes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Kubernetes.Helpers
{
    [TestClass]
    public class NamespacedNameTests
    {
        [TestMethod]
        public void WorksAsDictionaryKey()
        {
            // arrange
            var dictionary = new Dictionary<NamespacedName, string>();
            var name1 = new NamespacedName("ns", "n1");
            var name2 = new NamespacedName("ns", "n2");
            var name3 = new NamespacedName("ns", "n3");

            // act
            dictionary[name1] = "one";
            dictionary[name1] = "one again";
            dictionary[name2] = "two";

            // assert
            dictionary.ShouldSatisfyAllConditions(
                () => dictionary.ShouldContainKeyAndValue(name1, "one again"),
                () => dictionary.ShouldContainKeyAndValue(name2, "two"),
                () => dictionary.ShouldNotContainKey(name3));
        }

        [TestMethod]
        [DataRow("ns", "n1", "ns", "n1", true)]
        [DataRow("ns", "n1", "ns", "n2", false)]
        [DataRow("ns", "n1", "ns-x", "n1", false)]
        [DataRow(null, "n1", null, "n1", true)]
        [DataRow(null, "n1", null, "n2", false)]
        public void NameEqualityAndInequality(string namespace1, string name1, string namespace2, string name2, bool shouldBeEqual)
        {
            // arrange
            var namespacedName1 = new NamespacedName(namespace1, name1);
            var namespacedName2 = new NamespacedName(namespace2, name2);

            // act
            var areEqual = namespacedName1 == namespacedName2;
            var areNotEqual = namespacedName1 != namespacedName2;

            // assert
            areEqual.ShouldNotBe(areNotEqual);
            areEqual.ShouldBe(shouldBeEqual);
        }
    }
}
