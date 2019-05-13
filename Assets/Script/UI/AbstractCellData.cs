using System;
public abstract class AbstractCellData {

    public abstract Type CellControllerType { get; }

    public AbstractCellController CellControllerRef = null;

    public Action<AbstractCellData> OnSpawn;
    public Action<AbstractCellData> OnDespawn;

    protected AbstractCellData() {
            
    }

    public void UpdateCellController() {
        if (CellControllerRef != null) {
            CellControllerRef.UpdateCell();
        }
    }
}
