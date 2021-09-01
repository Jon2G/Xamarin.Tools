using System.Net;

namespace Kit.Services.Web
{
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
}
