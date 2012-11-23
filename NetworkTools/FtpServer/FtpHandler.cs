using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Marler.NetworkTools
{
    public class FtpHandler
    {
        private readonly IFtpCommandHandler handler;
        private readonly NetworkStream stream;
        private readonly MessageLogger messageLogger;
        private readonly IDataLogger dataLogger;

        public FtpHandler(IFtpCommandHandler handler, NetworkStream stream,
            MessageLogger messageLogger, IDataLogger dataLogger)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            if (stream == null) throw new ArgumentNullException("null");

            this.handler = handler;
            this.stream = stream;

            this.messageLogger = (messageLogger == null) ? MessageLogger.NullMessageLogger : messageLogger;
            this.dataLogger = (dataLogger == null) ? DataLogger.Null : dataLogger;
        }


        public void Run()
        {
            byte[] buffer = new byte[4096];
            StringBuilder responseBuilder = new StringBuilder();

            responseBuilder.Append("220 localhost FTP Service ready.\r\n");

            Byte[] resposeBytes = Encoding.UTF8.GetBytes(responseBuilder.ToString());
            dataLogger.LogData(resposeBytes, 0, resposeBytes.Length);
            stream.Write(resposeBytes, 0, resposeBytes.Length);
            responseBuilder.Length = 0;

            while (true)
            {
                int bytesRead;
                int offset = 0;

                while (true)
                {
                    messageLogger.Log("Waiting for command...");
                    bytesRead = stream.Read(buffer, offset, buffer.Length);

                    if (bytesRead <= 0)
                    {
                        messageLogger.Log("Connection Closed");
                        return;
                    }

                    messageLogger.Log("Got {0} bytes", bytesRead);

                    int limit = offset + bytesRead;
                    while (offset < limit)
                    {
                        if (buffer[offset] == (byte)'\n') { goto FOUND_NEWLINE; }
                        offset++;
                    }

                    if (offset >= buffer.Length) throw new InvalidOperationException("buffer not big enough");
                }

            FOUND_NEWLINE:

                offset++;
                if (bytesRead > offset)
                {
                    throw new NotImplementedException("I need to shift the rest of the bytes here");
                }

                int commandEnd = (offset > 1 && buffer[offset - 2] == '\r') ? offset - 2 : offset - 1;

                String command = Encoding.UTF8.GetString(buffer, 0, commandEnd);

                String commandUpperCase = null;
                String commandArgs = null;
                
                Int32 spaceIndex = command.IndexOf(' ');

                if(spaceIndex < 0)
                {
                    commandUpperCase = command.ToUpper();
                }
                else
                {
                    commandUpperCase = command.Remove(spaceIndex).ToUpper();
                    if(spaceIndex + 1 <= command.Length)
                    {
                        commandArgs = command.Substring(spaceIndex + 1);
                    }
                }
                messageLogger.Log("Got Command '{0}'{1}", commandUpperCase, (commandArgs == null)?"":String.Format(" args='{0}'",commandArgs));

                handler.HandleCommand(responseBuilder, commandUpperCase, commandArgs);

                String responseString = "500 Command was not handled correctly.\r\n";
                if (responseBuilder.Length > 0)
                {
                    responseString = responseBuilder.ToString();
                    responseBuilder.Length = 0;
                }
                else
                {                    
                    messageLogger.Log("Command '{0}' was not handled", command);
                }

                resposeBytes = Encoding.UTF8.GetBytes(responseString);
                stream.Write(resposeBytes, 0, resposeBytes.Length);
                dataLogger.LogData(resposeBytes, 0, resposeBytes.Length);
            }
        }

    }
}
