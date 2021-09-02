using AsyncAwaitBestPractices.MVVM;
using Kit.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kit.Forms.Controls.LogsViewer
{
    public class LogViewerModel : IDisposable
    {
        public int MaxLogs { get; set; }
        public ObservableCollection<LogMsg> Logs { get; set; }
        private ICommand _AlertLogCommand;
        public ICommand AlertLogCommand => _AlertLogCommand ??= new AsyncCommand<LogMsg>(AlertLog);

        private Task AlertLog(LogMsg msg) => Acr.UserDialogs.UserDialogs.Instance.AlertAsync(msg.Text, msg.Level, "Ok");

        public LogViewerModel()
        {
            Logs = new ObservableCollection<LogMsg>();
            Log.LogsSink.OnLogEmit = new Command<LogMsg>(OnLogEmit);
            MaxLogs = 500;
        }


        private void OnLogEmit(LogMsg msg)
        {
            Tools.Instance.SynchronizeInvoke.BeginInvokeOnMainThread(() =>
            {
                if (Logs.Count > MaxLogs)
                {
                    Logs.Clear();
                }
                Logs.Insert(0, msg);
            });
            //if (LogsViewModel.logFile != null)
            //    LogsViewModel.logFile.AllText.Add(string.Format(Format, msg.TimeStamp, msg.Lvl, msg.Text));
            //LogsViewModel.logFile.AppendText();
        }

        public void Dispose()
        {
            Log.LogsSink.OnLogEmit = null;
        }
    }
}
