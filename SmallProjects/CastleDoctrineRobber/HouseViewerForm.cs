using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
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
    //
    public partial class HouseViewerForm : Form
    {
        static readonly byte[] secret = Encoding.ASCII.GetBytes("@Please do not use this secret string to connect unfairly modded clients to the main server.  Keep in mind that this is an indie, open-source game made entirely by one person.  I am trusting you to do the right thing.  --Jason");


        public static void HandleEncryptedMap(Byte[] mapKey, Byte[] encryptedData)
        {
            //
            // Decrypt the map
            //
            //Console.WriteLine("SHA: {0}", BitConverter.ToString(mapKey));
            //Console.WriteLine("DAT: {0}", BitConverter.ToString(encryptedData));

            String map = CDSha.Decrypt(mapKey, encryptedData);

            String[] objectIds = map.Split('#');
            if (objectIds.Length != 1024) throw new FormatException(String.Format(
                 "Expected map.Split('#') to be 1024 but was {0}", objectIds.Length));

            //CDLoader.PrintMap(objectIds);

            HouseObjectDefinition[] mapObjects = CDLoader.ParseMap(objectIds);
            houseViewerForm.Invoke((Action)(() => { houseViewerForm.LoadMap(mapObjects); }));
        }

        static HouseViewerForm houseViewerForm;
        static TcpSelectServer2 selectServer;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {

                //String gameDirectory = @"C:\Users\Jonathan Marler\Desktop\CastleDoctrine_v31";
                //String gameDirectory = @"D:\Tools\CastleDoctrine_v32";
                String gameDirectory = Environment.CurrentDirectory;

                String cdServerHostname = "thecastledoctrine.net";
                //String cdServerConnectorString = "gateway:proxy.houston.hp.com:8080%thecastledoctrine.net";


                //
                // Initialize Static Variables
                //
                CDLoader.Load(gameDirectory);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                houseViewerForm = new HouseViewerForm();

                //
                // Setup WebProxy
                //
                ISocketConnector cdConnector = CDProxy.SetupProxyInterceptor(gameDirectory);


                //
                // Start the listen thread
                //
                EndPoint cdServerEndPoint = EndPoints.EndPointFromIPOrHost(cdServerHostname, 80);
                GameClientAcceptor acceptor = new GameClientAcceptor(cdServerEndPoint, cdConnector);
                selectServer = new TcpSelectServer2(new Byte[2048], new TcpSelectListener2[] {
                    new TcpSelectListener2(new IPEndPoint(IPAddress.Any, 80), 32, acceptor.Accept),
                });

                acceptor.selectServer = selectServer;

                Thread listenThread = new Thread(selectServer.Run);
                listenThread.Name = "ListenThread";
                listenThread.Start();

                Application.Run(houseViewerForm);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                CDProxy.RestoreProxy();
            }
        }

        public class TileBoxSet : Control
        {
            //public const Int32 TileSize = 32;

            public readonly HouseViewerForm house;
            public readonly Int32 x, y, size;
            public TileBoxSet(HouseViewerForm house, Int32 x, Int32 y, Int32 size)
            {
                DoubleBuffered = true;
                this.house = house;
                this.x = x;
                this.y = y;
                this.size = size;
                MouseMove += MouseMoveHandler;
            }
            void MouseMoveHandler(Object sender, MouseEventArgs e)
            {
                Int32 boxX = e.X / house.tileSize;
                Int32 boxY = e.Y / house.tileSize;
                if (boxX < 0 || boxX > size || boxY < 0 || boxY > size) return;

                Int32 houseObjectOffset = (31 - (y + boxY)) * 32 + (x + boxX);
                house.MouseOverHouseObject(houseObjectOffset);
            }
            static readonly SolidBrush SemiTransparentBrush =
                new SolidBrush(Color.FromArgb(0x88, 1, 1, 1));
            protected override void OnPaint(PaintEventArgs pe)
            {
                for (int boxY = 0; boxY < size; boxY++)
                {
                    Int32 houseObjectOffset = (31 - (y + boxY)) * 32;
                    Int32 controlRowOffset = boxY * house.tileSize;
                    Int32 controlColOffset = 0;
                    for (int boxX = 0; boxX < size; boxX++)
                    {
                        pe.Graphics.DrawRectangle(Pens.Black, new Rectangle(controlColOffset, controlRowOffset, house.tileSize, house.tileSize));
                        //pe.Graphics.DrawString(String.Format("Box {0} {1}", x, y), this.Font, Brushes.Black, new PointF(0, 0));
                        HouseObjectDefinition houseObject =
                            house.houseObjects[houseObjectOffset + (x + boxX)];
                        pe.Graphics.DrawImage(houseObject.Bitmap, controlColOffset, controlRowOffset, house.tileSize, house.tileSize);
                            //new Point(controlColOffset, controlRowOffset));

                        if (houseObject.id == 999)
                        {
                            pe.Graphics.DrawRectangle(house.goalPen,
                                controlColOffset + house.quarterTileSize / 2,
                                controlRowOffset + house.quarterTileSize / 2,
                                house.tileSize - house.quarterTileSize,
                                house.tileSize - house.quarterTileSize);
                        }

                        String extra = houseObject.Extra;
                        if (extra != null)
                        {
                            pe.Graphics.FillRectangle(SemiTransparentBrush, 0, 0, extra.Length * Font.Size, Font.GetHeight());
                            pe.Graphics.DrawString(extra, Font, Brushes.Black, new PointF(0, 0));
                        }


                        controlColOffset += house.tileSize;
                    }

                }
            }
        }
        readonly Label label;
        HouseObjectDefinition currentMouseOverObject;

        readonly HouseObjectDefinition[] houseObjects = new HouseObjectDefinition[1024];
        readonly TileBoxSet[] tileBoxSets = new TileBoxSet[16];


        Int32 tileSize;
        Int32 quarterTileSize;
        Pen goalPen;


        public HouseViewerForm()
        {
            InitializeComponent();


            Text = "Castle Doctine Robber";
            //Width = 1024;
            //Height = 1024;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Location = new Point(Location.X, 10);


            label = new Label();
            Controls.Add(label);
            label.Location = new Point(3, 3);
            label.BackColor = Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF);

            Button zoomOutButton = new Button();
            zoomOutButton.Text = "-";
            zoomOutButton.Width = 25;
            zoomOutButton.Location = new Point(150, 3);
            zoomOutButton.MouseClick += (s, e) =>
            {
                if (tileSize > 0)
                {
                    SetTileSize(tileSize - 1);
                }
            };
            Controls.Add(zoomOutButton);
            Button zoomInButton = new Button();
            zoomInButton.Text = "+";
            zoomInButton.Width = 25;
            zoomInButton.Location = new Point(180, 3);
            zoomInButton.MouseClick += (s, e) =>
            {
                SetTileSize(tileSize + 1);
            };
            Controls.Add(zoomInButton);

            tileBoxSets[0]  = new TileBoxSet(this,  0,  0, 8);
            tileBoxSets[1]  = new TileBoxSet(this,  8,  0, 8);
            tileBoxSets[2]  = new TileBoxSet(this, 16,  0, 8);
            tileBoxSets[3]  = new TileBoxSet(this, 24,  0, 8);
            tileBoxSets[4]  = new TileBoxSet(this,  0,  8, 8);
            tileBoxSets[5]  = new TileBoxSet(this,  8,  8, 8);
            tileBoxSets[6]  = new TileBoxSet(this, 16,  8, 8);
            tileBoxSets[7]  = new TileBoxSet(this, 24,  8, 8);
            tileBoxSets[8]  = new TileBoxSet(this,  0, 16, 8);
            tileBoxSets[9]  = new TileBoxSet(this,  8, 16, 8);
            tileBoxSets[10] = new TileBoxSet(this, 16, 16, 8);
            tileBoxSets[11] = new TileBoxSet(this, 24, 16, 8);
            tileBoxSets[12] = new TileBoxSet(this,  0, 24, 8);
            tileBoxSets[13] = new TileBoxSet(this,  8, 24, 8);
            tileBoxSets[14] = new TileBoxSet(this, 16, 24, 8);
            tileBoxSets[15] = new TileBoxSet(this, 24, 24, 8);
            for (int i = 0; i < tileBoxSets.Length; i++)
            {
                TileBoxSet tileBoxSet = tileBoxSets[i];
                Controls.Add(tileBoxSet);
            }

            Rectangle screen = Screen.FromControl(this).Bounds;
            Int32 screenSize = (screen.Width > screen.Height) ? screen.Height : screen.Width;

            SetTileSize(screenSize / 36);

            HouseObjectDefinition floor = CDLoader.HouseObjectDefinitionMap[0];
            for (int i = 0; i < houseObjects.Length; i++)
            {
                houseObjects[i] = floor;
            }

            //
            // Tests
            //
            /*
            UInt32 index = (UInt32)houseObjects.Length - 33;
            foreach (HouseObjectDefinition h in CDLoader.HouseObjectDefinitions)
            {
                foreach(HouseObjectStateDefinition state in h.states.Values)
                {
                    houseObjects[index--] = new TestHouseObject(h, state.bitmap, state.id.ToString());
                }
                houseObjects[index--] = floor;
                /*
                HouseObjectStateDefinition state0, state100;
                if(h.states.TryGetValue(100, out state100))
                {
                    state0 = h.states[0];
                    for (int i = 0; i < 32; i++)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            Color color1 = state0.bitmap.GetPixel(j, i);
                            Color color2 = state100.bitmap.GetPixel(j, i);
                            Int32 newRed = color1.R + color2.R;
                            Int32 newGreen = color1.G + color2.G;
                            Int32 newBlue = color1.B + color2.B;
                            if (newRed > 255) newRed = 255;
                            if (newGreen > 255) newGreen = 255;
                            if (newBlue > 255) newBlue = 255;
                            state0.bitmap.SetPixel(j, i, Color.FromArgb(0xFF,
                                newRed, newGreen, newBlue));
                        }
                    }
                    houseObjects[index--] = new TestHouseObject(h, state0.bitmap);
                }
                //
            }
        */

        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            selectServer.Stop();
        }


        /*
        class TestHouseObject : HouseObjectDefinition
        {
            public readonly Bitmap bitmap;
            public readonly String extra;
            public TestHouseObject(HouseObjectDefinition other, Bitmap bitmap, String extra)
                : base(other.id, other.pathName, other.states)
            {
                this.bitmap = bitmap;
                this.extra = extra;
            }
            public override Bitmap Bitmap
            {
                get
                {
                    return bitmap;
                }
            }
            public override string Extra
            {
                get
                {
                    return extra;
                }
            }
        }
        */


        public void SetTileSize(Int32 tileSize)
        {
            this.tileSize = tileSize;
            this.quarterTileSize = tileSize / 4;
            this.goalPen = new Pen(Color.GreenYellow, quarterTileSize);


            for (int i = 0; i < tileBoxSets.Length; i++)
            {
                TileBoxSet tileBoxSet = tileBoxSets[i];
                tileBoxSet.Location = new Point(tileBoxSet.x * tileSize,
                    tileBoxSet.y * tileSize);
                tileBoxSet.Size = new Size(tileBoxSet.size * tileSize,
                    tileBoxSet.size * tileSize);
                tileBoxSet.Invalidate();
            }
            Invalidate();
        }


        internal void MouseOverHouseObject(int houseObjectOffset)
        {
            HouseObjectDefinition houseObjectDefinition = houseObjects[houseObjectOffset];
            if (houseObjectDefinition != this.currentMouseOverObject)
            {
                this.currentMouseOverObject = houseObjectDefinition;
                this.label.Text = houseObjectDefinition.pathName;
                this.label.Invalidate();
            }
        }
        public void LoadMap(HouseObjectDefinition[] houseObjects)
        {
            Array.Copy(houseObjects, this.houseObjects, houseObjects.Length);
            for (int i = 0; i < tileBoxSets.Length; i++)
            {
                tileBoxSets[i].Invalidate();
            }
        }
    }

    public class GameClientAcceptor
    {
        readonly EndPoint cdServerEndPoint;
        readonly ISocketConnector connector;

        public TcpSelectServer2 selectServer;
        UInt32 nextID;
        public GameClientAcceptor(EndPoint cdServerEndPoint, ISocketConnector connector)
        {
            this.cdServerEndPoint = cdServerEndPoint;
            this.connector = connector;
        }
        public SocketDataHandler Accept(Socket clientSocket)
        {
            Socket serverSocket = new Socket(cdServerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (connector == null)
            {
                serverSocket.Connect(cdServerEndPoint);
            }
            else
            {
                connector.Connect(serverSocket, cdServerEndPoint);
            }

            GameClientDataHandler dataHandler = new GameClientDataHandler(nextID, clientSocket, serverSocket);
            selectServer.AddDataSocket(serverSocket, dataHandler.DataFromServer);

            //Console.WriteLine("[INFO] New Client/Server {0}", nextID);
            nextID++;
            return dataHandler.DataFromClient;
        }
    }

    public class GameClientDataHandler
    {
        static Boolean printTraffic = false;

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
                if (printTraffic) if (rest != null) Console.WriteLine("[SERVER {0}] {1}", id, rest);

                if(printTraffic) Console.WriteLine("[SERVER {0}] [Closed]", id);
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
                if (printTraffic) Console.WriteLine("[SERVER {0}] {1}", id, line);

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
                if (printTraffic) if (rest != null) Console.WriteLine("[CLIENT {0}] {1}", id, rest);

                if (printTraffic) Console.WriteLine("[CLIENT {0}] [Closed]", id);
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
                if (printTraffic) Console.WriteLine("[CLIENT {0}] {1}", id, line);

                // Search for 'action=start_rob_house'
                if (serverParseState == ServerParseState.Idle)
                {
                    if (line.Contains(RobAction))
                    {
                        Int32 encryptionKeyIndex = line.IndexOf(MapEncryption);
                        if (encryptionKeyIndex < 0) throw new FormatException(String.Format("Client Request had '{0}' but not '{1}'", RobAction, MapEncryption));

                        nextMapEncryptionKey = new Byte[40];
                        Encoding.ASCII.GetBytes(line, (Int32)(encryptionKeyIndex + MapEncryption.Length), 40,
                            nextMapEncryptionKey, 0);

                        /*
                        Console.Write("[INFO] Rob House key= (Byte array of characters) '");
                        for (int i = 0; i < nextMapEncryptionKey.Length; i++)
                        {
                            Console.Write((Char)nextMapEncryptionKey[i]);
                        }
                        Console.WriteLine("'");
                        */
                        serverParseState = ServerParseState.BlankLine;
                    }
                }
            }

            return DataFromClient;
        }
    }
}
