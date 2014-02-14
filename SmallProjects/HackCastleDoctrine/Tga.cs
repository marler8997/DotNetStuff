using System;

namespace More
{
    public class TgaHeader
    {
        public const UInt32 Length = 18;

        // 
        public Byte    idLength;
        public Byte    colorMapType;
        public Byte    imageType;
        public Boolean runLengthEncoding;

        // Color Map Specification
        public UInt16 colorMapIndex;
        public UInt32 colorMapLength;
        public Byte   colorMapBpp;

        // Image Specification
        public UInt16 width;
        public UInt16 height;
        public Byte   imageBpp;
        public Byte   imageDescriptor;

        public void Load(Byte[] header, UInt32 offset)
        {
            this.idLength           = header[offset + 0];
            this.colorMapType       = header[offset + 1];
            this.imageType          = header[offset + 2];
            this.runLengthEncoding  = (imageType & 0x08) == 0x08;

            if(colorMapType > 0)
            {
                this.colorMapIndex  = header.LittleEndianReadUInt16(offset + 3);
                this.colorMapLength = header.LittleEndianReadUInt16(offset + 5);
                this.colorMapBpp    = header[offset + 7];
            }

            this.width              = header.LittleEndianReadUInt16(offset + 12);
            this.height             = header.LittleEndianReadUInt16(offset + 14);
            this.imageBpp           = header[offset + 16];
            this.imageDescriptor    = header[offset + 17];
        }
    }
    class Tga
    {


        /*
        public static Byte[] getImageType10()
        {
            // header stores total bits per pixel
            int bytesPerPixel = header.bpp / 8;
            int total = header.width * header.height * bytesPerPixel;

            // number of bytes we've read so far
            int count = 0;
            int repeat;
            byte packetHdr;
            int packetType;

            // temp storage
            byte[] bytes;
            byte[] pixData = new byte[total];

            while (count < total)
            {
                packetHdr = inFile.ReadByte();
                packetType = packetHdr & (1 << 7);

                // RLE packet
                if (packetType == 128)
                {
                    // packet stores number of times following pixel is repeated
                    repeat = (packetHdr & ~(1 << 7)) + 1;
                    bytes = inFile.ReadBytes(bytesPerPixel);
                    for (int j = 0; j < repeat; j++)
                    {
                        foreach (byte b in bytes)
                        {
                            pixData[count] = b;
                            count += 1;
                        }
                    }
                }

                // raw packet
                else if (packetType == 0)
                {
                    // packet stores number of pixels that follow
                    repeat = ((packetHdr & ~(1 << 7)) + 1) * bytesPerPixel;
                    for (int j = 0; j < repeat; j++)
                    {
                        pixData[count] = inFile.ReadByte();
                        count += 1;
                    }
                }
            }
            return pixData;
        }
        */
    }
}
