using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Marler.NetworkTools
{
    public class ProxyHandler
    {       
        enum ProxyState { Initial, Version4, Version5, Done};

        private readonly MessageLogger logger;
        private readonly NetworkStream stream;

        public ProxyHandler(MessageLogger logger, NetworkStream stream)
        {
            this.logger = logger;
            this.stream = stream;
        }

        public void Run()
        {
            ProxyState proxyState = ProxyState.Initial;
            Int32 bytesRead;
            byte[] readBuffer = new byte[1024];
            StringBuilder stringBuilder = new StringBuilder();

            while (true)
            {

                logger.Log("Local: Reading");                
                bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);

                if (bytesRead <= 0) break;

                stringBuilder.Append(Encoding.UTF8.GetString(readBuffer, 0, bytesRead));

                int offset = 0;

                do
                {
                    switch (proxyState)
                    {
                        case ProxyState.Initial:
                            if(readBuffer[offset] == SocksProxy.ProxyVersion4)
                            {
                                offset++;
                                proxyState = ProxyState.Version4;
                            }
                            else if (readBuffer[offset] == SocksProxy.ProxyVersion5)
                            {
                                offset++;
                                proxyState = ProxyState.Version5;
                            }
                            else
                            {
                                throw new FormatException(String.Format("Expected Proxy Version 4 or 5, but got {0}", readBuffer[offset]));
                            }
                            break;
                        case ProxyState.Version4:
                            throw new NotImplementedException();
                        case ProxyState.Version5:
                            throw new NotImplementedException();

                        default:
                            throw new InvalidOperationException(String.Format("Unrecognized Proxy State '{0}' ({1})", proxyState, (int)proxyState));
                    }
                }
                while (offset < bytesRead);

            } while ((proxyState != ProxyState.Done) && stream.DataAvailable);



        }



    }
}
