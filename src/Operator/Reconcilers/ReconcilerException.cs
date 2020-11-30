﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s.Models;
using Microsoft.Rest;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Kubernetes.Operator.Reconcilers
{
    [Serializable]
    internal class ReconcilerException : Exception
    {
        public ReconcilerException()
        {
        }

        public ReconcilerException(string message) : base(message)
        {
        }

        public ReconcilerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ReconcilerException(V1Status status) : base(status.Message)
        {
            Status = status;
        }

        public ReconcilerException(V1Status status, Exception innerException) : base(status.Message, innerException)
        {
            Status = status;
        }

        protected ReconcilerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public V1Status Status { get; }
    }
}
