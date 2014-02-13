using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace More
{
    public class DeviceStatus
    {
        public Boolean good;
        public DateTime bootTime;
        public UInt32 statusCode;

        public DeviceStatus()
        {
        }
        public DeviceStatus(Boolean good, DateTime bootTime, UInt32 statusCode)
        {
            this.good = good;
            this.bootTime = bootTime;
            this.statusCode = statusCode;
        }
        public override Boolean Equals(Object obj)
        {
            DeviceStatus deviceStatus = obj as DeviceStatus;
            if (deviceStatus == null)
            {
                return false;
            }

            return
                this.good == deviceStatus.good &&
                this.bootTime.Equals(deviceStatus.bootTime) &&
                this.statusCode == deviceStatus.statusCode;
        }
        public override String ToString()
        {
            return String.Format("{0};{1};{2}", good, bootTime, statusCode);
        }

        public static DeviceStatus NpcParse(String str)
        {
            String[] fields = str.Split(';');
            if (fields.Length != 3) throw new FormatException(String.Format("Expected 2 semicolons got {0}", fields.Length - 1));
            return new DeviceStatus(Boolean.Parse(fields[0]), DateTime.Parse(fields[1]), UInt32.Parse(fields[2]));
        }
    }
    [NpcInterface]
    public interface RemoteDeviceNpcInterface
    {
        DeviceStatus GetDeviceStatus();
        void SetDeviceStatus(DeviceStatus deviceStatus);
    }
    public class TestRemoteDevice : RemoteDeviceNpcInterface
    {
        public DeviceStatus deviceStatus;
        public TestRemoteDevice()
        {
            this.deviceStatus = null;
        }

        public DeviceStatus GetDeviceStatus()
        {
            return deviceStatus;
        }
        public void SetDeviceStatus(DeviceStatus deviceStatus)
        {
            this.deviceStatus = deviceStatus;
        }
    }
    [TestClass]
    public class TestUserDefinedTypes
    {
        [TestMethod]
        public void UserDefinedTypesTest()
        {
            TestRemoteDevice remoteDevice = new TestRemoteDevice();

            NpcReflector npcReflector = new NpcReflector(remoteDevice);

            Assert.AreEqual(null, npcReflector.ExecuteWithStrings("TestRemoteDevice.GetDeviceStatus").value);
            DeviceStatus deviceStatus = new DeviceStatus(false, DateTime.MinValue, 38849);
            npcReflector.ExecuteWithStrings("TestRemoteDevice.SetDeviceStatus", deviceStatus.SerializeObject());
            Assert.AreEqual(deviceStatus, (DeviceStatus)npcReflector.ExecuteWithStrings("TestRemoteDevice.GetDeviceStatus").value);
        }
    }
}
