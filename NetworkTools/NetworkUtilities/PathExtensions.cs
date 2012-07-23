﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.NetworkTools
{
    public static class PathExtensions
    {
        public static String SystemPathToUrlPath(String systemPath)
        {
            if (String.IsNullOrEmpty(systemPath)) return systemPath;

            if (systemPath.Length <= 1) return systemPath;

            if (systemPath[1] == ':')
            {
                if(systemPath.Length <= 2) return "/";
                systemPath = systemPath.Substring(2);
            }

            return systemPath.Replace('\\', '/');
        }
    }
}
