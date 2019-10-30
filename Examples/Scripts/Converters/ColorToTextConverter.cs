using System;
using MVVM.Binding.Converters;
using UnityEngine;

namespace MVVM.Binding.Examples.Scripts.Converters
{
    public class ColorToTextConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter)
        {
            return string.Format("The Color is: {0}", ((Color)value));
        }

        public override object ConvertBack(object value, Type targetType, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
