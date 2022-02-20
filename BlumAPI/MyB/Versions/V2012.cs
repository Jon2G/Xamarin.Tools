using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlumAPI.MyB.Versions
{
   public class V2012:MybVersion
    {
        public override string ConfigDbRealtivePath => "\\config.db";
        public V2012(VersionMyBusinessPos version, DirectoryInfo directory, DirectoryInfo virtualDirectory) : base(version, directory, virtualDirectory)
        {

        }
    }
}
