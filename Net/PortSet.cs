using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.Net
{
    public abstract class PortSet
    {
        public abstract Int32 Length { get; }
        public abstract UInt16 this[Int32 key] { get; }

        public abstract Boolean Contains(UInt16 port);

        public abstract PortSet Combine(UInt16 port);
        public abstract PortSet Combine(PortSet portSet);

        public abstract PortSetSingle CastAsPortSetSingle { get; }
        public abstract PortSetDouble CastAsPortSetDouble { get; }
        public abstract PortSetArray CastAsPortSetArray { get; }
    }

    public static class PortSetMethods
    {
        public static void CombineHelper(UInt16[] portBuffer, Int32 offset, UInt16 lowPort, UInt16 highPort, UInt16 unknownPort)
        {
            if (unknownPort < lowPort)
            {
                portBuffer[offset++] = unknownPort;
                portBuffer[offset++] = lowPort;
                portBuffer[offset] = highPort;
            }
            else
            {
                portBuffer[0] = lowPort;
                if (unknownPort < highPort)
                {
                    portBuffer[1] = unknownPort;
                    portBuffer[2] = highPort;
                }
                else
                {
                    portBuffer[1] = highPort;
                    portBuffer[2] = unknownPort;
                }
            }
        }

        public static UInt16[] SortedCombine(UInt16[] a, UInt16[] b)
        {
            UInt16[] c = new UInt16[a.Length + b.Length];

            Int32 aOffset = 1, bOffset = 1, cOffset = 0;
            UInt16 nextAValue = a[0], nextBValue = b[0];

            while (true)
            {
                if (nextAValue < nextBValue)
                {
                    c[cOffset++] = nextAValue;
                    if (aOffset >= a.Length)
                    {
                        c[cOffset++] = nextBValue;
                        break;
                    }
                    nextAValue = a[aOffset++];
                }
                else if (nextAValue > nextBValue)
                {
                    c[cOffset++] = nextBValue;
                    if (bOffset >= b.Length)
                    {
                        c[cOffset++] = nextAValue;
                        break;
                    }
                    nextBValue = b[bOffset++];
                }
                else
                {
                    c[cOffset++] = nextAValue;
                    if (aOffset >= a.Length || bOffset >= b.Length) break;
                    nextAValue = a[aOffset++];
                    nextBValue = b[bOffset++];
                }
            }

            while (aOffset < a.Length)
            {
                c[cOffset++] = a[aOffset++];
            }
            while (bOffset < b.Length)
            {
                c[cOffset++] = b[bOffset++];
            }

            if (c.Length == cOffset) return c;
            UInt16[] newC = new UInt16[cOffset];
            for (cOffset--; cOffset >= 0; cOffset--)
            {
                newC[cOffset] = c[cOffset];
            }
            return newC;
        }
    }


    public class PortSetSingle : PortSet
    {
        private readonly UInt16 port;

        public PortSetSingle(UInt16 port)
        {
            if (port <= 0) throw new ArgumentOutOfRangeException("port");
            this.port = port;
        }

        public override Int32 Length
        {
            get { return 1; }
        }

        public override UInt16 this[Int32 key]
        {
            get { if (key == 0) return port; throw new IndexOutOfRangeException(); }
        }

        public override Boolean Contains(UInt16 port) { return this.port == port; }

        public override PortSet Combine(UInt16 port)
        {
            if (this.port == port) return this;
            return new PortSetDouble(this.port, port);
        }

        public override PortSet Combine(PortSet portSet)
        {
            if (portSet.Length == 1)
            {
                if (this.port == portSet[0])
                {
                    return this;
                }
                else
                {
                    return new PortSetDouble(this.port, portSet[0]);
                }
            }
            return portSet.Combine(this.port);
        }

        public override PortSetSingle CastAsPortSetSingle
        {
            get { return this; }
        }
        public override PortSetDouble CastAsPortSetDouble
        {
            get { throw new InvalidOperationException("You can't cast a PortSetSingle as a PortSetDouble"); }
        }
        public override PortSetArray CastAsPortSetArray
        {
            get { throw new InvalidOperationException("You can't cast a PortSetSingle as a PortSetArray"); }
        }

        public override String ToString()
        {
            return port.ToString();
        }
    }

    public class PortSetDouble : PortSet
    {
        private readonly UInt16 lowPort, highPort;

        public PortSetDouble(UInt16 port1, UInt16 port2)
        {
            if (port1 < port2)
            {
                lowPort = port1;
                highPort = port2;
            }
            else if (port1 > port2)
            {
                lowPort = port2;
                highPort = port1;
            }
            else
            {
                throw new ArgumentException(String.Format("The ports are the same ({0})", port1));
            }
            if (lowPort <= 0) throw new ArgumentOutOfRangeException("Port cannot be 0");
        }

        public override Int32 Length
        {
            get { return 2; }
        }

        public override UInt16 this[Int32 key]
        {
            get { if (key == 0) return lowPort; if (key == 1) return highPort; throw new IndexOutOfRangeException(); }
        }

        public override Boolean Contains(UInt16 port)
        {
            return (this.lowPort == port || this.highPort == port);
        }

        public override PortSet Combine(UInt16 port)
        {
            if (this.lowPort == port || this.highPort == port) return this;
            UInt16[] newPortArray = new UInt16[3];
            if (port < this.lowPort)
            {
                newPortArray[0] = port;
                newPortArray[1] = this.lowPort;
                newPortArray[2] = this.highPort;
            }
            else
            {
                newPortArray[0] = this.lowPort;
                if (port < this.highPort)
                {
                    newPortArray[1] = port;
                    newPortArray[2] = this.highPort;
                }
                else
                {
                    newPortArray[1] = this.highPort;
                    newPortArray[2] = port;
                }
            }

            return new PortSetArray(newPortArray);
        }

        public override PortSet Combine(PortSet portSet)
        {
            if (portSet.Length == 1)
            {
                UInt16 otherPort = portSet[0];
                if (otherPort == lowPort) return this;
                if (otherPort == highPort) return this;

                UInt16[] newPortArray = new UInt16[3];
                if (otherPort < lowPort)
                {
                    newPortArray[0] = otherPort;
                    newPortArray[1] = lowPort;
                    newPortArray[2] = highPort;
                }
                else
                {
                    newPortArray[0] = lowPort;
                    if (otherPort < highPort)
                    {
                        newPortArray[1] = otherPort;
                        newPortArray[2] = highPort;
                    }
                    else
                    {
                        newPortArray[1] = highPort;
                        newPortArray[2] = otherPort;
                    }
                }
                return new PortSetArray(newPortArray);
            }

            UInt16[] thisArray = new UInt16[2];
            thisArray[0] = lowPort;
            thisArray[1] = highPort;

            if (portSet.Length == 2)
            {
                PortSetDouble otherPortSetDouble = portSet.CastAsPortSetDouble;
                if ((this.lowPort == otherPortSetDouble.lowPort) &&
                    (this.highPort == otherPortSetDouble.highPort))
                {
                    return this;
                }

                UInt16[] otherArray = new UInt16[2];
                otherArray[0] = otherPortSetDouble.lowPort;
                otherArray[1] = otherPortSetDouble.highPort;
                return new PortSetArray(PortSetMethods.SortedCombine(thisArray, otherArray));
            }

            PortSetArray otherPortSetArray = portSet.CastAsPortSetArray;
            return new PortSetArray(PortSetMethods.SortedCombine(thisArray, otherPortSetArray.sortedPortArray));
        }

        public override PortSetSingle CastAsPortSetSingle
        {
            get { throw new InvalidOperationException("You can't cast a PortSetDouble as a PortSetSingle"); }
        }
        public override PortSetDouble CastAsPortSetDouble
        {
            get { return this; }
        }
        public override PortSetArray CastAsPortSetArray
        {
            get { throw new InvalidOperationException("You can't cast a PortSetDouble as a PortSetArray"); }
        }

        public override String ToString()
        {
            return String.Format("{0},{1}",lowPort, highPort);
        }
    }

    public class PortSetArray : PortSet
    {
        public readonly UInt16[] sortedPortArray;

        public PortSetArray(UInt16[] sortedPortArray)
        {
            if (sortedPortArray == null || sortedPortArray.Length < 2) throw new ArgumentException("A port set must have at least 2 ports in it", "sortedPortArray");
            if (sortedPortArray[0] <= 0) throw new ArgumentException(String.Format("The first port {0} is invalid because it is <= 0", sortedPortArray[0]));

            //
            // Check that the port array has not common elements
            //
            for (int i = 1; i < sortedPortArray.Length; i++)
            {
                if (sortedPortArray[i] <= sortedPortArray[i - 1])
                {
                    throw new ArgumentException(String.Format("The sortedPortArray is not sorted, port[{0}]={1} is not less that port[{2}]={3}",
                        i - 1, sortedPortArray[i - 1], i, sortedPortArray[i]));
                }
            }

            this.sortedPortArray = sortedPortArray;
        }

        public override Int32 Length
        {
            get { return sortedPortArray.Length; }
        }

        public override UInt16 this[Int32 key]
        {
            get { return sortedPortArray[key]; }
        }

        public override Boolean Contains(UInt16 port)
        {
            for (int i = 0; i < sortedPortArray.Length; i++)
            {
                if (port == sortedPortArray[i]) return true;
                if (port < sortedPortArray[i]) return false;
            }
            return false;
        }

        public override PortSet Combine(UInt16 port)
        {
            for (int i = 0; i < sortedPortArray.Length; i++)
            {
                if (port == sortedPortArray[i]) return this;
                if (port < sortedPortArray[i]) break;
            }

            UInt16[] newPortArray = new UInt16[sortedPortArray.Length + 1];
            Int32 sortedPortOffset = 0, newPortOffset = 0;

            while (sortedPortOffset < sortedPortArray.Length)
            {
                if (port < sortedPortArray[sortedPortOffset])
                {
                    newPortArray[newPortOffset++] = port;
                    while (newPortOffset < newPortArray.Length)
                    {
                        newPortArray[newPortOffset++] = sortedPortArray[sortedPortOffset++];
                    }
                    return new PortSetArray(newPortArray);
                }
                newPortArray[newPortOffset++] = sortedPortArray[sortedPortOffset++];
            }
            newPortArray[newPortOffset++] = port;
            return new PortSetArray(newPortArray);
        }

        public override PortSet Combine(PortSet portSet)
        {
            if (portSet.Length <= 2)
            {
                return portSet.Combine(this);
            }
            return new PortSetArray(PortSetMethods.SortedCombine(this.sortedPortArray, portSet.CastAsPortSetArray.sortedPortArray));
        }

        public override PortSetSingle CastAsPortSetSingle
        {
            get { throw new InvalidOperationException("You can't cast a PortSetArray as a PortSetSingle"); }
        }
        public override PortSetDouble CastAsPortSetDouble
        {
            get { throw new InvalidOperationException("You can't cast a PortSetArray as a PortSetDouble"); }
        }
        public override PortSetArray CastAsPortSetArray
        {
            get { return this; }
        }

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder(3 * sortedPortArray.Length);
            Int32 i;
            for (i = 0; i < sortedPortArray.Length - 1; i++)
            {
                builder.Append(sortedPortArray[i].ToString());
                builder.Append(',');
            }
            builder.Append(sortedPortArray[i].ToString());

            return builder.ToString();
        }
    }
}
