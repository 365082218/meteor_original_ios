public class AnimationCellData : AbstractCellData {

    public override System.Type CellControllerType {
        get {
            return typeof(AnimationCellController);
        }
    }

    public int animation;

    public AnimationCellData(int animationIdx) {
        animation = animationIdx;
    }
}
