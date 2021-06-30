using BlumAPI.Enums;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kit;
using Kit.Services;
using Kit.Services.Web;
using static Kit.Services.Web.WebService;

namespace BlumAPI
{
    public sealed class APICaller
    {
        private readonly string DeviceId;
        private readonly WebService WebService;

        public APICaller(string DeviceId)
        {
            this.DeviceId = DeviceId;
            this.WebService=new WebService("http://www.spika.mx:88/AppAuthentication");
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
            ResponseResult result = await WebService.GET( "IsDeviceAutenticated", DeviceId);
            result.Response=result.Response.Replace("\"", string.Empty);
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
            ResponseResult result = await WebService.GET( "IsDeviceEnrolled", DeviceId, AppKey);
            result.Response= result.Response.Replace("\"", string.Empty);
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


        internal async Task<string> EnrollDevice(string DeviceBrand, string Platform, string Name, string Model, string AppKey, string UserName, string Password)
        {
            ResponseResult response = await WebService.GET("EnrollDevice", this.DeviceId, DeviceBrand, Platform, Name, Model, AppKey, UserName, Password);
            return response.Response.Replace("\"",string.Empty);
        }

        internal async Task<string> DevicesLeft(string appKey, string userName)
        {
            ResponseResult response = await WebService.GET("DevicesLeft", appKey, userName);
            return response.Response;
        }

        internal async Task<string> LogIn(string userName, string password)
        {
            try
            {
                ResponseResult response = await WebService.GET("LogIn", userName, password);
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
