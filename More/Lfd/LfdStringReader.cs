using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace More.Lfd
{
    public class LfdStringReader : LfdReader
    {
        public readonly String lfdString;

        public LfdStringReader(String lfdString)
            : base(new StringReader(lfdString))
        {
            this.lfdString = lfdString;
        }

    }
}
