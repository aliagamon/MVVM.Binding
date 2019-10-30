using System;
using UnityEngine;

namespace MVVM.Binding.Converters
{
    public class BoolToSpriteConverter : ValueConverterBase
    {
        [SerializeField] Sprite _trueSprite;
        [SerializeField] Sprite _falseSprite;

        public override object Convert(object value, Type targetType, object parameter)
        {
            var b = (bool)value;

            return b ? _trueSprite : _falseSprite;

        }

        public override object ConvertBack(object value, Type targetType, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}