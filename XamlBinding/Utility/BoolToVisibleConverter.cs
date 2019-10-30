using System;
using System.Windows;

namespace XamlBinding.Utility
{
    internal sealed class BoolToVisibleConverter : DelegateConverter
    {
        public BoolToVisibleConverter()
            : base(BoolToVisibleConverter.Convert)
        {
        }

        public static object Convert(object value, Type targetType, object parameter)
        {
            if (value is bool b)
            {
                if (parameter is bool inverse && inverse)
                {
                    return b ? Visibility.Collapsed : Visibility.Visible;
                }

                return b ? Visibility.Visible : Visibility.Collapsed;
            }

            throw new InvalidOperationException();
        }
    }
}
