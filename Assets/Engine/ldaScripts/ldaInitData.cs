using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Data
{
	[Serializable]
	public class ldaInitData  {
		
		//需要加载的角色数据（英雄x1，怪物x4），如果3dsmax导出资源导入不提供Scale和Rotae，坐标的变换，必须要3dsmmax按要求做好比例和局部坐标
		[Serializable]
		public class ldaCharacterRes
		{
			string mPrefabName = "";
			[DisplayName("Prefab资源名")]//需要加载的Prefab名称
			[DefaultValue("")]
			public string PrefabName { get { return mPrefabName; } set { mPrefabName = value; } }

			//动作ID
			int mUnitID = -1;
			public int UnitID { get { return mUnitID; } set { mUnitID = value; } }

			bool mIsExRes = false;
			//[DisplayName("是否外部加载资源")]//是否外部打包资源
			//[DefaultValue(false)]
			public bool IsExRes { get { return mIsExRes; } set { mIsExRes = value; } }

			string mExUnityResFile = "./ldaActionData/ExAssetBundle/";//如果是外部加载的资源名称（非游戏原来有的）需要填写"外部资源文件名"
			//[DisplayName("外部资源文件名")]
			//[DefaultValue("ldaActionDataExAssetBundle")]
			public string ExUnityResFile { get { return mExUnityResFile; } set { mExUnityResFile = value; } }

			bool mIsAttack = false; // 怪物是否自动攻击
			public bool IsAttack { get { return mIsAttack; } set { mIsAttack = value; } }

			int mCharacterType = 0; // 0.英雄 hero  1.怪物 guaiwu
			//[DisplayName("加载角色类型")]
			//[DefaultValue(0)]
			public int CharacterType { get { return mCharacterType; } set { mCharacterType = value; } }


		}
		
//		//需要额外加载的声音资源（非游戏原来打包的资源，需要额外添加的）
//		[Serializable]
//		public class ldaSoundRes
//		{
//			string mExUnityResFile = "";//如果是外部加载的资源名称（非游戏原来有的）需要填写"外部资源文件名"
//			[DefaultValue(""), DisplayName("外部资源文件名"), XmlAttribute("ExUnityResFile"), Description("外部打包好的资源文件")]
//			public string ExUnityResFile { get { return mExUnityResFile; } set { mExUnityResFile = value; } }
//			
//			string mPrefabName = "";
//			[DefaultValue(""), DisplayName("Prefab资源名"), XmlAttribute("PrefabName"), Description("需要加载的Prefab名称")]
//			public string PrefabName { get { return mPrefabName; } set { mPrefabName = value; } }
//		}
//
//		//需要额外加载的特效资源（非游戏原来打包的资源，需要额外添加的）
//		[Serializable]
//		public class ldaEffectsRes
//		{
//			string mExUnityResFile = "";//如果是外部加载的资源名称（非游戏原来有的）需要填写"外部资源文件名"
//			[DefaultValue(""), DisplayName("外部资源文件名"), XmlAttribute("ExUnityResFile"), Description("外部打包好的资源文件")]
//			public string ExUnityResFile { get { return mExUnityResFile; } set { mExUnityResFile = value; } }
//			
//			string mPrefabName = "";
//			[DefaultValue(""), DisplayName("Prefab资源名"), XmlAttribute("PrefabName"), Description("需要加载的Prefab名称")]
//			public string PrefabName { get { return mPrefabName; } set { mPrefabName = value; } }
//		}

		//加载角色列表 1x英雄，3x怪物
		private List<ldaCharacterRes> mldaCharacterResGroups = new List<ldaCharacterRes>();
		public List<ldaCharacterRes> ldaCharacterResGroups { get { return mldaCharacterResGroups; } }

//		//外部声音资源
//		private List<ldaSoundRes> mldaSoundResGroups = new List<ldaSoundRes>();
//		public List<ldaSoundRes> ldaSoundResGroups { get { return mldaSoundResGroups; } }
//
//		//外部特效资源
//		private List<ldaEffectsRes> mldaEffectsResGroups = new List<ldaEffectsRes>();
//		public List<ldaEffectsRes> ldaEffectsResGroups { get { return mldaEffectsResGroups; } }
	}
}
