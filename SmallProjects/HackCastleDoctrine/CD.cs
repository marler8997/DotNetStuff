using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

using More;

public static class CDSha
{
    public const String sharedServerSecretString =
        "Please do not use this secret string to connect unfairly modded clients to the main server.  Keep in mind that this is an indie, open-source game made entirely by one person.  I am trusting you to do the right thing.  --Jason";
    public static readonly Byte[] sharedServerSecret = Encoding.ASCII.GetBytes(sharedServerSecretString);

    public static String Decrypt(Byte[] mapKey, Byte[] encrypted)
    {
        Sha1 sha1 = new Sha1();

        Byte[] xorKey = new Byte[encrypted.Length + 19];
        UInt32 xorKeyLength = 0;

        int counter = 0;
        while (xorKeyLength < encrypted.Length)
        {
            Byte[] counterStringBytes = Encoding.ASCII.GetBytes(counter.ToString());
            sha1.Add(counterStringBytes, 0, counterStringBytes.Length);
            sha1.Add(mapKey, 0, mapKey.Length);
            sha1.Add(sharedServerSecret, 0, sharedServerSecret.Length);
            sha1.Add(counterStringBytes, 0, counterStringBytes.Length);

            UInt32[] hash = sha1.Finish();

            xorKey.BigEndianSetUInt32(xorKeyLength + 0, hash[0]);
            xorKey.BigEndianSetUInt32(xorKeyLength + 4, hash[1]);
            xorKey.BigEndianSetUInt32(xorKeyLength + 8, hash[2]);
            xorKey.BigEndianSetUInt32(xorKeyLength + 12, hash[3]);
            xorKey.BigEndianSetUInt32(xorKeyLength + 16, hash[4]);
            xorKeyLength += 20;

            sha1.Reset();
            counter++;
        }

        Char[] decrypted = new Char[encrypted.Length];
        for (int i = 0; i < decrypted.Length; i++)
        {
            decrypted[i] = (Char)(xorKey[i] ^ encrypted[i]);
        }

        return new String(decrypted);
    }
}

public static class CDTgaNotThreadSafe
{
    static readonly ByteBuffer imageLoadBuffer = new ByteBuffer(4096, 2048);
    static readonly TgaHeader header = new TgaHeader();

    // WARNING: This methid is NOT THREAD SAFE
    public static Bitmap LoadNotThreadSafe(Stream stream)
    {
        stream.ReadFullSize(imageLoadBuffer.array, 0, (Int32)TgaHeader.Length);
        header.Load(imageLoadBuffer.array, 0);

        if(header.imageType != 2) throw new FormatException(String.Format(
            "Expected TGA image type to be 2 but is {0}", header.imageType));
        if(header.runLengthEncoding) throw new FormatException(
            "Did not expect TGA image to use RLE");
        if(header.imageBpp != 24) throw new FormatException(String.Format(
            "Expected TGA image bpp to be 24 but is {0}", header.imageBpp));
        //if(header.width != 16) throw new FormatException(String.Format(
        //    "Expected TGA image width to be 16 but is {0}", header.width));

        UInt32 imagePixelLength = (UInt32)header.width * (UInt32)header.height;
        UInt32 imageByteLength = imagePixelLength * 3;
        imageLoadBuffer.EnsureCapacityNoCopy(imageByteLength);
        Byte[] imageData = imageLoadBuffer.array;
        stream.ReadFullSize(imageData, 0, (Int32)imageByteLength);

        Int32 size = header.width;
        Bitmap bitmap = new Bitmap(size, size);

        UInt32 byteOffset = 0;
        for(int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
            {
                /*
                bitmap.SetPixel((Int32)x, (Int32)y, Color.FromArgb((Int32)(
                    (0xFF000000 & ((UInt32)imageData[byteOffset    ] << 24)) |
                    (0x00FF0000 & ((UInt32)imageData[byteOffset + 1] << 16)) |
                    (0x0000FF00 & ((UInt32)imageData[byteOffset + 2] << 8)))));*/
                // Check for transparent
                if (imageData[byteOffset] == 0xFF && imageData[byteOffset + 1] == 0xFF)
                {
                    bitmap.SetPixel((Int32)x, (Int32)y, Color.Transparent);
                }
                else
                {
                    bitmap.SetPixel((Int32)x, (Int32)y, Color.FromArgb(
                        0xFF, imageData[byteOffset + 2], imageData[byteOffset + 1], imageData[byteOffset + 0]));
                }
                byteOffset += 3;
            }
        }

        // Scale the image
        if (size < 32)
        {
            bitmap = new Bitmap(bitmap, new Size(32, 32));
        }

        return bitmap;
    }
}

