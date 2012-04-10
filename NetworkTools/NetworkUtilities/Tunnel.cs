using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marler.NetworkTools
{
    public interface ITunnel
    {
        PortSet FullPortSet { get; }
        Boolean IsValidTunnel(UInt16 port1, UInt16 port2);
    }

    //
    // TODO: Make the naming better, distinguish between Tunnes that loop back on themselve and
    //       ones that have different input/outputs
    //
    // Also implement a * tunnel:)
    //

    public class TunnelOnSinglePort : ITunnel
    {
        public readonly UInt16 port;
        public readonly PortSetSingle fullPortSet;

        public TunnelOnSinglePort(UInt16 port)
        {
            this.port = port;
            this.fullPortSet = new PortSetSingle(port);
        }

        PortSet ITunnel.FullPortSet { get { return fullPortSet; } }

        Boolean ITunnel.IsValidTunnel(UInt16 port1, UInt16 port2)
        {
            return ((this.port == port1) && (this.port == port2));
        }

        public override String ToString()
        {
            return port.ToString();
        }
    }

    public class TunnelOneInOneOut : ITunnel
    {
        public readonly UInt16 port1, port2;
        public readonly PortSet fullPortSet;

        public TunnelOneInOneOut(UInt16 port1, UInt16 port2)
        {
            if (port1 == port2) throw new ArgumentException(String.Format("port1 and port2 ({0}) cannot be the same, you must have wanted a TunnelOnSinglePort", port1));
            this.port1 = port1;
            this.port2 = port2;
            this.fullPortSet = new PortSetDouble(port1, port2);
        }

        PortSet ITunnel.FullPortSet { get { return fullPortSet; } }

        Boolean ITunnel.IsValidTunnel(UInt16 port1, UInt16 port2)
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

    public class TunnelOneInMultipleOut : ITunnel
    {
        public readonly UInt16 port;
        public readonly PortSet portSet;
        public readonly PortSet fullPortSet;

        public TunnelOneInMultipleOut(UInt16 port, PortSet portSet)
        {
            if (portSet == null) throw new ArgumentNullException("portSet");
            this.port = port;
            this.portSet = portSet;
            this.fullPortSet = portSet.Combine(port);
        }

        PortSet ITunnel.FullPortSet { get { return fullPortSet; } }

        Boolean ITunnel.IsValidTunnel(UInt16 port1, UInt16 port2)
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

    public class TunnelMultipleInAndOut : ITunnel
    {
        public readonly PortSet portSet1, portSet2;
        public readonly PortSet fullPortSet;

        public TunnelMultipleInAndOut(PortSet portSet1, PortSet portSet2)
        {
            if (portSet1 == null) throw new ArgumentNullException("portSet1");
            if (portSet2 == null) throw new ArgumentNullException("portSet2");
            this.portSet1 = portSet1;
            this.portSet2 = portSet2;
            this.fullPortSet = portSet1.Combine(portSet2);
        }

        PortSet ITunnel.FullPortSet { get { return fullPortSet; } }

        Boolean ITunnel.IsValidTunnel(UInt16 port1, UInt16 port2)
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
}
