using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using More;
using More.Net;

namespace CDViewer
{
    //
    // How to get a map
    //
    // Trigger: An HTTP GET request with the variable 'action=start_rob_house'
    // 1. In the get request, save the 'map_encryption_key=BEB16782E62EAD7AA134FFABDF81C7BF4B21B889' variable
    //    This is a 40 character hex string representing a sha1 hash
    // 2. In the response, the first token (tokens separated by whitespace) is the owner name, the second token is the encrypted map
    // 3. Decrypt the map using the SHA1 

    public partial class HouseViewerForm : Form
    {
        static readonly byte[] secret = Encoding.ASCII.GetBytes("@Please do not use this secret string to connect unfairly modded clients to the main server.  Keep in mind that this is an indie, open-source game made entirely by one person.  I am trusting you to do the right thing.  --Jason");


        public static void HandleEncryptedMap(Byte[] shaKey, Byte[] encryptedData)
        {
            //
            // Decrypt the map
            //
            Console.WriteLine("SHA: {0}", BitConverter.ToString(shaKey));
            Console.WriteLine("DAT: {0}", BitConverter.ToString(encryptedData));
            Console.WriteLine("Map: {0}", CDSha.Decrypt(shaKey, encryptedData));

        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CDLoader.Load(@"C:\Users\Jonathan Marler\Desktop\CastleDoctrine_v31");

            //
            // Start the listen thread
            //
            GameClientAcceptor acceptor = new GameClientAcceptor(EndPoints.EndPointFromIPOrHost("thecastledoctrine.net", 80));
            TcpSelectServer2 selectServer = new TcpSelectServer2(new Byte[2048], new TcpSelectListener2[] {
                new TcpSelectListener2(new IPEndPoint(IPAddress.Any, 80), 32, acceptor.Accept),
            });
            acceptor.selectServer = selectServer;

            Thread listenThread = new Thread(selectServer.Run);
            listenThread.Name = "ListenThread";
            listenThread.Start();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HouseViewerForm());
        }
        public HouseViewerForm()
        {
            InitializeComponent();
        }
    }

    public class GameClientAcceptor
    {
        readonly EndPoint cdServerEndPoint;
        public TcpSelectServer2 selectServer;
        UInt32 nextID;
        public GameClientAcceptor(EndPoint cdServerEndPoint)
        {
            this.cdServerEndPoint = cdServerEndPoint;
        }
        public SocketDataHandler Accept(Socket clientSocket)
        {
            Socket serverSocket = new Socket(cdServerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Connect(cdServerEndPoint);

            GameClientDataHandler dataHandler = new GameClientDataHandler(nextID, clientSocket, serverSocket);
            selectServer.AddDataSocket(serverSocket, dataHandler.DataFromServer);

            Console.WriteLine("[INFO] New Client/Server {0}", nextID);
            nextID++;
            return dataHandler.DataFromClient;
        }
    }
    public static class CDSha
    {
        public const String sharedServerSecretString =
            "Please do not use this secret string to connect unfairly modded clients to the main server.  Keep in mind that this is an indie, open-source game made entirely by one person.  I am trusting you to do the right thing.  --Jason";
        public static readonly Byte[] sharedServerSecret = Encoding.ASCII.GetBytes(sharedServerSecretString);

        public static String Decrypt(Byte[] key, Byte[] encrypted)
        {
            Sha1 sha1 = new Sha1();

            Byte[] baseChars = new Byte[encrypted.Length + 19];
            UInt32 baseCharsLength = 0;

            int counter = 0;
            while (baseCharsLength < encrypted.Length)
            {
                Byte[] counterStringBytes = Encoding.ASCII.GetBytes(counter.ToString());
                sha1.Add(counterStringBytes, 0, counterStringBytes.Length);
                sha1.Add(key, 0, key.Length);
                sha1.Add(sharedServerSecret, 0, sharedServerSecret.Length);
                sha1.Add(counterStringBytes, 0, counterStringBytes.Length);

                UInt32[] hash = sha1.Finish();

                baseChars.BigEndianSetUInt32(baseCharsLength + 0, hash[0]);
                baseChars.BigEndianSetUInt32(baseCharsLength + 4, hash[1]);
                baseChars.BigEndianSetUInt32(baseCharsLength + 8, hash[2]);
                baseChars.BigEndianSetUInt32(baseCharsLength + 12, hash[3]);
                baseChars.BigEndianSetUInt32(baseCharsLength + 16, hash[4]);
                baseCharsLength += 20;

                sha1.Reset();
                counter++;
            }

            Char[] decrypted = new Char[encrypted.Length];
            for (int i = 0; i < decrypted.Length; i++)
            {
                decrypted[i] = (Char)(baseChars[i] ^ encrypted[i]);
            }

            return new String(decrypted);
        }
    }


    public class GameClientDataHandler
    {
        public const String RobAction = "action=start_rob_house";
        public const String MapEncryption = "map_encryption_key=";

        readonly UInt32 id;
        readonly Socket clientSocket, serverSocket;
        readonly LineParser clientLineParser = new LineParser(Encoding.ASCII, 1024, 1024);
        readonly LineParser serverLineParser = new LineParser(Encoding.ASCII, 1024, 1024);

        Byte[] nextMapEncryptionKey;
        enum ServerParseState
        {
            Idle,

            // Get Encrypted Map
            BlankLine,
            Owner,
            Map,
        }
        ServerParseState serverParseState;

        public GameClientDataHandler(UInt32 id, Socket clientSocket, Socket serverSocket)
        {
            this.id = id;
            this.clientSocket = clientSocket;
            this.serverSocket = serverSocket;
            serverParseState = ServerParseState.Idle;
        }
        public SocketDataHandler DataFromServer(Socket socket, Byte[] data, UInt32 length)
        {
            if (length == 0)
            {
                String rest = serverLineParser.Flush();
                //if (rest != null) Console.WriteLine("[SERVER {0}] {1}", id, rest);

                Console.WriteLine("[SERVER {0}] [Closed]", id);
                if(clientSocket.Connected) clientSocket.Shutdown(SocketShutdown.Both);
                return null;
            }


            clientSocket.Send(data, 0, (Int32)length, SocketFlags.None);

            serverLineParser.Add(data, 0, length);
            String line;
            while (true)
            {
                line = serverLineParser.GetLine();
                if (line == null) break;
                Console.WriteLine("[SERVER {0}] {1}", id, line);

                //
                // Check for encrypted map
                //
                if (serverParseState > ServerParseState.Idle)
                {
                    if (serverParseState == ServerParseState.BlankLine)
                    {
                        if (line.Length <= 0) serverParseState++;
                    }
                    else if (serverParseState == ServerParseState.Owner)
                    {
                        serverParseState++;
                    }
                    else if (serverParseState == ServerParseState.Map)
                    {
                        HouseViewerForm.HandleEncryptedMap(nextMapEncryptionKey, Convert.FromBase64String(line));
                        nextMapEncryptionKey = null;
                        serverParseState = ServerParseState.Idle;
                    }
                    else
                    {
                        throw new InvalidOperationException(String.Format("Invalid server parse state '{0}'", serverParseState));
                    }
                }
            }

            return DataFromServer;
        }
        public SocketDataHandler DataFromClient(Socket socket, Byte[] data, UInt32 length)
        {
            if (length == 0)
            {
                String rest = clientLineParser.Flush();
                if (rest != null) Console.WriteLine("[CLIENT {0}] {1}", id, rest);

                Console.WriteLine("[CLIENT {0}] [Closed]", id);
                if (serverSocket.Connected) serverSocket.Shutdown(SocketShutdown.Both);
                return null;
            }

            serverSocket.Send(data, 0, (Int32)length, SocketFlags.None);

            clientLineParser.Add(data, 0, length);
            String line;
            while(true)
            {
                line = clientLineParser.GetLine();
                if (line == null) break;
                Console.WriteLine("[CLIENT {0}] {1}", id, line);

                // Search for 'action=start_rob_house'
                if (serverParseState == ServerParseState.Idle)
                {
                    if (line.Contains(RobAction))
                    {
                        Int32 encryptionKeyIndex = line.IndexOf(MapEncryption);
                        if (encryptionKeyIndex < 0) throw new FormatException(String.Format("Client Request had '{0}' but not '{1}'", RobAction, MapEncryption));

                        String encryptionKeyString = line.Substring(encryptionKeyIndex + MapEncryption.Length, 40);
                        nextMapEncryptionKey = new Byte[20];
                        nextMapEncryptionKey.ParseHex(0, encryptionKeyString, 0, 40);

                        Console.WriteLine("[INFO] Rob House key={0}", BitConverter.ToString(nextMapEncryptionKey));
                        serverParseState = ServerParseState.BlankLine;
                    }
                }
            }

            return DataFromClient;
        }
    }
}
