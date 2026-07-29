using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataPersistenceManager : MonoBehaviour
{
    private readonly List<IDataPersistence> dataPersistenceObjects = new();
    private FileDataHandler dataHandler;

    [Header("File Storage Config")] [SerializeField]
    private string fileName;

    private GameData gameData;

    // True once LoadGame/NewGame has produced authoritative data; late
    // registrants (battle-spawned units) get that data applied immediately.
    private bool hasAuthoritativeData;

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
        // DontDestroyOnLoad only works on root objects; the scene may have
        // this manager nested under a parent.
        transform.SetParent(null);
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
        hasAuthoritativeData = true;

        // Push the fresh data to every live object so stale in-memory state
        // from a previous session cannot leak into the new game's first save.
        PruneNullEntries();
        foreach (var dataPersistenceObject in dataPersistenceObjects)
            dataPersistenceObject.LoadData(gameData);
    }

    public void LoadGame()
    {
        PruneNullEntries();

        // load any saved data from a file using the data handler
        gameData = dataHandler.Load();

        // if no data can be loaded, initialize a new game
        if (gameData == null)
        {
            Debug.Log("No data was found. Initializing to defaults.");
            NewGame();
            return; // NewGame already pushed the data
        }

        hasAuthoritativeData = true;

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
        return FindObjectsByType<MonoBehaviour>().OfType<IDataPersistence>();
    }

    private void PruneNullEntries()
    {
        for (var i = dataPersistenceObjects.Count - 1; i >= 0; i--)
        {
            var obj = dataPersistenceObjects[i];
            // The interface-typed null check misses destroyed Unity objects
            // (overloaded == is bypassed), so check both.
            if (obj == null || (obj is Object unityObj && unityObj == null))
                dataPersistenceObjects.RemoveAt(i);
        }
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

        // Battle units spawn long after LoadGame ran — apply the loaded data
        // to late registrants so saved progression is actually restored.
        if (hasAuthoritativeData && gameData != null)
            persistence.LoadData(gameData);
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