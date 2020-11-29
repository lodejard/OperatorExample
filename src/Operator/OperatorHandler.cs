// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kubernetes.Controller.Informers;
using Microsoft.Kubernetes.Controller.Queues;
using Microsoft.Kubernetes.Operator.Caches;
using Microsoft.Kubernetes.Operator.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Operator
{
    public class OperatorHandler<TResource> : IOperatorHandler<TResource>
        where TResource : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        private static GroupApiVersionKind _names = GroupApiVersionKind.From<TResource>();
        private readonly List<IResourceInformerRegistration> _registrations = new List<IResourceInformerRegistration>();
        private readonly IRateLimitingQueue<NamespacedName> _queue;
        private readonly IOperatorCache<TResource> _cache;
        private readonly IGenerator<TResource> _generator;
        private readonly IReconciler<TResource> _reconciler;
        private readonly ILogger<OperatorHandler<TResource>> _logger;
        private bool _disposedValue;

        public OperatorHandler(
            IOptionsSnapshot<OperatorOptions> optionsSnapshot,
            IOperatorCache<TResource> cache,
            IGenerator<TResource> generator,
            IReconciler<TResource> reconciler,
            ILogger<OperatorHandler<TResource>> logger)
        {
            if (optionsSnapshot is null)
            {
                throw new ArgumentNullException(nameof(optionsSnapshot));
            }

            var options = optionsSnapshot.Get(_names.PluralNameGroup);

            foreach (var informer in options.Informers)
            {
                _registrations.Add(informer.Register(Notification));
            }

            var rateLimiter = options.NewRateLimiter();
            _queue = options.NewRateLimitingQueue(rateLimiter);
            _cache = cache;
            _generator = generator;
            _reconciler = reconciler;
            _logger = logger;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var registration in _registrations)
                    {
                        registration.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Notification(WatchEventType eventType, IKubernetesObject<V1ObjectMeta> resource)
        {
            if (resource is TResource customResource)
            {
                OnPrimaryResourceWatchEvent(eventType, customResource);
            }
            else
            {
                OnRelatedResourceWatchEvent(eventType, resource);
            }
        }

        private void OnPrimaryResourceWatchEvent(WatchEventType watchEventType, TResource resource)
        {
            var key = NamespacedName.From(resource);

            _cache.UpdateWorkItem(key, workItem =>
            {
                if (watchEventType == WatchEventType.Added || watchEventType == WatchEventType.Modified)
                {
                    workItem = workItem.SetResource(resource);
                }
                else if (watchEventType == WatchEventType.Deleted)
                {
                    workItem = workItem.SetResource(default);
                }

                _queue.Add(key);
                return workItem;
            });
        }

        private void OnRelatedResourceWatchEvent(WatchEventType watchEventType, IKubernetesObject<V1ObjectMeta> resource)
        {
            // Check each owner reference on the notified resource
            foreach (var ownerReference in resource.OwnerReferences() ?? Enumerable.Empty<V1OwnerReference>())
            {
                // If this operator's resource type is an owner
                if (string.Equals(ownerReference.Kind, _names.Kind, StringComparison.Ordinal) &&
                    string.Equals(ownerReference.ApiVersion, _names.GroupApiVersion, StringComparison.Ordinal))
                {
                    // Then hold on to the resource's current state under the owner's cache entry

                    var resourceKey = new NamespacedName(
                        @namespace: resource.Namespace(),
                        name: ownerReference.Name);

                    var relatedKey = GroupKindNamespacedName.From(resource);

                    _cache.UpdateWorkItem(resourceKey, workItem =>
                    {
                        if (watchEventType == WatchEventType.Added || watchEventType == WatchEventType.Modified)
                        {
                            workItem = workItem.SetRelated(workItem.Related.SetItem(relatedKey, resource));
                        }
                        else if (watchEventType == WatchEventType.Deleted)
                        {
                            workItem = workItem.SetRelated(workItem.Related.Remove(relatedKey));
                        }

                        _queue.Add(resourceKey);
                        return workItem;
                    });
                }
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(new EventId(1, "WaitingForInformers"), "Waiting for resource informers to finish synchronizing.");
            foreach (var registration in _registrations)
            {
                await registration.ReadyAsync(cancellationToken);
            }
            _logger.LogInformation(new EventId(2, "InformersReady"), "All resource informers are ready.");

            while (await ProcessNextWorkItemAsync(cancellationToken))
            {
                // loop until complete
            }
        }

        /// <summary>
        /// processNextWorkItem will read a single work item off the workqueue and attempt to process it, by calling the reconcileHandler.
        /// </summary>
        private async Task<bool> ProcessNextWorkItemAsync(CancellationToken cancellationToken)
        {
            // pkg\internal\controller\controller.go:194
            if (cancellationToken.IsCancellationRequested)
            {
                // Stop working
                return false;
            }

            var (key, shutdown) = await _queue.GetAsync(cancellationToken);
            if (shutdown || cancellationToken.IsCancellationRequested)
            {
                // Stop working
                return false;
            }

            try
            {
                return await ReconcileWorkItemAsync(key, cancellationToken);
            }
            finally
            {
                // We call Done here so the workqueue knows we have finished
                // processing this item. We also must remember to call Forget if we
                // do not want this work item being re-queued. For example, we do
                // not call Forget if a transient error occurs, instead the item is
                // put back on the workqueue and attempted again after a back-off
                // period.
                _queue.Done(key);
            }
        }

        private async Task<bool> ReconcileWorkItemAsync(NamespacedName key, CancellationToken cancellationToken)
        {
            return true;
        }
    }
}
