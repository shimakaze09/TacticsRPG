using System;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private readonly string dataDirPath;
    private readonly string dataFileName;

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load()
    {
        // use Path.Combine to account for different OS's having different path separators
        var fullPath = Path.Combine(dataDirPath, dataFileName);
        if (!File.Exists(fullPath))
            return null;

        try
        {
            var dataToLoad = File.ReadAllText(fullPath);
            var loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

            if (loadedData == null)
                throw new Exception("Save file deserialized to null");

            return loadedData;
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);

            // Preserve the unreadable file instead of letting a subsequent
            // save silently destroy possibly-recoverable data.
            QuarantineCorruptFile(fullPath);
            return null;
        }
    }

    public void Save(GameData data)
    {
        var fullPath = Path.Combine(dataDirPath, dataFileName);
        var tempPath = fullPath + ".tmp";
        var backupPath = fullPath + ".bak";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            var dataToStore = JsonUtility.ToJson(data, true);

            // Write to a temp file first, then swap it in — a crash mid-write
            // can no longer truncate the only copy of the save.
            File.WriteAllText(tempPath, dataToStore);

            if (File.Exists(fullPath))
                File.Replace(tempPath, fullPath, backupPath);
            else
                File.Move(tempPath, fullPath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    private static void QuarantineCorruptFile(string fullPath)
    {
        try
        {
            var quarantinePath = fullPath + ".corrupt-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            File.Copy(fullPath, quarantinePath, false);
            Debug.LogWarning("Corrupt save preserved at: " + quarantinePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not quarantine corrupt save: " + e.Message);
        }
    }
}
