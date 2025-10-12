using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationPanel : MonoBehaviour
{
    public GameObject arrow;
    public TextMeshProUGUI message;
    public Panel panel;
    public Image speaker;

    private void Start()
    {
        var pos = arrow.transform.localPosition;
        arrow.transform.localPosition = new Vector3(pos.x, pos.y + 5, pos.z);
        var t = arrow.transform.MoveToLocal(new Vector3(pos.x, pos.y - 5, pos.z), 0.5f, EasingEquations.EaseInQuad);
        t.loopType = EasingControl.LoopType.PingPong;
        t.loopCount = -1;
    }

    public IEnumerator Display(SpeakerData sd)
    {
        speaker.sprite = sd.speaker;
        speaker.SetNativeSize();

        for (var i = 0; i < sd.messages.Count; i++)
        {
            message.text = sd.messages[i];
            arrow.SetActive(i + 1 < sd.messages.Count);
            yield return null;
        }
    }
}