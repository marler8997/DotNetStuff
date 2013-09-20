using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

using Microsoft.Win32;
using System.Diagnostics;

namespace More.Net
{
    public interface IResourceHandler
    {
        void HandleResource(ParsedHttpRequest request, HttpResponse response);
    }

}
