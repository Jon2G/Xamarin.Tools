using Kit.Daemon.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Daemon.Helpers
{
    public static class Helper
    {
        public static SyncDirecction InvertDirection(this SyncDirecction Direccion)
        {
            switch (Direccion)
            {
                case SyncDirecction.Remote:
                    return SyncDirecction.Local;
                case SyncDirecction.Local:
                    return SyncDirecction.Remote;
            }
            return SyncDirecction.INVALID;
        }
    }
}
