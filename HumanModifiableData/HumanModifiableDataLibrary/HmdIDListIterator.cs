using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Hmd
{
    public static class HmdIDListIterator
    {
        public delegate void AtValueID(HmdValueID valueID);
        public delegate void AtBlockID(HmdBlockID blockID);

        public static void Iterate(this HmdBlockID blockID, AtValueID atValueID, AtBlockID atBlockID)
        {
            for(int i = 0; i < blockID.ChildCount; i++)
            {
                HmdID id = blockID.GetChild(i);
                if (id.isBlock)
                {
                    atBlockID((HmdBlockID)id);              
                }
                else
                {
                    atValueID((HmdValueID)id);
                }
            }
        }

        public delegate void AtValueIDWithProperties(HmdValueID valueID, HmdValueIDProperties valueIDProperties, HmdProperties hmdProperties);
        public delegate void AtBlockIDWithProperties(HmdBlockID blockID, HmdBlockIDProperties blockIDProperties, HmdProperties hmdProperties);

        public static void Iterate(this HmdBlockID blockID, HmdProperties hmdProperties,
            AtValueIDWithProperties atValueIDWithProperties, AtBlockIDWithProperties atBlockIDWithProperties)
        {
            for (int i = 0; i < blockID.ChildCount; i++)
            {
                HmdID childID = blockID.GetChild(i);
                if (childID.isBlock)
                {
                    HmdBlockID childBlockID = childID.CastAsBlockID;
                    HmdBlockIDProperties childBlockIDProperties = hmdProperties.GetProperties(childBlockID);
                    if (childBlockIDProperties == null)
                    {
                        throw new InvalidOperationException(String.Format("Found a block id \"{0}\", but it was not defined in the property dictionary", childID.idOriginalCase));
                    }
                    atBlockIDWithProperties(childBlockID, childBlockIDProperties, hmdProperties);
                }
                else
                {
                    HmdValueID childValueID = childID.CastAsValueID;
                    HmdValueIDProperties childValueIDProperties = hmdProperties.GetProperties(childValueID);
                    if (childValueIDProperties == null)
                    {
                        throw new InvalidOperationException(String.Format("Found a value id \"{0}\", but it was not defined in the property dictionary", childID.idOriginalCase));
                    }
                    atValueIDWithProperties(childValueID, childValueIDProperties, hmdProperties);
                }
            }
        }

    }
}
