using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

using static Modules.Utility.Utility;

using ViitorCloud.API.StandardTemplates;

namespace ViitorCloud.API {
    /// <summary>
    /// This class is responsible for handling REST API requests to remote server.
    /// To extend this class you just need to add new API methods.
    /// </summary>
    public class ServerCommunication : MonoBehaviour {
        #region [Server Communication]
        public static ServerCommunication Instance;

        private void Awake() {
            Singleton();
        }

        private void Singleton() {
            if (Instance == null) {
                Instance = this;

                if (server == Constants.API.Server.FromConfig) {
                    Constants.API.LoadFromConfig();
                }
                
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }
        
        public API.Constants.API.Server server = API.Constants.API.Server.Development;
        public static bool debug = false;
        public static string ViitorCloudToken = "";

        /// <summary>
        /// This method request post method .
        /// </summary>
        /// <param name="form">Data send from local in JSON format.</param>
        /// <param name="url">URL for post method</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        public void SendRequestPost<T>(string form, string url, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            StartCoroutine(RequestCoroutinePost(form, url, callbackOnSuccess, callbackOnFail));
        }
        
        
        /// <summary>
        /// This method request post method .
        /// </summary>
        /// <param name="form">Data send from local in JSON format.</param>
        /// <param name="url">URL for post method</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        public void SendRequestPostWithFile<T>(string fieldName,string filePath, string url, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            StartCoroutine(RequestCoroutinePostMultipart(fieldName,filePath, url, callbackOnSuccess, callbackOnFail));
        }


        /// <summary>
        /// This method request post method .
        /// </summary>
        /// <param name="form">Data send from local in JSON format.</param>
        /// <param name="url">URL for post method</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        public void SendRequestPostWithMultiFile<T>(string url,List<FileUpload> files, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            StartCoroutine(RequestCoroutinePostMultiFilesMultipart(url,files, callbackOnSuccess, callbackOnFail));
        }


        /// <summary>
        /// This method request delete method .
        /// </summary>
        /// <param name="url">URL for delete method</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        public void SendRequestDelete(string url, UnityAction callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            StartCoroutine(RequestCoroutineDelete(url, callbackOnSuccess, callbackOnFail));
        }

        /// <summary>
        /// This method request get method .
        /// </summary>
        /// <param name="url">URL for get method</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        public void SendRequestGet<T>(string url, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            StartCoroutine(RequestCoroutineGet(url, callbackOnSuccess, callbackOnFail));
        }

        /// <summary>
        /// This method request Put method .
        /// </summary>
        /// <param name="form">Data send from local in JSON format.</param>
        /// <param name="url">URL for put method</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        public void SendRequestPut<T>(string form, string url, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            StartCoroutine(RequestCoroutinePut(form, url, callbackOnSuccess, callbackOnFail));
        }

        private IEnumerator RequestCoroutinePost<T>(string jsonData, string url, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            if (debug) {
                Debug.Log("url " + url + " jsonData " + jsonData);
            }
            using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData)) {
                request.method = UnityWebRequest.kHttpVerbPOST;
                SetHeader(request);
                yield return request.SendWebRequest();
                SendResponseToAPIMethod(request, url, callbackOnSuccess, callbackOnFail);
            }
        }

        private IEnumerator RequestCoroutinePut<T>(string jsonData, string url, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            if (debug) {
                Debug.Log("url " + url + " jsonData " + jsonData);
            }
            using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData)) {
                request.method = UnityWebRequest.kHttpVerbPUT;
                SetHeader(request);
                yield return request.SendWebRequest();
                SendResponseToAPIMethod(request, url, callbackOnSuccess, callbackOnFail);
            }
        }

        private IEnumerator RequestCoroutineGet<T>(string url, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            if (debug) {
                Debug.Log("url " + url);
            }
            using (UnityWebRequest request = UnityWebRequest.Get(url)) {
                request.method = UnityWebRequest.kHttpVerbGET;
                SetHeader(request);
                yield return request.SendWebRequest();
                SendResponseToAPIMethod(request, url, callbackOnSuccess, callbackOnFail);
            }
        }

        private IEnumerator RequestCoroutinePostMultipart<T>(string fieldName, string filePath, string url, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            if (!File.Exists(filePath)) {
                Debug.LogError("File not exist: " + filePath);
                callbackOnFail.Invoke("File not exist: " + filePath);
                yield break;
            }


            if (debug) {
                Debug.Log("url: " + url + " filePath: " + filePath);
            }

            // Wait until the file is unlocked
            while (IsFileLocked(filePath)) {
                if (debug) {
                    Debug.Log($"File {filePath} is locked, waiting...");
                }
                yield return new WaitForSeconds(0.5f); // Wait before retrying
            }

            // Read file data after ensuring it's not locked
            byte[] fileData = File.ReadAllBytes(filePath);

            WWWForm form = new WWWForm();
            form.AddBinaryData(fieldName, fileData, Path.GetFileName(filePath), "application/json");

            using (UnityWebRequest request = UnityWebRequest.Post(url, form)) {
                request.SetRequestHeader("accept", "application/json");
                if (!string.IsNullOrEmpty(ViitorCloudToken)) {
                    request.SetRequestHeader("Authorization", "Bearer " + ViitorCloudToken);
                }

                yield return request.SendWebRequest();

                SendResponseToAPIMethod(request, url, callbackOnSuccess, callbackOnFail);
            }
        }
        
        
        private IEnumerator RequestCoroutinePostMultiFilesMultipart<T>(string url,List<FileUpload> files, UnityAction<T> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            WWWForm form = new WWWForm();
            for (int i = 0; i < files.Count; i++) {
                
                if (!File.Exists(files[i].filePath)) {
                    Debug.LogError("File not exist: " + files[i].filePath);
                    callbackOnFail.Invoke("File not exist: " + files[i].filePath);
                    yield break;
                }
                
                while (IsFileLocked(files[i].filePath)) {
                    if (debug) {
                        Debug.Log($"File {files[i].filePath} is locked, waiting...");
                    }
                    yield return new WaitForSeconds(0.5f); // Wait before retrying
                }
                if (debug) {
                    Debug.Log("url: " + url + " filePath: " + files[i].filePath);
                }
            // Read file data after ensuring it's not locked
                byte[] fileData = File.ReadAllBytes(files[i].filePath);
                form.AddBinaryData(files[i].fieldName, fileData, Path.GetFileName(files[i].filePath), "application/json");
            }

            using (UnityWebRequest request = UnityWebRequest.Post(url, form)) {
                request.SetRequestHeader("accept", "application/json");
                if (!string.IsNullOrEmpty(ViitorCloudToken)) {
                    request.SetRequestHeader("Authorization", "Bearer " + ViitorCloudToken);
                }

                yield return request.SendWebRequest();

                SendResponseToAPIMethod(request, url, callbackOnSuccess, callbackOnFail);
            }
        }

