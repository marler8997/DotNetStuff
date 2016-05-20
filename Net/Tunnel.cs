using System;
using System.Collections.Generic;
using System.Text;

namespace More.Net
{
    public interface IPortTunnel
    {
        SortedNumberSet FullPortSet { get; }
        Boolean IsValidTunnel(UInt16 port1, UInt16 port2);
    }

    //
    // TODO: Make the naming better, distinguish between Tunnes that loop back on themselve and
    //       ones that have different input/outputs
    //
    // Also implement a * tunnel:)
    //

    public class TunnelOnSinglePort : IPortTunnel
    {
        public readonly UInt16 port;
        public readonly SortedNumberSet fullPortSet;

        public TunnelOnSinglePort(UInt16 port)
        {
            this.port = port;
            this.fullPortSet = new SortedNumberSet();
            this.fullPortSet.Add(port);
        }

        SortedNumberSet IPortTunnel.FullPortSet { get { return fullPortSet; } }

        Boolean IPortTunnel.IsValidTunnel(UInt16 port1, UInt16 port2)
        {
            return ((this.port == port1) && (this.port == port2));
        }

        public override String ToString()
        {
            return port.ToString();
        }
    }

    public class TunnelOneInOneOut : IPortTunnel
    {
        public readonly UInt16 port1, port2;
        public readonly SortedNumberSet fullPortSet;

        public TunnelOneInOneOut(UInt16 port1, UInt16 port2)
        {
            if (port1 == port2) throw new ArgumentException(String.Format("port1 and port2 ({0}) cannot be the same, you must have wanted a TunnelOnSinglePort", port1));
            this.port1 = port1;
            this.port2 = port2;
            this.fullPortSet = new SortedNumberSet();
            this.fullPortSet.Add(port1);
            this.fullPortSet.Add(port2);
        }

        SortedNumberSet IPortTunnel.FullPortSet { get { return fullPortSet; } }

        Boolean IPortTunnel.IsValidTunnel(UInt16 port1, UInt16 port2)
        {
            return (
                (this.port1 == port1) ? (this.port2 == port2) :
                ((this.port1 == port2) && (this.port2 == port1))
                );
        }

        public override String ToString()
        {
            return String.Format("{0}-{1}", port1, port2);
        }
    }
    /*
    public class TunnelOneInMultipleOut : IPortTunnel
    {
        public readonly UInt16 port;
        public readonly SortedNumberSet portSet;
        public readonly SortedNumberSet fullPortSet;

        public TunnelOneInMultipleOut(UInt16 port, SortedNumberSet portSet)
        {
            if (portSet == null) throw new ArgumentNullException("portSet");
            this.port = port;
            this.portSet = portSet;
            this.fullPortSet = portSet.Combine(port);
        }

        SortedNumberSet IPortTunnel.FullPortSet { get { return fullPortSet; } }

        Boolean IPortTunnel.IsValidTunnel(UInt16 port1, UInt16 port2)
        {
            if (this.port == port1)
            {
                return portSet.Contains(port2);
            }

            return (this.port == port2) ? portSet.Contains(port1) : false;
        }

        public override String ToString()
        {
            return String.Format("{0}-{1}", port, portSet);
        }
    }

    public class TunnelMultipleInAndOut : IPortTunnel
    {
        public readonly SortedNumberSet portSet1, portSet2;
        public readonly SortedNumberSet fullPortSet;

        public TunnelMultipleInAndOut(SortedNumberSet portSet1, SortedNumberSet portSet2)
        {
            if (portSet1 == null) throw new ArgumentNullException("portSet1");
            if (portSet2 == null) throw new ArgumentNullException("portSet2");
            this.portSet1 = portSet1;
            this.portSet2 = portSet2;
            this.fullPortSet = portSet1.Combine(portSet2);
        }

        SortedNumberSet IPortTunnel.FullPortSet { get { return fullPortSet; } }

        Boolean IPortTunnel.IsValidTunnel(UInt16 port1, UInt16 port2)
        {
            if (portSet1.Contains(port1))
            {
                if (portSet2.Contains(port2)) return true;
            }
            return portSet2.Contains(port1) ? portSet1.Contains(port2) : false;
        }

        public override String ToString()
        {
            return String.Format("{0}-{1}", portSet1, portSet2);
        }
    }
    */
}
