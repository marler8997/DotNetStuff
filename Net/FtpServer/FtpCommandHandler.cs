using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace More.Net
{
    public interface IFtpCommandHandler
    {
        void HandleCommand(StringBuilder resposeBuilder, String commandUpperCase, String commandArgs);
    }


    public class DictionaryCommandHandler : IFtpCommandHandler
    {
        delegate void CommandHandler(StringBuilder responseBuilder, String args);
        delegate void DataHandler(Socket socket);

        private readonly Dictionary<String,CommandHandler> dictionary;

        public readonly IPEndPoint clientSocketLocalEndPoint;
        public readonly String passiveAddressString;

        public readonly String rootDirectoryServerSystemFormat;
        public readonly String rootDirectoryFtpFormat;

        private String currentSubdirectoryFromRootSystemFormat;
        //private String currentSubdirectoryFromRootFtpFormat;

        private Ftp.TransferType currentTransferType;
        private Ftp.TransferType2 currentTransferType2;
        private Boolean receivedTypeCommand;

        private Socket currentPassiveListenSocket;
        private EventWaitHandle passiveWaitHandle;
        private DataHandler passiveDataHandler;

        public DictionaryCommandHandler(IPEndPoint clientSocketLocalEndPoint, String rootDirectoryServerSystemFormat)
        {
            this.dictionary = new Dictionary<String,CommandHandler>();
            this.dictionary.Add("ABOR",ABOR);
            this.dictionary.Add("ACCT",ACCT);
            this.dictionary.Add("ADAT",ADAT);
            this.dictionary.Add("ALLO",ALLO);
            this.dictionary.Add("APPE",APPE);
            this.dictionary.Add("AUTH",AUTH);
            this.dictionary.Add("CCC",CCC);
            this.dictionary.Add("CDUP",CDUP);
            this.dictionary.Add("CONF",CONF);
            this.dictionary.Add("CWD",CWD);
            this.dictionary.Add("DELE",DELE);
            this.dictionary.Add("ENC",ENC);
            this.dictionary.Add("EPRT",EPRT);
            this.dictionary.Add("EPSV",EPSV);
            this.dictionary.Add("FEAT",FEAT);
            this.dictionary.Add("HELP",HELP);
            this.dictionary.Add("LANG",LANG);
            this.dictionary.Add("LIST",LIST);
            this.dictionary.Add("LPRT",LPRT);
            this.dictionary.Add("LPSV",LPSV);
            this.dictionary.Add("MDTM",MDTM);
            this.dictionary.Add("MIC",MIC);
            this.dictionary.Add("MKD",MKD);
            this.dictionary.Add("MLSD",MLSD);
            this.dictionary.Add("MLST",MLST);
            this.dictionary.Add("MODE",MODE);
            this.dictionary.Add("NLST",NLST);
            this.dictionary.Add("NOOP",NOOP);
            this.dictionary.Add("OPTS",OPTS);
            this.dictionary.Add("PASS",PASS);
            this.dictionary.Add("PASV",PASV);
            this.dictionary.Add("PBSZ",PBSZ);
            this.dictionary.Add("PORT",PORT);
            this.dictionary.Add("PROT",PROT);
            this.dictionary.Add("PWD",PWD);
            this.dictionary.Add("QUIT",QUIT);
            this.dictionary.Add("REIN",REIN);
            this.dictionary.Add("REST",REST);
            this.dictionary.Add("RETR",RETR);
            this.dictionary.Add("RMD",RMD);
            this.dictionary.Add("RNFR",RNFR);
            this.dictionary.Add("RNTO",RNTO);
            this.dictionary.Add("SITE",SITE);
            this.dictionary.Add("SIZE",SIZE);
            this.dictionary.Add("SMNT",SMNT);
            this.dictionary.Add("STAT",STAT);
            this.dictionary.Add("STOR",STOR);
            this.dictionary.Add("STOU",STOU);
            this.dictionary.Add("STRU",STRU);
            this.dictionary.Add("SYST",SYST);
            this.dictionary.Add("TYPE",TYPE);
            this.dictionary.Add("USER",USER);
            this.dictionary.Add("XCUP",XCUP);
            this.dictionary.Add("XMKD",XMKD);
            this.dictionary.Add("XPWD",XPWD);
            this.dictionary.Add("XRCP",XRCP);
            this.dictionary.Add("XRMD",XRMD);
            this.dictionary.Add("XRSQ",XRSQ);
            this.dictionary.Add("XSEM",XSEM);
            this.dictionary.Add("XSEN", XSEN);

            this.clientSocketLocalEndPoint = clientSocketLocalEndPoint;
            Byte [] clientSocketLocalAddressBytes = clientSocketLocalEndPoint.Address.GetAddressBytes();
            this.passiveAddressString = String.Format("{0},{1},{2},{3}", clientSocketLocalAddressBytes[0],
                clientSocketLocalAddressBytes[1], clientSocketLocalAddressBytes[2], clientSocketLocalAddressBytes[3]);

            this.rootDirectoryServerSystemFormat = rootDirectoryServerSystemFormat;
            this.rootDirectoryFtpFormat = PathExtensions.SystemPathToUrlPath(rootDirectoryServerSystemFormat);
            this.currentSubdirectoryFromRootSystemFormat = null;

            this.receivedTypeCommand = false;

            //
            // Passive State Variables
            //
            this.currentPassiveListenSocket = null;
            this.passiveWaitHandle = null;
            this.passiveDataHandler = null;
        }

        private void PassiveConnectionListener()
        {
            if(this.currentPassiveListenSocket == null) throw new InvalidOperationException();
            Socket listenSocket = currentPassiveListenSocket;
            this.currentPassiveListenSocket = null;

            try
            {
                while (true)
                {
                    Socket client = listenSocket.Accept();
                    this.passiveWaitHandle.WaitOne();

                }
            }
            finally
            {
                this.passiveWaitHandle = null;
                if (listenSocket.Connected)
                {
                    try
                    {
                        listenSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception) { }
                }
                listenSocket.Close();
            }
        }


        public void HandleCommand(StringBuilder resposeBuilder, String commandUpperCase, String commandArgs)
        {
            CommandHandler handler;
            if (dictionary.TryGetValue(commandUpperCase, out handler))
            {
                lock (dictionary)
                {
                    handler(resposeBuilder, commandArgs);
                }
            }
        }

        void ABOR(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void ACCT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void ADAT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void ALLO(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void APPE(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void AUTH(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void CCC(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void CDUP(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void CONF(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void CWD(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void DELE(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void ENC(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void EPRT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void EPSV(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void FEAT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void HELP(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void LANG(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void LIST(StringBuilder responseBuilder, String args)
        {
            if (passiveWaitHandle == null)
            {
                responseBuilder.Append("503 You must either send PASV or PORT command first.\r\n");
                return;
            }

            passiveDataHandler = (Socket s) =>
            {
                s.Send(Encoding.UTF8.GetBytes("hello\r\n"));
            };
        }
        void LPRT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void LPSV(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void MDTM(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void MIC(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void MKD(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void MLSD(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void MLST(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void MODE(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void NLST(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void NOOP(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void OPTS(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void PASS(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void PASV(StringBuilder responseBuilder, String args)
        {
            if (currentPassiveListenSocket == null)
            {
                this.currentPassiveListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.currentPassiveListenSocket.Bind(new IPEndPoint(clientSocketLocalEndPoint.Address, 0));
                
                IPEndPoint localEndPoint = (IPEndPoint)this.currentPassiveListenSocket.LocalEndPoint;
                Byte[] localAddress = localEndPoint.Address.GetAddressBytes();
                Int32 localPort = localEndPoint.Port;

                this.passiveWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                this.passiveDataHandler = null;

                this.currentPassiveListenSocket.Listen(1);
                new Thread(PassiveConnectionListener).Start();

                responseBuilder.Append(String.Format("227 Entering Passive Mode ({0},{1},{2}).\r\n",
                    passiveAddressString, (Byte)(localPort>>8), (Byte)localPort));
            }
            else
            {
                responseBuilder.Append("503 Already listening.\r\n");
            }
        }
        void PBSZ(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void PORT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void PROT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void PWD(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append(String.Format("200 \"{0}\" is current directory.\r\n",
                (currentSubdirectoryFromRootSystemFormat == null) ? rootDirectoryFtpFormat :
                PathExtensions.SystemPathToUrlPath(Path.Combine(rootDirectoryServerSystemFormat, currentSubdirectoryFromRootSystemFormat))));
        }
        void QUIT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void REIN(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void REST(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void RETR(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void RMD(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void RNFR(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void RNTO(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void SITE(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void SIZE(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void SMNT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void STAT(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void STOR(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void STOU(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void STRU(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void SYST(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void TYPE(StringBuilder responseBuilder, String args)
        {
            if (String.IsNullOrEmpty(args)) return;

            switch (args[0])
            {
                case 'A':
                    this.currentTransferType = Ftp.TransferType.Ascii;
                    if (args.Length > 1)
                    {
                        responseBuilder.Append("502 Not Implemented.\r\n");
                        return;
                    }
                    responseBuilder.Append("200 Type set to A.\r\n");
                    this.receivedTypeCommand = true;
                    break;
                case 'E':
                    this.currentTransferType = Ftp.TransferType.Ebcdic;
                    if (args.Length > 1)
                    {
                        responseBuilder.Append("502 Not Implemented.\r\n");
                        return;
                    }
                    responseBuilder.Append("200 Type set to E.\r\n");
                    this.receivedTypeCommand = true;
                    break;
                case 'I':
                    this.currentTransferType = Ftp.TransferType.Image;
                    responseBuilder.Append("200 Type set to I.\r\n");
                    this.receivedTypeCommand = true;
                    break;
                case 'L':
                    this.currentTransferType = Ftp.TransferType.Local;
                    responseBuilder.Append("200 Type set to L.\r\n");
                    this.receivedTypeCommand = true;
                    break;
            }
        }
        void USER(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("230 Welcome to the super simple FTP Server\r\n230 User ");
            responseBuilder.Append(args);
            responseBuilder.Append(" logged in.\r\n");
        }
        void XCUP(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void XMKD(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void XPWD(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void XRCP(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void XRMD(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void XRSQ(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void XSEM(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
        void XSEN(StringBuilder responseBuilder, String args)
        {
            responseBuilder.Append("502 Not Implemented.\r\n");
        }
    }



}
