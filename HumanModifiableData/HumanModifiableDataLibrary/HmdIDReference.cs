using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Hmd
{
    public interface HmdIDReference
    {
        String IDOriginalCase { get; }
        String IDLowerCase { get; }
        HmdIDProperties TryToGetReference();
    }
    
    public interface HmdParentReference : HmdIDReference
    {
        HmdBlockIDProperties TryToGetReferenceAsBlock();
    }

    public class HmdParentReferenceByString : HmdParentReference
    {
        public readonly String idOriginalCase;
        public readonly String idLowerCase;

        public HmdParentReferenceByString(String id) { this.idOriginalCase = id; this.idLowerCase = id.ToLower(); }

        String HmdIDReference.IDOriginalCase { get { return idOriginalCase; } }
        String HmdIDReference.IDLowerCase { get { return idLowerCase; } }
        HmdIDProperties HmdIDReference.TryToGetReference() { return null; }

        HmdBlockIDProperties HmdParentReference.TryToGetReferenceAsBlock() { return null; }
    }
    
}
