using System;

namespace MVVM.Binding.Converters
{
    public class ValueRelayConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter)
        {
            return value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}