using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class JoyAdjustWnd : Window<JoyAdjustWnd> {
    public override string PrefabName
    {
        get
        {
            return "JoyAdjustWnd";
        }
    }
    protected override int GetZ()
    {
        return -10;
    }
    protected override bool OnOpen()
    {
        Control("Close").GetComponent<Button>().onClick.AddListener(() => {
            Close();
        });
        return true;
    }
}
