using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Data
{
    public interface ITools
    {
        ITools InitAll(string LogDirectory, bool AlertAfterCritical = false);
        ITools InitLoggin(string LogDirectory, bool AlertAfterCritical = false);
        void CriticalAlert(object sender, EventArgs e);
    }
}
