using System;
using System.Collections.Generic;
using System.Text;

using Marler.Net.Nfs3Procedure;

namespace Marler.Net
{
    public interface IPermissions
    {
        ModeFlags GetPermissions(ShareObject shareObject);
    }

    public class DumbPermissions : IPermissions
    {
        public readonly ModeFlags defaultDirectoryPermissions;
        public readonly ModeFlags defaultFilePermissions;

        public DumbPermissions(Nfs3Procedure.ModeFlags defaultDirectoryPermissions,
            ModeFlags defaultFilePermissions)
        {
            this.defaultDirectoryPermissions = defaultDirectoryPermissions;
            this.defaultFilePermissions = defaultFilePermissions;
        }
        public ModeFlags GetPermissions(ShareObject shareObject)
        {
            return (shareObject.fileType == Nfs3Procedure.FileType.Directory) ? defaultDirectoryPermissions :
                defaultFilePermissions;
        }
    }
}
