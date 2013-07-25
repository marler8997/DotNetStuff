using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using More;

namespace DataVis
{
    public abstract class DataFollower
    {
        readonly AutoResetEvent dataEvent;
        readonly Queue<String> data;
        Boolean reachedEndOfData;

        public AutoResetEvent DataEvent { get { return dataEvent; } }

        public DataFollower(AutoResetEvent dataEvent)
        {
            this.dataEvent = dataEvent;
            this.data = new Queue<String>();
        }
        public Boolean EndOfData()
        {
            return data.Count <= 0 && reachedEndOfData;
        }
        public String ReadAvailable()
        {
            lock(data)
            {
                if(data.Count <= 0) return null;
                return data.Dequeue();
            }
        }
        public void Run()
        {
            try
            {
                DataReadLoop();
            }
            finally
            {
                reachedEndOfData = true;
                dataEvent.Set();
            }
        }
        protected abstract void DataReadLoop();
        protected void AddData(String dataLine)
        {
            lock(this.data)
            {
                this.data.Enqueue(dataLine);
            }
            dataEvent.Set();
        }
    }
    public class FileFollower : DataFollower
    {
        readonly FileStream fileStream;
        readonly LineParser lineParser;
        readonly Byte[] readBuffer;

        public FileFollower(AutoResetEvent dataAvailableEvent, String filename, Int32 readBufferSize)
            : base(dataAvailableEvent)
        {
            this.fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            this.lineParser = new LineParser(Encoding.ASCII, ByteBuffer.DefaultInitialCapacity,
                ByteBuffer.DefaultExpandLength);
            this.readBuffer = new Byte[readBufferSize];
        }
        protected override void DataReadLoop()
        {
            while (true)
            {
                Int32 bytesRead = fileStream.Read(readBuffer, 0, readBuffer.Length);
                if(bytesRead <= 0) break;
                
                lineParser.Add(readBuffer, 0, (UInt32)bytesRead);

                while(true)
                {
                    String dataLine = lineParser.GetLine();
                    if(dataLine == null) break;
                    AddData(dataLine);
                }
            }
        }
    }
}
