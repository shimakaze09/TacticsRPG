using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One conversation participant: portrait, anchor side, and the messages they
/// speak.
/// </summary>
[Serializable]
public class SpeakerData
{
    public TextAnchor anchor;
    public List<string> messages;
    public Sprite speaker;
}