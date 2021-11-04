using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlumAPI.MyB.Versions
{
    public class V2011 : MybVersion
    {

        public V2011(VersionMyBusinessPos version, DirectoryInfo directory, DirectoryInfo virtualDirectory) : base(version, directory, virtualDirectory)
        {

        }

        public override string ConfigDbRealtivePath => "\\config.db";
    }
}
