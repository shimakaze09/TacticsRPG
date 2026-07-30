using System.Collections.Generic;

/// <summary>
/// The gameplay law for each terrain type: which locomotion may pass through
/// a tile, which may end a move on it, whether it blocks line of sight, and
/// its default block prefab. Design constants, like StatLimits — code, not
/// per-tile data, so every map obeys the same physics.
/// </summary>
public static class TerrainRules
{
    private struct Entry
    {
        public TileTraversalFlags pass;
        public TileTraversalFlags stop;
        public bool blocksSight;
        public string skin;
    }

    private const TileTraversalFlags All =
        TileTraversalFlags.Ground | TileTraversalFlags.Fly | TileTraversalFlags.Teleport;

    private static readonly Dictionary<TerrainType, Entry> table = new Dictionary<TerrainType, Entry>
    {
        // Open ground: everything moves and stands here
        [TerrainType.Field] = new Entry { pass = All, stop = All, blocksSight = false, skin = "Tile" },
        [TerrainType.Road] = new Entry { pass = All, stop = All, blocksSight = false, skin = "Road" },
        // Rivers/floodwater: flyers cross and hover, teleporters blink over
        // but can't end submerged, walkers are simply stopped
        [TerrainType.Water] = new Entry
        {
            pass = TileTraversalFlags.Fly | TileTraversalFlags.Teleport,
            stop = TileTraversalFlags.Fly,
            blocksSight = false,
            skin = "Water"
        },
        // Trees/wreckage: flyers clear the canopy, nobody stands in it,
        // and it hides whatever is behind it
        [TerrainType.Obstacle] = new Entry
        {
            pass = TileTraversalFlags.Fly,
            stop = TileTraversalFlags.None,
            blocksSight = true,
            skin = "Obstacle"
        },
        // Intact structures: a hard wall for every locomotion and for sight
        [TerrainType.Building] = new Entry
        {
            pass = TileTraversalFlags.None,
            stop = TileTraversalFlags.None,
            blocksSight = true,
            skin = "Building"
        },
        // Spans over water: mechanically open ground, its own type so maps
        // read and paint as bridges
        [TerrainType.Bridge] = new Entry { pass = All, stop = All, blocksSight = false, skin = "Bridge" }
    };

    public static TileTraversalFlags Pass(TerrainType terrain)
    {
        return table[terrain].pass;
    }

    public static TileTraversalFlags Stop(TerrainType terrain)
    {
        return table[terrain].stop;
    }

    public static bool BlocksSight(TerrainType terrain)
    {
        return table[terrain].blocksSight;
    }

    /// <summary>Default block prefab name (under Resources/Prefabs/Blocks).</summary>
    public static string Skin(TerrainType terrain)
    {
        return table[terrain].skin;
    }

    /// <summary>
    /// Terrain for a block prefab name — how levels saved before terrain
    /// data existed get their types back.
    /// </summary>
    public static TerrainType FromSkin(string prefabName)
    {
        foreach (var pair in table)
        {
            if (pair.Value.skin == prefabName)
                return pair.Key;
        }

        return TerrainType.Field;
    }
}
