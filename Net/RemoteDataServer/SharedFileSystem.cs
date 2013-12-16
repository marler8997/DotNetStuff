using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using ObjectID = System.UInt32;
//using ObjectID = System.UInt64;

namespace More
{
    public class RootShareDirectory
    {
        public readonly DirectoryInfo directoryInfo;
        public readonly DriveInfo driveInfo;
        public readonly DirectoryShareObject shareObject;

        public RootShareDirectory(DirectoryShareObject shareObject)
        {
            //if(shareName.Contains("/"))
            //    throw new ArgumentException(String.Format("The share name you provided '{0}' is not valid (cannot have '/')", shareName));

            if (!Directory.Exists(shareObject.localPathAndName)) throw new InvalidOperationException(String.Format(
                 "Directory '{0}' does not exist", shareObject.localPathAndName));

            this.directoryInfo = new DirectoryInfo(shareObject.localPathAndName);
            this.driveInfo = new DriveInfo(directoryInfo.Root.FullName);
            this.shareObject = shareObject;
        }
        public override String ToString()
        {
            return (shareObject == null) ?
                String.Format("LocalDirectory '{0}' ShareName '{1}'", shareObject.localPathAndName, shareObject.name) :
                shareObject.ToString();
        }
    }
    public class SharedFileSystem
    {
        readonly UniqueIndexObjectDictionary<ShareObject> shareObjects;
        //public readonly IPermissions permissions;

        public readonly RootShareDirectory[] rootShareDirectories;
        public readonly DirectoryShareObject[] rootShareDirectoryObjects;

        private readonly Dictionary<String, ShareObject> shareObjectsByLocalPath;

        public SharedFileSystem(UniqueIndexObjectDictionary<ShareObject> shareObjects, /*IPermissions permissions, */RootShareDirectory[] rootShareDirectories)
        {
            this.shareObjects = shareObjects;
            //this.permissions = permissions;

            this.rootShareDirectories = rootShareDirectories;

            shareObjectsByLocalPath = new Dictionary<String, ShareObject>();


            this.rootShareDirectoryObjects = new DirectoryShareObject[rootShareDirectories.Length];
            for (int i = 0; i < rootShareDirectoryObjects.Length; i++)
            {
                RootShareDirectory rootShareDirectory = rootShareDirectories[i];
                rootShareDirectoryObjects[i] = new DirectoryShareObject(
                    rootShareDirectory.shareObject.localPathAndName,
                    rootShareDirectory.shareObject.name,
                    rootShareDirectory.shareObject.permissions);
            }

            /*
            for (int i = 0; i < rootShareDirectories.Length; i++)
            {
                RootShareDirectory rootShareDirectory = rootShareDirectories[i];
                ShareObject shareObject = CreateNewShareObject(FileType.Directory, rootShareDirectory.localShareDirectory, rootShareDirectory.shareName);
                if (shareObject == null)
                {
                    throw new DirectoryNotFoundException(String.Format(
                        "You are trying to share local directory '{0}', but it either does not exist or is not a directory", rootShareDirectory.localShareDirectory));
                }
                rootShareDirectory.shareObject = shareObject;
            }
            */
        }

        public ShareObject[] CreateArrayOfShareObjects()
        {
            ShareObject[] shareObjects = new ShareObject[shareObjectsByLocalPath.Count];
            int i = 0;
            foreach (KeyValuePair<String, ShareObject> pair in shareObjectsByLocalPath)
            {
                shareObjects[i] = pair.Value;
                i++;
            }
            return shareObjects;
        }
        /*
        public Nfs3Procedure.Status TryGetShareDirectory(Byte[] handle, out RootShareDirectory rootShareDirectory, out ShareObject shareDirectoryObject)
        {
            ShareObject shareObject;
            Nfs3Procedure.Status status = TryGetSharedObject(handle, out shareObject);
            if (status != Nfs3Procedure.Status.Ok) { outputShareDirectory = null; return status; }
            if (shareObject == null) { outputShareDirectory = null; return Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory; }

            //
            // Check that the share object is a root share directory
            //
            for (int i = 0; i < shareDirectories.Length; i++)
            {
                ShareDirectory shareDirectory = shareDirectories[i];
                if (shareDirectory.shareObject == shareObject)
                {
                    outputShareDirectory = shareDirectory;
                    return Nfs3Procedure.Status.Ok;
                }
            }

            outputShareDirectory = null;
            return Nfs3Procedure.Status.ErrorBadHandle;
        }
        */

