using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ToggleAnimationCreator : EditorWindow
{
    private GameObject mainObject;
    private List<SelectedObjectData> selectedObjects = new List<SelectedObjectData>();
    private Vector2 scrollPosition;
    private DefaultAsset defaultFolder;
    private string animationName = "NewAnimation";

    [MenuItem("QuickToggle/ToggleAnimationCreator")]
    static void Init()
    {
        ToggleAnimationCreator window = (ToggleAnimationCreator)EditorWindow.GetWindow(typeof(ToggleAnimationCreator));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Parent Object", EditorStyles.boldLabel);
        mainObject = EditorGUILayout.ObjectField("Parent Object", mainObject, typeof(GameObject), true) as GameObject;

        GUILayout.Label("Selected Objects", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            selectedObjects[i].gameObject = EditorGUILayout.ObjectField(selectedObjects[i].gameObject, typeof(GameObject), true) as GameObject;
            selectedObjects[i].isSelected = EditorGUILayout.Toggle(selectedObjects[i].isSelected);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Selected Objects"))
            foreach (GameObject selectedObject in Selection.objects)
                if (selectedObject != null && selectedObject != mainObject && !IsObjectAlreadySelected(selectedObject))
                    selectedObjects.Add(new SelectedObjectData(selectedObject, true));

        if (GUILayout.Button("Remove Selected Object"))
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject != null)
            {
                SelectedObjectData dataToRemove = selectedObjects.Find(data => data.gameObject == selectedObject);
                if (dataToRemove != null)
                    selectedObjects.Remove(dataToRemove);
            }
        }

        if (GUILayout.Button("Clear Selected Objects"))
            selectedObjects.Clear();

        EditorGUILayout.Space();
        GUILayout.Label("Animation Settings", EditorStyles.boldLabel);
        defaultFolder = EditorGUILayout.ObjectField("Default Save Folder", defaultFolder, typeof(DefaultAsset), false) as DefaultAsset;
        animationName = EditorGUILayout.TextField("Animation Name", animationName);

        if (GUILayout.Button("Create Animation"))
        {
            CreateAnimation();
            selectedObjects.Clear();
        }
    }

    bool IsObjectAlreadySelected(GameObject obj)
    {
        return selectedObjects.Exists(data => data.gameObject == obj);
    }

    void CreateAnimation()
    {
        AnimationClip clip = new AnimationClip();
        Transform parentTransform = mainObject != null ? mainObject.transform : null;

        foreach (SelectedObjectData data in selectedObjects)
        {
            float startValue = data.isSelected ? 1 : 0;
            float endValue = data.isSelected ? 1 : 0;
            AnimationCurve curve = AnimationCurve.Linear(0, startValue, 1, endValue);

            EditorCurveBinding binding = new EditorCurveBinding();
            binding.path = AnimationUtility.CalculateTransformPath(data.gameObject.transform, parentTransform);
            binding.propertyName = "m_IsActive";

            clip.SetCurve(binding.path, typeof(GameObject), binding.propertyName, curve);

        }

        clip.frameRate = 1; clip.wrapMode = WrapMode.Once;

        string saveFolderPath = defaultFolder != null ? AssetDatabase.GetAssetPath(defaultFolder) : "Assets";

        string animationPath = System.IO.Path.Combine(saveFolderPath, animationName + ".anim");

        AssetDatabase.CreateAsset(clip, animationPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    class SelectedObjectData
    {
        public GameObject gameObject;
        public bool isSelected;

        public SelectedObjectData(GameObject gameObject, bool isSelected)
        {
            this.gameObject = gameObject;
            this.isSelected = isSelected;
        }
    }
}
