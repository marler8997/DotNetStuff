using System;
using More.Net.Nfs3Procedure;

namespace More.Net
{
#if !WindowsCE
    [NpcInterface]
#endif
    public interface INfs3ServerNiceInterface
    {
        String[] ShareNames();
        ShareObject[] ShareObjects();
        FSInfoReply FSInfoByName(String shareName);
        NonRecursiveReadDirPlusReply ReadDirPlus(String directoryName, UInt64 cookie, UInt32 maxDirectoryInfoBytes);
    }
    
    public class NonRecursiveEntryPlus
    {
        public UInt64 fileID;
        public String fileName;
        public UInt64 cookie;
        public OptionalFileAttributes optionalAttributes;
        public OptionalFileHandle optionalHandle;
        public NonRecursiveEntryPlus(EntryPlus entry)
        {
            this.fileID = entry.fileID;
            this.fileName = entry.fileName;
            this.cookie = entry.cookie;
            this.optionalAttributes = entry.optionalAttributes;
            this.optionalHandle = entry.optionalHandle;
        }
    }
    public class NonRecursiveReadDirPlusReply
    {
        public Status status;
        public OptionalFileAttributes optionalDirectoryAttributes;
        public Byte[] cookieVerifier;
        NonRecursiveEntryPlus[] entries;
        public Boolean endOfEntries;

        public NonRecursiveReadDirPlusReply(ReadDirPlusReply reply)
        {
            this.status = reply.status;
            this.optionalDirectoryAttributes = reply.optionalDirectoryAttributes;
            this.cookieVerifier = reply.cookieVerifier;
            
            // Count reply entries
            UInt32 replyCount = 0;
            for(EntryPlus entry = reply.entry; entry != null; entry = entry.NextEntry)
            {
                replyCount++;
            }

            this.entries = new NonRecursiveEntryPlus[replyCount];
            UInt32 i = 0;
            for (EntryPlus entry = reply.entry; entry != null; entry = entry.NextEntry)
            {
                this.entries[i] = new NonRecursiveEntryPlus(entry);
                i++;
            }

            this.endOfEntries = reply.endOfEntries;
        }
    }

}