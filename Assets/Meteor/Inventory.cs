using CoClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//服务器版本的背包.服务器已砍

    public static class InventoryItemHelper
    {
        public static bool IsEquip(this InventoryItem item)
        {
            return (GameData.FindItemByIdx(item.Idx)).MainType == (int)UnitType.Equip;
        }

        public static int type(this InventoryItem item)
        {
            return (GameData.FindItemByIdx(item.Idx)).MainType;
        }

        public static bool IsSpecail(this InventoryItem item)
        {
            return (GameData.FindItemByIdx(item.Idx)).MainType == (int)UnitType.Special;
        }

        public static bool IsMaterial(this InventoryItem item)
        {
            return (GameData.FindItemByIdx(item.Idx).MainType == (int)UnitType.Material);
        }

        public static string Name(this InventoryItem item)
        {
            return StringTbl.unitPrefix + (GameData.FindItemByIdx(item.Idx)).Name + StringTbl.unitSuffix;
        }

        public static ItemBase Info(this InventoryItem item)
        {
            return (GameData.FindItemByIdx(item.Idx));
        }
    }

    
    public static class UnitBaseHelper
    {
        public static string[] colorPrefix = { "<color=#ffffffff>", "<color=#1eff00ff>", "<color=#0081ffff>", "<color=#c600ffff>", "<color=#ff8000ff>", "<color=#e5cc80ff>" };
        public static string colorSuffix = "</color>";
        public static string[] desfix = { "", "(精密的)", "(上古)", "(绝世)" };
        public static string ColorName(this ItemBase info)
        {
            return colorPrefix[info.Quality] + info.Name + desfix[info.Quality] + colorSuffix;
        }

        public static bool IsMaterial(this ItemBase item)
        {
            return (item.MainType == (int)UnitType.Material);
        }
        public static bool IsSpecail(this ItemBase item)
        {
            return (item.MainType == (int)UnitType.Special);
        }
        public static bool IsEquip(this ItemBase item)
        {
            return (item.MainType == (int)UnitType.Equip);
        }
    }

    
    public class Inventory
    {
        public Inventory()
        {

        }
        public Inventory(List<InventoryItem> it)
        {
            if (it != null)
                items = it;
        }
        List<InventoryItem> items = new List<InventoryItem>();

        public List<int> GetItemIdxs()
        {
            List<int> lst = new List<int>();
            for (int i = 0; i < items.Count; i++)
            {
                if (!lst.Contains(items[i].Idx))
                    lst.Add(items[i].Idx);
            }
            return lst;
        }

        public Dictionary<int, uint> GetAllItemCnt()
        {
            Dictionary<int, uint> itAndCnt = new Dictionary<int, uint>();
            List<int> it = GetItemIdxs();
            for (int i = 0; i < it.Count; i++)
            {
                uint cnt = GetItemCount(it[i]);
                itAndCnt[it[i]] = cnt;
            }
            return itAndCnt;
        }
        

        public List<InventoryItem> Item { get { return items; } }
        public List<uint> GetItemId()
        {
            List<uint> ret = new List<uint>();
            for (int i = 0; i < Item.Count; i++)
                ret.Add(Item[i].ItemId);
            return ret;
        }

        //完全合并，则返回真，否则返回假，代表可能有部分合并.
        public bool JoinItem(InventoryItem injoin)
        {
            if (injoin.type() == (int)UnitType.Equip || injoin.type() == (int)UnitType.Book)
                return false;
            for (int i = 0; i < items.Count; i++)
            {
                ItemBase it = GameData.FindItemByIdx(items[i].Idx);
                if (items[i].Idx == injoin.Idx && items[i].Count + injoin.Count <= it.Stack)
                {
                    items[i].Count += injoin.Count;
                    injoin.Count = 0;
                    return true;
                }
                if (items[i].Idx == injoin.Idx && items[i].Count == it.Stack)
                    continue;
                if (items[i].Idx == injoin.Idx && injoin.Count == it.Stack)
                    return false;//不需要合并，因为新物品是满堆叠的
                if (items[i].Idx == injoin.Idx)
                {
                    uint remove = (uint)it.Stack - items[i].Count;
                    items[i].Count = it.Stack;
                    injoin.Count -= remove;
                }
            }
            return false;
        }

        //把某个物品叠加
        public bool JoinItem(int idx, uint cnt)
        {
            for (int i = 0; i < Item.Count; i++)
            {
                try
                {
                    if (Item[i].Idx == idx && ((Item[i].Count + cnt) <= Item[i].Info().Stack))
                    {
                        Item[i].Count += cnt;
                        string text = "得到物品:" + Item[i].Name() + ":" + cnt;
                        U3D.PopupTip(text);
                        return true;
                    }
                }
                catch
                {
                    
                }
            }
            return false;
        }
        //是否存在道具相同ID的物品。此函数只用来判断除装备以外的物品，因为只有非装备才能堆叠.
        public bool ExistItem(int idx)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Idx == idx)
                    return true;
            }
            return false;
        }

        //银两只是一个数字，不再放到背包里,可以提取银两到物品里
        //public uint GetTotalMoney()
        //{
        //    for (int i = 0; i < items.Count; i++)
        //    {
        //        if (items[i].Idx == (int)UnitSpecial.Money)
        //            return (uint)items[i].Count;
        //    }
        //    return 0;
        //}

        public uint GetItemCount(int idx)
        {
            uint existcnt = 0;
            try
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Idx == idx)
                        existcnt += (uint)items[i].Count;
                }
            }
            catch
            {
                //越界了
                return uint.MaxValue;
            }
            return existcnt;
        }

        //调用之前，需要自行判断是否拥有足够数量的物品
        public void RemoveItemCnt(int idx, uint cnt)
        {
            List<InventoryItem> willRemove = new List<InventoryItem>();
            for (int i = 0; i < Item.Count; i++)
            {
                if (items[i].Idx == idx)
                {
                    if (cnt <= items[i].Count)
                    {
                        items[i].Count -= cnt;
                        cnt = 0;
                        if (items[i].Count == 0)
                            willRemove.Add(items[i]);
                        break;
                    }
                    else
                    {
                        willRemove.Add(items[i]);
                        cnt -= items[i].Count;
                    }
                }
            }

            for (int i = 0; i < willRemove.Count; i++)
                items.Remove(willRemove[i]);
        }

        //public uint GetTotalExp()
        //{
        //    for (int i = 0; i < items.Count; i++)
        //    {
        //        if (items[i].Idx == (int)UnitId.Exp)
        //            return (uint)items[i].Count;
        //    }
        //    return 0;
        //}


        //
        public bool HaveEnoughItem(int idx, uint cnt)
        {
            uint existcnt = 0;
            try
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Idx == idx)
                        existcnt += items[i].Count;
                }
            }
            catch
            {
                //越界了,一定是物品数量总和大于请求数字
                return true;
            }
            return existcnt >= cnt;
        }

        public void Clear()
        {
            Item.Clear();
        }
    }
