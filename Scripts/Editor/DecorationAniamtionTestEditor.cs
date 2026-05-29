using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ArtUtils;
namespace AboloLib
{
    [CustomEditor(typeof(DecorationAniamtionTest))]
    public class DecorationAniamtionTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            EditorGUILayout.LabelField("选择需要演示动画的节点序列范围");
            DecorationAniamtionTest decorationAniamtionTest = (DecorationAniamtionTest)target;
            float from = decorationAniamtionTest.PresentFrom;
            float to = decorationAniamtionTest.PresentTo;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 120f;
            EditorGUILayout.MinMaxSlider(new GUIContent("Decoration Range:"), ref from, ref to, 0, decorationAniamtionTest.PresentMax);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(30));
            from = EditorGUILayout.FloatField(from);
            to = EditorGUILayout.FloatField(to);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            decorationAniamtionTest.PresentFrom = (int)from;
            decorationAniamtionTest.PresentTo = (int)to;


            base.OnInspectorGUI();

            if (GUILayout.Button("一键解锁"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    decorationAniamtionTest.EditorModeUnlockImmediate();
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                });
            }
            if (GUILayout.Button("一键重置"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    decorationAniamtionTest.EditorModeResetImmediate();
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                });
            }

            if (GUILayout.Button("隐藏遮盖"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    decorationAniamtionTest.HideCovers();
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                });
            }

            if (GUILayout.Button("相机来！"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() =>
                {
                    Transform CameraRoot = GameObject.Find("CameraRoot").transform;
                    if (Selection.activeTransform != null)
                    {
                        if (Selection.activeObject != CameraRoot.gameObject)
                        {
                            CameraRoot.position = Selection.activeTransform.position;
                        }
                        else
                        {
                            Debug.LogWarning(ArtUtility.WarningLog + "当前选中的物体就是相机根节点，相机来不了");
                        }
                    }
                    else
                    {
                        Debug.LogWarning(ArtUtility.WarningLog + "当前没有选中任何物体，相机来不了");
                    }
                    Selection.activeObject = CameraRoot.gameObject;
                });
            }


            if (GUILayout.Button("检查全局Mesh是否引用有实例"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => decorationAniamtionTest.CheckMeshFilterGlitch());
            }

            if (GUILayout.Button("清除装修节点MeshCollider引用缓存Mesh"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => decorationAniamtionTest.ClearMeshColliderBuffer());
            }


            if (GUILayout.Button("检测装修节点命名"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => decorationAniamtionTest.CheckRepeatNodeName());
            }

            if (GUILayout.Button("装修场景整体缩放"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => decorationAniamtionTest.ScalingAllSubRoots());
            }

            if (GUILayout.Button("修改SpriteSortingPoint为Pivot"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => decorationAniamtionTest.SetSpriteRenderersSortingPointToPivot());
            }
            if (GUILayout.Button("获取场景挂点资源"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => decorationAniamtionTest.SetupBuildingList());
            }
            if (GUILayout.Button("打开装修场景Scene"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => decorationAniamtionTest.AddDecorationScene());
            }
        }
    }
    
}
