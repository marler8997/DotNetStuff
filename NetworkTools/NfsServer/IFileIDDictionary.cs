using System;
using System.Collections.Generic;
using System.Text;

using FileID = System.UInt64;

namespace Marler.NetworkTools
{
    /*
    public interface IFileHandleGenerator
    {
        Byte[] GenerateFileHandle(FileID fileID);
    }
    */
    public interface IFileIDsAndHandlesDictionary : IEqualityComparer<Byte[]>
    {
        FileID GetFileID(Byte[] fileHandle);
        Byte[] GetFileHandle(FileID fileID);
        Byte[] NewFileHandle(out FileID fileID);
        void Dispose(FileID fileID);
    }

    public class FreeStackFileIDDictionary : IFileIDsAndHandlesDictionary, UniqueIndexObjectDictionary<Byte[]>.IObjectGenerator
    {
        UniqueIndexObjectDictionary<Byte[]> fileHandles;

        public FreeStackFileIDDictionary(Int32 initialFreeStackCapacity, Int32 freeStackExtendLength, 
            Int32 initialFileHandleCapacity, Int32 fileHandleExtendLength)
        {
            this.fileHandles = new UniqueIndexObjectDictionary<Byte[]>(
                initialFreeStackCapacity, freeStackExtendLength,
                initialFileHandleCapacity, fileHandleExtendLength, this);
        }

        Boolean IEqualityComparer<byte[]>.Equals(Byte[] x, Byte[] y)
        {
            return x[0] == y[0] && x[1] == x[1] && x[2] == y[2] && x[3] == y[3];
        }

        Int32 IEqualityComparer<byte[]>.GetHashCode(Byte[] obj)
        {
            return
                (unchecked((Int32)0xFF000000) & (obj[0] << 24)) |
                (                 0x00FF0000  & (obj[1] << 16)) |
                (                 0x0000FF00  & (obj[2] <<  8)) |
                (                 0x000000FF  & (obj[3]      )) ;
        }

        Byte[] UniqueIndexObjectDictionary<Byte[]>.IObjectGenerator.GenerateObject(Int32 uniqueIndex)
        {
            return new Byte[] {
                (Byte)(uniqueIndex >> 24),
                (Byte)(uniqueIndex >> 16),
                (Byte)(uniqueIndex >>  8),
                (Byte)(uniqueIndex      ),
            };
        }

        public FileID GetFileID(Byte[] fileHandle)
        {
            return (UInt64)fileHandles.GetUniqueIndexOf(fileHandle);
        }
        public Byte[] GetFileHandle(FileID fileID)
        {
            return fileHandles.GetObject((Int32)fileID);
        }
        public Byte[] NewFileHandle(out UInt64 fileID)
        {
            Int32 newFileID;
            Byte[] newFileHandle = fileHandles.GenerateNewObject(out newFileID, this);
            fileID = (UInt64)newFileID;
            return newFileHandle;
        }
        public void Dispose(FileID fileID)
        {
            fileHandles.Free((Int32)fileID);
        }
    }
}
