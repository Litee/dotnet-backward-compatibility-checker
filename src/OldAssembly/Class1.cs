using System;

namespace Assembly
{
    public class Class1
    {
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
}
