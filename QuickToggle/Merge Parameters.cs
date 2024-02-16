#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Collections.Generic;
using UnityEditor.Animations;

public class AnimatorToVRCParameterMerger : EditorWindow
{
    public AnimatorController animatorController;
    public VRCExpressionParameters vrcParameterAsset;

    [MenuItem("QuickToggle/Animator to VRC Parameter Merger")]
    public static void ShowWindow()
    {
        GetWindow(typeof(AnimatorToVRCParameterMerger), false, "Animator to VRC Parameter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animator to VRC Parameter", EditorStyles.boldLabel);

        animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(AnimatorController), false);
        vrcParameterAsset = (VRCExpressionParameters)EditorGUILayout.ObjectField("VRC Parameter Asset", vrcParameterAsset, typeof(VRCExpressionParameters), false);

        if (GUILayout.Button("Merge Parameters"))
            MergeParameters();
    }

    private void MergeParameters()
    {
        if (animatorController == null || vrcParameterAsset == null)
        {
            Debug.LogError("Please assign valid assets");
            return;
        }

        List<AnimatorControllerParameter> animatorParams = new List<AnimatorControllerParameter>(animatorController.parameters);
        List<VRCExpressionParameters.Parameter> vrcParams = new List<VRCExpressionParameters.Parameter>(vrcParameterAsset.parameters);
        bool t = false;
        foreach (var param in animatorParams)
        {
            if (param.name == "QuickToggleParam_____-____QuickToggleParm") { t = true; continue; }
            if (!t) continue;
            if (!vrcParams.Exists(p => p.name == param.name))
            {
                VRCExpressionParameters.Parameter newParameter = new VRCExpressionParameters.Parameter();
                newParameter.name = param.name;
                newParameter.valueType = VRCExpressionParameters.ValueType.Int; // Change this to match the parameter type in your VRC Parameter Asset
                newParameter.saved = false;
                vrcParams.Add(newParameter);
            }
            else
                Debug.LogWarning($"Parameter {param.name} already exists in VRC Parameter Asset. Skipping.");
        }

        vrcParameterAsset.parameters = vrcParams.ToArray();

        Debug.Log("Parameters merged successfully");
    }
}
#endif