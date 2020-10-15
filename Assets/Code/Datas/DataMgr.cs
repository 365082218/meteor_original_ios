using Excel2Json;
using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Excel2Json {
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public interface ITableItem {
        int Key { get; }
    }

    public class DataMgr:Singleton<DataMgr> {
        public LevelDataMgr debugData;
        public LevelDataMgr levelData;
        public ItemDataMgr itemData;
        public WeaponDataMgr weaponData;
        public ModelDataMgr modelData;
        public ActionDataMgr actionData;
        public InputDataMgr inputData;
        protected T LoadData<T>(string table) where T : class {
            T createTable = System.Activator.CreateInstance<T>();
            loadJson<T>(createTable, table);
            return createTable;
        }

        public void loadJson<T>(T container, string file) {
            string json_content;
            if (System.IO.File.Exists(file)) {
                string js = System.IO.File.ReadAllText(file);
                json_content = js;
            } else {
                TextAsset asset = Resources.Load<TextAsset>(file);
                if (asset != null) {
                    json_content = asset.text;
                } else {
                    json_content = file;
                }
            }
            System.Type tp = container.GetType();
            System.Reflection.FieldInfo[] fields = tp.GetFields();
            for (int i = 0; i < fields.Length; i++) {
                fields[i].SetValue(container, System.Activator.CreateInstance(fields[i].FieldType));
                Parse(fields[i].GetValue(container), json_content, fields[i].Name.Substring(0, fields[i].Name.Length - 1));
            }
        }

        public static void Parse(object destion, string json, string table) {
            Dictionary<string, object> dict = Json.Deserialize(json) as Dictionary<string, object>;
            System.Reflection.MethodInfo method = destion.GetType().GetMethod("Add");
            if (dict.ContainsKey(table)) {
                System.Type tp = System.Type.GetType("Excel2Json." + table);
                Dictionary<string, object> sheetData = dict[table] as Dictionary<string, object>;
                foreach (var each in sheetData) {
                    object data = System.Activator.CreateInstance(tp);
                    SyncField(data, each.Value as Dictionary<string, object>);
                    try {
                        method.Invoke(destion, new object[] { System.Convert.ToInt32(each.Key), data });
                    }
                    catch (System.Exception exp) {
                        Debug.LogError(each.Key + " " + exp.StackTrace);
                    }
                    
                }
            }
        }

        public static void SyncField<T>(T target, Dictionary<string, object> source) {
            System.Reflection.FieldInfo[] fields = target.GetType().GetFields();
            for (int i = 0; i < fields.Length; i++) {
                try {
                    if (source.ContainsKey(fields[i].Name))
                        fields[i].SetValue(target, source[fields[i].Name]);
                } catch (System.Exception exp) {
                    Debug.LogError("exist error:" + fields[i].Name + " " + exp.StackTrace);
                }
                
            }
        }

        //读取所有默认的表.除了插件的
        public void LoadAllData() {
            debugData = LoadData<LevelDataMgr>("Debug");
            levelData = LoadData<LevelDataMgr>("Level");
            itemData = LoadData<ItemDataMgr>("Item");
            weaponData = LoadData<WeaponDataMgr>("Weapon");
            modelData = LoadData<ModelDataMgr>("Model");
            actionData = LoadData<ActionDataMgr>("Action");
            inputData = LoadData<InputDataMgr>("Input");
        }

        //对每个表会生成一系列代理方法，去查找每个sheet,当一个表有多个sheet时
        //1：寻找该表的每个对象的
        public LevelData GetLevelData(int id) {
            if (levelData.LevelDatas.ContainsKey(id))
                return levelData.LevelDatas[id];
            return null;
        }

        public List<LevelData> GetLevelDatas() {
            return levelData.LevelDatas.Values.ToList();
        }

        public List<LevelData> GetDebugLevelDatas() {
            return debugData.LevelDatas.Values.ToList();
        }
        public ItemData GetItemData(int id) {
            if (itemData.ItemDatas.ContainsKey(id))
                return itemData.ItemDatas[id];
            return null;
        }

        public List<ItemData> GetItemDatas() {
            return itemData.ItemDatas.Values.ToList();
        }

        public WeaponData GetWeaponData(int id) {
            if (weaponData.WeaponDatas.ContainsKey(id))
                return weaponData.WeaponDatas[id];
            return null;
        }

        public List<WeaponData> GetWeaponDatas() {
            return weaponData.WeaponDatas.Values.ToList();
        }

        public ModelData GetModelData(int id) {
            if (modelData.ModelDatas.ContainsKey(id))
                return modelData.ModelDatas[id];
            return null;
        }

        public List<ModelData> GetModelDatas() {
            return modelData.ModelDatas.Values.ToList();
        }

        public ActionData GetActionData(int id) {
            if (actionData.ActionDatas.ContainsKey(id))
                return actionData.ActionDatas[id];
            return null;
        }

        public List<ActionData> GetActionDatas() {
            return actionData.ActionDatas.Values.ToList();
        }

        public InputData GetInputData(int id) {
            if (inputData.InputDatas.ContainsKey(id))
                return inputData.InputDatas[id];
            return null;
        }
        public List<InputData> GetInputDatas() {
            return inputData.InputDatas.Values.ToList();
        }
    }

}
