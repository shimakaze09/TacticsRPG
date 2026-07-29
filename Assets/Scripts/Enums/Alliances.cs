/// <summary>
/// Which side a unit fights for (None/Neutral/Hero/Enemy); flags so targeting
/// can match multiple sides.
/// </summary>
public enum Alliances
{
    None = 0,
    Neutral = 1 << 0,
    Hero = 1 << 1,
    Enemy = 1 << 2
}