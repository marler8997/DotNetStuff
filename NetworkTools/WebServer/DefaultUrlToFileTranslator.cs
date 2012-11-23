using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Marler.NetworkTools
{
    public class DefaultUrlToFileTranslator : IUrlToFileTranslator
    {
        public readonly String rootDirectory;

        public DefaultUrlToFileTranslator(String rootDirectory)
        {
            if (rootDirectory == null) throw new ArgumentNullException("rootDirectory");
            if (!Directory.Exists(rootDirectory)) throw new InvalidOperationException(String.Format("Directory '{0}' does not exist", rootDirectory));

            this.rootDirectory = rootDirectory;
        }

        public string UrlToFile(String url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                if (url[0] == '/') url = url.Substring(1);
                if (Path.DirectorySeparatorChar != '/')
                {
                    url = url.Replace('/', Path.DirectorySeparatorChar);
                }            }

            return Path.Combine(rootDirectory, url);
        }
    }
}
