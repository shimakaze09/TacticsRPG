/// <summary>
/// The AI's chosen plan for a turn: which ability, from where, aimed at what,
/// facing which way.
/// </summary>
public class PlanOfAttack
{
    public Ability ability;
    public Directions attackDirection;
    public Point fireLocation;
    public Point moveLocation;
    public Targets target;
}