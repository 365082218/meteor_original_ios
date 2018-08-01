
namespace Outfit7.UI {
    public abstract class AbstractCellController : UnityEngine.EventSystems.UIBehaviour {
    
        protected AbstractCellData Data;

        public virtual void UpdateCell(AbstractCellData data) {
            Data = data;
            data.CellControllerRef = this;
        }

        public void UpdateCell() {
            if (Data != null)
                UpdateCell(Data);
        }

        public virtual string GetInputText()
        {
            return string.Empty;
        }
    }
}