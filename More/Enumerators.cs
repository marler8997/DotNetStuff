using System;
using System.Collections;
using System.Collections.Generic;

namespace More
{
    public static class EnumeratorExtensions
    {
        public static IEnumerator<T> GetArrayEnumerator<T>(this T[] array)
        {
            if (array == null) return EmptyEnumerator<T>.Instance;
            return new ArrayEnumerator<T>(array);
        }
    }
    public class EmptyEnumerator<T> : IEnumerator<T>
    {
        private static EmptyEnumerator<T> instance = null;
        public static EmptyEnumerator<T> Instance
        {
            get
            {
                if (instance == null) instance = new EmptyEnumerator<T>();
                return instance;
            }
        }
        private EmptyEnumerator() { }
        public T Current { get { throw new InvalidOperationException(); } }
        public void Dispose() { throw new NotImplementedException(); }
        Object IEnumerator.Current { get { throw new InvalidOperationException(); } }
        public bool MoveNext() { return false; }
        public void Reset() { }
    }
    public class SingleObjectEnumerator<T> : IEnumerator<T>
    {
        T obj;
        Boolean done;
        public SingleObjectEnumerator(T obj)
        {
            this.obj = obj;
            this.done = false;
        }
        public T Current { get { return obj; } }
        Object IEnumerator.Current { get { return obj; } }
        public Boolean MoveNext() { return !done; }
        public void Reset() { this.done = false; }
        public void Dispose() { }
    }
    public class SingleObjectList<T> : IList<T> where T : class
    {
        public T obj;
        public SingleObjectList()
        {
        }
        public SingleObjectList(T obj)
        {
            this.obj = obj;
        }
        public void SetObject(T obj)
        {
            this.obj = obj;
        }
        public Int32 IndexOf(T item)
        {
            return (item == obj) ? 0 : -1;
        }
        public void Insert(Int32 index, T item)
        {
            if (index != 0) throw new ArgumentOutOfRangeException("index");
            obj = item;
        }
        public void RemoveAt(Int32 index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException("index");
            this.obj = null;
        }
        public T this[int index]
        {
            get
            {
                if (index != 0) throw new ArgumentOutOfRangeException("index");
                return obj;
            }
            set
            {
                if (index != 0) throw new ArgumentOutOfRangeException("index");
                this.obj = value;
            }
        }
        public void Add(T item)
        {
            if (this.obj != null) throw new InvalidOperationException("This list only holds one object");
            this.obj = item;
        }
        public void Clear()
        {
            this.obj = null;
        }
        public Boolean Contains(T item)
        {
            return this.obj != null && this.obj == item;
        }
        public void CopyTo(T[] array, Int32 arrayIndex)
        {
            if (this.obj == null) return;
            array[arrayIndex] = this.obj;
        }
        public Int32 Count { get { return (this.obj == null) ? 0 : 1; } }
        public Boolean IsReadOnly { get { return false; } }

