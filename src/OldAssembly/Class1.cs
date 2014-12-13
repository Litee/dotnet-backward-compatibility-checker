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
    }

    public class ClassThatWillChangeItsBaseClass : BaseClass
    {
    }

    public class BaseClass
    {
    }
}
