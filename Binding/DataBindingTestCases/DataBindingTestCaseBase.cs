using System.Collections.Generic;
using UnityEngine;
using MVVM.Extension.Views.Common;
using MVVM.Extension.Services;
using Utils.Reflection;

namespace MVVM.Binding.Binding.DataBindingTestCases
{
    public abstract class DataBindingTestCaseBase : IDataBindingTestCase
    {
        private readonly DataBindingBase _binder;

        public Stack<string> Errors { get; } = new Stack<string>();

        public DataBindingTestCaseBase(DataBindingBase binder)
        {
            _binder = binder as DataBindingBase;
        }

        public virtual bool ExecuteTest()
        {
            if (_binder is null)
            {
                LogNotMineError();
                return false;
            }


            bool ValidatePreSelectedView(ViewBase preSelectedView, string viewModelName)
            {
                if (preSelectedView is null) return true;
                if (!preSelectedView.GetType()
                    .TryGetGenericTypeRecursive(DataBindingBase.PreSelectedViewFindTest, out var vmType)) return false;
                return vmType.GetGenericArguments()[0].ToString() == viewModelName;
            }

            var validateViewModelName = ViewModelProvider.IsViewModelTypeNameValid(_binder.ViewModelName);
            if(!validateViewModelName) Errors.Push("ViewModel name is not valid");
            var validatePreselectedView = ValidatePreSelectedView(_binder.PreSelectedView, _binder.ViewModelName);
            if(!validatePreselectedView) Errors.Push("PreSelected view is no longer valid");

            return validateViewModelName &&
                    validatePreselectedView;
        }

        protected void LogNotMineError()
        {
            Debug.unityLogger.LogError("passed object binder is not mine to test!", this);
        }
    }
}
