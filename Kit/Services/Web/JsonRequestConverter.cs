using System;
using Kit.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using BaseClass = Kit.Services.Web.Request;
namespace Kit.Services.Web
{
    internal class JsonRequestConverter : CustomCreationConverter<BaseClass>
    {
        private RequestType _currentObjectType;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jobj = JObject.ReadFrom(reader);
            if (jobj is null || !jobj.HasValues)
            {
                return null;
            }
            _currentObjectType = jobj[nameof(BaseClass.RequestType)].ToObject<RequestType>();
            return base.ReadJson(jobj.CreateReader(), objectType, existingValue, serializer);
        }

        public override BaseClass Create(Type objectType)
        {
            switch (_currentObjectType)
            {
                case RequestType.POST:
                    return new PostRequest();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
