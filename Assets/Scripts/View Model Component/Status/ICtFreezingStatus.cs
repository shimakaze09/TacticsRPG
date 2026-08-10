/// <summary>
/// Marker for status effects that freeze the owner's CT entirely, denying it
/// any turns while active (FreezeFrame, Blackout, Graycast). Duration
/// conditions consult this to know when their owner cannot tick naturally
/// and the battle-round fallback clock must run instead (issue #57) —
/// without the marker, ordinary statuses on merely slow units would decay
/// early.
/// </summary>
public interface ICtFreezingStatus
{
}