// Function to check if a file is locked
       


        private IEnumerator RequestCoroutineDelete(string url, UnityAction callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            if (debug) {
                Debug.Log("url Delete " + url);
            }
            using (UnityWebRequest request = UnityWebRequest.Delete(url)) {
                request.method = UnityWebRequest.kHttpVerbDELETE;
                SetHeader(request);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.DataProcessingError ||
                    request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError) {

                    Debug.LogError("Delete url " + url + " " + request.error);
                    var apiResponse = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
                    callbackOnFail?.Invoke(apiResponse.message);

                    if (debug) {
                        Debug.Log("Delete url " + url + " Data " + request.downloadHandler.text);
                    }
                } else {
                    callbackOnSuccess.Invoke();
                }
            }
        }

        private void SetHeader(UnityWebRequest request) {
            request.timeout = 20;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            if (!string.IsNullOrEmpty(ViitorCloudToken)) {
                request.SetRequestHeader("Authorization", "Bearer " + ViitorCloudToken);
            }
        }

        public void SendResponseToAPIMethod<T>(UnityWebRequest request, string url, UnityAction<T> callbackOnSuccess,
            //For responseCode
            //UnityAction<UnityWebRequest> callbackOnFail) {
            UnityAction<string> callbackOnFail) {
            if (request.result == UnityWebRequest.Result.DataProcessingError ||
                request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError) {

                Debug.LogError("url " + url + " error " + request.error + " error code " + request.responseCode + " Data " + request.downloadHandler.text);

                APIResponse apiResponse = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
                if (apiResponse != null) {
                    callbackOnFail?.Invoke(apiResponse.message);
                } else {
                    callbackOnFail?.Invoke(request.error);
                }
                //For responseCode
                //callbackOnFail?.Invoke(request);
            } else {
                if (string.IsNullOrEmpty(request.downloadHandler.text)) {
                    Debug.LogError("DownloadHandler text is null");
                } else {
                    if (debug) {
                        Debug.Log("url " + url + " Data " + request.downloadHandler.text);
                    }
                    ParseResponse(request.downloadHandler.text, callbackOnSuccess);
                }
            }
        }

        /// <summary>
        /// This method finishes request process and remove $ sign.
        /// </summary>
        /// <param name="data">Data received from server in JSON format.</param>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <typeparam name="T">Data Model Type.</typeparam>
        private void ParseResponse<T>(string data, UnityAction<T> callbackOnSuccess) {
            data = data.Replace("$oid", "oid");
            data = data.Replace("$date", "date");
            var parsedData = JsonUtility.FromJson<T>(data);
            callbackOnSuccess?.Invoke(parsedData);
        }

        public class FileUpload{
            public string fieldName;
            public string filePath;
        }
        
        #endregion [Server Communication]
    }
}
