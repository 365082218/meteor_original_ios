using System;
using System.Collections.Generic;
using UnityEngine;

class HeroAttrLabelItemControl : MonoBehaviour
{
    public GameObject AttrNameLabel = null;
    public GameObject AttrValueLabel = null;

    public void ShowContent(string name, string value)
    {
        AttrNameLabel.GetComponent<UILabel>().text = name;
        AttrValueLabel.GetComponent<UILabel>().text = value;
    }
}
