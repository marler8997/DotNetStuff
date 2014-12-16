using System;
using System.Collections;
using System.Collections.Generic;

namespace More
{
    public class LinkedQueue<T> : IEnumerable<T>, ICollection<T>
    {
        readonly LinkedList<T> items = new LinkedList<T>();
        public LinkedQueue()
            : base()
        {
        }
        public Int32 Count { get { return items.Count; } }
        public void Enqueue(T item)
        {
            items.AddLast(item);
        }
        public T Dequeue()
        {
            var first = items.First;
            if (first == null)
                throw new InvalidOperationException("Cannot call Dequeue on an empty queue");
            var firstValue = first.Value;
            items.RemoveFirst();
            return firstValue;
        }
        public T Peek()
        {
            var first = items.First;
            if (first == null)
                throw new InvalidOperationException("Cannot call Peek on an empty queue");
            return first.Value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>Same as Enqueue, only exists to accomodate the ICollection interface</summary>
        /// <param name="item">The item to enqueue</param>
        public void Add(T item)
        {
            Enqueue(item);
        }
        /// <summary>
        /// Removes the first occurrence of the specified value.  Performance cost is O(n).
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>true if the value was removed, otherwise, false</returns>
        public Boolean Remove(T item)
        {
            return items.Remove(item);
        }
        public void Clear()
        {
            items.Clear();
        }
        public Boolean Contains(T item)
        {
            return items.Contains(item);
        }
        public void CopyTo(T[] array, int index)
        {
            items.CopyTo(array, index);
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}