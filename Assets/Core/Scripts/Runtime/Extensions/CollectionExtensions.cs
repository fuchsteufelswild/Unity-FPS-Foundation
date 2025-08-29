using System.Collections.Generic;
using System;
using UnityEngine;

namespace Nexora
{
    public static partial class CollectionExtensions
    {
        public enum SelectionMethod
        {
            Random,
            RandomExcludeLastSelected,
            Sequence
        }

        public static bool IsEmpty<T>(this IReadOnlyCollection<T> container)
            => container.Count == 0;

        public static bool Contains<T>(this IReadOnlyList<T> container, T item) 
            => IndexOf(container, item) != CollectionConstants.NotFound;

        /// <summary>
        /// Returns the index of the first occurence of the item in the container
        /// </summary>
        public static int IndexOf<T>(this IReadOnlyList<T> container, T item)
        {
            if(container is List<T> list)
            {
                return list.IndexOf(item);
            }

            if(container is T[] array)
            {
                return Array.IndexOf(array, item);
            }

            for(int i = 0; i < container.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(container[i], item))
                {
                    return i;
                }
            }

            return -1;
        }

        public static T First<T>(this IReadOnlyList<T> container)
        {
            if (container.Count == 0)
            {
                return default;
            }

            return container[0];
        }

        public static bool TryGetFirst<T>(this IReadOnlyList<T> container, out T result)
        {
            if (container.Count == 0)
            {
                result = default;
                return false;
            }

            result = container[0];
            return true;
        }

        public static T Last<T>(this IReadOnlyList<T> container)
        {
            if(container.Count == 0)
            {
                return default;
            }

            return container[^1];
        }

        public static bool TryGetLast<T>(this IReadOnlyList<T> container, out T result)
        {
            if (container.Count == 0)
            {
                result = default;
                return false;
            }

            result = container[^1];
            return true;
        }

        public static T RemoveLast<T>(this IList<T> list)
        {
            if(list.Count == 0)
            {
                return default;
            }

            T lastElement = list[^1];
            list.RemoveAt(list.Count - 1);
            return lastElement;
        }

        /// <summary>
        /// Returns first element in the <paramref name="container"/> that matches the <paramref name="predicate"/>.
        /// </summary>
        public static T First<T>(this IReadOnlyList<T> container, Func<T, bool> predicate)
            where T : class
        {
            foreach(T element in container)
            {
                if(predicate(element))
                {
                    return element;
                }
            }

            return null;
        }

