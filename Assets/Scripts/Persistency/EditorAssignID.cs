#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(HouseObject))]
public class HouseObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        HouseObject h = (HouseObject)target;


        if (GUILayout.Button("Assign GUID (Editor only)"))
        {
            if (string.IsNullOrEmpty(h.uniqueID))
            {
                h.uniqueID = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(h);
            }
            else
            {
                if (EditorUtility.DisplayDialog("Overwrite ID?", "This object already has an ID. Overwrite?", "Yes", "No"))
                {
                    h.uniqueID = System.Guid.NewGuid().ToString();
                    EditorUtility.SetDirty(h);
                }
            }
        }


        if (GUILayout.Button("Validate All IDs in Scene"))
        {
            var objs = GameObject.FindObjectsByType<HouseObject>(FindObjectsSortMode.None);
            var used = new System.Collections.Generic.HashSet<string>();
            foreach (var o in objs)
            {
                if (string.IsNullOrEmpty(o.uniqueID))
                    Debug.LogError($"Missing ID on {o.name}");
                else if (used.Contains(o.uniqueID))
                    Debug.LogError($"Duplicate ID {o.uniqueID} on {o.name}");
                else used.Add(o.uniqueID);
            }
        }
    }
}
#endif