using Outfit7.Util;
using System;

namespace Outfit7.UI {
    public abstract class AbstractCellData {

        public abstract Type CellControllerType { get; }

        public AbstractCellController CellControllerRef = null;

        public Action<AbstractCellData> OnSpawn;
        public Action<AbstractCellData> OnDespawn;

        protected AbstractCellData() {
            O7Log.DebugT("AbstractCellData", "CellData (" + typeof(AbstractCellData).Name + ") with CellController (" + CellControllerType.Name + ") created");
        }

        public void UpdateCellController() {
            if (CellControllerRef != null) {
                CellControllerRef.UpdateCell();
            }
        }

        public string GetInputText()
        {
            if (CellControllerRef != null)
                return CellControllerRef.GetInputText();
            return string.Empty;
        }
    }
}