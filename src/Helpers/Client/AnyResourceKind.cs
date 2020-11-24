﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

using k8s;
using k8s.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kubernetes.Helpers.Client
{
    public class AnyResourceKind : IServiceOperations<k8s.Kubernetes>, IAnyResourceKind
    {
        public AnyResourceKind(IKubernetes kubernetes)
        {
            Client = (k8s.Kubernetes)kubernetes;
        }

        public k8s.Kubernetes Client { get; }
        
        /// <inheritdoc/>
        public async Task<HttpOperationResponse<KubernetesList<TResource>>> ListClusterAnyResourceKindWithHttpMessagesAsync<TResource>(string group, string version, string plural, string continueParameter = null, string fieldSelector = null, string labelSelector = null, int? limit = null, string resourceVersion = null, int? timeoutSeconds = null, bool? watch = null, string pretty = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default) where TResource : IKubernetesObject
        {
            if (group == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "group");
            }
            if (version == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "version");
            }
            if (plural == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "plural");
            }
            // Tracing
            bool _shouldTrace = ServiceClientTracing.IsEnabled;
            string _invocationId = null;
            if (_shouldTrace)
            {
                _invocationId = ServiceClientTracing.NextInvocationId.ToString();
                Dictionary<string, object> tracingParameters = new Dictionary<string, object>();
                tracingParameters.Add("continueParameter", continueParameter);
                tracingParameters.Add("fieldSelector", fieldSelector);
                tracingParameters.Add("labelSelector", labelSelector);
                tracingParameters.Add("limit", limit);
                tracingParameters.Add("resourceVersion", resourceVersion);
                tracingParameters.Add("timeoutSeconds", timeoutSeconds);
                tracingParameters.Add("watch", watch);
                tracingParameters.Add("pretty", pretty);
                tracingParameters.Add("group", group);
                tracingParameters.Add("version", version);
                tracingParameters.Add("plural", plural);
                tracingParameters.Add("cancellationToken", cancellationToken);
                ServiceClientTracing.Enter(_invocationId, this, "ListClusterAnyResourceKind", tracingParameters);
            }
            // Construct URL
            var _baseUrl = Client.BaseUri.AbsoluteUri;
            var _url = new System.Uri(new System.Uri(_baseUrl + (_baseUrl.EndsWith("/") ? "" : "/")), "apis/{group}/{version}/{plural}").ToString();
            if (string.IsNullOrEmpty(group))
            {
                _url = _url.Replace("apis/{group}", "api");
            }
            else
            {
                _url = _url.Replace("{group}", System.Uri.EscapeDataString(group));
            }
            _url = _url.Replace("{version}", System.Uri.EscapeDataString(version));
            _url = _url.Replace("{plural}", System.Uri.EscapeDataString(plural));
            List<string> _queryParameters = new List<string>();
            if (continueParameter != null)
            {
                _queryParameters.Add(string.Format("continue={0}", System.Uri.EscapeDataString(continueParameter)));
            }
            if (fieldSelector != null)
            {
                _queryParameters.Add(string.Format("fieldSelector={0}", System.Uri.EscapeDataString(fieldSelector)));
            }
            if (labelSelector != null)
            {
                _queryParameters.Add(string.Format("labelSelector={0}", System.Uri.EscapeDataString(labelSelector)));
            }
            if (limit != null)
            {
                _queryParameters.Add(string.Format("limit={0}", System.Uri.EscapeDataString(SafeJsonConvert.SerializeObject(limit, Client.SerializationSettings).Trim('"'))));
            }
            if (resourceVersion != null)
            {
                _queryParameters.Add(string.Format("resourceVersion={0}", System.Uri.EscapeDataString(resourceVersion)));
            }
            if (timeoutSeconds != null)
            {
                _queryParameters.Add(string.Format("timeoutSeconds={0}", System.Uri.EscapeDataString(SafeJsonConvert.SerializeObject(timeoutSeconds, Client.SerializationSettings).Trim('"'))));
            }
            if (watch != null)
            {
                _queryParameters.Add(string.Format("watch={0}", System.Uri.EscapeDataString(SafeJsonConvert.SerializeObject(watch, Client.SerializationSettings).Trim('"'))));
            }
            if (pretty != null)
            {
                _queryParameters.Add(string.Format("pretty={0}", System.Uri.EscapeDataString(pretty)));
            }
            if (_queryParameters.Count > 0)
            {
                _url += "?" + string.Join("&", _queryParameters);
            }
            // Create HTTP transport objects
            var _httpRequest = new HttpRequestMessage();
            HttpResponseMessage _httpResponse = null;
            _httpRequest.Method = new HttpMethod("GET");
            _httpRequest.RequestUri = new System.Uri(_url);
            // Set Headers


            if (customHeaders != null)
            {
                foreach (var _header in customHeaders)
                {
                    if (_httpRequest.Headers.Contains(_header.Key))
                    {
                        _httpRequest.Headers.Remove(_header.Key);
                    }
                    _httpRequest.Headers.TryAddWithoutValidation(_header.Key, _header.Value);
                }
            }

            // Serialize Request
            string _requestContent = null;
            // Set Credentials
            if (Client.Credentials != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Client.Credentials.ProcessHttpRequestAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            }
            // Send Request
            if (_shouldTrace)
            {
                ServiceClientTracing.SendRequest(_invocationId, _httpRequest);
            }
            cancellationToken.ThrowIfCancellationRequested();
            _httpResponse = await Client.HttpClient.SendAsync(_httpRequest, cancellationToken).ConfigureAwait(false);
            if (_shouldTrace)
            {
                ServiceClientTracing.ReceiveResponse(_invocationId, _httpResponse);
            }
            HttpStatusCode _statusCode = _httpResponse.StatusCode;
            cancellationToken.ThrowIfCancellationRequested();
            string _responseContent = null;
            if ((int)_statusCode != 200)
            {
                var ex = new HttpOperationException(string.Format("Operation returned an invalid status code '{0}'", _statusCode));
                if (_httpResponse.Content != null)
                {
                    _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    _responseContent = string.Empty;
                }
                ex.Request = new HttpRequestMessageWrapper(_httpRequest, _requestContent);
                ex.Response = new HttpResponseMessageWrapper(_httpResponse, _responseContent);
                if (_shouldTrace)
                {
                    ServiceClientTracing.Error(_invocationId, ex);
                }
                _httpRequest.Dispose();
                if (_httpResponse != null)
                {
                    _httpResponse.Dispose();
                }
                throw ex;
            }
            // Create Result
            var _result = new HttpOperationResponse<KubernetesList<TResource>>();
            _result.Request = _httpRequest;
            _result.Response = _httpResponse;
            // Deserialize Response
            if ((int)_statusCode == 200)
            {
                _responseContent = await _httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    _result.Body = SafeJsonConvert.DeserializeObject<KubernetesList<TResource>>(_responseContent, Client.DeserializationSettings);
                }
                catch (Newtonsoft.Json.JsonException ex)
                {
                    _httpRequest.Dispose();
                    if (_httpResponse != null)
                    {
                        _httpResponse.Dispose();
                    }
                    throw new SerializationException("Unable to deserialize the response.", _responseContent, ex);
                }
            }
            if (_shouldTrace)
            {
                ServiceClientTracing.Exit(_invocationId, _result);
            }
            return _result;
        }
    }
}
