using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using More;

namespace HackCastleDoctrineTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        /*
        [TestMethod]
        public void TestMethod1()
        {
            CDLoader.Load(CDLoader.DefaultInstallPath);

            HouseObjectDefinition houseObjectDefinition = CDLoader.HouseObjectDefinitions[0];

            ByteBuffer buffer = new ByteBuffer(1024, 1024);

            using (FileStream stream = new FileStream(Path.Combine(
                CDLoader.HouseObjectsPath, Path.Combine(
                houseObjectDefinition.pathName, Path.Combine(
                "0",
                houseObjectDefinition.pathName + ".tga"))), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                UInt32 totalBytesRead = 0;

                while (true)
                {
                    Int32 bytesRead = stream.Read(buffer.array, (Int32)totalBytesRead,
                        (Int32)(buffer.array.Length - totalBytesRead));
                    if (bytesRead <= 0) break;
                    totalBytesRead += (UInt32)bytesRead;

                    buffer.EnsureCapacityCopyData(totalBytesRead + 256);
                }
            }

            TgaHeader tgaHeader = new TgaHeader();
            Console.WriteLine("idLength : {0}", tgaHeader.idLength);
            Console.WriteLine("colorMapType : {0}", tgaHeader.colorMapType);
            Console.WriteLine("imageType : {0}", tgaHeader.imageType);
            Console.WriteLine("  runLengthEncoding : {0}", tgaHeader.runLengthEncoding);
            Console.WriteLine();
            Console.WriteLine("colorMapIndex : {0}", tgaHeader.colorMapIndex);
            Console.WriteLine("colorMapLength : {0}", tgaHeader.colorMapLength);
            Console.WriteLine("colorMapBpp : {0}", tgaHeader.colorMapBpp);
            Console.WriteLine();
            Console.WriteLine("width : {0}", tgaHeader.width);
            Console.WriteLine("height : {0}", tgaHeader.height);
            Console.WriteLine("imageBpp : {0}", tgaHeader.imageBpp);
            Console.WriteLine("imageDescriptor : {0}", tgaHeader.imageDescriptor);

        }
        */
    }
}
