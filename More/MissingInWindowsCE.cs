﻿
#if WindowsCE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace System
{
    public static class CEMissingTypeExtensions
    {
        /*
        public static Type MakeArrayType(this Type elementtype)
        {
            
        }
        */
    }
    public static class MissingInCEEnumReflection
    {
        public static String[] GetNames(Type enumType)
        {
            List<String> names = new List<String>();
            foreach (FieldInfo fieldInfo in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                names.Add(fieldInfo.Name);
            }
            return names.ToArray();
        }
        public static Array GetValues(Type enumType)
        {
            ArrayBuilder builder = new ArrayBuilder(enumType);
            foreach (FieldInfo fieldInfo in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                builder.Add(fieldInfo.GetValue(null));
            }
            return builder.Build();
        }
    }
    public static class MissingInCEString
    {
        public static String ToLowerInvariant(this String str)
        {
            return str.ToLower(System.Globalization.CultureInfo.InvariantCulture);
        }
        public static String Remove(this String str, Int32 index)
        {
            return str.Substring(0, index);
        }
        public static String[] Split(this String str, Char[] chars, Int32 maxStrings)
        {
            String[] strings = str.Split(chars);
            if (strings == null) return strings;

            if (strings.Length <= maxStrings) return strings;

            String[] newStrings = new String[maxStrings];
            Array.Copy(strings, newStrings, maxStrings);
            return newStrings;
        }
    }
}
namespace System.Net
{
    public static class MissingInCEIPParser
    {
        public static Boolean TryParse(String ipString, out IPAddress address)
        {
            try
            {
                address = IPAddress.Parse(ipString);
                return true;
            }
            catch (FormatException)
            {
                address = null;
                return false;
            }
        }
    }
}
namespace System.Collections.Generic
{
    public class HashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        Dictionary<T, Boolean> dictionary;

        public HashSet()
        {
            this.dictionary = new Dictionary<T, bool>();
        }

        public void Add(T item)
        {
            dictionary.Add(item, false);
        }
        public void Clear()
        {
            dictionary.Clear();
        }
        public bool Contains(T item)
        {
            return dictionary.ContainsKey(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            dictionary.Keys.CopyTo(array, arrayIndex);
        }
        public int Count
        {
            get { return dictionary.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(T item)
        {
            return dictionary.Remove(item);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }
    }
}
namespace System.Runtime.Serialization
{
    public sealed class FormatterServices
    {
        public static object GetUninitializedObject(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
namespace System.IO
{
    public enum DriveType
    {
        // Summary:
        //     The type of drive is unknown.
        Unknown = 0,
        //
        // Summary:
        //     The drive does not have a root directory.
        NoRootDirectory = 1,
        //
        // Summary:
        //     The drive is a removable storage device, such as a floppy disk drive or a
        //     USB flash drive.
        Removable = 2,
        //
        // Summary:
        //     The drive is a fixed disk.
        Fixed = 3,
        //
        // Summary:
        //     The drive is a network drive.
        Network = 4,
        //
        // Summary:
        //     The drive is an optical disc device, such as a CD or DVD-ROM.
        CDRom = 5,
        //
        // Summary:
        //     The drive is a RAM disk.
        Ram = 6,
    }

    // Summary:
    //     Provides access to information on a drive.
    [Serializable]
    public sealed class DriveInfo
    {
        public readonly String driveName;

        // Summary:
        //     Provides access to information on the specified drive.
        //
        // Parameters:
        //   driveName:
        //     A valid drive path or drive letter. This can be either uppercase or lowercase,
        //     'a' to 'z'. A null value is not valid.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     The drive letter cannot be null.
        //
        //   System.ArgumentException:
        //     The first letter of driveName is not an uppercase or lowercase letter from
        //     'a' to 'z'.  -or- driveName does not refer to a valid drive.
        public DriveInfo(string driveName)
        {
            this.driveName = driveName;
        }

        // Summary:
        //     Indicates the amount of available free space on a drive.
        //
        // Returns:
        //     The amount of free space available on the drive, in bytes.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        public long AvailableFreeSpace { get { return 999999; } }
        //
        // Summary:
        //     Gets the name of the file system, such as NTFS or FAT32.
        //
        // Returns:
        //     The name of the file system on the specified drive.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        public string DriveFormat { get { return "Unknown"; } }
        //
        // Summary:
        //     Gets the drive type.
        //
        // Returns:
        //     One of the System.IO.DriveType values.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred.
        public DriveType DriveType { get { return DriveType.Unknown; } }
        //
        // Summary:
        //     Gets a value indicating whether a drive is ready.
        //
        // Returns:
        //     true if the drive is ready; false if the drive is not ready.
        public bool IsReady { get { return true; } }
        //
        // Summary:
        //     Gets the name of a drive.
        //
        // Returns:
        //     The name of the drive.
        public string Name { get { return driveName; } }
        //
        // Summary:
        //     Gets the root directory of a drive.
        //
        // Returns:
        //     A System.IO.DirectoryInfo object that contains the root directory of the
        //     drive.
        public DirectoryInfo RootDirectory { get { throw new NotSupportedException(); } }
        //
        // Summary:
        //     Gets the total amount of free space available on a drive.
        //
        // Returns:
        //     The total free space available on a drive, in bytes.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        public long TotalFreeSpace { get { return 999999; } }
        //
        // Summary:
        //     Gets the total size of storage space on a drive.
        //
        // Returns:
        //     The total size of the drive, in bytes.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        public long TotalSize { get { return 999999; } }
        //
        // Summary:
        //     Gets or sets the volume label of a drive.
        //
        // Returns:
        //     The volume label.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        //
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        //
        //   System.UnauthorizedAccessException:
        //     The volume label is being set on a network or CD-ROM drive.
        public string VolumeLabel { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

        // Summary:
        //     Retrieves the drive names of all logical drives on a computer.
        //
        // Returns:
        //     An array of type System.IO.DriveInfo that represents the logical drives on
        //     a computer.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     An I/O error occurred (for example, a disk error or a drive was not ready).
        //
        //   System.UnauthorizedAccessException:
        //     The caller does not have the required permission.
        public static DriveInfo[] GetDrives()
        {
            throw new NotSupportedException();
        }
        //
        // Summary:
        //     Returns a drive name as a string.
        //
        // Returns:
        //     The name of the drive.
        public override string ToString()
        {
            return driveName;
        }
    }
}
#endif