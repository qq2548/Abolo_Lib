using UnityEditor;
using UnityEngine;
using ArtUtils;

namespace AboloLib
{

    [CustomEditor(typeof(SetLookAt))]
    [CanEditMultipleObjects]
    public class SetLookAtEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SetLookAt setLookAt = (SetLookAt)target;
            if (GUILayout.Button("所有子节点Sprite面向相机"))
            {
                setLookAt.ChildrenSpriteLookAtCamera();
                EditorUtility.SetDirty(setLookAt.gameObject);
            }
            if (GUILayout.Button("自身transform面向相机"))
            {
                setLookAt.LookAtTarget();
                EditorUtility.SetDirty(setLookAt.gameObject);
            }
        }
    }
}
