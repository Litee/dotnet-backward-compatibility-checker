using System;

namespace BackwardCompatibilityChecker.Introspection.Diff
{
    public class DiffResult<T>
    {
        public DiffOperation Operation  { get; private set;   }
        public T ObjectV1   { get; private set; }

        public DiffResult(T v1, DiffOperation diffType)
        {
            this.ObjectV1 = v1;
            this.Operation = diffType;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}", this.ObjectV1, this.Operation.IsAdded ? "added" : "removed");
        }
    }
}
