using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Xamarin.Tools.Shared.Blumitech
{
    internal sealed class WebService
    {
        private readonly string DeviceId;
        private readonly string Url;
        public WebService(string DeviceId)
        {
            this.DeviceId = DeviceId;
            this.Url = "http://192.168.0.32:51425/Activacion.asmx";
        }
        public async Task<ProjectActivationState> RequestProjectAccess(string ProjectKey)
        {
            ProjectActivationState ProjectStatus = await IsDeviceAutenticated();
            if (ProjectStatus == ProjectActivationState.Registered)
            {
                ProjectStatus = await GetProjectStatus(ProjectKey);
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
            await Task.Yield();
            GET(this.Url, "IsDeviceAutenticated", "http://tempuri.org/"
                , new Dictionary<string, string>()
                {{"DeviceId",this.DeviceId} }, out string Respuesta);
            if (Respuesta == "ERROR")
            {
                return ProjectActivationState.ConnectionFailed;
            }
            if (Respuesta == "REGISTERED")
            {
                return ProjectActivationState.Registered;
            }
            return ProjectActivationState.LoginRequired;
        }
        private async Task<ProjectActivationState> GetProjectStatus(string ProjectKey)
        {
            return ProjectActivationState.Active;
        }
        private static HttpStatusCode GET(string url, string metodo, string EspacioNombres, Dictionary<string, string> parameters, out string Respuesta)
        {
            HttpStatusCode status = HttpStatusCode.Unused;
            string parametersText;
            if (parameters != null && parameters.Count > 0)
            {
                StringBuilder sb = new StringBuilder("?");
                foreach (KeyValuePair<string, string> oneParameter in parameters)
                {
                    sb.AppendFormat("{0}={1}&", oneParameter.Key, oneParameter.Value);
                }
                parametersText = sb.ToString();
                if (parametersText.Last() == '&')
                {
                    parametersText = parametersText.Substring(0, parametersText.Length - 1);
                }
            }
            else
            {
                parametersText = "";
            }
            string GetUrl = string.Format("{0}/{1}{2}", url, metodo, parametersText);
            string responseText = String.Empty;
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("Accept-Language", " en-US");
                    client.Headers.Add("Accept", " text/html, application/xhtml+xml, */*");
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)");
                    responseText = client.DownloadString(GetUrl);
                    status = HttpStatusCode.OK;
                }
            }
            catch (WebException ex)
            {
                status = (HttpStatusCode)ex.Status;
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    using (StreamReader reader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                Respuesta = "ERROR";
                return status;
            }
            Respuesta = null;
            if (!string.IsNullOrEmpty(responseText))
            {
                string responseElement = @$"<string xmlns=""{EspacioNombres}"">";
                int pos1 = responseText.IndexOf(responseElement);
                if (pos1 >= 0)
                {
                    pos1 += responseElement.Length;
                    int pos2 = responseText.IndexOf("</", pos1);

                    if (pos2 > pos1)
                    {
                        responseText = responseText.Substring(pos1, pos2 - pos1);
                    }
                }
                else
                {
                    responseText = ""; // No result
                }
                if (!string.IsNullOrEmpty(responseText) && responseText != "ERROR")
                {
                    Respuesta = responseText;
                }
            }
            return status;
        }
        private HttpStatusCode SOAP(string webWebServiceUrl,
                                string webServiceNamespace,
                                string methodName,
                                Dictionary<string, string> parameters,
                                out byte[] Respuesta)
        {
            string responseText = null;
            const string soapTemplate =
        @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
   xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
   xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"">
  <soap:Body>
    <{0} xmlns=""{1}"">
      {2}    </{0}>
  </soap:Body>
</soap:Envelope>";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(webWebServiceUrl);

            req.ContentType = "application/soap+xml;";
            req.Method = "POST";

            string parametersText;

            if (parameters != null && parameters.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, string> oneParameter in parameters)
                {
                    sb.AppendFormat("  <{0}>{1}</{0}>\r\n", oneParameter.Key, oneParameter.Value);
                }

                parametersText = sb.ToString();
            }
            else
            {
                parametersText = "";
            }

            string soapText = string.Format(soapTemplate, methodName, webServiceNamespace, parametersText);
            HttpStatusCode responseHttpStatusCode = HttpStatusCode.Unused;
            try
            {
                using (Stream stm = req.GetRequestStream())
                {
                    using (StreamWriter stmw = new StreamWriter(stm))
                    {
                        stmw.Write(soapText);
                    }
                }
                responseHttpStatusCode = HttpStatusCode.Unused;
                responseText = null;
                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    responseHttpStatusCode = response.StatusCode;

                    if (responseHttpStatusCode == HttpStatusCode.OK)
                    {
                        int contentLength = (int)response.ContentLength;

                        if (contentLength > 0)
                        {
                            int readBytes = 0;
                            int bytesToRead = contentLength;
                            byte[] resultBytes = new byte[contentLength];

                            using (Stream responseStream = response.GetResponseStream())
                            {
                                while (bytesToRead > 0)
                                {
                                    // Read may return anything from 0 to 10. 
                                    int actualBytesRead = responseStream.Read(resultBytes, readBytes, bytesToRead);

                                    // The end of the file is reached. 
                                    if (actualBytesRead == 0)
                                    {
                                        break;
                                    }

                                    readBytes += actualBytesRead;
                                    bytesToRead -= actualBytesRead;
                                }
                                responseText = Encoding.ASCII.GetString(resultBytes);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Respuesta = null;
            }

            Respuesta = null;
            if (!string.IsNullOrEmpty(responseText))
            {
                string responseElement = methodName + "Result>";
                int pos1 = responseText.IndexOf(responseElement);

                if (pos1 >= 0)
                {
                    pos1 += responseElement.Length;
                    int pos2 = responseText.IndexOf("</", pos1);

                    if (pos2 > pos1)
                    {
                        responseText = responseText.Substring(pos1, pos2 - pos1);
                    }
                }
                else
                {
                    responseText = ""; // No result
                }
                if (!string.IsNullOrEmpty(responseText))
                {
                    Respuesta = Convert.FromBase64String(responseText);
                }
            }

            return responseHttpStatusCode;
        }
    }
}
