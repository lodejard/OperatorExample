// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Kubernetes.Testing.Models
{
    public class ListResult
    {
        public string Continue { get; set; }

        public string ResourceVersion { get; set; }

        public IList<ResourceObject> Items { get; set; }
    }
}
