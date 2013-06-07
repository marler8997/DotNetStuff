using System;
using System.Collections.Generic;

namespace More
{
    public class UniqueIndexObjectDictionary<ObjectType>
    {
        public interface IObjectGenerator
        {
            ObjectType GenerateObject(Int32 uniqueIndex);
        }

        ObjectType[] objects;
        Int32 nextIndex;

        readonly Int32 extendLength;

        readonly SortedList<Int32> sortedFreeIndices;
        readonly Dictionary<ObjectType, Int32> objectToIndexDictionary;

        public UniqueIndexObjectDictionary(Int32 initialFreeStackCapacity, Int32 freeStackExtendLength,
            Int32 initialTotalObjectsCapacity, Int32 extendLength, IEqualityComparer<ObjectType> objectComparer)
        {
            this.objects = new ObjectType[initialTotalObjectsCapacity];
            nextIndex = 0;

            this.extendLength = extendLength;

            this.sortedFreeIndices = new SortedList<Int32>(initialFreeStackCapacity, freeStackExtendLength, Int32IncreasingComparer.Instance);
            this.objectToIndexDictionary = new Dictionary<ObjectType, Int32>(objectComparer);
        }
        public UniqueIndexObjectDictionary(Int32 initialFreeStackCapacity, Int32 freeStackExtendLength,
            Int32 initialTotalObjectsCapacity, Int32 extendLength)
        {
            this.objects = new ObjectType[initialTotalObjectsCapacity];
            nextIndex = 0;

            this.extendLength = extendLength;

            this.sortedFreeIndices = new SortedList<Int32>(initialFreeStackCapacity, freeStackExtendLength, Int32IncreasingComparer.Instance);
            this.objectToIndexDictionary = new Dictionary<ObjectType, Int32>();
        }
        private Int32 GetFreeUniqueIndex()
        {
            if (sortedFreeIndices.count > 0) return sortedFreeIndices.GetAndRemoveLastElement();

            if (nextIndex >= Int32.MaxValue)
                throw new InvalidOperationException(String.Format("The Free Stack Unique Object Tracker is tracking too many objects: {0}", nextIndex));

            // Make sure the local path buffer is big enough
            if (nextIndex >= objects.Length)
            {
                // extend local path array
                ObjectType[] newObjectsArray = new ObjectType[objects.Length + extendLength];
                Array.Copy(objects, newObjectsArray, objects.Length);
                objects = newObjectsArray;
            }

            Int32 newestObjectIndex = nextIndex;
            nextIndex++;
            return newestObjectIndex;
        }
        public Int32 GetUniqueIndexOf(ObjectType obj)
        {
            Int32 uniqueIndex;
            if (objectToIndexDictionary.TryGetValue(obj, out uniqueIndex)) return uniqueIndex;

            uniqueIndex = GetFreeUniqueIndex();
            objects[uniqueIndex] = obj;
            objectToIndexDictionary.Add(obj, uniqueIndex);

            return uniqueIndex;
        }
        public ObjectType GetObject(Int32 uniqueIndex)
        {
            return objects[uniqueIndex];
        }
        public ObjectType GenerateNewObject(out Int32 uniqueIndex, IObjectGenerator objectGenerator)
        {
            uniqueIndex = GetFreeUniqueIndex();

            ObjectType newObject = objectGenerator.GenerateObject(uniqueIndex);
            objects[uniqueIndex] = newObject;
            objectToIndexDictionary.Add(newObject, uniqueIndex);

            return newObject;
        }
        public void Free(Int32 uniqueIndex)
        {
            ObjectType obj = objects[uniqueIndex];
            objectToIndexDictionary.Remove(obj);

            if (uniqueIndex == nextIndex - 1)
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
                sortedFreeIndices.Add(uniqueIndex);
            }
        }
    }
}
