using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using CoClass;
using DG.Tweening;

public class ArmyShopCtrl : MonoBehaviour {
    public Text NpcName;
    public Text NpcTalk;
    // Use this for initialization
    public RectTransform ShopItemRoot;
    public static ArmyShopCtrl Ins;
    public Text page;
    public Text totalPrice;
    public Text number;
    public Text haveMoney;
    public Button[] ShopItem;

    public Button OnYes;
    public Button NextPage;
    public Button PrevPage;
    public Button MaxValue;
    public Button AddValue;
    public Button DelValue;
    public InputField inputNumber;
    public Text selectMonsterText;
    const int PageItem = 6;
    int[] PageItems = new int[PageItem];
    uint curPage;
    uint totalPage;
    uint MonsterPrice;
    uint MonsterCount;
    uint MonsterTotalPrice;
    int MonsterSelect = -1;
    void Awake()
    {
        Ins = this;
    }

    void OnDestroy()
    {
        Ins = null;
    }
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    int Job;
    List<int> DstArmyType = new List<int>();
    public void BindArmyType(int destType)
    {
        Job = destType;
        switch (Job)
        {
            case 0:NpcName.text = "招募处";break;
            case 1: NpcName.text = "刺客招募处"; break;
            case 2: NpcName.text = "剑客招募处"; break;
            case 3: NpcName.text = "刀客招募处"; break;
            case 4: NpcName.text = "枪豪招募处"; break;
            case 5: NpcName.text = "狙击手招募处"; break;
        }

        Dictionary<int, ArmyType> kv = new Dictionary<int, ArmyType>();
        kv.Add(1, ArmyType.Assassin);
        kv.Add(2, ArmyType.Swordman);
        kv.Add(3, ArmyType.Blademan);
        kv.Add(4, ArmyType.Lanceman);
        kv.Add(5, ArmyType.Marksman);

        OnYes.GetComponentInChildren<Text>().text = "招募";
        OnYes.onClick.RemoveAllListeners();
        OnYes.onClick.AddListener(OnBuyItem);
        //for (int i = 0; i < GameData.MainRole.OpenedArmyType.Count; i++)
        //{
        //    MonsterUnit unit = GameData.FindMonsterById(GameData.MainRole.OpenedArmyType[i]);
        //    if (Job != 0 && unit.Job == (int)kv[Job])
        //        DstArmyType.Add(unit.Idx);
        //    else if (Job == 0)
        //        DstArmyType.Add(unit.Idx);
        //}
    }

    //绑定好了数据后，准备显示前，把所有的页码之类的重置
    public void Reset()
    {
        curPage = 0;
        totalPage = 0;
        MonsterPrice = 0;
        MonsterCount = 0;
        MonsterTotalPrice = 0;
        MonsterSelect = -1;
        totalPage = (uint)(DstArmyType.Count / PageItem) + (uint)(DstArmyType.Count % PageItem == 0 ? 0 : 1);
        curPage = 1;
        if (totalPage < curPage)
            totalPage = curPage;
        PrevPage.gameObject.SetActive(false);
        if (totalPage == curPage)
            NextPage.gameObject.SetActive(false);
        AddValue.gameObject.SetActive(false);
        DelValue.gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        //haveMoney.text = PlayerEx.TotalMoney.ToString();
        page.text = curPage + "/" + totalPage;
        if (curPage == totalPage)
            NextPage.gameObject.SetActive(false);
        else
            NextPage.gameObject.SetActive(true);
        if (curPage == 1)
            PrevPage.gameObject.SetActive(false);
        else
            PrevPage.gameObject.SetActive(true);
        if (MonsterSelect == -1)
        {
            selectMonsterText.text = "";
            number.text = "";
            totalPrice.text = "";
        }
        else
        {
        }

        int k = 1;
        for (int i = 0; i < PageItem; i++)
        {
            PageItems[i] = -1;
            ShopItem[i].gameObject.SetActive(false);
        }

        foreach (var each in DstArmyType)
        {
            if (k > (curPage - 1) * PageItem && k <= (curPage * PageItem))
            {
                //MonsterUnit it = GameData.FindMonsterById(each);
                //PageItems[k - (curPage - 1) * PageItem - 1] = it.Idx;
                ShopItemCtrl item = ShopItem[k - (curPage - 1) * PageItem - 1].GetComponent<ShopItemCtrl>();
                //item.ItemName.text = it.Name;
                //item.ItemPrice.text = it.Coin.ToString();
                //ShopItem[k - (curPage - 1) * PageItem - 1].GetComponentInChildren<Text>().text = it.Name;
                ShopItem[k - (curPage - 1) * PageItem - 1].gameObject.SetActive(true);
            }
            k++;
        }

        k = 0;
        for (int i = 0; i < ShopItem.Length; i++)
        {
            if (ShopItem[i].IsActive())
                k++;
        }
        ShopItemRoot.sizeDelta = new Vector2(ShopItemRoot.sizeDelta.x, k * 100 + 20 + 20 + (k - 1) * 20);
    }

