namespace BackwardCompatibilityChecker.Introspection.Diff
{
    public class DiffOperation
    {
        public bool IsAdded { get; private set; }
        public bool IsRemoved { get { return !this.IsAdded; } }

        public DiffOperation(bool isAdded)
        {
            this.IsAdded = isAdded;
        }
    }


}
