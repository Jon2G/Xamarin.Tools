using System;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Kit
{
    public static class Elevator
    {
        public static T Elevate<T>(this object Lower)
            where T : new()
        {
            var ElevatedType = typeof(T);
            var LowerType = Lower.GetType();
            if (LowerType.IsSubclassOf(ElevatedType))
            {
                throw new InvalidCastException($"{LowerType.Name} is not subclass of {ElevatedType.Name}");
            }
            string json = JsonSerializer.Serialize(Lower);

            return JsonSerializer.Deserialize<T>(json);
        }
    }
}