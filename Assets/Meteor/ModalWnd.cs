using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ModalWnd : MonoBehaviour {
    public void OnClick()
    {
        WindowMng.OnModalClick(transform.parent.parent.gameObject);
    }
}
