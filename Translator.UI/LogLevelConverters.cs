using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Serilog.Events;

namespace Translator.UI
{
    class LogLevelBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = value as LogEventLevel?;
            switch (v)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Information:
                case LogEventLevel.Debug:
                    return new SolidColorBrush(Colors.AliceBlue);
                case LogEventLevel.Warning:
                    return new SolidColorBrush(Colors.LightYellow);
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    return new SolidColorBrush(Colors.LightCoral);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class LogLevelIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = value as double?;
            if (v != null)
            {
                return (LogEventLevel)(int)Math.Round(v.Value);
            }
            return LogEventLevel.Verbose;
        }
    }
}
