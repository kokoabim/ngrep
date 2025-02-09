namespace Kokoabim.NGrep;

internal static class IEnumerableExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action, Func<T, bool>? predicate = null)
    {
        List<T> results = [];

        foreach (T item in source)
        {
            if (predicate == null || predicate(item))
            {
                action(item);
                results.Add(item);
            }
        }

        return results;
    }
}
