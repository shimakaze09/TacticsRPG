/// <summary>
/// The AI's chosen plan for a turn: which ability, from where, aimed at what,
/// facing which way — and in which order (move-then-act, or act-then-move
/// for hit-and-run turns).
/// </summary>
public class PlanOfAttack
{
    public Ability ability;
    public Directions attackDirection;
    public Point fireLocation;
    public Point moveLocation;
    public Targets target;

    /// <summary>
    /// When true, the unit attacks from its current tile first and then moves
    /// to postActMoveLocation (kiting). Otherwise it moves to moveLocation
    /// before acting, as normal.
    /// </summary>
    public bool actFirst;

    /// <summary>Retreat destination for act-first turns.</summary>
    public Point postActMoveLocation;
}
