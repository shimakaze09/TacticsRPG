using UnityEngine;

/// <summary>
/// Terrain line-of-sight for ranged targeting: a shot travels a straight line
/// between tile centers at standing height; any intermediate tile whose
/// terrain rises above that line blocks it, and sight-blocking terrain
/// (trees, buildings) blocks regardless of height. Units do not block shots
/// (v1 — only terrain does). Infinite ranges (Wakener engine strikes) bypass
/// this entirely by design.
/// </summary>
public static class LineOfSight
{
    /// <summary>How far above its tile a unit's eyes/muzzle sit.</summary>
    private const float StandingHeight = 1f;

    /// <summary>Grace margin so level ground never blocks a flat shot.</summary>
    private const float Tolerance = 0.25f;

    /// <summary>
    /// True when no terrain between the two tiles rises above the sight line
    /// interpolated from shooter to target (both at standing height).
    /// </summary>
    public static bool Clear(Board board, Tile from, Tile to)
    {
        if (board == null || from == null || to == null || from == to)
            return true;

        int dx = to.pos.x - from.pos.x;
        int dy = to.pos.y - from.pos.y;
        int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        if (steps <= 1)
            return true;

        for (var i = 1; i < steps; i++)
        {
            float t = (float)i / steps;
            var sample = new Point(
                from.pos.x + Mathf.RoundToInt(dx * t),
                from.pos.y + Mathf.RoundToInt(dy * t));
            if (sample == from.pos || sample == to.pos)
                continue;

            var tile = board.GetTile(sample);
            if (tile == null)
                continue;

            if (tile.BlocksSight)
                return false;

            float lineHeight = Mathf.Lerp(from.height, to.height, t) + StandingHeight;
            if (tile.height > lineHeight + Tolerance)
                return false;
        }

        return true;
    }
}
