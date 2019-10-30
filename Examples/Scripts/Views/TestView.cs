using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using DryIocAttributes;
using MVVM.Extension.Views.Common;
using UnityEngine;

namespace MVVM.Binding.Examples
{
    [Export]
    public class TestView : InjectableViewBase<TestViewModel>
    {
        [Export, AsDecorator()]
        public static TestView Init(TestView self, TestViewModel vm)
        {
            self.Initialize(vm);

            vm.Initialize();
            vm.Start();

            return self;
        }
    }
}
