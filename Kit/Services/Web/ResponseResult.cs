using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

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
