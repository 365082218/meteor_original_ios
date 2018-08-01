using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.Ad
{
    public class AdO7Interstitial
    {
        public bool Ready { get; set; }

        public int LastSessionShown { get; set; }

        public int LastSessionShownCount { get; set; }

        public int PerSession { get; private set; }

        public string Url { get; private set; }

        public string Channel { get; private set; }

        public List<int> Sequence { get; private set; }

        public AdO7Interstitial(string channel, List<int> sequence, int perSession, string url){
            LastSessionShown = -1;
            Channel = channel;
            Sequence = sequence;
            PerSession = perSession;
            Url = url;
        }

        public void UpdateWithOld(AdO7Interstitial interstitial){
            Ready = interstitial.Ready;
            LastSessionShown = interstitial.LastSessionShown;
            LastSessionShownCount = interstitial.LastSessionShownCount;
        }

        public override string ToString() {
            return string.Format("[AdO7Interstitial: Ready={0}, LastSessionShown={1}, PerSession={2}, Url={3}, Channel={4}, Sequence={5}]", Ready, LastSessionShown, PerSession, Url, Channel, Sequence);
        }
    }
}
