using UnityEngine;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Xml;
public class BMFontTool
{
    [MenuItem("Tools/Font")]
    static void Font()
    {
        Material mtr = Resources.Load<Material>("MeteorTimeFont"); //把我们创建的材质球加载进来
        Texture2D texture = Resources.Load<Texture2D>("meteorFont_0"); //把我们在位图制作工具生成的图片加载进来
        mtr.SetTexture(0, texture);//把图片赋给材质球

        Font font = Resources.Load<Font>("MeteorTimeFont"); //把我们创建的字体加载进来
        XmlDocument xml = new XmlDocument();
        xml.Load(Application.dataPath + "/Scene/Resources/meteorFont.fnt");//这是在BMFont里得到的那个.fnt文件,因为是xml文件，所以我们就用xml来解析
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
            float texWidth = texture.width;
            float texHeight = texture.width;

            chtInfo.glyphHeight = texture.height;
            chtInfo.glyphWidth = texture.width;
            chtInfo.index = int.Parse(xe.GetAttribute("id"));
            //这里注意下UV坐标系和从BMFont里得到的信息的坐标系是不一样的哦，前者左下角为（0,0），
            //右上角为（1,1）。而后者则是左上角上角为（0,0），右下角为（图宽，图高）
            chtInfo.uvTopLeft = new Vector2((float)x / texture.width, 1 - (float)y / texture.height);
            chtInfo.uvTopRight = new Vector2((float)(x + width) / texture.width, 1 - (float)y / texture.height);
            chtInfo.uvBottomLeft = new Vector2((float)x / texture.width, 1 - (float)(y + height) / texture.height);
            chtInfo.uvBottomRight = new Vector2((float)(x + width) / texture.width, 1 - (float)(y + height) / texture.height);

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