public class HouseObjectStateDefinition
{
    public readonly Byte id;
    public readonly Bitmap bitmap;
    public HouseObjectStateDefinition(Byte id, Bitmap bitmap)
    {
        this.id = id;
        this.bitmap = bitmap;
    }
}


public class HouseObjectDefinition
{
    public readonly UInt16 id;
    public readonly String pathName;

    readonly HouseObjectStateDefinition defaultState;
    public readonly Dictionary<Byte, HouseObjectStateDefinition> states;

    public HouseObjectDefinition(UInt16 id, String pathName, Dictionary<Byte,HouseObjectStateDefinition> states)
    {
        this.id = id;
        this.pathName = pathName;        
        this.defaultState = states[0];
        this.states = states;
    }
    public virtual Bitmap Bitmap { get { return defaultState.bitmap; } }
    public virtual String Extra { get { return null; } }
}
public class HouseObjectDefinitionWithExtra : HouseObjectDefinition
{
    public readonly String extra;
    public HouseObjectDefinitionWithExtra(HouseObjectDefinition definition, String extra)
        : base(definition.id, definition.pathName, definition.states)
    {
        this.extra = extra;
    }
    public override String Extra { get { return extra; } }
}



public static class CDLoader
{
    public const String DefaultInstallPath = @"C:\Users\Jonathan Marler\Desktop\CastleDoctrine_v31";

    static String gameInstallPath;
    static String houseObjectsPath;
    static readonly List<HouseObjectDefinition> houseObjectDefinitions = new List<HouseObjectDefinition>();
    static readonly Dictionary<UInt16, HouseObjectDefinition> houseObjectDefinitionMap =
        new Dictionary<UInt16,HouseObjectDefinition>();

    static InvalidOperationException NotLoaded()
    {
        return new InvalidOperationException("CDLoader.Load has not been called yet");
    }
    public static String HouseObjectsPath
    {
        get
        {
            if (gameInstallPath == null) throw NotLoaded();
            return houseObjectsPath;
        }
    }
    public static List<HouseObjectDefinition> HouseObjectDefinitions
    {
        get
        {
            if (gameInstallPath == null) throw NotLoaded();
            return houseObjectDefinitions;
        }
    }
    public static Dictionary<UInt16, HouseObjectDefinition> HouseObjectDefinitionMap
    {
        get
        {
            if (gameInstallPath == null) throw NotLoaded();
            return houseObjectDefinitionMap;
        }
    }

    static void Reset()
    {
        gameInstallPath = null;
        houseObjectDefinitions.Clear();
        houseObjectDefinitionMap.Clear();
    }

