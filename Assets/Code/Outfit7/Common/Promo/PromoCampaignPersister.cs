using Outfit7.Util.Io;
using SimpleJSON;
using UnityEngine;
using System;
using Outfit7.Util;

namespace Outfit7.Common.Promo {
    public abstract class PromoCampaignPersister : JsonFileReaderWriter {

        private static string CreateFilePath(string fileName) {
            return Application.persistentDataPath + "/" + fileName;
        }

        protected static void DeleteFile(string fileName) {
            string filePath = CreateFilePath(fileName);
            try {
                O7File.Delete(filePath);
            } catch (Exception e) { // HR-5214
                O7Log.WarnT("PromoCampaignPersister", e, "Cannot delete file {0}", filePath);
            }
        }

        private JSONNode CampaignJ;

        protected PromoCampaignPersister(string fileName) : base(CreateFilePath(fileName)) {
        }

        public JSONNode LoadCampaign() {
            if (CampaignJ == null) {
                CampaignJ = ReadJson();
            }
            return CampaignJ;
        }

        public void SaveCampaign(JSONNode campaignJ) {
            CampaignJ = campaignJ;
            WriteJson(campaignJ);
        }

        public abstract void DeleteCampaign();
    }
}
