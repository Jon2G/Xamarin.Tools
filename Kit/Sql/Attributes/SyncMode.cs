using Kit.Daemon.Enums;
using Kit.Sql.Enums;
using System;

namespace Kit.Sql.Attributes
{
    [Preserve(AllMembers = true), AttributeUsage(AttributeTargets.Class)]
    public class SyncMode : Attribute
    {
        public SyncDirection Direction { get; private set; }
        public SyncTrigger Trigger { get; private set; }
        public bool ReserveNewId { get; private set; }
        public int Order { get; private set; }

        public SyncMode()
        {
            Direction = SyncDirection.NoSync;
            Trigger = SyncTrigger.None;
            Order = 0;
        }

        public SyncMode(int Direction, int Trigger, int Order = 0, bool ReserveNewId = true)
        : this((SyncDirection)Direction, (SyncTrigger)Trigger, Order, ReserveNewId)
        {
        }

        public SyncMode(SyncDirection Direction, SyncTrigger Trigger, int Order = 0, bool ReserveNewId = true)
        {
            this.Direction = Direction;
            this.Trigger = Trigger;
            this.Order = Order;
            this.ReserveNewId = ReserveNewId;
        }
    }
}