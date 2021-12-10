using System;
using System.Linq;
using System.Reflection;
using Kit.Sql.Enums;

namespace Kit.Sql.Attributes
{
    [Preserve(AllMembers = true), AttributeUsage(AttributeTargets.Class)]
    public class SyncMode : Attribute
    {
        public SyncDirection Direction { get; private set; }
        public bool ReserveNewId { get; private set; }
        public int Order { get; private set; }

        public SyncMode()
        {
            Direction = SyncDirection.NoSync;
            Order = 0;
        }

        public SyncMode(int Direction, int Order = 0, bool ReserveNewId = true)
        : this((SyncDirection)Direction, Order, ReserveNewId)
        {
        }

        public SyncMode(SyncDirection Direction, int Order = 0, bool ReserveNewId = true)
        {
            this.Direction = Direction;
            this.Order = Order;
            this.ReserveNewId = ReserveNewId;
        }
        public static SyncDirection GetSyncDirection(Type type)
        {
            return GetSyncMode(type).Direction;
        }
        public static SyncMode GetSyncMode(Type type)
        {
            var typeInfo = type.GetTypeInfo();

#if ENABLE_IL2CPP
			var directionAttribute = typeInfo.GetCustomAttribute<SyncMode> ();
#else
            var directionAttribute =
                typeInfo.CustomAttributes
                    .Where(x => x.AttributeType == typeof(SyncMode))
                    .Select(x => x.InflateAttribute<SyncMode>())
                    .FirstOrDefault();
#endif
            return directionAttribute;
        }
    }
}