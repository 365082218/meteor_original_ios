using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class MoreGameCtrl : MonoBehaviour {
    public Image Preview;
    public Text Desc;
    public Button Install;
    public Text Title;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Load(AdGame game)
    {
        Title.text = game.Name;
        Desc.text = game.Desc;
        string urlIconPreview = game.Preview;
        try
        {
            WebClient preview = new WebClient();
            byte[] bitIcon = preview.DownloadData(urlIconPreview);
            if (bitIcon != null && bitIcon.Length != 0)
            {
                Texture2D tex = new Texture2D(200, 150, TextureFormat.ARGB32, false);
                tex.LoadImage(bitIcon);
                Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }
            preview.Dispose();
            preview = null;
        }
        catch
        {

        }

        Install.onClick.AddListener(() => { Application.OpenURL(game.Url + game.Query); });
    }
}
