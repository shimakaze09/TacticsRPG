using UnityEngine;

/// <summary>
/// Serializable meta-progress (story flags, unlocked content) — placeholder for
/// the future world layer.
/// </summary>
public class PlayerProgress : MonoBehaviour, IDataPersistence
{
    private float playTime;

    public void LoadData(GameData data)
    {
        playTime = data.playTime;
    }

    public void SaveData(ref GameData data)
    {
        data.playTime = playTime;
    }

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
}