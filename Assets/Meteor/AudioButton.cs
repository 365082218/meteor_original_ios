using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class AudioButton : Button
{
    [SerializeField] private AudioAsset audio = null;
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (audio != null)
            SoundManager.Instance.PlaySound(audio.clipName);
        base.OnPointerClick(eventData);
    }
}