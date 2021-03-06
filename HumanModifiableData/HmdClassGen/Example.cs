/*
 * This file was autogenerated by HmdClassGen.exe
 *
 * PropertyDictionary:
Enums Defined:
   %root.usbswitch: off port1 port2 port3 port4
   usbdevice.legacyname: dsk ghd cat kbd scr mou
   usbdevice.speed: full high
Value Props:
[HmdValueID Message DefinitionContext= Type=String Count=0-* Parents=(%root)]
[HmdValueID UsbSwitch DefinitionContext= Type=Enumeration(%root.usbswitch) Count=0-* Parents=(%root)]
[HmdValueID Sleep DefinitionContext= Type=UInt4 Count=0-* Parents=(%root)]
[HmdValueID LegacyName DefinitionContext=usbdevice Type=Enumeration(usbdevice.legacyname) Count=1 Parents=(UsbDevice)]
[HmdValueID Protocol DefinitionContext=usbdevice Type=UInt1 Count=1 Parents=(UsbDevice)]
[HmdValueID SubClass DefinitionContext=usbdevice Type=UInt1 Count=1 Parents=(UsbDevice)]
[HmdValueID Class DefinitionContext=usbdevice Type=UInt1 Count=1 Parents=(UsbDevice)]
[HmdValueID Interface DefinitionContext=usbdevice Type=UInt1 Count=1 Parents=(UsbDevice)]
[HmdValueID SerialNumber DefinitionContext=usbdevice Type=String Count=1 Parents=(UsbDevice)]
[HmdValueID Product DefinitionContext=usbdevice Type=String Count=1 Parents=(UsbDevice)]
[HmdValueID Manufacturer DefinitionContext=usbdevice Type=String Count=1 Parents=(UsbDevice)]
[HmdValueID ProductID DefinitionContext=usbdevice Type=UInt2 Count=1 Parents=(UsbDevice)]
[HmdValueID VendorID DefinitionContext=usbdevice Type=UInt2 Count=1 Parents=(UsbDevice)]
[HmdValueID Speed DefinitionContext=usbdevice Type=Enumeration(usbdevice.speed) Count=1 Parents=(UsbDevice)]
[HmdValueID PortName DefinitionContext=usbdevice Type=String Count=0-1 Parents=(UsbDevice)]

Block Props:
[HmdBlockID UsbDevice DefinitionContext= Count=0-* Parents=(VerifyDevicesArePresent [HmdBlockID VerifyDevicesAreNotPresent DefinitionContext= Count=0-* Parents=(%root) Children=[UsbDevice]]) Children=[LegacyName Protocol SubClass Class Interface SerialNumber Product Manufacturer ProductID VendorID Speed PortName]]
[HmdBlockID VerifyDevicesArePresent DefinitionContext= Count=0-* Parents=(%root) Children=[UsbDevice]]
[HmdBlockID VerifyDevicesAreNotPresent DefinitionContext= Count=0-* Parents=(%root) Children=[UsbDevice]]

Extra Value Links:
verifydevicesarepresent.usbdevice.legacyname
verifydevicesarepresent.usbdevice.protocol
verifydevicesarepresent.usbdevice.subclass
verifydevicesarepresent.usbdevice.class
verifydevicesarepresent.usbdevice.interface
verifydevicesarepresent.usbdevice.serialnumber
verifydevicesarepresent.usbdevice.product
verifydevicesarepresent.usbdevice.manufacturer
verifydevicesarepresent.usbdevice.productid
verifydevicesarepresent.usbdevice.vendorid
verifydevicesarepresent.usbdevice.speed
verifydevicesarepresent.usbdevice.portname
verifydevicesarenotpresent.usbdevice.legacyname
verifydevicesarenotpresent.usbdevice.protocol
verifydevicesarenotpresent.usbdevice.subclass
verifydevicesarenotpresent.usbdevice.class
verifydevicesarenotpresent.usbdevice.interface
verifydevicesarenotpresent.usbdevice.serialnumber
verifydevicesarenotpresent.usbdevice.product
verifydevicesarenotpresent.usbdevice.manufacturer
verifydevicesarenotpresent.usbdevice.productid
verifydevicesarenotpresent.usbdevice.vendorid
verifydevicesarenotpresent.usbdevice.speed
verifydevicesarenotpresent.usbdevice.portname

Extra Block Links:
verifydevicesarepresent.usbdevice
verifydevicesarenotpresent.usbdevice
 *
 */
using System;
using System.Collections.Generic;

