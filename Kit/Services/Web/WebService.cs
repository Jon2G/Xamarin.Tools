using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kit.Services.Web
{
    public sealed class WebService
    {
        private readonly string Url;
        public WebService(string Url)
        {
            this.Url = Url;
        }
       
        public async Task<Kit.Services.Web.ResponseResult> GET(string metodo, params string[] parameters)
        {
            Kit.Services.Web.ResponseResult result = new Kit.Services.Web.ResponseResult
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
            string GetUrl = string.Format("{0}/{1}{2}", this.Url, metodo, parametersText);
            string responseText = string.Empty;
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler()
                {
                    Proxy = null,
                    UseProxy = false
                })
                {
                    //if (Kit.Tools.Instance.RuntimePlatform != RuntimePlatform.WPF)
                    //{
                    //    try
                    //    {
                    //        handler.ServerCertificateCustomValidationCallback +=
                    //            (arg1, arg2, arg3, arg4) => { return true; };
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Log.Logger.Error(ex);
                    //    }
                    //}

                    using (HttpClient client = new HttpClient(handler))
                    {
                        result.Response = await client.GetStringAsync(GetUrl);
                        result.HttpStatusCode = HttpStatusCode.OK;
                    }
                }
            }
            catch (WebException ex)
            {
                Log.Logger.Error(ex, $"GET: {this.Url}");
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
                Log.Logger.Error(ex, $"GET: {this.Url}");
                result.Response = "ERROR";
            }
            return result;
        }
    }
}