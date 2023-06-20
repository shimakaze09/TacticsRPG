using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : MonoBehaviour, IDataPersistence
{
    private float playTime, startTime;

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        playTime += Time.time - startTime;
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