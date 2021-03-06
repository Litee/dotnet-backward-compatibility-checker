﻿namespace Assembly
{
    public class ClassWithChangingMembers
    {
        public string FieldThatWillChangeItsType;

        public int PropertyThatWillChangeType
        {
            get { return 0; }
        }

        public string MethodThatWillChangeReturnType()
        {
            return null;
        }

        public void MethodThatWillGetNewParameter(int x, string y)
        {
        }
        public void MethodThatWillGetNewParameterWithDefaultValue(int x, string y = "abc")
        {
        }
    }

    public class NewClass
    {
    }
    public interface NewInterface
    {
    }

    public class ClassThatWillChangeItsBaseClass : NewBaseClass
    {
    }

    public class ClassThatGetsGenericParameter<T>
    {
    }

    public class BaseClass
    {
    }

    public class NewBaseClass
    {
    }

    public enum EnumWithReorderedItems
    {
        Two, One
    }
}