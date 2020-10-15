using UnityEngine;
using System.Collections;
using System.IO;
using I18N.CJK;
using System.Collections.Generic;

public class MenuResLoader:Singleton<MenuResLoader>
{
    //文件头 可能是60字节 第53字节开始 2字节是项数+2字节是除去头60字节后文件剩余字节数
    //对象[
    //    [武器]
    //    [动作]
    //    [物品]
    //    [镖物]
    //    [地图]
    //    ]
    public void Clear() {
        Info.Clear();
    }
    public List<Option> Info = new List<Option>();
    public Option FindOpt(int idx, int type)
    {
        for (int i = 0; i < Info.Count; i++)
        {
            if (Info[i].Type == type && Info[i].Idx == idx)
                return Info[i];
        }
        return null;
    }

    public Option GetPoseInfo(int idx)
    {
        return FindOpt(idx, 3);
    }

    public Option GetItemInfo(int idx)
    {
        return FindOpt(idx, 2);
    }

    public void Init()
    {
        if (Info.Count != 0)
            return;
        TextAsset asset = Resources.Load<TextAsset>("Meteor.res");
        MemoryStream ms = new MemoryStream(asset.bytes);
        BinaryReader binRead = new BinaryReader(ms);
        binRead.BaseStream.Seek(52, SeekOrigin.Begin);
        int itemCnt = binRead.ReadInt16();
        int bytesLeft = binRead.ReadInt16();
        binRead.BaseStream.Seek(4, SeekOrigin.Current);
        //武器每一项195字节 因为内部名称原因，各个大小不是定长的，可能在195左右
        //招式空-45字节 4字节3 4字节招式编号
        while (itemCnt != 0)
        {
            int type = binRead.ReadInt32();
            int Idx = binRead.ReadInt32();//包含属性的武器编号
            int charLength = binRead.ReadInt32();//包含/0在内的字符串长度是
            string iden = "";
            if (type == 3 || type == 1)
                iden = I18N.CJK.GB18030Encoding.GetEncoding(950).GetString(binRead.ReadBytes(charLength), 0, charLength - 1);
            else
                iden = I18N.CJK.GB18030Encoding.GetEncoding(936).GetString(binRead.ReadBytes(charLength), 0, charLength - 1);
            
            int modelLength = binRead.ReadInt32();
            string model = "";
            if (type == 3 || type == 1)
                model = I18N.CJK.GB18030Encoding.GetEncoding(950).GetString(binRead.ReadBytes(modelLength), 0, modelLength - 1);
            else
                model = I18N.CJK.GB18030Encoding.GetEncoding(936).GetString(binRead.ReadBytes(modelLength), 0, modelLength - 1);
            int lastLength = binRead.ReadInt32();
            string last = "";
            if (type == 3 || type == 1)
                last = I18N.CJK.GB18030Encoding.GetEncoding(950).GetString(binRead.ReadBytes(lastLength), 0, lastLength - 1);
            else
                last = I18N.CJK.GB18030Encoding.GetEncoding(936).GetString(binRead.ReadBytes(lastLength), 0, lastLength - 1);
            int firstBodyItemCnt = 0;
            int secBodyItemCnt = 0;
            Option op = new Option();
            op.Type = type;
            op.Idx = Idx;
            op.Identify = iden;
            op.model = model;

            firstBodyItemCnt = binRead.ReadInt32();
            op.first = new FirstOption[firstBodyItemCnt];
            for (int x = 0; x < firstBodyItemCnt; x++)
            {
                op.first[x] = new FirstOption();
                op.first[x].flag[0] = binRead.ReadInt32();
                op.first[x].flag[1] = binRead.ReadInt32();
                op.first[x].flag[2] = binRead.ReadInt32();
            }
            secBodyItemCnt = binRead.ReadInt32();
            op.second = new SecondOption[secBodyItemCnt];
            for (int x = 0; x < secBodyItemCnt; x++)
            {
                op.second[x] = new SecondOption();
                for (int y = 0; y < 10; y++)
                {
                    op.second[x].flag[y] = binRead.ReadInt32();
                }
            }
            //Debug.Log("type:" + type + ":idx" + Idx + ":" + iden +  " model:" + op.model);
            Info.Add(op);
            /*
            if (type == 3)//招式
            {
                string str = Idx + ":" + iden;
                if (op.second.Length == 1 && op.second[0].flag[2] == 3)
                    str += "-攻击力:" + op.second[0].flag[6];
                WSLog.LogInfo(str);
            }
            else if (type == 1)//武器
            {

                WSLog.LogInfo(Idx + ":" + iden + " 模型:" + op.model + "攻击:" + op.second[0].flag[6] + " 防御:" + op.second[1].flag[6] + " 移动速度:" + op.second[2].flag[6]);
            }
            else if (type == 2)//物品
            {
                WSLog.LogInfo(Idx + ":" + iden + " 模型:" + op.model);
            }
            else if (type == 4)//镖物
            {
                WSLog.LogInfo(Idx + ":" + iden + " 模型" + op.model);
            }
            else if (type == 5)//地图
            {
                WSLog.LogInfo(Idx + ":" + iden + " 模型" + op.model);
            }
            */
            itemCnt--; 
        }

        //Debug.Log("info loaded");
    }
}
[System.Serializable]
public class Option
{
    public int Idx;
    public string Identify;
    public string model;
    public int Type;
    public FirstOption[] first;
    public SecondOption[] second;
    public bool IsFlag() { return Type == 4; }
    public bool IsWeapon() { return Type == 1; }
    public bool IsItem() { return Type == 2; }
    public bool IsPose() { return Type == 3; }
    public bool IsMap() { return Type == 5; }
}
[System.Serializable]
public class FirstOption
{
    public int[] flag = new int[3];
}
[System.Serializable]
public class SecondOption
{
    public int[] flag = new int[10];
}