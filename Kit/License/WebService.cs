using Kit.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Kit.Enums;

namespace Kit.License
{
    internal sealed class WebService
    {
        private readonly string DeviceId;
        private readonly string Url;
        public WebService(string DeviceId)
        {
            this.DeviceId = DeviceId;
            Url = "http://www.spika.mx:88/AppAuthentication";
        }
        public async Task<ProjectActivationState> RequestProjectAccess(string ProjectKey)
        {
            ProjectActivationState ProjectStatus = await IsDeviceAutenticated();
            if (ProjectStatus == ProjectActivationState.Registered)
            {
                ProjectStatus = await IsDeviceEnrolled(ProjectKey);
                if (ProjectStatus == ProjectActivationState.Active)
                {
                    return ProjectActivationState.Active;
                }
                else
                {
                    return ProjectStatus;
                }
            }
            else
            {
                return ProjectStatus;
            }
        }
        private async Task<ProjectActivationState> IsDeviceAutenticated()
        {
            ResponseResult result = await GET(Url, "IsDeviceAutenticated", DeviceId);
            if (result.Response == "ERROR")
            {
                return ProjectActivationState.ConnectionFailed;
            }
            if (result.Response == "REGISTERED")
            {
                return ProjectActivationState.Registered;
            }
            return ProjectActivationState.LoginRequired;
        }
        private async Task<ProjectActivationState> IsDeviceEnrolled(string AppKey)
        {
            ResponseResult result = await GET(Url, "IsDeviceEnrolled", DeviceId, AppKey);
            switch (result.Response)
            {
                case "ERROR":
                    return ProjectActivationState.ConnectionFailed;
                case "PROJECT_NOT_ENROLLED":
                    return ProjectActivationState.LoginRequired;
                case "INVALID_REQUEST":
                    return ProjectActivationState.Denied;
                case "ACTIVE":
                    return ProjectActivationState.Active;
            }
            return ProjectActivationState.Unknown;
        }

        public struct ResponseResult
        {
            public HttpStatusCode HttpStatusCode;
            public string Response;
            public ResponseResult(HttpStatusCode httpStatusCode, string response)
            {
                HttpStatusCode = httpStatusCode;
                Response = response;
            }
        }
        private static async Task<ResponseResult> GET(string url, string metodo, params string[] parameters)
        {
            ResponseResult result = new ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };
            string parametersText;
            if (parameters != null && parameters.Length > 0)
            {
                StringBuilder sb = new StringBuilder("/");
                foreach (string oneParameter in parameters)
                {
                    sb.AppendFormat("{0}/", Uri.EscapeDataString(oneParameter));
                }
                parametersText = sb.ToString();
                if (parametersText.Last() == '/')
                {
                    parametersText = parametersText.Substring(0, parametersText.Length - 1);
                }
            }
            else
            {
                parametersText = "";
            }
            string GetUrl = string.Format("{0}/{1}{2}", url, metodo, parametersText);
            string responseText = string.Empty;
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler()
                {
                    Proxy = null,
                    UseProxy = false
                })
                {
                    handler.ServerCertificateCustomValidationCallback +=
                        (arg1, arg2, arg3, arg4) => { return true; };

                    using (HttpClient client = new HttpClient(handler))
                    {
                        result.Response = await client.GetStringAsync(GetUrl);
                        result.HttpStatusCode = HttpStatusCode.OK;
                    }
                }
            }
            catch (WebException ex)
            {
                Log.Logger.Error(ex, $"GET: {url}");
                result.HttpStatusCode = (HttpStatusCode)ex.Status;
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    using (StreamReader reader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                result.Response = "ERROR";
                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"GET: {url}");
                result.Response = "ERROR";
            }
            return result;
        }

        internal async Task<string> EnrollDevice(string DeviceBrand, string Platform, string Name, string Model, string AppKey, string UserName, string Password)
        {
            ResponseResult response = await GET(Url, "EnrollDevice", this.DeviceId, DeviceBrand, Platform, Name, Model, AppKey, UserName, Password);
            return response.Response;
        }

        internal async Task<string> DevicesLeft(string appKey, string userName)
        {
            ResponseResult response = await GET(Url, "DevicesLeft", appKey, userName);
            return response.Response;
        }

        internal async Task<string> LogIn(string userName, string password)
        {
            try
            {
                ResponseResult response = await GET(Url, "LogIn", userName, password);
                return response.Response;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"LogIn userName: {userName}");
                return "ERROR";
            }
        }

    }
}
