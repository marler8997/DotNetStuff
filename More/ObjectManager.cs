using System;
using System.Collections.Generic;

namespace More
{
    public class ObjectManager<ObjectType> : Dictionary<ObjectType, Int32>
    {
        public interface IObjectFactory
        {
            ObjectType GenerateObject();
        }

        readonly IObjectFactory factory;
        readonly Int32 extendLength;

        ObjectType[] objects;
        Int32 nextIndex;

        readonly SortedList<Int32> sortedFreeIndices;
        readonly Dictionary<ObjectType, Int32> objectToIndexDictionary;

        public ObjectManager(IObjectFactory factory, Int32 initialCapacity, Int32 extendLength)
        {
            if (extendLength <= 0) throw new ArgumentOutOfRangeException("Extend length cannot be less than 1");

            this.factory = factory;
            this.extendLength = extendLength;

            this.objects = new ObjectType[initialCapacity];
            nextIndex = 0;

            this.sortedFreeIndices = new SortedList<Int32>(initialCapacity, extendLength, Int32IncreasingComparer.Instance);
            this.objectToIndexDictionary = new Dictionary<ObjectType, Int32>();
        }
        public Boolean ThereExistsAllocatedObjectsThatAreFree()
        {
            return sortedFreeIndices.count > 0;
        }
        public Int32 AllocatedObjectsCount()
        {
            return nextIndex;
        }
        public Int32 ReservedObjectsCount()
        {
            return nextIndex - sortedFreeIndices.count;
        }
        public ObjectType Reserve()
        {
            if (sortedFreeIndices.count > 0)
            {
                Int32 index = sortedFreeIndices.GetAndRemoveLastElement();
                return objects[index];
            }
            if (nextIndex >= Int32.MaxValue)
                throw new InvalidOperationException(String.Format("The Free Stack Unique Object Tracker is tracking too many objects: {0}", nextIndex));

            if (nextIndex >= objects.Length)
            {
                // extend local path array
                ObjectType[] newObjectsArray = new ObjectType[objects.Length + extendLength];
                Array.Copy(objects, newObjectsArray, objects.Length);
                objects = newObjectsArray;
            }

            Int32 newestObjectIndex = nextIndex;
            nextIndex++;

            ObjectType newestObject = factory.GenerateObject();
            objects[newestObjectIndex] = newestObject;
            Add(newestObject, newestObjectIndex);

            return newestObject;

        }
        public void Release(ObjectType obj)
        {
            Int32 index;
            if (!TryGetValue(obj, out index))
                throw new InvalidOperationException(String.Format("Object '{0}' was not found", obj));

            if (index == nextIndex - 1)
            {
                while (true)
                {
                    nextIndex--;
                    if (nextIndex <= 0) break;
                    if (sortedFreeIndices.count <= 0) break;
                    if (sortedFreeIndices.elements[sortedFreeIndices.count - 1] != nextIndex - 1) break;
                    sortedFreeIndices.count--;
                }
            }
            else
            {
                sortedFreeIndices.Add(index);
            }
        }
    }
}
