using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using Marler.Common;
using Marler.Net.Nfs3Procedure;

namespace Marler.Net
{
    public class Nfs3Server : RpcServerHandler
    {
        private readonly RpcServicesManager servicesManager;
        private readonly SharedFileSystem sharedFileSystem;
        private readonly PartialByteArraySerializer fileContents;
        private readonly UInt32 suggestedReadSizeMultiple;

        public Nfs3Server(RpcServicesManager servicesManager, SharedFileSystem sharedFileSystem, ByteBuffer sendBuffer,
            UInt32 readSizeMax, UInt32 suggestedReadSizeMultiple)
            : base("Nfs3", sendBuffer)
        {
            this.servicesManager = servicesManager;
            this.sharedFileSystem = sharedFileSystem;
            this.fileContents = new PartialByteArraySerializer(new Byte[readSizeMax], 0, 0);
            this.suggestedReadSizeMultiple = suggestedReadSizeMultiple;
        }
        public override Boolean ProgramHeaderSupported(RpcProgramHeader programHeader)
        {
            return programHeader.program == Nfs3.ProgramNumber && programHeader.programVersion == 3;
        }
        public override RpcReply Call(String clientString, RpcCall call, byte[] callParameters, int callOffset, int callMaxOffset, out ISerializer replyParameters)
        {
            ISerializer callData;
            replyParameters = VoidSerializer.Instance;

            Int64 beforeCall = Stopwatch.GetTimestamp();

            Boolean printCall = true;

            switch (call.procedure)
            {
                case (UInt32)Nfs3Command.NULL:
                    callData = VoidSerializer.Instance;
                    break;
                case (UInt32)Nfs3Command.GETATTR:

                    GetFileAttributesCall getFileAttributesCall = new GetFileAttributesCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(getFileAttributesCall);
                    callData = getFileAttributesCall;

                    break;
                case (UInt32)Nfs3Command.SETATTR:

                    SetFileAttributesCall setFileAttributesCall = new SetFileAttributesCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(setFileAttributesCall);
                    callData = setFileAttributesCall;

                    break;
                case (UInt32)Nfs3Command.LOOKUP:

                    LookupCall lookupCall = new LookupCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(lookupCall);
                    callData = lookupCall;

                    break;
                case (UInt32)Nfs3Command.ACCESS:

                    AccessCall accessCall = new AccessCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(accessCall);
                    callData = accessCall;

                    break;
                case (UInt32)Nfs3Command.READ:

                    ReadCall readCall = new ReadCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(readCall);
                    callData = readCall;

                    break;
                case (UInt32)Nfs3Command.WRITE:

                    WriteCall writeCall = new WriteCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(writeCall);
                    callData = writeCall;
                    printCall = false;

                    break;
                case (UInt32)Nfs3Command.CREATE:

                    CreateCall createCall = new CreateCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(createCall);
                    callData = createCall;

                    break;
                case (UInt32)Nfs3Command.MKDIR:

                    MkdirCall mkdirCall = new MkdirCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(mkdirCall);
                    callData = mkdirCall;

                    break;

                case (UInt32)Nfs3Command.SYMLINK:

                    SymLinkCall symLinkCall = new SymLinkCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(symLinkCall);
                    callData = symLinkCall;
                    
                    break;

                case (UInt32)Nfs3Command.REMOVE:
                case (UInt32)Nfs3Command.RMDIR:
                    
                    RemoveCall removeCall = new RemoveCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(removeCall);
                    callData = removeCall;
                    break;

                case (UInt32)Nfs3Command.RENAME:

                    RenameCall renameCall = new RenameCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(renameCall);
                    callData = renameCall;

                    break;
                case (UInt32)Nfs3Command.READDIRPLUS:

                    ReadDirPlusCall readDirPlusCall = new ReadDirPlusCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(readDirPlusCall);
                    callData = readDirPlusCall;

                    break;
                case (UInt32)Nfs3Command.FSINFO:

                    FsInfoCall getPortCall = new FsInfoCall(callParameters, callOffset, callMaxOffset);
                    replyParameters = Handle(getPortCall);
                    callData = getPortCall;

                    break;
                default:
                    if (NfsServerLog.warningLogger != null)
                        NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] client '{1}' sent unknown procedure number {2}", serviceName, clientString, call.procedure);
                    return new RpcReply(RpcVerifier.None, RpcAcceptStatus.ProcedureUnavailable);
            }

