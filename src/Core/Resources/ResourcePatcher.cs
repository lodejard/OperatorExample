// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Microsoft.Kubernetes.Core.Resources
{
    public class ResourcePatcher : IResourcePatcher
    {
        private readonly IEqualityComparer<JToken> _tokenEqualityComparer = new JTokenEqualityComparer();

        public JsonPatchDocument CreateJsonPatch(CreateJsonPatchContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var state = new State(context);

            var patch = AccumulatePatch(new JsonPatchDocument(), state);

            return patch;
        }

        private JsonPatchDocument AccumulatePatch(JsonPatchDocument patch, State state)
        {
            return state.Element.MergeStrategy switch
            {
                ElementMergeStrategy.Unknown => ReplaceUnknown(patch, state),
                _ => throw new Exception("Unhandled merge strategy"),
            };
        }

        private JsonPatchDocument ReplaceUnknown(JsonPatchDocument patch, State state)
        {
            if (state.ApplyToken is JObject)
            {
                return MergeObject(patch, state);
            }
            else if (state.ApplyToken is JArray)
            {
                return ReplaceListOfObjectOrPrimative(patch, state);
            }
            else if (state.ApplyToken is JValue)
            {
                return ReplacePrimative(patch, state);
            }
            else if (state.ApplyToken == null && state.LastAppliedToken != null)
            {
                return patch.Remove(state.Path);
            }
            else
            {
                throw NewFormatException(state);
            }
        }

        private FormatException NewFormatException(State context)
        {
            return new FormatException($"{context.Kind.Kind}.{context.Kind.ApiVersion} {context.Path} type {context.ApplyToken?.Type} is incorrect for {context.Element?.MergeStrategy}");
        }

        private JsonPatchDocument ReplacePrimative(JsonPatchDocument patch, State context)
        {
            if (context.ApplyToken is JValue apply)
            {
                if (context.LiveToken is JValue live &&
                    _tokenEqualityComparer.Equals(apply, live))
                {
                    // live value is correct
                }
                else
                {
                    // live value is different, or live is not a primative value
                    patch = patch.Replace(context.Path, apply);
                }
            }
            else
            {
                throw NewFormatException(context);
            }

            return patch;
        }

        private JsonPatchDocument MergeObject(JsonPatchDocument patch, State state)
        {
            var apply = (JObject)state.ApplyToken;
            var lastApplied = state.LastAppliedToken as JObject;
            var live = state.LiveToken as JObject;

            if (live == null)
            {
                return patch.Replace(state.Path, apply);
            }

            foreach (var applyProperty in apply.Properties())
            {
                var name = applyProperty.Name;
                var path = $"{state.Path}/{EscapePath(name)}";

                var liveProperty = live.Property(name, StringComparison.Ordinal);

                if (liveProperty == null)
                {
                    patch = patch.Add(path, applyProperty.Value);
                }
                else
                {
                    var lastAppliedProperty = lastApplied?.Property(name, StringComparison.Ordinal);

                    var nested = state.Push(
                        path, 
                        state.Element.GetProperty(name), 
                        applyProperty.Value, 
                        lastAppliedProperty?.Value, 
                        liveProperty.Value);
                 
                    patch = AccumulatePatch(patch, nested);
                }
            }

            foreach (var liveProperty in live.Properties())
            {
                var name = liveProperty.Name;
                var applyProperty = apply.Property(name, StringComparison.Ordinal);
                
                if (applyProperty == null)
                {
                    var lastAppliedProperty = lastApplied?.Property(name, StringComparison.Ordinal);
                    if (lastAppliedProperty != null)
                    {
                        var path = $"{state.Path}/{EscapePath(name)}";
                        patch = patch.Remove(path);
                    }
                }
            }

            return patch;
        }

         private JsonPatchDocument ReplaceListOfObjectOrPrimative(JsonPatchDocument patch, State context)
        {
            if (context.ApplyToken is JArray apply)
            {
                if (context.LiveToken is JArray live &&
                    _tokenEqualityComparer.Equals(apply, live))
                {
                    // live is correct
                }
                else
                {
                    // live array has any differences, or live is not an array
                    patch = patch.Replace(context.Path, context.ApplyToken);
                }
            }
            else
            {
                throw NewFormatException(context);
            }

            return patch;
        }

        private string EscapePath(string name)
        {
            return name.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);
        }

        struct State
        {
            public State(CreateJsonPatchContext context)
            {
                Path = string.Empty;
                Kind = context.ResourceKind ?? DefaultResourceKind.Instance;
                Element = context.ResourceKind?.Schema ?? DefaultResourceKindElement.Instance;
                ApplyToken = (JToken)context.ApplyResource;
                LastAppliedToken = (JToken)context.LastAppliedResource;
                LiveToken = (JToken)context.LiveResource;
            }

            public State(string path, IResourceKind kind, IResourceKindElement element, JToken apply, JToken lastApplied, JToken live) : this()
            {
                Path = path;
                Kind = kind;
                Element = element;
                ApplyToken = apply;
                LastAppliedToken = lastApplied;
                LiveToken = live;
            }

            public string Path { get; }
            public IResourceKind Kind { get; }
            public IResourceKindElement Element { get; }
            public JToken ApplyToken { get; }
            public JToken LastAppliedToken { get; }
            public JToken LiveToken { get; }

            public State Push(string path, IResourceKindElement element, JToken apply, JToken lastApplied, JToken live)
            {
                return new State(path, Kind, element, apply, lastApplied, live);
            }
        }
    }
}
