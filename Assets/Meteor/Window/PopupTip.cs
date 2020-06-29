using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

public class PopupTipController: SimpleDialogController<PopupTip>
{
    public override string DialogName { get { return "PopupTip"; } }
    public void Popup(string message)
    {
        Show();
        Instance.Popup(message);
    }
}

public class PopupTip : SimpleDialog {
    public Text message;
    public Text title;
    public float duraction = 3.0f;
	// Update is called once per frame
	void Update () {
        duraction -= Time.deltaTime;
        if (duraction <= 0.0f)
            Destroy(gameObject);
    }

    public void Popup(string str)
    {
        title.text = "消息";
        message.text = str;
        gameObject.FlyTo(duraction - 1.0f, 100.0f);
    }
}
