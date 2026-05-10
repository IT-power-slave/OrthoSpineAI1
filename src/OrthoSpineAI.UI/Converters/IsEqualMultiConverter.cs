using System;
using System.Globalization;
using System.Windows.Data;

namespace OrthoSpineAI.UI.Converters;

/// <summary>
/// Returns true when all bound values are equal (used to compare a selected item
/// against the current DataTemplate item inside a DataTrigger.Binding).
/// </summary>
public class IsEqualMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return false;

        return Equals(values[0], values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