        /*
        private ShareObject CreateNewShareObject(ObjectType type, String localPathAndName, String shareName)
        {
            ObjectID newFileID;
            Byte[] newFileHandle = filesDictionary.NewFileHandle(out newFileID);

            ShareObject shareObject = new ShareObject(fileType, newFileID, newFileHandle, localPathAndName, shareName);
            shareObjectsByLocalPath.Add(shareObject.localPathAndName, shareObject);
            shareObjectsByHandle.Add(shareObject.fileHandleBytes, shareObject);

            if (NfsServerLog.sharedFileSystemLogger != null)
                NfsServerLog.sharedFileSystemLogger.WriteLine("[SharedFileSystem] New Share Object: {0}", shareObject);
            return shareObject;
        }
        private void DisposeShareObject(ShareObject shareObject)
        {
            if (NfsServerLog.sharedFileSystemLogger != null)
                NfsServerLog.sharedFileSystemLogger.WriteLine("[SharedFileSystem] Disposing Share Object: {0}", shareObject);

            filesDictionary.Dispose(shareObject.fileID);
            shareObjectsByLocalPath.Remove(shareObject.localPathAndName);
            shareObjectsByHandle.Remove(shareObject.fileHandleBytes);
        }
        public void UpdateShareObjectPathAndName(ShareObject shareObject, String newLocalPathAndName, String newName)
        {
            // Dispose share object at new location
            ShareObject overwriteShareObject;
            if (shareObjectsByLocalPath.TryGetValue(newLocalPathAndName, out overwriteShareObject))
            {
                DisposeShareObject(overwriteShareObject);
            }

            // Update share object with new location
            String oldLocalPathAndName = shareObject.localPathAndName;
            shareObjectsByLocalPath.Remove(shareObject.localPathAndName);

            shareObject.UpdatePathAndName(newLocalPathAndName, newName);
            shareObjectsByLocalPath.Add(newLocalPathAndName, shareObject);

            if (NfsServerLog.sharedFileSystemLogger != null)
                NfsServerLog.sharedFileSystemLogger.WriteLine("[SharedFileSystem] Updated Share Object: '{0}' to '{1}'", oldLocalPathAndName, newLocalPathAndName);
        }

        //
        // Returns true on success, false if object did not exist
        public Boolean RemoveFileOrDirectory(String parentDirectory, String name)
        {
            String localPathAndName = Path.Combine(parentDirectory, name);

            ShareObject shareObject;
            if (shareObjectsByLocalPath.TryGetValue(localPathAndName, out shareObject))
            {
                DisposeShareObject(shareObject);
            }

            if (File.Exists(localPathAndName))
            {
                File.Delete(localPathAndName);
                return true;
            }

            if (Directory.Exists(localPathAndName))
            {
                Directory.Delete(localPathAndName);
                return true;
            }

            return false;
        }
        */
 