        /// <summary>
        /// Fills <paramref name="result"/> with 
        /// first element in the <paramref name="container"/> that matches the <paramref name="predicate"/>.
        /// </summary>
        /// <returns><see langword="true"/> if it is found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetFirst<T>(this IReadOnlyList<T> container, Func<T, bool> predicate, out T result)
            where T : class
        {
            foreach (T element in container)
            {
                if (predicate == null || predicate(element))
                {
                    result = element;
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Returns last element in the <paramref name="container"/> that matches the <paramref name="predicate"/>.
        /// </summary>
        public static T Last<T>(this IReadOnlyList<T> container, Func<T, bool> predicate)
            where T : class
        {
            foreach (T element in container.AsReverseEnumerator())
            {
                if (predicate(element))
                {
                    return element;
                }
            }

            return null;
        }

        /// <summary>
        /// Fills <paramref name="result"/> with 
        /// last element in the <paramref name="container"/> that matches the <paramref name="predicate"/>.
        /// </summary>
        /// <returns><see langword="true"/> if it is found, <see langword="false"/> otherwise.</returns>
        public static bool TryGetLast<T>(this IReadOnlyList<T> container, Func<T, bool> predicate, out T result)
            where T : class
        {
            foreach (T element in container.AsReverseEnumerator())
            {
                if (predicate(element))
                {
                    result = element;
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Gets the element at index but starting from the <b>end</b>.
        /// E.g for <paramref name="index"/> value of 5, it would return 
        /// the 5th element from the end.
        /// </summary
        /// <remarks>
        /// <![CDATA[index -> container.Count - 1 - index]]>
        /// </remarks>
        public static T GetFromReverse<T>(this IReadOnlyList<T> container, int index)
        {
            if(index < 0 || container.Count <= index)
            {
                throw new ArgumentOutOfRangeException();        
            }

            return container[container.Count - 1 - index];
        }

        /// <summary>
        /// Adds the element to the <paramref name="list"/>, only if <paramref name="element"/>
        /// is not already in the list.
        /// </summary>
        public static void AddUnique<T>(this List<T> list, T element)
        {
            if(list.Contains(element) == false)
            {
                list.Add(element);
            }
        }

        /// <summary>
        /// Moves the passed <paramref name="element"/>, to the end of the <paramref name="list"/>.
        /// </summary>
        public static void MoveToEnd<T>(this List<T> list, T element)
        {
            list.Remove(element);
            list.Add(element);
        }

        public static IEnumerable<T> AsReverseEnumerator<T>(this IReadOnlyList<T> container)
        {
            for(int i = container.Count - 1; i>= 0; i--)
            {
                yield return container[i];
            }
        }

        public static T SelectElement<T>(
            this IReadOnlyList<T> container, 
            ref int lastSelectedIndex, 
            SelectionMethod selectionMethod = SelectionMethod.RandomExcludeLastSelected)
        {
            return selectionMethod switch
            {
                SelectionMethod.Random => SelectRandom(container),
                SelectionMethod.RandomExcludeLastSelected => SelectRandomExcludeLastSelected(container, ref lastSelectedIndex),
                SelectionMethod.Sequence => SelectSequence(container, ref lastSelectedIndex),
                _ => default
            };
        }

        public static T SelectRandom<T>(this IReadOnlyList<T> container)
        {
            if(container == null || container.Count == 0)
            {
                return default;
            }

            return container[UnityEngine.Random.Range(0, container.Count)];
        }

        /// <summary>
        /// Randomly selects an element while avoiding the last selected index
        /// </summary>
        public static T SelectRandomExcludeLastSelected<T>(this IReadOnlyList<T> container, ref int lastSelectedIndex)
        {
            if(container == null || container.Count == 0)
            {
                return default;
            }

            // Prevent infinite loop as the last selected index always be 0
            if(container.Count == 1)
            {
                return container[0];
            }

            int nextIndex;
            do
            {
                nextIndex = UnityEngine.Random.Range(0, container.Count);
            } while (nextIndex == lastSelectedIndex);

            lastSelectedIndex = nextIndex;
            return container[nextIndex];
        }

        /// <summary>
        /// Selects items in a sequence, like 0,1,2,3,4...n,0,1,.. indices.
        /// </summary>
        public static T SelectSequence<T>(this IReadOnlyList<T> container, ref int lastSelectedIndex)
        {
            if(container == null || container.Count == 0)
            {
                return default;
            }

            lastSelectedIndex = (int)Mathf.Repeat(lastSelectedIndex + 1, container.Count);
            return container[lastSelectedIndex];
        }

        /// <summary>
        /// Returns an element randomly that passes the filter.
        /// </summary>
        /// <remarks>
        /// If later needed to use with other selection types, may pass additional index selector functor and select index accordingly
        /// instead of simple Random(0, n).
        /// </remarks>
        public static T SelectRandomFiltered<T>(this IReadOnlyList<T> container, Func<T, bool> filter)
        {
            const int MaxStackAllocSize = 64;

            if(container == null || container.Count == 0)
            {
                return default;
            }

            Span<int> validIndices = container.Count <= MaxStackAllocSize
                ? stackalloc int[container.Count]
                : new int[container.Count];

            int validIndexCount = 0;
            for (int i = 0; i < container.Count; i++)
            {
                if(filter == null || filter(container[i]))
                {
                    validIndices[validIndexCount++] = i;
                }
            }

            return validIndexCount == 0
                ? default
                : container[validIndices[UnityEngine.Random.Range(0, validIndexCount)]];
        }

        public static void RemoveDuplicates<T>(ref T[] array) 
        {
            HashSet<T> seen = new HashSet<T>();

            int windowLeft = 0;
            int windowRight = 0;

            for(; windowRight < array.Length; windowRight++)
            {
                if(seen.Add(array[windowRight]))
                {
                    array[windowLeft++] = array[windowRight];
                }
            }

            Array.Resize(ref array, windowLeft);
        }

        public static void RemoveAtIndex<T>(ref T[] array, int index)
        {
            var modifiedArray = new T[array.Length - 1];

            Array.Copy(array, 0, modifiedArray, 0, index);
            Array.Copy(array, index + 1, modifiedArray, index, modifiedArray.Length - index);

            array = modifiedArray;
        }
    }
}
