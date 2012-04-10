using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Marler.Hmd
{
    public abstract class HmdIDProperties : HmdIDReference
    {
        public static String CombineIDContext(String context, String idLowerCase)
        {
            return (context == null || context.Length <= 0) ? idLowerCase : 
                String.Format("{0}.{1}", context, idLowerCase);
        }

        public readonly Boolean isBlock;

        public readonly String idOriginalCase;
        public readonly String idLowerCase;
        public readonly String definitionContext;
        public readonly String idWithContext;

        protected ICountProperty countProperty;
        public ICountProperty CountProperty { get { return countProperty; } }

        public readonly HmdBlockIDProperties directParent; // The parent as it appears when it is defined
                                                               // Note: The definitionParent of %root is null
        private HmdParentReference[] parentOverrideList;
        private List<HmdBlockIDProperties> additionalParents;

        public abstract HmdValueIDProperties CastAsValueIDProperties { get; }
        public abstract HmdBlockIDProperties CastAsBlockIDProperties { get; }

        protected HmdIDProperties(Boolean isBlock, String id, ICountProperty countProperty,
            HmdBlockIDProperties directParent, HmdParentReference[] parentOverrideList)
        {
            if (id == null)
            {
                throw new ArgumentNullException("idString");
            }
            
            this.idOriginalCase = id;
            this.idLowerCase = id.ToLower();
            if (directParent == null)
            {
                this.definitionContext = null;
            }
            else if (directParent.definitionContext == null)
            {
                this.definitionContext = String.Empty;
            }
            else
            {
                this.definitionContext = directParent.idWithContext;
            }
            this.idWithContext = CombineIDContext(this.definitionContext, idLowerCase);

            this.isBlock = isBlock;

            if (countProperty == null)
            {
                throw new ArgumentNullException("countProperty");
            }
            this.countProperty = countProperty;

            this.directParent = directParent;
            this.parentOverrideList = parentOverrideList;
            this.additionalParents = null;
        }

        String HmdIDReference.IDOriginalCase { get { return idOriginalCase; } }
        String HmdIDReference.IDLowerCase { get { return idLowerCase; } }
        HmdIDProperties HmdIDReference.TryToGetReference() { return this; }        

        public Boolean IsRoot
        {
            get { return (directParent == null); }
        }

        public Boolean DirectParentIsOverriden
        {
            get { return (parentOverrideList != null); }
        }

        public int ParentOverrideCount { get { return (parentOverrideList == null) ? 0 : parentOverrideList.Length; } }

        public void OverrideCountProperty(ICountProperty countProperty)
        {
            if (countProperty == null)
            {
                throw new ArgumentNullException("countProperty");
            }
            this.countProperty = countProperty;
        }


        public void SetParentOverrideList(HmdParentReference[] parentOverrideList)
        {
            // Can't override the root parent list
            if (IsRoot)
            {
                throw new InvalidOperationException("You can't override the parent list of the root!");
            }

            if (this.parentOverrideList != null)
            {
                throw new InvalidOperationException(String.Format("The {0} ID \"{1}\" already has a parent override list",
                    isBlock ? "Block" : "Value", idOriginalCase));
            }
            
            this.parentOverrideList = parentOverrideList;
        }

        public Boolean IsInParentOverrideList(String parent)
        {
            if (parentOverrideList != null)
            {
                for (int i = 0; i < parentOverrideList.Length; i++)
                {
                    if (parent.Equals(parentOverrideList[i].IDLowerCase, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Boolean IsValidParent(HmdBlockIDProperties parent)
        {
            if (parentOverrideList == null)
            {
                if (directParent == parent)
                {
                    return true;
                }
            }
            else
            {
                for (int i = 0; i < parentOverrideList.Length; i++)
                {
                    if (parent == parentOverrideList[i])
                    {
                        return true;
                    }
                    if (parent.idLowerCase.Equals(parentOverrideList[i].IDLowerCase, StringComparison.CurrentCulture))
                    {
                        parentOverrideList[i] = parent; // cache the reference
                        return true;
                    }
                }
            }

            if (additionalParents != null)
            {
                for (int i = 0; i < additionalParents.Count; i++)
                {
                    if (parent == additionalParents[i])
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        public void ResolveParentOverrideLinks(HmdProperties hmdProperties)
        {
            if (parentOverrideList != null)
            {
                TextWriter debugOutput = HmdDebug.DebugOutput;
                if (debugOutput == null) debugOutput = TextWriter.Null;

                for (int i = 0; i < parentOverrideList.Length; i++)
                {
                    //
                    // Get the parent reference
                    //
                    HmdBlockIDProperties parentBlock = parentOverrideList[i].TryToGetReferenceAsBlock();
                    if (parentBlock == null)
                    {
                        parentBlock = hmdProperties.GetParentPropertiesInScope(this, parentOverrideList[i]);
                        if (parentBlock == null)
                        {
                            throw new InvalidOperationException(String.Format("Parent \"{0}\" is not defined in the property dictionary",
                                parentOverrideList[i].IDOriginalCase));
                        }
                        debugOutput.WriteLine("For ID \"{0}\", resolved Parent reference to \"{1}\"", idOriginalCase, parentBlock.idOriginalCase);
                        parentOverrideList[i] = parentBlock; // cache the reference
                    }
                    //
                    // Make sure the parent has this child linked
                    //
                    parentBlock.AddIndirectChildFromFromItsOverrideList(this);
                    //
                    // Add this link as a key to the HmdPropertiesTable
                    //
                    hmdProperties.AddPropertiesFromExtraLinks(parentBlock, this);
                }
            }
        }

        public void AddAdditionalParentFromItsAdditionalChildrenList(HmdBlockIDProperties newParent)
        {
            throw new NotImplementedException();
            //need to figure out what kind of children can be added to a block id's AdditionalChildList

        }

        public String AllParentsString()
        {
            StringBuilder stringBuilder = null;

            if (directParent == null)
            {
                return "()";
            }
            if (parentOverrideList == null && additionalParents == null)
            {
                return String.Format("({0})", directParent.idOriginalCase);
            }

            if (parentOverrideList != null)
            {
                stringBuilder = new StringBuilder();
                stringBuilder.Append('(');

                Int32 i;
                for (i = 0; i < parentOverrideList.Length - 1; i++)
                {
                    stringBuilder.Append(parentOverrideList[i].IDOriginalCase);
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(parentOverrideList[i]);
            }
            if (additionalParents != null)
            {
                if (stringBuilder == null)
                {
                    stringBuilder = new StringBuilder();
                    stringBuilder.Append('(');
                }

                Int32 i;
                for (i = 0; i < additionalParents.Count - 1; i++)
                {
                    stringBuilder.Append(additionalParents[i].idOriginalCase);
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(additionalParents[i]);
            }
            if (stringBuilder == null)
            {
                return "()";
            }
            stringBuilder.Append(')');
            return stringBuilder.ToString();
        }

    }

    public class HmdBlockIDProperties : HmdIDProperties,HmdParentReference,IEnumerable<HmdIDProperties>
    {
        public const Boolean IsBlock = true;

        // List Conditions
        // 1. To be in directChildrenWithNoParentOverrideList
        //     - Defined inside the block
        //     - Does not have a Parent override list
        // 2. To be in additionalChildrenList
        //     - Inside the %props: [<child> <child> ...] list
        // 3. To be in indirectChildrenLinkedFromTheirParentOverrideList
        //     - This block is inside the childs (<parent> <parent> ...) override list
        // Note: double references are not allowed.  In other words, a parent cannot have a child in 
        //       the additionalChildList if the child has the parent in it's parentOverrideList
        public List<HmdIDProperties> directChildrenWithNoParentOverrideList;
        public HmdIDReference[] additionalChildrenList;
        public List<HmdIDProperties> indirectChildrenLinkedFromTheirParentOverrideList;

        public override HmdValueIDProperties CastAsValueIDProperties
        {
            get { throw new InvalidOperationException("You can't case an HmdBlockIDProperties class as an HmdValueIDProperties class"); }
        }
        public override HmdBlockIDProperties CastAsBlockIDProperties
        {
            get { return this; }
        }

        public HmdBlockIDProperties(String idString, ICountProperty countProperty, HmdBlockIDProperties definitionParent)
            : base(IsBlock, idString, countProperty, definitionParent, null)
        {
        }

        HmdBlockIDProperties HmdParentReference.TryToGetReferenceAsBlock()
        {
            return this;
        }

        public void SetAdditionalChildrenList(HmdIDReference[] newAdditionalChildrenList)
        {
            if (this.additionalChildrenList != null)
            {
                throw new InvalidOperationException(String.Format("The Block ID \"{0}\" already has an additional children list", idOriginalCase));
            }
            this.additionalChildrenList = newAdditionalChildrenList;
        }

        public Int32 TotalChildCount
        {
            get
            {
                return
                    ((directChildrenWithNoParentOverrideList == null)    ? 0 : directChildrenWithNoParentOverrideList.Count) +
                    ((indirectChildrenLinkedFromTheirParentOverrideList == null) ? 0 : indirectChildrenLinkedFromTheirParentOverrideList.Count) +
                    ((additionalChildrenList == null)                    ? 0 : additionalChildrenList.Length);
            }
        }
        
        public void AddDirectChildWithNoParentOverrideList(HmdIDProperties newDirectChild)
        {
            if(newDirectChild.directParent != this)
            {
                throw new InvalidOperationException(
                    String.Format("Hey, you tried to add the child \"{0}\" to the block \"{1}\" as a direct child, but the child's directParent does not match (\"{2}\")",
                    newDirectChild.idOriginalCase, idOriginalCase, newDirectChild.directParent.idOriginalCase));
            }
            if (newDirectChild.DirectParentIsOverriden)
            {
                throw new InvalidOperationException(
                    String.Format("Hey, you tried to add the child \"{0}\" to the block \"{1}\" as a direct child, but the child's directParent is overriden",
                    newDirectChild.idOriginalCase, idOriginalCase));
            }

            if (directChildrenWithNoParentOverrideList == null)
            {
                directChildrenWithNoParentOverrideList = new List<HmdIDProperties>();
            }
            else
            {
                // Check for duplicate definitions of newDirectChild
                for (int i = 0; i < directChildrenWithNoParentOverrideList.Count; i++)
                {
                    if (newDirectChild.idLowerCase.Equals(directChildrenWithNoParentOverrideList[i].idLowerCase, StringComparison.CurrentCultureIgnoreCase))
                    {
                        throw new FormatException(String.Format("Block \"{0}\" has multiple direct children with an id of \"{1}\"",
                            idOriginalCase, newDirectChild.idOriginalCase));
                    }
                }
            }
            directChildrenWithNoParentOverrideList.Add(newDirectChild);
        }

        public void AddIndirectChildFromFromItsOverrideList(HmdIDProperties newChild)
        {
            //
            // Check that this newChild does not conflict with any definitionChildren
            //
            if (directChildrenWithNoParentOverrideList != null)
            {
                for (int i = 0; i < directChildrenWithNoParentOverrideList.Count; i++)
                {
                    if (newChild.idLowerCase.Equals(directChildrenWithNoParentOverrideList[i].idLowerCase, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (newChild == directChildrenWithNoParentOverrideList[i])
                        {
                            throw new InvalidOperationException(String.Format("Internal code error: For some reason, you called the block \"{0}\"'s AddNewChildFromFromAnOverrideList from child \"{1}\", but, this child was already in the directChildrenWithNoParentOverrideList, but this should have never happened because only children with no ParentOverrideList should be in that list",
                                idOriginalCase, newChild.idOriginalCase));
                        }
                        throw new FormatException(String.Format("Block \"{0}\" has 2 children with the same id \"{1}\", one was defined directly, and the other has this block in it's Parent Override List",
                            idOriginalCase, newChild.idOriginalCase));
                    }
                }
            }

            //
            // Don't allow a Parent/Child to reference each other
            //
            if (additionalChildrenList != null)
            {
                for (int i = 0; i < additionalChildrenList.Length; i++)
                {
                    if (newChild.idLowerCase.Equals(additionalChildrenList[i].IDLowerCase, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (newChild == additionalChildrenList[i])
                        {
                            throw new FormatException(String.Format("Block \"{0}\" has a double reference to child \"{1}\".  The child is in the parent's AdditionalChildrenList and the parent is in the child's ParentOverrideList",
                                idOriginalCase, newChild.idOriginalCase));
                        }
                        throw new FormatException(String.Format("Block \"{0}\" has 2 children with the same id \"{1}\", one was defined in the AdditionalChildrenList, and is defined in the child's ParentOverrideList",
                            idOriginalCase, newChild.idOriginalCase));
                    }
                }

            }

            //
            // Check if this child is already in the DirectChild list
            //
            if (indirectChildrenLinkedFromTheirParentOverrideList == null)
            {
                indirectChildrenLinkedFromTheirParentOverrideList = new List<HmdIDProperties>();
            }
            else
            {
                // Check for multiple definitions of newAdditionalChild
                for (int i = 0; i < indirectChildrenLinkedFromTheirParentOverrideList.Count; i++)
                {
                    if (newChild.idLowerCase.Equals(indirectChildrenLinkedFromTheirParentOverrideList[i].idLowerCase, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Note: Here we should never get that newChild == childrenLinkedFromTheirParentOverrideList[i], if this happens, then maybe this parent was specified twice in the child's parentOverrideList...to fix this, maybe check for duplicates in the parentOverrideList
                        throw new FormatException(String.Format("Block \"{0}\" has multiple children with an id of \"{1}\", one is a direct child and the other is an indirect child",
                            idOriginalCase, newChild.idOriginalCase));
                    }
                }
            }
            indirectChildrenLinkedFromTheirParentOverrideList.Add(newChild);
        }

        public void ResolveAdditionalChildrenLinks(HmdProperties hmdProperties)
        {
            TextWriter debugOutput = HmdDebug.DebugOutput;
            if (debugOutput == null) debugOutput = TextWriter.Null;

            if (additionalChildrenList != null)
            {
                for (int i = 0; i < additionalChildrenList.Length; i++)
                {
                    HmdIDProperties childProperties = additionalChildrenList[i].TryToGetReference();
                    if (childProperties == null)
                    {
                        childProperties = hmdProperties.TryToGetChildInScope(this, additionalChildrenList[i]);
                        if (childProperties == null)
                        {
                            throw new InvalidOperationException(String.Format("Parent \"{0}\" is not defined in the property dictionary",
                                additionalChildrenList[i].IDOriginalCase));
                        }
                        debugOutput.WriteLine("For ID \"{0}\", resolved Parent->Child reference to \"{1}\"", idOriginalCase, childProperties.idOriginalCase);
                        additionalChildrenList[i] = childProperties; // cache the reference
                    }

                    childProperties.AddAdditionalParentFromItsAdditionalChildrenList(this);
                }
            }
        }

        public String AllChildrenString()
        {
            StringBuilder stringBuilder = null;

            if (directChildrenWithNoParentOverrideList != null)
            {
                stringBuilder = new StringBuilder();
                stringBuilder.Append('[');

                Int32 i;
                for (i = 0; i < directChildrenWithNoParentOverrideList.Count - 1; i++)
                {
                    stringBuilder.Append(directChildrenWithNoParentOverrideList[i].idOriginalCase);
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(directChildrenWithNoParentOverrideList[i].idOriginalCase);
            }
            if (additionalChildrenList != null)
            {
                if (stringBuilder == null)
                {
                    stringBuilder = new StringBuilder();
                    stringBuilder.Append('[');
                }

                Int32 i;
                for (i = 0; i < additionalChildrenList.Length - 1; i++)
                {
                    stringBuilder.Append(additionalChildrenList[i].IDOriginalCase);
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(additionalChildrenList[i].IDOriginalCase);
            }
            if (indirectChildrenLinkedFromTheirParentOverrideList != null)
            {
                if (stringBuilder == null)
                {
                    stringBuilder = new StringBuilder();
                    stringBuilder.Append('[');
                }

                Int32 i;
                for (i = 0; i < indirectChildrenLinkedFromTheirParentOverrideList.Count - 1; i++)
                {
                    stringBuilder.Append(indirectChildrenLinkedFromTheirParentOverrideList[i].idOriginalCase);
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(indirectChildrenLinkedFromTheirParentOverrideList[i].idOriginalCase);
            }
            if (stringBuilder == null)
            {
                return "[]";
            }            

            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return String.Format("[HmdBlockID {0} DefinitionContext={1} Count={2} Parents={3} Children={4}]", idOriginalCase,
                definitionContext, countProperty, AllParentsString(), AllChildrenString());
        }

        IEnumerator<HmdIDProperties> IEnumerable<HmdIDProperties>.GetEnumerator()
        {
            return new HmdBlockIDPropertiesEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new HmdBlockIDPropertiesEnumerator(this);
        }
    }

    public struct HmdBlockIDPropertiesEnumerator : IEnumerator<HmdIDProperties>
    {
        private byte state; // state 0: initial state
                            // state 1: directChildrenWithNoParentOverrideList
                            // state 2: additionalChildrenList
                            // state 3: indirectChildrenLinkedFromTheirParentOverrideList
                            // state 4: all child lists were null
        private int index;

        private readonly HmdBlockIDProperties blockIDProperties;

        public HmdBlockIDPropertiesEnumerator(HmdBlockIDProperties blockIDProperties)
        {            
            this.blockIDProperties = blockIDProperties;
            state = 0;
            index = -1;
        }

        HmdIDProperties IEnumerator<HmdIDProperties>.Current
        {
            get
            {
                switch(state)
                {
                    case 0:
                        throw new InvalidOperationException("You must call MoveNext() before you access Current");
                    case 1:
                        return blockIDProperties.directChildrenWithNoParentOverrideList[index];
                    case 2:
                        return blockIDProperties.additionalChildrenList[index].TryToGetReference();
                    case 3:
                        return blockIDProperties.indirectChildrenLinkedFromTheirParentOverrideList[index];
                    case 4:
                        return null;
                    default:
                        throw new InvalidOperationException("Invalid State");
                }
            }
        }

        void IDisposable.Dispose()
        {
        }

        Object System.Collections.IEnumerator.Current
        {
            get
            {
                switch (state)
                {
                    case 0:
                        throw new InvalidOperationException("You must call MoveNext() before you access Current");
                    case 1:
                        return blockIDProperties.directChildrenWithNoParentOverrideList[index];
                    case 2:
                        return blockIDProperties.additionalChildrenList[index].TryToGetReference();
                    case 3:
                        return blockIDProperties.indirectChildrenLinkedFromTheirParentOverrideList[index];
                    case 4:
                        return null;
                    default:
                        throw new InvalidOperationException("Invalid State");
                }
            }
        }

        Boolean System.Collections.IEnumerator.MoveNext()
        {
            index++;
            switch (state)
            {
                case 0:
                    if (blockIDProperties.directChildrenWithNoParentOverrideList != null)
                    {
                        state = 1;
                    }
                    else if(blockIDProperties.additionalChildrenList != null)
                    {
                        state = 2;
                    }
                    else if(blockIDProperties.indirectChildrenLinkedFromTheirParentOverrideList != null)
                    {
                        state = 3;
                    }
                    else
                    {
                        state = 4;
                        return false;
                    }
                    return true;
                case 1:
                    if (index >= blockIDProperties.directChildrenWithNoParentOverrideList.Count)
                    {
                        if (blockIDProperties.additionalChildrenList == null)
                        {
                            if (blockIDProperties.indirectChildrenLinkedFromTheirParentOverrideList == null)
                            {
                                return false;
                            }
                            state+=2;
                        }
                        else
                        {
                            state++;
                        }
                        index = 0;
                    }
                    return true;
                case 2:
                    if (index >= blockIDProperties.additionalChildrenList.Length)
                    {
                        if(blockIDProperties.indirectChildrenLinkedFromTheirParentOverrideList == null)
                        {
                            return false;
                        }
                        state++;
                        index = 0;
                    }
                    return true;
                case 3:
                    if (index >= blockIDProperties.indirectChildrenLinkedFromTheirParentOverrideList.Count)
                    {
                        return false;
                    }
                    return true;
                case 4:
                    return false;
                default:
                    throw new InvalidOperationException("Invalid State");
            }
        }

        void System.Collections.IEnumerator.Reset()
        {
            state = 0;
            index = -1;
        }
    }

    public class HmdValueIDProperties : HmdIDProperties
    {
        public const Boolean IsBlock = false;

        public readonly HmdType hmdType;
        private HmdEnumReference enumReference;
        public HmdEnumReference EnumReference { get { return enumReference; } }

        public override HmdValueIDProperties CastAsValueIDProperties
        {
            get { return this; }
        }
        public override HmdBlockIDProperties CastAsBlockIDProperties
        {
            get { throw new InvalidOperationException("You can't case an HmdValueIDProperties class as an HmdBlockIDProperties class"); }
        }

        public HmdValueIDProperties(String idString, ICountProperty countProperty, HmdType hmdType, HmdEnumReference enumReference, 
            HmdBlockIDProperties directParent, HmdParentReference[] parentOverrideList)
            : base(IsBlock, idString, countProperty, directParent, parentOverrideList)
        {
            if (hmdType == HmdType.Enumeration && enumReference == null)
            {
                throw new ArgumentNullException("For Enumeration types, must have an enumReference", "enumReference");
            }
            this.hmdType = hmdType;
            this.enumReference = enumReference;
        }

        public void ResolveEnumReference(HmdEnum hmdEnum)
        {
            if (hmdEnum == null) throw new ArgumentNullException("hmdEnum");
            this.enumReference = hmdEnum;
        }

        public Boolean IsValidValue(String value, HmdProperties hmdProperties)
        {
            switch (hmdType)
            {
                case HmdType.String:
                    return true;
                case HmdType.Boolean:
                    return (value.Equals("true", StringComparison.CurrentCultureIgnoreCase) || 
                        value.Equals("false", StringComparison.CurrentCultureIgnoreCase));
                case HmdType.Int:
                    return value.IsValidInteger(false, 4);
                case HmdType.Int1:
                    return value.IsValidInteger(false, 1);
                case HmdType.Int2:
                    return value.IsValidInteger(false, 2);
                case HmdType.Int3:
                    return value.IsValidInteger(false, 3);
                case HmdType.Int4:
                    return value.IsValidInteger(false, 4);
                case HmdType.Int5:
                    return value.IsValidInteger(false, 5);
                case HmdType.Int6:
                    return value.IsValidInteger(false, 6);
                case HmdType.Int7:
                    return value.IsValidInteger(false, 7);
                case HmdType.Int8:
                    return value.IsValidInteger(false, 8);
                case HmdType.Int9:
                    return value.IsValidInteger(false, 9);
                case HmdType.Int10:
                    return value.IsValidInteger(false, 10);
                case HmdType.Int11:
                    return value.IsValidInteger(false, 11);
                case HmdType.Int12:
                    return value.IsValidInteger(false, 12);
                case HmdType.Int13:
                    return value.IsValidInteger(false, 13);
                case HmdType.Int14:
                    return value.IsValidInteger(false, 14);
                case HmdType.Int15:
                    return value.IsValidInteger(false, 15);
                case HmdType.Int16:
                    return value.IsValidInteger(false, 16);
                case HmdType.UInt:
                    return value.IsValidInteger(true, 4);
                case HmdType.UInt1:
                    return value.IsValidInteger(true, 1);
                case HmdType.UInt2:
                    return value.IsValidInteger(true, 2);
                case HmdType.UInt3:
                    return value.IsValidInteger(true, 3);
                case HmdType.UInt4:
                    return value.IsValidInteger(true, 4);
                case HmdType.UInt5:
                    return value.IsValidInteger(true, 5);
                case HmdType.UInt6:
                    return value.IsValidInteger(true, 6);
                case HmdType.UInt7:
                    return value.IsValidInteger(true, 7);
                case HmdType.UInt8:
                    return value.IsValidInteger(true, 8);
                case HmdType.UInt9:
                    return value.IsValidInteger(true, 9);
                case HmdType.UInt10:
                    return value.IsValidInteger(true, 10);
                case HmdType.UInt11:
                    return value.IsValidInteger(true, 11);
                case HmdType.UInt12:
                    return value.IsValidInteger(true, 12);
                case HmdType.UInt13:
                    return value.IsValidInteger(true, 13);
                case HmdType.UInt14:
                    return value.IsValidInteger(true, 14);
                case HmdType.UInt15:
                    return value.IsValidInteger(true, 15);
                case HmdType.UInt16:
                    return value.IsValidInteger(true, 16);
                case HmdType.Decimal:
                    throw new NotImplementedException();
                case HmdType.Enumeration:
                    HmdEnum hmdEnum = enumReference.TryGetReference;
                    if (hmdEnum == null)
                    {
                        hmdEnum = hmdProperties.TryGetEnum(enumReference.Name);
                        if (hmdEnum == null)
                        {
                            throw new InvalidOperationException(String.Format("Can't resolve enum reference '{0}'", enumReference.Name));
                        }
                        this.enumReference = hmdEnum;
                    }
                    return hmdEnum.IsValidEnumValue(value.Trim());
                case HmdType.Empty:
                    throw new InvalidOperationException("Cannot validate the type of a null type");
                default:
                    throw new InvalidOperationException(String.Format("HmdType {0} ({1}) is unrecognized", hmdType, (Int32)hmdType));
            }
        }

        public override string ToString()
        {
            String hmdPrintType = (hmdType == HmdType.Enumeration) ?
                String.Format("Enumeration({0})", EnumReference.Name) : hmdType.ToString();
            return String.Format("[HmdValueID {0} DefinitionContext={1} Type={2} Count={3} Parents={4}]", idOriginalCase,
                definitionContext, hmdPrintType, countProperty, AllParentsString());                
        }
    }
}
