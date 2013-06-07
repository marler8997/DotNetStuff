using System;
using System.Collections.Generic;
using System.Text;

namespace More.Net
{
    public class Int32IncreasingComparer : IComparer<Int32>
    {
        private static Int32IncreasingComparer instance = null;
        public static Int32IncreasingComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Int32IncreasingComparer();
                }
                return instance;
            }
        }
        private Int32IncreasingComparer() { }
        public Int32 Compare(Int32 x, Int32 y)
        {
            return (x > y) ? 1 : ((x < y) ? -1 : 0);
        }
    }
    public class Int32DecreasingComparer : IComparer<Int32>
    {
        private static Int32DecreasingComparer instance = null;
        public static Int32DecreasingComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Int32DecreasingComparer();
                }
                return instance;
            }
        }
        private Int32DecreasingComparer() {}
        public Int32 Compare(Int32 x, Int32 y)
        {
            return (x > y) ? -1 : ((x < y) ? 1 : 0);
        }
    }



    public class SortedList<ElementType>
    {
        public ElementType[] elements;
        public Int32 count;

        private readonly Int32 extendLength;

        private readonly IComparer<ElementType> comparer;

        public SortedList(Int32 initialCapacity, Int32 extendLength, IComparer<ElementType> comparer)
        {
            this.elements = new ElementType[initialCapacity];
            this.count = 0;

            this.extendLength = extendLength;

            this.comparer = comparer;
        }

        public void Clear()
        {
            // remove references if necessary
            if (typeof(ElementType).IsClass)
            {
                for (int i = 0; i < count; i++)
                {
                    this.elements[i] = default(ElementType);
                }
            }
            this.count = 0;
        }

        public void Add(ElementType newElement)
        {
            if (count >= elements.Length)
            {
                ElementType[] newElements = new ElementType[elements.Length + extendLength];
                Array.Copy(elements, newElements, elements.Length);
                elements = newElements;
            }

            Int32 position;
            for (position = 0; position < count; position++)
            {
                ElementType element = elements[position];
                if (comparer.Compare(newElement, element) <= 0)
                {
                    // Move remaining elements
                    for (Int32 copyPosition = count; copyPosition > position; copyPosition--)
                    {
                        elements[copyPosition] = elements[copyPosition - 1];
                    }
                    break;
                }
            }

            elements[position] = newElement;
            count++;
        }

        public ElementType GetAndRemoveLastElement()
        {
            count--;
            ElementType element = elements[count];

            elements[count] = default(ElementType); // Delete reference to this object

            return element;
        }

        public void Remove(ElementType element)
        {
            for (int i = 0; i < count; i++)
            {
                if (element.Equals(elements[i]))
                {
                    while (i < count - 1)
                    {
                        elements[i] = elements[i + 1];
                        i++;
                    }
                    count--;
                    return;
                }
            }
            throw new InvalidOperationException(String.Format("Element {0} was not in the list", element));
        }

        public void RemoveFromStart(Int32 count)
        {
            if (count <= 0) return;
            if (count >= this.count)
            {
                this.count = 0;
                return;
            }

            this.count -= count;
            for (int i = 0; i < this.count; i++)
            {
                elements[i] = elements[count + i];
            }
        }

    }
}
