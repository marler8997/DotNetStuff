using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Hmd
{
    public abstract class HmdID
    {
        public readonly Boolean isBlock;
        public readonly String idOriginalCase;
        public readonly String idLowerCase;

        public readonly HmdBlockID parent;

        protected HmdID(Boolean isBlockID, String id, HmdBlockID parent)
        {
            this.isBlock = isBlockID;
            this.idOriginalCase = id;
            this.idLowerCase = id.ToLower();
            this.parent = parent;
        }

        public abstract HmdValueID CastAsValueID { get; }
        public abstract HmdBlockID CastAsBlockID { get; }

        public String CreateContextString()
        {
            HmdBlockID currentParent = parent;
            Stack<HmdBlockID> parents = new Stack<HmdBlockID>();
            while (currentParent != null)
            {
                parents.Push(currentParent);
                currentParent = currentParent.parent;
            }
            
            if (parents.Count <= 1) return idLowerCase;

            // Pop off the root
            parents.Pop();

            StringBuilder stringBuilder = new StringBuilder();
            while (parents.Count > 0)
            {
                stringBuilder.Append(parents.Pop().idLowerCase);
                stringBuilder.Append('.');
            }
            stringBuilder.Append(idLowerCase);
            return stringBuilder.ToString();
        }

        public void PrintTree() { PrintTree(0); }
        public abstract void PrintTree(Int32 level);
        public abstract void PrintCompact();
    }

    public class HmdValueID : HmdID
    {
        public const Boolean IsBlockID = false;

        public readonly String value;

        public HmdValueID(String idString, String value,HmdBlockID parent)
            : base(IsBlockID, idString, parent)
        {
            this.value = value;
        }

        public override HmdValueID CastAsValueID { get { return this; } }
        public override HmdBlockID CastAsBlockID { get { throw new InvalidOperationException("This is not a Block ID"); } }

        public override void PrintTree(Int32 level)
        {
            Console.Out.WriteLine(level, "{0}:{1};", idOriginalCase, value);
        }
        public override void PrintCompact()
        {
            Console.Write("{0}:{1};", idOriginalCase, value);
        }

        public override string ToString()
        {
            return (value == null) ? (idOriginalCase + ";") : String.Format("{0}:{1};", idOriginalCase, value);
        }
    }

    public class HmdBlockID : HmdID , HmdParentReference
    {
        public const Boolean IsBlockID = true;

        private readonly List<HmdID> children = new List<HmdID>();

        public HmdBlockID(String idString, HmdBlockID parent)
            : base(IsBlockID, idString, parent)
        {

        }

        String HmdIDReference.IDOriginalCase { get { return idOriginalCase; } }
        String HmdIDReference.IDLowerCase { get { return idLowerCase; } }
        HmdIDProperties HmdIDReference.TryToGetReference() { return null; }
        HmdBlockIDProperties HmdParentReference.TryToGetReferenceAsBlock() { return null; }

        public Int32 ChildCount
        {
            get { return children.Count; }
        }

        public HmdID TryGetChild(String childID)
        {
            for (int i = 0; i < children.Count; i++)
            {
                HmdID child = children[i];
                if (childID.Equals(child.idLowerCase, StringComparison.CurrentCultureIgnoreCase))
                {
                    return child;
                }
            }
            return null;
        }

        public HmdID GetChild(Int32 index)
        {
            return children[index];
        }

        public void AddChild(HmdID id)
        {
            children.Add(id);
        }

        public override HmdValueID CastAsValueID { get { throw new InvalidOperationException("This is not a Value ID"); } }
        public override HmdBlockID CastAsBlockID { get { return this; } }

        public override void PrintTree(Int32 level)
        {
            Console.Out.WriteLine(level,"{0} {{", idOriginalCase);

            level++;
            for (Int32 i = 0; i < children.Count; i++)
            {
                children[i].PrintTree(level);
            }
            level--;

            Console.Out.WriteLine(level, "}");
        }

        public override void PrintCompact()
        {
            Console.Write(idOriginalCase);
            Console.Write("{");
            for (Int32 i = 0; i < children.Count; i++)
            {
                children[i].PrintCompact();
            }
            Console.Write("}");
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(idOriginalCase);
            stringBuilder.Append('{');
            for (int i = 0; i < children.Count; i++)
            {
                stringBuilder.Append(children[i].ToString());
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }
    }

}
