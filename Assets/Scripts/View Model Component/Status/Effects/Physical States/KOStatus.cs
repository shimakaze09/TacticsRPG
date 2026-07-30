using UnityEngine;

/// <summary>
/// KO (Knocked Out): Unit's HP reached 0. The body drops visibly (squashed
/// flat and ash-gray until the art pass brings real death animation).
/// CT still increases; when it reaches 100, death counter drops by 1 (from 3).
/// If counter reaches 0 on next active turn, becomes Crystal or Treasure.
/// </summary>
public class KOStatus : StatusEffect
{
    [Tooltip("Number of turns KO'd before becoming crystal/treasure")]
    public int deathCounter = 3;

    [Tooltip("How flat the body squashes while down (Y scale factor)")]
    public float downedSquash = 0.25f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly Color DeadTint = new Color(0.35f, 0.33f, 0.3f);

    private Unit owner;
    private Stats stats;
    private int currentCounter;
    private Transform body;
    private Vector3 bodyScale;

    private void OnEnable()
    {
        owner = GetComponentInParent<Unit>();
        stats = GetComponentInParent<Stats>();
        currentCounter = deathCounter;

        if (owner != null)
        {
            this.SubscribeToSender<TurnCheckEvent>(OnTurnCheck, owner);
        }

        if (stats != null)
        {
            // Subscribe to HP changes in case of revival
            this.SubscribeToSender<StatDidChangeEvent>(OnStatChanged, stats);
        }

        ApplyDeathPose();
    }

    private void OnDisable()
    {
        if (owner != null)
            this.UnsubscribeFromSender<TurnCheckEvent>(OnTurnCheck, owner);

        if (stats != null)
            this.UnsubscribeFromSender<StatDidChangeEvent>(OnStatChanged, stats);

        RestoreDeathPose();
    }

    // The fallen unit must READ as fallen: body squashed to the ground and
    // tinted dead — walkers already step over it, so the visual has to agree
    private void ApplyDeathPose()
    {
        if (owner == null)
            return;

        body = owner.transform.Find("Jumper");
        if (body == null)
            return;

        bodyScale = body.localScale;
        body.localScale = new Vector3(bodyScale.x, bodyScale.y * downedSquash, bodyScale.z);

        foreach (var renderer in body.GetComponentsInChildren<Renderer>())
        {
            var block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, DeadTint);
            renderer.SetPropertyBlock(block);
        }
    }

    // Revival stands the body back up in its original colors
    private void RestoreDeathPose()
    {
        if (body == null)
            return;

        body.localScale = bodyScale;
        foreach (var renderer in body.GetComponentsInChildren<Renderer>())
            renderer.SetPropertyBlock(null);
        body = null;
    }

    private void OnTurnCheck(TurnCheckEvent e)
    {
        // A KO'd unit never acts. Each time it *would* have activated, the
        // death counter ticks down instead (and CT resets, as in FFT).
        if (!e.Exception.toggle)
            return;

        e.Exception.FlipToggle();

        if (stats != null)
            stats.SetValue(StatTypes.CTR, 0, false);

        currentCounter--;
        if (currentCounter <= 0)
        {
            // Become crystal or treasure
            BecomePermaKO();
        }
    }

    private void OnStatChanged(StatDidChangeEvent e)
    {
        if (e.StatType != StatTypes.HP)
            return;

        // If HP is restored above 0, remove KO status
        if (e.NewValue > 0)
        {
            var cond = GetComponentInChildren<StatusCondition>();
            if (cond != null)
                cond.Remove();
            else
                Destroy(this);
        }
    }

    // Converts the fallen unit into collectible remains and removes it from
    // battle entirely — it no longer occupies its tile, takes scheduler
    // ticks, or appears in AI planning.
    private void BecomePermaKO()
    {
        bool isCore = Random.value > 0.5f;
        RemainsPickup.Spawn(owner, isCore);

        var bc = FindAnyObjectByType<BattleController>();
        if (bc != null)
            bc.units.Remove(owner);

        if (owner.tile != null && owner.tile.content == owner.gameObject)
            owner.tile.content = null;

        Destroy(owner.gameObject);
    }

    public int GetDeathCounter()
    {
        return currentCounter;
    }
}
