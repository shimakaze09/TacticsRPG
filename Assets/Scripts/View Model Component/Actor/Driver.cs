using UnityEngine;

/// <summary>
/// Selects who controls this unit each turn: human input or the AI (overridable
/// by statuses like Swayed).
/// </summary>
public class Driver : MonoBehaviour
{
    public Drivers normal;
    public Drivers special;

    public Drivers Current => special != Drivers.None ? special : normal;
}