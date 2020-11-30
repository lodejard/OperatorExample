// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Kubernetes.Operator
{
    public struct ReconcileResult
    {
        public bool Requeue { get; set; }
        public TimeSpan RequeueAfter { get; set; }
        public Exception Error { get; set; }
    }
}
