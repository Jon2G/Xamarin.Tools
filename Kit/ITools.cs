using System;
using System.Collections.Generic;
using System.Text;

namespace Kit
{
    public interface ITools
    {
        ITools Init(Kit.Services.Interfaces.ICustomMessageBox CustomMessageBox, string LogDirectory, bool AlertAfterCritical = false);
        void CriticalAlert(object sender, EventArgs e);
    }
}
