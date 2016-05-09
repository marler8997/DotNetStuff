using System;
using System.Collections.Generic;

namespace More
{
    public struct Priority
    {
        public const UInt32 HighestValue = UInt32.MaxValue;

        public static readonly Priority Highest = new Priority(HighestValue);
        public static readonly Priority SecondHighest = new Priority(HighestValue - 1);
        public static readonly Priority Ignore = new Priority(0);

        public readonly UInt32 value;
        public Priority(UInt32 value)
        {
            this.value = value;
        }
        public Boolean IsHighest { get { return value == HighestValue; } }
        public Boolean IsIgnore { get { return value == 0; } }
    }
    public struct PriorityValue<T>
    {
        public readonly Priority priority;
        public readonly T value;
        public PriorityValue(Priority priority, T value)
        {
            this.priority = priority;
            this.value = value;
        }
    }
    /// <summary>
    /// The higher the value, the higher the priority.
    /// A value of PriorityLogic.HighestPriority means to immediately stop
    /// checking values and return the one given.
    /// 
    /// A value of Ignore means the value should be ignored.
    /// </summary>
    /// <param name="address">Address to query for priority.</param>
    /// <returns>Priority of the given value (higher value is higher priority)</returns>
    public delegate Priority PriorityQuery<T>(T value);
    public static class PriorityExtensions
    {
        public static PriorityValue<T> PrioritySelect<T>(this IEnumerable<T> values, PriorityQuery<T> priorityQuery)
        {
            PriorityValue<T> highestPriorityValue = new PriorityValue<T>(Priority.Ignore, default(T));

            foreach (var value in values)
            {
                Priority priority = priorityQuery(value);
                if (priority.IsHighest)
                {
                    return new PriorityValue<T>(priority, value);
                }
                if (priority.value > highestPriorityValue.priority.value)
                {
                    highestPriorityValue = new PriorityValue<T>(priority, value);
                }
            }
            return highestPriorityValue;
        }
    }
}