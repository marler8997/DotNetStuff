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
    /// <summary>
    /// A struct that contains one item, and a list of other items if there are any more.
    /// This is used for lists of items, where there will typically be 1 item per entry, but
    /// could have more then 1, and each entry will also never have 0 entries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct OneOrMore<T> : IList<T>
    {
        public T first;
        List<T> others;
        public OneOrMore(T first)
        {
            this.first = first;
            this.others = null;
        }
        public Int32 Count
        {
            get { return (others == null) ? 1 : 1 + others.Count; }
        }
        public void Add(T item)
        {
            if (others == null)
            {
                others = new List<T>();
            }
            others.Add(item);
        }
        public void Clear()
        {
            throw new NotSupportedException("Cannot clear an instance of OneOrMore, must always have at least one item");
        }
        public Boolean Contains(T item)
        {
            if (first.Equals(item))
            {
                return true;
            }
            return (others == null) ? false : others.Contains(item);
        }
        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            array[arrayIndex] = first;
            if (others != null)
            {
                others.CopyTo(array, arrayIndex + 1);
            }
        }
        public Boolean IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(T item)
        {
            if (item.Equals(first))
            {
                if (others == null || others.Count == 0)
                {
                    throw new InvalidOperationException("Cannot remove because this list must always have at least 1 item");
                }
                first = others[others.Count - 1];
                others.RemoveAt(others.Count - 1);
                return true;
            }
            return (others == null) ? false : others.Remove(item);
        }
        public IEnumerator<T> GetEnumerator()
        {
            yield return first;
            if (others != null)
            {
                for (int i = 0; i < others.Count; i++)
                {
                    yield return others[i];
                }
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public int IndexOf(T item)
        {
            if (item.Equals(first))
            {
                return 0;
            }
            return (others == null) ? -1 : others.IndexOf(item);
        }
        public T this[int index]
        {
            get
            {
                return (index == 0) ? first : others[index - 1];
            }
            set
            {
                if (index == 0)
                {
                    first = value;
                }
                else
                {
                    others[index - 1] = value;
                }
            }
        }
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }

}