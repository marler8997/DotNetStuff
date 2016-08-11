using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using HP.Libraries.Npc;

public enum UsbLegacyName {
    DSK = 0,
    GHD = 1,
    CAT = 2,
    KBD = 3,
    SCR = 4,
    MOU = 5,
    Invalid = 6
};

public enum UsbSwitchState
{
    Invalid = -1,
    Off     =  0,
    Port1   =  1,
    Port2   =  2,
    Port3   =  3,
    Port4   =  4
};

[NpcInterface]
public interface IUsbSwitch
{
    UsbSwitchState UsbSwitchGetState();
    void UsbSwitchStateChange(UsbSwitchState state);
}

[NpcInterface]
public interface IUsbHost
{
    UsbDeviceInformation[] GetDevices();
    void AddUsbDevice(UsbDeviceInformation device);
    void RemoveUsbDevice(UsbDeviceInformation device);
    void ClearAllUsbDevices();
    Boolean UsbDeviceIsPresent(UsbDeviceInformation device);
}

public class UsbImpl : IUsbHost, IUsbSwitch
{
    private UsbSwitchState switchState;
    private List<UsbDeviceInformation> devices;

    public UsbImpl()
    {
        this.switchState = UsbSwitchState.Off;
        this.devices = new List<UsbDeviceInformation>();
    }

    public UsbSwitchState UsbSwitchGetState()
    {
        return switchState;
    }

    public void UsbSwitchStateChange(UsbSwitchState switchState)
    {
        this.switchState = switchState;
    }

    public UsbDeviceInformation[] GetDevices()
    {
        return devices.ToArray();
    }

    public void AddUsbDevice(UsbDeviceInformation device)
    {
        if (UsbDeviceIsPresent(device))
        {
            throw new InvalidOperationException(String.Format("Device {0} is already added", device));
        }
        devices.Add(device);
    }

    public void RemoveUsbDevice(UsbDeviceInformation device)
    {
        for (int i = 0; i < devices.Count; i++)
        {
            if (device.Equals(devices[i]))
            {
                devices.RemoveAt(i);
                return;
            }
        }
        throw new InvalidOperationException(String.Format("Device {0} is not present", device));
    }

    public void ClearAllUsbDevices()
    {
        devices.Clear();
    }

    public Boolean UsbDeviceIsPresent(UsbDeviceInformation device)
    {
        foreach (UsbDeviceInformation deviceInList in devices)
        {
            if (device.Equals(deviceInList)) return true;
        }
        return false;
    }
}

public class UsbDeviceInformation
{
    public UsbLegacyName legacyName;
    public Byte @class;
    public String manufacturer;

    public UsbDeviceInformation()
    {
    }

    public UsbDeviceInformation(
        UsbLegacyName legacyName,
        Byte @class,
        String manufacturer)
    {
        this.legacyName = legacyName;
        this.@class = @class;
        this.manufacturer = manufacturer;
    }

    public Boolean Equals(UsbDeviceInformation device)
    {
        return (this.legacyName == device.legacyName &&
            this.@class == device.@class &&
            this.manufacturer.Equals(device.manufacturer));
    }

    public override string ToString()
    {
        return String.Format("UsbDevice(legacyName={0},class={1},manufacturer={2})", legacyName, @class, manufacturer);
    }
}
