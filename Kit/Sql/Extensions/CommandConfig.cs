using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Kit
{
    public class CommandConfig
    {
        public bool ManualRead = false;
        public int CommandTimeout = 30;

        public CommandType CommandType = CommandType.Text;

        public CommandConfig()

        {
        }
    }
}
