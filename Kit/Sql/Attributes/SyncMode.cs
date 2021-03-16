using System;
using System.Collections.Generic;
using Kit.Sql.Enums;

namespace Kit.Sql.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SyncMode : Attribute
    {
        public SyncDirection Direction { get; private set; }
        public bool ReserveNewId { get; private set; }

        public SyncMode()
        {
            Direction = SyncDirection.NoSync;
        }


        public SyncMode(int Direction, bool ReserveNewId = true)
        : this((SyncDirection)Direction, ReserveNewId)
        {

        }
        public SyncMode(SyncDirection Direction, bool ReserveNewId = true)
        {
            this.Direction = Direction;
            this.ReserveNewId = ReserveNewId;
        }
    }
}