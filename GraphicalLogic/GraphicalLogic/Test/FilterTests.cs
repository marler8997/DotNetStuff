using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GraphicalLogic
{
    class LineHandlers
    {
        Int32 infoCount, debugCount;

        Int32 snmpLibLineCount;
        Int32 trayCount;
        
        public void GotInfo(String line)
        {
            infoCount++;
        }
        public void GotDebug(String line)
        {
            debugCount++;
        }
        public void GotSnmpLibLine(String line)
        {
            snmpLibLineCount++;
        }
        public void GotTray(String line)
        {
            snmpLibLineCount++;
        }
    }


    [TestClass]
    public class FilterTests
    {


        [TestMethod]
        public void TestMethod1()
        {
            Filter filter = new Filter();
            LineHandlers lineHandlers = new LineHandlers();

            LineHandler[] infoHandler  = new LineHandler[] { lineHandlers.GotInfo };
            LineHandler[] debugHandler = new LineHandler[] { lineHandlers.GotDebug };
            LineHandler[] snmpHandler  = new LineHandler[] { lineHandlers.GotSnmpLibLine };
            LineHandler[] trayhandler  = new LineHandler[] { lineHandlers.GotTray };


            filter.SetFilterHandlers("<INFO>", infoHandler);
            filter.SetFilterHandlers("<DEBUG>", debugHandler);
            filter.SetFilterHandlers("<SnmpLib::", snmpHandler);
            filter.SetFilterHandlers("<Tray::", trayhandler);

            using(FileStream fileStream = new FileStream(@"C:\temp\HP.Test.Framework.log", FileMode.Open))
            {

            }
        }
    }
}
