#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.IO;
using System.Collections.Generic;

public class AnimationLayerCreator : EditorWindow
{
    private AnimatorController animatorController;
    private DefaultAsset animationFolder;

    [MenuItem("QuickToggle/Create Animation Layers")]
    static void Init()
    {
        AnimationLayerCreator window = (AnimationLayerCreator)EditorWindow.GetWindow(typeof(AnimationLayerCreator));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Animator Controller", EditorStyles.boldLabel);
        animatorController = EditorGUILayout.ObjectField("Controller", animatorController, typeof(AnimatorController), false) as AnimatorController;

        GUILayout.Space(10);

        GUILayout.Label("Animation Folder", EditorStyles.boldLabel);
        animationFolder = EditorGUILayout.ObjectField("Folder", animationFolder, typeof(DefaultAsset), false) as DefaultAsset;

        GUILayout.Space(20);

        if (GUILayout.Button("Add Animation Layers")){
            AnimatorControllerParameter param = new AnimatorControllerParameter();
            param.type = AnimatorControllerParameterType.Bool;
            param.name = "QuickToggleParam_____-____QuickToggleParm";
            animatorController.AddParameter(param);
            AddLayersFromFolder();
        }
    }

    void AddLayersFromFolder()
    {
        if (animatorController == null)
        {
            Debug.LogError("Please assign an Animator Controller.");
            return;
        }

        if (animationFolder == null)
        {
            Debug.LogError("Please provide a valid animation folder.");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(animationFolder);
        string[] animationFiles = Directory.GetFiles(folderPath, "*.anim");

        List<AnimatorControllerLayer> existingLayers = new List<AnimatorControllerLayer>(animatorController.layers);

        foreach (string animationFile in animationFiles)
        {
            string animationName = Path.GetFileNameWithoutExtension(animationFile);

            bool layerExists = false;
            foreach (AnimatorControllerLayer layer in existingLayers)
                if (layer.name == animationName)
                {
                    layerExists = true;
                    Debug.LogWarning("Animation layer '" + animationName + "' already exists. Skipping.");
                    break;
                }

            if (!layerExists)
            {
                AnimatorControllerLayer layer = new AnimatorControllerLayer();
                layer.name = animationName;
                layer.stateMachine = new AnimatorStateMachine();
                layer.defaultWeight = 1;

                AnimatorControllerParameter param = new AnimatorControllerParameter();
                param.type = AnimatorControllerParameterType.Int;
                param.name = animationName + "_" + UnityEngine.Random.Range(1000, 9999);
                animatorController.AddParameter(param);

                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animationFile);
                AnimatorState state = layer.stateMachine.AddState(animationName);
                state.motion = clip;

                AnimatorState emptyState = layer.stateMachine.AddState("EmptyState");
                layer.stateMachine.defaultState = emptyState;

                AnimatorStateTransition transitionToAnimation = emptyState.AddTransition(state);
                transitionToAnimation.AddCondition(AnimatorConditionMode.Equals, 1, param.name);
                transitionToAnimation.duration = 0;
                transitionToAnimation.exitTime = 0;
                transitionToAnimation.hasFixedDuration = false;

                AnimatorStateTransition transitionToEmpty = state.AddTransition(emptyState);
                transitionToEmpty.AddCondition(AnimatorConditionMode.Equals, 0, param.name);
                transitionToEmpty.duration = 0;
                transitionToEmpty.exitTime = 0;
                transitionToEmpty.hasFixedDuration = false;

                AssetDatabase.AddObjectToAsset(state, animatorController);
                AssetDatabase.AddObjectToAsset(transitionToAnimation, animatorController);
                AssetDatabase.AddObjectToAsset(transitionToEmpty, animatorController);
                AssetDatabase.AddObjectToAsset(layer.stateMachine, animatorController);

                existingLayers.Add(layer);
            }
        }

        animatorController.layers = existingLayers.ToArray();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Animation layers, parameters, and transitions added successfully.");
    }
}
#endif
