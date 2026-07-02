using System.Collections.Generic;

using UnityEngine.Events;

using ViitorCloud.API.StandardTemplates;

namespace ViitorCloud.API {
    /// <summary>
    /// Example wrapper around <see cref="ServerCommunication"/>.
    /// </summary>
    /// <remarks>
    /// This class is intentionally small and is meant to show how to call the shared API client from gameplay code.
    /// You can keep adding methods here for your own endpoints, or call <see cref="ServerCommunication"/> directly.
    ///
    /// <example>
    /// <code>
    /// var api = ServerCommunicationTemplate.Instance;
    /// api.RequestLogin(
    ///     new Login { emailId = "user@example.com", password = "secret" },
    ///     response => UnityEngine.Debug.Log(response.data?.user?.emailId),
    ///     error => UnityEngine.Debug.LogError(error));
    /// </code>
    /// </example>
    /// </remarks>
    public class ServerCommunicationTemplate : MonoBehaviour {
        #region Auth Examples

        /// <summary>
        /// Example login request using the shared REST client.
        /// </summary>
        /// <param name="request">Login payload.</param>
        /// <param name="callbackOnSuccess">Called when the server returns a successful response.</param>
        /// <param name="callbackOnFail">Called when the request fails or the response cannot be parsed.</param>
        /// <param name="logMode">Controls how much logging this request should produce.</param>
        public void RequestLogin(Login request, UnityAction<APIResponse<LoginResponse>> callbackOnSuccess,
            UnityAction<string> callbackOnFail, ServerCommunication.RequestLogMode logMode = ServerCommunication.RequestLogMode.Default) {
            ServerCommunication.Instance.SendJsonRequest<Login, APIResponse<LoginResponse>>(
                ServerCommunication.HttpMethod.Post,
                Constants.API.Login,
                request,
                callbackOnSuccess,
                callbackOnFail,
                null,
                logMode);
        }

        /// <summary>
        /// Example register request using the shared REST client.
        /// </summary>
        /// <param name="request">Register payload.</param>
        /// <param name="callbackOnSuccess">Called when the server returns a successful response.</param>
        /// <param name="callbackOnFail">Called when the request fails or the response cannot be parsed.</param>
        /// <param name="logMode">Controls how much logging this request should produce.</param>
        public void RequestRegister(Register request, UnityAction<APIResponse<LoginResponse>> callbackOnSuccess,
            UnityAction<string> callbackOnFail, ServerCommunication.RequestLogMode logMode = ServerCommunication.RequestLogMode.Default) {
            ServerCommunication.Instance.SendJsonRequest<Register, APIResponse<LoginResponse>>(
                ServerCommunication.HttpMethod.Post,
                Constants.API.Register,
                request,
                callbackOnSuccess,
                callbackOnFail,
                null,
                logMode);
        }

        #endregion

        #region Generic Examples

        /// <summary>
        /// Sends any JSON body to any endpoint and deserializes the response.
        /// </summary>
        /// <typeparam name="TRequest">Request model type.</typeparam>
        /// <typeparam name="TResponse">Response model type.</typeparam>
        /// <param name="method">HTTP method to use.</param>
        /// <param name="url">Full endpoint URL.</param>
        /// <param name="request">Request payload.</param>
        /// <param name="callbackOnSuccess">Called when the server returns a successful response.</param>
        /// <param name="callbackOnFail">Called when the request fails or the response cannot be parsed.</param>
        /// <param name="logMode">Controls how much logging this request should produce.</param>
        public void RequestJson<TRequest, TResponse>(ServerCommunication.HttpMethod method, string url, TRequest request,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            ServerCommunication.RequestLogMode logMode = ServerCommunication.RequestLogMode.Default) where TRequest : class {
            ServerCommunication.Instance.SendJsonRequest(method, url, request, callbackOnSuccess, callbackOnFail, null, logMode);
        }

