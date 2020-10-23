using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Enums
{
    public enum ProjectActivationState
    {
        LoginRequired, Expired, Denied, Active, ConnectionFailed,
        Registered, Unknown
    }
}
