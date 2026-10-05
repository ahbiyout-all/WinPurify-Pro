using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WinPurifyPro.Models;

namespace WinPurifyPro.Converters
{
    public class RiskToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isLightTheme = false;
            if (Application.Current?.Resources["TextPrimary"] is SolidColorBrush tp && tp.Color.R < 100)
            {
                isLightTheme = true;
            }

            if (value is RiskLevel risk)
            {
                if (isLightTheme)
                {
                    return risk switch
                    {
                        RiskLevel.Safe => new SolidColorBrush(Color.FromRgb(21, 128, 61)),   // Emerald-700
                        RiskLevel.Deep => new SolidColorBrush(Color.FromRgb(180, 83, 9)),    // Amber-700
                        RiskLevel.Risky => new SolidColorBrush(Color.FromRgb(185, 28, 28)),  // Red-700
                        _ => new SolidColorBrush(Color.FromRgb(71, 85, 105))
                    };
                }

                return risk switch
                {
                    RiskLevel.Safe => new SolidColorBrush(Color.FromRgb(34, 197, 94)),   // Green-500
                    RiskLevel.Deep => new SolidColorBrush(Color.FromRgb(234, 179, 8)),   // Amber-500
                    RiskLevel.Risky => new SolidColorBrush(Color.FromRgb(239, 68, 68)),  // Red-500
                    _ => new SolidColorBrush(Color.FromRgb(148, 163, 184))
                };
            }
            return isLightTheme ? new SolidColorBrush(Color.FromRgb(71, 85, 105)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class RiskToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RiskLevel risk)
            {
                return risk switch
                {
                    RiskLevel.Safe => new SolidColorBrush(Color.FromArgb(40, 34, 197, 94)),
                    RiskLevel.Deep => new SolidColorBrush(Color.FromArgb(40, 234, 179, 8)),
                    RiskLevel.Risky => new SolidColorBrush(Color.FromArgb(40, 239, 68, 68)),
                    _ => new SolidColorBrush(Color.FromArgb(40, 148, 163, 184))
                };
            }
            return new SolidColorBrush(Color.FromArgb(40, 148, 163, 184));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class StatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskStatus status && status != TaskStatus.Pending)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : false;
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }

    public class EqualityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return value.ToString()?.Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                if (int.TryParse(parameter?.ToString(), out int intVal)) return intVal;
                return parameter?.ToString() ?? string.Empty;
            }
            return Binding.DoNothing;
        }
    }

    public class CategoryToActiveBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = IsCategoryMatch(value, parameter);
            if (isSelected)
            {
                // Active Category distinctive background
                string catName = parameter?.ToString() ?? "All";
                return catName switch
                {
                    "Privacy" => new SolidColorBrush(Color.FromRgb(88, 28, 135)),   // Purple-900 (#581C87)
                    "Browser" => new SolidColorBrush(Color.FromRgb(12, 74, 110)),   // Sky-900 (#0C4A6E)
                    "Security" => new SolidColorBrush(Color.FromRgb(127, 29, 29)),  // Red-900 (#7F1D1D)
                    "Registry" => new SolidColorBrush(Color.FromRgb(120, 53, 15)),  // Amber-900 (#78350F)
                    "Update" => new SolidColorBrush(Color.FromRgb(19, 78, 74)),     // Teal-900 (#134E4A)
                    "System" => new SolidColorBrush(Color.FromRgb(20, 83, 45)),     // Green-900 (#14532D)
                    "Storage" => new SolidColorBrush(Color.FromRgb(49, 46, 129)),   // Indigo-900 (#312E81)
                    "Special" => new SolidColorBrush(Color.FromRgb(136, 19, 55)),   // Rose-900 (#881337)
                    _ => new SolidColorBrush(Color.FromRgb(30, 58, 138))            // Blue-900 (#1E3A8A)
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static bool IsCategoryMatch(object value, object parameter)
        {
            if (value == null && (parameter == null || parameter.ToString() == "All")) return true;
            if (value is TaskCategory selectedCat)
            {
                if (parameter is TaskCategory targetCat) return selectedCat == targetCat;
                if (parameter is string paramStr && Enum.TryParse<TaskCategory>(paramStr, true, out var parsedCat))
                {
                    return selectedCat == parsedCat;
                }
            }
            return false;
        }
    }

    public class CategoryToActiveForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = IsCategoryMatch(value, parameter);
            if (isSelected)
            {
                // Active Category distinctive highlight foreground
                string catName = parameter?.ToString() ?? "All";
                return catName switch
                {
                    "Privacy" => new SolidColorBrush(Color.FromRgb(216, 180, 254)),  // Purple-300
                    "Browser" => new SolidColorBrush(Color.FromRgb(125, 211, 252)),  // Sky-300
                    "Security" => new SolidColorBrush(Color.FromRgb(252, 165, 165)), // Red-300
                    "Registry" => new SolidColorBrush(Color.FromRgb(253, 224, 71)),  // Amber-300
                    "Update" => new SolidColorBrush(Color.FromRgb(94, 234, 212)),    // Teal-300
                    "System" => new SolidColorBrush(Color.FromRgb(134, 239, 172)),   // Green-300
                    "Storage" => new SolidColorBrush(Color.FromRgb(165, 180, 252)),  // Indigo-300
                    "Special" => new SolidColorBrush(Color.FromRgb(253, 164, 175)),  // Rose-300 (#FDA4AF)
                    _ => new SolidColorBrush(Color.FromRgb(147, 197, 253))           // Blue-300
                };
            }
            return Application.Current?.Resources["TextSecondary"] as Brush ?? new SolidColorBrush(Color.FromRgb(148, 163, 184));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static bool IsCategoryMatch(object value, object parameter)
        {
            if (value == null && (parameter == null || parameter.ToString() == "All")) return true;
            if (value is TaskCategory selectedCat)
            {
                if (parameter is TaskCategory targetCat) return selectedCat == targetCat;
                if (parameter is string paramStr && Enum.TryParse<TaskCategory>(paramStr, true, out var parsedCat))
                {
                    return selectedCat == parsedCat;
                }
            }
            return false;
        }
    }

    public class CategoryToActiveBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = IsCategoryMatch(value, parameter);
            if (isSelected)
            {
                string catName = parameter?.ToString() ?? "All";
                return catName switch
                {
                    "Privacy" => new SolidColorBrush(Color.FromRgb(168, 85, 247)),  // Purple-500
                    "Browser" => new SolidColorBrush(Color.FromRgb(14, 165, 233)),  // Sky-500
                    "Security" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),  // Red-500
                    "Registry" => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // Amber-500
                    "Update" => new SolidColorBrush(Color.FromRgb(20, 184, 166)),   // Teal-500
                    "System" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),    // Green-500
                    "Storage" => new SolidColorBrush(Color.FromRgb(99, 102, 241)),  // Indigo-500
                    "Special" => new SolidColorBrush(Color.FromRgb(244, 63, 94)),   // Rose-500 (#F43F5E)
                    _ => new SolidColorBrush(Color.FromRgb(59, 130, 246))           // Blue-500
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static bool IsCategoryMatch(object value, object parameter)
        {
            if (value == null && (parameter == null || parameter.ToString() == "All")) return true;
            if (value is TaskCategory selectedCat)
            {
                if (parameter is TaskCategory targetCat) return selectedCat == targetCat;
                if (parameter is string paramStr && Enum.TryParse<TaskCategory>(paramStr, true, out var parsedCat))
                {
                    return selectedCat == parsedCat;
                }
            }
            return false;
        }
    }

    public class CategoryToSpecialVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskCategory cat && cat == TaskCategory.Special)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BoolToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isDetected = value is bool b && b;
            string param = parameter?.ToString()?.ToLowerInvariant() ?? "";

            if (param == "dot")
            {
                return isDetected ? "🟢" : "⚪";
            }

            if (isDetected)
            {
                return new SolidColorBrush(Color.FromRgb(52, 211, 153)); // Emerald-400 (#34D399)
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(148, 163, 184)); // Slate-400 (#94A3B8)
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SpecialSubCategoryToActiveBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string current = value?.ToString() ?? "All";
            string target = parameter?.ToString() ?? "All";
            bool isMatch = string.Equals(current, target, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                return target switch
                {
                    "Account" => new SolidColorBrush(Color.FromRgb(136, 19, 55)),     // Rose-900 (#881337)
                    "FactoryReset" => new SolidColorBrush(Color.FromRgb(127, 29, 29)),// Red-900 (#7F1D1D)
                    "Profile" => new SolidColorBrush(Color.FromRgb(88, 28, 135)),     // Purple-900 (#581C87)
                    "Forensics" => new SolidColorBrush(Color.FromRgb(30, 41, 59)),    // Slate-800 (#1E293B)
                    _ => new SolidColorBrush(Color.FromRgb(76, 29, 149))              // Purple-900 (#4C1D95) - All
                };
            }
            return new SolidColorBrush(Color.FromRgb(15, 23, 42)); // Slate-900 (#0F172A)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SpecialSubCategoryToActiveForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string current = value?.ToString() ?? "All";
            string target = parameter?.ToString() ?? "All";
            bool isMatch = string.Equals(current, target, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                return target switch
                {
                    "Account" => new SolidColorBrush(Color.FromRgb(254, 205, 211)),      // Rose-200
                    "FactoryReset" => new SolidColorBrush(Color.FromRgb(254, 202, 202)), // Red-200
                    "Profile" => new SolidColorBrush(Color.FromRgb(233, 213, 255)),      // Purple-200
                    "Forensics" => new SolidColorBrush(Color.FromRgb(241, 245, 249)),    // Slate-100
                    _ => new SolidColorBrush(Color.FromRgb(245, 208, 254))               // Fuchsia-200
                };
            }
            return new SolidColorBrush(Color.FromRgb(148, 163, 184)); // Slate-400
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SpecialSubCategoryToActiveBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string current = value?.ToString() ?? "All";
            string target = parameter?.ToString() ?? "All";
            bool isMatch = string.Equals(current, target, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                return target switch
                {
                    "Account" => new SolidColorBrush(Color.FromRgb(244, 63, 94)),      // Rose-500
                    "FactoryReset" => new SolidColorBrush(Color.FromRgb(239, 68, 68)), // Red-500
                    "Profile" => new SolidColorBrush(Color.FromRgb(168, 85, 247)),     // Purple-500
                    "Forensics" => new SolidColorBrush(Color.FromRgb(100, 116, 139)),  // Slate-500
                    _ => new SolidColorBrush(Color.FromRgb(192, 38, 211))              // Fuchsia-600
                };
            }
            return new SolidColorBrush(Color.FromRgb(51, 65, 85)); // Slate-700 (#334155)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SpecialCategoryToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string current = value?.ToString() ?? "All";
            string target = parameter?.ToString() ?? "All";
            if (string.Equals(current, "All", StringComparison.OrdinalIgnoreCase)) return Visibility.Visible;
            return string.Equals(current, target, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
