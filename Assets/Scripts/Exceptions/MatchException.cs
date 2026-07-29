/// <summary>
/// A BaseException carrying attacker and target, used for automatic-hit/miss
/// checks.
/// </summary>
public class MatchException : BaseException
{
    public readonly Unit attacker;
    public readonly Unit target;

    public MatchException(Unit attacker, Unit target) : base(false)
    {
        this.attacker = attacker;
        this.target = target;
    }
}