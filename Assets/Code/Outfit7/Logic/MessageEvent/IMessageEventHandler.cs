namespace Outfit7.Logic {
    public interface IMessageEventHandler {
        bool OnMessageEvent(MessageEvent msgEvent);
    }
}