using System;
using System.Collections;
using System.Collections.Generic;

namespace Marler.Common
{
    public class ArrayEnumerator<T> : IEnumerator<T>
    {
        readonly T[] array;
        Int32 state;
        public ArrayEnumerator(T[] array)
        {
            this.array = array;
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
}
