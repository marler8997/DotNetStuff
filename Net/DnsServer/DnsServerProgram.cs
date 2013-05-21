using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Net
{
    class DnsServerProgram
    {
        /*
         * Idea:
         * DnsServer that intercepts dns requests.
         * This DnsServer can work in conjunction with another dns server and a proxy server.
         * Let's say that a program wants to connect to asite.com.  If asite.com is behind a proxy,
         * then you can run this dns server with the following configuration:
         *  1.
         *  
         */


        static void Main(string[] args)
        {
            DnsServerOptions optionsParser = new DnsServerOptions();
            List<String> nonOptionArgs = optionsParser.Parse(args);


            DomainNameResolver resolver = new DomainNameResolver();


            DnsServer dnsServer = new DnsServer(resolver, optionsParser.port.ArgValue);
            dnsServer.Run();
        }
    }
}
