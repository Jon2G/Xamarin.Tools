using System;

namespace Kit
{
    public static class Elevator
    {
        public static object Elevate(this object Lower, Type elevatedType)
        {
            var LowerType = Lower.GetType();
            if (LowerType.IsSubclassOf(elevatedType))
            {
                throw new InvalidCastException($"{LowerType.Name} is not subclass of {elevatedType.Name}");
            }
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Lower);

            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, elevatedType);
        }

        public static T Elevate<T>(this object Lower)
            where T : new()
        {
            var elevatedType = typeof(T);
            return (T)Lower.Elevate(elevatedType);


        }
    }
}