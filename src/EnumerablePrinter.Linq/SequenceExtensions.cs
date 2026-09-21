namespace EnumerablePrinter.Linq;

/// <summary>
/// Adds Python-style slicing to enumerable sequences.
/// </summary>
public static class SequenceExtensions
{
    /// <summary>
    /// Returns the elements in the specified range using a positive step.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The sequence to slice.</param>
    /// <param name="start">The inclusive starting index, or the beginning when omitted.</param>
    /// <param name="end">The exclusive ending index, or the end when omitted.</param>
    /// <param name="step">The number of positions to advance between elements.</param>
    /// <returns>A lazily evaluated sequence containing the selected elements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="step"/> is not positive.</exception>
    public static IEnumerable<T> Slice<T>(this IEnumerable<T> source,
        int? start = null,
        int? end = null,
        int step = 1)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        if (step <= 0)
            throw new ArgumentOutOfRangeException(nameof(step), "Step must be greater than zero.");



        return SliceIterator(source, start, end, step);
    }

    private static IEnumerable<T> SliceIterator<T>(IEnumerable<T> source, int? start, int? end, int step)
    {
        bool requiresBuffering = start < 0 || end < 0;

        if (!requiresBuffering)
        {
            int index = 0;
            int s = start ?? 0;
            int e = end ?? int.MaxValue;

            foreach (var item in source)
            {
                if (index >= s && index < e && ((index - s) % step == 0))
                {
                    yield return item;
                }
                if (index >= e)
                {
                    yield break;
                }
                index++;
            }
            yield break;
        }
        var buffer = new List<T>();
        foreach (var item in source)
        {
            buffer.Add(item);
        }

        int count = buffer.Count;

        int ResolveIndex(int? index, int defaultValue)
        {
            if (index.HasValue)
            {
                int resolvedIndex = index.Value >= 0 ? index.Value : count + index.Value;
                return Math.Max(0, Math.Min(resolvedIndex, count));
            }
            return defaultValue;
        }

        int startIndex = ResolveIndex(start, 0);
        int endIndex = ResolveIndex(end, count);

        for (int i = startIndex; i < endIndex; i += step)
        {
            yield return buffer[i];
        }

        yield break;
    }
}