        public bool Remove(T item)
        {
            if (this.obj == item)
            {
                this.obj = null;
                return true;
            }
            return false;
        }
        public IEnumerator<T> GetEnumerator()
        {
            if (obj == null) return EmptyEnumerator<T>.Instance;
            return new SingleObjectEnumerator<T>(this.obj);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (obj == null) return EmptyEnumerator<T>.Instance;
            return new SingleObjectEnumerator<T>(this.obj);
        }
    }
    public class SingleObjectList : IList
    {
        public Object obj;
        public SingleObjectList() { }
        public SingleObjectList(Object obj) { this.obj = obj; }
        public void SetObject(Object obj)   { this.obj = obj; }
        public int Add(object value)
        {
            if (this.obj != null) throw new InvalidOperationException("This list only holds one object");
            this.obj = value;
            return 0;
        }
        public void Clear() { this.obj = null; }
        public Boolean Contains(object value)
        {
            return this.obj != null && this.obj == value;
        }
        public int IndexOf(object value)
        {
            return (value == obj) ? 0 : -1;
        }
        public void Insert(int index, object value)
        {
            if (index != 0) throw new ArgumentOutOfRangeException("index");
            obj = value;
        }
        public bool IsFixedSize { get { return true; } }
        public bool IsReadOnly { get { return false; } }
        public void Remove(object value)
        {
            if (this.obj == value)
            {
                this.obj = null;
            }
        }
        public void RemoveAt(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException("index");
            this.obj = null;
        }
        public object this[int index]
        {
            get
            {
                if (index != 0) throw new ArgumentOutOfRangeException("index");
                return obj;
            }
            set
            {
                if (index != 0) throw new ArgumentOutOfRangeException("index");
                this.obj = value;
            }
        }
        public void CopyTo(Array array, int index)
        {
            if (this.obj == null) return;
            array.SetValue(this.obj, index);
        }
        public int Count
        {
            get { return (this.obj == null) ? 0 : 1; }
        }
        public bool IsSynchronized
        {
            get { return false; }
        }
        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }
        public IEnumerator GetEnumerator()
        {
            if (obj == null) return EmptyEnumerator<Object>.Instance;
            return new SingleObjectEnumerator<Object>(this.obj);
        }
    }
    public class ArrayEnumerator<T> : IEnumerator<T>
    {
        readonly T[] array;
        Int32 state;
        public ArrayEnumerator(T[] array)
        {
            this.array = (array == null) ? new T[0] : array;
            this.state = -1;
        }
        public T Current
        {
            get { return array[state]; }
        }
        object IEnumerator.Current
        {
            get { return array[state]; }
        }
        public bool MoveNext()
        {
            state++;
            return state < array.Length;
        }
        public void Dispose()
        {
        }
        public void Reset()
        {
            state = -1;
        }
    }
    public class RollingArray<T> : IEnumerable<T>
    {
        public readonly T[] array;
        public readonly UInt32 startIndex;
        public RollingArray(T[] array, UInt32 startIndex)
        {
            this.array = array;
            this.startIndex = startIndex;
        }
        public T[] CreateNewArray(UInt32 length)
        {
            if (length == 0) return null;
            if (length > (UInt32)array.Length) throw new ArgumentOutOfRangeException("length",
                String.Format("The length you provided {0} cannot exceed the array length of {1}", length, array.Length));

            T[] newArray = new T[length];
            IEnumerator<T> enumerator = GetEnumerator();

            for(int i = 0; i < length; i++)
            {
                enumerator.MoveNext();
                newArray[i] = enumerator.Current;
            }

            return newArray;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new RollingArrayEnumerator<T>(array, startIndex);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException("The non-generic GetEnumerator method is not supported");
        }
    }
    public class RollingArrayReversed<T> : IEnumerable<T>
    {
        public readonly T[] array;
        public readonly UInt32 startIndex;
        public RollingArrayReversed(T[] array, UInt32 startIndex)
        {
            this.array = array;
            this.startIndex = startIndex;
        }
        public T[] CreateNewArray(UInt32 length)
        {
            if (length == 0) return null;
            if (length > (UInt32)array.Length) throw new ArgumentOutOfRangeException("length",
                String.Format("The length you provided {0} cannot exceed the array length of {1}", length, array.Length));

            T[] newArray = new T[length];
            IEnumerator<T> enumerator = GetEnumerator();

            for (int i = 0; i < length; i++)
            {
                enumerator.MoveNext();
                newArray[i] = enumerator.Current;
            }

            return newArray;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new RollingArrayReverseEnumerator<T>(array, startIndex);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException("The non-generic GetEnumerator method is not supported");
        }
    }

    // Used to loop through all the elements in an array starting at any index
    public class RollingArrayEnumerator<T> : IEnumerator<T>
    {
        readonly T[] array;
        public readonly UInt32 startIndex;
        UInt32 currentIndex;
        Boolean done;

        public RollingArrayEnumerator(T[] array, UInt32 startIndex)
        {
            this.array = array;
            this.startIndex = startIndex;
            if (startIndex >= array.Length) throw new InvalidOperationException(String.Format(
                 "The start index {0} is not less thant he array length {1}", startIndex, array.Length));

            this.currentIndex = UInt32.MaxValue;
            this.done = false;
        }
        public void Reset()
        {
            this.currentIndex = UInt32.MaxValue;
            this.done = false;
        }
        public T Current
        {
            get
            {
                if (this.currentIndex == UInt32.MaxValue)
                    throw new InvalidOperationException("Have not called MoveNext()");
                return array[currentIndex];
            }
        }
        public Boolean MoveNext()
        {
            if (currentIndex == UInt32.MaxValue)
            {
                currentIndex = startIndex;
                return true;
            }
            if (done)
            {
                throw new InvalidOperationException(
                "You have called MoveNext twice after the enumerator has already finished");
            }

            currentIndex++;

            //
            // Roll the index
            //
            if (currentIndex >= (UInt32)array.Length) currentIndex = 0;

            //
            // Check if the enumerator is finished
            //
            if (currentIndex == startIndex)
            {
                done = true;
            }
            return !done;
        }
        public void Dispose()
        {
        }
        object System.Collections.IEnumerator.Current
        {
            get { throw new NotSupportedException("The non-generic Current property is not supported"); }
        }
    }
    // Used to loop through all the elements in an array starting at any index
    public class RollingArrayReverseEnumerator<T> : IEnumerator<T>
    {
        readonly T[] array;
        public readonly UInt32 startIndex;
        UInt32 currentIndex;
        Boolean done;

        public RollingArrayReverseEnumerator(T[] array, UInt32 startIndex)
        {
            this.array = array;
            this.startIndex = startIndex;
            if (startIndex >= array.Length) throw new InvalidOperationException(String.Format(
                 "The start index {0} is not less thant he array length {1}", startIndex, array.Length));

            this.currentIndex = UInt32.MaxValue;
            this.done = false;
        }
        public void Reset()
        {
            this.currentIndex = UInt32.MaxValue;
            this.done = false;
        }
        public T Current
        {
            get
            {
                if (this.currentIndex == UInt32.MaxValue)
                    throw new InvalidOperationException("Have not called MoveNext()");
                return array[currentIndex];
            }
        }
        public Boolean MoveNext()
        {
            if (currentIndex == UInt32.MaxValue)
            {
                currentIndex = startIndex;
                return true;
            }
            if (done)
            {
                throw new InvalidOperationException(
                "You have called MoveNext twice after the enumerator has already finished");
            }

            // Decrement the index and roll it
            if (currentIndex == 0)
            {
                currentIndex = (UInt32)array.Length - 1;
            }
            else
            {
                currentIndex--;
            }

            //
            // Check if the enumerator is finished
            //
            if (currentIndex == startIndex)
            {
                done = true;
            }
            return !done;
        }
        public void Dispose()
        {
        }
        object System.Collections.IEnumerator.Current
        {
            get { throw new NotSupportedException("The non-generic Current property is not supported"); }
        }
    }
}
