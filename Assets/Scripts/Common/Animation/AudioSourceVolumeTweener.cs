using UnityEngine;

public class AudioSourceVolumeTweener : Tweener
{
    protected AudioSource _source;

    public AudioSource source
    {
        get
        {
            if (_source == null)
                _source = GetComponent<AudioSource>();
            return _source;
        }
        set => _source = value;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        source.volume = currentValue;
    }
}