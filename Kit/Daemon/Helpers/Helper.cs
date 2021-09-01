using Kit.Daemon.Enums;

namespace Kit.Daemon.Helpers
{
    public static class Helper
    {
        public static SyncTarget InvertDirection(this SyncTarget Direccion)
        {
            switch (Direccion)
            {
                case SyncTarget.Remote:
                    return SyncTarget.Local;

                case SyncTarget.Local:
                    return SyncTarget.Remote;
            }
            return SyncTarget.NOT_SET;
        }
    }
}