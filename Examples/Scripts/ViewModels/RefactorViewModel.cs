#define CLASS_ENABLED
#define FIRST_PROPERTY_ENABLED

#if FIRST_PROPERTY_ENABLED
#define FIRST_PROPERTY_ORIGINAL_NAME
#endif

#if CLASS_ENABLED
#define CLASS_ORIGINAL_NAME
#if !CLASS_ORIGINAL_NAME
#define CLASS_NON_ORGINAL_NAMESPACE
#endif
#endif

#if CLASS_ENABLED

using System.ComponentModel.Composition;
using MVVM.Core.ViewModels;
using MVVM.Binding.Binding;

namespace MVVM.Binding.Examples
#if CLASS_NON_ORGINAL_NAMESPACE
.Refactored
#endif
{
    [Export]
#if CLASS_ORIGINAL_NAME
    [FormerlyBindedAs("RefactoredViewModel")]
#else
     [FormerlyBindedAs("RefactorViewModel")]
#endif
    public class
#if CLASS_ORIGINAL_NAME
        RefactorViewModel
#else 
        RefactoredViewModel
#endif
        : SimpleViewModel
    {
#if FIRST_PROPERTY_ENABLED
#if FIRST_PROPERTY_ORIGINAL_NAME
        [FormerlyBindedAs("StaticTextTest")]
#else
        [FormerlyBindedAs("TestStaticText")]
#endif
        public string
#if FIRST_PROPERTY_ORIGINAL_NAME
           TestStaticText
#else
           StaticTextTest
#endif
        { get; set; } = "Observable Collection";
#endif

    }
}

#endif
