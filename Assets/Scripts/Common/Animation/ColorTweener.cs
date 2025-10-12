using TMPro;

public class ColorTweener : Vector4Tweener
{
    private TextMeshProUGUI textMeshPro;

    private void Awake()
    {
        textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        textMeshPro.color = currentTweenValue;
    }
}