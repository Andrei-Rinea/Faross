using System.Collections.Generic;
using System.Linq;

namespace Faross.Util
{
    public static class EqualsUtil
    {
        /// <summary>
        /// Checks two <see cref="IReadOnlyCollection{T}"/> instances for equivalent contents (i.e. they contain the
        /// same number of items and each item in the first one contains an equivalent ("Equals") item in the second)
        /// </summary>
        /// <param name="first">the first collection</param>
        /// <param name="second">the second collection</param>
        /// <typeparam name="T">the type of the items</typeparam>
        /// <returns>true if the collections are equivalent, false if not equivalent, including the case when one or
        /// both are null</returns>
        public static bool Equivalent<T>(this IReadOnlyCollection<T> first, IReadOnlyCollection<T> second)
        {
            if (first == null || second == null) return false;
            if (ReferenceEquals(first, second)) return true;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (first.Count != second.Count) return false;
            return first.All(second.Contains);
        }

        /// <summary>
        /// Checks two arrays for contents equals ("deep-equals")
        /// </summary>
        /// <param name="first">The first array</param>
        /// <param name="second">The second array</param>
        /// <typeparam name="T">The type of the arrays' items</typeparam>
        /// <returns>True if the arrays are not null, the same size, the same contens, in the same order
        /// or false otherwise</returns>
        public static bool ArraysEqual<T>(this T[] first, T[] second)
        {
            if (first == null || second == null) return false;
            if (ReferenceEquals(first, second)) return true;
            if (first.Length != second.Length) return false;
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < first.Length; i++)
                if (!Equals(first[i], second[i])) return false;
            return true;
        }
    }
}