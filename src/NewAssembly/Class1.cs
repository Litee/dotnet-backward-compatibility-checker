namespace Assembly
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
    }

    public class ClassThatWillChangeItsBaseClass : NewBaseClass
    {
    }

    public class BaseClass
    {
    }
    public class NewBaseClass
    {
    }
}