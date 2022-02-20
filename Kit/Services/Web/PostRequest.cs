using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Kit.Enums;
using Kit.Sql.Attributes;
using Kit.Sql.Enums;
using Newtonsoft.Json;

namespace Kit.Services.Web
{
    [Table("PostRequest"), Serializable, Preserve(AllMembers = true), SyncMode(SyncDirection.Upload)]
    public class PostRequest : Request
    {
        public override RequestType RequestType => RequestType.POST;
        // ReSharper disable once MemberCanBePrivate.Global
        public byte[] Body { get; set; }
        /// <summary>
        /// For JSON and SQLITE
        /// </summary>
        public PostRequest() : base()
        {

        }
        public PostRequest(string url, byte[] body) : base(url)
        {
            Body = body;
        }
        public override async Task<ResponseResult> Execute()
        {
            await Task.Yield();
            ByteArrayContent body = new ByteArrayContent(Body);
            body.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            HttpResponseMessage message = await WebService.HttpClient.PostAsync(Url, body);
            this.Result.HttpStatusCode = message.StatusCode;
            this.Result.Response = await message.Content.ReadAsStringAsync();
            return this.Result;
        }
    }
}
