using Outfit7.Common.Promo;

namespace Outfit7.DreamingOf {

    public class DreamingOfCampaignPersister : PromoCampaignPersister {

        private const string CampaignFileName = "DreamingOfCampaign.json";

        public static void ClearPrefs() {
            DeleteFile(CampaignFileName);
        }

        public DreamingOfCampaignPersister() : base(CampaignFileName) {
        }

        public override void DeleteCampaign() {
            ClearPrefs();
        }
    }
}
