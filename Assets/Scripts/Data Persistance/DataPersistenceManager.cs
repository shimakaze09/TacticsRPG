using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")] [SerializeField]
    private string fileName;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    public static DataPersistenceManager Instance { get; private set; }

    #region MonoBehaviour

    private void Awake()
    {
        if (Instance != null)
            Debug.LogError("Found more than one Data Persistence Manager");
        Instance = this;
    }

    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    #endregion

    #region Public

    public void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadGame()
    {
        // load any saved data from a file using the data handler
        gameData = dataHandler.Load();

        // if no data can be loaded, initialize the a new game

        if (gameData == null)
        {
            Debug.Log("No data was found. Initializing to defaults.");
            NewGame();
        }

        // push the loaded data to all other scripts that need it
        foreach (var dataPersistenceObject in dataPersistenceObjects) dataPersistenceObject.LoadData(gameData);
    }

    public void SaveGame()
    {
        // pass the data to other scripts so they can update it

        foreach (var dataPersistenceObject in dataPersistenceObjects) dataPersistenceObject.SaveData(ref gameData);

        // save that data to file using the data handler
        dataHandler.Save(gameData);
    }

    #endregion

    #region Private

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        var dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    #endregion
}