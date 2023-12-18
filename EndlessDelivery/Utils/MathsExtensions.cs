using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EndlessDelivery.Utils
{
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

        public static T Pick<T>(this List<T> list)
        {
            return list[Random.Range(0, list.Count - 1)];
        }

        public static Vector3 Only(this Vector3 vector, params Axis[] axes)
        {
            return new Vector3(
                axes.Contains(Axis.X) ? vector.x : 0, 
                axes.Contains(Axis.Y) ? vector.y : 0,
                axes.Contains(Axis.Z) ? vector.z : 0);
        }
    }
}