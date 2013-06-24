using System;
using System.Collections.Generic;
using System.Text;

using HP.Libraries.Npc;

namespace TestNamespace
{

    [NpcInterface]
    public interface DeviceNpcInterface
    {
        void SetId(UInt32 id);
        UInt32 GetId();

        void SetName(String name);
        String GetName();

        void SetVersion(Byte[] version);
        Byte[] GetVersion();

        void SaveStrings(String[] someStrings);
        String[] GetStrings();

        void Overloaded();
        void Overloaded(Int32 value);
        void Overloaded(Int32 value, Boolean value2);


        void ThrowAnException(String message);


        DateTime GetDate();
        void ToNormalDate();
        void OverrideDate(Byte month, Byte day, UInt16 year);
        void OverrideDate(DateTime date);
    }


    public class Device : DeviceNpcInterface
    {
        private UInt32 id;
        private String name;
        private Byte[] version;
        private String[] someStrings;

        private Boolean overrideDateTime;
        private DateTime dateTimeOverride;


        public void SetId(UInt32 id)
        {
            this.id = id;
        }
        public UInt32 GetId()
        {
            return this.id;
        }

        public void SetName(String name)
        {
            this.name = name;
        }
        public String GetName()
        {
            return name;
        }

        public void SetVersion(Byte[] version)
        {
            this.version = version;
        }
        public Byte[] GetVersion()
        {
            return version;
        }

        public void SaveStrings(String[] someStrings)
        {
            this.someStrings = someStrings;
        }
        public String[] GetStrings()
        {
            return someStrings;
        }

        public void ThrowAnException(string message)
        {
            throw new Exception(message);
        }



        public DateTime GetDate()
        {
            if (overrideDateTime) return dateTimeOverride;
            return DateTime.Now;
        }
        public void ToNormalDate()
        {
            overrideDateTime = false;
        }

        public void OverrideDate(Byte month, Byte day, UInt16 year)
        {
            overrideDateTime = true;
            this.dateTimeOverride = new DateTime(year, month, day);
        }
        public void OverrideDate(DateTime date)
        {
            overrideDateTime = true;
            this.dateTimeOverride = date;
        }

        public void Overloaded()
        {
        }

        public void Overloaded(int value)
        {
        }

        public void Overloaded(int value, bool value2)
        {
        }
    }
}