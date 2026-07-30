/// <summary>
/// What a board tile is made of. Gameplay meaning (who can pass, who can
/// stop, what blocks sight) lives in TerrainRules; visuals come from the
/// matching block prefab.
/// </summary>
public enum TerrainType
{
    Field,
    Road,
    Water,
    Obstacle,
    Building,
    Bridge
}
