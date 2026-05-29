using UnityEngine;
using UnityEditor;
using ArtUtils;

namespace AboloLib
{
    [CustomEditor(typeof(ComplexGrowNewDecoration))]
    [CanEditMultipleObjects]
    public class ComplexGrowNewDecorationEditor : DecorationAnimEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ComplexGrowNewDecoration complexGrow = (ComplexGrowNewDecoration)target;
            if (GUILayout.Button("添加子节点动画标记"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    complexGrow.AddTypeInfoToRenderers();
                    EditorUtility.SetDirty(complexGrow.gameObject);
                });
            }
            if (GUILayout.Button("清除子节点动画标记"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    complexGrow.RemoveTypeInfoOfRenderers();
                    EditorUtility.SetDirty(complexGrow.gameObject);
                });
            }
            if (GUILayout.Button("生长动画组子节点反序"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    complexGrow.ReverseAnimRootRenderersQueue();
                    EditorUtility.SetDirty(complexGrow.gameObject);
                });
            }
        }
    }
}
