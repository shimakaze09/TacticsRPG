using System.Collections;
using UnityEngine;

public class AnchorTests : MonoBehaviour
{
    [SerializeField] private readonly float delay = 0.5f;
    [SerializeField] private bool animated;

    private IEnumerator Start()
    {
        var anchor = GetComponent<LayoutAnchor>();
        while (true)
            for (var i = 0; i < 9; ++i)
            for (var j = 0; j < 9; ++j)
            {
                var a1 = (TextAnchor)i;
                var a2 = (TextAnchor)j;
                Debug.Log($"A1:{a1}   A2:{a2}");
                if (animated)
                {
                    var t = anchor.MoveToAnchorPosition(a1, a2, Vector2.zero);
                    while (t != null)
                        yield return null;
                }
                else
                {
                    anchor.SnapToAnchorPosition(a1, a2, Vector2.zero);
                }

                yield return new WaitForSeconds(delay);
            }
    }
}