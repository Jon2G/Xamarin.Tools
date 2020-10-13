using System;
using System.Collections.Generic;
using System.Text;

namespace SQLHelper.Linker
{
    public sealed class Preserve : System.Attribute
    {
        public bool AllMembers;
        public bool Conditional;
    }
}
