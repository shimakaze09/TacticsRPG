using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField]
    private string fileName;

    private GameData gameData;
    private readonly List<IDataPersistence> dataPersistenceObjects = new();
    private FileDataHandler dataHandler;

    public static DataPersistenceManager Instance { get; private set; }

    #region MonoBehaviour

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Found more than one Data Persistence Manager");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        RegisterExistingSceneObjects();
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
        PruneNullEntries();

        // load any saved data from a file using the data handler
        gameData = dataHandler.Load();

        // if no data can be loaded, initialize the a new game

        if (gameData == null)
        {
            Debug.Log("No data was found. Initializing to defaults.");
            NewGame();
        }

        // push the loaded data to all other scripts that need it
        foreach (var dataPersistenceObject in dataPersistenceObjects)
            dataPersistenceObject.LoadData(gameData);
    }

    public void SaveGame()
    {
        if (gameData == null)
            gameData = new GameData();

        PruneNullEntries();

        // pass the data to other scripts so they can update it

        foreach (var dataPersistenceObject in dataPersistenceObjects)
            dataPersistenceObject.SaveData(ref gameData);

        // save that data to file using the data handler
        dataHandler.Save(gameData);
    }

    #endregion

    #region Private

    private IEnumerable<IDataPersistence> FindAllDataPersistenceObjects()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistence>();
    }

    private void PruneNullEntries()
    {
        for (var i = dataPersistenceObjects.Count - 1; i >= 0; i--)
            if (dataPersistenceObjects[i] == null)
                dataPersistenceObjects.RemoveAt(i);
    }

    private void RegisterExistingSceneObjects()
    {
        foreach (var persistence in FindAllDataPersistenceObjects())
            RegisterInternal(persistence);
    }

    private void RegisterInternal(IDataPersistence persistence)
    {
        if (persistence == null || dataPersistenceObjects.Contains(persistence))
            return;

        dataPersistenceObjects.Add(persistence);
    }

    private void UnregisterInternal(IDataPersistence persistence)
    {
        if (persistence == null)
            return;

        dataPersistenceObjects.Remove(persistence);
    }

    #endregion

    #region Static API

    public static void Register(IDataPersistence persistence)
    {
        if (persistence == null)
            return;

        if (Instance == null)
        {
            var manager = FindAnyObjectByType<DataPersistenceManager>();
            manager?.RegisterInternal(persistence);
            return;
        }

        Instance.RegisterInternal(persistence);
    }

    public static void Unregister(IDataPersistence persistence)
    {
        if (persistence == null)
            return;

        if (Instance == null)
        {
            var manager = FindAnyObjectByType<DataPersistenceManager>();
            manager?.UnregisterInternal(persistence);
            return;
        }

        Instance.UnregisterInternal(persistence);
    }

    #endregion
}