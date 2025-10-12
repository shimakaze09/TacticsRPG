using UnityEngine;

public class FacingIndicator : MonoBehaviour
{
    [SerializeField] private Renderer[] directions;
    [SerializeField] private Material normal;
    [SerializeField] private Material selected;

    public void SetDirection(Directions dir)
    {
        var index = (int)dir;
        for (var i = 0; i < 4; i++)
            directions[i].sharedMaterial = i == index ? selected : normal;
    }
}