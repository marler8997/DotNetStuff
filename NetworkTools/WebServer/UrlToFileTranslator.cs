using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.NetworkTools
{
    public interface IUrlToFileTranslator
    {
        String UrlToFile(String url);
    }
}
