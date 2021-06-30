using System;
using System.Collections.Generic;
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

        public Task<Kit.Services.Web.ResponseResult> GET(string metodo, params string[] parameters) => GET(metodo, null, parameters);

        public async Task<Kit.Services.Web.ResponseResult> GET(string metodo,Dictionary<string,string> query, params string[] parameters)
        {
            Kit.Services.Web.ResponseResult result = new Kit.Services.Web.ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };

            StringBuilder sb = new StringBuilder("/");
            if (parameters != null && parameters.Length > 0)
            {
                foreach (string oneParameter in parameters.Where(x=>x is not null))
                {
                    sb.AppendFormat("{0}/", Uri.EscapeDataString(oneParameter));
                }
                if (sb.Last() == '/')
                {
                    sb=sb.Remove(sb.Length - 1, 1);
                }
            }

            if (query != null && query.Any())
            {
                foreach (KeyValuePair<string, string> oneParameter in query)
                {
                    sb.AppendFormat("?{0}={1}", oneParameter.Key, Uri.EscapeDataString(oneParameter.Value));
                }
            }

            string GetUrl = string.Format("{0}/{1}{2}", this.Url, metodo, sb.ToString());
            string responseText = string.Empty;
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler()
                {
                    Proxy = null,
                    UseProxy = false,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                })
                { 
                    using (HttpClient client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
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
                Log.Logger.Error(ex, $"GET: {GetUrl}");
                result.Response = "ERROR";
            }
            return result;
        }
    }
}