using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class AudioButton : Button
{
    [SerializeField] private AudioAsset audioS = null;
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (audioS != null)
            SoundManager.Instance.PlaySound(audioS.clipName);
        base.OnPointerClick(eventData);
    }
}