// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using k8s.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kubernetes.Controller.Hosting;
using Microsoft.Kubernetes.Controller.Informers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Operator
{
    public class OperatorHandler<TResource> : BackgroundHostedService, IOperatorHandler<TResource>
    {
        private readonly List<IResourceInformerRegistration> _registrations = new List<IResourceInformerRegistration>();

        public OperatorHandler(
            IOptionsSnapshot<OperatorOptions> optionsSnapshot,
            IOptions<OperatorOptions> options2,
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<OperatorHandler<TResource>> logger)
            : base(hostApplicationLifetime, logger)
        {
            if (optionsSnapshot is null)
            {
                throw new ArgumentNullException(nameof(optionsSnapshot));
            }

            var options = optionsSnapshot.Get(typeof(TResource).Name);

            foreach (var informer in options.Informers)
            {
                _registrations.Add(informer.Register(Notification));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var registration in _registrations)
                {
                    registration.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void Notification(WatchEventType eventType, IKubernetesObject<V1ObjectMeta> resource)
        {
        }

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            foreach (var registration in _registrations)
            {
                await registration.ReadyAsync(cancellationToken);
            }

            await Task.Delay(int.MaxValue, cancellationToken);
        }
    }
}
