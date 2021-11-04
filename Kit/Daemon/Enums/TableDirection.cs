using System;

namespace Kit.Daemon.Enums
{
    [Flags]
    public enum TableDirection
    {
        DOWNLOAD=0,
        UPLOAD=1,
        TWO_WAY=2
    }
}
