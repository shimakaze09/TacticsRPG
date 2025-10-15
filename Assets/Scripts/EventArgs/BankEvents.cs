/// <summary>
/// Event published when the bank's gold amount changes
/// </summary>
public class GoldChangedEvent
{
    public int PreviousAmount { get; }
    public int NewAmount { get; }

    public GoldChangedEvent(int previousAmount, int newAmount)
    {
        PreviousAmount = previousAmount;
        NewAmount = newAmount;
    }
}
