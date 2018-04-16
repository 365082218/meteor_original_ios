using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShopWnd : Window<ShopWnd>
{

    public override string PrefabName { get { return "UICommonWndPrefab"; } }
    enum ConfirmTag
    {
        timeRefresh = 1,
        DiaRefresh = 2,
        Bug = 3,
        UpgradeShop = 4
    }
    private bool IsRefresh = false;
    UILabel PlayerManualLabel;
    UILabel PlayerGoldLabel;
    UILabel LimitManualLabel;
    UILabel TimeLabel;
    UISprite Titlespt;
    public int curSelectShopType = 1;
    private int curSelectIndex = 0;


    GameObject ShopList;
    GameObject RefreshBtn;
    GameObject UpgradeShopBtn;
    GameObject WareTableList;
    GameObject mConfirmBugPanel;
    GameObject mCurSlectItem = null;
    private GameObject LeftBtn;
    private GameObject RightBtn;
    private List<GameObject> newWareList = new List<GameObject>();


    public CloseThisAndBackWnd mCloseThisAndBackWnd = CloseThisAndBackWnd.BackTo_None;

    //关闭此界面之后打开的界面，添加类型枚举，在return函数里调用相应界面的show函数
    public enum CloseThisAndBackWnd
    {
        BackTo_None = 0,
        BackTo_HeroMainWnd,
        BackTo_JingJiChangMainWnd,
    };

    protected override int GetZ()
    {
        return 0;
    }

    protected override bool OnOpen()
    {
        base.OnOpen();
        LoadWndPrefab();
        Init();
        refresh();
        return true;
    }

    void Init()
    {
        ShopList = Control("ShopList");
        PlayerManualLabel = Control("PlayerManualLabel").GetComponent<UILabel>() as UILabel;
        PlayerGoldLabel = Control("PlayerGoldLabel").GetComponent<UILabel>() as UILabel;
        LimitManualLabel = Control("LimitManualLabel").GetComponent<UILabel>() as UILabel;
        TimeLabel = Control("TimeLabel").GetComponent<UILabel>();
        RefreshBtn = Control("RefreshBtn");
        mConfirmBugPanel = ldaControl("ConfirmBugPanel");
        LeftBtn=ldaControl("LeftBtn");
        RightBtn = ldaControl("RightBtn");
        UpgradeShopBtn = ldaControl("UpgradeShopBtn");
        UIEventListener.Get(ldaControl("Returnbutton")).onClick = Return;
        UIEventListener.Get(ldaControl("RefreshBtn")).onClick = OnClickRefresh;
        UIEventListener.Get(UpgradeShopBtn).onClick = OnClickUpgradeShop;
        UIEventListener.Get(LeftBtn).onClick = OnLeftBtnClick;
        UIEventListener.Get(RightBtn).onClick = OnRightBtnClick;
        //GameData.Instance.RegisterHandler(refresh);
    }

    private void LoadWndPrefab()
    {
        UIAtlas atlas = Resources.Load("UI/Shop/Atlas/ShopAtlas", typeof(UIAtlas)) as UIAtlas;//系统名称.
        Titlespt = ldaControl("JueseNameSprite").GetComponent<UISprite>() as UISprite;
        Titlespt.gameObject.SetActive(false);
        Titlespt = ldaControl("Title1Sprite").GetComponent<UISprite>() as UISprite;
        Titlespt.spriteName = "title_shop1";
        Titlespt.atlas = atlas;
        Titlespt.transform.localScale = new Vector3(26, 48, 1);

        ldaControl("TitlePanel").gameObject.SetActive(true);
        CreateGameObjectByPrefabName("ShopWnd", "MiddleMiddle");

    }

    void OnNameBtnClick(GameObject go)
    {

        return;
    }

    public void refresh()
    {
        //if (!WndObject.activeSelf)
        //{
        //    IsRefresh = true;
        //    return;
        //}

        //RequestCmd request = new RequestCmd();
        //request.cmd = CMD.OpenShopAction;
        //Global.ShowLoadingStart();
        //Request(request, delegate(string err, Response response)
        //{
        //    Global.ShowLoadingEnd();
        //    if (response == null)
        //        return;
        //    ResOpenShopsVO vo = response.ParseVO<ResOpenShopsVO>();
        //    if (vo != null)
        //    {
        //        List<OpenShopVO> M_shops = vo.M_shops;
        //        if (M_shops == null)
        //            return;
        //        m_Shop.Clear();
        //        m_Shop = M_shops;
        //        SetRefreshTime(TimeLabel.gameObject, m_Shop[0].refreshTime);
        //    }
        //    if (m_Shop.Count < 1) return;
        //    curSelectShopType = m_Shop[curSelectIndex].shopType;
        //    for (int shopId = 0; shopId < m_Shop.Count; shopId++)
        //    {
        //        SetWareTable(shopId);
        //    }
        //});

        //PlayerInfoVO playerInfoVo = GameData.Instance.InfoVO;

        //PlayerManualLabel.text = playerInfoVo.energy.ToString();
        //PlayerGoldLabel.text = GoldFormat(playerInfoVo.gold);

        //int EnergyLim = 0;// ExpManager.Instance.GetItem(GameData.Instance.InfoVO.level).maxEnergy;
        //LimitManualLabel.GetComponent<UILabel>().text = "/" + EnergyLim;

    }
    private void ChangeShopBtn()
    {
        curSelectShopType = 0;

        if (curSelectShopType > 3)
        {
            UpgradeShopBtn.SetActive(false);
        }
        else
        {
            UpgradeShopBtn.SetActive(true);
        }

        int ShopListCnt = ShopList.transform.childCount;
        for (int count = 0; count < ShopListCnt; count++)
        {
            GameObject child = ShopList.transform.GetChild(count).gameObject;
            if (child != null)
            {
            }

        }

      
    }

    public void OnClickRefresh(GameObject go)
    {

        if (go != null)
        {
            string tips = LanguagesManager.Instance.GetItem("Shop_Refresh_Tips");
            string refreshBtn = LanguagesManager.Instance.GetItem("Shop_Refresh_Btn");
            string NorefreshBtn = LanguagesManager.Instance.GetItem("Shop_NoRefresh_Btn");
            string Title = LanguagesManager.Instance.GetItem("Shop_Refresh_Title");

            BigConfirmWnd<ShopWnd> cwnd = new BigConfirmWnd<ShopWnd>();
            cwnd.OnOpen();
            cwnd.SetParentWnd(this, (int)ConfirmTag.DiaRefresh, tips, refreshBtn, NorefreshBtn, Title);
        }
        else
        {
            RefreshRp((int)ConfirmTag.timeRefresh);
        }

    }
    public void OnClickUpgradeShop(GameObject go)
    {
        if (curSelectShopType < 4)
        {
            //ShopSetting shopSetting = ShopSettingManager.Instance.GetItem(curSelectShopType + 1);
            //string pageDia = shopSetting.needDiamond.ToString();
            //string tips = LanguagesManager.Instance.GetItem("Shop_UpgradeShop");
            //tips = tips.Replace("$0", pageDia);
            //string upgradeBtn = LanguagesManager.Instance.GetItem("Shop_Upgrade_Title");
            //string Title = LanguagesManager.Instance.GetItem("Shop_Upgrade_Title");

            //if (GameData.Instance.InfoVO.level >= shopSetting.needLevel)
            //{
            //    BigConfirmWnd<ShopWnd> cwnd = new BigConfirmWnd<ShopWnd>();
            //    cwnd.OnOpen();
            //    cwnd.SetParentWnd(this, (int)ConfirmTag.UpgradeShop, tips, upgradeBtn, "", Title);
            //}
            //else
            //{
            //    UpgradeShopRp(curSelectShopType);

            //}
        }

    }

    public void OnLeftBtnClick(GameObject go)
    {
        if (curSelectIndex > 0)
        {
            curSelectIndex = curSelectIndex - 1;
        }
        else
        {

        }
        ChangeShopBtn();
        //Titlespt.spriteName = GetShopType(curSelectShopType).ToString();
    }

    public void OnRightBtnClick(GameObject go)
    {
       
        ChangeShopBtn();
        //Titlespt.spriteName = GetShopType(curSelectShopType).ToString();
    
    
    
    }

    protected override bool OnConfirm(bool confirm, int tag)
    {
        switch (tag)
        {
            case (int)ConfirmTag.DiaRefresh:
                if (confirm)
                {
                    RefreshRp((int)ConfirmTag.DiaRefresh);
                }
                break;
            case (int)ConfirmTag.Bug:
                if (confirm)
                {
                    BugRp();
                }
                mConfirmBugPanel.SetActive(false);
                break;
            case (int)ConfirmTag.UpgradeShop:
                if (confirm)
                {
                    UpgradeShopRp(curSelectShopType);
                }
                break;

        }
        return true;

    }

    public void RefreshRp(int intValue1)
    {
        //RequestCmd request = new RequestCmd();
        //request.cmd = CMD.RefreshShopAction;
        //RParamVO param = new RParamVO();
        //param.intValue1 = intValue1;
        //param.intValue2 = curSelectShopType;
        //request.param = param;
        //Global.ShowLoadingStart();
        //Request(request, delegate(string err, Response response)
        //{
        //    Global.ShowLoadingEnd();
        //    if (response == null)
        //        return;
        //    OpenShopVO vo = response.ParseVO<OpenShopVO>();
        //    if (vo != null)
        //    {

        //        int count = ShopList.transform.childCount;
        //        for (int i = 0; i < count; i++)
        //        {
        //            GameObject WareTableList = ShopList.transform.GetChild(i).gameObject;
        //            UIVO uivo = WareTableList.GetComponent<UIVO>();
        //            int Type = uivo.val;
        //            if (WareTableList != null && Type == curSelectShopType)
        //            {
        //                SetRefreshTime(TimeLabel.gameObject, vo.refreshTime);
        //                m_Shop[i] = vo;
        //                GameObject.Destroy(WareTableList);
        //                SetWareTable(i);
        //            }
        //        }

        //        //for (int shopId = 0; shopId < m_Shop.Count; shopId++)
        //        //{

        //        //    SetWareTable(shopId);
        //        //}

        //    }
        //});

    }

    public void BugRp()
    {
        //if (mCurSlectItem == null) return;
        //ShopGoodInfoVO goodinfo = mCurSlectItem.GetComponent<UIVO>().vo as ShopGoodInfoVO;
        //int goodId = goodinfo.goodId;
        //UIVO parentUIVo = mCurSlectItem.transform.parent.parent.GetComponent<UIVO>();
        //int shopId = parentUIVo.val;
        //RequestCmd request = new RequestCmd();
        //request.cmd = CMD.BuyGoodFromShopAction;
        //RParamVO param = new RParamVO();
        //param.intValue1 = shopId;
        //param.intValue2 = goodId;
        //request.param = param;
        //Global.ShowLoadingStart();
        //Request(request, delegate(string err, Response response)
        //{
        //    Global.ShowLoadingEnd();
        //    if (response == null)
        //        return;
        //    Global.ShowLoadingEnd();
        //    Prompt vo = response.ParseVO<Prompt>();
        //    if (vo != null && vo.lang == 50103)
        //    {
        //        ldaControl("SoldOutSprite", mCurSlectItem).SetActive(true);

        //        ShopGoodInfoVO goodsInfo = mCurSlectItem.GetComponent<UIVO>().vo as ShopGoodInfoVO;
        //        goodsInfo.buy = true;
        //    }

        //});

    }

    public void UpgradeShopRp(int shopType)
    {
        //RequestCmd request = new RequestCmd();
        //request.cmd = CMD.UpgradeShopAction;
        //RParamVO param = new RParamVO();
        //param.intValue1 = shopType;
        //request.param = param;
        //Global.ShowLoadingStart();
        //Request(request, delegate(string err, Response response)
        //{
        //    Global.ShowLoadingEnd();
        //    if (response == null)
        //        return;
        //    OpenShopVO vo = response.ParseVO<OpenShopVO>();
        //    if (vo != null)
        //    {
        //        int count = ShopList.transform.childCount;
        //        for (int i = 0; i < count; i++)
        //        {
        //            GameObject WareTableList = ShopList.transform.GetChild(i).gameObject;
        //            if (WareTableList != null)
        //            {
        //                GameObject.Destroy(WareTableList);
        //            }
        //        }
        //        curSelectShopType = curSelectShopType + 1;
        //        refresh();
        //    }

        //});

    }

    private void OnItemClick(GameObject go)
    {
        //mCurSlectItem = go;
        //ShopGoodInfoVO goodinfo = go.GetComponent<UIVO>().vo as ShopGoodInfoVO;

        //if (goodinfo.buy == true)
        //{
        //    BugRp();
        //    return;
        //}
        //int goodId = goodinfo.goodId;
        //Shopgoods shopgoods = ShopManager.Instance.GetGoodsItem(curSelectShopType, goodId);
        //int goodType = shopgoods.itemType;

        //if (goodType == 2)
        //{

        //    ItemBase1 itemBaseInfo = ItemManager.Instance.GetItem(goodId);
        //    if (itemBaseInfo == null) return;
        //    UISprite quilitySpt;
        //    quilitySpt = ldaControl("QuilitySprite", mConfirmBugPanel).GetComponent<UISprite>() as UISprite;
        //    quilitySpt.spriteName = GetQualityString(itemBaseInfo.quality);

        //    UISprite spt;
        //    spt = ldaControl("WareSprite", mConfirmBugPanel).GetComponent<UISprite>() as UISprite;
        //    spt.spriteName = itemBaseInfo.icon != null ? itemBaseInfo.icon : "i_fw_2011101";

        //    UILabel ItemdescLabel;
        //    ItemdescLabel = ldaControl("UseDescLabel", mConfirmBugPanel).GetComponent<UILabel>() as UILabel;
        //    ItemdescLabel.text = itemBaseInfo.dec;

        //    string descStr = LanguagesManager.Instance.GetItem("Shop_Bug_Page");
        //    int moneyType = shopgoods.moneyType;
        //    descStr = descStr.Replace("\\n", "\n");
        //    descStr = descStr.Replace("$0", goodinfo.num.ToString());
        //    descStr = descStr.Replace("$1", GetMoneyType(moneyType));
        //    descStr = descStr.Replace("$2", shopgoods.price.ToString());

        //    UILabel PagedescLabel;
        //    PagedescLabel = ldaControl("PageDescLabel", mConfirmBugPanel).GetComponent<UILabel>() as UILabel;
        //    PagedescLabel.text = descStr;


        //    if (itemBaseInfo.type == 4)
        //    {
        //        GameObject HsTips = ldaControl("HSTipsSprite");
        //        HsTips.SetActive(true);

        //    }

        //}
        //else if (goodType == 3)
        //{
        //    Equip equipInfo = EquipManager.Instance.GetItem(goodId);
        //    if (equipInfo == null) return;
        //    UISprite quilitySpt;
        //    quilitySpt = ldaControl("QuilitySprite", mConfirmBugPanel).GetComponent<UISprite>() as UISprite;
        //    quilitySpt.spriteName = GetQualityString(equipInfo.quality);

        //    UISprite spt;
        //    spt = ldaControl("WareSprite", mConfirmBugPanel).GetComponent<UISprite>() as UISprite;
        //    spt.spriteName = equipInfo.icon != null ? equipInfo.icon : "i_fw_2011101";

        //    UILabel ItemdescLabel;
        //    ItemdescLabel = ldaControl("UseDescLabel", mConfirmBugPanel).GetComponent<UILabel>() as UILabel;
        //    ItemdescLabel.text = equipInfo.desc;

        //    string descStr = LanguagesManager.Instance.GetItem("Shop_Bug_Page");
        //    int moneyType = shopgoods.moneyType;
        //    descStr = descStr.Replace("\\n", "\n");
        //    descStr = descStr.Replace("$0", goodinfo.num.ToString());
        //    descStr = descStr.Replace("$1", GetMoneyType(moneyType));
        //    descStr = descStr.Replace("$2", shopgoods.price.ToString());

        //    UILabel PagedescLabel;
        //    PagedescLabel = ldaControl("PageDescLabel", mConfirmBugPanel).GetComponent<UILabel>() as UILabel;
        //    PagedescLabel.text = descStr;

        //    GameObject HsTips = ldaControl("HSTipsSprite");
        //    HsTips.SetActive(false);
        //}

        //UILabel goodName;
        //goodName = ldaControl("WareNameLabel", mConfirmBugPanel).GetComponent<UILabel>() as UILabel;
        //goodName.text = shopgoods.itemName;

        ////UILabel goldNum;
        ////goldNum = Control("GoldLabel", mConfirmBugPanel).GetComponent<UILabel>() as UILabel;
        ////goldNum.text = shopgoods.price.ToString();

        ////UISprite coinSpt;
        ////coinSpt = ldaControl("Goldicon", mConfirmBugPanel).GetComponent<UISprite>() as UISprite;




        //string BugBtn = LanguagesManager.Instance.GetItem("Shop_Bug_Btn");
        //string BugTitle = LanguagesManager.Instance.GetItem("Shop_Bug_Title");

        //BigConfirmWnd<ShopWnd> cwnd = new BigConfirmWnd<ShopWnd>();
        //cwnd.OnOpen();
        //cwnd.SetParentWnd(this, (int)ConfirmTag.Bug, "", BugBtn, "", BugTitle, mConfirmBugPanel);


    }
    public void SetWareTable(int shopId)
    {
       
        //int prefabCnt = 0;
        //int type = m_Shop[shopId].shopType;
        //WareTableList = CreateGameObjectByPrefabNameForParent("WareTableList", ShopList);
        //WareTableList.transform.localPosition = Vector3.zero;
        //WareTableList.transform.localScale = Vector3.one;
        //WareTableList.name = "WareTableList" + type;

        //UIVO uivo = WareTableList.AddComponent<UIVO>();
        //uivo.val = type;
        //UITable tableList = WareTableList.GetComponent<UITable>();
        //tableList.Reposition();
        //UIDraggablePanel uidraggablePanel = WareTableList.GetComponent<UIDraggablePanel>();

        //if (type != curSelectShopType)
        //{
        //    if (WareTableList != null)
        //    {
        //        WareTableList.SetActive(false);
        //    }
        //}
        //else
        //{
        //    Titlespt.spriteName = GetShopType(type).ToString();
        //}

        //ShopSetting shopSetting = ShopSettingManager.Instance.GetItem(type);
        //int maxSize = shopSetting.type1Num + shopSetting.type2Num;
        //int size = m_Shop[shopId].M_refreshGoods.Count;
        //if (size > maxSize) size = maxSize;
        //if (size > 0)
        //{
        //    prefabCnt = size / 8;
        //    prefabCnt = size % 8 == 0 ? prefabCnt : prefabCnt + 1;
        //}
        //GameObject[] gos = new GameObject[prefabCnt];
        //for (int i = 0; i < prefabCnt; i++)
        //{
        //    gos[i] = CreateGameObjectByPrefabNameForParent("WareTable", WareTableList);
        //    uivo.val = type;
        //}
        //int id = 0;
        //for (id = 0; id < size; id++)
        //{
        //    ShopGoodInfoVO goodinfo = m_Shop[shopId].M_refreshGoods[id] as ShopGoodInfoVO;
        //    int goodId = goodinfo.goodId;
        //    int shopType = m_Shop[shopId].shopType;
        //    Shopgoods shopgoods = ShopManager.Instance.GetGoodsItem(shopType, goodId);
        //    if (shopgoods == null) return; 
        //    int goodType = shopgoods.itemType;
        //    GameObject item = ldaControl("Ware" + id % 8, gos[id / 8]);
        //    UILabel NumLabel;
        //    NumLabel = ldaControl("NumLabel", item).GetComponent<UILabel>() as UILabel;
        //    NumLabel.text = goodinfo.num.ToString();
        //    item.GetComponent<UIDragPanelContents>().draggablePanel = uidraggablePanel;

        //    if (goodType == 2)
        //    {

        //        ItemBase1 itemBaseInfo = ItemManager.Instance.GetItem(goodId);
        //        if (itemBaseInfo == null) return;
        //        UISprite quilitySpt;
        //        quilitySpt = ldaControl("Warequility", item).GetComponent<UISprite>() as UISprite;
        //        quilitySpt.spriteName = GetQualityString(itemBaseInfo.quality);

        //        UISprite spt;
        //        spt = ldaControl("Wareicon", item).GetComponent<UISprite>() as UISprite;
        //        spt.spriteName = itemBaseInfo.icon != null ? itemBaseInfo.icon : "i_fw_2011101";

        //    }
        //    else if (goodType == 3)
        //    {
        //        Equip equipInfo = EquipManager.Instance.GetItem(goodId);
        //        if (equipInfo == null) return;
        //        UISprite quilitySpt;
        //        quilitySpt = ldaControl("Warequility", item).GetComponent<UISprite>() as UISprite;
        //        quilitySpt.spriteName = GetQualityString(equipInfo.quality);

        //        UISprite spt;
        //        spt = ldaControl("Wareicon", item).GetComponent<UISprite>() as UISprite;
        //        spt.spriteName = equipInfo.icon != null ? equipInfo.icon : "i_fw_2011101";


        //    }
        //    if (goodinfo.buy == true)
        //    {
        //        ldaControl("SoldOutSprite", item).SetActive(true);
        //    }

        //    else
        //    {
        //        ldaControl("SoldOutSprite", item).SetActive(false);
        //    }
        //    UILabel goodName;
        //    goodName = ldaControl("NameLabel", item).GetComponent<UILabel>() as UILabel;
        //    goodName.text = shopgoods.itemName;

        //    UILabel goldNum;
        //    goldNum = ldaControl("GoldLabel", item).GetComponent<UILabel>() as UILabel;
        //    goldNum.text = shopgoods.price.ToString();

        //    UISprite coinSpt;
        //    coinSpt = ldaControl("Goldicon", item).GetComponent<UISprite>() as UISprite;

        //    UIVO itemUIVO = item.GetComponent<UIVO>();
        //    itemUIVO.vo = goodinfo;

        //    UIEventListener.Get(item).onClick = OnItemClick;
        //    coinSpt.spriteName = GetCoin(shopgoods.moneyType).ToString();



        //}

        //for (int i = id; i < prefabCnt * 8; i++)
        //{
        //    GameObject item = ldaControl("Ware" + id % 8, gos[id / 8]);
        //    item.SetActive(false);
        //    id++;
        //}
    }

    //private ConeSprite GetCoin(int moneyType)
    //{

    //    ConeSprite coneSprete = ConeSprite.icon_jinbi;
    //    switch (moneyType)
    //    {
    //        case 1:
    //            coneSprete = ConeSprite.icon_jinbi2;//钻石
    //            break;
    //        case 2:
    //            coneSprete = ConeSprite.icon_jinbi;//金币
    //            break;
    //        case 3:
    //            coneSprete = ConeSprite.icon_zhanhun;//血斑石
    //            break;
    //        case 4:
    //            coneSprete = ConeSprite.exp;//经验
    //            break;
    //        case 5:
    //            coneSprete = ConeSprite.icon_jinbi1;//体力
    //            break;
    //        case 6:
    //            coneSprete = ConeSprite.icon_hb_jjb;//竞技币
    //            break;
    //        case 7:
    //            coneSprete = ConeSprite.icon_hb_hb;//黑币
    //            break;

    //        case 8:
    //            coneSprete = ConeSprite.icon_hb_jjbMOBA;//MOBA竞技币
    //            break;

    //        case 9:
    //            coneSprete = ConeSprite.icon_hb_jjbYuanZheng;//远征
    //            break;
    //    }
    //    return coneSprete;
    //}
    //private ShopTypeSprite GetShopType(int ShopType)
    //{

    //    ShopTypeSprite shopTypeSprite = ShopTypeSprite.title_shop1;
    //    switch (ShopType)
    //    {
    //        case 1:
    //            shopTypeSprite = ShopTypeSprite.title_shop1;//一级
    //            Titlespt.transform.localScale = new Vector3(26, 48, 1);
    //            //Titlespt.transform.localRotation = new Quaternion(0, 0, 90, 0);
    //            break;
    //        case 2:
    //            shopTypeSprite = ShopTypeSprite.title_shop2;//二级
    //            Titlespt.transform.localScale = new Vector3(38, 48, 1);
    //            //Titlespt.transform.localRotation = new Quaternion(0, 0, 90, 0);
    //            break;
    //        case 3:
    //            shopTypeSprite = ShopTypeSprite.title_shop3;//三级
    //            Titlespt.transform.localScale = new Vector3(50, 48, 1);
    //            //Titlespt.transform.localRotation = new Quaternion(0, 0, 90, 0);
    //            break;
    //        case 4:
    //            shopTypeSprite = ShopTypeSprite.z_vipsd;//vip
    //            Titlespt.transform.localScale = new Vector3(91, 44, 1);
    //            //Titlespt.transform.localRotation=new Quaternion(0,0,0,0);
    //            break;
    //        case 5:
    //            shopTypeSprite = ShopTypeSprite.z_jjsd;//竞技
    //            Titlespt.transform.localScale = new Vector3(88, 53, 1);
    //            //Titlespt.transform.localRotation = new Quaternion(0, 0, 0, 0);
    //            break;
    //        case 6:
    //            shopTypeSprite = ShopTypeSprite.z_yzsd;//远征
    //            Titlespt.transform.localScale = new Vector3(89, 52, 1);
    //            //Titlespt.transform.localRotation = new Quaternion(0, 0, 0, 0);
    //            break;
    //        case 7:
    //            shopTypeSprite = ShopTypeSprite.z_moba;//Moba
    //            Titlespt.transform.localScale = new Vector3(148,43, 1);
    //            //Titlespt.transform.localRotation = new Quaternion(0, 0, 0, 0);
    //            break;
    //    }
    //    return shopTypeSprite;
    //}

    protected override bool OnShow()
    {
        bool val = base.OnShow();
        if (IsRefresh)
        {
            refresh();
        }
        IsRefresh = false;
        return val;
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }
    public void Return(GameObject go)
    {
        if (CloseThisAndBackWnd.BackTo_None == mCloseThisAndBackWnd)
        {
            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
        }
        else if (CloseThisAndBackWnd.BackTo_HeroMainWnd == mCloseThisAndBackWnd)
        {
        }
        else if(CloseThisAndBackWnd.BackTo_JingJiChangMainWnd == mCloseThisAndBackWnd)
        {
        }

        Close();
    }


    GameObject CreateGameObjectByPrefabName(string Name, string parentname)
    {
        GameObject go = GameObject.Instantiate(Resources.Load(Name)) as GameObject;
        go.transform.parent = Control(parentname).transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return go;
    }
    GameObject CreateGameObjectByPrefabNameForParent(string Name, GameObject parentname)
    {
        GameObject go = GameObject.Instantiate(Resources.Load(Name)) as GameObject;
        go.transform.parent = parentname.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return go;
    }

    private string GetQualityString(int q)
    {
        string res = "icon_dk_l";
        switch (q)
        {
            case 1:
                res = "icon_dk_l";
                break;
            case 2:
                res = "icon_dk_z";
                break;
            case 3:
                res = "icon_dk_c";
                break;
            case 4:
                res = "icon_dk_h";
                break;
            case 5:
                res = "icon_dk_hu";
                break;
        }

        return res;
    }

    private void SetRefreshTime(GameObject lab, int time)
    {
        CountDownLabel countDownLabel = lab.GetComponent<CountDownLabel>();
        countDownLabel.CountDownSeconds = time;
    }

    private string GoldFormat(int gold)
    {
        string goldFormatStr = null;
        if (gold < 99999)
        {
            goldFormatStr = gold.ToString();
            return goldFormatStr;

        }
        else
        {
            goldFormatStr = (gold / 10000 > 100) ? ((gold / 10000).ToString()) : ((gold / 10000.0f).ToString("F1"));
            goldFormatStr += "万";
        }
        return goldFormatStr;
    }

    private string GetMoneyType(int moneyType)
    {
        string str = LanguagesManager.Instance.GetItem("AutoTips_GetItemAward_Gold");
        switch (moneyType)
        {
            case 1:
                str = LanguagesManager.Instance.GetItem("AutoTips_GetItemAward_Diamond");
                break;
            case 2:
                str = LanguagesManager.Instance.GetItem("AutoTips_GetItemAward_Gold");
                break;
            case 3:
                str = LanguagesManager.Instance.GetItem("AutoTips_GetItemAward_Xuebanshi");
                break;
            case 4:
                str = LanguagesManager.Instance.GetItem("AutoTips_GetItemAward_Exp");
                break;
            case 5:
                str = LanguagesManager.Instance.GetItem("AutoTips_GetItemAward_Life");
                break;
            case 6:
                str = LanguagesManager.Instance.GetItem("AutoTips_GetItemAward_Jingjibi");
                break;
            case 7:
                str = LanguagesManager.Instance.GetItem("AutoTips_GetItemAward_Heibi");
                break;
            case 8:
                str = LanguagesManager.Instance.GetItem("Shop_Bug_JingJiBi");
                break;
            case 9:
                str = LanguagesManager.Instance.GetItem("Shop_Bug_YuanZhengBi");
                break;

        }

        return str;
    }
}