            Int64 afterCall = Stopwatch.GetTimestamp();
            Int64 callStopwatchTicks = afterCall - beforeCall;
            if (NfsServerLog.rpcCallLogger != null)
            {
                String callString = printCall ? callData.ToNiceSmallString() : callData.GetType().Name;
                NfsServerLog.rpcCallLogger.WriteLine("[{0}] Rpc {1} => {2} {3:0.00} milliseconds", serviceName, callString,
                    replyParameters.GetType().Name, callStopwatchTicks.StopwatchTicksAsDoubleMilliseconds());
            }
            if (NfsServerLog.storePerformance)
            {
                NfsServerLog.StoreNfsCallPerformance((Nfs3Command)call.procedure, (Int32)callStopwatchTicks.StopwatchTicksAsMicroseconds());
            }

            //servicesManager.PrintPerformance();
            return new RpcReply(RpcVerifier.None);
        }
        GetFileAttributesReply Handle(GetFileAttributesCall getFileAttributesCall)
        {
            ShareObject shareObject;
            Status status = sharedFileSystem.TryGetSharedObject(getFileAttributesCall.fileHandle, out shareObject);
            if (status != Status.Ok) return new GetFileAttributesReply(status);

            shareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            return new GetFileAttributesReply(shareObject.fileAttributes);
        }
        SetFileAttributesReply Handle(SetFileAttributesCall setFileAttributesCall)
        {
            ShareObject shareObject;
            Status status = sharedFileSystem.TryGetSharedObject(setFileAttributesCall.fileHandle, out shareObject);
            if (status != Status.Ok) return new SetFileAttributesReply(status, BeforeAndAfterAttributes.None);

            shareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            SizeAndTimes before = new SizeAndTimes(shareObject.fileAttributes);

            // TODO: change the permissions


            return new SetFileAttributesReply(Status.Ok, new BeforeAndAfterAttributes(before, shareObject.fileAttributes));
        }
        LookupReply Handle(LookupCall lookupCall)
        {
            //
            // Get Directory Object
            //
            ShareObject directoryShareObject;
            Status status = sharedFileSystem.TryGetSharedObject(lookupCall.directoryHandle, out directoryShareObject);            
            if (status != Status.Ok) return new LookupReply(status, OptionalFileAttributes.None);

            if (directoryShareObject.fileType != FileType.Directory) return new LookupReply(Status.ErrorNotDirectory, OptionalFileAttributes.None);

            //
            // Get File
            //
            String localPathAndName = NfsPath.LocalCombine(directoryShareObject.localPathAndName, lookupCall.fileName);
            ShareObject fileShareObject;
            sharedFileSystem.TryGetSharedObject(localPathAndName, lookupCall.fileName, out fileShareObject);

            if (status != Status.Ok) return new LookupReply(status, OptionalFileAttributes.None);
            if (fileShareObject == null) return new LookupReply(Status.ErrorNoSuchFileOrDirectory, OptionalFileAttributes.None);

            directoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            fileShareObject.RefreshFileAttributes(sharedFileSystem.permissions);

            return new LookupReply(fileShareObject.fileHandleBytes, directoryShareObject.optionalFileAttributes,
                fileShareObject.optionalFileAttributes);
        }
        AccessReply Handle(AccessCall accessCall)
        {
            ShareObject shareObject;
            Status status = sharedFileSystem.TryGetSharedObject(accessCall.fileHandle, out shareObject);
            if (status != Status.Ok) return new AccessReply(status, OptionalFileAttributes.None);

            //
            // For now just give every all permissions
            //
            return new AccessReply(OptionalFileAttributes.None,
                AccessFlags.Delete | AccessFlags.Execute | AccessFlags.Extend |
                AccessFlags.Lookup | AccessFlags.Modify | AccessFlags.Read);
        }
        ReadReply Handle(ReadCall readCall)
        {
            ShareObject shareObject;
            Status status = sharedFileSystem.TryGetSharedObject(readCall.fileHandle, out shareObject);
            if (status != Status.Ok) return new ReadReply(status, OptionalFileAttributes.None);

            if(shareObject.fileType != FileType.Regular) return new ReadReply(Status.ErrorInvalidArgument, OptionalFileAttributes.None);

            if (readCall.count > (UInt32)fileContents.bytes.Length) return new ReadReply(Status.ErrorInvalidArgument, OptionalFileAttributes.None);

            Boolean reachedEndOfFile;
            Int32 bytesRead = FileExtensions.ReadFile(shareObject.AccessFileInfo(), (Int32)readCall.offset, fileContents.bytes, FileShare.ReadWrite, out reachedEndOfFile);

            fileContents.length = bytesRead;
            return new ReadReply(OptionalFileAttributes.None, (UInt32)bytesRead, reachedEndOfFile, fileContents);
        }
        WriteReply Handle(WriteCall writeCall)
        {
            ShareObject shareObject;
            Status status = sharedFileSystem.TryGetSharedObject(writeCall.fileHandle, out shareObject);
            if (status != Status.Ok) return new WriteReply(status, BeforeAndAfterAttributes.None);

            if (shareObject.fileType != FileType.Regular) return new WriteReply(Status.ErrorInvalidArgument, BeforeAndAfterAttributes.None);

            shareObject.RefreshFileAttributes(sharedFileSystem.permissions);

            SizeAndTimes sizeAndTimesBeforeWrite = new SizeAndTimes(shareObject.fileAttributes);

            FileInfo fileInfo = shareObject.AccessFileInfo();

            using (FileStream fileStream = fileInfo.Open(FileMode.Open))
            {
                fileStream.Position = (Int64)writeCall.offset;
                fileStream.Write(writeCall.data, 0, writeCall.data.Length);
            }

            shareObject.RefreshFileAttributes(sharedFileSystem.permissions);

            return new WriteReply(new BeforeAndAfterAttributes(sizeAndTimesBeforeWrite, shareObject.fileAttributes),
                (UInt32)writeCall.data.Length, writeCall.stableHow, null);
        }
        CreateReply Handle(CreateCall createCall)
        {
            if(createCall.mode == CreateModeEnum.Exclusive)
                return new CreateReply(Status.ErrorNotSupported, BeforeAndAfterAttributes.None);

            ShareObject directoryShareObject;
            Status status = sharedFileSystem.TryGetSharedObject(createCall.directoryHandle, out directoryShareObject);
            if (status != Status.Ok) return new CreateReply(status, BeforeAndAfterAttributes.None);

            FileStream fileStream = null;
            try
            {
                String localPathAndName = NfsPath.LocalCombine(directoryShareObject.localPathAndName, createCall.newFileName);

                ShareObject fileShareObject;
                status = sharedFileSystem.TryGetSharedObject(localPathAndName, createCall.newFileName, out fileShareObject);

                if (status == Nfs3Procedure.Status.Ok)
                {
                    fileShareObject.RefreshFileAttributes(sharedFileSystem.permissions);

                    // The file already exists
                    if (createCall.mode == CreateModeEnum.Guarded)
                        return new CreateReply(Status.ErrorAlreadyExists, BeforeAndAfterAttributes.None);
                }
                else 
                {
                    if(status != Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory)
                        return new CreateReply(status, BeforeAndAfterAttributes.None);
                }

                directoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
                SizeAndTimes directorySizeAndTimesBeforeCreate = new SizeAndTimes(directoryShareObject.fileAttributes);

                // Todo: handle exceptions
                fileStream = new FileStream(localPathAndName, FileMode.Create);
                fileStream.Dispose();

                status = sharedFileSystem.TryGetSharedObject(localPathAndName, createCall.newFileName, out fileShareObject);
                if (status != Nfs3Procedure.Status.Ok) return new CreateReply(status, BeforeAndAfterAttributes.None);

                fileShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
                directoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);

                return new CreateReply(fileShareObject.optionalFileHandleClass,
                    fileShareObject.optionalFileAttributes,
                    new BeforeAndAfterAttributes(directorySizeAndTimesBeforeCreate, directoryShareObject.fileAttributes));
            }
            finally
            {
                if (fileStream != null) fileStream.Dispose();
            }
        }
        MkdirReply Handle(MkdirCall mkdirCall)
        {
            ShareObject parentDirectoryShareObject;
            Status status = sharedFileSystem.TryGetSharedObject(mkdirCall.directoryHandle, out parentDirectoryShareObject);
            if (status != Status.Ok) return new MkdirReply(status, BeforeAndAfterAttributes.None);

            String localPathAndName = NfsPath.LocalCombine(parentDirectoryShareObject.localPathAndName, mkdirCall.newDirectoryName);

            ShareObject mkdirDirectoryShareObject;
            status = sharedFileSystem.TryGetSharedObject(localPathAndName, mkdirCall.newDirectoryName, out mkdirDirectoryShareObject);
            if (status == Nfs3Procedure.Status.Ok) return new MkdirReply(Status.ErrorAlreadyExists, BeforeAndAfterAttributes.None);
            if (status != Nfs3Procedure.Status.ErrorNoSuchFileOrDirectory) return new MkdirReply(status, BeforeAndAfterAttributes.None);

            parentDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            SizeAndTimes directorySizeAndTimesBeforeCreate = new SizeAndTimes(parentDirectoryShareObject.fileAttributes);

            // Todo: handle exceptions
            Directory.CreateDirectory(localPathAndName);

            status = sharedFileSystem.TryGetSharedObject(localPathAndName, mkdirCall.newDirectoryName, out mkdirDirectoryShareObject);
            if (status != Nfs3Procedure.Status.Ok) return new MkdirReply(status, BeforeAndAfterAttributes.None);

            mkdirDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            parentDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);

            return new MkdirReply(mkdirDirectoryShareObject.optionalFileHandleClass,
                parentDirectoryShareObject.optionalFileAttributes,
                new BeforeAndAfterAttributes(directorySizeAndTimesBeforeCreate, parentDirectoryShareObject.fileAttributes));
        }
        private ISerializer Handle(SymLinkCall symLinkCall)
        {
            ShareObject shareObject;
            Status status = sharedFileSystem.TryGetSharedObject(symLinkCall.linkToHandle, out shareObject);
            if (status != Status.Ok) return new SymLinkReply(status, BeforeAndAfterAttributes.None);

            //
            // Todo: implement this, for now just return an error
            //
            return new SymLinkReply(Status.ErrorNotSupported, BeforeAndAfterAttributes.None);
        }
        RemoveReply Handle(RemoveCall removeCall)
        {
            ShareObject parentDirectoryShareObject;
            Status status = sharedFileSystem.TryGetSharedObject(removeCall.directoryHandle, out parentDirectoryShareObject);
            if (status != Status.Ok) return new RemoveReply(status, BeforeAndAfterAttributes.None);

            parentDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            SizeAndTimes directorySizeAndTimesBeforeCreate = new SizeAndTimes(parentDirectoryShareObject.fileAttributes);

            status = sharedFileSystem.RemoveFileOrDirectory(parentDirectoryShareObject.localPathAndName, removeCall.fileName);
            if (status != Status.Ok) return new RemoveReply(status, BeforeAndAfterAttributes.None);

            parentDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);

            return new RemoveReply(Status.Ok, new BeforeAndAfterAttributes(directorySizeAndTimesBeforeCreate,
                parentDirectoryShareObject.fileAttributes));
        }
        RenameReply Handle(RenameCall renameCall)
        {
            ShareObject oldParentDirectoryShareObject;
            ShareObject newParentDirectoryShareObject;

            Status status = sharedFileSystem.TryGetSharedObject(renameCall.oldDirectoryHandle, out oldParentDirectoryShareObject);
            if (status != Status.Ok) return new RenameReply(status, BeforeAndAfterAttributes.None, BeforeAndAfterAttributes.None);

            status = sharedFileSystem.TryGetSharedObject(renameCall.newDirectoryHandle, out newParentDirectoryShareObject);
            if (status != Status.Ok) return new RenameReply(status, BeforeAndAfterAttributes.None, BeforeAndAfterAttributes.None);


            oldParentDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            SizeAndTimes oldDirectorySizeAndTimesBeforeCreate = new SizeAndTimes(oldParentDirectoryShareObject.fileAttributes);

            newParentDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            SizeAndTimes newDirectorySizeAndTimesBeforeCreate = new SizeAndTimes(newParentDirectoryShareObject.fileAttributes);            
            
            status = sharedFileSystem.Move(oldParentDirectoryShareObject, renameCall.oldName,
                newParentDirectoryShareObject, renameCall.newName);
            if (status != Status.Ok) return new RenameReply(status, BeforeAndAfterAttributes.None, BeforeAndAfterAttributes.None);


            oldParentDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);
            newParentDirectoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);

            return new RenameReply(Status.Ok,
                new BeforeAndAfterAttributes(oldDirectorySizeAndTimesBeforeCreate, oldParentDirectoryShareObject.fileAttributes),
                new BeforeAndAfterAttributes(newDirectorySizeAndTimesBeforeCreate, newParentDirectoryShareObject.fileAttributes));
        }
        ReadDirPlusReply Handle(ReadDirPlusCall readDirPlusCall)
        {
            ShareObject directoryShareObject;
            Status status = sharedFileSystem.TryGetSharedObject(readDirPlusCall.directoryHandle, out directoryShareObject);
            if (status != Status.Ok) return new ReadDirPlusReply(status, OptionalFileAttributes.None);

            EntryPlus lastEntry = null;

            String [] localFiles = Directory.GetFiles(directoryShareObject.localPathAndName);
            if(localFiles != null)
            {
                for(int i = 0; i < localFiles.Length; i++)
                {
                    String localFile = localFiles[i];
                    ShareObject shareObject = sharedFileSystem.TryGetSharedObject(FileType.Regular, directoryShareObject.localPathAndName, localFile);
                    if(shareObject == null)
                    {
                        if(NfsServerLog.warningLogger != null)
                            NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] Could not create or access share object for local file '{1}'", serviceName, localFile);
                        continue;
                    }
                    shareObject.RefreshFileAttributes(sharedFileSystem.permissions);

                    lastEntry = new EntryPlus(
                        shareObject.fileID,
                        shareObject.shareName,
                        0,
                        OptionalFileAttributes.None,//shareObject.optionalFileAttributes,
                        shareObject.optionalFileHandleClass,
                        lastEntry);
                }
            }
            String[] localDirectories = Directory.GetDirectories(directoryShareObject.localPathAndName);
            if(localDirectories != null)
            {
                for(int i = 0; i < localDirectories.Length; i++)
                {
                    String localDirectory = localDirectories[i];
                    ShareObject shareObject = sharedFileSystem.TryGetSharedObject(FileType.Directory, directoryShareObject.localPathAndName, localDirectory);
                    if(shareObject == null)
                    {
                        if (NfsServerLog.warningLogger != null)
                            NfsServerLog.warningLogger.WriteLine("[{0}] [Warning] Could not create or access share object for local directory '{1}'", serviceName, localDirectory);
                        continue;
                    }
                    shareObject.RefreshFileAttributes(sharedFileSystem.permissions);

                    lastEntry = new EntryPlus(
                        shareObject.fileID,
                        shareObject.shareName,
                        0,
                        OptionalFileAttributes.None,//shareObject.optionalFileAttributes,
                        shareObject.optionalFileHandleClass,
                        lastEntry);
                }
            }
            // Get "." and ".."
            /*
            lastEntry = new EntryPlus(
                directoryShareObject.fileID,
                ".",
                0,
                directoryShareObject.optionalFileAttributes,
                directoryShareObject.optionalFileHandleClass,
                lastEntry);
            */
            directoryShareObject.RefreshFileAttributes(sharedFileSystem.permissions);

            return new ReadDirPlusReply(directoryShareObject.optionalFileAttributes, null, lastEntry, true);
        }
        FsInfoReply Handle(FsInfoCall getPortCall)
        {
            return new FsInfoReply(
                OptionalFileAttributes.None,
                (UInt32)fileContents.bytes.Length, (UInt32)fileContents.bytes.Length, suggestedReadSizeMultiple,
                0x10000, 0x10000, 0x1000,
                0x1000,
                0x10000000000,
                1,
                0,
                FileProperties.Fsf3Link | FileProperties.Fsf3SymLink | FileProperties.Fsf3Homogeneous | FileProperties.Fsf3CanSetTime
                );
        }
    }
}
