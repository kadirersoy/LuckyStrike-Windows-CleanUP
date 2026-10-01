using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LuckyStrikeCleanUp
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StepStatus)
            {
                StepStatus status = (StepStatus)value;
                switch (status)
                {
                    case StepStatus.Running:
                        return new SolidColorBrush(Color.FromRgb(0, 210, 255)); // Cyan
                    case StepStatus.Completed:
                        return new SolidColorBrush(Color.FromRgb(0, 230, 118)); // Green
                    case StepStatus.Warning:
                        return new SolidColorBrush(Color.FromRgb(255, 171, 0)); // Amber
                    case StepStatus.Error:
                        return new SolidColorBrush(Color.FromRgb(255, 82, 82)); // Red
                    case StepStatus.Skipped:
                        return new SolidColorBrush(Color.FromRgb(100, 110, 130)); // Muted
                    default:
                        return new SolidColorBrush(Color.FromRgb(160, 170, 190)); // Idle
                }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToBadgeBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StepStatus)
            {
                StepStatus status = (StepStatus)value;
                switch (status)
                {
                    case StepStatus.Running:
                        return new SolidColorBrush(Color.FromArgb(40, 0, 210, 255));
                    case StepStatus.Completed:
                        return new SolidColorBrush(Color.FromArgb(40, 0, 230, 118));
                    case StepStatus.Warning:
                        return new SolidColorBrush(Color.FromArgb(40, 255, 171, 0));
                    case StepStatus.Error:
                        return new SolidColorBrush(Color.FromArgb(40, 255, 82, 82));
                    case StepStatus.Skipped:
                        return new SolidColorBrush(Color.FromArgb(30, 100, 110, 130));
                    default:
                        return new SolidColorBrush(Color.FromArgb(25, 255, 255, 255));
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StepStatus)
            {
                StepStatus status = (StepStatus)value;
                switch (status)
                {
                    case StepStatus.Running: return "⚡";
                    case StepStatus.Completed: return "✓";
                    case StepStatus.Warning: return "⚠";
                    case StepStatus.Error: return "✕";
                    case StepStatus.Skipped: return "⊘";
                    default: return "○";
                }
            }
            return "○";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
