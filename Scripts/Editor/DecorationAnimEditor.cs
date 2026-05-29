#define USE_3D_MAP
using UnityEngine;
using UnityEditor;
using ArtUtils;

namespace AboloLib
{
    [CustomEditor(typeof(DecorationAnim))]
    public class DecorationAnimEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DecorationAnim decorationAnim = (DecorationAnim)target;
#if USE_3D_MAP
            if (GUILayout.Button("所有Sprite子节点Z轴随机偏移"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    decorationAnim.RandomOffsetZaxis();
                    EditorUtility.SetDirty(decorationAnim.gameObject);
                });
            }
            if (GUILayout.Button("所有Sprite子节点Z轴随SortingOder值偏移"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    decorationAnim.OffsetZaxisBySortingOrder();
                    EditorUtility.SetDirty(decorationAnim.gameObject);
                });

            }
            if (GUILayout.Button("所有Sprite子节点Z轴归零"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    decorationAnim.ResetZaxis();
                    EditorUtility.SetDirty(decorationAnim.gameObject);
                });
            }
#endif
            if(GUILayout.Button("设置SortingPoint"))
            {
                decorationAnim.SetSortingPoint();
                EditorUtility.SetDirty(decorationAnim.gameObject);
            }
        }
    }

    [CustomEditor(typeof(ClearDecorAnim))]
    [CanEditMultipleObjects]
    public class ClearDecorAnimEditor : DecorationAnimEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ClearDecorAnim clearAnim = (ClearDecorAnim)target;
        }
    }

    [CustomEditor(typeof(FixedDecorAnim))]
    [CanEditMultipleObjects]
    public class FixedDecorAnimEditor : DecorationAnimEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            FixedDecorAnim fixAnim = (FixedDecorAnim)target;
        }
    }

    [CustomEditor(typeof(NewFurnitureDecorAnim))]
    [CanEditMultipleObjects]
    public class NewFurnitureDecorAnimEditor : DecorationAnimEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            NewFurnitureDecorAnim decorationAnim = (NewFurnitureDecorAnim)target;
        }
    }
}
