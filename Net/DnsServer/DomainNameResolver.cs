using System;
using System.Net;

namespace More.Net
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
