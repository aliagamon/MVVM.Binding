using System;
using System.Collections.Generic;
using System.Linq;

namespace MVVM.Binding.Binding
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class FormerlyBindedAs : Attribute
    {
        public readonly string OldName;

        public FormerlyBindedAs(string oldName)
        {
            OldName = oldName;
        }

        public static IEnumerable<Type> GetNewName(string oldName)
        {
            return from asm in AppDomain.CurrentDomain.GetAssemblies()
                from type in asm.GetTypes()
                let attributes = type.GetCustomAttributes(typeof(FormerlyBindedAs), true)
                where attributes.Length > 0
                where attributes.Any(att => ((FormerlyBindedAs) att).OldName == oldName)
                select type;
        }
    }
}
