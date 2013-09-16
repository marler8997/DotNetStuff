using System;
using System.Collections.Generic;

namespace More
{
    public interface ISimpleList<T>
    {
        UInt32 Count { get; }
        T this[UInt32 index] { get; }
        void Add(T value);
    }
    public class FixedSimpleList<T> : ISimpleList<T>
    {
        readonly T[] array;
        UInt32 nextIndex;
        public FixedSimpleList(UInt32 fixedLength)
        {
            this.array = new T[fixedLength];
        }
        public UInt32 Count
        {
            get { return nextIndex; }
        }
        public T this[UInt32 index]
        {
            get { return array[index]; }
        }
        public void Add(T value)
        {
            array[nextIndex++] = value;
        }
    }
    public class DynamicSimpleList<T> : ISimpleList<T>
    {
        readonly List<T> list;
        public DynamicSimpleList()
        {
            this.list = new List<T>();
        }
        public UInt32 Count
        {
            get { return (UInt32)list.Count; }
        }
        public T this[UInt32 index]
        {
            get { return list[(Int32)index]; }
        }
        public void Add(T value)
        {
            list.Add(value);
        }
    }
}
