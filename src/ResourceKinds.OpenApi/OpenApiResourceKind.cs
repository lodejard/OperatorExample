// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Kubernetes.ResourceKinds;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Kubernetes.ResourceKindProvider.OpenApi
{
    public class OpenApiResourceKind : IResourceKind
    {
        private readonly Dictionary<JsonSchema, IResourceKindElement> _elements = new Dictionary<JsonSchema, IResourceKindElement>();
        private readonly object _elementsSync = new object();

        public OpenApiResourceKind(string apiVersion, string kind, JsonSchema jsonSchema)
        {
            ApiVersion = apiVersion;
            Kind = kind;
            Schema = BindElement(jsonSchema.ActualSchema);
        }

        public string ApiVersion { get; }

        public string Kind { get; }

        public IResourceKindElement Schema { get; }

        internal IResourceKindElement BindElement(JsonSchema schema)
        {
            lock (_elementsSync)
            {
                if (_elements.TryGetValue(schema, out var element))
                {
                    return element;
                }

                if (IsPrimative(schema))
                {
                    element = DefaultResourceKindElement.ReplacePrimative;
                }
                else if (schema.IsArray)
                {
                    var itemSchema = schema.Item.ActualSchema;
                    var hasMergePatchStrategy = HasPatchStrategy(schema, "merge");

                    if (IsPrimative(itemSchema))
                    {
                        if (hasMergePatchStrategy)
                        {
                            element = new OpenApiResourceKindElement(this, schema, ElementMergeStrategy.MergeListOfPrimative);
                        }
                        else
                        {
                            element = new OpenApiResourceKindElement(this, schema, ElementMergeStrategy.ReplaceListOfPrimative);
                        }
                    }
                    else
                    {
                        var collectionTypeSchema = schema.Item.HasReference ? schema.Item.Reference : schema.Item;

                        if (hasMergePatchStrategy && HasPatchMergeKey(schema, out var patchMergeKey))
                        {
                            element = new OpenApiResourceKindElement(this, schema, ElementMergeStrategy.MergeListOfObject, mergeKey: patchMergeKey as string);
                        }
                        else
                        {
                            element = new OpenApiResourceKindElement(this, schema, ElementMergeStrategy.ReplaceListOfObject);
                        }
                    }
                }
                else if (schema.IsDictionary)
                {
                    element = new OpenApiResourceKindElement(this, schema, ElementMergeStrategy.MergeMap);
                }
                else if (schema.IsObject)
                {
                    element = new OpenApiResourceKindElement(this, schema, ElementMergeStrategy.MergeObject);
                }
                else
                {
                    throw new NotImplementedException("Unable to process schema.");
                }

                _elements[schema] = element;
                return element;
            }
        }

        private bool HasPatchStrategy(JsonSchema schema, string value)
        {
            return
                schema.ExtensionData != null &&
                schema.ExtensionData.TryGetValue("x-kubernetes-patch-strategy", out var patchStrategy) &&
                (patchStrategy as string ?? string.Empty).Split(',').Any(part => part == value);
        }

        private bool HasPatchMergeKey(JsonSchema schema, out string mergeKey)
        {
            if (schema.ExtensionData != null &&
                schema.ExtensionData.TryGetValue("x-kubernetes-patch-merge-key", out var value) &&
                value is string stringValue)
            {
                mergeKey = stringValue;
                return true;
            }

            mergeKey = default;
            return false;
        }

        private bool IsPrimative(JsonSchema schema)
        {
            return
                schema.Type == JsonObjectType.String ||
                schema.Type == JsonObjectType.Integer ||
                schema.Type == JsonObjectType.Boolean ||
                schema.Type == JsonObjectType.Number;
        }
    }
}
