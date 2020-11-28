// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Kubernetes.Core.Resources
{
    public enum ElementMergeStrategy
    {
        /// <summary>
        /// Unknown json schema are handled by MergeObject, ReplacePrimative, and ReplaceListOfPrimative.
        /// </summary>
        Unknown,

        /// <summary>
        /// Updating object by matching property names.
        /// </summary>
        MergeObject,

        /// <summary>
        /// Updating primative by replacing when different.
        /// </summary>
        ReplacePrimative,

        /// <summary>
        /// Updating dictionary by matching keys.
        /// </summary>
        MergeMap,
        MergeListOfPrimative,
        ReplaceListOfPrimative,
        MergeListOfObject,
        ReplaceListOfObject,
    }
}
