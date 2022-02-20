using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlumAPI.MyB.Versions
{
   public class V2019:MybVersion
    {
        public override string ConfigDbRealtivePath => "\\configuracion\\config.db";
        public V2019(VersionMyBusinessPos version, DirectoryInfo directory, DirectoryInfo virtualDirectory) : base(version, directory, virtualDirectory)
        {

        }
    }
}
