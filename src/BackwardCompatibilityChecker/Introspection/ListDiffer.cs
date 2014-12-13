using System;
using System.Collections.Generic;
using System.Linq;

namespace BackwardCompatibilityChecker.Introspection
{
    /// <summary>
    /// Compares two lists and creates two diff lists with added and removed elements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListDiffer<T>
    {
        readonly Func<T,T,bool> comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListDiffer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparer">The comparer function to check for equality in the collections to be compared.</param>
        public ListDiffer(Func<T, T, bool> comparer)
        {
            this.comparer = comparer;
        }

        /// <summary>
        /// Compare two lists A and B and add the new elements in B to added and the elements elements
        /// which occur only in A and not in B to the removed collection.
        /// </summary>
        /// <param name="listV1">The list v1.</param>
        /// <param name="listV2">The list v2.</param>
        /// <param name="added">New added elements in version 2</param>
        /// <param name="removed">Removed elements in version 2</param>
        public void Diff(IEnumerable<T> listV1, IEnumerable<T> listV2, Action<T> added, Action<T> removed)
        {
            foreach (var ai in listV1.Where(x => !listV2.Any(bi => this.comparer(x, bi))))
            {
                removed(ai);
            }

            foreach (var bi in listV2.Where(x => !listV1.Any(ai => this.comparer(x, ai))))
            {
                added(bi);
            }
        }
    }
}
