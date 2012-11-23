using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace Marler.NetworkTools
{
    public class ConsoleClient
    {
        private static CommandDictionary commandDictionary;
        private const Int32 OpenCommand     = 0;
        private const Int32 CloseCommand    = 1;
        private const Int32 SendCommand     = 2;
        private const Int32 SendFileCommand = 3;
        private const Int32 HelpCommand     = 4;
        private const Int32 ExitCommand     = 5;
        private const Int32 SaveCommand     = 6;
        private const Int32 AppendCommand   = 7;
        private const Int32 EchoCommand     = 8;

        static ConsoleClient()
        {
            commandDictionary = new CommandDictionary();
            commandDictionary.AddCommand(new StringCommand(OpenCommand,     "open", "open a connection", "connect"));
            commandDictionary.AddCommand(new StringCommand(CloseCommand,    "close", "close the connection"));
            commandDictionary.AddCommand(new StringCommand(SendCommand,     "send", "send data"));
            commandDictionary.AddCommand(new StringCommand(SendFileCommand, "sendfile", "Send data from a file"));
            commandDictionary.AddCommand(new StringCommand(HelpCommand,     "help", "Display the Help", "h"));
            commandDictionary.AddCommand(new StringCommand(ExitCommand,     "exit", "Exit", "quit", "byte"));
            commandDictionary.AddCommand(new StringCommand(SaveCommand,     "save", "Save the given string to a file (overrites the file if it exists)"));
            commandDictionary.AddCommand(new StringCommand(AppendCommand,   "append", "Append the given string to a file"));
            commandDictionary.AddCommand(new StringCommand(EchoCommand,     "echo", "Echo the given string"));
        }

        private readonly Int32 sendFileBufferSize;

        private Socket socket;

        public ConsoleClient(Int32 sendFileBufferSize)
        {
            this.sendFileBufferSize = sendFileBufferSize;
        }

        private String GetArg(String str, ref Int32 offset)
        {
            // skip whitespace
            while (offset < str.Length)
            {
                if (!Char.IsWhiteSpace(str[offset])) break;
                offset++;
            }
            if (offset >= str.Length) return null;

            Int32 startOffset = offset;

            while (true)
            {
                offset++;
                if (offset >= str.Length) return str.Substring(startOffset, offset - startOffset);
                if (Char.IsWhiteSpace(str[offset])) return str.Substring(startOffset, offset - startOffset);
            }
        }

        private byte[] GetData(String str, Int32 offset, out Int32 outLength)
        {
            if (offset + 1 >= str.Length)
            {
                Console.WriteLine("Please supply data");
                outLength = 0;
                return null;
            }
            if (str[offset] != ' ')
            {
                Console.WriteLine("I expected a space ' ' after the command, but got '{0}'", str[offset]);
                outLength = 0;
                return null;
            }

            offset++;
            byte[] data = ParseUtilities.ParseLiteralString(str, offset, out outLength);
            if(outLength <= 0)
            {
                Console.WriteLine("Please supply data");
                return null;
            }
            return data;
        }


        private void Disconnect()
        {
            Socket socketCache = socket;
            socket = null;

            if (socketCache != null)
            {
                if (socketCache.Connected) try { socketCache.Shutdown(SocketShutdown.Both); }
                    catch (SocketException) { }
                    catch (ObjectDisposedException) { };
                socketCache.Close();
            }
        }

        private void ReadThreadGotDisconnect(Socket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");

            Socket thisSocketCache = this.socket;
            if (thisSocketCache != null && socket != null)
            {
                if (thisSocketCache != socket)
                {
                    Console.WriteLine("[WARNING: The Read Thread signaled the disconnected event, but it's socket '{0}' does not match the current socket '{1}']",
                       socket.RemoteEndPoint, thisSocketCache.RemoteEndPoint);
                }
                else
                {

                    Console.WriteLine("[Server Closed The Connection]");
                    Console.Write("[Not-Connected]>");
                    this.socket = null;
                }
            }
        }

        public void Shell(IDataLogger receivedDataLogger, Int32 readBufferSize, ISocketConnector connector)
        {
            ShellReadThread readThread = new ShellReadThread(this, new ConsoleMessageLogger("Read Thread"), receivedDataLogger, readBufferSize);

            try
            {
                if (connector != null)
                {
                    Console.WriteLine("[Connecting to {0}...]", connector);
                    try
                    {
                        socket = connector.Connect();
                        Console.WriteLine("[Connected]");
                        readThread.Start(socket);
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("Failed to connect: {0}", e.Message);
                    }
                }


                Boolean isSaveCommand = false; // must be initialized to false


                while (true)
                {
                    try
                    {
                        Int32 offset = 0;

                        Console.Write(String.Format("{0}>",(socket == null) ? "[Not-Connected]" : connector.ConnectionSpecifier));
                        String commandLine = Console.ReadLine();
                        Int32 commandID;

                        String command = GetArg(commandLine, ref offset);
                        if (command == null) continue;
                        if (commandDictionary.TryGetCommandID(command, out commandID))
                        {
                            String filename;
                            Byte[] dataAsBytes;
                            Int32 dataLength;

                            switch (commandID)
                            {
                                case OpenCommand:
                                    if (socket != null)
                                    {
                                        Console.WriteLine("Stream already open to {0}", connector);
                                        continue;
                                    }

                                    // Parse the extra arguments if there are any
                                    String newHostString = GetArg(commandLine, ref offset);
                                    if (newHostString != null)
                                    {
                                        try
                                        {
                                            connector = ParseUtilities.ParseConnectionSpecifier(newHostString);
                                        }
                                        catch (ParseException pe)
                                        {
                                            Console.WriteLine("Could not parse host '{0}': {1}", newHostString, pe.Message);
                                            continue;
                                        }
                                    }

                                    if (connector == null)
                                    {
                                        Console.WriteLine("Please specify a host");
                                        continue;
                                    }

                                    Console.WriteLine("[Connecting to {0}...]", connector);
                                    try
                                    {
                                        socket = connector.Connect();
                                        Console.WriteLine("[Connected]");
                                        readThread.Start(socket);
                                    }
                                    catch (SocketException e)
                                    {
                                        Console.WriteLine("Failed to connect: {0}", e.Message);
                                    }
                                    break;
                                case CloseCommand:
                                    if (readThread != null) readThread.Join();
                                    Disconnect();
                                    break;
                                case SendCommand:
                                    if (socket == null)
                                    {
                                        Console.WriteLine("[Not-Connected]");
                                        continue;
                                    }
                                    if (!socket.Connected)
                                    {
                                        Console.WriteLine("[Not-Connected]");
                                        if (readThread != null) readThread.Join();
                                        Disconnect();
                                    }

                                    dataAsBytes = GetData(commandLine, offset, out dataLength);
                                    if (dataAsBytes != null && dataLength > 0)
                                    {
                                        //dataAsBytes = Encoding.UTF8.GetBytes(data);
                                        socket.Send(dataAsBytes, dataLength, SocketFlags.None);
                                    }
                                    break;

                                case SendFileCommand:
                                    if (socket == null)
                                    {
                                        Console.WriteLine("[Not-Connected]");
                                        continue;
                                    }
                                    if (!socket.Connected)
                                    {
                                        Console.WriteLine("[Not-Connected]");
                                        if (readThread != null) readThread.Join();
                                        Disconnect();
                                    }

                                    socket.SendFile(GetArg(commandLine, ref offset));
                                    //socket.SendFile(GetArg(commandLine, ref offset), sendFileBufferSize);
                                    break;
                                case HelpCommand:
                                    commandDictionary.PrintCommands();
                                    break;
                                case ExitCommand:
                                    return;
                                case SaveCommand:
                                    isSaveCommand = true;
                                    goto case AppendCommand;
                                case AppendCommand:
                                    FileMode fileMode = isSaveCommand ? FileMode.Create : FileMode.Append;
                                    isSaveCommand = false;

                                    filename = GetArg(commandLine, ref offset);
                                    if (filename == null)
                                    {
                                        Console.WriteLine("Please supply a filename");
                                        continue;
                                    }

                                    dataAsBytes = GetData(commandLine, offset, out dataLength);
                                    if (dataAsBytes != null && dataLength > 0)
                                    {
                                        if(fileMode == FileMode.Create)
                                        {
                                            Console.WriteLine("[Opening file '{0}']", filename);
                                        }
                                        else
                                        {
                                            Console.WriteLine("[Appending to file '{0}']", filename);
                                        }

                                        using (FileStream fileStream = new FileStream(filename, fileMode))
                                        {
                                            //dataAsBytes = Encoding.UTF8.GetBytes(data);
                                            fileStream.Write(dataAsBytes, 0, dataLength);
                                            Console.WriteLine("[Success]");
                                        }
                                    }

                                    break;
                                case EchoCommand:

                                    dataAsBytes = GetData(commandLine, offset, out dataLength);
                                    if (dataAsBytes != null && dataLength > 0)
                                    {
                                        Console.WriteLine(Encoding.UTF8.GetString(dataAsBytes, 0, dataLength));
                                    }

                                    break;
                                default:
                                    Console.WriteLine("Unrecognized command id {0} which came from string '{1}'", commandID, command);
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unrecognized Command '{0}'", command);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: {0}", e.Message);
                        Console.WriteLine(e.StackTrace);

                        Socket socketCache = socket;
                        if (socketCache != null)
                        {
                            if (!socketCache.Connected)
                            {
                                if (readThread != null) readThread.Join();
                                Disconnect();
                            }
                        }
                    }
                }
            }
            finally
            {
                if (readThread != null) readThread.Join();
                Disconnect();
            }
        }




        private class ShellReadThread
        {
            private readonly ConsoleClient commandClient;
            private readonly MessageLogger messageLogger;
            private readonly IDataLogger receivedDataLogger;
            private readonly Int32 bufferSize;

            private Socket socket;
            private Thread thread;
            private Boolean keepRunning;

            public ShellReadThread(ConsoleClient commandClient, MessageLogger messageLogger,
                IDataLogger receivedDataLogger, Int32 bufferSize)
            {
                if (commandClient == null) throw new ArgumentNullException("commandClient");
                if (receivedDataLogger == null) throw new ArgumentNullException("receivedDataLogger");

                this.commandClient = commandClient;
                this.messageLogger = messageLogger;
                this.receivedDataLogger = receivedDataLogger;
                this.bufferSize = bufferSize;

                this.socket = null;
                this.thread = null;
                this.keepRunning = false;
            }

            public void Start(Socket socket)
            {
                if (socket == null) throw new ArgumentNullException("socket");
                if (thread != null)
                {
                    throw new InvalidOperationException("already started");
                }

                this.socket = socket;
                this.thread = new Thread(Run);
                this.thread.IsBackground = true;
                this.keepRunning = true;
                this.thread.Start();
            }

            public void Join()
            {
                this.keepRunning = false;
                Socket socketCache = socket;
                socket = null;
                Thread threadCache = thread;
                thread = null;

                if (socketCache != null)
                {
                    if (socketCache.Connected) try { socketCache.Shutdown(SocketShutdown.Both); }
                        catch (SocketException) { }
                        catch (ObjectDisposedException) { };
                    socketCache.Close();
                }

                if (threadCache != null && threadCache.IsAlive)
                {
                    threadCache.Join();
                }
            }


            public void Run()
            {
                try
                {
                    byte[] buffer = new byte[bufferSize];
                    while (keepRunning)
                    {
                        Int32 bytesRead = socket.Receive(buffer, SocketFlags.None);
                        if (bytesRead <= 0)
                        {
                            return;
                        }

                        receivedDataLogger.LogData(buffer, 0, bytesRead);
                    }
                }
                catch (SocketException se)
                {
                    messageLogger.Log(String.Format("SocketException: {0}", se.Message));
                }
                catch (ObjectDisposedException ode)
                {
                    messageLogger.Log(String.Format("SocketException: {0}", ode.Message));
                }
                finally
                {
                    thread = null;

                    Boolean keepRunningCache = keepRunning;
                    this.keepRunning = false;

                    Socket socketCache = socket;
                    socket = null;

                    if (socketCache != null)
                    {
                        if (keepRunningCache) commandClient.ReadThreadGotDisconnect(socketCache);

                        if (socketCache.Connected) try { socketCache.Shutdown(SocketShutdown.Both); }
                            catch (SocketException) { }
                            catch (ObjectDisposedException) { };
                        socketCache.Close();
                    }
                }
            }

        }
    }
}
