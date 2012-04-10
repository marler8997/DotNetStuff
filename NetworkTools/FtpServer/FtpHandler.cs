using System;
using System.Collections.Generic;
using System.Linq;
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

            byte[] response = new byte[5];
            response[3] = (byte)'\r';
            response[4] = (byte)'\n';

            response[0] = (byte)'2';
            response[1] = (byte)'2';
            response[2] = (byte)'0';

            messageLogger.Log("Sending '220'...");
            dataLogger.LogData(response, 0, 5);
            stream.Write(response, 0, 5);

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
                messageLogger.Log("Got Command '{0}'", command);

                if (command.Length < 3)
                {

                    messageLogger.Log("Command was only {0} characters", command.Length);

                RESPOND_500:
                    response[0] = (byte)'5';
                    response[1] = (byte)'0';
                    response[2] = (byte)'0';
                }
                else
                {
                    String commandName = command.Substring(0,
                        (command.Length == 3 ||
                        ((command[3] < 'a' || command[3] > 'z') && (command[3] < 'A' || command[3] > 'Z')) ?
                        3 : 4)).ToUpper();

                    UInt16 returnCode = handler.HandleCommand(commandName);

                    String codeString = returnCode.ToString();
                    if (codeString.Length != 3)
                    {
                        response[0] = (byte)'5';
                        response[1] = (byte)'0';
                        response[2] = (byte)'0';
                    }
                    else
                    {
                        response[0] = (byte)codeString[0];
                        response[1] = (byte)codeString[1];
                        response[2] = (byte)codeString[2];
                    }
                }

                messageLogger.Log("Return Code '{0}{1}{2}'", (char)response[0], (char)response[1], (char)response[2]);
                stream.Write(response, 0, 5);

            }
        }

    }
}
