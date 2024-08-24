using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Extensions
{
    public static class ListExtensions
    {
        #region Class Methods

        public static T GetRandomFromList<T>(this List<T> list)
        {
            T random = list[Random.Range(0, list.Count)];
            return random;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion Class Methods
    }
}