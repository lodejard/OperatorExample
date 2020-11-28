// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
                ElementMergeStrategy.Unknown => MergeApplyAny(patch, state),
                ElementMergeStrategy.ReplacePrimative => ReplacePrimative(patch, state),

                ElementMergeStrategy.MergeObject => MergeObject(patch, state),
                ElementMergeStrategy.MergeMap => MergeMap(patch, state),

                ElementMergeStrategy.MergeListOfPrimative => MergeListOfPrimative(patch, state),
                ElementMergeStrategy.ReplaceListOfPrimative => ReplaceListOfPrimative(patch, state),
                ElementMergeStrategy.MergeListOfObject => MergeListOfObject(patch, state),
                ElementMergeStrategy.ReplaceListOfObject => ReplaceListOfObject(patch, state),

                _ => throw new Exception("Unhandled merge strategy"),
            };
        }

        private JsonPatchDocument MergeApplyAny(JsonPatchDocument patch, State state)
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
                        state.Element.GetPropertyElementType(name),
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

        private JsonPatchDocument MergeMap(JsonPatchDocument patch, State state)
        {
            var apply = (JObject)state.ApplyToken;
            var lastApplied = state.LastAppliedToken as JObject;
            var live = state.LiveToken as JObject;

            if (live == null)
            {
                return patch.Replace(state.Path, apply);
            }

            var collectionElement = state.Element.GetCollectionElementType();

            foreach (var applyProperty in apply.Properties())
            {
                var key = applyProperty.Name;
                var path = $"{state.Path}/{EscapePath(key)}";

                var liveProperty = live.Property(key, StringComparison.Ordinal);

                if (liveProperty == null)
                {
                    patch = patch.Add(path, applyProperty.Value);
                }
                else
                {
                    var lastAppliedProperty = lastApplied?.Property(key, StringComparison.Ordinal);

                    var nested = state.Push(
                        path,
                        collectionElement,
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

        private JsonPatchDocument MergeListOfPrimative(JsonPatchDocument patch, State context)
        {
            if (!(context.ApplyToken is JArray applyArray))
            {
                throw NewFormatException(context);
            }

            if (!(context.LiveToken is JArray liveArray))
            {
                // live is not an array, so replace it
                return patch.Replace(context.Path, applyArray);
            }

            List<JToken> lastAppliedList;
            if (context.LastAppliedToken is JArray lastAppliedArray)
            {
                lastAppliedList = lastAppliedArray.ToList();
            }
            else
            {
                lastAppliedList = new List<JToken>();
            }

            var applyEnumerator = applyArray.GetEnumerator();
            var applyIndex = 0;
            var applyAvailable = applyIndex < applyArray.Count;
            var applyValue = applyAvailable ? applyArray[applyIndex] : null;

            var liveIndex = 0;
            foreach (var liveValue in liveArray)
            {
                // match live value to remaining last applied values
                var lastAppliedIndex = lastAppliedList.FindIndex(lastAppliedValue => _tokenEqualityComparer.Equals(lastAppliedValue, liveValue));
                var wasLastApplied = lastAppliedIndex != -1;
                if (wasLastApplied)
                {
                    // remove from last applied list to preserve the number of live values that are accounted for
                    lastAppliedList.RemoveAt(lastAppliedIndex);
                }

                if (applyAvailable && _tokenEqualityComparer.Equals(applyValue, liveValue))
                {
                    // next live value matches next apply value in order, take no action and advance
                    liveIndex++;
                    applyIndex++;
                    applyAvailable = applyIndex < applyArray.Count;
                    applyValue = applyAvailable ? applyArray[applyIndex] : null;
                }
                else if (wasLastApplied)
                {
                    // next live value matches last applied, but is either removed or does not match next apply value
                    patch = patch.Remove($"{context.Path}/{liveIndex}");
                }
                else
                {
                    // next live value is not controlled by last applied, so take no action and advance live
                    liveIndex++;
                }
            }

            var path = $"{context.Path}/-";
            while (applyAvailable)
            {
                // remaining apply values are appended
                patch = patch.Add(path, applyValue);

                applyIndex++;
                applyAvailable = applyIndex < applyArray.Count;
                applyValue = applyAvailable ? applyArray[applyIndex] : null;
            }

            return patch;
        }

        private JsonPatchDocument ReplaceListOfPrimative(JsonPatchDocument patch, State state)
        {
            return ReplaceListOfObjectOrPrimative(patch, state);
        }

        private JsonPatchDocument MergeListOfObject(JsonPatchDocument patch, State context)
        {
            if (!(context.ApplyToken is JArray apply))
            {
                throw NewFormatException(context);
            }

            if (!(context.LiveToken is JArray live))
            {
                // live is not an array, so replace it
                return patch.Replace(context.Path, apply);
            }

            var applyItems = apply.Select((item, index) => (name: item[context.Element.MergeKey]?.Value<string>(), index, item)).ToArray();
            var liveItems = live.Select((item, index) => (name: item[context.Element.MergeKey]?.Value<string>(), index, item)).ToArray();
            var lastAppliedItems = context.LastAppliedToken?.Select((item, index) => (name: item[context.Element.MergeKey]?.Value<string>(), index, item))?.ToArray() ?? Array.Empty<(string name, int index, JToken item)>();

            var element = context.Element.GetCollectionElementType();

            foreach (var (name, _, applyToken) in applyItems)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new Exception("Merge key is required on object");
                }

                var (_, index, liveToken) = liveItems.SingleOrDefault(item => item.name == name);
                var (_, _, lastAppliedToken) = lastAppliedItems.SingleOrDefault(item => item.name == name);

                if (liveToken != null)
                {
                    var itemState = context.Push(
                        path: $"{context.Path}/{index}",
                        element: element,
                        apply: applyToken,
                        lastApplied: lastAppliedToken,
                        live: liveToken);

                    patch = AccumulatePatch(patch, itemState);
                }
                else
                {
                    patch = patch.Add($"{context.Path}/-", applyToken);
                }
            }

            foreach (var (name, _, lastApplyToken) in lastAppliedItems)
            {
                var (_, index, liveToken) = liveItems.SingleOrDefault(item => item.name == name);
                var (_, _, applyToken) = applyItems.SingleOrDefault(item => item.name == name);

                if (applyToken == null && liveToken != null)
                {
                    patch = patch.Remove($"{context.Path}/{index}");
                }
            }

            return patch;
        }

        private JsonPatchDocument ReplaceListOfObject(JsonPatchDocument patch, State state)
        {
            return ReplaceListOfObjectOrPrimative(patch, state);
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

        private struct State
        {
            public State(CreateJsonPatchContext context)
            {
                Path = string.Empty;
                Kind = context.ResourceKind ?? DefaultResourceKind.Unknown;
                Element = context.ResourceKind?.Schema ?? DefaultResourceKindElement.Unknown;
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
