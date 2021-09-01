using System;
using Serilog.Core;
using Serilog.Events;

namespace Kit.Extensions
{
    public struct LogMsg
    {
        public string Lvl;
        public string Text;
        public string TimeStamp;
    }
    public class LogsSink : ILogEventSink
    {
        public Command<LogMsg> OnLogEmit { get; set; }
        public LogsSink()
        {
            
        }
        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write</param>
        public void Emit(LogEvent logEvent)
        {
            OnLogEmit?.Execute(new LogMsg()
            {
                Text = logEvent.RenderMessage(),
                Lvl = LevelToSeverity(logEvent),
                TimeStamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " -" + logEvent.Timestamp.Offset.ToString(@"hh\:mm"),
            });
        }

        static string LevelToSeverity(LogEvent logEvent)
        {
            switch (logEvent.Level)
            {
                case LogEventLevel.Debug:
                    return "[DBG]";
                case LogEventLevel.Error:
                    //MainPageViewModel.statusBar.AddError();
                    return "[ERR]";
                case LogEventLevel.Fatal:
                    return "[FTL]";
                case LogEventLevel.Verbose:
                    return "VERBOSE";
                case LogEventLevel.Warning:
                    //MainPageViewModel.statusBar.AddWarning();
                    return "WARNING";
                default:
                    return "[INF]";
            }
        }
    }
}
