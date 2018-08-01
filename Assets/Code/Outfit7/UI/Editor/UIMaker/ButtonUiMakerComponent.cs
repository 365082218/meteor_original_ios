using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace Outfit7.UI {
    public class ButtonUiMakerComponent : UiMakerComponent {

        public override string TypeName { get { return "Button"; } }

        public override bool IsSimpleType { get { return false; } }

        public override string NamePrefix { get { return "btn"; } }

        private bool DialogButton = false;

        private Type DialogActionButtonType;
        private Type GameActionButtonType;

        private RuntimeAnimatorController RuntimeAnimatorController = null;

        private ButtonUiMakerData Data;

        /*
        Use this method if needed
        private Type GetTypeIfExists(string className) {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.Name == className
                select type).FirstOrDefault();
        }
        */

        public void SetGameActionButton() {
            DialogButton = false;
        }

        public void SetDialogActionButton() {
            DialogButton = true;
        }

        public override void Init() {

            Data = ProjectUiMakerComponent.GetData<ButtonUiMakerData>();

            DialogActionButtonType = Type.GetType(string.Format("{0}.DialogActionButton, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", Data.DialogActionButtonNamespace));
            GameActionButtonType = Type.GetType(string.Format("{0}.GameActionButton, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", Data.GameActionButtonNamespace));
        }

        public override void OnGui() {
            base.OnGui();

            if (GameActionButtonType == null || DialogActionButtonType == null) {
                if (DialogActionButtonType == null) {
                    EditorGUILayout.HelpBox("DialogActionButton class doesn't exist in the project", MessageType.Warning);
                }
                if (GameActionButtonType == null) {
                    EditorGUILayout.HelpBox("GameActionButton class doesn't exist in the project", MessageType.Warning);
                }
                return;
            }

            DialogButton = EditorGUILayout.Toggle("Dialog Button", DialogButton);

            GUILayout.Label("Runtime Animator Controller", EditorStyles.label);
            RuntimeAnimatorController = EditorGUILayout.ObjectField(RuntimeAnimatorController, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
            if (RuntimeAnimatorController == null) {
                RuntimeAnimatorController[] animatorControllers = GetAssetsWithFilter<RuntimeAnimatorController>("t:AnimatorController");

                if (animatorControllers.Length > 0) {
                    for (int i = 0; i < animatorControllers.Length; i++) {
                        if (animatorControllers[i].name.Contains("Button")) {
                            RuntimeAnimatorController = animatorControllers[i];
                            break;
                        }
                    }
                }
            }

            SetCommonCreatePanel();
        }

        protected override RectTransform OnCreateExecute() {
            RectTransform rectTransform = base.OnCreateExecute();

            AbstractActionButton actionButton;
            if (DialogButton) {
                actionButton = rectTransform.gameObject.AddComponent(DialogActionButtonType) as AbstractActionButton;
            } else {
                actionButton = rectTransform.gameObject.AddComponent(GameActionButtonType) as AbstractActionButton;
            }

            actionButton.transition = UnityEngine.UI.Selectable.Transition.Animation;
            Animator animator = actionButton.gameObject.AddComponent<Animator>();

            animator.runtimeAnimatorController = RuntimeAnimatorController;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            return rectTransform;
        }
    }
}
