using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EndlessDelivery.Utils;

public static class MathsExtensions
{
    //https://stackoverflow.com/questions/1287567/is-using-random-and-orderby-a-good-shuffle-algorithm
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        System.Random rng = new();

        T[] elements = source.ToArray();
        for (int i = elements.Length - 1; i >= 0; i--)
        {
            // Swap element "i" with a random earlier element it (or itself)
            // ... except we don't really need to swap it fully, as we can
            // return it immediately, and afterwards it's irrelevant.
            int swapIndex = rng.Next(i + 1);
            yield return elements[swapIndex];
            elements[swapIndex] = elements[i];
        }
    }

    public static List<T> ShuffleAndToList<T>(this IEnumerable<T> source)
    {
        List<T> returnList = new();
        System.Random rng = new();

        T[] elements = source.ToArray();
        for (int i = elements.Length - 1; i >= 0; i--)
        {
            // Swap element "i" with a random earlier element it (or itself)
            // ... except we don't really need to swap it fully, as we can
            // return it immediately, and afterwards it's irrelevant.
            int swapIndex = rng.Next(i + 1);
            returnList.Add(elements[swapIndex]);
            elements[swapIndex] = elements[i];
        }

        return returnList;
    }

    public static T Pick<T>(this T[] array)
    {
        if (array.Length == 0)
        {
            return (T)(object)null;
        }

        return array[Random.Range(0, array.Length)];
    }

    public static T Pick<T>(this List<T> list)
    {
        if (list.Count == 0)
        {
            return (T)(object)null;
        }

        return list[Random.Range(0, list.Count)];
    }

    public static Vector3 Only(this Vector3 vector, params Axis[] axes)
    {
        return new Vector3(
            axes.Contains(Axis.X) ? vector.x : 0,
            axes.Contains(Axis.Y) ? vector.y : 0,
            axes.Contains(Axis.Z) ? vector.z : 0);
    }
}
