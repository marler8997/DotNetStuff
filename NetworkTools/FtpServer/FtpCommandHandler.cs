using System;
using System.Collections.Generic;

namespace Marler.NetworkTools
{
    public interface IFtpCommandHandler
    {
        UInt16 HandleCommand(String command);
    }


    public class DictionaryCommandHandler : IFtpCommandHandler
    {
        delegate UInt16 CommandHandler();

        private readonly Dictionary<String,CommandHandler> dictionary;

        public DictionaryCommandHandler()
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
            this.dictionary.Add("XSEN",XSEN);
        }

        public UInt16 HandleCommand(String command)
        {
            CommandHandler handler;
            if (dictionary.TryGetValue(command, out handler))
            {
                return handler();
            }

            return 500;
        }

        UInt16 ABOR()
        {
            return 500;
        }
        UInt16 ACCT()
        {
            return 500;
        }
        UInt16 ADAT()
        {
            return 500;
        }
        UInt16 ALLO()
        {
            return 500;
        }
        UInt16 APPE()
        {
            return 500;
        }
        UInt16 AUTH()
        {
            return 500;
        }
        UInt16 CCC()
        {
            return 500;
        }
        UInt16 CDUP()
        {
            return 500;
        }
        UInt16 CONF()
        {
            return 500;
        }
        UInt16 CWD()
        {
            return 500;
        }
        UInt16 DELE()
        {
            return 500;
        }
        UInt16 ENC()
        {
            return 500;
        }
        UInt16 EPRT()
        {
            return 500;
        }
        UInt16 EPSV()
        {
            return 500;
        }
        UInt16 FEAT()
        {
            return 500;
        }
        UInt16 HELP()
        {
            return 500;
        }
        UInt16 LANG()
        {
            return 500;
        }
        UInt16 LIST()
        {
            return 500;
        }
        UInt16 LPRT()
        {
            return 500;
        }
        UInt16 LPSV()
        {
            return 500;
        }
        UInt16 MDTM()
        {
            return 500;
        }
        UInt16 MIC()
        {
            return 500;
        }
        UInt16 MKD()
        {
            return 500;
        }
        UInt16 MLSD()
        {
            return 500;
        }
        UInt16 MLST()
        {
            return 500;
        }
        UInt16 MODE()
        {
            return 500;
        }
        UInt16 NLST()
        {
            return 500;
        }
        UInt16 NOOP()
        {
            return 500;
        }
        UInt16 OPTS()
        {
            return 500;
        }
        UInt16 PASS()
        {
            return 500;
        }
        UInt16 PASV()
        {
            return 500;
        }
        UInt16 PBSZ()
        {
            return 500;
        }
        UInt16 PORT()
        {
            return 500;
        }
        UInt16 PROT()
        {
            return 500;
        }
        UInt16 PWD()
        {
            return 500;
        }
        UInt16 QUIT()
        {
            return 500;
        }
        UInt16 REIN()
        {
            return 500;
        }
        UInt16 REST()
        {
            return 500;
        }
        UInt16 RETR()
        {
            return 500;
        }
        UInt16 RMD()
        {
            return 500;
        }
        UInt16 RNFR()
        {
            return 500;
        }
        UInt16 RNTO()
        {
            return 500;
        }
        UInt16 SITE()
        {
            return 500;
        }
        UInt16 SIZE()
        {
            return 500;
        }
        UInt16 SMNT()
        {
            return 500;
        }
        UInt16 STAT()
        {
            return 500;
        }
        UInt16 STOR()
        {
            return 500;
        }
        UInt16 STOU()
        {
            return 500;
        }
        UInt16 STRU()
        {
            return 500;
        }
        UInt16 SYST()
        {
            return 500;
        }
        UInt16 TYPE()
        {
            return 500;
        }
        UInt16 USER()
        {
            return 230;
        }
        UInt16 XCUP()
        {
            return 500;
        }
        UInt16 XMKD()
        {
            return 500;
        }
        UInt16 XPWD()
        {
            return 500;
        }
        UInt16 XRCP()
        {
            return 500;
        }
        UInt16 XRMD()
        {
            return 500;
        }
        UInt16 XRSQ()
        {
            return 500;
        }
        UInt16 XSEM()
        {
            return 500;
        }
        UInt16 XSEN()
        {
            return 500;
        }
    }



}
