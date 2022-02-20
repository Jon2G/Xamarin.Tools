using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlumAPI.MyB.Versions
{
   public class V2017:MybVersion
    {
        public override string ConfigDbRealtivePath => "\\configuracion\\config.db";
        public V2017(VersionMyBusinessPos version, DirectoryInfo directory, DirectoryInfo virtualDirectory) : base(version, directory, virtualDirectory)
        {

        }
    }
}
