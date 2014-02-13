using System;
using System.Collections.Generic;

namespace More
{
    public class Sha1
    {
        public const Int32 HashByteLength = 20;
        public const Int32 BlockByteLength = 64;

        public const Int32 HashUInt32Length = 5;

        public static Boolean Equals(UInt32[] hashA, UInt32[] hashB)
        {
            return
                hashA[0] == hashB[0] &&
                hashA[1] == hashB[1] &&
                hashA[2] == hashB[2] &&
                hashA[3] == hashB[3] &&
                hashA[4] == hashB[4];
        }
        public static String HashString(UInt32[] hash)
        {
            return String.Format("{0:X8}{1:X8}{2:X8}{3:X8}{4:X8}",
                hash[0], hash[1], hash[2], hash[3], hash[4]);
        }
        public static void Parse(String hashString, Int32 hashStringOffset, out UInt32[] hash)
        {
            Byte[] hashBytes = new Byte[HashByteLength];
            hashBytes.ParseHex(0, hashString, hashStringOffset, HashByteLength * 2);
            
            hash = new UInt32[HashUInt32Length];
            hash[0] = hashBytes.BigEndianReadUInt32( 0);
            hash[1] = hashBytes.BigEndianReadUInt32( 4);
            hash[2] = hashBytes.BigEndianReadUInt32( 8);
            hash[3] = hashBytes.BigEndianReadUInt32(12);
            hash[4] = hashBytes.BigEndianReadUInt32(16);
        }

        static readonly UInt32[] InitialHash = new UInt32[] {
            0x67452301,
            0xEFCDAB89,
            0x98BADCFE,
            0x10325476,
            0xC3D2E1F0,
        };
        static readonly UInt32[] K = new UInt32[] {
            0x5A827999,
            0x6ED9EBA1,
            0x8F1BBCDC,
            0xCA62C1D6,
        };

        readonly UInt32[] hash;

        readonly Byte[] block;
        Int32 blockIndex;

        UInt64 messageBitLength;

        UInt32[] finishedHash;
        public UInt32[] FinishedHash
        {
            get
            {
                if (finishedHash == null) throw new InvalidOperationException("This hash has not been finished yet");
                return finishedHash;
            }
        }

        public Sha1()
        {
            this.hash = new UInt32[HashUInt32Length];
            this.block = new Byte[BlockByteLength];
            Reset();
        }
        public void Reset()
        {
            this.blockIndex = 0;
            this.messageBitLength = 0;
            hash[0] = InitialHash[0];
            hash[1] = InitialHash[1];
            hash[2] = InitialHash[2];
            hash[3] = InitialHash[3];
            hash[4] = InitialHash[4];
            this.finishedHash = null;
        }
        public void Add(String str, Int32 offset, Int32 length)
        {
            if (finishedHash != null) throw new InvalidOperationException("This hash has already been finished");

            throw new NotImplementedException();
        }

        public void Add(Byte[] bytes, Int32 offset, Int32 length)
        {
            if (finishedHash != null) throw new InvalidOperationException("This hash has already been finished");

            while(length > 0)
            {
                Int32 blockBytesLeft = BlockByteLength - blockIndex;
                if (length < blockBytesLeft)
                {
                    Array.Copy(bytes, offset, block, blockIndex, length);
                    blockIndex += length;
                    messageBitLength += ((UInt64)length << 3); // length * 8
                    return;
                }

                Array.Copy(bytes, offset, block, blockIndex, blockBytesLeft);
                blockIndex = 0;
                messageBitLength += ((UInt64)blockBytesLeft << 3); // length * 8
                HashBlock();
                offset += blockBytesLeft;
                length -= blockBytesLeft;
            }
        }

        public UInt32[] Finish()
        {
            if (finishedHash != null) throw new InvalidOperationException("This hash has already been finished");

            Pad();
            block.BigEndianSetUInt64(56, messageBitLength);
            HashBlock();

            finishedHash = new UInt32[HashUInt32Length];
            finishedHash[0] = hash[0];
            finishedHash[1] = hash[1];
            finishedHash[2] = hash[2];
            finishedHash[3] = hash[3];
            finishedHash[4] = hash[4];


            return finishedHash;
        }

        void Pad()
        {
            block[blockIndex++] = 0x80;
            if (blockIndex > 56)
            {
                while (blockIndex < BlockByteLength) block[blockIndex++] = 0;
                blockIndex = 0;
                HashBlock();
            }
            while(blockIndex < 56) block[blockIndex++] = 0;
        }

        UInt32 CircularShift(UInt32 value, Int32 shift)
        {
            return (value << shift) | (value >> (32 - shift));
        }

        void HashBlock()
        {
            Byte temp8;
            UInt32 temp32;

            UInt32[] W = new UInt32[80];

            // Initialize the first 16 words in array W
            for (Byte i = 0; i < 16; i++)
            {
                temp8 = (Byte)(i << 2);
                W[i] = (UInt32)(
                    (block[temp8   ] << 24) |
                    (block[temp8 + 1] << 16) |
                    (block[temp8 + 2] <<  8) |
                    (block[temp8 + 3]      ) );
            }

            // Initialize the rest of the words in array W
            for (Byte i = 16; i < 80; i++)
            {
                W[i] = CircularShift(W[i - 3] ^ W[i - 8] ^ W[i - 14] ^ W[i - 16], 1);
            }

            UInt32
                A = hash[0],
                B = hash[1],
                C = hash[2],
                D = hash[3],
                E = hash[4];

            for (int i = 0; i < 20; i++)
            {
                temp32 = CircularShift(A, 5) + ((B & C) | ((~B) & D)) + E + W[i] + K[0];
                E = D;
                D = C;
                C = CircularShift(B, 30);
                B = A;
                A = temp32;
            }
            for (int i = 20; i < 40; i++)
            {
                temp32 = CircularShift(A, 5) + (B ^ C ^ D) + E + W[i] + K[1];
                E = D;
                D = C;
                C = CircularShift(B, 30);
                B = A;
                A = temp32;
            }
            for (int i = 40; i < 60; i++)
            {
                temp32 = CircularShift(A, 5) + ((B & C) | (B & D) | (C & D)) + E + W[i] + K[2];
                E = D;
                D = C;
                C = CircularShift(B, 30);
                B = A;
                A = temp32;
            }
            for (int i = 60; i < 80; i++)
            {
                temp32 = CircularShift(A, 5) + (B ^ C ^ D) + E + W[i] + K[3];
                E = D;
                D = C;
                C = CircularShift(B, 30);
                B = A;
                A = temp32;
            }

            hash[0] += A;
            hash[1] += B;
            hash[2] += C;
            hash[3] += D;
            hash[4] += E;
        }
    }
}
