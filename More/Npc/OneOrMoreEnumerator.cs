using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    public class OneOrMoreEnumerator<T> : IEnumerator<T>
    {
        private Byte state;

        private readonly T first;
        private readonly IList<T> rest;

        public OneOrMoreEnumerator(T first, IList<T> rest)
        {
            this.state = 0;
            this.first = first;
            this.rest = rest;
        }
        public T Current
        {
            get
            {
                switch (state)
                {
                    case 0:
                        throw new InvalidOperationException("Must call MoveNext() before Current");
                    case 1:
                        return first;
                }

                return rest[state - 2];
            }
        }
        Object System.Collections.IEnumerator.Current
        {
            get
            {
                switch (state)
                {
                    case 0:
                        throw new InvalidOperationException("Must call MoveNext() before Current");
                    case 1:
                        return first;
                }

                return rest[state - 2];
            }
        }
        public void Dispose()
        {
        }
        public Boolean MoveNext()
        {
            switch (state)
            {
                case 0:
                    state++;
                    return true;
                case 1:
                    state++;
                    return (rest != null && rest.Count > 0);
            }

            state++;
            return state - 2 < rest.Count;
        }
        public void Reset()
        {
            state = 0;
        }
    }
}
