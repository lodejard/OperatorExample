﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Kubernetes.Core;
using Microsoft.Kubernetes.Testing;
using Microsoft.Kubernetes.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Controller.Informers
{
    [TestClass]
    public class ResourceInformerTests
    {
        [TestMethod]
        public async Task ResourcesAreListedWhenReadyAsyncIsComplete()
        {
            // arrange
            using var cancellation = new CancellationTokenSource(Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(5));

            var testYaml = TestYaml.LoadFromEmbeddedStream<(V1Pod[] pods, NamespacedName[] shouldBe)>();

            using var clusterHost = new TestClusterHostBuilder()
                .UseInitialResources(testYaml.pods)
                .Build();

            using var testHost = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddKubernetesControllerRuntime();
                    services.RegisterResourceInformer<V1Pod>();
                    services.Configure<KubernetesClientOptions>(options =>
                    {
                        options.Configuration = KubernetesClientConfiguration.BuildConfigFromConfigObject(clusterHost.KubeConfig);
                    });
                })
                .Build();

            var informer = testHost.Services.GetRequiredService<IResourceInformer<V1Pod>>();
            var pods = new Dictionary<NamespacedName, V1Pod>();
            using var registration = informer.Register((eventType, pod) =>
            {
                pods[NamespacedName.From(pod)] = pod;
            });

            // act
            await clusterHost.StartAsync(cancellation.Token);
            await testHost.StartAsync(cancellation.Token);

            await registration.ReadyAsync(cancellation.Token);

            // assert
            pods.Keys.ShouldBe(testYaml.shouldBe);
        }

        [TestMethod]
        public async Task ResourcesWithApiGroupAreListed()
        {
            // arrange
            using var cancellation = new CancellationTokenSource(Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(5));

            var testYaml = TestYaml.LoadFromEmbeddedStream<(V1Deployment[] deployments, NamespacedName[] shouldBe)>();

            using var clusterHost = new TestClusterHostBuilder()
                .UseInitialResources(testYaml.deployments)
                .Build();

            using var testHost = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddKubernetesControllerRuntime();
                    services.RegisterResourceInformer<V1Deployment>();
                    services.Configure<KubernetesClientOptions>(options =>
                    {
                        options.Configuration = KubernetesClientConfiguration.BuildConfigFromConfigObject(clusterHost.KubeConfig);
                    });
                })
                .Build();

            var informer = testHost.Services.GetRequiredService<IResourceInformer<V1Deployment>>();
            var deployments = new Dictionary<NamespacedName, V1Deployment>();
            using var registration = informer.Register((eventType, deployment) =>
            {
                deployments[NamespacedName.From(deployment)] = deployment;
            });

            // act
            await clusterHost.StartAsync(cancellation.Token);
            await testHost.StartAsync(cancellation.Token);

            await registration.ReadyAsync(cancellation.Token);

            // assert
            deployments.Keys.ShouldBe(testYaml.shouldBe);
        }
    }
}
