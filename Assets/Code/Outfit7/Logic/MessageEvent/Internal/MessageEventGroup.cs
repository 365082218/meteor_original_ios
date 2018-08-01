using Outfit7.Logic.Internal;
using Outfit7.Util;

namespace Outfit7.Logic.MessageEventInternal {

    public class MessageEventGroup {

        public BinarySortList<IMessageEventHandler> MessageEventHandlers = new BinarySortList<IMessageEventHandler>(32);

        public bool OnMessageEvent(MessageEvent messageEvent) {
            for (int i = 0; i < MessageEventHandlers.Count; ++i) {
                IMessageEventHandler handler = MessageEventHandlers[i];
                if (handler.OnMessageEvent(messageEvent)) {
#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
                    string desc = MessageEventManager.Instance.GetMessageEventDesc(messageEvent);
                    O7Log.DebugT("MessageEventGroup", "EventMessage processed by {0}\n{1}", handler.ToString(), desc);
#endif
                    return true;
                }
            }
            return false;
        }
    }

}