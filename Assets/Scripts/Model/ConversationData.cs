using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset holding an ordered list of conversation speakers and their lines for
/// cutscenes.
/// </summary>
public class ConversationData : ScriptableObject
{
    public List<SpeakerData> list;
}