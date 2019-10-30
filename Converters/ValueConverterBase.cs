﻿using System;
using UnityEngine;

namespace MVVM.Binding.Converters
{
    public abstract class ValueConverterBase : MonoBehaviour, IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter);

        public abstract object ConvertBack(object value, Type targetType, object parameter);
    }
}