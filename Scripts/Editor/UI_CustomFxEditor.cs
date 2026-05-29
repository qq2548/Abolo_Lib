using UnityEngine;
using UnityEditor;
using ArtUtils;

namespace AboloLib
{
    [CustomEditor(typeof(UI_CustomFx))]
    public class UI_CustomFxEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UI_CustomFx ui_customFx = (UI_CustomFx)target;
            if (GUILayout.Button("获取子节点动画"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    ui_customFx.GetSubAnimations();
                    EditorUtility.SetDirty(ui_customFx.gameObject);
                });
            }
            if (GUILayout.Button("清除子节点动画"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    ui_customFx.ClearSubAnimations();
                    EditorUtility.SetDirty(ui_customFx.gameObject);
                });
            }
        }
    }
}
