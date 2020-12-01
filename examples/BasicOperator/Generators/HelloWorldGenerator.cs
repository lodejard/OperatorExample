// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BasicOperator.Models;
using k8s;
using k8s.Models;
using Microsoft.Kubernetes.Operator.Generators;
using Microsoft.Kubernetes.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BasicOperator.Generators
{
    public class HelloWorldGenerator : IOperatorGenerator<V1alpha1HelloWorld>
    {
        private readonly IResourceSerializers _serializers;
        private readonly string _managedBy;
        private readonly string _version;

        public HelloWorldGenerator(IResourceSerializers serializers)
        {
            _serializers = serializers;

            _managedBy = typeof(HelloWorldGenerator).Assembly
                .GetCustomAttributes(typeof(AssemblyTitleAttribute))
                .Cast<AssemblyTitleAttribute>()
                .Single()
                .Title;

            _version = typeof(HelloWorldGenerator).Assembly
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute))
                .Cast<AssemblyInformationalVersionAttribute>()
                .Single()
                .InformationalVersion;
        }

        public async Task<GenerateResult> GenerateAsync(V1alpha1HelloWorld helloWorld)
        {
            if (helloWorld is null)
            {
                throw new ArgumentNullException(nameof(helloWorld));
            }

            var result = new GenerateResult();

            // create deployment and service
            var deployment = Add(CreateDeployment(helloWorld));
            var service = Add(CreateService(helloWorld));

            // create serviceaccount if needed
            var createServiceAccount = helloWorld?.Spec?.CreateServiceAccount ?? false;
            if (createServiceAccount)
            {
                var serviceAccount = Add(CreateServiceAccount(helloWorld));

                deployment.Spec.Template.Spec.ServiceAccountName = serviceAccount.Name();
            }
            else
            {
                deployment.Spec.Template.Spec.ServiceAccountName = "default";
            }

            SetMetadataLabels(result.Resources, new Dictionary<string, string>
            {
                { "app.kubernetes.io/managed-by", _managedBy },
                { "app.kubernetes.io/version", _version },
            });

            result.ShouldReconcile = true;
            return result;

            // nested function to add resources easily
            T Add<T>(T t) where T : IKubernetesObject<V1ObjectMeta>
            {
                result.Resources.Add(t);
                return t;
            }
        }

        public V1Deployment CreateDeployment(V1alpha1HelloWorld helloWorld)
        {
            var deployment = _serializers.DeserializeYaml<V1Deployment>($@"
apiVersion: apps/v1
kind: Deployment
metadata:
    name: {helloWorld.Name()}
    namespace: {helloWorld.Namespace()}
spec:
    selector:
        matchLabels:
            app.kubernetes.io/instance: {helloWorld.Name()}
            app.kubernetes.io/component: kuard
    template:
        metadata:
            labels:
                app.kubernetes.io/instance: {helloWorld.Name()}
                app.kubernetes.io/component: kuard
        spec:
            containers:
            -   name: kuard
                image: gcr.io/kuar-demo/kuard-amd64:{helloWorld?.Spec?.KuardLabel ?? "blue"}
                ports:
                -   name: http
                    containerPort: 8080
");

            if (helloWorld?.Spec?.NodeSelector != null)
            {
                deployment.Spec.Template.Spec.NodeSelector = helloWorld?.Spec?.NodeSelector;
            }

            return deployment;
        }

        public V1Service CreateService(V1alpha1HelloWorld helloWorld)
        {
            var serviceType = (helloWorld?.Spec?.CreateLoadBalancer ?? false) ? "LoadBalancer" : "ClusterIP";

            return _serializers.DeserializeYaml<V1Service>($@"
apiVersion: v1
kind: Service
metadata:
    name: {helloWorld.Name()}
    namespace: {helloWorld.Namespace()}
spec:
    type: {serviceType}
    selector:
        app.kubernetes.io/instance: {helloWorld.Name()}
        app.kubernetes.io/component: kuard
    ports:
        -   name: http
            port: 80
            targetPort: http
");
        }

        private V1ServiceAccount CreateServiceAccount(V1alpha1HelloWorld helloWorld)
        {
            return _serializers.DeserializeYaml<V1ServiceAccount>($@"
apiVersion: v1
kind: ServiceAccount
metadata:
    name: {helloWorld.Name()}
    namespace: {helloWorld.Namespace()}
");
        }

        private static void SetMetadataLabels(IEnumerable<IKubernetesObject<V1ObjectMeta>> resources, IEnumerable<KeyValuePair<string, string>> labels)
        {
            foreach (var resource in resources)
            {
                foreach (var (key, value) in labels)
                {
                    resource.SetLabel(key, value);
                }

                if (resource is V1Deployment deployment)
                {
                    var podLabels = deployment.Spec.Template.Metadata.EnsureLabels();

                    foreach (var (key, value) in labels)
                    {
                        podLabels[key] = value;
                    }
                }
            }
        }
    }
}
