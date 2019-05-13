using UnityEngine;
using System;
using System.Collections.Generic;

public abstract partial class AbstractScrollViewController : IList<AbstractCellData> {
    public virtual void Add(AbstractCellData item) {
        CellDataList.Add(item);
    }

    public virtual void Clear() {
        CellDataList.Clear();
    }

    public virtual int IndexOf(AbstractCellData item) {
        return CellDataList.IndexOf(item);
    }

    public virtual void Insert(int index, AbstractCellData item) {
        CellDataList.Insert(index, item);
    }

    public virtual void RemoveAt(int index) {
        CellDataList.RemoveAt(index);
    }

    public virtual AbstractCellData this[int index] {
        get {
            return CellDataList[index];
        }
        set {
            CellDataList[index] = value;
        }
    }

    public virtual bool Contains(AbstractCellData item) {
        return CellDataList.Contains(item);
    }

    public virtual void CopyTo(AbstractCellData[] array, int arrayIndex) {
        throw new NotImplementedException();
//            CellDataList.CopyTo(array, arrayIndex);
//            for (int i = 0; i < array.Length; i++) {
//                SetCellParent(Spawn(array[i]));
//            }
//            for (int i = arrayIndex; i < CellDataList.Count; i++) {
//                SetSiblingIndex(i);
//            }
    }

    public virtual bool Remove(AbstractCellData item) {
        return CellDataList.Remove(item);
    }

    public virtual int Count {
        get {
            return CellDataList.Count;
        }
    }

    public virtual bool IsReadOnly {
        get {
            return false;
        }
    }

    public virtual IEnumerator<AbstractCellData> GetEnumerator() {
        return CellDataList.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
        return CellDataList.GetEnumerator();
    }
}