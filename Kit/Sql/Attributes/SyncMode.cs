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
        public string AffectsMethodName { get; private set; }
        public string CustomUploadAction { get; private set; }

        public SyncMode()
        {
            Direction = SyncDirection.NoSync;
        }


        public SyncMode(int Direction, bool ReserveNewId = true, string AffectsMethodName = null, string CustomUploadAction = null)
        : this((SyncDirection)Direction, ReserveNewId, AffectsMethodName, CustomUploadAction)
        {

        }
        public SyncMode(SyncDirection Direction, bool ReserveNewId = true, string AffectsMethodName = null, string CustomUploadAction = null)
        {
            this.Direction = Direction;
            this.ReserveNewId = ReserveNewId;
            this.AffectsMethodName = AffectsMethodName;
            this.CustomUploadAction = CustomUploadAction;
        }
    }
}