using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AnimatorControllerAutomator : EditorWindow
{
    [MenuItem("Tools/Animator Automator")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorControllerAutomator>("Animator Automator");
    }

    private AnimatorController animatorController;
    private string clipsFolder = "Assets/Synty/AnimationIdles/Animations/Polygon/Feminine";
    private string baseStateName = "A_POLY_IDL_Base_Femn";

    void OnGUI()
    {
        GUILayout.Label("Animator Controller Setup", EditorStyles.boldLabel);
        animatorController = EditorGUILayout.ObjectField("Controller", animatorController, typeof(AnimatorController), false) as AnimatorController;
        clipsFolder = EditorGUILayout.TextField("Clips Folder", clipsFolder);
        baseStateName = EditorGUILayout.TextField("Base State Name", baseStateName);

        if (GUILayout.Button("Generate Triggers & States"))
        {
            if (animatorController == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign an Animator Controller.", "OK");
                return;
            }
            AddClipsToController();
        }
    }

    private void AddClipsToController()
    {
        // Find all AnimationClips in the specified folder
        var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { clipsFolder });
        var clips = guids
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .Select(path => AssetDatabase.LoadAssetAtPath<AnimationClip>(path))
            .Where(clip => clip != null)
            .ToList();

        var layer = animatorController.layers[0];
        var sm = layer.stateMachine;

        // Find Base state
        var baseState = sm.states
            .Select(s => s.state)
            .FirstOrDefault(st => st.name == baseStateName);
        if (baseState == null)
        {
            EditorUtility.DisplayDialog("Error", $"Base state '{baseStateName}' not found in state machine.", "OK");
            return;
        }

        int added = 0;
        foreach (var clip in clips)
        {
            string clipName = clip.name;

            // Add Trigger parameter if missing
            if (!animatorController.parameters.Any(p => p.name == clipName))
            {
                animatorController.AddParameter(clipName, AnimatorControllerParameterType.Trigger);
            }

            // Add State if missing
            if (!sm.states.Any(s => s.state.name == clipName))
            {
                var state = sm.AddState(clipName);
                state.motion = clip;

                // Transition from Base -> clip on trigger
                var trans = baseState.AddTransition(state);
                trans.AddCondition(AnimatorConditionMode.If, 0, clipName);
                trans.hasExitTime = false;
                trans.exitTime = 0;

                // Transition from clip -> Base after finish
                var exit = state.AddTransition(baseState);
                exit.hasExitTime = true;
                exit.exitTime = 1;

                added++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done", $"Added {added} clips to '{animatorController.name}'.", "OK");
    }
}
