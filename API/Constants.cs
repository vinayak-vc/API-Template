
namespace ViitorCloud.API.Constants {
    /// <summary>
    /// API response.
    /// </summary>
    public class APIResponse {
        public const string StatusSuccess = "200";
    }

    public class API {
        
        public static AppConfig appConfig;

        public static string APIDevelopmentBaseURL = "https://stg-demourl.com/";
        public static string APIProductionBaseURL = "https://prod-demourl.com/";

        public static string Login = APIBaseURL + "users/login";
        public static string Register = APIBaseURL + "users/register";

        public static string APIBaseURL {
            get {
                switch (ServerCommunication.Instance.server) {
                    case Server.Live:
                        return APIProductionBaseURL;
                    case Server.Development:
                        return APIDevelopmentBaseURL;
                    case Server.FromConfig:
                        return appConfig.APIBaseURL;
                }

                return APIProductionBaseURL;
            }
        }
        
        public class AppConfig {
            public bool offline;
            public string APIBaseURL;
        }
        
        public static void LoadFromConfig() {
            appConfig = null;
            string _configPath = UnityEngine.Application.dataPath + "/config.json";

            try {
                if (System.IO.File.Exists(_configPath)) {
                    appConfig = UnityEngine.JsonUtility.FromJson<AppConfig>(System.IO.File.ReadAllText(_configPath));
                } else {
                    GenerateJSON();
                }
            } catch (System.Exception e) {
                appConfig = new AppConfig();
                appConfig.APIBaseURL = APIProductionBaseURL;
                appConfig.offline = false;
                UnityEngine.Debug.LogError(e.Message);
            }
        }
        private static void GenerateJSON() {
            string _configPath = UnityEngine.Application.dataPath + "/config.json";
            AppConfig appConfig = new AppConfig();
            appConfig.APIBaseURL = APIProductionBaseURL;
            appConfig.offline = false;
           System.IO. File.WriteAllText(_configPath, UnityEngine.JsonUtility.ToJson(appConfig, true));
            appConfig = null;
        }

        public enum Server {
            Live,
            Development,
            FromConfig
        }
    }
}