using System;
using System.Linq;
using UnityEngine;

namespace MVVM.Binding.Converters
{
    public class ToBooleanConverterOrAggregatorConverter : ValueConverterBase
    {
        [SerializeField]
        private ValueConverterBase[] _converters;
        
        public override object Convert(object value, Type targetType, object parameter)
        {
            return _converters.Aggregate(true, (r, c) => r || (bool) c.Convert(value, typeof(bool), parameter));
        }

        public override object ConvertBack(object value, Type targetType, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}