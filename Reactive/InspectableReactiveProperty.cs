using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace MVVM.Binding.Reactive
{
    /// <summary>
    /// Inspectable ReactiveProperty.
    /// </summary>
    [Serializable]
    public class RxIntProperty : RxProperty<int>
    {
        public RxIntProperty()
            : base()
        {

        }

        public RxIntProperty(int initialValue)
            : base(initialValue)
        {

        }
    }

    /// <summary>
    /// Inspectable ReactiveProperty.
    /// </summary>
    [Serializable]
    public class RxLongProperty : RxProperty<long>
    {
        public RxLongProperty()
            : base()
        {

        }

        public RxLongProperty(long initialValue)
            : base(initialValue)
        {

        }
    }


    /// <summary>
    /// Inspectable ReactiveProperty.
    /// </summary>
    [Serializable]
    public class RxByteProperty : RxProperty<byte>
    {
        public RxByteProperty()
            : base()
        {

        }

        public RxByteProperty(byte initialValue)
            : base(initialValue)
        {

        }
    }

    /// <summary>
    /// Inspectable ReactiveProperty.
    /// </summary>
    [Serializable]
    public class RxFloatProperty : RxProperty<float>
    {
        public RxFloatProperty()
            : base()
        {

        }

        public RxFloatProperty(float initialValue)
            : base(initialValue)
        {

        }
    }

    /// <summary>
    /// Inspectable ReactiveProperty.
    /// </summary>
    [Serializable]
    public class RxDoubleProperty : RxProperty<double>
    {
        public RxDoubleProperty()
            : base()
        {

        }

        public RxDoubleProperty(double initialValue)
            : base(initialValue)
        {

        }
    }

    /// <summary>
    /// Inspectable ReactiveProperty.
    /// </summary>
    [Serializable]
    public class RxStringProperty : RxProperty<string>
    {
        public RxStringProperty()
            : base()
        {

        }

        public RxStringProperty(string initialValue)
            : base(initialValue)
        {

        }
    }

    /// <summary>
    /// Inspectable ReactiveProperty.
    /// </summary>
    [Serializable]
    public class RxBoolProperty : RxProperty<bool>
    {
        public RxBoolProperty()
            : base()
        {

        }

        public RxBoolProperty(bool initialValue)
            : base(initialValue)
        {

        }
    }

    /// <summary>Inspectable ReactiveProperty.</summary>
    [Serializable]
    public class RxVector2Property : RxProperty<Vector2>
    {
        public RxVector2Property()
        {

        }

        public RxVector2Property(Vector2 initialValue)
            : base(initialValue)
        {

        }

        protected override IEqualityComparer<Vector2> EqualityComparer
        {
            get { return UnityEqualityComparer.Vector2; }
        }
    }

    /// <summary>Inspectable ReactiveProperty.</summary>
    [Serializable]
    public class RxVector3Property : RxProperty<Vector3>
    {
        public RxVector3Property()
        {

        }

        public RxVector3Property(Vector3 initialValue)
            : base(initialValue)
        {

        }

        protected override IEqualityComparer<Vector3> EqualityComparer
        {
            get { return UnityEqualityComparer.Vector3; }
        }
    }

    /// <summary>Inspectable ReactiveProperty.</summary>
    [Serializable]
    public class RxVector4Property : RxProperty<Vector4>
    {
        public RxVector4Property()
        {

        }

        public RxVector4Property(Vector4 initialValue)
            : base(initialValue)
        {

        }

        protected override IEqualityComparer<Vector4> EqualityComparer
        {
            get { return UnityEqualityComparer.Vector4; }
        }
    }

    /// <summary>Inspectable ReactiveProperty.</summary>
    [Serializable]
    public class RxColorProperty : RxProperty<Color>
    {
        public RxColorProperty()
        {

        }

        public RxColorProperty(Color initialValue)
            : base(initialValue)
        {

        }

        protected override IEqualityComparer<Color> EqualityComparer
        {
            get { return UnityEqualityComparer.Color; }
        }
    }

    /// <summary>Inspectable ReactiveProperty.</summary>
    [Serializable]
    public class RxRectProperty : RxProperty<Rect>
    {
        public RxRectProperty()
        {

        }

        public RxRectProperty(Rect initialValue)
            : base(initialValue)
        {

        }

        protected override IEqualityComparer<Rect> EqualityComparer
        {
            get { return UnityEqualityComparer.Rect; }
        }
    }

    /// <summary>Inspectable ReactiveProperty.</summary>
    [Serializable]
    public class RxAnimationCurveProperty : RxProperty<AnimationCurve>
    {
        public RxAnimationCurveProperty()
        {

        }

        public RxAnimationCurveProperty(AnimationCurve initialValue)
            : base(initialValue)
        {

        }
    }

    /// <summary>Inspectable ReactiveProperty.</summary>
    [Serializable]
    public class RxBoundsProperty : RxProperty<Bounds>
    {
        public RxBoundsProperty()
        {

        }

        public RxBoundsProperty(Bounds initialValue)
            : base(initialValue)
        {

        }

        protected override IEqualityComparer<Bounds> EqualityComparer
        {
            get { return UnityEqualityComparer.Bounds; }
        }
    }

    /// <summary>Inspectable ReactiveProperty.</summary>
    [Serializable]
    public class RxQuaternionProperty : RxProperty<Quaternion>
    {
        public RxQuaternionProperty()
        {

        }

        public RxQuaternionProperty(Quaternion initialValue)
            : base(initialValue)
        {

        }

        protected override IEqualityComparer<Quaternion> EqualityComparer
        {
            get { return UnityEqualityComparer.Quaternion; }
        }
    }
}