        /// <summary>
        /// Sends a request and returns the raw response text.
        /// </summary>
        /// <param name="method">HTTP method to use.</param>
        /// <param name="url">Full endpoint URL.</param>
        /// <param name="jsonBody">Optional JSON body.</param>
        /// <param name="callbackOnSuccess">Called with raw response text.</param>
        /// <param name="callbackOnFail">Called when the request fails.</param>
        /// <param name="additionalHeaders">Optional custom headers.</param>
        /// <param name="logMode">Controls how much logging this request should produce.</param>
        public void RequestRaw(ServerCommunication.HttpMethod method, string url, string jsonBody,
            UnityAction<string> callbackOnSuccess, UnityAction<string> callbackOnFail,
            IDictionary<string, string> additionalHeaders = null,
            ServerCommunication.RequestLogMode logMode = ServerCommunication.RequestLogMode.Default) {
            ServerCommunication.Instance.SendRequestRaw(method, url, jsonBody, callbackOnSuccess, callbackOnFail,
                additionalHeaders, logMode);
        }

        /// <summary>
        /// Sends a GET request and deserializes the response.
        /// </summary>
        /// <typeparam name="TResponse">Response model type.</typeparam>
        /// <param name="url">Full endpoint URL.</param>
        /// <param name="callbackOnSuccess">Called when the server returns a successful response.</param>
        /// <param name="callbackOnFail">Called when the request fails or the response cannot be parsed.</param>
        /// <param name="logMode">Controls how much logging this request should produce.</param>
        public void RequestGet<TResponse>(string url, UnityAction<TResponse> callbackOnSuccess,
            UnityAction<string> callbackOnFail, ServerCommunication.RequestLogMode logMode = ServerCommunication.RequestLogMode.Default) {
            ServerCommunication.Instance.SendRequestGet(url, callbackOnSuccess, callbackOnFail, logMode);
        }

        /// <summary>
        /// Sends a DELETE request.
        /// </summary>
        /// <param name="url">Full endpoint URL.</param>
        /// <param name="callbackOnSuccess">Called when the request succeeds.</param>
        /// <param name="callbackOnFail">Called when the request fails.</param>
        /// <param name="logMode">Controls how much logging this request should produce.</param>
        public void RequestDelete(string url, UnityAction callbackOnSuccess, UnityAction<string> callbackOnFail,
            ServerCommunication.RequestLogMode logMode = ServerCommunication.RequestLogMode.Default) {
            ServerCommunication.Instance.SendRequestDelete(url, callbackOnSuccess, callbackOnFail, logMode);
        }

        #endregion

        #region Upload Examples

        /// <summary>
        /// Example single-file upload.
        /// </summary>
        /// <typeparam name="TResponse">Response model type.</typeparam>
        /// <param name="fieldName">Multipart field name.</param>
        /// <param name="filePath">File path to upload.</param>
        /// <param name="url">Full endpoint URL.</param>
        /// <param name="callbackOnSuccess">Called when upload succeeds.</param>
        /// <param name="callbackOnFail">Called when upload fails.</param>
        /// <param name="logMode">Controls how much logging this request should produce.</param>
        public void RequestUpload<TResponse>(string fieldName, string filePath, string url,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            ServerCommunication.RequestLogMode logMode = ServerCommunication.RequestLogMode.Default) {
            ServerCommunication.Instance.SendRequestPostWithFile(fieldName, filePath, url, callbackOnSuccess,
                callbackOnFail, logMode);
        }

        /// <summary>
        /// Example multi-file upload.
        /// </summary>
        /// <typeparam name="TResponse">Response model type.</typeparam>
        /// <param name="url">Full endpoint URL.</param>
        /// <param name="files">Files to upload.</param>
        /// <param name="callbackOnSuccess">Called when upload succeeds.</param>
        /// <param name="callbackOnFail">Called when upload fails.</param>
        /// <param name="logMode">Controls how much logging this request should produce.</param>
        public void RequestUpload<TResponse>(string url, List<ServerCommunication.FileUpload> files,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            ServerCommunication.RequestLogMode logMode = ServerCommunication.RequestLogMode.Default) {
            ServerCommunication.Instance.SendRequestPostWithMultiFile(url, files, callbackOnSuccess, callbackOnFail,
                logMode);
        }

        #endregion
    }
}
