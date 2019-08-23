using System;
using System.Collections.Generic;
using Idevgame.StateManagement;
using Idevgame.StateManagement.GameStateManagement;
using Idevgame.Util;
using Idevgame.GameState.DialogState;
using UnityEngine;
//�־û�״̬,��פ�ڳ����ϵ�.
namespace Idevgame.GameState {
    public class PersistStateMgr:Singleton<PersistStateMgr>{
        public GameOverlayDialogState GameOverlay;//�������������
        public FightDialogState FightDialog;//ս���������
        List<PersistState> activeState;
        Dictionary<MonoBehaviour, PersistState> StateHash = new Dictionary<MonoBehaviour, PersistState>();
        public PersistStateMgr() {

        }

        public void Init() {
            activeState = new List<PersistState>();
            GameOverlay = new GameOverlayDialogState();
        }

        public void EnterState(PersistState state)
        {
            if (activeState.Contains(state))
                return;
            state.OnStateEnter();
            activeState.Add(state);
            if (state.Owner != null)
                StateHash.Add(state.Owner, state);
        }

        public void ExitStateByOwner(UnityEngine.MonoBehaviour Owner)
        {
            if (StateHash.ContainsKey(Owner))
            {
                PersistState state = StateHash[Owner];
                ExitState(state);
            }
        }

        public void ExitState(PersistState state)
        {
            if (!activeState.Contains(state))
                return;
            StateHash.Remove(state.Owner);
            state.OnStateExit();
            activeState.Remove(state);
        }

        public bool StateActive(PersistState state)
        {
            return activeState.Contains(state);
        }

        public void Update()
        {
            for (int i = 0; i < activeState.Count; i++)
            {
                activeState[i].OnUpdate();
            }
        }

        public void OnLateUpdate()
        {
            for (int i = 0; i < activeState.Count; i++)
            {
                activeState[i].OnLateUpdate();
            }
        }
    }
    
    public class GameOverlayDialogState: PersistDialog<GameOverlayWnd>
    {
        public override string DialogName { get { return "GameOverlayWnd"; } }
        public GameOverlayDialogState():base()
        {

        }

        //�Դ������������ڻ���֮�µ�Ԥ��
        protected override bool CanvasMode()
        {
            return true;
        }
    }

    public class FightDialogState: PersistDialog<FightUiConroller>
    {
        public override string DialogName { get { return "FightWnd"; } }
        public FightDialogState():base()
        {

        }
    }
}