    public void OnAddNumberPer10()
    {
        if (MonsterSelect != -1)
        {
            MonsterCount += 10;
            UpdateNumber();
        }
    }

    public void OnAddNumberPer100()
    {
        if (MonsterSelect != -1)
        {
            MonsterCount += 100;
            UpdateNumber();
        }
    }

    public void OnAddNumberPer1000()
    {
        if (MonsterSelect != -1)
        {
            MonsterCount += 1000;
            UpdateNumber();
        }
    }

    public void OnAddNumber()
    {
        if (MonsterSelect != -1)
        {
            MonsterCount++;
            UpdateNumber();
        }
    }

    void UpdateNumber()
    {
        //if (MonsterSelect != -1)
        //{
        //    MonsterUnit it = GameData.FindMonsterById(MonsterSelect);
        //    if (it != null)
        //    {
        //        if (ItemCount > it.Stack)
        //            ItemCount = it.Stack;
        //    }
        //}

        number.text = MonsterCount.ToString();
        MonsterTotalPrice = MonsterCount * MonsterPrice;
        //if (MonsterTotalPrice > PlayerEx.TotalMoney)
        //    totalPrice.text = "<color=#ff0000ff>" + MonsterTotalPrice + "</color>";
        //else
        //    totalPrice.text = MonsterTotalPrice.ToString();
        if (inputNumber.IsActive())
            inputNumber.text = MonsterCount.ToString();
        //if (ItemCount > 1)
        //    Del.gameObject.SetActive(true);
        //if (ItemCount == 1)
        //    Del.gameObject.SetActive(false);
    }

    public void OnDelNumber()
    {
        if (MonsterSelect != -1)
        {
            if (MonsterCount > 1)
            {
                MonsterCount--;
                UpdateNumber();
            }
        }
    }

    public void OnDelNumberPer10()
    {
        if (MonsterSelect != -1)
        {
            if (MonsterCount > 10)
            {
                MonsterCount -= 10;
                UpdateNumber();
            }
        }
    }
    public void OnDelNumberPer100()
    {
        if (MonsterSelect != -1)
        {
            if (MonsterCount > 100)
            {
                MonsterCount -= 100;
                UpdateNumber();
            }
        }
    }
    public void OnDelNumberPer1000()
    {
        if (MonsterSelect != -1)
        {
            if (MonsterCount > 1000)
            {
                MonsterCount -= 1000;
                UpdateNumber();
            }
        }
    }
    public void OnSelItem(int id)
    {
        if (PageItems.Length > id && PageItems[id] != -1)
        {
            inputNumber.gameObject.SetActive(true);
            MaxValue.gameObject.SetActive(true);
            AddValue.gameObject.SetActive(true);
            DelValue.gameObject.SetActive(true);
            if (MonsterSelect != PageItems[id])
            {
                MonsterCount = 1;
                inputNumber.text = MonsterCount.ToString();
                MonsterPrice = 0;
                MonsterTotalPrice = 0;
                MonsterSelect = PageItems[id];
                //MonsterUnit it = GameData.FindMonsterById(MonsterSelect);
                //if (it != null)
                //{
                //    selectMonsterText.text = it.Desc.Replace("\\n", "\n");
                //    selectMonsterText.text += "\n生命:" + it.Hp;
                //    selectMonsterText.text += "\n攻击:" + it.Damage;
                //    selectMonsterText.text += "\n防御:" + it.Def;
                //    MonsterCount = 1;
                //    number.text = MonsterCount.ToString();
                //    MonsterPrice = it.Coin;
                //    MonsterTotalPrice = it.Coin * MonsterCount;

                //    if (MonsterTotalPrice > PlayerEx.TotalMoney)
                //        totalPrice.text = "<color=#ff0000ff>" + MonsterTotalPrice + "</color>";
                //    else
                //        totalPrice.text = MonsterTotalPrice.ToString();
                //}
            }
            else
                OnAddNumber();
        }
    }

