using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AboloLib
{
    public class ArtWorkSpaceEditor : Editor
    {
        [MenuItem("GameObject/ArtUtilsCreate/NewDecorationView", false, 10)]
        private static void CreateCustomNewDecorationNode(MenuCommand menuCommand)
        {
            //创建一个新的游戏物体
            GameObject go = new GameObject("NewDecorationNode");
            //menuCommand.context是当前鼠标左键选中的GameObjet游戏物体，通过GameObjectUtility.SetParentAndAlign函数设置为新创建的go物体的父节点
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);


            //创建一个新的游戏物体
            GameObject view = CreateCustomGameObject("view1" , go.transform);
            var _animator = view.AddComponent<Animator>();
            _animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>
                (
                    AboloConfig.DecorAnimatorPath+"ani_decoration_common_show/ani_common_show.controller"
                );
            var newFurnitureDecor = view.AddComponent<NewFurnitureDecorAnim>();
            if (newFurnitureDecor._myAudioName != AboloConfig.DecorAudioName_Done)
            {
                newFurnitureDecor._myAudioName = AboloConfig.DecorAudioName_Done;
                newFurnitureDecor._myAudioPlayDelay = 0.4f;
            }
            if (newFurnitureDecor.DoneFx == null)
            {
                newFurnitureDecor.DoneFx = AssetDatabase.LoadAssetAtPath<DecorationParticle>
                (
                    AboloConfig.DecorFxPath + "particle_decoration_done.prefab"
                );
            }

            GameObject root = CreateCustomGameObject("root" , view.transform);

            GameObject scale_items = CreateCustomGameObject("scale_items" , root.transform);

            CreateCustomGameObject("anim_items", root.transform);
            CreateCustomGameObject("pop_items", scale_items.transform);

            //注册到U3D的Undo系统中。就是指我们可以使用Ctrl+Z组合键对这个物体进行撤销操作。
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            //将鼠标的选中物体自动的移动到刚刚创建的go物体上
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/ArtUtilsCreate/ClearDecorationView", false, 10)]
        private static void CreateCustomClearDecorationNode(MenuCommand menuCommand)
        {
            //创建一个新的游戏物体
            GameObject go = new GameObject("ClearDecorationNode");
            //menuCommand.context是当前鼠标左键选中的GameObjet游戏物体，通过GameObjectUtility.SetParentAndAlign函数设置为新创建的go物体的父节点
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);


            //创建一个新的游戏物体
            GameObject view = CreateCustomGameObject("view1", go.transform);
            var _animator = view.AddComponent<Animator>();
            _animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>
                (
                    AboloConfig.DecorAnimatorPath+"ani_decoration_common_hide/ani_common_shrink.controller"
                );
            var clearDecorAnim = view.AddComponent<ClearDecorAnim>();
            if (clearDecorAnim._myAudioName != AboloConfig.DecorAudioName_Clean)
            {
                clearDecorAnim._myAudioName = AboloConfig.DecorAudioName_Clean;
                clearDecorAnim._myAudioPlayDelay = 0.1f;
            }

            if (clearDecorAnim.ClearFx == null)
            {
                clearDecorAnim.ClearFx = AssetDatabase.LoadAssetAtPath<GameObject>
                (
                    AboloConfig.DecorFxPath+AboloConfig.DecorFxName_Clean
                );
            }

            GameObject root = CreateCustomGameObject("root", view.transform);

            GameObject scale_items = CreateCustomGameObject("scale_items", root.transform);

            CreateCustomGameObject("anim_items", root.transform);
            CreateCustomGameObject("pop_items", scale_items.transform);

            //注册到U3D的Undo系统中。就是指我们可以使用Ctrl+Z组合键对这个物体进行撤销操作。
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name); 
            //将鼠标的选中物体自动的移动到刚刚创建的go物体上
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/ArtUtilsCreate/FixDecorationView", false, 10)]
        private static void CreateCustomFixDecorationNode(MenuCommand menuCommand)
        {
            //创建一个新的游戏物体
            GameObject go = new GameObject("FixDecorationNode");
            //menuCommand.context是当前鼠标左键选中的GameObjet游戏物体，通过GameObjectUtility.SetParentAndAlign函数设置为新创建的go物体的父节点
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            //创建一个新的游戏物体
            GameObject view = CreateCustomGameObject("view1", go.transform);
            var _animator = view.AddComponent<Animator>();
            _animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>
                (
                    AboloConfig.DecorAnimatorPath+"ani_decoration_common_hide/ani_common_shrink.controller"
                );
            var fixedDecor = view.AddComponent<FixedDecorAnim>();
            if (fixedDecor._myAudioName != AboloConfig.DecorAudioName_Fix)
            {
                fixedDecor._myAudioName = AboloConfig.DecorAudioName_Fix;
                fixedDecor._myAudioPlayDelay = 0.0f;
            }

            if (fixedDecor.FixFx == null)
            {
                fixedDecor.ClearFx = AssetDatabase.LoadAssetAtPath<GameObject>
                (
                    AboloConfig.DecorFxPath+AboloConfig.DecorFxName_Cover_S
                );
            }

            if (fixedDecor.FixFxStopDelay == 0f)
            {
                fixedDecor.FixFxStopDelay = 2.0f;
            }

            GameObject root = CreateCustomGameObject("root", view.transform);

            GameObject scale_items = CreateCustomGameObject("scale_items", root.transform);

            CreateCustomGameObject("anim_items", root.transform);
            CreateCustomGameObject("pop_items", scale_items.transform);

            //注册到U3D的Undo系统中。就是指我们可以使用Ctrl+Z组合键对这个物体进行撤销操作。
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            //将鼠标的选中物体自动的移动到刚刚创建的go物体上
            Selection.activeObject = go;
        }

        public static void FunctionEditorOnly(System.Action action)
        {
            if (!Application.isPlaying)
            {
                action.Invoke();
            }
            else
            {
                Debug.LogWarning("此功能仅限编辑模式下使用！！！");
            }
        }

        static GameObject CreateCustomGameObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.layer = parent.gameObject.layer;
            return go;
        }

    }

    [CustomEditor(typeof(FlyFxItemCtrl))]
    public class FlyFxItemCtrlEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();
            FlyFxItemCtrl fly_ctrl = (FlyFxItemCtrl)target;
            if (GUILayout.Button("发射飞行道具"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => fly_ctrl.ShootFlyItem());
            }
        }
    }

    [CustomPropertyDrawer(typeof(PrefabSelectAttribute))]
    public class PrefabSelectAttributeEditor : PropertyDrawer
    {
        string[] GetNames(PrefabSelectAttribute attribute)
        {
            var itemfactory = AssetDatabase.LoadAssetAtPath<MyObjectFactory>(attribute.AssetPath);
            int index = itemfactory.prefabs.Length;
            string[] names = new string[index];
            for (int i = 0; i < index; i++)
            {
                names[i] = i.ToString() + "|" + itemfactory.prefabs[i].name;
            }
            return names;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PrefabSelectAttribute prefabSelect = attribute as PrefabSelectAttribute;
            string[] static_names = GetNames(prefabSelect);
            string cntString = property.stringValue;
            int selected = 0;

            for (int i = 1; i < static_names.Length; ++i)
            {
                if (static_names[i].Equals(cntString))
                {
                    selected = i;
                    break;
                }
            }
            selected = EditorGUI.Popup(position, label.text, selected, static_names);

            if (GUI.changed)
            {
                var name = static_names[selected];
                property.stringValue = name;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }
    }



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

    [CustomEditor(typeof(ConvertToImage))]
    [CanEditMultipleObjects]
    public class ConvertToImageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ConvertToImage toImage = (ConvertToImage)target;
            if (GUILayout.Button("Sprite转换Image"))
            {
                toImage.ConvertSpriteRenererToImage();
                EditorUtility.SetDirty(toImage.gameObject);
            }
        }
    }

    [CustomEditor(typeof(FxSlider))]
    [CanEditMultipleObjects]
    public class FxSliderEditor : UnityEditor.UI.SliderEditor
    {
        SerializedProperty hideFillOnLowValue;
        SerializedProperty parentSlider;
        protected override void OnEnable()
        {
            base.OnEnable();
            hideFillOnLowValue = serializedObject.FindProperty("hideFillOnLowValue");
            parentSlider = serializedObject.FindProperty("parentSlider");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(hideFillOnLowValue);
            EditorGUILayout.PropertyField(parentSlider);
            serializedObject.ApplyModifiedProperties();
            FxSlider fxSlider = (FxSlider)target;
        }
    }

    [CustomEditor(typeof(MyCellsManager))]
    public class MyCellsManagerEditor : Editor
    {
       public override void OnInspectorGUI()
       {
           base.OnInspectorGUI();
           MyCellsManager drawGrid = (MyCellsManager)target;

           if (GUILayout.Button("生成MergeCell网格"))
           {
               drawGrid.DrawCellsForWork();
           }
           if (GUILayout.Button("清除当前网格"))
           {
               ArtUtility.ClearChildGameObjects(drawGrid.transform.Find("root/cells"));
           }
       }
    }




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

            if (GUILayout.Button("填充指定根节点下的装修挂点参数"))
            {
                ArtWorkSpaceEditor.FunctionEditorOnly(() => decorationAniamtionTest.FillupDecorParameter());
            }

        }
    }


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

    #region 装修动画自定义编辑器面板

    [CustomEditor(typeof(DecorationAnim))]
    public class DecorationAnimEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DecorationAnim decorationAnim = (DecorationAnim)target;
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
        }
    }




    [CustomEditor(typeof(ClearDecorAnim))]
    public class ClearDecorAnimEditor : DecorationAnimEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ClearDecorAnim clearAnim = (ClearDecorAnim)target;
        }
    }

    [CustomEditor(typeof(FixedDecorAnim))]
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
    #endregion
}