using System;

/// <summary>
/// Flags for what kinds of locomotion a tile permits.
/// </summary>
[Flags]
public enum TileTraversalFlags
{
    None = 0,
    Ground = 1 << 0,
    Fly = 1 << 1,
    Teleport = 1 << 2,
}