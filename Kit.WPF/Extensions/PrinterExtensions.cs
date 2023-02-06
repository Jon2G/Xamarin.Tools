using System.Collections.Generic;
using System.Linq;

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