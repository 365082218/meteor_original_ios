using System.Collections.Generic;
using SimpleJSON;
using Outfit7.Json;
using Outfit7.Util;

namespace Outfit7.Ad {
    public class AdO7InterstitialUnmarshaller {

        private const string Tag = "AdO7InterstitialUnmarshaller";

        public static Dictionary<string,AdO7Interstitial> Unmarshall(JSONNode json) {

            Dictionary<string,AdO7Interstitial> interstitials = new Dictionary<string,AdO7Interstitial>();

            JSONArray o7InterstitialsJ = SimpleJsonUtils.EnsureJsonArray(json);

            if (o7InterstitialsJ != null) {

                foreach (JSONNode o7InterstitialJ in o7InterstitialsJ) {

                    List<int> sequences = new List<int>();
                    string channel = o7InterstitialJ["channelId"];
                    string url = o7InterstitialJ["url"];
                    int perSession = -1;
                    if (!StringUtils.IsNullOrEmpty(o7InterstitialJ["perSession"])) {
                        perSession = o7InterstitialJ["perSession"].AsInt;
                    }

                    JSONArray sequencesJ = SimpleJsonUtils.EnsureJsonArray(o7InterstitialJ["sequence"]);

                    if (sequencesJ != null) {
                        foreach (JSONNode sequenceJ in sequencesJ) {
                            int sequence = sequenceJ.AsInt;
                            if (sequence > 0) {
                                sequences.Add(sequence);
                            }
                        }
                    }

                    if (!StringUtils.IsNullOrEmpty(channel) && !StringUtils.IsNullOrEmpty(url) && sequences.Count > 0) {
                        AdO7Interstitial ad = new AdO7Interstitial(channel, sequences, perSession, url);
                        interstitials.Add(ad.Channel, ad);
                        O7Log.DebugT(Tag, "adding {0}",ad.ToString());
                    }


                }// for each interstitial
            }

            return interstitials;
        }
    }
}
