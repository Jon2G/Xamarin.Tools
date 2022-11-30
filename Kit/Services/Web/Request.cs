using Kit.Daemon.Sync;
using Kit.Enums;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Interfaces;
using Kit.Sql.SqlServer;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using TableMapping = Kit.Sql.Base.TableMapping;

namespace Kit.Services.Web
{
    [JsonConverter(typeof(JsonRequestConverter)), Serializable, Preserve(AllMembers = true), SyncMode(SyncDirection.Upload, SyncTrigger.None)]
    public abstract class Request : ISync, IGuid
    {
        public abstract RequestType RequestType { get; }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Url { get; set; }

        protected ResponseResult Result;
        /// <summary>
        /// For JSON and SQLITE
        /// </summary>
        public Request()
        {

        }
        public Request(string url)
        {
            Url = url;
            Result = new Kit.Services.Web.ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };
        }
        public abstract Task<ResponseResult> Execute();
        public override bool CustomUpload(SqlBase con, SqlBase targetcon, TableMapping map)
        {
            if (targetcon is not SQLServerConnection)
            {
                return true;
            }
            return Execute().GetAwaiter().GetResult().ToResponse().ResponseResult == APIResponseResult.OK;
        }
    }
}
