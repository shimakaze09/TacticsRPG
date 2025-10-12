using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : MonoBehaviour, IDataPersistence
{
    private float playTime;

    private void OnEnable()
    {
        DataPersistenceManager.Register(this);
    }

    private void OnDisable()
    {
        DataPersistenceManager.Unregister(this);
    }

    private void Update()
    {
        playTime += Time.deltaTime;
    }

    public void LoadData(GameData data)
    {
        playTime = data.playTime;
    }

    public void SaveData(ref GameData data)
    {
        data.playTime = playTime;
    }
}