using System;
using System.Collections.Generic;

namespace ViitorCloud.API.StandardTemplates {
    [Serializable]
    public class APIResponse {
        //public string code;
        public string data;
        public string message;
        public string token;
        public string statusCode;
        public string error;
    }

    [Serializable]
    public class APIResponse<T> where T : class {
        //public string code;
        public T data;
        public string message;
        public string token;
        public string statusCode;
        public string error;
    }

    [Serializable]
    public class APIEventListResponse<T> where T : class {
        //public string code;
        public List<T> data;
        public string message;
        public string token;
        public string statusCode;
        public string error;
    }

    [Serializable]
    public class Login {
        public string emailId;
        public string password;
    }

    [Serializable]
    public class LoginResponse {
        public User user;
        [Serializable]
        public class User {
            public string firstName;
            public string lastName;
            public string emailId;
            public string image;
        }
    }

    [Serializable]// Mostly to remove
    public class Register {
        public string firstName;
        public string lastName;
        public string emailId;
        public string password;
    }
}
