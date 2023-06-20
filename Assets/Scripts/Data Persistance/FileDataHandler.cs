using System;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath;
    private string dataFileName;

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load()
    {
        // use Path.Combine to account for different OS's having different path separators
        var fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
            try
            {
                // Load the serialized data from the file
                var dataToLoad = "";
                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // deserialize the data from Json back into the C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data to file: " + fullPath + "\n" + e);
            }

        return loadedData;
    }

    public void Save(GameData data)
    {
        // use Path.Combine to account for different OS's having different path separators
        var fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // create the directory for the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            var dataToStore = JsonUtility.ToJson(data, true);

            // write the serialized data to the file 
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
    }
}