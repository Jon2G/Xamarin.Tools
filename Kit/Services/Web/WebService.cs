using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
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

        private string BuildUrl(string metodo, Dictionary<string, string> query, params string[] parameters)
        {
            StringBuilder sb_parameters = new StringBuilder("/");
            if (parameters != null && parameters.Length > 0)
            {
                foreach (string oneParameter in parameters.Where(x => x is not null))
                {
                    sb_parameters.AppendFormat("{0}/", Uri.EscapeDataString(oneParameter));
                }
                if (sb_parameters.Last() == '/')
                {
                    sb_parameters = sb_parameters.Remove(sb_parameters.Length - 1, 1);
                }
            }
            if (query != null && query.Any())
            {
                foreach (KeyValuePair<string, string> oneParameter in query)
                {
                    sb_parameters.AppendFormat("?{0}={1}", oneParameter.Key, Uri.EscapeDataString(oneParameter.Value));
                }
            }
            StringBuilder sb = new StringBuilder(this.Url).Append('/').Append(metodo).Append(sb_parameters);
            return sb.ToString();
        }
        public Task<Kit.Services.Web.ResponseResult> GET(string metodo, params string[] parameters) => GET(metodo, null, parameters);

        public async Task<Kit.Services.Web.ResponseResult> GET(string metodo, Dictionary<string, string> query, params string[] parameters)
        {
            Kit.Services.Web.ResponseResult result = new Kit.Services.Web.ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };
            string GetUrl = BuildUrl(metodo, query, parameters);
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
        public Task<Kit.Services.Web.ResponseResult> PostAsBody(byte[] byteArray, string metodo, params string[] parameters) => PostAsBody(byteArray, metodo, null, parameters);
        public async Task<Kit.Services.Web.ResponseResult> PostAsBody(byte[] byteArray, string method, Dictionary<string, string> query, params string[] parameters)
        {
            Kit.Services.Web.ResponseResult result = new Kit.Services.Web.ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };
            string geturl = String.Empty;
            try
            {
                geturl = BuildUrl(method, query, parameters);
                using (HttpClientHandler handler = new HttpClientHandler()
                {
                    Proxy = null,
                    UseProxy = false,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                })
                {
                    using (var client = new HttpClient(handler))
                    {
                        var body = new ByteArrayContent(byteArray);
                        body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                        HttpResponseMessage message = await client.PostAsync(geturl, body);
                        result.HttpStatusCode = message.StatusCode;
                        result.Response = await message.Content.ReadAsStringAsync();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"GET: {geturl}");
                result.Response = "ERROR";
                return result;
            }
        }
    }
}