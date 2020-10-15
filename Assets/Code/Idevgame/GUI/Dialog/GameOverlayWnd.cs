using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

public class GameOverlayDialogState : PersistDialog<GameOverlayWnd>
{
    public override string DialogName { get { return "GameOverlayWnd"; } }

    //自带画布，不属于画布之下的预设
    protected override bool CanvasMode()
    {
        return true;
    }

    //不要卸载，这个界面永久的.
    public override void OnStateExit() {

    }
}

public class GameOverlayWnd : Dialog  {
    public GameObject content;
    public AutoMsgCtrl ctrl;
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        GameObject.DontDestroyOnLoad(gameObject);
    }

    public static int MaxMsg = 10;
    public void InsertSystemMsg(string msg)
    {
        if (content != null)
        {
            GameObject obj = new GameObject();
            obj.name = (content.transform.childCount + 1).ToString();
            Text txt = obj.AddComponent<Text>();
            txt.text = msg;
            //00AAFFFF
            txt.font = Main.Ins.TextFont;
            txt.fontSize = 32;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.raycastTarget = false; 
            txt.color = new Color(1, 0XF8 / 255.0f, 0XDD/255.0F, 1);
            for (int i = 0; i < content.transform.childCount - MaxMsg; i++)
                GameObject.Destroy(content.transform.GetChild(i).gameObject);
            obj.transform.SetParent(content.transform);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

        }
    }

    public void ClearSystemMsg()
    {
        if (content != null)
        {
            for (int i = 0; i < content.transform.childCount; i++)
                GameObject.Destroy(content.transform.GetChild(i).gameObject);
        }
    }
}
