namespace Lib.Utilities
{
    public static class LinqExUtil
    {
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                var elementValue = keySelector(element);
                if (seenKeys.Add(elementValue))
                    yield return element;
            }
        }

        /// <summary>
        /// 組合 C n 取 k
        /// </summary>
        /// <param name="elements">集合</param>
        /// <param name="k">取幾</param>
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0 ? new[] { new T[0] } :
              elements.SelectMany((e, i) =>
                elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }

        /// <summary>
        /// Flattens a sequence containing arbitrarily-nested sequences.
        /// </summary>
        /// <param name="source">The sequence that will be flattened.</param>
        /// <param name="selector">A function that receives each element of the sequence as an object and projects an inner sequence to be flattened.</param>
        /// <returns>A sequence that contains the elements of source and all nested sequences projected via the selector function.</returns>
        /// <remarks>https://stackoverflow.com/questions/11830174/how-to-flatten-tree-via-linq</remarks>
        public static IEnumerable<TSource> Flatten<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> selector)
        {
            if (source == null)
                yield break;

            var stack = new Stack<TSource>(source);
            while (stack.Any())
            {
                var current = stack.Pop();
                yield return current;

                if (current == null) continue;

                var children = selector(current);
                if (children == null) continue;

                foreach (var child in children)
                    stack.Push(child);
            }
        }

    }
}
