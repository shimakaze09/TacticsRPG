using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LayoutAnchor : MonoBehaviour
{
    #region Fields / Properties

    private RectTransform myRT;
    private RectTransform parentRT;

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        myRT = transform as RectTransform;
        parentRT = transform.parent as RectTransform;
        if (parentRT == null)
            Debug.LogError("This component requires a RectTransform parent to work.", gameObject);
    }

    #endregion

    #region Public

    public void SnapToAnchorPosition(TextAnchor myAnchor, TextAnchor parentAnchor, Vector2 offset)
    {
        myRT.anchoredPosition = AnchorPosition(myAnchor, parentAnchor, offset);
    }

    public Tweener MoveToAnchorPosition(TextAnchor myAnchor, TextAnchor parentAnchor, Vector2 offset)
    {
        return myRT.AnchorTo(AnchorPosition(myAnchor, parentAnchor, offset));
    }

    public Vector2 AnchorPosition(TextAnchor myAnchor, TextAnchor parentAnchor, Vector2 offset)
    {
        var myOffset = GetPosition(myRT, myAnchor);
        var parentOffset = GetPosition(parentRT, parentAnchor);
        var anchorCenter = new Vector2(Mathf.Lerp(myRT.anchorMin.x, myRT.anchorMax.x, myRT.pivot.x),
            Mathf.Lerp(myRT.anchorMin.y, myRT.anchorMax.y, myRT.pivot.y));
        var myAnchorOffset = new Vector2(parentRT.rect.width * anchorCenter.x, parentRT.rect.height * anchorCenter.y);
        var myPivotOffset = new Vector2(myRT.rect.width * myRT.pivot.x, myRT.rect.height * myRT.pivot.y);
        var pos = parentOffset - myAnchorOffset - myOffset + myPivotOffset + offset;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);
        return pos;
    }

    #endregion

    #region Private

    private Vector2 GetPosition(RectTransform rt, TextAnchor anchor)
    {
        var retValue = Vector2.zero;

        switch (anchor)
        {
            case TextAnchor.LowerCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.UpperCenter:
                retValue.x += rt.rect.width * 0.5f;
                break;
            case TextAnchor.LowerRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.UpperRight:
                retValue.x += rt.rect.width;
                break;
        }

        switch (anchor)
        {
            case TextAnchor.MiddleLeft:
            case TextAnchor.MiddleCenter:
            case TextAnchor.MiddleRight:
                retValue.y += rt.rect.height * 0.5f;
                break;
            case TextAnchor.UpperLeft:
            case TextAnchor.UpperCenter:
            case TextAnchor.UpperRight:
                retValue.y += rt.rect.height;
                break;
        }

        return retValue;
    }

    #endregion
}