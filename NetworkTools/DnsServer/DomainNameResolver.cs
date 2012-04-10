using System;
using System.Net;

namespace Marler.NetworkTools
{
    public class DomainNameResolver
    {
        private readonly Object sync = new Object();

        public DomainNameResolver()
        {

        }


        public IPAddress Resolve(DomainName domainName)
        {
            lock (sync)
            {
                return null;
            }
        }



    }
}
