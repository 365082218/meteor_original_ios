using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharactorWnd : Window<CharactorWnd>
{
	public enum QualitySprite
	{
		Button10_BaseItem_Quality_00,
		Button10_BaseItem_Quality_01,
		Button10_BaseItem_Quality_02,
		Button10_BaseItem_Quality_03,
		Button10_BaseItem_Quality_04,
	};
	
    public override string PrefabName { get { return "CharactorWnd"; } }
	
	private GameObject mEquipRoot;
	private UILabel mGoldLabel;
    private UILabel mGemLabel;
	private UILabel mEnergy;
	private GameObject mExpSlider;
    private GameObject mItemFocus;
	private GameObject mSubItemFocus;
	private GameObject mAttibIcon;
	
	private int mEquipItemMax = 7;
	private int mStarMax = 5;
	private int mItemFocusId = -1;
	protected override bool OnOpen()
    {
		Init();
		WinStyle = WindowStyle.WS_Ext;
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }
	
	private void Init()
	{
		UIEventListener.Get(Control("Return")).onClick = OnClickCloseWnd;
		mEquipRoot = Control( "EquipRoot" );
		
		mGemLabel = Control("LabelGem").GetComponent<UILabel>();
        mGoldLabel = Control("LabelGold").GetComponent<UILabel>();	
		mEnergy	= Control ("LabelEnergy").GetComponent<UILabel>();
		mItemFocus = Control("ItemFocus");//Left
		mExpSlider = Control("ExpSlider");
		
		InitExp();
		InitCharactor();
		InitAttrib();
		InitItemFocus();
		InitEquipment();
		UpdateEquipment();
		UpdateAttribAllPanel();
	}
	
	private void InitExp()
	{
		//cancel by Lindean 20141031
//		MainAttrib attrib = PlayerDataManager.Instance.Attrib;
//		float Cur = (float)attrib.CurExp;
//		float ExpMax = (float)attrib.Exp;
//		float percent = (Cur/ExpMax);
//		mExpSlider.GetComponent<UISlider>().sliderValue = percent;
//		Control("ExpLabel",mExpSlider).GetComponent<UILabel>().text = attrib.CurExp.ToString()+"/"+attrib.NextExp.ToString();
	}
	
	private void InitItemFocus()
	{
		mItemFocus.SetActive(false);
	}
	
	private void UpdateItemFocus(GameObject go)
	{
		if(!mItemFocus.activeSelf)
			mItemFocus.SetActive(true);
		mItemFocus.transform.parent = go.transform;
		mItemFocus.transform.localPosition = Vector3.zero;
		mItemFocus.transform.localScale = new Vector3(100.0f,100.0f,0.0f);
		
		if(mSubItemFocus.activeSelf)
			mSubItemFocus.SetActive(false);
	}
	
	
	private void InitAttrib()
	{
		//cancel by Lindean 20141031
//        MainAttrib attrib = PlayerDataManager.Instance.Attrib;
//        mGoldLabel.text = attrib.Gold.ToString();
//        mGemLabel.text = attrib.Gem.ToString();	
//		
//		Control("LevelLabel",Control("CharactorInfo")).GetComponent<UILabel>().text = "Lv " + attrib.Level;
//		Control("NameLabel",Control("CharactorInfo")).GetComponent<UILabel>().text = attrib.Name;
	}
	
	private void UpdateAttrib()
	{
		InitAttrib();
	}

	private void InitCharactor()
	{
		Control ("CharactorCollider").AddComponent<PressDrag>().target = GameObject.Find("Charactor_Root").transform;
		UIEventListener.Get( Control ("CharactorCollider") ).onClick = OnClickCharactor;
	}
	
	private void UpdateCharactor()
	{
	}
	
	private void InitGridIconDesc()
	{
		mAttibIcon.SetActive(false);
	}
	
	private void UpdateGridIconDesc()
	{
		//cancel by Lindean 20141031
//		if(	mItemFocusId == -1 )
//			return;
//		
//		Item item = PlayerDataManager.Instance.BackPack.Find( mItemFocusId );
//		if( item == null )
//			return;
//		
//		UpdateOneDescAttrib();
//		
//		if(CompareWithTwoEquip(item) > 0){
//			if(!mAttibIcon.activeSelf){
//				mAttibIcon.SetActive(true);		
//			}
//			mAttibIcon.GetComponent<UISprite>().spriteName = "Button24_Attribute_Compare_01";
//		}else if(CompareWithTwoEquip(item) < 0){
//			if(!mAttibIcon.activeSelf){
//				mAttibIcon.SetActive(true);		
//			}
//			mAttibIcon.GetComponent<UISprite>().spriteName =  "Button24_Attribute_Compare_02";
//		}else{
//			if(mAttibIcon.activeSelf){
//				mAttibIcon.SetActive(false);
//			}
//		}
	}
	
	void OnClickGridItem(GameObject go)
	{
		GameObject Subgo = Control("ItemID",go);
		
		if(!mSubItemFocus.activeSelf)
			mSubItemFocus.SetActive(true);
		mSubItemFocus.transform.parent = go.transform;
		mSubItemFocus.transform.localPosition = Vector3.zero;
		mSubItemFocus.transform.localScale = new Vector3(80.0f,80.0f,0.0f);
		
		
		if(mItemFocus.activeSelf)
			mItemFocus.SetActive(false);
		
		mItemFocusId = int.Parse( Subgo.GetComponent<UILabel>().text );
		
		UpdateGridIconDesc();
	}
	
	void ResetOneAttrib(int nItemIndex,int nIconIndex)
	{
		GameObject goItem;
		GameObject goEquip = Control("AttribOne(Clone)");
		if(nItemIndex > mEquipItemMax)
			return; 
		
		for(int i = nItemIndex; i<= mEquipItemMax;i++)
		{
			goItem = Control ("item" + i.ToString(),goEquip);
			goItem.transform.Find("QualityFrame").gameObject.SetActive(false);
			Control("Icon",goItem).GetComponent<UISprite>().spriteName = "Button08_EquipItem_Bottom_0" + nIconIndex;
			Control("Icon",goItem).GetComponent<UISprite>().alpha = 1.0f;
			UIEventListener.Get(goItem).onClick = null;
		}
	}
	
	void UpdateOneAttrib()
	{
		//cancel by Lindean 20141031
//		if(mItemFocusId == -1)
//			return;
//		
//		int nSubType = PlayerDataManager.Instance.BackPack.Find(mItemFocusId).ItemBase.SubType;
//		
//		int nCount = 0;
//		GameObject goEquip = Control("AttribOne(Clone)");
//		GameObject goItem;
//		foreach( Item itm in PlayerDataManager.Instance.BackPack.Items.Values ){
//			if( itm.ToEquipItem() != null && itm.ItemBase.SubType == nSubType ){
//				goItem = Control ("item" + nCount.ToString(),goEquip);
//				Control("Icon",goItem).GetComponent<UISprite>().spriteName = itm.ItemBase.Icon;
//				if(itm.ToEquipItem().Equiped){
//					Control("Icon",goItem).GetComponent<UISprite>().alpha = 0.3f;
//				}else{
//					Control("Icon",goItem).GetComponent<UISprite>().alpha = 1.0f;
//				}
//				UpdateItemQualityFrameIcon(itm,goItem.transform.Find("QualityFrame").gameObject);
//				
//				Control("ItemID",goItem).GetComponent<UILabel>().text = itm.ID.ToString();
//				UIEventListener.Get(goItem).onClick = OnClickGridItem;
//				nCount++;
//				
//				//ForTest only for seven things
//				if(nCount > mEquipItemMax)
//					break;
//			}
//		}
//		ResetOneAttrib( nCount,nSubType );	
	}
	
	void UpdateOneAttrib( GameObject go )
	{
		//cancel by Lindean 20141031
//		int nCount = 0;
//		int nIndex = int.Parse(go.name);
//		GameObject goEquip = Control("AttribOne(Clone)");
//		GameObject goItem;
//		foreach( Item itm in PlayerDataManager.Instance.BackPack.Items.Values ){
//			if( itm.ToEquipItem() != null && itm.ItemBase.SubType == nIndex ){
//				goItem = Control ("item" + nCount.ToString(),goEquip);
//				Control("Icon",goItem).GetComponent<UISprite>().spriteName = itm.ItemBase.Icon;
//				if(itm.ToEquipItem().Equiped){
//					Control("Icon",goItem).GetComponent<UISprite>().alpha = 0.3f;
//				}else{
//					Control("Icon",goItem).GetComponent<UISprite>().alpha = 1.0f;
//				}
//				UpdateItemQualityFrameIcon(itm,goItem.transform.Find("QualityFrame").gameObject);
//				Control("ItemID",goItem).GetComponent<UILabel>().text = itm.ID.ToString();
//				UIEventListener.Get(goItem).onClick = OnClickGridItem;
//				nCount++;
//				//ForTest
//				if(nCount >mEquipItemMax)
//					break;
//			}
//		}
//		ResetOneAttrib( nCount,nIndex );
	}
	
	void UpdateMainAttrib()
	{
		//cancel by Lindean 20141031
//		MainAttrib Attrib = PlayerDataManager.Instance.Data.Attrib;
////		Control ("Name").GetComponent<UILabel>().text =        	 "Name:" + Attrib.Name;
////		Control ("Role").GetComponent<UILabel>().text =          "Role:" + Attrib.Role.ToString();
////		Control ("Level").GetComponent<UILabel>().text =         "Level:"+ Attrib.Level.ToString();
////		Control ("Gold").GetComponent<UILabel>().text =        "Gold:"+Attrib.Gold.ToString();
////		Control ("Gem").GetComponent<UILabel>().text =          "Gem:"+Attrib.Gem.ToString();
////		Control ("CurHP").GetComponent<UILabel>().text =         "CurHP:"+Attrib.CurHP.ToString();
////		Control ("CurExp").GetComponent<UILabel>().text =        "CurExp:"+Attrib.CurExp.ToString();
////		Control ("NextExp").GetComponent<UILabel>().text =       "NextExp:"+Attrib.NextExp.ToString();
////		Control ("Exp").GetComponent<UILabel>().text =           "Exp:"+Attrib.CurExp.ToString();
//		Control ("HPMax").GetComponent<UILabel>().text =         Attrib.HPMax.ToString();
////		Control ("HPRestore").GetComponent<UILabel>().text =     "HPRestore:"+Attrib.HPRestore.ToString();
//		Control ("SoulMax").GetComponent<UILabel>().text =       Attrib.SoulMax.ToString();
//		
////		Control ("SoulRestore").GetComponent<UILabel>().text =   "SoulRestore:"+Attrib.SoulRestore.ToString();
//		Control ("Damage").GetComponent<UILabel>().text =        Attrib.Damage.ToString();
//		Control ("Defense").GetComponent<UILabel>().text =       Attrib.Defense.ToString();
//		Control ("SpecialDamage").GetComponent<UILabel>().text = Attrib.SpecialDamage.ToString();
//		Control ("SpecialDefense").GetComponent<UILabel>().text =Attrib.SpecialDefense.ToString();
//		
//		Control ("Critical").GetComponent<UILabel>().text =      Attrib.Critical.ToString();
//		Control ("Tough").GetComponent<UILabel>().text =         Attrib.Tough.ToString();
//		Control ("Hit").GetComponent<UILabel>().text =           Attrib.Hit.ToString();
//		Control ("Block").GetComponent<UILabel>().text =         Attrib.Block.ToString();
//		
////		Control ("MoveSpeed").GetComponent<UILabel>().text =     "MoveSpeed:"+Attrib.MoveSpeed.ToString();
////		Control ("FastRate").GetComponent<UILabel>().text =      "FastRate:"+Attrib.FastRate.ToString();
////		Control ("StiffAdd").GetComponent<UILabel>().text =      "StiffAdd:"+Attrib.StiffAdd.ToString();
////		Control ("StiffSub").GetComponent<UILabel>().text =      "StiffSub:"+Attrib.StiffSub.ToString();
//		Control ("AbilityMax").GetComponent<UILabel>().text =    Attrib.AbilityMax.ToString();
////		Control ("AbHitAdd").GetComponent<UILabel>().text =      "AbHitAdd:"+Attrib.AbHitAdd.ToString();
////		Control ("AbRestore").GetComponent<UILabel>().text =     "AbRestore:"+Attrib.AbRestore.ToString();
////		Control ("AbUseAdd").GetComponent<UILabel>().text =      "AbUseAdd:"+Attrib.AbHitAdd.ToString();		
	}
	
	void OnClickCloseWnd(GameObject go)
	{
		Close();
	}
	
	//Here for LeftSide
	void InitEquipment()
	{
		GameObject go ;
		for(int i= 1; i < 10; i++)
		{
			go = mEquipRoot.transform.Find(i.ToString()).gameObject;
			UIEventListener.Get( go ).onClick = OnClickEquipItem;
			go.transform.Find("Icon").GetComponent<UISprite>().spriteName = "Button08_EquipItem_Bottom_0"+i;
			go.transform.Find("QualityFrame").gameObject.SetActive(false);
			//go.GetComponent<UIButtonScale>().enabled = false;
			//go.transform.Find("ItemID").GetComponent<UILabel>().text = "0" ;
		}
	}
	
	void ResetEquipment()
	{
		InitEquipment();
	}
	
		
	//private void UpdateItemQualityFrameIcon(Item item, GameObject goQuality)
	//{
	//	switch( item.ItemBase.Quality )
	//	{
	//		case (int) QualitySprite.Button10_BaseItem_Quality_00:
	//			goQuality.SetActive(false);
	//			break;
	//		case (int) QualitySprite.Button10_BaseItem_Quality_01:
	//			goQuality.SetActive(true);
	//			goQuality.GetComponent<UISprite>().spriteName= "Button10_BaseItem_Quality_01";
	//			break;
	//		case (int) QualitySprite.Button10_BaseItem_Quality_02:
	//			goQuality.SetActive(true);
	//			goQuality.GetComponent<UISprite>().spriteName= "Button10_BaseItem_Quality_02";
	//			break;
	//		case (int) QualitySprite.Button10_BaseItem_Quality_03:
	//			goQuality.SetActive(true);
	//			goQuality.GetComponent<UISprite>().spriteName= "Button10_BaseItem_Quality_03";
	//			break;
	//		case (int) QualitySprite.Button10_BaseItem_Quality_04:
	//			goQuality.SetActive(true);
	//			goQuality.GetComponent<UISprite>().spriteName=  "Button10_BaseItem_Quality_04";
	//			break;
	//		default:
	//			goQuality.SetActive(false);
	//			break;
	//	}
	//}
	
	
	void UpdateEquipment()
	{
		//cancel by Lindean 20141031
//		ResetEquipment();
//		GameObject tmpUISp;
//		foreach (Item item in PlayerDataManager.Instance.BackPack.Items.Values)
//		{
//			if( item.ItemBase.MainType == (int)EItemType.Equip 
//				&& item.ToEquipItem().Equiped )
//			{
//				mEquipRoot.transform.Find(((int)item.ItemBase.SubType).ToString()).Find("Icon").gameObject.SetActive(true);
//				mEquipRoot.transform.Find(((int)item.ItemBase.SubType).ToString()).Find("Icon").gameObject.GetComponent<UISprite>().spriteName 
//					= item.ItemBase.Icon;
//				mEquipRoot.transform.Find(((int)item.ItemBase.SubType).ToString()).Find("ItemID").GetComponent<UILabel>().text
//					= item.ID.ToString();
//				
//				tmpUISp = mEquipRoot.transform.Find(((int)item.ItemBase.SubType).ToString()).Find("QualityFrame").gameObject;
//				
//				UpdateItemQualityFrameIcon(item,tmpUISp);
//				
//			}
//		}
	}
	
	void UpdateAttribAllPanel()
	{
		GameObject go = Control("AttribRoot");
		GameObject Subgo = Control("AttribOne(Clone)");
		if( Subgo != null ){
			GameObject.Destroy(Subgo);
		}
		
		Subgo = Control ("AttribAll(Clone)");
		if(Subgo)
			return;
		
		GameObject All = GameObject.Instantiate(Resources.Load("AttribAll")) as GameObject;
		
		All.transform.parent = go.transform;
		All.transform.localScale = Vector3.one;
		All.transform.localPosition = Vector3.zero;
		UpdateMainAttrib();
	}
	
	void OnUnEquSuccess()
	{
	}
	
	void OnEquSuccess()
	{
	}
	
	void OnClickEquip( GameObject go )
	{
		int nIndex = 0;
		if(mItemFocusId == -1)
			return;
		else
			nIndex = mItemFocusId;
        OnEquSuccess();
	}
	
	void OnClickUnEquip( GameObject go )
	{
		int nIndex = 0;
		if(mItemFocusId == -1)
			return;
		else
			nIndex = mItemFocusId;
        OnUnEquSuccess();
	}
	
	void UpdateStarByStrengthCount(int nCount)
	{
		GameObject go = Control ("StartRoot");
		GameObject goStarItem;
		for(int i=0; i<5; i++){
			goStarItem = Control("Star" + i, go);
			if( nCount > 0 && i<= nCount)
				goStarItem.GetComponent<UISprite>().spriteName = "Button04_Package_Star_01";
			else
				goStarItem.GetComponent<UISprite>().spriteName = "Button04_Package_Star_02";
		}
	}
	
	//int CompareWithTwoEquip(Item item)
	//{
		//cancel by Lindean 20141031
//		if(item.ToEquipItem()== null || 
//			(int)item.ItemBase.MainType != (int)EItemType.Equip){
//			Debug.Log("Item is not Equip");
//			return 0;
//		}
//		
//		foreach(Item itemTmp in PlayerDataManager.Instance.BackPack.Items.Values){
//			if(itemTmp.ID == item.ID || (int)itemTmp.ItemBase.MainType != (int)EItemType.Equip )
//				continue;
//			
//			if( itemTmp.ItemBase.SubType == item.ItemBase.SubType
//					&& (itemTmp.ToEquipItem().Equiped)){
//				return GetMainAttribBySubType(item) 
//						- GetMainAttribBySubType(itemTmp);
//			}
//		}
//		return 0;
		//return 0;
	//}
	
//	int GetMainAttribBySubType(Item item)
//	{
//		switch(item.ItemBase.SubType){
//			case (int)EquipSubType.EST_Weapon://武器
//					return item.ToEquipItem().GetAttrib(EquipAttrib.Damage);
//			case (int)EquipSubType.EST_Helmet://头盔
//					return item.ToEquipItem().GetAttrib(EquipAttrib.HPMax);
//			case (int)EquipSubType.EST_Armor://上衣
//					return item.ToEquipItem().GetAttrib(EquipAttrib.Defense);
//			case (int)EquipSubType.EST_Shoulder://肩甲
//					return item.ToEquipItem().GetAttrib(EquipAttrib.SpecialDamage);
//			case (int)EquipSubType.EST_Shoe://鞋子
//					return item.ToEquipItem().GetAttrib(EquipAttrib.SpecialDefense);
//			case (int)EquipSubType.EST_NeckLace://项链
//					return item.ToEquipItem().GetAttrib(EquipAttrib.Hit);
//			case (int)EquipSubType.EST_WaistBand://腰带
//					return item.ToEquipItem().GetAttrib(EquipAttrib.Block);
//			case (int)EquipSubType.EST_Wrist://护腕
//					return item.ToEquipItem().GetAttrib(EquipAttrib.Tough);
//			case (int)EquipSubType.EST_Ring://戒指	
//					return item.ToEquipItem().GetAttrib(EquipAttrib.Critical);
//			//Control("LabelMain").GetComponent<UILabel>().text = Ta.Attrib + " :" + item.ToEquipItem().GetAttrib(EquipAttrib.HPMax);
//		}
//		return 0;
//	}
	
//	void SetStarCount(Item item)
//	{
////		UISprite UISp;
////		GameObject go = Control ("StarRoot") ;
////		GameObject goSub ;
////		
////		if( 0 == EquipBaseManager.Instance.GetItem(item.ItemBase.ID).StrengthenCount ){
////			for(int i = 0; i< mStarMax; i++){
////				goSub = go.transform.Find("Star"+i+"Root").gameObject;
////				goSub.gameObject.SetActive(false);
////			}
////			return;
////		}
////		
////		float Base = item.ToEquipItem().GetAttrib(EquipAttrib.StrengthenCount)*item.ItemBase.Quality/
////			EquipBaseManager.Instance.GetItem(item.ItemBase.ID).StrengthenCount;
////		int StarCount = (int)Base;
////		float Percent = Base - StarCount;
////		
////		for(int i = 0; i< mStarMax; i++){
////			goSub = go.transform.Find("Star"+i+"Root").gameObject;
////			if( item.ItemBase.Quality <= i ){
////				goSub.gameObject.SetActive(false);
////			}else{
////				goSub.gameObject.SetActive(true);
////				UISp = goSub.transform.Find("Star"+i).GetComponentInChildren<UISprite>();
////				if( StarCount > i ){
////					UISp.fillAmount = 1.0f;
////				}else{
////					UISp.fillAmount = Percent;
////				}
////			}
////		}
//	}
	
	
	void UpdateOneDescAttrib()
	{
		//cancel by Lindean 20141031
//		if(	mItemFocusId == -1 )
//			return;
//		
//		Item item = PlayerDataManager.Instance.BackPack.Find( mItemFocusId );
//		if( item == null )
//			return;
//		
//		UILabel uigo = Control("Name").GetComponent<UILabel>();
//		switch(item.ItemBase.Quality)
//		{
//			case (int)ItemQuality.IQ_White:uigo.text = "[FFFFFF]" + item.ItemBase.Name + "[-]";break;
//			case (int)ItemQuality.IQ_Green:uigo.text = "[3BBC34]" + item.ItemBase.Name + "[-]";break;
//			case (int)ItemQuality.IQ_Blue:uigo.text = "[0F7BBF]" + item.ItemBase.Name + "[-]";break;
//			case (int)ItemQuality.IQ_Purple:uigo.text = "[400080]" + item.ItemBase.Name + "[-]";break;
//		}
//		SetStarCount(item);	
//		Control ("Desc").GetComponent<UILabel>().text = item.ItemBase.Desc;
//		TargetAttrib Ta = TargetAttribManager.Instance.GetItem(item.ItemBase.MainType,item.ItemBase.SubType);
//		Control("LabelMain").GetComponent<UILabel>().text = Ta.ShowName + "   [C78100]" + GetMainAttribBySubType(item);	
	}
	
	void UpdateAttribOnePanel()
	{
		GameObject go = Control("AttribRoot");
		GameObject Subgo = Control("AttribAll(Clone)");
		if( Subgo != null ){
			GameObject.Destroy(Subgo);
		}
		Subgo = Control ("AttribOne(Clone)");
//		if( Subgo != null ){
//			GameObject.Destroy(Subgo);
//		}
		if(Subgo)
			return;
		
		GameObject One = GameObject.Instantiate(Resources.Load("AttribOne")) as GameObject;
		
		One.transform.parent = go.transform;
		One.transform.localScale = Vector3.one;
		One.transform.localPosition = Vector3.zero;
		
		mSubItemFocus = Control("GridItemFocus",One);
		mSubItemFocus.SetActive(false);
		mAttibIcon = Control("IconAttrib",One);
		UIEventListener.Get(Control("Equip")).onClick = OnClickEquip;
		UIEventListener.Get(Control("UnEquip")).onClick = OnClickUnEquip;
	}
	
	void OnClickCharactor ( GameObject go )
	{
		mItemFocus.SetActive(false);
		UpdateAttribAllPanel();
	}
	
	void OnClickEquipItem( GameObject go)
	{
		mItemFocusId = int.Parse( go.transform.Find("ItemID").GetComponent<UILabel>().text );
		UpdateAttribOnePanel();
		InitGridIconDesc();
		UpdateOneAttrib(go);
		UpdateItemFocus(go);
		UpdateOneDescAttrib();
	}
	
	//*********************************************************************************************************//
	//void RequestPackageInfo()
	//{
 //       Request(new GetBackPackCmd(), delegate(string err, Response response)
 //       {
 //           if (!string.IsNullOrEmpty(err))
 //           {
 //               Debug.LogError("RequestPackageInfo Error!!" + err);
	//			MessageBoxWnd.Instance.Show(err);
 //               return;
 //           }
 //           // update the user 
 //           Package backPackData = (response != null) ? response.Parse<Package>() : null;

 //           Debug.Log("backPackData:" + LitJson.JsonMapper.ToJson(backPackData));

 //           if (backPackData != null)
 //           {
 //               PlayerDataManager.Instance.OnBackPack(backPackData, true);
 //               //ShowProcessLoadingEnd();
	//			UpdateEquipment();
	//			UpdateOneAttrib();
	//			UpdateCharactor();
 //           }
 //       });
	//	//ShowProcessLoadingStart();
	//}
}
