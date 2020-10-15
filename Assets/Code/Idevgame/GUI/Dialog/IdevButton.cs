using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;
public class IdevButton:MonoBehaviour, IPointerUpHandler
{
    [SerializeField]
    DialogAction Action;
    public void OnPointerUp(PointerEventData eventData) {
        Main.Ins.DialogStateManager.FireAction(Action);
    }
}
