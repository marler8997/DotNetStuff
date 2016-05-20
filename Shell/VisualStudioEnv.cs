using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More.Shell
{
    public enum VisualStudioVersion
    {
        Year2002_Version7,
        Year2003_Version7_1,
        Year2005_Version8,
        Year2008_Version9,
        Year2010_Version10,
        Year2012_Version11,
        Year2013_Version12,
        Year2015_Version14,
        Year2016_Version15,
    }

    public class VisualStudio
    {
        public static Boolean EnvironmentSet()
        {
            return !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DevEnvDir"));
        }
    }
}
