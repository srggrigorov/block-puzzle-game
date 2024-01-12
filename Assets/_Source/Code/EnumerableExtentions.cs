using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
    public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        return !source.Any(predicate);
    }

    public static bool None<TSource>(this IEnumerable<TSource> source)
    {
        return !source.Any();
    }

    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
    {
        foreach (T item in enumeration)
        {
            action(item);
        }
    }

    public static T Random<T>(this IEnumerable<T> source)
    {
        return source.Random(s => 1);
    }

    public static T Random<T>(this IEnumerable<T> source, Func<T, float> chanceSelector)
    {
        return source.Random(chanceSelector, () => UnityEngine.Random.value);
    }

    public static T Random<T>(this IEnumerable<T> source, Func<T, float> chanceSelector, Func<float> randomProvider)
    {
        var chances = source.Select(chanceSelector).ToArray();
        var cumulativeChance = chances.Sum();
        var random = randomProvider();

        foreach (var item in source.OrderByDescending(i => chanceSelector(i)))
        {
            var chance = chanceSelector(item) / cumulativeChance;
            if (random < chance)
            {
                return item;
            }

            random -= chance;
        }

        throw new InvalidOperationException("Sequence is empty or no element was selected based on chance.");
    }
}