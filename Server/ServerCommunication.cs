using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

using ViitorCloud.API.StandardTemplates;

namespace ViitorCloud.API {
    /// <summary>
    /// Handles REST API requests for the package.
    /// </summary>
    public class ServerCommunication : MonoBehaviour {
        private const int MaxLoggedBodyLength = 512;

        #region Singleton

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        public static ServerCommunication Instance;

        /// <summary>
        /// Selected server environment.
        /// </summary>
        public API.Constants.API.Server server = API.Constants.API.Server.Development;

        /// <summary>
        /// Enables verbose request logging.
        /// </summary>
        public static bool debug;

        /// <summary>
        /// Optional bearer token used for authenticated requests.
        /// </summary>
        public static string ViitorCloudToken = string.Empty;

        [SerializeField]
        [Tooltip("Default timeout in seconds for every request.")]
        private int defaultTimeout = 20;

        private void Awake() {
            Singleton();
        }

        private void Singleton() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LogInfo("Initialized.");
                return;
            }

            if (Instance != this) {
                LogWarning("Duplicate instance detected. Destroying the extra object.");
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sends a GET request and parses the response as <typeparamref name="TResponse"/>.
        /// </summary>
        public void SendRequestGet<TResponse>(string url, UnityAction<TResponse> callbackOnSuccess,
            UnityAction<string> callbackOnFail, RequestLogMode logMode = RequestLogMode.Default) {
            StartCoroutine(SendRequestCoroutine(HttpMethod.Get, url, null, callbackOnSuccess, callbackOnFail, logMode));
        }

        /// <summary>
        /// Sends a DELETE request.
        /// </summary>
        public void SendRequestDelete(string url, UnityAction callbackOnSuccess,
            UnityAction<string> callbackOnFail, RequestLogMode logMode = RequestLogMode.Default) {
            StartCoroutine(SendDeleteCoroutine(url, callbackOnSuccess, callbackOnFail, logMode));
        }

        /// <summary>
        /// Sends a POST request with JSON body and parses the response as <typeparamref name="TResponse"/>.
        /// </summary>
        public void SendRequestPost<TResponse>(string form, string url, UnityAction<TResponse> callbackOnSuccess,
            UnityAction<string> callbackOnFail, RequestLogMode logMode = RequestLogMode.Default) {
            StartCoroutine(SendRequestCoroutine(HttpMethod.Post, url, form, callbackOnSuccess, callbackOnFail, logMode));
        }

        /// <summary>
        /// Sends a PUT request with JSON body and parses the response as <typeparamref name="TResponse"/>.
        /// </summary>
        public void SendRequestPut<TResponse>(string form, string url, UnityAction<TResponse> callbackOnSuccess,
            UnityAction<string> callbackOnFail, RequestLogMode logMode = RequestLogMode.Default) {
            StartCoroutine(SendRequestCoroutine(HttpMethod.Put, url, form, callbackOnSuccess, callbackOnFail, logMode));
        }

        /// <summary>
        /// Uploads a single file using multipart form data and parses the response as <typeparamref name="TResponse"/>.
        /// </summary>
        public void SendRequestPostWithFile<TResponse>(string fieldName, string filePath, string url,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            RequestLogMode logMode = RequestLogMode.Default) {
            var files = new List<FileUpload> {
                new FileUpload {
                    fieldName = fieldName,
                    filePath = filePath
                }
            };

            StartCoroutine(SendMultipartCoroutine(url, files, callbackOnSuccess, callbackOnFail, logMode));
        }

        /// <summary>
        /// Uploads multiple files using multipart form data and parses the response as <typeparamref name="TResponse"/>.
        /// </summary>
        public void SendRequestPostWithMultiFile<TResponse>(string url, List<FileUpload> files,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            RequestLogMode logMode = RequestLogMode.Default) {
            StartCoroutine(SendMultipartCoroutine(url, files, callbackOnSuccess, callbackOnFail, logMode));
        }

        /// <summary>
        /// Sends a request with a strongly typed request and response body.
        /// </summary>
        public void SendJsonRequest<TRequest, TResponse>(HttpMethod method, string url, TRequest requestBody,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            RequestLogMode logMode = RequestLogMode.Default,
            IDictionary<string, string> additionalHeaders = null) where TRequest : class {
            string json = requestBody == null ? string.Empty : JsonUtility.ToJson(requestBody);
            StartCoroutine(SendRequestCoroutine(method, url, json, callbackOnSuccess, callbackOnFail, additionalHeaders,
                logMode));
        }

        /// <summary>
        /// Sends a request and returns the raw response text.
        /// </summary>
        public void SendRequestRaw(HttpMethod method, string url, string jsonBody,
            UnityAction<string> callbackOnSuccess, UnityAction<string> callbackOnFail,
            RequestLogMode logMode = RequestLogMode.Default,
            IDictionary<string, string> additionalHeaders = null) {
            StartCoroutine(SendRawRequestCoroutine(method, url, jsonBody, callbackOnSuccess, callbackOnFail,
                additionalHeaders, logMode));
        }

        #endregion

        #region Coroutines

        private IEnumerator SendRequestCoroutine<TResponse>(HttpMethod method, string url, string jsonBody,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            RequestLogMode logMode = RequestLogMode.Default,
            IDictionary<string, string> additionalHeaders = null) {
            using (UnityWebRequest request = CreateRequest(method, url, jsonBody, logMode)) {
                bool includeJsonContentType = method == HttpMethod.Post || method == HttpMethod.Put;
                ApplyHeaders(request, additionalHeaders, includeJsonContentType);
                LogRequestStart(method, url, jsonBody, logMode);
                yield return request.SendWebRequest();
                HandleTypedResponse(request, method, url, callbackOnSuccess, callbackOnFail, logMode);
            }
        }

        private IEnumerator SendRawRequestCoroutine(HttpMethod method, string url, string jsonBody,
            UnityAction<string> callbackOnSuccess, UnityAction<string> callbackOnFail,
            RequestLogMode logMode = RequestLogMode.Default,
            IDictionary<string, string> additionalHeaders = null) {
            using (UnityWebRequest request = CreateRequest(method, url, jsonBody, logMode)) {
                bool includeJsonContentType = method == HttpMethod.Post || method == HttpMethod.Put;
                ApplyHeaders(request, additionalHeaders, includeJsonContentType);
                LogRequestStart(method, url, jsonBody, logMode);
                yield return request.SendWebRequest();
                HandleRawResponse(request, method, url, callbackOnSuccess, callbackOnFail, logMode);
            }
        }

        private IEnumerator SendDeleteCoroutine(string url, UnityAction callbackOnSuccess,
            UnityAction<string> callbackOnFail, RequestLogMode logMode = RequestLogMode.Default) {
            using (UnityWebRequest request = UnityWebRequest.Delete(url)) {
                request.timeout = defaultTimeout;
                ApplyHeaders(request, null, includeJsonContentType: false);
                LogRequestStart(HttpMethod.Delete, url, null, logMode);
                yield return request.SendWebRequest();

                if (IsRequestError(request)) {
                    LogRequestFailure(HttpMethod.Delete, url, request, logMode);
                    callbackOnFail?.Invoke(ExtractErrorMessage(request));
                    yield break;
                }

                LogRequestSuccess(HttpMethod.Delete, url, request, logMode);
                callbackOnSuccess?.Invoke();
            }
        }

        private IEnumerator SendMultipartCoroutine<TResponse>(string url, List<FileUpload> files,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            RequestLogMode logMode = RequestLogMode.Default) {
            if (files == null || files.Count == 0) {
                callbackOnFail?.Invoke("No files were provided.");
                yield break;
            }

            WWWForm form = new WWWForm();

            for (int i = 0; i < files.Count; i++) {
                FileUpload file = files[i];
                if (file == null || string.IsNullOrWhiteSpace(file.filePath)) {
                    callbackOnFail?.Invoke("Invalid file upload entry.");
                    yield break;
                }

                if (!File.Exists(file.filePath)) {
                    LogError("File does not exist: " + file.filePath);
                    callbackOnFail?.Invoke("File does not exist: " + file.filePath);
                    yield break;
                }

                yield return WaitForUnlockedFileCoroutine(file.filePath, callbackOnFail, logMode);
                if (IsFileLocked(file.filePath)) {
                    yield break;
                }

                byte[] fileData = File.ReadAllBytes(file.filePath);
                string mimeType = GetMimeType(file.filePath);
                string fieldName = string.IsNullOrWhiteSpace(file.fieldName) ? "file" : file.fieldName;
                form.AddBinaryData(fieldName, fileData, Path.GetFileName(file.filePath), mimeType);
                if (ShouldLogVerbose(logMode)) {
                    LogInfo($"Prepared upload file: field='{fieldName}', path='{file.filePath}', mime='{mimeType}', size={fileData.Length} bytes");
                }
            }

            using (UnityWebRequest request = UnityWebRequest.Post(url, form)) {
                request.timeout = defaultTimeout;
                ApplyHeaders(request, null, includeJsonContentType: false);
                LogRequestStart(HttpMethod.Post, url, $"multipart[{files.Count}]", logMode);
                yield return request.SendWebRequest();

                HandleTypedResponse(request, HttpMethod.Post, url, callbackOnSuccess, callbackOnFail, logMode);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Represents an upload payload for multipart requests.
        /// </summary>
        [Serializable]
        public class FileUpload {
            /// <summary>
            /// Form field name used by the server.
            /// </summary>
            public string fieldName;

            /// <summary>
            /// Absolute or project-relative file path.
            /// </summary>
            public string filePath;
        }

        private UnityWebRequest CreateRequest(HttpMethod method, string url, string jsonBody, RequestLogMode logMode) {
            if (ShouldLogVerbose(logMode)) {
                LogInfo($"{method} {url} body: {FormatBodyForLog(jsonBody)}");
            }

            UnityWebRequest request;
            switch (method) {
                case HttpMethod.Get:
                    request = UnityWebRequest.Get(url);
                    break;
                case HttpMethod.Delete:
                    request = UnityWebRequest.Delete(url);
                    break;
                default:
                    byte[] bodyRaw = string.IsNullOrEmpty(jsonBody) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(jsonBody);
                    request = new UnityWebRequest(url, method.ToUnityVerb()) {
                        uploadHandler = new UploadHandlerRaw(bodyRaw),
                        downloadHandler = new DownloadHandlerBuffer()
                    };
                    break;
            }

            request.timeout = defaultTimeout;
            return request;
        }

        private void ApplyHeaders(UnityWebRequest request, IDictionary<string, string> additionalHeaders,
            bool includeJsonContentType = true) {
            if (includeJsonContentType) {
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.SetRequestHeader("Accept", "application/json");

            if (!string.IsNullOrEmpty(ViitorCloudToken)) {
                request.SetRequestHeader("Authorization", "Bearer " + ViitorCloudToken);
            }

            if (additionalHeaders == null) {
                return;
            }

            foreach (KeyValuePair<string, string> header in additionalHeaders) {
                if (!string.IsNullOrWhiteSpace(header.Key)) {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
        }

        private void HandleTypedResponse<TResponse>(UnityWebRequest request, HttpMethod method, string url,
            UnityAction<TResponse> callbackOnSuccess, UnityAction<string> callbackOnFail,
            RequestLogMode logMode) {
            if (IsRequestError(request)) {
                LogRequestFailure(method, url, request, logMode);
                callbackOnFail?.Invoke(ExtractErrorMessage(request));
                return;
            }

            string responseText = request.downloadHandler?.text;
            if (string.IsNullOrWhiteSpace(responseText)) {
                callbackOnFail?.Invoke("Server returned an empty response.");
                return;
            }

            if (ShouldLogVerbose(logMode)) {
                LogInfo($"Response from {url}: {FormatBodyForLog(responseText)}");
            }

            try {
                callbackOnSuccess?.Invoke(ParseResponse<TResponse>(responseText));
                LogRequestSuccess(method, url, request, logMode);
            } catch (Exception exception) {
                LogError($"Failed to parse response from {url}: {exception.Message}");
                callbackOnFail?.Invoke("Unable to parse server response.");
            }
        }

        private void HandleRawResponse(UnityWebRequest request, HttpMethod method, string url, UnityAction<string> callbackOnSuccess,
            UnityAction<string> callbackOnFail, RequestLogMode logMode) {
            if (IsRequestError(request)) {
                LogRequestFailure(method, url, request, logMode);
                callbackOnFail?.Invoke(ExtractErrorMessage(request));
                return;
            }

            string responseText = request.downloadHandler?.text ?? string.Empty;
            if (ShouldLogVerbose(logMode)) {
                LogInfo($"Response from {url}: {FormatBodyForLog(responseText)}");
            }

            LogRequestSuccess(method, url, request, logMode);
            callbackOnSuccess?.Invoke(responseText);
        }

        private static bool IsRequestError(UnityWebRequest request) {
            return request.result == UnityWebRequest.Result.DataProcessingError ||
                   request.result == UnityWebRequest.Result.ConnectionError ||
                   request.result == UnityWebRequest.Result.ProtocolError;
        }

        private static string ExtractErrorMessage(UnityWebRequest request) {
            string responseText = request.downloadHandler?.text;
            if (string.IsNullOrWhiteSpace(responseText)) {
                return string.IsNullOrWhiteSpace(request.error) ? "Unknown request error." : request.error;
            }

            try {
                APIResponse apiResponse = JsonUtility.FromJson<APIResponse>(responseText);
                if (apiResponse != null && !string.IsNullOrWhiteSpace(apiResponse.message)) {
                    return apiResponse.message;
                }
            } catch {
                // Fallback below.
            }

            return string.IsNullOrWhiteSpace(request.error) ? responseText : $"{request.error}: {responseText}";
        }

        private static T ParseResponse<T>(string data) {
            data = NormalizeMongoJson(data);
            return JsonUtility.FromJson<T>(data);
        }

        private static string NormalizeMongoJson(string data) {
            if (string.IsNullOrEmpty(data)) {
                return data;
            }

            return data.Replace("$oid", "oid")
                .Replace("$date", "date");
        }

        private static IEnumerator WaitForUnlockedFileCoroutine(string filePath, UnityAction<string> callbackOnFail,
            RequestLogMode logMode) {
            const int maxAttempts = 60;
            const float delaySeconds = 0.5f;

            for (int attempt = 0; attempt < maxAttempts; attempt++) {
                if (!IsFileLocked(filePath)) {
                    yield break;
                }

                if (ShouldLogVerbose(logMode)) {
                    LogInfo($"File {filePath} is locked, waiting...");
                }

                yield return new WaitForSeconds(delaySeconds);
            }

            callbackOnFail?.Invoke($"File is locked and could not be read in time: {filePath}");
        }

        private static bool IsFileLocked(string filePath) {
            try {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
                    return false;
                }
            } catch (IOException) {
                return true;
            }
        }

        private static string GetMimeType(string filePath) {
            string extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            switch (extension) {
                case ".json":
                    return "application/json";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".pdf":
                    return "application/pdf";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream";
            }
        }

        private static void LogInfo(string message) {
            Debug.Log("[ServerCommunication] " + message);
        }

        private static void LogWarning(string message) {
            Debug.LogWarning("[ServerCommunication] " + message);
        }

        private static void LogError(string message) {
            Debug.LogError("[ServerCommunication] " + message);
        }

        private void LogRequestStart(HttpMethod method, string url, string body, RequestLogMode logMode) {
            if (!ShouldLogVerbose(logMode)) {
                return;
            }

            LogInfo($"[{method}] START {url}{(string.IsNullOrEmpty(body) ? string.Empty : $" | body={FormatBodyForLog(body)}")}");
        }

        private void LogRequestSuccess(HttpMethod method, string url, UnityWebRequest request, RequestLogMode logMode) {
            if (!ShouldLogVerbose(logMode)) {
                return;
            }

            LogInfo($"[{method}] OK {url} | status={(long)request.responseCode} | bytes={request.downloadedBytes}");
        }

        private void LogRequestFailure(HttpMethod method, string url, UnityWebRequest request, RequestLogMode logMode) {
            string responseText = request.downloadHandler?.text;
            LogError($"[{method}] FAIL {url} | status={(long)request.responseCode} | error={request.error} | body={FormatBodyForLog(responseText)}");
        }

        private static string FormatBodyForLog(string body) {
            if (string.IsNullOrEmpty(body)) {
                return "<empty>";
            }

            string trimmed = body.Trim();
            if (trimmed.Length <= MaxLoggedBodyLength) {
                return trimmed;
            }

            return trimmed.Substring(0, MaxLoggedBodyLength) + "...(truncated)";
        }

        private static bool ShouldLogVerbose(RequestLogMode logMode) {
            switch (logMode) {
                case RequestLogMode.Quiet:
                    return false;
                case RequestLogMode.Verbose:
                    return true;
                default:
                    return debug;
            }
        }

        #endregion

        #region Types

        /// <summary>
        /// Common HTTP verbs supported by the client.
        /// </summary>
        public enum HttpMethod {
            /// <summary>GET.</summary>
            Get,

            /// <summary>POST.</summary>
            Post,

            /// <summary>PUT.</summary>
            Put,

            /// <summary>DELETE.</summary>
            Delete
        }

        /// <summary>
        /// Controls how much logging a request should produce.
        /// </summary>
        public enum RequestLogMode {
            /// <summary>
            /// Uses the global <see cref="debug"/> flag.
            /// </summary>
            Default,

            /// <summary>
            /// Suppresses request lifecycle and body logs for this request.
            /// Errors are still reported.
            /// </summary>
            Quiet,

            /// <summary>
            /// Forces request lifecycle and body logs for this request.
            /// </summary>
            Verbose
        }

        #endregion
    }

    internal static class HttpMethodExtensions {
        public static string ToUnityVerb(this ServerCommunication.HttpMethod method) {
            switch (method) {
                case ServerCommunication.HttpMethod.Get:
                    return UnityWebRequest.kHttpVerbGET;
                case ServerCommunication.HttpMethod.Post:
                    return UnityWebRequest.kHttpVerbPOST;
                case ServerCommunication.HttpMethod.Put:
                    return UnityWebRequest.kHttpVerbPUT;
                case ServerCommunication.HttpMethod.Delete:
                    return UnityWebRequest.kHttpVerbDELETE;
                default:
                    return UnityWebRequest.kHttpVerbGET;
            }
        }
    }
}
