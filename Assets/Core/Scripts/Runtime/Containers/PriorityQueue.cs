using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Nexora
{
    /// <summary>
    /// Classic Priority Queue(Heap) implementation to use as it is not still supported in Unity. 
    /// </summary>
    /// <remarks>
    /// Binary Min-Heap implementation is done, that is, the top is the smallest object. 
    /// You can provide custom custom <see cref="IComparer{T}"/> class to override the logic.
    /// </remarks>
    /// <typeparam name="T">Type of the objects stored in the Priority Queue</typeparam>
    public class PriorityQueue<T> :
        IReadOnlyCollection<T>
    {
        private readonly List<T> _heap;
        private readonly IComparer<T> _comparer;

        public int Count => _heap.Count;

        public PriorityQueue()
        {
            _heap = new List<T>();
            _comparer = Comparer<T>.Default;
        }

        public PriorityQueue(IComparer<T> comparer)
            => _comparer = comparer;

        public PriorityQueue(IEnumerable<T> collection) : this(collection, Comparer<T>.Default) { }

        public PriorityQueue(IEnumerable<T> collection, IComparer<T> comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _heap = new List<T>();

            for (int i = (_heap.Count / 2) - 1; i >= 0; i--)
            {
                SiftDown(i);
            }
        }

        /// <summary>
        /// Moves the item in the index down to its designated place, while moving higher priorties up if exists.
        /// </summary>
        private void SiftDown(int index)
        {
            int leftChildIndex;
            while ((leftChildIndex = 2 * index + 1) < _heap.Count)
            {
                int rightChildIndex = leftChildIndex + 1;
                int smallerChildIndex = leftChildIndex;

                if (rightChildIndex < _heap.Count
                && _comparer.Compare(_heap[rightChildIndex], _heap[leftChildIndex]) < 0)
                {
                    smallerChildIndex = rightChildIndex;
                }

                if (_comparer.Compare(_heap[index], _heap[smallerChildIndex]) <= 0)
                {
                    break;
                }

                Swap(index, smallerChildIndex);
                index = smallerChildIndex;
            }
        }

        private void Swap(int index1, int index2)
        {
            T temp = _heap[index1];
            _heap[index1] = _heap[index2];
            _heap[index2] = temp;
        }

        /// <summary>
        /// Adds the item to the end of the list, then calls <see cref="SiftUp(int)"/> to maintain heap property.
        /// That is, having parents higher priority than children.
        /// </summary>
        public void Enqueue(T item)
        {
            _heap.Add(item);
            SiftUp(_heap.Count - 1);
        }

        /// <summary>
        /// Moves the index up in the heap until the parent is higher priority than the child.
        /// </summary>
        private void SiftUp(int index)
        {
            while(index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_comparer.Compare(_heap[index], _heap[parentIndex]) >= 0)
                {
                    break;
                }

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        /// <summary>
        /// Removes the top item from the queue, 
        /// then moves the smallest priority item to its designated place while at the same time finding the top item.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public T Dequeue()
        {
            if (_heap.IsEmpty())
            {
                throw new InvalidOperationException("Priority Queue is empty");
            }

            T item = _heap[0];
            int lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);

            if(_heap.Count > 1)
            {
                SiftDown(0);
            }

            return item;
        }

        public T Peek()
        {
            if(_heap.IsEmpty())
            {
                throw new InvalidOperationException("Priority Queue is empty");
            }

            return _heap[0];
        }

        public void Clear() => _heap.Clear();

        /// <summary>
        /// Sequentially checks if <paramref name="item"/> in the heap, use cautiously as it has O(n) complexity
        /// </summary>
        public bool Contains(T item) => _heap.Contains(item);

        public IEnumerator<T> GetEnumerator() => _heap.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
