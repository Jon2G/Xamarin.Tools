using System;
using Kit.Sql.Enums;

namespace Kit.Sql.SqlServer
{
    public class NotifyTableChangedEventArgs : EventArgs
    {
        public Base.TableMapping Table { get; private set; }
        public NotifyTableChangedAction Action { get; private set; }

        public NotifyTableChangedEventArgs(Base.TableMapping table, NotifyTableChangedAction action)
        {
            Table = table;
            Action = action;
        }
    }
}