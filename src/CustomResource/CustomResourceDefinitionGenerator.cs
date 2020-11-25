// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;

namespace Microsoft.Kubernetes.CustomResources
{
    /// <summary>
    /// Class CustomResourceDefinitionGenerator generates CRD documents for .NET types.
    /// Implements the <see cref="ICustomResourceDefinitionGenerator" />.
    /// </summary>
    /// <seealso cref="ICustomResourceDefinitionGenerator" />.
    public class CustomResourceDefinitionGenerator : ICustomResourceDefinitionGenerator
    {
        private readonly JsonSchemaGeneratorSettings _jsonSchemaGeneratorSettings;
        private readonly JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomResourceDefinitionGenerator"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public CustomResourceDefinitionGenerator()
        {
            _jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings()
            {
                SchemaType = SchemaType.OpenApi3,
                TypeMappers =
                {
                    new ObjectTypeMapper(
                        typeof(V1ObjectMeta),
                        new JsonSchema4
                        {
                            Type = JsonObjectType.Object,
                        }),
                },
            };

            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new Iso8601TimeSpanConverter(),
                },
            };
        }

        /// <summary>
        /// generate custom resource definition as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource to generate.</typeparam>
        /// <param name="scope">The scope indicates whether the defined custom resource is cluster- or namespace-scoped. Allowed values are `Cluster` and `Namespaced`.</param>
        /// <returns>The generated V1CustomResourceDefinition instance.</returns>
        public async Task<V1CustomResourceDefinition> GenerateCustomResourceDefinitionAsync(Type resourceType, string scope)
        {
            var entity = resourceType.GetTypeInfo().GetCustomAttribute<KubernetesEntityAttribute>();

            var group = entity.Group;
            var version = entity.ApiVersion;
            var kind = entity.Kind;
            var plural = entity.PluralName;
            var name = $"{plural}.{group}";

            var schema = await GenerateJsonSchemaAsync(resourceType);

            return new V1CustomResourceDefinition(
                apiVersion: $"{V1CustomResourceDefinition.KubeGroup}/{V1CustomResourceDefinition.KubeApiVersion}",
                kind: V1CustomResourceDefinition.KubeKind,
                metadata: new V1ObjectMeta(
                    name: name),
                spec: new V1CustomResourceDefinitionSpec(
                    group: group,
                    names: new V1CustomResourceDefinitionNames(
                        kind: kind,
                        plural: plural),
                    scope: scope,
                    versions: new List<V1CustomResourceDefinitionVersion>
                    {
                        new V1CustomResourceDefinitionVersion(
                            name: version,
                            served: true,
                            storage: true,
                            schema: new V1CustomResourceValidation(schema)),
                    }));
        }

        /// <summary>
        /// generate custom resource definition as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TResource">The type of the resource to generate.</typeparam>
        /// <param name="scope">The scope indicates whether the defined custom resource is cluster- or namespace-scoped. Allowed values are `Cluster` and `Namespaced`.</param>
        /// <returns>The generated V1CustomResourceDefinition instance.</returns>
        public Task<V1CustomResourceDefinition> GenerateCustomResourceDefinitionAsync<TResource>(string scope)
        {
           return GenerateCustomResourceDefinitionAsync(typeof(TResource), scope);
        }

        private async Task<V1JSONSchemaProps> GenerateJsonSchemaAsync(Type resourceType)
        {
            // start with JsonSchema
            var schema = await JsonSchema4.FromTypeAsync(resourceType, _jsonSchemaGeneratorSettings);

            // convert to JToken to make alterations
            var rootToken = JObject.Parse(schema.ToJson());
            rootToken = RewriteObject(rootToken);
            rootToken.Remove("$schema");
            rootToken.Remove("definitions");

            // convert to k8s.Models.V1JSONSchemaProps to return
            using var reader = new JTokenReader(rootToken);
            return JsonSerializer
                .Create(_serializerSettings)
                .Deserialize<V1JSONSchemaProps>(reader);
        }

        private JObject RewriteObject(JObject sourceObject)
        {
            var targetObject = new JObject();

            var queue = new Queue<JObject>();
            queue.Enqueue(sourceObject);
            while (queue.Count != 0)
            {
                sourceObject = queue.Dequeue();
                foreach (var property in sourceObject.Properties())
                {
                    if (property.Name == "$ref")
                    {
                        // resolve the target of the "$ref"
                        var reference = sourceObject;
                        foreach (var part in property.Value.Value<string>().Split("/"))
                        {
                            if (part == "#")
                            {
                                reference = (JObject)reference.Root;
                            }
                            else
                            {
                                reference = (JObject)reference[part];
                            }
                        }

                        // the referenced object should be merged into the current target
                        queue.Enqueue(reference);

                        // and $ref property is not added
                        continue;
                    }

                    if (property.Name == "additionalProperties" &&
                        property.Value.Type == JTokenType.Boolean &&
                        property.Value.Value<bool>() == false)
                    {
                        // don't add this property when it has a default value
                        continue;
                    }

                    if (property.Name == "oneOf" &&
                        property.Value.Type == JTokenType.Array &&
                        property.Value.Children().Count() == 1)
                    {
                        // a single oneOf array item should be merged into current object
                        queue.Enqueue(RewriteObject(property.Value.Children().Cast<JObject>().Single()));

                        // and don't add the oneOf property
                        continue;
                    }

                    // all other properties are added after the value is rewritten recursively
                    if (!targetObject.ContainsKey(property.Name))
                    {
                        targetObject.Add(property.Name, RewriteToken(property.Value));
                    }
                }
            }

            return targetObject;
        }

        private JToken RewriteToken(JToken sourceToken)
        {
            if (sourceToken is JObject sourceObject)
            {
                return RewriteObject(sourceObject);
            }
            else if (sourceToken is JArray sourceArray)
            {
                return new JArray(sourceArray.Select(RewriteToken));
            }
            else
            {
                return sourceToken;
            }
        }
    }
}
