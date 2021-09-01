using Kit.Extensions;
using System.Collections.ObjectModel;

namespace Kit.Forms.Controls.LogsViewer
{
   public class LogViewerModel
    {
        public int MaxLogs { get; set; }
        public ObservableCollection<LogMsg> Logs { get; set; }

        public LogViewerModel()
        {
            Log.LogsSink.OnLogEmit = new Command<LogMsg>(OnLogEmit);
            MaxLogs = 500;
        }
    

        private void OnLogEmit(LogMsg msg)
        {
            if (Logs.Count > MaxLogs)
            {
                Logs.Clear();
            }
            Logs.Add(msg);

            //if (LogsViewModel.logFile != null)
            //    LogsViewModel.logFile.AllText.Add(string.Format(Format, msg.TimeStamp, msg.Lvl, msg.Text));
            //LogsViewModel.logFile.AppendText();
        }
    }
}
