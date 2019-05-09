using UnityEngine;
public class LockBehaviour : MonoBehaviour {
    public enum OrderType {
        Normal,
        Late
    }

    [SerializeField]
    protected OrderType orderType = OrderType.Normal;

    protected void Awake() {
        if (this.orderType == OrderType.Normal) {
            FrameReplay.UpdateEvent += this.LockUpdateWrap;
        }
        else {
            FrameReplay.LateUpdateEvent += this.LockUpdateWrap;
        }
    }
        
    protected void OnDestroy() {
        if (this.orderType == OrderType.Normal) {
            FrameReplay.UpdateEvent -= this.LockUpdateWrap;
        }
        else {
            FrameReplay.LateUpdateEvent -= this.LockUpdateWrap;
        }
    }

    private void LockUpdateWrap() {
        if (this.isActiveAndEnabled) {
            this.LockUpdate();
        }
    }
        
    protected virtual void LockUpdate() {}
}