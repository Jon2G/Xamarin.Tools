using System;
using Kit.Sql.Enums;

namespace Kit.Sql.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SyncMode : Attribute
    {
        public SyncDirection Direction { get; private set; }

        public SyncMode()
        {
            Direction = SyncDirection.NoSync;
        }

        public SyncMode(int Direction)
        {
            this.Direction = (SyncDirection) Direction;
        }
        public SyncMode(SyncDirection Direction)
        {
            this.Direction = Direction;
        }
    }
}