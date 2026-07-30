/// <summary>
/// How a ranged weapon's shot travels. Direct fire (guns) is stopped by
/// standing units and cover in the path; arcing fire (bows, lobbed shots)
/// sails over both — only sight-blocking terrain stops it.
/// </summary>
public enum WeaponArc
{
    Direct,
    Arcing
}
