using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class WayLoader
{
    //每进入一关加载唯一的一次.
    public static List<WayPoint> ReLoad(string file)
    {
        string key = file + ".wp";
        List<WayPoint> wp = new List<WayPoint>();
        byte[] body = null;
        //尝试先读DLC数据
        if (Main.Ins != null) {
            if (CombatData.Ins.Chapter != null) {
                body = CombatData.Ins.Chapter.GetResBytes(FileExt.WayPoint, file);
            }
        }
        TextAsset asset = null;
        if (body == null)
            asset = Resources.Load<TextAsset>(key);
        if (asset != null) {
            body = asset.bytes;
        }
        if (body != null) {
            MemoryStream ms = new MemoryStream(body);
            StreamReader read = new StreamReader(ms);
            int wayCount = 0;
            WayPoint wpeach = null;
            int wayPointIdx = 0;
            while (!read.EndOfStream) {
                string line = read.ReadLine();
                string[] eachline = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (eachline[0] == "WayPoints") {
                    wayCount = int.Parse(eachline[1]);
                } else if (eachline[0] == "Pos") {
                    if (wpeach != null)
                        wp.Add(wpeach);
                    wpeach = new WayPoint();
                    wpeach.pos = new Vector3(float.Parse(eachline[1]), float.Parse(eachline[3]), float.Parse(eachline[2]));
                    wpeach.index = wayPointIdx;
                    wayPointIdx++;
                } else if (eachline[0] == "Size") {
                    wpeach.size = int.Parse(eachline[1]);
                } else if (eachline[0] == "Link") {
                    wpeach.link = new SortedDictionary<int, WayLength>();
                } else {
                    WayLength wayl = new WayLength();
                    wayl.length = float.Parse(eachline[2]);
                    wayl.mode = int.Parse(eachline[1]);
                    wpeach.link.Add(int.Parse(eachline[0]), wayl);
                }
            }
            if (!wp.Contains(wpeach) && wayCount != 0)
                wp.Add(wpeach);
        }
        return wp;
    }
}
