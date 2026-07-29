using UnityEngine;

/// <summary>
/// Scene music: plays an intro clip once, then the loop clip forever via
/// AudioSequence.
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    public AudioClip introClip;
    public AudioClip loopClip;
    public AudioSequence sequence { get; private set; }

    private void Start()
    {
        sequence = gameObject.AddComponent<AudioSequence>();
        sequence.Play(introClip, loopClip);
        var data = sequence.GetData(loopClip);
        data.source.loop = true;
    }
}