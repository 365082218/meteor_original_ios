using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Xml;
public class BMFontEditor:EditorWindow
{
    [MenuItem("Tools/BMFont Maker")]
    static public void OpenBMFontMaker()
    {
        EditorWindow.GetWindow<BMFontEditor>(false, "BMFont Maker", true).Show();
    }

    [SerializeField]
    private Font targetFont;
    [SerializeField]
    private TextAsset fntData;
    [SerializeField]
    private Material fontMaterial;
    [SerializeField]
    private Texture2D fontTexture;

    private BMFont bmFont = new BMFont();

    public BMFontEditor()
    {
    }
    void OnGUI()
    {
        targetFont = EditorGUILayout.ObjectField("Target Font", targetFont, typeof(Font), false) as Font;
        fntData = EditorGUILayout.ObjectField("Fnt Data", fntData, typeof(TextAsset), false) as TextAsset;
        fontMaterial = EditorGUILayout.ObjectField("Font Material", fontMaterial, typeof(Material), false) as Material;
        fontTexture = EditorGUILayout.ObjectField("Font Texture", fontTexture, typeof(Texture2D), false) as Texture2D;

        if (GUILayout.Button("CreateFont"))
        {
            fontMaterial.SetTexture(0, fontTexture);//把图片赋给材质球

            Font font = targetFont; //把我们创建的字体加载进来
            XmlDocument xml = new XmlDocument();
            xml.Load(AssetDatabase.GetAssetPath(fntData));//这是在BMFont里得到的那个.fnt文件,因为是xml文件，所以我们就用xml来解析
            List<CharacterInfo> chtInfoList = new List<CharacterInfo>();
            XmlNode node = xml.SelectSingleNode("font/chars");
            foreach (XmlNode nd in node.ChildNodes)
            {
                XmlElement xe = (XmlElement)nd;
                int x = int.Parse(xe.GetAttribute("x"));
                int y = int.Parse(xe.GetAttribute("y"));
                int width = int.Parse(xe.GetAttribute("width"));
                int height = int.Parse(xe.GetAttribute("height"));
                int advance = int.Parse(xe.GetAttribute("xadvance"));
                CharacterInfo chtInfo = new CharacterInfo();
                float texWidth = fontTexture.width;
                float texHeight = fontTexture.width;

                chtInfo.glyphHeight = fontTexture.height;
                chtInfo.glyphWidth = fontTexture.width;
                chtInfo.index = int.Parse(xe.GetAttribute("id"));
                //这里注意下UV坐标系和从BMFont里得到的信息的坐标系是不一样的哦，前者左下角为（0,0），
                //右上角为（1,1）。而后者则是左上角上角为（0,0），右下角为（图宽，图高）
                chtInfo.uvTopLeft = new Vector2((float)x / fontTexture.width, 1 - (float)y / fontTexture.height);
                chtInfo.uvTopRight = new Vector2((float)(x + width) / fontTexture.width, 1 - (float)y / fontTexture.height);
                chtInfo.uvBottomLeft = new Vector2((float)x / fontTexture.width, 1 - (float)(y + height) / fontTexture.height);
                chtInfo.uvBottomRight = new Vector2((float)(x + width) / fontTexture.width, 1 - (float)(y + height) / fontTexture.height);

                chtInfo.minX = 0;
                chtInfo.minY = -height;
                chtInfo.maxX = width;
                chtInfo.maxY = 0;

                chtInfo.advance = advance;

                chtInfoList.Add(chtInfo);
            }
            font.characterInfo = chtInfoList.ToArray();
            AssetDatabase.Refresh();
        }
    }
}
