using System.Collections.Generic;

/// <summary>
/// Event arguments for ability system.
/// Handles ability execution, validation, and effect application.
/// </summary>

/// <summary>
/// Raised to check if an ability can be performed.
/// Allows costs, cooldowns, and status effects to prevent execution.
/// </summary>
public class AbilityCanPerformCheckEvent
{
    public Ability Ability { get; }
    public BaseException Exception { get; }

    public AbilityCanPerformCheckEvent(Ability ability, BaseException exception)
    {
        Ability = ability;
        Exception = exception;
    }
}

/// <summary>
/// Raised when an ability fails to execute (e.g., insufficient resources).
/// </summary>
public class AbilityFailedEvent
{
    public Ability Ability { get; }

    public AbilityFailedEvent(Ability ability)
    {
        Ability = ability;
    }
}

/// <summary>
/// Raised when an ability successfully executes.
/// Triggers resource consumption and cooldowns.
/// </summary>
public class AbilityDidPerformEvent
{
    public Ability Ability { get; }

    public AbilityDidPerformEvent(Ability ability)
    {
        Ability = ability;
    }
}

/// <summary>
/// Raised to gather attack stat modifiers for ability damage calculation.
/// </summary>
public class GetAttackStatEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public List<ValueModifier> Modifiers { get; }

    public GetAttackStatEvent(Unit attacker, Unit target, List<ValueModifier> modifiers)
    {
        Attacker = attacker;
        Target = target;
        Modifiers = modifiers;
    }
}

/// <summary>
/// Raised to gather defense stat modifiers for ability damage calculation.
/// </summary>
public class GetDefenseStatEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public List<ValueModifier> Modifiers { get; }

    public GetDefenseStatEvent(Unit attacker, Unit target, List<ValueModifier> modifiers)
    {
        Attacker = attacker;
        Target = target;
        Modifiers = modifiers;
    }
}

/// <summary>
/// Raised to gather power modifiers for ability effects.
/// </summary>
public class GetPowerEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public List<ValueModifier> Modifiers { get; }

    public GetPowerEvent(Unit attacker, Unit target, List<ValueModifier> modifiers)
    {
        Attacker = attacker;
        Target = target;
        Modifiers = modifiers;
    }
}

/// <summary>
/// Raised to allow final damage tweaking before application.
/// </summary>
public class TweakDamageEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public List<ValueModifier> Modifiers { get; }

    public TweakDamageEvent(Unit attacker, Unit target, List<ValueModifier> modifiers)
    {
        Attacker = attacker;
        Target = target;
        Modifiers = modifiers;
    }
}

/// <summary>
/// Raised when an ability attack misses its target.
/// </summary>
public class AbilityMissedEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }

    public AbilityMissedEvent(Unit attacker, Unit target)
    {
        Attacker = attacker;
        Target = target;
    }
}

/// <summary>
/// Raised when an ability attack hits its target.
/// </summary>
public class AbilityHitEvent
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public int Damage { get; }

    public AbilityHitEvent(Unit attacker, Unit target, int damage)
    {
        Attacker = attacker;
        Target = target;
        Damage = damage;
    }
}
