/// <summary>
/// Contract for anything that saves/loads state through DataPersistenceManager.
/// </summary>
public interface IDataPersistence
{
    void LoadData(GameData data);
    void SaveData(ref GameData data);
}