// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Kubernetes.ResourceKindProvider.OpenApi;
using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.ResourceKinds.OpenApi
{
    public class OpenApiResourceKindProvider : IResourceKindProvider
    {
        private readonly Dictionary<(string apiVersion, string kind), OpenApiResourceKind> _resourceKinds = new Dictionary<(string apiVersion, string kind), OpenApiResourceKind>();
        private readonly object _resourceKindsSync = new object();
        private readonly Lazy<Task<IDictionary<string, JsonSchema>>> _lazyDefinitions;
        private readonly Lazy<Task<ApiVersionKindSchemas>> _lazyApiVersionKindSchemas;

        public OpenApiResourceKindProvider()
        {
            _lazyDefinitions = new Lazy<Task<IDictionary<string, JsonSchema>>>(LoadDefinitions, LazyThreadSafetyMode.ExecutionAndPublication);
            _lazyApiVersionKindSchemas = new Lazy<Task<ApiVersionKindSchemas>>(LoadApiVersionKindSchemas, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public async Task<IResourceKind> GetResourceKindAsync(string apiVersion, string kind)
        {
            var key = (apiVersion, kind);
            lock (_resourceKindsSync)
            {
                if (_resourceKinds.TryGetValue(key, out var cachedResourceKind))
                {
                    return cachedResourceKind;
                }
            }

            var apiVersionKindSchemas = await _lazyApiVersionKindSchemas.Value;
            if (!apiVersionKindSchemas.TryGetValue(key, out var schema))
            {
                return null;
            }

            var resourceKind = new OpenApiResourceKind(apiVersion, kind, schema);

            lock (_resourceKindsSync)
            {
                if (!_resourceKinds.TryAdd(key, resourceKind))
                {
                    resourceKind = _resourceKinds[key];
                }
            }
            return resourceKind;
        }

        public async Task<IDictionary<string, JsonSchema>> LoadDefinitions()
        {
            using var stream = typeof(OpenApiResourceKindProvider).Assembly.GetManifestResourceStream(typeof(OpenApiResourceKindProvider), "swagger.json");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var openApiDocument = await OpenApiDocument.FromJsonAsync(json);
            return openApiDocument.Definitions;
        }

        public async Task<ApiVersionKindSchemas> LoadApiVersionKindSchemas()
        {
            var definitions = await _lazyDefinitions.Value;

            var schemas = new ApiVersionKindSchemas();

            foreach (var (_, definition) in definitions)
            {
                if (definition.ExtensionData?.TryGetValue("x-kubernetes-group-version-kind", out var _) ?? false)
                {
                    var groupVersionKindElements = (object[])definition.ExtensionData["x-kubernetes-group-version-kind"];
                    var groupVersionKind = (Dictionary<string, object>)groupVersionKindElements[0];

                    var group = (string)groupVersionKind["group"];
                    var version = (string)groupVersionKind["version"];
                    var kind = (string)groupVersionKind["kind"];

                    if (string.IsNullOrEmpty(group))
                    {
                        schemas[(version, kind)] = definition;
                    }
                    else
                    {
                        schemas[($"{group}/{version}", kind)] = definition;
                    }
                }
            }

            return schemas;
        }

        public class ApiVersionKindSchemas : Dictionary<(string apiVersion, string kind), JsonSchema>
        {
        }
    }
}
