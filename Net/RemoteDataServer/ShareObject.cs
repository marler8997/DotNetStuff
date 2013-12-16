using System;
using System.IO;

using ObjectID = System.UInt32;
//using ObjectID = System.UInt64;

namespace More
{
    public abstract class ShareObject
    {
        public readonly ObjectType type;
        public readonly String name;
        protected ShareObject(ObjectType type, String name)
        {
            this.name = name;
        }
    }
    public class FileShareObject : ShareObject
    {
        public readonly Permissions permissions;
        public readonly ObjectID id;
        public FileShareObject(String name, Permissions permissions, ObjectID id)
            : base(ObjectType.Data, name)
        {
            this.permissions = permissions;
            this.id = id;
        }
    }
    public class StreamShareObject : ShareObject
    {
        public readonly Permissions permissions;
        public readonly ObjectID id;
        public StreamShareObject(String name, Permissions permissions, ObjectID id)
            : base(ObjectType.Stream, name)
        {
            this.permissions = permissions;
            this.id = id;
        }
    }
    public class LinkShareObject : ShareObject
    {
        public readonly Permissions permissions;
        public LinkShareObject(String name, Permissions permissions)
            : base(ObjectType.Link, name)
        {
            this.permissions = permissions;
        }
    }
    public class DirectoryShareObject : ShareObject
    {
        public ObjectID id;
        public readonly String localPathAndName;
        public readonly Permissions permissions;

        public DirectoryShareObject(String localPathAndName, String name, Permissions permissions)
            : base(ObjectType.Directory, name)
        {
            this.localPathAndName = localPathAndName;
            this.permissions = permissions;
        }
    }



    /*
    public class ShareObject
    {
        public readonly ObjectType type;
        public readonly ObjectID id;

        public String localPathAndName;
        public String shareLeafName;
        private FileInfo fileInfo;

        public readonly Byte[] fileHandleBytes;
        public readonly Nfs3Procedure.OptionalFileHandle optionalFileHandleClass;

        public readonly FileAttributes fileAttributes;
        public readonly Nfs3Procedure.OptionalFileAttributes optionalFileAttributes;

        public ShareObject(FileType fileType, UInt64 fileID, Byte[] fileHandleBytes, String localPathAndName, String shareLeafName)
        {
            this.fileType = fileType;
            this.fileID = fileID;
            this.cookie = (fileID == 0) ? UInt64.MaxValue : fileID; // A cookie value of 0 is not valid

            this.fileHandleBytes = fileHandleBytes;
            this.optionalFileHandleClass = new Nfs3Procedure.OptionalFileHandle(fileHandleBytes);

            this.localPathAndName = localPathAndName;
            SetShareLeafName(shareLeafName);
            this.fileInfo = null;


            this.fileAttributes = new FileAttributes();
            this.fileAttributes.fileType = fileType;
            this.fileAttributes.fileID = fileID;
            this.fileAttributes.fileSystemID = 0;
            if (fileType != FileType.Regular)
            {
                this.fileAttributes.fileSize = 0;
                this.fileAttributes.diskSize = 0;
            }
            this.fileAttributes.lastAccessTime = new Time();
            this.fileAttributes.lastModifyTime = new Time();
            this.fileAttributes.lastAttributeModifyTime = new Time();

            if (fileType == FileType.Directory)
            {
                this.fileAttributes.fileSize = 4096;
                this.fileAttributes.diskSize = 4096;
            }

            this.optionalFileAttributes = new Nfs3Procedure.OptionalFileAttributes(fileAttributes);
        }
        public void UpdatePathAndName(String localPathAndName, String shareName)
        {
            this.localPathAndName = localPathAndName;
            SetShareLeafName(shareLeafName);
            this.fileInfo = null;
        }
        void SetShareLeafName(String shareLeafName)
        {
            if (NfsPath.IsValidUnixFileName(shareLeafName))
            {
                this.shareLeafName = shareLeafName;
            }
            else
            {
                String newShareLeafName = NfsPath.LeafName(shareLeafName);
                if (!NfsPath.IsValidUnixFileName(newShareLeafName))
                    throw new InvalidOperationException(String.Format("The file you supplied '{0}' is not a valid unix file name", shareLeafName));
                this.shareLeafName = newShareLeafName;
            }
        }

        public Nfs3Procedure.Status CheckStatus()
        {
            switch (fileType)
            {
                case FileType.Regular:
                    if (!File.Exists(localPathAndName)) return Nfs3Procedure.Status.ErrorStaleFileHandle;
                    break;
                case FileType.Directory:
                    if (!Directory.Exists(localPathAndName)) return Nfs3Procedure.Status.ErrorStaleFileHandle;
                    break;
            }
            return Nfs3Procedure.Status.Ok;
        }
        public FileInfo AccessFileInfo()
        {
            if (fileInfo == null) fileInfo = new FileInfo(localPathAndName);
            return fileInfo;
        }
        public Boolean RefreshFileAttributes(IPermissions permissions)
        {
            Boolean attributesChanged = false;

            if (fileInfo == null)
            {
                attributesChanged = true;
                fileInfo = new FileInfo(localPathAndName);
            }
            else
            {
                fileInfo.Refresh();
            }

            //
            // Update file attributes
            //
            ModeFlags newPermissions = permissions.GetPermissions(this);
            if (newPermissions != fileAttributes.protectionMode)
            {
                attributesChanged = true;
                fileAttributes.protectionMode = newPermissions;
            }

            fileAttributes.hardLinks = (fileType == FileType.Directory) ?
                2U : 1U;

            fileAttributes.ownerUid = 0;
            fileAttributes.gid = 0;

            if (fileType == FileType.Regular)
            {
                UInt64 newFileSize = (UInt64)fileInfo.Length;
                if (fileAttributes.fileSize != newFileSize)
                {
                    attributesChanged = true;
                    fileAttributes.fileSize = newFileSize;
                }
                fileAttributes.diskSize = fileAttributes.fileSize;
            }

            fileAttributes.specialData1 = 0;
            fileAttributes.specialData2 = 0;

            {
                DateTime lastAccessDateTime = fileInfo.LastAccessTime;
                UInt32 newLastAccessTimeSeconds = lastAccessDateTime.ToUniversalTime().ToUnixTime();
                if (fileAttributes.lastAccessTime.seconds != newLastAccessTimeSeconds)
                {
                    attributesChanged = true;
                    fileAttributes.lastAccessTime.seconds = newLastAccessTimeSeconds;
                }
            }
            {
                DateTime lastModifyTime = fileInfo.LastWriteTime;
                UInt32 newLastModifyTimeSeconds = lastModifyTime.ToUniversalTime().ToUnixTime();
                if (fileAttributes.lastModifyTime.seconds != newLastModifyTimeSeconds)
                {
                    attributesChanged = true;
                    fileAttributes.lastModifyTime.seconds = newLastModifyTimeSeconds;
                }
            }

            if (attributesChanged)
            {
                fileAttributes.lastAttributeModifyTime.seconds =
                    (fileAttributes.lastAccessTime.seconds > fileAttributes.lastModifyTime.seconds) ?
                    fileAttributes.lastAccessTime.seconds : fileAttributes.lastModifyTime.seconds;
            }

            return attributesChanged;
        }
        public override String ToString()
        {
            return String.Format("Type '{0}' ID '{1}' LocalPathAndName '{2}' Handle '{3}'",
                fileType, fileID, localPathAndName, BitConverter.ToString(fileHandleBytes));
        }
    }
    */
}
