using System;
using System.Collections.Generic;
using System.IO;

public class HouseObjectDefinition
{
    public readonly UInt16 id;
    public readonly String pathName;
    public HouseObjectDefinition(UInt16 id, String pathName)
    {
        this.id = id;
        this.pathName = pathName;
    }
}
public static class CDLoader
{
    static String gameInstallPath;
    static readonly List<HouseObjectDefinition> houseObjectDefinitions = new List<HouseObjectDefinition>();
    static readonly Dictionary<UInt16, HouseObjectDefinition> houseObjectDefinitionMap =
        new Dictionary<UInt16,HouseObjectDefinition>();

    static InvalidOperationException NotLoaded()
    {
        return new InvalidOperationException("CDLoader.Load has not been called yet");
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

    public static void Load(String gameInstallPath)
    {
        try
        {
            if (CDLoader.gameInstallPath != null) throw new InvalidOperationException("CLLoader.Load has already been called");
            CDLoader.gameInstallPath = gameInstallPath;

            if (!Directory.Exists(gameInstallPath))
                throw new InvalidOperationException(String.Format("Directory '{0}' does not exist", gameInstallPath));

            String houseObjectsPath = Path.Combine(gameInstallPath, Path.Combine("gameElements", "houseObjects"));
            if (!Directory.Exists(houseObjectsPath))
                throw new InvalidOperationException(String.Format("Directory '{0}' does not exist", houseObjectsPath));


            String[] objectFullPathNames = Directory.GetDirectories(houseObjectsPath);
            for (int i = 0; i < objectFullPathNames.Length; i++)
            {
                String objectFullPathName = objectFullPathNames[i];
                String objectPathName = Path.GetFileName(objectFullPathName);
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
                    houseObjectDefinition = new HouseObjectDefinition(id, objectPathName);
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



}