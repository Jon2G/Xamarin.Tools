using Kit.Daemon.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Kit.WPF.Converters
{
    public class SyncStatusToTextConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SyncStatus status)
            {
                switch (status)
                {
                    case SyncStatus.Pending:
                    case SyncStatus.Unknown:
                        return "En cola";
                    case SyncStatus.Done:
                        return "Hecho";
                    case SyncStatus.Failed:
                        return "Fallido";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            throw new ArgumentOutOfRangeException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
