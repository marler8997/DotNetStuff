using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Marler.Hmd
{
    /// <summary>
    /// Summary description for HighLevelTests
    /// </summary>
    [TestClass]
    public class HighLevelTests
    {
        [TestMethod]
        public void TestConcreteExample()
        {
            //
            // Parse Properties File
            //
            TextReader propertiesReader = new System.IO.StringReader("%enum:ExecuteEnum Local Remote Ignore Unsupported;Script{%props:1;Source:enum(File Remote);File:0-1;ListenPort:0-1 int4;}RebootCommand {%props:1;Execute:1 enum(Ignore Unsupported Emulator);RemoteHost:0-1;RemotePort:0-1 int4;}UsbSwitchCommand {	%props:1;	Execute:1 enum ExecuteEnum;	Platform:0-1 enum(Windows Linux);	RemoteHost:0-1;	RemotePort:0-1 int4;}TestCommands {	%props:1;	Execute:1 enum ExecuteEnum;	RemoteHost:0-1;	RemotePort:0-1 int4;}");
            HmdProperties properties = HmdFileParser.ParsePropertiesFile(propertiesReader, null);
            properties.ResolveChildParentReferences();
            properties.Print(Console.Out);
            

            HmdEnum hmdEnum;

            hmdEnum = properties.TryGetEnum("ExecuteEnum");
            Assert.IsNotNull(hmdEnum);
            Assert.IsTrue(hmdEnum.IsValidEnumValue("local"));
            Assert.IsTrue(hmdEnum.IsValidEnumValue("remote"));
            Assert.IsTrue(hmdEnum.IsValidEnumValue("IGNORE"));
            Assert.IsTrue(hmdEnum.IsValidEnumValue("unsupported"));

            hmdEnum = properties.TryGetEnum("script.source");
            Assert.IsNotNull(hmdEnum);
            Assert.IsTrue(hmdEnum.IsValidEnumValue("FILE"));
            Assert.IsTrue(hmdEnum.IsValidEnumValue("remote"));

            HmdBlockID fileRoot = new HmdBlockID("THE_ROOT!", null);
            
            HmdBlockID script = new HmdBlockID("script", fileRoot);
            Assert.IsNotNull(properties.GetProperties(script));

            HmdValueID scriptSource = new HmdValueID("source", "file", script);
            Assert.IsNotNull(properties.GetProperties(scriptSource));
            




            //
            // Parse Hmd File
            //
            TextReader hmdReader = new System.IO.StringReader("Script {Source:Remote;}RebootCommand {Execute:Unsupported;}UsbSwitchCommand {Execute:Unsupported;}TestCommands {Execute:Local;}");
            HmdBlockID rootID = new HmdBlockID(String.Empty, null);
            HmdFileParser.Parse(rootID, hmdReader, "", null);


            //HmdValidator.ValidateStatic(rootID, properties);

        }
    }
}
