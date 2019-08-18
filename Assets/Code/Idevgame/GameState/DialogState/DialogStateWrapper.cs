//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//
using UnityEngine;
using System.Collections;

namespace  Idevgame.GameState.DialogState {
    public class DialogStateWrapper {
        public BaseDialogState DialogState;
        public object Data;

        public DialogStateWrapper(BaseDialogState dialogState, object data) {
            DialogState = dialogState;
            Data = data;
        }
    }
}
