using System;

namespace Assembly
{
    public class ClassWithChangingMembers
    {
        public int FieldThatWillBeRemoved;
        public int FieldThatWillChangeItsType;

        public event EventHandler EventThatWillBeRemoved;

        public string PropertyThatWillBeRemoved
        {
            get { return null; }
        }
        public string PropertyThatWillChangeType
        {
            get { return null; }
        }

        public void MethodThatWillBeRemoved()
        {
        }
        public void MethodThatWillGetNewParameter(int x)
        {
        }

        public bool MethodThatWillChangeReturnType()
        {
            return false;
        }

        public void MethodThatWillGetNewParameterWithDefaultValue(int x)
        {
        }
    }

    public class ClassThatWillChangeItsBaseClass : BaseClass
    {
    }

    public class BaseClass
    {
    }

    public enum EnumWithReorderedItems
    {
        One, Two
    }
}