        /*
        public Nfs3Procedure.Status Move(ShareObject oldParentShareObject, String oldName,
            ShareObject newParentShareObject, String newName)
        {
            Nfs3Procedure.Status status;

            status = newParentShareObject.CheckStatus();
            if (status != Nfs3Procedure.Status.Ok)
            {
                DisposeShareObject(newParentShareObject);
                return status;
            }

            status = oldParentShareObject.CheckStatus();
            if (status != Nfs3Procedure.Status.Ok)
            {
                DisposeShareObject(oldParentShareObject);
                return status;
            }

            //
            // Get Old Share Object
            //
            String oldLocalPathAndName = NfsPath.LocalCombine(oldParentShareObject.localPathAndName, oldName);

            ShareObject oldShareObject;
            if (!shareObjectsByLocalPath.TryGetValue(oldLocalPathAndName, out oldShareObject))
                return Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory;

            status = oldShareObject.CheckStatus();
            if (status != Nfs3Procedure.Status.Ok)
            {
                DisposeShareObject(oldShareObject);
                return status;
            }

            //
            // Move
            //
            String newLocalPathAndName = NfsPath.LocalCombine(newParentShareObject.localPathAndName, newName);
            FileType fileType = oldShareObject.fileType;

            if (Directory.Exists(newLocalPathAndName))
            {
                if (oldShareObject.fileType != FileType.Directory)
                    return Nfs3Procedure.Status.ErrorAlreadyExists;

                try
                {
                    Directory.Delete(newLocalPathAndName);
                }
                catch (IOException)
                {
                    return Nfs3Procedure.Status.ErrorDirectoryNotEmpty; // The directory is not empty
                }

                Directory.Move(oldLocalPathAndName, newLocalPathAndName);
            }
            else if (File.Exists(newLocalPathAndName))
            {
                if (oldShareObject.fileType != FileType.Regular)
                    return Nfs3Procedure.Status.ErrorAlreadyExists;

                File.Delete(newLocalPathAndName);

                File.Move(oldLocalPathAndName, newLocalPathAndName);
            }
            else
            {
                if (oldShareObject.fileType == FileType.Regular)
                {
                    File.Move(oldLocalPathAndName, newLocalPathAndName);
                }
                else if (oldShareObject.fileType == FileType.Directory)
                {
                    Directory.Move(oldLocalPathAndName, newLocalPathAndName);
                }
                else
                {
                    return Nfs3Procedure.Status.ErrorInvalidArgument;
                }
            }

            //
            // Update the share object and return
            //
            UpdateShareObjectPathAndName(oldShareObject, newLocalPathAndName, newName);
            oldShareObject.RefreshFileAttributes(permissions);
            status = oldShareObject.CheckStatus();
            if (status != Nfs3Procedure.Status.Ok)
            {
                DisposeShareObject(oldShareObject);
            }
            return status;
        }

        public Nfs3Procedure.Status TryGetSharedObject(Byte[] handle, out ShareObject shareObject)
        {
            if (!shareObjectsByHandle.TryGetValue(handle, out shareObject))
            {
                if (NfsServerLog.sharedFileSystemLogger != null)
                    NfsServerLog.sharedFileSystemLogger.WriteLine("[SharedFileSystem] [Warning] File handle not found in dictionary: {0}", BitConverter.ToString(handle));
                return Nfs3Procedure.Status.ErrorBadHandle;
            }

            Nfs3Procedure.Status status = shareObject.CheckStatus();
            if (status != Nfs3Procedure.Status.Ok) DisposeShareObject(shareObject);
            return status;
        }




        public Nfs3Procedure.Status TryGetDirectory(String shareDirectoryName, out RootShareDirectory rootShareDirectory, out ShareObject shareDirectoryObject)
        {
            String subPath;
            String rootShareName = NfsPath.SplitShareNameAndSubPath(shareDirectoryName, out subPath);
            if (rootShareName == null)
            {
                rootShareDirectory = null;
                shareDirectoryObject = null;
                return Nfs3Procedure.Status.ErrorInvalidArgument;
            }

            Nfs3Procedure.Status status = TryGetRootSharedDirectory(rootShareName, out rootShareDirectory);
            if (status != Nfs3Procedure.Status.Ok) { shareDirectoryObject = null; return status; }
            if (rootShareDirectory == null) { shareDirectoryObject = null; return Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory; }

            if (subPath == null)
            {
                shareDirectoryObject = rootShareDirectory.shareObject;
            }
            else
            {
                String localPathAndName = NfsPath.LocalCombine(rootShareDirectory.localShareDirectory, subPath);

                status = TryGetSharedObject(localPathAndName, subPath, out shareDirectoryObject);
                if (status != Nfs3Procedure.Status.Ok) return status;
                if (shareDirectoryObject == null) return Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory;

                shareDirectoryObject.RefreshFileAttributes(permissions);
            }
            return Nfs3Procedure.Status.Ok;
        }

        //Nfs3Procedure.Status TryGetShareObject(String sharePathAndName, out RootShareDirectory rootShareDirectory, out ShareObject share)




        Nfs3Procedure.Status TryGetRootSharedDirectory(String rootShareName, out RootShareDirectory rootShareDirectory)
        {
            if (rootShareName[0] == '/')
            {
                rootShareName = rootShareName.Substring(1);
            }
            if (rootShareName.Contains("/"))
            {
                rootShareDirectory = null;
                return Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory;
            }

            for (int i = 0; i < rootShareDirectories.Length; i++)
            {
                rootShareDirectory = rootShareDirectories[i];
                if (rootShareName.Equals(rootShareDirectory.shareName))
                {
                    Nfs3Procedure.Status status = rootShareDirectory.shareObject.CheckStatus();
                    if (status != Nfs3Procedure.Status.Ok)
                        throw new InvalidOperationException(String.Format("The root share directory [{0}] has become invalid (status={1})", rootShareDirectory, status));
                    return Nfs3Procedure.Status.Ok;
                }
            }

            rootShareDirectory = null;
            return Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory;
        }

        public Nfs3Procedure.Status TryGetRootSharedDirectory(Byte[] handle, out RootShareDirectory rootShareDirectory)
        {
            ShareObject shareObject;
            Nfs3Procedure.Status status = TryGetSharedObject(handle, out shareObject);
            if (status != Nfs3Procedure.Status.Ok) { rootShareDirectory = null; return status; }

            for (int i = 0; i < rootShareDirectories.Length; i++)
            {
                rootShareDirectory = rootShareDirectories[i];
                if (shareObject == rootShareDirectory.shareObject || shareObject.localPathAndName.StartsWith(rootShareDirectory.localShareDirectory))
                {
                    return Nfs3Procedure.Status.Ok;
                }
            }
            rootShareDirectory = null;
            return Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory;
        }


        public Nfs3Procedure.Status TryGetSharedObject(String localPathAndName, out ShareObject shareObject)
        {
            return TryGetSharedObject(localPathAndName, NfsPath.LeafName(localPathAndName), out shareObject);
        }
        public Nfs3Procedure.Status TryGetSharedObject(String localPathAndName, String shareName, out ShareObject shareObject)
        {
            if (shareObjectsByLocalPath.TryGetValue(localPathAndName, out shareObject))
            {
                Nfs3Procedure.Status status = shareObject.CheckStatus();
                if (status != Nfs3Procedure.Status.Ok) DisposeShareObject(shareObject);
                return status;
            }
            else
            {
                if (File.Exists(localPathAndName))
                {
                    shareObject = CreateNewShareObject(FileType.Regular, localPathAndName, shareName);
                    return Nfs3Procedure.Status.Ok;
                }
                else if (Directory.Exists(localPathAndName))
                {
                    shareObject = CreateNewShareObject(FileType.Directory, localPathAndName, shareName);
                    return Nfs3Procedure.Status.Ok;
                }
                else
                {
                    return Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory;
                }
            }
        }
        public ShareObject TryGetSharedObject(FileType expectedFileType, String localParentDirectory, String localPathAndName)
        {
            switch (expectedFileType)
            {
                case FileType.Regular:
                    if (!File.Exists(localPathAndName)) return null;
                    break;
                case FileType.Directory:
                    if (!Directory.Exists(localPathAndName)) return null;
                    break;
                default:
                    return null;
            }

            ShareObject shareObject;
            if (shareObjectsByLocalPath.TryGetValue(localPathAndName, out shareObject))
            {
                if (shareObject.fileType == expectedFileType) return shareObject;
                DisposeShareObject(shareObject);
            }

            String shareName = NfsPath.LocalPathDiff(localParentDirectory, localPathAndName);
            if (!NfsPath.IsValidUnixFileName(shareName))
                throw new InvalidOperationException(String.Format("The file you supplied '{0}' is not a valid unix file name", shareName));

            return CreateNewShareObject(expectedFileType, localPathAndName, shareName);
        }
        */
    }
}
