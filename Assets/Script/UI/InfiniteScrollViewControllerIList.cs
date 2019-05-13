using System;
using System.Collections.Generic;

public partial class InfiniteScrollViewController : IList<AbstractCellData> {
    public override void Add(AbstractCellData item) {
        base.Add(item);
    }

    public override void Clear() {
        base.Clear();
    }

    public override void Insert(int index, AbstractCellData item) {
        base.Insert(index, item);
        throw new NotImplementedException();
    }

    public override void RemoveAt(int index) {
        base.RemoveAt(index);
        throw new NotImplementedException();
    }

    public override AbstractCellData this[int index] {
        get {
            return CellDataList[index];
        }
        set {
            throw new NotImplementedException();
        }
    }

    public override void CopyTo(AbstractCellData[] array, int arrayIndex) {
        base.CopyTo(array, arrayIndex);
        throw new NotImplementedException();
    }

    public override bool Remove(AbstractCellData item) {
        base.Remove(item);
        throw new NotImplementedException();
    }
}