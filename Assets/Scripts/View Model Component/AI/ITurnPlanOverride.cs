/// <summary>
/// A status that dictates its owner's turn instead of the normal AI brain
/// (Scrambled acts randomly, Redline force-attacks). CommandSelectionState
/// asks the acting unit for one before consulting the battle's CPU; the
/// first override found in the hierarchy wins.
/// </summary>
public interface ITurnPlanOverride
{
    PlanOfAttack BuildPlan(BattleController bc, Unit actor);
}
