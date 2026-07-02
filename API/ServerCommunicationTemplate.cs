using UnityEngine;
using UnityEngine.Events;

using ViitorCloud.API.StandardTemplates;

namespace ViitorCloud.API {
    public class ServerCommunicationTemplate : MonoBehaviour {
        #region [API]
        /// <summary>
        /// This method call server API to get Login.
        /// </summary>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        public void RequestLogin(string form, UnityAction<APIResponse<LoginResponse>> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            ServerCommunication.Instance.SendRequestPost(form, Constants.API.Login,
                callbackOnSuccess, callbackOnFail);
        }

        /// <summary>
        /// This method call server API to get Login.
        /// </summary>
        /// <param name="callbackOnSuccess">Callback on success.</param>
        /// <param name="callbackOnFail">Callback on fail.</param>
        public void RequestRegister(string form, UnityAction<APIResponse<LoginResponse>> callbackOnSuccess,
            UnityAction<string> callbackOnFail) {
            ServerCommunication.Instance.SendRequestPost(form, Constants.API.Register,
                callbackOnSuccess, callbackOnFail);
        }
     
        #endregion [API]          
    }
}
