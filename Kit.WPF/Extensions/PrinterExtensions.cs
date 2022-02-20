using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kit.WPF
{
    public static class PrinterExtensions
    {
        public static List<string> GetInstalledPrinters()
        {
            return System.Drawing.Printing.PrinterSettings.InstalledPrinters.OfType<string>().ToList();
        }
    }
}