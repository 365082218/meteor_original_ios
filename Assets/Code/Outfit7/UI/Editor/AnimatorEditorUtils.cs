using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Outfit7.UI {
    public static class AnimatorEditorUtils {

        [MenuItem("Assets/Outfit7 Unity Animation (Old)/Create Animations in selected Animator", true)]
        public static bool ValidateCreateAnimationInAnimator() {
            return Selection.activeObject as AnimatorController != null;
        }

        [MenuItem("Assets/Outfit7 Unity Animation (Old)/Create Animations in selected Animator", false)]
        public static void CreateAnimationInAnimator() {
            AnimatorController animator = Selection.activeObject as AnimatorController;

            for (int i = 0; i < animator.layers[0].stateMachine.states.Length; i++) {
                if (animator.layers[0].stateMachine.states[i].state.motion != null) {
                    continue;
                }
                AnimationClip animationClip = new AnimationClip();
                animationClip.name = animator.layers[0].stateMachine.states[i].state.name;
                AssetDatabase.AddObjectToAsset(animationClip, animator);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animationClip));
                animator.layers[0].stateMachine.states[i].state.motion = animationClip;
            }

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animator), ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh();
        }
    }
}
