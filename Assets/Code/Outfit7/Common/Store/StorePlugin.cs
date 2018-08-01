//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Store {

    /// <summary>
    /// Store plugin for native calls.
    /// </summary>
    public class StorePlugin : MonoBehaviour {

        private const string Tag = "StorePlugin";

        public AbstractStoreManager StoreManager { get; set; }

#if UNITY_IPHONE
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _IsStoreAvailable();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartLoadingStoreData(string iapIds);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartBuying(string iapId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ConfirmPurchaseProcessed(string iapId, string receiptData);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartRestoring();
#endif

#if UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
        private Outfit7.Threading.Executor MainExecutor;

        private void Awake() {
            MainExecutor = new Outfit7.Threading.Executor();
            O7.Plugins.Wp8.UnityCommon.StoreNativeProvider.OnStoreDataLoad += __OnStoreDataLoad;
            O7.Plugins.Wp8.UnityCommon.StoreNativeProvider.OnBuyComplete += __OnBuyComplete;
            O7.Plugins.Wp8.UnityCommon.StoreNativeProvider.OnBuyFail += __OnBuyFail;
        }

        private void __OnStoreDataLoad(string data) {
            MainExecutor.Post(delegate{
                _OnStoreDataLoad(data);
            });
        }

        private void __OnBuyComplete(string purchaseId, string transactionId, double price, string currencyId, string receiptData) {
            MainExecutor.Post(delegate{
                OnBuyComplete(purchaseId, transactionId, price, currencyId, receiptData, null);
            });
        }

        private void __OnBuyFail(string purchaseId, string errorId) {
            MainExecutor.Post(delegate{
                OnBuyFail(purchaseId, errorId);
            });
        }
#endif

        public bool IsStoreAvailable() {
            O7Log.DebugT(Tag, "IsStoreAvailable()");

#if UNITY_EDITOR || NATIVE_SIM
            return true;
#elif UNITY_IPHONE
            return _IsStoreAvailable();
#elif UNITY_ANDROID
            return Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>("getPurchaseManagerWrapper","isStoreAvailable");
#elif UNITY_WP8
            return O7.Plugins.Wp8.UnityCommon.StoreNativeProvider.IsStoreAvaliable();
#endif
        }

        public void StartLoadingStoreData(HashSet<string> iapIds) {
            string iapIdsS = StringUtils.CollectionToCommaDelimitedString(iapIds);
            O7Log.DebugT(Tag, "StartLoadingStoreData({0})", iapIdsS);

#if UNITY_EDITOR
            WWW www = new WWW("file:///" + Application.dataPath + "/EditorTestFiles/store_sample.json.txt");
            while (!www.isDone) {
            }
            _OnStoreDataLoad(www.text);
#elif NATIVE_SIM
            TextAsset jsonFile = ResourceManager.Load("store_sample.json") as TextAsset;
            if (jsonFile != null) _OnStoreDataLoad(jsonFile.text);
#elif UNITY_IPHONE
            _StartLoadingStoreData(iapIdsS);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef("getPurchaseManagerWrapper", "startLoadingStoreData", iapIdsS);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.StoreNativeProvider.StartLoadingStoreData();
#endif
        }

        public void _OnStoreDataLoad(string data) {
            O7Log.DebugT(Tag, "_OnStoreDataLoad({0})", data);
            StoreManager.OnStoreDataLoad(data);
        }

        public void StartBuying(string iapId) {
            O7Log.DebugT(Tag, "StartBuying({0})", iapId);

#if UNITY_EDITOR || NATIVE_SIM
            string data = "{ id: " + iapId + ", transactionId: \"7834534hsjbfs88389234\", price: 1.99, currencyId: \"USD\", receiptData: \"<xml receiptData></xml>\", payload: 4504, }";
            _OnBuyComplete(data);
//            string data ="{ " + iapId + ": \"CANCELED\" }";
//            string data ="{ " + iapId + ": \"FAILURE\" }";
//            _OnBuyFail(data);
#elif UNITY_IPHONE
            _StartBuying(iapId);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef("getPurchaseManagerWrapper", "startBuying", iapId);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.StoreNativeProvider.StartBuying(iapId);
#endif
        }

        private void OnBuyComplete(string purchaseId, string transactionId, double price, string currencyId, string receiptData, string payload) {
            O7Log.DebugT(Tag, "OnBuyComplete({0}, {1}, {2}, {3}, {4}, {5})", purchaseId, transactionId, price, currencyId, receiptData, payload);
            StoreManager.OnBuyComplete(purchaseId, transactionId, price, currencyId, receiptData, payload);
        }

        public void _OnBuyComplete(string data) {
            O7Log.DebugT(Tag, "_OnBuyComplete({0})", data);

            JSONNode dataJ = JSON.Parse(data);
            string id = dataJ["id"];
            string transactionId = dataJ["transactionId"];
            double price = dataJ["price"].AsDouble;
            string currencyId = dataJ["currencyId"];
            string receipt = dataJ["receiptData"];
            string payload = dataJ["payload"];

            OnBuyComplete(id, transactionId, price, currencyId, receipt, payload);
        }

        private void OnBuyFail(string purchaseId, string errorId) {
            O7Log.DebugT(Tag, "OnBuyFail({0}, {1})", purchaseId, errorId);
            StoreManager.OnBuyFail(purchaseId, errorId);
        }

        public void _OnBuyFail(string data) {
            O7Log.DebugT(Tag, "_OnBuyFail({0})", data);

            JSONNode dataJ = JSON.Parse(data);
            JSONNode purchaseJ = dataJ[0]; // Only one node
            string id = purchaseJ.Key;
            string errorId = purchaseJ.Value;

            OnBuyFail(id, errorId);
        }

        public void ConfirmPurchaseProcessed(string iapId, string receiptData) {
            O7Log.DebugT(Tag, "ConfirmPurchaseProcessed({0}, {1})", iapId, receiptData);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _ConfirmPurchaseProcessed(iapId, receiptData);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef("getPurchaseManagerWrapper", "confirmPurchaseProcessed", iapId, receiptData);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.StoreNativeProvider.ConfirmPurchaseProcessed(iapId, receiptData);
#endif
        }

        public void StartRestoring() {
            O7Log.DebugT(Tag, "StartRestoring()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _StartRestoring();
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }
    }
}
