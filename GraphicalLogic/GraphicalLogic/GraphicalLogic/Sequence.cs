using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicalLogic
{
    public delegate void LineHandler(String line);


    public abstract class CharacterFilter
    {
        public readonly LineHandler[] lineHandlers;
        public CharacterFilter(LineHandler[] lineHandlers)
        {
            this.lineHandlers = lineHandlers;
        }
    }
    public class CharacterTreeFilter : CharacterFilter
    {
        public readonly Char c;

        public readonly Dictionary<Char, CharacterFilter> nextCharacterFilters;
        public CharacterTreeFilter(Char c, LineHandler[] lineHandlers)
            : base(lineHandlers)
        {
            this.c = c;
            this.nextCharacterFilters = new Dictionary<Char, CharacterFilter>();
        }
    }
    public class CharacterStringFilter : CharacterFilter
    {
        public readonly String filterString;
        public readonly Int32 filterTreeOffset;
        public CharacterStringFilter(String filterString, Int32 filterTreeOffset, LineHandler[] lineHandlers)
            : base(lineHandlers)
        {
        }
    }


    public class Filter
    {
        readonly Dictionary<Char, CharacterFilter> characterFilterMap;

        public Filter()
        {
            characterFilterMap = new Dictionary<Char, CharacterFilter>();
        }

        public void SetFilterHandlers(String filterString, LineHandler[] lineHandlers)
        {
            Char firstChar = filterString[0];

            CharacterFilter filter;
            if (!characterFilterMap.TryGetValue(firstChar, out filter))
            {
                filter = new CharacterStringFilter(filterString, 1, lineHandlers);
                characterFilterMap.Add(firstChar, filter);
            }

            throw new NotImplementedException();
            /*
            for (int i = 1; i < filterString.Length; i++)
            {
                Char c = filterString[i];
                if (!filter.nextCharacterFilters.TryGetValue(c, out filter))
                {
                    break;
                }
            }

            if (filter.lineHandlers == null)
            {
                filter.lineHandlers = new LineHandler[] {lineHandler};
            }
            */
        }
    }




    public abstract class SequenceNode
    {
        public readonly String name;

    }

    public class Sequence
    {
        readonly SequenceNode[] orderedNodes;

        public Sequence(SequenceNode[] orderedNodes)
        {
            this.orderedNodes = orderedNodes;
        }
    }



    //
    // A graph sequence is a set of connected nodes
    // that objects can traverse
    //
    public class GraphSequence
    {

    }





}
