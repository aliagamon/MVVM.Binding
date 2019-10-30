using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MVVM.Binding.Binding;
using MVVM.Binding.Reactive;
using MVVM.Core.ViewModels;
using UniRx;
using Utils.Unity.Runtime;
using Random = UnityEngine.Random;

namespace MVVM.Binding.Examples
{
    [Export]
    public class TestViewModel : SimpleViewModel
    {
        public class TestDataViewModel : SimpleViewModel
        {
            public string Message { get; set; }
            public UnityEngine.Color Color { get; set; }
        }

        public string TestStaticText { get; set; } = "Observable Collection";
        [TwoWayBindable] public RxStringProperty Text { get; set; } = new RxStringProperty();
        public RxBoolProperty BoolProp { get; set; } = new RxBoolProperty();
        public RxColorProperty Color { get; set; } = new RxColorProperty();
        public RxCommand ChangeColor { get; set; }
        public RxBoolProperty FlagProp { get; set; } = new RxBoolProperty();
        public RxBoolProperty VisabilityProp { get; set; } = new RxBoolProperty(true);
        public RxCollection<TestDataViewModel> TestCollection { get; set; } = new RxCollection<TestDataViewModel>();

        public RxCollection<TestDataViewModel> StaticItemsCollection { get; set; } =
            new RxCollection<TestDataViewModel>(
                new List<TestDataViewModel>()
                {
                    new TestDataViewModel()
                        {Message = Random.Range(0, 1000).ToString(), Color = Random.ColorHSV()},
                    new TestDataViewModel()
                        {Message = Random.Range(0, 1000).ToString(), Color = Random.ColorHSV()},
                    new TestDataViewModel()
                        {Message = Random.Range(0, 1000).ToString(), Color = Random.ColorHSV()},
                    new TestDataViewModel()
                        {Message = Random.Range(0, 1000).ToString(), Color = Random.ColorHSV()}
                });


        public void Start()
        {
            ChangeColor = FlagProp.ToMVVMReactiveCommand();
            ChangeColor.Subscribe(_ => Color.Value = Random.ColorHSV());
            Text.Value = DateTime.Now.ToShortTimeString();
            CoExecute.Execute(ChangeRoutine);
            CoExecute.Execute(UnityUpdate);
        }

        private IEnumerator UnityUpdate()
        {
            while (true)
            {
                if (DateTime.Now.Second % 5 == 0)
                    Text.Value = DateTime.Now.ToShortTimeString();
                BoolProp.Value = DateTime.Now.Second % 2 == 0;
                yield return null;
            }
        }

        private IEnumerator ChangeRoutine()
        {
            while (true)
            {
                FlagProp.Value = !FlagProp.Value;
                VisabilityProp.Value = !VisabilityProp.Value;
                if (TestCollection.Count > 0)
                    TestCollection.RemoveAt(Random.Range(0, TestCollection.Count - 1));
                for (var i = 0; i < 2; i++)
                    TestCollection.Add(new TestDataViewModel()
                        {Message = Random.Range(0, 1000).ToString(), Color = Random.ColorHSV()});
                yield return new UnityEngine.WaitForSeconds(3f);
            }

        }
    }
}
