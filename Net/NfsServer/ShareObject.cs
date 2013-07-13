using System;
using System.IO;

using More;

namespace More.Net
{
    public class ShareObject
    {
        public readonly Nfs3Procedure.FileType fileType;
        public readonly UInt64 fileID;
        public readonly UInt64 cookie;

        public String localPathAndName;
        public String shareName;
        private FileInfo fileInfo;

        public readonly Byte[] fileHandleBytes;
        public readonly Nfs3Procedure.OptionalFileHandle optionalFileHandleClass;

        public readonly Nfs3Procedure.FileAttributes fileAttributes;
        public readonly Nfs3Procedure.OptionalFileAttributes optionalFileAttributes;

        public ShareObject(Nfs3Procedure.FileType fileType, UInt64 fileID, Byte[] fileHandleBytes, String localPathAndName, String shareName)
        {
            this.fileType = fileType;
            this.fileID = fileID;
            this.cookie = (fileID == 0) ? UInt64.MaxValue : fileID; // A cookie value of 0 is not valid

            this.fileHandleBytes = fileHandleBytes;
            this.optionalFileHandleClass = new Nfs3Procedure.OptionalFileHandle(fileHandleBytes);

            this.localPathAndName = localPathAndName;
            this.shareName = shareName;
            this.fileInfo = null;

            if (!NfsPath.IsValidUnixFileName(shareName))
                throw new InvalidOperationException(String.Format("The file you supplied '{0}' is not a valid unix file name", shareName));

            this.fileAttributes = new Nfs3Procedure.FileAttributes();
            this.fileAttributes.fileType = fileType;
            this.fileAttributes.fileID = fileID;
            this.fileAttributes.fileSystemID = 0;
            if (fileType != Nfs3Procedure.FileType.Regular)
            {
                this.fileAttributes.fileSize = 0;
                this.fileAttributes.diskSize = 0;
            }
            this.fileAttributes.lastAccessTime                      = new Nfs3Procedure.Time();
            this.fileAttributes.lastModifyTime                      = new Nfs3Procedure.Time();
            this.fileAttributes.lastAttributeModifyTime             = new Nfs3Procedure.Time();

            if (fileType == Nfs3Procedure.FileType.Directory)
            {
                this.fileAttributes.fileSize = 4096;
                this.fileAttributes.diskSize = 4096;
            }

            this.optionalFileAttributes = new Nfs3Procedure.OptionalFileAttributes(fileAttributes);
        }
        public void UpdatePathAndName(String localPathAndName, String shareName)
        {
            this.localPathAndName = localPathAndName;
            this.shareName = shareName;
            this.fileInfo = null;
            if (!NfsPath.IsValidUnixFileName(shareName))
                throw new InvalidOperationException(String.Format("The file you supplied '{0}' is not a valid unix file name", shareName));
        }
        public Nfs3Procedure.Status CheckStatus()
        {
            switch (fileType)
            {
                case Nfs3Procedure.FileType.Regular:
                    if (!File.Exists(localPathAndName)) return Nfs3Procedure.Status.ErrorStaleFileHandle;
                    break;
                case Nfs3Procedure.FileType.Directory:
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
            Nfs3Procedure.ModeFlags newPermissions = permissions.GetPermissions(this);
            if (newPermissions != fileAttributes.protectionMode)
            {
                attributesChanged = true;
                fileAttributes.protectionMode = newPermissions;
            }

            fileAttributes.hardLinks = (fileType == Nfs3Procedure.FileType.Directory) ?
                2U : 1U;

            fileAttributes.ownerUid = 0;
            fileAttributes.gid = 0;

            if (fileType == Nfs3Procedure.FileType.Regular)
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
}
