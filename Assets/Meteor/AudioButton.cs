using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class AudioButton : Button
{
    [SerializeField] private AudioClip audioS = null;
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (audioS != null)
            SoundManager.Ins.PlaySound(audioS.name);
        base.OnPointerClick(eventData);
    }
}