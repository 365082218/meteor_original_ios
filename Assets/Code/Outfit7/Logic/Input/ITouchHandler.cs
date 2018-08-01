namespace Outfit7.Logic.Input {

    public interface ITouchHandler {
        bool HandleTouch(TouchWrapper touch);

        int Priority { get; }
    }
}
