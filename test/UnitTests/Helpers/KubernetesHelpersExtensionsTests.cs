// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kubernetes.Helpers.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Kubernetes.Helpers.Helpers
{
    [TestClass]
    public class KubernetesHelpersExtensionsTests
    {
        [TestMethod]
        public void KubernetesClientIsAdded()
        {
            // arrange 
            var services = new ServiceCollection();

            // act
            services.AddKubernetesHelpers();

            // assert
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IKubernetes>().ShouldNotBeNull();
        }

        [TestMethod]
        public void HelperServicesAreAdded()
        {
            // arrange 
            var services = new ServiceCollection();

            // act
            services.AddKubernetesHelpers();

            // assert
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IResourceSerializers>().ShouldNotBeNull();
        }

        
        [TestMethod]
        public void ExistingClientIsNotReplaced()
        {
            // arrange 
            using var client = new k8s.Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
            var services = new ServiceCollection();

            // act
            services.AddSingleton<IKubernetes>(client);
            services.AddKubernetesHelpers();
            
            // assert
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<IKubernetes>().ShouldBeSameAs(client);
        }
    }
}