    static HouseObjectStateDefinition LoadState(Byte id, String path, ByteBuffer imageLoadBuffer)
    {
        String[] fullPathImages = Directory.GetFiles(path, "*.tga");
        for (int i = 0; i < fullPathImages.Length; i++)
        {
            String fullPathImage = fullPathImages[i];
            if (fullPathImage.Contains("shaderMap")) continue;

            Bitmap bitmap;
            using (FileStream stream = new FileStream(fullPathImage, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap = CDTgaNotThreadSafe.LoadNotThreadSafe(stream);
            }
            return new HouseObjectStateDefinition(id, bitmap);
        }
        throw new InvalidOperationException(String.Format(
            "House Object '{0}' does not have any 'tga' images", path));
    }

    public static void Load(String gameInstallPath)
    {
        try
        {
            if (CDLoader.gameInstallPath != null) throw new InvalidOperationException("CLLoader.Load has already been called");
            CDLoader.gameInstallPath = gameInstallPath;

            if (!Directory.Exists(gameInstallPath))
                throw new InvalidOperationException(String.Format("Directory '{0}' does not exist", gameInstallPath));


            houseObjectsPath = Path.Combine(gameInstallPath, Path.Combine("gameElements", "houseObjects"));
            if (!Directory.Exists(houseObjectsPath))
                throw new InvalidOperationException(String.Format("Directory '{0}' does not exist", houseObjectsPath));

            ByteBuffer imageLoadBuffer = new ByteBuffer(4096, 2048);

            String[] objectFullPathNames = Directory.GetDirectories(houseObjectsPath);
            for (int objIndex = 0; objIndex < objectFullPathNames.Length; objIndex++)
            {
                String objectFullPathName = objectFullPathNames[objIndex];
                String objectPathName = Path.GetFileName(objectFullPathName);

                //
                // Read Object States and Images
                //
                Dictionary<Byte, HouseObjectStateDefinition> states = new Dictionary<Byte, HouseObjectStateDefinition>();
                String[] stateFullPathNames = Directory.GetDirectories(objectFullPathName);
                for (int stateIndex = 0; stateIndex < stateFullPathNames.Length; stateIndex++)
                {
                    String stateFullPathName = stateFullPathNames[stateIndex];
                    String stateName = Path.GetFileName(stateFullPathName);
                    Byte stateID = Byte.Parse(stateName);

                    states.Add(stateID, LoadState(stateID, stateFullPathName, imageLoadBuffer));
                }


                //
                // Read Object Info
                //
                String infoFile = Path.Combine(objectFullPathName, "info.txt");
                if (!File.Exists(infoFile)) throw new InvalidOperationException(String.Format(
                     "Missing 'info.txt' file in path '{0}'", houseObjectsPath));

                UInt16 id;
                HouseObjectDefinition houseObjectDefinition;
                using (TextReader reader = new StreamReader(new FileStream(infoFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    String idLine = reader.ReadLine();
                    if (idLine == null) throw new InvalidOperationException(
                         "info.txt file ended before the id was found");
                    id = UInt16.Parse(idLine);
                    houseObjectDefinition = new HouseObjectDefinition(id, objectPathName, states);
                }

                houseObjectDefinitions.Add(houseObjectDefinition);
                houseObjectDefinitionMap.Add(id, houseObjectDefinition);
            }
        }
        catch (Exception)
        {
            Reset();
            throw;
        }
    }


    public static void PrintMap(String map)
    {
        String[] tiles = map.Split('#');
        PrintMap(tiles);
    }
    public static void PrintMap(String[] tiles)
    {
        // Print in reverse order (map is from bottom to top)
        for (int i = 31; true; i--)
        {
            Console.WriteLine();
            int rowOffset = i * 32;
            for (int j = 0; j < 32; j++)
            {
                Console.Write(" {0,4}", tiles[rowOffset + j]);
            }
            if (i == 0) break;
        }

    }


    public static HouseObjectDefinition[] ParseMap(String[] objectStrings)
    {
        if(gameInstallPath == null) throw NotLoaded();

        HouseObjectDefinition[] mapObjects = new HouseObjectDefinition[1024];

        for (int objIndex = 0; objIndex < objectStrings.Length; objIndex++)
        {
            String objectString = objectStrings[objIndex];

            String extra = null;
            for (int i = 0; i < objectString.Length; i++)
            {
                Char c = objectString[i];
                if(c < '0' || c > '9') {
                    extra = objectString.Substring(i);
                    objectString = objectString.Remove(i);
                    Console.WriteLine("EXTRA: '{0}'", extra);
                    break;
                }
            }
            UInt16 id = UInt16.Parse(objectString);

            HouseObjectDefinition houseObjectDefinition;
            if (!houseObjectDefinitionMap.TryGetValue(id, out houseObjectDefinition))
                throw new FormatException(String.Format("Unknown object id '{0}'", id));

            if (extra == null)
            {
                mapObjects[objIndex] = houseObjectDefinition;
            }
            else
            {
                mapObjects[objIndex] = new HouseObjectDefinitionWithExtra(houseObjectDefinition, extra);
            }
        }

        return mapObjects;
    }
}