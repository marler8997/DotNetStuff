using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Marler.NetworkTools
{
    public interface ISerializableData
    {
        /// <summary>
        /// A class has a fixed serialization length if and only if it's serialization length is always the same
        /// irregardless of the data it is serializing.
        /// </summary>
        /// <returns>-1 if serialization length is not fixed, otherwise it returns its fixed serialization length</returns>
        Int32 GetFixedSerializationLength();

        Int32 SerializationLength();
        Int32 Serialize(Byte[] array, Int32 offset);

        Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset);

        String ToNiceString();
        void ToNiceString(StringBuilder builder);
        String ToNiceSmallString();
    }
    public class VoidSerializableData : ISerializableData
    {
        private static VoidSerializableData instance = null;
        public static VoidSerializableData Instance
        {
            get
            {
                if (instance == null) instance = new VoidSerializableData();
                return instance;
            }
        }
        private VoidSerializableData() { }
        public Int32 GetFixedSerializationLength() { return 0; }

        public Int32 SerializationLength() { return 0; }
        public Int32 Serialize(Byte[] array, Int32 offset) { return offset; }

        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset) { return offset; }

        public String ToNiceString() { return "<void>"; }
        public void ToNiceString(StringBuilder builder) { builder.Append("<void>"); }
        public String ToNiceSmallString() { return "<void>"; }
    }
    public class PartialByteArraySerializable : ISerializableData
    {
        private static PartialByteArraySerializable nullInstance = null;
        public static PartialByteArraySerializable Null
        {
            get
            {
                if (nullInstance == null) nullInstance = new PartialByteArraySerializable(null, 0, 0);
                return nullInstance;
            }
        }

        public Byte[] bytes;
        public Int32 offset, length;
        public PartialByteArraySerializable()
        {
        }
        public PartialByteArraySerializable(Byte[] bytes, Int32 offset, Int32 length)
        {
            this.bytes = bytes;
            this.offset = offset;
            this.length = length;
            if (offset + length > bytes.Length) throw new ArgumentOutOfRangeException();
        }
        public Int32 GetFixedSerializationLength()
        {
            return -1; // length is 
        }
        public Int32 SerializationLength()
        {
            if (bytes == null) return 0;
            return length;
        }
        public Int32 Serialize(Byte[] array, Int32 offset)
        {
            if (this.bytes == null) return offset;
            Array.Copy(this.bytes, this.offset, array, offset, this.length);
            return offset + this.length;
        }
        public Int32 Deserialize(Byte[] array, Int32 offset, Int32 maxOffset)
        {
            Int32 length = maxOffset - offset;

            if (length <= 0)
            {
                this.bytes = null;
                this.offset = 0;
                this.length = 0;
                return offset;
            }

            this.bytes = new Byte[length];
            Array.Copy(array, offset, this.bytes, 0, length);

            this.offset = 0;
            this.length = length;

            return offset + length;
        }
        public String ToNiceString()
        {
            return (bytes == null) ? "<null>" : BitConverter.ToString(bytes, offset, length);
        }
        public void ToNiceString(StringBuilder builder)
        {
            builder.Append(ToNiceString());
        }
        public String ToNiceSmallString()
        {
            return (bytes == null) ? "<null>" :( (bytes.Length <= 10) ?
                BitConverter.ToString(bytes) :String.Format("[{0} bytes]", length) );
        }
    }
}
