using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

using Microsoft.Win32;
using System.Diagnostics;

namespace Marler.NetworkTools
{
    public interface IResourceHandler
    {
        void HandleResource(HttpRequest request, HttpResponse response);
    }

}
