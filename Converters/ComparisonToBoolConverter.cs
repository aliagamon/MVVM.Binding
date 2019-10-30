using System;
using UnityEngine;

namespace MVVM.Binding.Converters
{
    public class ComparisonToBoolConverter : ValueConverterBase
    {
        enum ComparisonType
        {
            GreaterThan,
            LessThan,
            EqualTo,
            NotEqualTo,
            InRange
        }

        [SerializeField]
        ComparisonType _comparisonType;

        [SerializeField]
        string _compareTo;

        [SerializeField]
        bool _invert;

        public override object Convert(object value, Type targetType, object parameter)
        {
            var val = System.Convert.ToSingle(value);
            var result = false;

            switch (_comparisonType)
            {
                case ComparisonType.LessThan:
                    result = val < float.Parse(_compareTo);
                    break;
                case ComparisonType.GreaterThan:
                    result = val > float.Parse(_compareTo);
                    break;
                case ComparisonType.EqualTo:
                    result = Mathf.Approximately(val,float.Parse(_compareTo));
                    break;
                case ComparisonType.NotEqualTo:
                    result = Mathf.Approximately(val,float.Parse(_compareTo)) == false;
                    break;
                case ComparisonType.InRange:
                    var minMax = _compareTo.ToString().Split(',');
                    result = val < float.Parse(minMax[1]) && val > float.Parse(minMax[0]);
                    break;
                default:
                    break;
            }

            return _invert ? !result : result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