    public void OnNextPage()
    {
        curPage++;
        MonsterSelect = -1;
        MonsterCount = 0;
        inputNumber.gameObject.SetActive(false);
        MaxValue.gameObject.SetActive(false);
        AddValue.gameObject.SetActive(false);
        DelValue.gameObject.SetActive(false);
        UpdateUI();
        UpdateNumber();
    }

    public void OnMaxValue()
    {
        if (MonsterSelect != -1)
        {
            //MonsterUnit it = GameData.FindMonsterById(MonsterSelect);
            //if (it != null)
            //{
            //    //MonsterCount = (uint)PlayerEx.TotalMoney / it.Coin;
            //    UpdateNumber();
            //}
        }
    }

    public void OnPrevPage()
    {
        curPage--;
        MonsterSelect = -1;
        MonsterCount = 0;
        inputNumber.gameObject.SetActive(false);
        MaxValue.gameObject.SetActive(false);
        AddValue.gameObject.SetActive(false);
        DelValue.gameObject.SetActive(false);
        UpdateUI();
        UpdateNumber();
    }

    public void OnClose()
    {
        WsWindow.Close(WsWindow.ArmyShop);
    }

    public void OnBuyItem()
    {
        //if (PlayerEx.TotalMoney < MonsterTotalPrice && MonsterTotalPrice > 0 && MonsterCount > 0 && MonsterSelect != -1)
        //{
        //    NpcTalk.text = "";
        //    NpcTalk.DOText("你的钱不够", 0.5f);
        //    return;
        //}
        //if (PlayerEx.TotalMoney >= MonsterTotalPrice && MonsterTotalPrice >= 0 && MonsterCount > 0 && MonsterSelect != -1)
        //{
        //    ////判断统率力是否足够.
        //    MonsterUnit it = GameData.FindMonsterById(MonsterSelect);
        //    //uint energy = (uint)it.energyCost * MonsterCount;
        //    //uint left = Player.GetLeftLeaderShip();
        //    //if (energy > left)
        //    //{
        //    //    NpcTalk.text = "";
        //    //    NpcTalk.DOText("你的统率力不足以统率如此多士兵，" + "您最多能再招募 " + StringTbl.unitPrefix + it.Name + StringTbl.unitSuffix + " X" + left / it.energyCost, 0.5f);
        //    //    return;
        //    //}
        //    U3D.AddArmy(MonsterSelect, (int)MonsterCount);
        //    U3D.UseMoney(MonsterTotalPrice);
        //    NpcTalk.text = "";
        //    NpcTalk.DOText("你花费了" + GameData.GetMoneyStr(MonsterTotalPrice) + " 招募了 " + StringTbl.unitPrefix + it.Name + StringTbl.unitSuffix + " X" + MonsterCount, 0.5f);
        //    UpdateUI();
        //}
    }

    public void OnInputNumber()
    {
        if (MonsterSelect == -1)
        {
            inputNumber.text = "";
            return;
        }
        try
        {
            int number = int.Parse(inputNumber.text);
            MonsterCount = (uint)number;
            UpdateNumber();
        }
        catch
        {
            inputNumber.text = "";
        }
    }
}
