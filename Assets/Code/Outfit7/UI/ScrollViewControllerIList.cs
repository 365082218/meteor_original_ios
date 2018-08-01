using UnityEngine;
using System;
using System.Collections.Generic;

namespace Outfit7.UI {
    public partial class ScrollViewController : IList<AbstractCellData> {
        public override void Add(AbstractCellData item) {
            base.Add(item);
            Type type = item.CellControllerType;
            RectTransform newContainer = SetupNewContainer(type);
            SetContainerIndexAndParentIt(CellDataList.Count - 1, newContainer, type);
            CountChanged = true;
            RefreshSnapParameters();
        }

        public override void Clear() {
            for (int i = CellDataList.Count - 1; i >= 0; i--) {
                Despawn(CellDataList[i]);
                RemoveContainer(i);
            }
            base.Clear();
            LastVisibleMinIndex = -1;
            LastVisibleMaxIndex = -1;
            RefreshView();
        }

        public override void Insert(int index, AbstractCellData item) {
            base.Insert(index, item);
            Type type = item.CellControllerType;
            RectTransform newContainer = SetupNewContainer(type);
            SetContainerIndexAndParentIt(index, newContainer, type);
//            Spawn(newContainer, item);
            for (int i = index + 1; i < CellDataList.Count; i++) {
                SetContainerIndex(i);
            }

            CountChanged = true;
            RefreshSnapParameters();
        }

        public override void RemoveAt(int index) {
            Despawn(CellDataList[index]);
            RemoveContainer(index);
            base.RemoveAt(index);
            for (int i = index; i < CellDataList.Count; i++) {
                SetContainerIndex(i);
            }
            CountChanged = true;
            RefreshView();
        }

        public override AbstractCellData this[int index] {
            get {
                return CellDataList[index];
            }
            set {
                if (CellDataList[index].GetType().Equals(value.GetType())) {
                    AbstractCellController cc = CellDataList[index].CellControllerRef;
                    CellDataList[index] = value;
                    CellDataList[index].CellControllerRef = cc;
                    UpdateCell(value);
                } else {
                    Despawn(CellDataList[index]);
                    SetupExistingContainer(ContainerList[index], value.CellControllerType);
                    CellDataList[index] = value;
                    RectTransform ct = ContainerRectTransformList[index];
                    SetContainerIndexAndParentIt(index, ct, value.CellControllerType);
                    Spawn(ct, CellDataList[index]);
                    RefreshSnapParameters();
                }
            }
        }

        public override void CopyTo(AbstractCellData[] array, int arrayIndex) {
            base.CopyTo(array, arrayIndex);
            throw new NotImplementedException();
        }

        public override bool Remove(AbstractCellData item) {
            int index = CellDataList.IndexOf(item);
            bool removed = base.Remove(item);
            if (removed) {
                Despawn(item);
                RemoveContainer(index);
                for (int i = index; i < CellDataList.Count; i++) {
                    SetContainerIndex(i);
                }
                CountChanged = true;
                RefreshView();
            }
            return removed;
        }
    }
}