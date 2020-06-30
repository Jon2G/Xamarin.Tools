using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.UWP.Services
{
    public class PrintHTML : Shared.Services.IPrintHTML
    {
        public bool Print(string HTML, string Printer)
        {
            try
            {
                DirectoryInfo TicketsPath = new DirectoryInfo(Shared.Tools.Instance.LibraryPath+ "\\TICKETS");
                if (!TicketsPath.Exists)
                {
                    TicketsPath.Create();
                }
                TicketsPath.Refresh();
                if (!TicketsPath.Exists)
                {
                    return false;
                }
                FileInfo file = new FileInfo(TicketsPath.FullName+$"\\{Guid.NewGuid().ToString("N")}.html");
                File.WriteAllText(file.FullName, HTML);
                file.Refresh();
                if (file.Exists)
                {
                    if (Printer == "Microsoft Print to PDF")
                    {
                        Process.Start(file.FullName);
                        return true;
                    }
                    // Spawn the code to print the packing slips
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.Arguments = $"-p \"{Printer}\" \"{file.FullName}\"";
                    string pathToExe = Shared.Tools.Instance.LibraryPath + "\\PrintHTML";
                    info.FileName = Path.Combine(pathToExe, "PrintHtml.exe");
                    using (Process p = Process.Start(info))
                    {
                        // Wait until it is finished
                        while (!p.HasExited)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                        // Return the exit code
                        return p.ExitCode == 0;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
    }
}