namespace Marler.Hmd
{
   public enum usbswitch {off, port1, port2, port3, port4};
   public enum usbdevicelegacyname {dsk, ghd, cat, kbd, scr, mou};
   public enum usbdevicespeed {full, high};
   public class HmdTypeusbdevice
   {
      public usbdevicelegacyname LegacyName;
      public Byte Protocol;
      public Byte SubClass;
      public Byte Class;
      public Byte Interface;
      public String SerialNumber;
      public String Product;
      public String Manufacturer;
      public UInt16 ProductID;
      public UInt16 VendorID;
      public usbdevicespeed Speed;
      public String PortName;
      public HmdTypeusbdevice(HmdBlockID blockID, HmdProperties hmdProperties)
      {
         for(int i = 0; i < blockID.ChildCount; i++)
         {
            HmdID childID = blockID.GetChild(i);
            if(childID.isBlock)
            {
               HmdBlockID childBlockID = (HmdBlockID)childID;
               throw new FormatException(String.Format("Unrecognized child block id \"{0}\"",childID.idOriginalCase));
            }
            else
            {
               HmdValueID childValueID = (HmdValueID)childID;
               // parse field LegacyName
               if(childValueID.idLowerCase.Equals("legacyname",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.LegacyName != null)
                  {
                     throw new FormatException("Found multiple value id's \"LegacyName\"");
                  }
                  this.LegacyName = (usbdevicelegacyname)Enum.Parse(typeof(usbdevicelegacyname),childValueID.value,true);
               }
               // parse field Protocol
               else if(childValueID.idLowerCase.Equals("protocol",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.Protocol != null)
                  {
                     throw new FormatException("Found multiple value id's \"Protocol\"");
                  }
                  this.Protocol = Byte.Parse(childValueID.value);
               }
               // parse field SubClass
               else if(childValueID.idLowerCase.Equals("subclass",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.SubClass != null)
                  {
                     throw new FormatException("Found multiple value id's \"SubClass\"");
                  }
                  this.SubClass = Byte.Parse(childValueID.value);
               }
               // parse field Class
               else if(childValueID.idLowerCase.Equals("class",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.Class != null)
                  {
                     throw new FormatException("Found multiple value id's \"Class\"");
                  }
                  this.Class = Byte.Parse(childValueID.value);
               }
               // parse field Interface
               else if(childValueID.idLowerCase.Equals("interface",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.Interface != null)
                  {
                     throw new FormatException("Found multiple value id's \"Interface\"");
                  }
                  this.Interface = Byte.Parse(childValueID.value);
               }
               // parse field SerialNumber
               else if(childValueID.idLowerCase.Equals("serialnumber",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.SerialNumber != null)
                  {
                     throw new FormatException("Found multiple value id's \"SerialNumber\"");
                  }
                  this.SerialNumber = childValueID.value;
               }
               // parse field Product
               else if(childValueID.idLowerCase.Equals("product",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.Product != null)
                  {
                     throw new FormatException("Found multiple value id's \"Product\"");
                  }
                  this.Product = childValueID.value;
               }
               // parse field Manufacturer
               else if(childValueID.idLowerCase.Equals("manufacturer",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.Manufacturer != null)
                  {
                     throw new FormatException("Found multiple value id's \"Manufacturer\"");
                  }
                  this.Manufacturer = childValueID.value;
               }
               // parse field ProductID
               else if(childValueID.idLowerCase.Equals("productid",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.ProductID != null)
                  {
                     throw new FormatException("Found multiple value id's \"ProductID\"");
                  }
                  this.ProductID = UInt16.Parse(childValueID.value);
               }
               // parse field VendorID
               else if(childValueID.idLowerCase.Equals("vendorid",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.VendorID != null)
                  {
                     throw new FormatException("Found multiple value id's \"VendorID\"");
                  }
                  this.VendorID = UInt16.Parse(childValueID.value);
               }
               // parse field Speed
               else if(childValueID.idLowerCase.Equals("speed",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.Speed != null)
                  {
                     throw new FormatException("Found multiple value id's \"Speed\"");
                  }
                  this.Speed = (usbdevicespeed)Enum.Parse(typeof(usbdevicespeed),childValueID.value,true);
               }
               // parse field PortName
               else if(childValueID.idLowerCase.Equals("portname",StringComparison.CurrentCultureIgnoreCase))
               {
                  // check that field is not set already
                  if(this.PortName != null)
                  {
                     throw new FormatException("Found multiple value id's \"PortName\"");
                  }
                  this.PortName = childValueID.value;
               }
               else
               {
                  throw new FormatException(String.Format("Unrecognized child value id \"{0}\"",childID.idOriginalCase));
               }
            }
         }
      }
   }
   public class HmdTypeverifydevicesarepresent
   {
      public List<HmdTypeusbdevice> UsbDevice;
      public HmdTypeverifydevicesarepresent(HmdBlockID blockID, HmdProperties hmdProperties)
      {
         for(int i = 0; i < blockID.ChildCount; i++)
         {
            HmdID childID = blockID.GetChild(i);
            if(childID.isBlock)
            {
               HmdBlockID childBlockID = (HmdBlockID)childID;
               // parse field UsbDevice
               if(childBlockID.idLowerCase.Equals("usbdevice",StringComparison.CurrentCultureIgnoreCase))
               {
                  // set List to not null
                  this.UsbDevice.Add(new HmdTypeusbdevice(childBlockID, hmdProperties));
               }
               else
               {
                  throw new FormatException(String.Format("Unrecognized child block id \"{0}\"",childID.idOriginalCase));
               }
            }
            else
            {
               HmdValueID childValueID = (HmdValueID)childID;
               throw new FormatException(String.Format("Unrecognized child value id \"{0}\"",childID.idOriginalCase));
            }
         }
      }
   }
   public class HmdTypeverifydevicesarenotpresent
   {
      public List<HmdTypeusbdevice> UsbDevice;
      public HmdTypeverifydevicesarenotpresent(HmdBlockID blockID, HmdProperties hmdProperties)
      {
         for(int i = 0; i < blockID.ChildCount; i++)
         {
            HmdID childID = blockID.GetChild(i);
            if(childID.isBlock)
            {
               HmdBlockID childBlockID = (HmdBlockID)childID;
               // parse field UsbDevice
               if(childBlockID.idLowerCase.Equals("usbdevice",StringComparison.CurrentCultureIgnoreCase))
               {
                  // set List to not null
                  this.UsbDevice.Add(new HmdTypeusbdevice(childBlockID, hmdProperties));
               }
               else
               {
                  throw new FormatException(String.Format("Unrecognized child block id \"{0}\"",childID.idOriginalCase));
               }
            }
            else
            {
               HmdValueID childValueID = (HmdValueID)childID;
               throw new FormatException(String.Format("Unrecognized child value id \"{0}\"",childID.idOriginalCase));
            }
         }
      }
   }
   public class DefaultRootClassName
   {
      public List<String> Message;
      public List<usbswitch> UsbSwitch;
      public List<UInt32> Sleep;
      public List<HmdTypeverifydevicesarepresent> VerifyDevicesArePresent;
      public List<HmdTypeverifydevicesarenotpresent> VerifyDevicesAreNotPresent;
      public DefaultRootClassName(HmdBlockID blockID, HmdProperties hmdProperties)
      {
         for(int i = 0; i < blockID.ChildCount; i++)
         {
            HmdID childID = blockID.GetChild(i);
            if(childID.isBlock)
            {
               HmdBlockID childBlockID = (HmdBlockID)childID;
               // parse field VerifyDevicesArePresent
               if(childBlockID.idLowerCase.Equals("verifydevicesarepresent",StringComparison.CurrentCultureIgnoreCase))
               {
                  // set List to not null
                  this.VerifyDevicesArePresent.Add(new HmdTypeverifydevicesarepresent(childBlockID, hmdProperties));
               }
               // parse field VerifyDevicesAreNotPresent
               else if(childBlockID.idLowerCase.Equals("verifydevicesarenotpresent",StringComparison.CurrentCultureIgnoreCase))
               {
                  // set List to not null
                  this.VerifyDevicesAreNotPresent.Add(new HmdTypeverifydevicesarenotpresent(childBlockID, hmdProperties));
               }
               else
               {
                  throw new FormatException(String.Format("Unrecognized child block id \"{0}\"",childID.idOriginalCase));
               }
            }
            else
            {
               HmdValueID childValueID = (HmdValueID)childID;
               // parse field Message
               if(childValueID.idLowerCase.Equals("message",StringComparison.CurrentCultureIgnoreCase))
               {
                  this.Message.Add(childValueID.value);
               }
               // parse field UsbSwitch
               else if(childValueID.idLowerCase.Equals("usbswitch",StringComparison.CurrentCultureIgnoreCase))
               {
                  this.UsbSwitch.Add((usbswitch)Enum.Parse(typeof(usbswitch),childValueID.value,true));
               }
               // parse field Sleep
               else if(childValueID.idLowerCase.Equals("sleep",StringComparison.CurrentCultureIgnoreCase))
               {
                  this.Sleep.Add(UInt32.Parse(childValueID.value));
               }
               else
               {
                  throw new FormatException(String.Format("Unrecognized child value id \"{0}\"",childID.idOriginalCase));
               }
            }
         }
      }
   }
}
