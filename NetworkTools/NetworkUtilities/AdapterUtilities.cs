using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.NetworkTools
{
    public enum AdapterType { Client, Server};
    public class AdapterUtilities
    {
        public static AdapterType GetAdapterType(String adapterType)
        {
            if (adapterType.Equals("client", StringComparison.CurrentCultureIgnoreCase))
            {
                return AdapterType.Client;
            }
            if (adapterType.Equals("server", StringComparison.CurrentCultureIgnoreCase))
            {
                return AdapterType.Server;
            }
            throw new ArgumentException(String.Format("Could not recognize adapter type '{0}', expected '{1}' or '{2}'",
                adapterType, "client", "server"));
        }
    }
}
