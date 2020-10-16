using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameButtonEx : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField]
    GameButton[] JoyButton;
    public void OnPointerDown(PointerEventData eventData)
    {
        for (int i = 0; i < JoyButton.Length; i++)
        {
            JoyButton[i].OnPointerDown(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        for (int i = 0; i < JoyButton.Length; i++)
        {
            JoyButton[i].OnPointerUp(eventData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        for (int i = 0; i < JoyButton.Length; i++)
        {
            JoyButton[i].OnPointerExit(eventData);
        }
    }
}