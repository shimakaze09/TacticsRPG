using UnityEngine;

/// <summary>
/// Battle camera: follows a target transform (usually the tile selection
/// indicator).
/// </summary>
public class CameraRig : MonoBehaviour
{
    private Transform _transform;
    public Transform follow;
    public float speed = 3f;

    private void Awake()
    {
        _transform = transform;
    }

    private void Update()
    {
        if (follow)
            _transform.position = Vector3.Lerp(_transform.position, follow.position, speed * Time.deltaTime);
    }
}