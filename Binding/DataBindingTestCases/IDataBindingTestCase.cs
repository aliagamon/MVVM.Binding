using System.Collections.Generic;

namespace MVVM.Binding.Binding.DataBindingTestCases
{
    public interface IDataBindingTestCase
    {
        bool ExecuteTest();
        Stack<string> Errors { get; }
    }
}
