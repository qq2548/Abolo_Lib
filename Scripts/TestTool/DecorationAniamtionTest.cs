
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Spine.Unity;

namespace AboloLib
{
    public class DecorationAniamtionTest : ArtAnimation
    {
        public static string MyDecorationSceneName = "Bloom_DecorScene_Art";
        public static DecorationAniamtionTest instance;

        [Header("如果要自由组合节点演示，勾选此项")]
        [SerializeField] bool _playCustomNodesIndexes = false;
        [Header("手动输入自由组合演示的节点序号")]
        [SerializeField] int[] _customNodeIndexes;

        [Header("播放解锁动画，节点之间的间隔时间")]
        [SerializeField] float delay = 1.0f;
        [Header("所有装修节点")]
        [SerializeField] List<Transform> _nodes;
        [Header("房顶遮挡类节点")]
        [SerializeField] List<Transform> _nodes_cover;

        //装修动画数组
        List<IDecorAnimCtrl> decorAnimCtrls;
        [Tooltip("使用这个编号来选择需要演示哪个节点的选中动画")]
        [SerializeField] int selectedIndex;
        [Tooltip("演示选中动画效果的UI开关")]
        [SerializeField] Toggle selectToggle;

        [Header("需要编辑的根节点")]
        [SerializeField] Transform _modifyRoot;

        [SerializeField] int TargetChapterIndex = 0;
        [SerializeField] PrefabFactory _buildingList;
        Dictionary<string , GameObject> GameBuildingDic;
        List<string> buildingData;
        List<Vector3> buildingPosition;

        [SerializeField]bool _useDynamicInstantiateMode = false;
        [Header("装修动画播放速度")]
        [SerializeField] float decorAnimPlaySpeed = 1.0f;
        int _presentMax = 160;
        public int PresentMax
        {
            get
            {
                return _presentMax;       
            }
        }
        int _presentFromIndex = 0;
        public int PresentFrom
        {
            get
            {
                return _presentFromIndex;
            }
            set
            {
                _presentFromIndex = value;
            }
        }
        int _presentToIndex = 9999;
        public int PresentTo
        {
            get
            {
                return _presentToIndex;
            }
            set
            {
                if (value <_presentMax)
                {
                    _presentToIndex = value;
                }
                else
                {
                    _presentToIndex = _presentMax;
                }
            }
        }


        private void Awake()
        {
            DecorationAnim.SPEED = decorAnimPlaySpeed;
            instance = this;

            if (SceneManager.sceneCount > 1)
            {
                var DecorScene = SceneManager.GetSceneByName("Bloom_DecorScene_Art");
                if (DecorScene != null)
                {
  
                    StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(0.05f , () => Initialize()));                  
                }
            }
            else
            {
                StartCoroutine(LoadDecorationScene(() => Initialize()));
            }

            if(selectToggle != null) selectToggle.onValueChanged.AddListener((value) => SelectAnim(value));

            StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(0.1f , () => RenameNodesOnPlay()));
            //Material material = GetComponent<Renderer>().material;
            //material.shader.name
        }


        public void SetupBuildingList()
        {
#if UNITY_EDITOR
            if(_buildingList == null) return;
            if(_buildingList.MyPrefabs == null)
            {
                _buildingList.MyPrefabs = new List<GameObjectPreSet>();
            }
            _buildingList.MyPrefabs.Clear();
            var rootFolder = "Assets/Buildings";
            var rs = AssetDatabase.FindAssets("t:prefab" + " " , new string[] { rootFolder });
            foreach (var guid in rs)
            {
                var asset_path = AssetDatabase.GUIDToAssetPath(guid);
                var ass = AssetDatabase.LoadAssetAtPath<GameObject>(asset_path);
                if(ass.transform.TryGetComponent(out DecorationAnim component))
                {
                    _buildingList.MyPrefabs.Add(new GameObjectPreSet(ass.name , ass));
                }
            }
            EditorUtility.SetDirty(_buildingList);
#endif
        }
        


        /// <summary>
        /// 重装或者活动装修场景选中效果的动效演示
        /// </summary>
        /// <param name="selected"></param>
        public void SelectAnim(bool selected)
        {
#if _ARTEST_PRESENTATION

            if (selectedIndex < NewFurnitureDecorAnim.Instances.Count && selectedIndex >= 0)
            {
                NewFurnitureDecorAnim.Instances[selectedIndex].Selecting = selected;
                StartCoroutine(MoveCameraRoot(ArtGameManager.instance.MainCameraRoot,
                                                                   NewFurnitureDecorAnim.Instances[selectedIndex].transform.position, 16f, 0.001f));
            }
            else
            {
                Debug.LogError("The giving index is out of range!!!! , current NewFurnitureDecorAnim count is " + NewFurnitureDecorAnim.Instances.Count);
            }
#endif
        }



        static string CoverNodePath = "Decorations/Area_Roof/Nodes";
        /// <summary>
        /// 不同区域的根节点路径
        /// </summary>
        static string[] DecorationNodesPath =
        {
            //"Decorations/Area1_Demo/Nodes",
            "Decorations/Area_Chapter_001/Nodes",
            "Decorations/Area_Chapter_002/Nodes",
            "Decorations/Area_Chapter_003/Nodes",
            // "Decorations/Area_PlayGround/Nodes",
            // "Decorations/Area_Kitchen/Nodes",
            // "Decorations/Area_Activity/Nodes",
            // "Decorations/Area_DuelFloorRestrant/Nodes",
            // "Decorations/Area_EntertainingHouse/Nodes",
            // "Decorations/Area_FlowerHouse/Nodes",
            // "Decorations/Area_CollectionHouse/Nodes",
            // "Decorations/Area_Activity_V2/Nodes",
            // "Decorations/Area_PlayGround_DLC_1/Nodes",
            // "Decorations/Area_PlayGround_DLC_2/Nodes",
            // "Decorations/Area_Market/Nodes",
            // "Decorations/Area_Gym/Nodes",
        };
     
        public void  Initialize()
        {
            //
            var DecorScene = SceneManager.GetSceneByName("Bloom_DecorScene_Art");
            SceneManager.SetActiveScene(DecorScene);
            GameObject EnvironmentRoot = DecorScene.GetRootGameObjects().FirstOrDefault(s => s.name == "EnvironmentRoot");
          
            if(Application.isPlaying)
            {
                if(_buildingList != null)
                {
                    GameBuildingDic = new Dictionary<string, GameObject>();
                    foreach(var building in _buildingList.MyPrefabs)
                    {
                        if(building.name.Contains($"cpt_{TargetChapterIndex}_")) 
                            GameBuildingDic.Add(building.name, building.prefab);
                    }
                }
                buildingData = new List<string>(); 
                buildingData.Clear();
                buildingPosition = new List<Vector3>();
                buildingPosition.Clear();
                if (_nodes == null) _nodes = new List<Transform>();
                _nodes.Clear();

                var decor_static_root = EnvironmentRoot.transform.Find("Props/root");
                for (int i = 0; i < decor_static_root.childCount; i++)
                {
                    decor_static_root.GetChild(i).gameObject.SetActive(i+1 == TargetChapterIndex);
                }

                var decor_root = EnvironmentRoot.transform.Find("Decorations");
                for (int i = 0; i < decor_root.childCount; i++)
                {
                    decor_root.GetChild(i).gameObject.SetActive(i+1 == TargetChapterIndex);
                }
                string nodeName = $"Area_Chapter_{TargetChapterIndex:D3}/Nodes";
                Debug.Log(nodeName + "========================================");
                var derNode = decor_root.Find(nodeName);
                if(derNode != null)
                {
                    for (int i = 0; i < derNode.childCount; i++)
                    {
                        buildingData.Add(derNode.GetChild(i).name);
                        buildingPosition.Add(derNode.GetChild(i).position);
                        //Debug.Log(derNode.GetChild(i).name);
                    }
                }
                int index = derNode.childCount;
                for (int i = 0; i < index; i++)
                {
                    GameObject.DestroyImmediate(derNode.GetChild(0).gameObject);
                }
                //加载资源
                for (int i = 0; i < buildingData.Count; i++)
                {
                    var ob = Instantiate(GameBuildingDic.FirstOrDefault(x => x.Key.Contains(buildingData[i])).Value);
                    var parent = new GameObject($"{i}_{buildingData[i]}");
                    parent.transform.SetParent(EnvironmentRoot.transform);
                    parent.transform.position = buildingPosition[i];
                    ob.transform.position = buildingPosition[i];
                    ob.name = ob.name.Replace("(Clone)","");
                    ob.transform.SetParent(parent.transform);
                    _nodes.Add(parent.transform);
                    ob.GetComponent<DecorationAnim>().SetUp();
                }
            }
            else
            {
                GetDecorationNodes(EnvironmentRoot);
            }


            GetCoverNodes(EnvironmentRoot);

            //

            // 运行后调整节点最大值不要超过实际节点数
            _presentMax = _nodes.Count -1;
            if (_presentToIndex >= _nodes.Count)
            {
                _presentToIndex = _presentMax;
            }

        }

        IEnumerator SetTargetSceneActicve(UnityEngine.SceneManagement.Scene scene)
        {
            while(!scene.isLoaded)
            {
                yield return null;
            }
            SceneManager.SetActiveScene(scene);
        }

        /// <summary>
        /// 获取场景里的遮盖父节点，保存到_nodes_cover数组
        /// </summary>
        /// <param name="RootGo">可装修场景根节点</param>
        void GetCoverNodes(GameObject RootGo)
        {

            if (_nodes_cover == null)
            {
                _nodes_cover = new List<Transform>();
            }

            _nodes_cover.Clear();
            Transform nodes_cover_root = RootGo.transform.Find(CoverNodePath);
            if(nodes_cover_root != null)
            {
                for (int c = 0; c < nodes_cover_root.childCount; c++)
                {
                    _nodes_cover.Add(nodes_cover_root.GetChild(c));
                }
            }
        }
        /// <summary>
        /// 获取场景里的装修父节点，保存到_nodes数组
        /// </summary>
        /// <param name="RootGo">可装修场景根节点</param>
        void GetDecorationNodes(GameObject RootGo)
        {

            if (_nodes == null)
            {
                _nodes = new List<Transform>();
            }

            _nodes.Clear();
            decorAnimCtrls = new List<IDecorAnimCtrl>();
            decorAnimCtrls.Clear();

            for (int n = 0; n < DecorationNodesPath.Length; n++)
            {
                Transform nodes_root = RootGo.transform.Find(DecorationNodesPath[n]);
                if (nodes_root != null)
                {
                    for (int i = 0; i < nodes_root.childCount; i++)
                    {
                        var node = nodes_root.GetChild(i);
                        _nodes.Add(node);  
                    }
                }
                else
                {
                    Debug.LogWarning("没有找到节点：" + DecorationNodesPath[n] );
                }
            }
            //70号位插入房顶节点
            //_nodes.Insert(70, _nodes_cover[0]);
            //_nodes.Insert(473, _nodes_cover[1]);
            //_nodes.Insert(661, _nodes_cover[2]);
        }

        /// <summary>
        /// 依次播放引用的装修节点动画
        /// </summary>
        public void Unlock()
        {
            StopAnimation();
            if (_nodes == null || _nodes.Count == 0) return;
            //ResetImmediateByRange();
            foreach (var item in _nodes)
            {
               var animators = item.GetComponentsInChildren<Animator>(true);
                for (int i = 0; i < animators.Length; i++)
                {
                    CheckEnableAnimator(animators[i]);
                    if (animators[i].isActiveAndEnabled)
                    {
                        if (animators[i].transform.parent.name.Contains("activity") && animators[i].gameObject.name == "view1")
                        {

                            animators[i].Play("Idle", 0);

                        }
                        else
                        {

                            animators[i].Play("Init", 0);

                        }
                    }
                }
            }
             ani = StartCoroutine(QueueUnlockAnimation());
        }

        /// <summary>
        /// 引用的所有装修节点解锁，用于验证装修动画的不同状态是否表现合理
        /// </summary>
        public void UnlockImmediate()
        {
            StopAnimation();

            if(_nodes_cover !=null && _nodes_cover.Count > 0)
            {
                //解锁房顶节点
                for (int c = 0; c < _nodes_cover.Count; c++)
                {
                    Animator[] animators = _nodes_cover[c].GetComponentsInChildren<Animator>(true);
                    foreach (var item in animators)
                    {
                        item.gameObject.SetActive(false);
                    }
                }
            }

            if (_nodes!= null && _nodes.Count > 0)
            {
               //解锁装修节点
                for (int i = 0; i < _nodes.Count; i++)
                {
                    Animator animator = GetDecorationNode(_nodes[i]);
                    if (!animator.gameObject.activeSelf)
                        {   
                            animator.gameObject.SetActive(true);
                        }
                    CheckEnableAnimator(animator);

                    animator.Play("Idle",0);
                    //DisableAnimatorWhenClipIsDone( animator);
                }
            }
         }

        [Space(20)]
        [Header("是否启用随机解锁模式，三选一随机一个选项解锁")]
        [SerializeField] bool _randomUnlock = true;
        [Header("装修解锁演示的节点序号")]
        [Tooltip("由于活动装修场景第一套外观默认解锁，序号0的时候使用一键解锁，活动装修场景不会有任何变化")]
        [SerializeField] int _unlockIndex;
        public int UnlockIndex
        {
            get => _unlockIndex;
            set => _unlockIndex = value;
        }
        /// <summary>
        /// 当前装修场景有多少套可选装修
        /// </summary>
        const int _maxActicityDecorCount_Old = 7;
        const int _maxActicityDecorCount_New = 7;

        void RemoveEditScript(GameObject go)
        {
            SetLookAt[] setLookats = go.GetComponentsInChildren<SetLookAt>(true);
            if (setLookats != null && setLookats.Length > 0)
            {
                foreach (var item in setLookats)
                {
                    DestroyImmediate(item);
                }
            }
        }
        /// <summary>
        /// 影藏场景里的屋顶遮盖
        /// </summary>
        public void HideCovers()
        {
            //获取场景数据
            Initialize();
            //解锁房顶节点
            for (int c = 0; c < _nodes_cover.Count; c++)
            {
                Animator[] animators = _nodes_cover[c].GetComponentsInChildren<Animator>(true);
                foreach (var item in animators)
                {
                    item.gameObject.SetActive(false);
                }
            }
            _nodes_cover.Clear();
            _nodes.Clear();
        }

        /// <summary>
        /// 检查多选一节点是否缺失碰撞体，并修复 BoxCollider Size值为负数时的编辑器警告
        /// </summary>
        /// <param name="node"></param>
        void CheckSelectCollider(Transform node)
        {
            if (node.childCount > 1 || node.name.Contains("activity"))
            {
                for (int i = 0; i < node.childCount; i++)
                {
                    var colliders = node.GetChild(i).GetComponentsInChildren<Collider>(true);
                    if (colliders == null || colliders.Length == 0)
                    {
                        Debug.LogError(node.name + "view"+ (i+1).ToString() + " 此三选一节点缺少碰撞体");
                    }
                    else
                    {
                        //BoxCollider 在世界坐标矩阵下的scale或者size不能为负数，编辑器会报警告信息，这里在编辑器解锁的时候遍历修复一下
                        for (int a = 0; a < colliders.Length; a++)
                        {
                            if (colliders[a].GetType().Name == "BoxCollider")
                            {
                                var boxcollider = colliders[a] as BoxCollider;
                                Vector3 size = boxcollider.size;
                                if (size.x * colliders[a].transform.lossyScale.x < 0f)
                                {
                                    boxcollider.size = new Vector3(-size.x, size.y, size.z);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void EditorModeUnlockImmediate()
        {
            int index ;
            Initialize();

            //解锁房顶节点
            for (int c = 0; c < _nodes_cover.Count; c++)
            {
                Animator[] animators = _nodes_cover[c].GetComponentsInChildren<Animator>(true);
                foreach (var item in animators)
                {
                    item.gameObject.SetActive(false);
                }
            }
            _nodes_cover.Clear();

            //解锁装修节点
            for (int i = 0; i < _nodes.Count; i++)
            {
                Debug.Log($"当前序号是：____{i}");
                //检测和修复三选一碰撞体
                CheckSelectCollider(_nodes[i]);
                var anis = GetComponentsInFirstLayerChildren(_nodes[i]);

                if (_unlockIndex > anis.Length - 1)
                {
                    index = anis.Length - 1;
                    //Debug.LogWarning("超出限定范围");
                }
                else
                {
                    if (!_randomUnlock)
                    {
                        index = _unlockIndex;
                    }
                    else
                    {
                        index = UnityEngine.Random.Range(0, anis.Length);
                    }
                }

                if (!_nodes[i].name.Contains("activity"))
                {
                    for (int a = 0; a < anis.Length; a++)
                    {

                        //检测删除SetLookAt脚本
                        RemoveEditScript(anis[a].gameObject);

                        if (a == index)
                        {
                            Animator animator = anis[a];
                            var decor = animator.transform.GetComponent<DecorationAnim>();

                            if (decor != null)
                            {   
                                var sps = decor.transform.Find("root/spine_items").GetComponentsInChildren<SkeletonAnimation>(true);
                                if (decor.GetType().Name.Equals("NewFurnitureDecorAnim") || decor.GetType().Name.Equals("ComplexGrowNewDecoration"))
                                {
                                    if (!animator.gameObject.activeSelf)
                                    {
                                        animator.gameObject.SetActive(true);
                                    }
                                    
                                }
                                else
                                {
                                    animator.gameObject.SetActive(false);
                                }
                                                            
                                foreach (var s in sps)
                                {
                                    SetEditorModeSpineAniamtion("ani_idle" , s ); 
                                }

                                
                            }
                            else
                            {
                                Debug.LogWarning(ArtUtility.WarningLog + anis[a].transform.parent.name +"缺少装修脚本，检查场景挂点资源");
                            }

                        } 
                        else
                        {
                            anis[a].gameObject.SetActive(false); 
                        }
                    }
                }
                else
                {

                    for (int b = 0; b < _maxActicityDecorCount_Old; b++)
                    {
                        if (b == index)
                        {
                            anis[b].gameObject.SetActive(true);
                        }
                        else
                        {
                            if (anis.Length >= _maxActicityDecorCount_Old)
                            {
                                anis[b].gameObject.SetActive(false);
                            }
                            else
                            {
                                if (anis.Length > b)
                                {
                                    anis[b].gameObject.SetActive(false);
                                }
                                else
                                {
                                    Debug.LogWarning($"{_nodes[i].transform.gameObject.name}_view{b+1}没有多余动画状态机@");
                                }
                            }
                        }
                    }

                }
 
            }
            
            _nodes.Clear(); 
    
        }
        public void EditorModeResetImmediate()
        {
            Initialize();
            
            //重置房顶节点
            for (int c = 0; c < _nodes_cover.Count; c++)
            {
                Animator[] animators = _nodes_cover[c].GetComponentsInChildren<Animator>(true);
                foreach (var item in animators)
                {
                    item.gameObject.SetActive(true);
                }
            }
            _nodes_cover.Clear();

            //重置装修节点
            for (int i = 0; i < _nodes.Count; i++)
            {
                Animator[] animators = _nodes[i].GetComponentsInChildren<Animator>(true);
                foreach (var item in animators)
                {
                    //检测删除SetLookAt脚本
                    RemoveEditScript(item.gameObject);
                    var sps = item.transform.Find("root/spine_items").GetComponentsInChildren<SkeletonAnimation>(true);
                    if (item.transform.TryGetComponent(out NewFurnitureDecorAnim decAnim))
                    {
                        if (item.transform.parent.name.Contains("activity") && item.gameObject.name == "view1")
                        {
                            item.gameObject.SetActive(true);
                        }
                        else
                        {
                            if (item.gameObject.activeSelf)
                            {
 
                                item.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        item.gameObject.SetActive(true);
                    }
                    foreach(var s in sps)
                    { 
                        SetEditorModeSpineAniamtion("ani_init" , s);
                    }
                }
            }
            _nodes.Clear();
        }


        void RenameNodesOnPlay()
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                string name = _nodes[i].name;
                _nodes[i].name = i.ToString() + "_" + name;
            }
        }


        /// <summary>
        /// 引用的所有装修节点重置为未解锁，用于验证装修动画的不同状态是否表现合理
        /// </summary>
        public void ResetImmediate()
        {
            StopAnimation();
            //if (_nodes_cover == null || _nodes_cover.Count == 0) return;
            //重置房顶节点
            if(_nodes_cover != null && _nodes_cover.Count > 0)
            {
                for (int c = 0; c < _nodes_cover.Count; c++)
                {
                    Animator[] animators = _nodes_cover[c].GetComponentsInChildren<Animator>(true);
                    foreach (var item in animators)
                    {
                        item.gameObject.SetActive(true);
                    }
                } 
            }
            //if (_nodes == null || _nodes.Count == 0) return;
            //重置装修节点
            if(_nodes != null && _nodes.Count > 0)
            {
                for (int i = 0; i < _nodes.Count; i++)
                {
                    Animator[] animators = _nodes[i].GetComponentsInChildren<Animator>(true);
                    foreach (var item in animators)
                    {
                        if (item.transform.TryGetComponent(out NewFurnitureDecorAnim decAnim))
                        {
                            decAnim.SetUp();
                            if (item.transform.parent.name.Contains("activity") && item.gameObject.name == "view1")
                            {
                                item.gameObject.SetActive(true);
                                CheckEnableAnimator(item);
                                //decAnim.ShowSelf();
                                item.Play("Idle", 0);
                                DisableAnimatorWhenClipIsDone(item);
                            }
                            else
                            {
                                if (item.gameObject.activeSelf)
                                {
                                    decAnim.ResetSubItems(factor: 0f);
                                    item.gameObject.SetActive(false);
                                }
                            }
                        }
                        else
                        {
                            item.gameObject.SetActive(true);
                            CheckEnableAnimator(item);
                            item.Play("Init", 0);
                            DisableAnimatorWhenClipIsDone(item);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// 引用的所有装修节点重置为未解锁，用于验证装修动画的不同状态是否表现合理
        /// </summary>
        public void ResetImmediateByRange()
        {
            //重置房顶节点
            for (int c = 0; c < _nodes_cover.Count; c++)
            {
                Animator[] animators = _nodes_cover[c].GetComponentsInChildren<Animator>(true);
                foreach (var item in animators)
                {
                    item.gameObject.SetActive(false);
                }
            }

            //重置装修节点
            for (int i = 0; i < _nodes.Count; i++)
            {
                Animator[] animators = _nodes[i].GetComponentsInChildren<Animator>(true);
                foreach (var item in animators)
                {
                    if (item.transform.TryGetComponent(out NewFurnitureDecorAnim decAnim))
                    {
                        decAnim.SetUp();
                        if (item.transform.parent.name.Contains("activity") && item.gameObject.name == "view1")
                        {
                            item.gameObject.SetActive(true);
                            CheckEnableAnimator(item);
                            //decAnim.ShowSelf();
                            item.Play("Idle", 0);
                            DisableAnimatorWhenClipIsDone(item);
                        }
                        else
                        {
                            if (item.gameObject.activeSelf)
                            {
                                decAnim.ResetSubItems(factor: 0f);
                                item.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        if (i >= PresentFrom && i <= PresentTo)
                        {
                            item.gameObject.SetActive(true);
                            CheckEnableAnimator(item);
                            item.Play("Init", 0);
                            DisableAnimatorWhenClipIsDone(item);
                        }
                        else
                        {
                            item.gameObject.SetActive(true);
                            CheckEnableAnimator(item);
                            item.Play("Idle", 0);
                            DisableAnimatorWhenClipIsDone(item);
                        }
                    }

                }
            }
        }

        Animator[] GetComponentsInFirstLayerChildren(Transform _transform)
        {
            Animator[] anis = new Animator[_transform.childCount];
            for (int i = 0; i < _transform.childCount; i++)
            {
                anis[i] = _transform.GetChild(i).GetComponent<Animator>();
            }
            return anis;
        }

        Animator GetDecorationNode(Transform input_transform)
        {
            //input_transform.c
            var anis = GetComponentsInFirstLayerChildren(input_transform);//input_transform.GetComponentsInChildren<Animator>(true);
            int index;
            if (!input_transform.name.Contains("activity"))
            {
                if (_randomUnlock)
                {
                    index = /*ant.Length > 1 ? 1 : 0;//*/UnityEngine.Random.Range(0, anis.Length);
                }
                else
                {
                    index = anis.Length > _unlockIndex ? _unlockIndex : anis.Length -1;
                }
                
            }
            else
            {
                //活动装修场景目前只做了废旧和活动节点，固定解锁第2位节点
                //index = 1;
                ///临时代码直接把上面的复制过来，活动胡装修已经达到3套，不需要做区分判断了
                if (_randomUnlock)
                {
                    index = /*ant.Length > 1 ? 1 : 0;//*/UnityEngine.Random.Range(0, anis.Length);
                }
                else
                {
                    index = anis.Length > _unlockIndex ? _unlockIndex : anis.Length - 1;
                    //Debug.Log(index);
                }
            }
#if _ARTEST_PRESENTATION
            Debug.Log(input_transform.name + "-------------------"+index + "号位装修点解锁");
#endif
            for (int i = 0; i < anis.Length; i++)
            {
                if (i != index)
                {
                    anis[i].gameObject.SetActive(false);
                }
            }
            return anis[index];
        }

        void CheckViewLocalPosition()
        {

        }

        /// <summary>
        /// 动画播放完毕后关闭动画状态机
        /// </summary>
        /// <param name="animator"></param>
        void DisableAnimatorWhenClipIsDone(Animator animator)
        {
            if (animator.TryGetComponent(out DecorationAnim decoration))
            {
                float delay = decoration.MyUnlockAnimDuration + 1.0f;
                StartCoroutine(ArtAnimDelayCoroutine(delay, () => { animator.enabled = false; }));
            }
        }

        void CheckEnableAnimator(Animator animator)
        {
            if (!animator.enabled)
            {
                animator.enabled = true;
            }
        }

        IEnumerator QueueUnlockAnimation()
        {
            if (!_playCustomNodesIndexes)
            {
                for (int i = _presentFromIndex; i < _presentToIndex +1; i++)
                {
                    yield return StartCoroutine(QueuedUnlock(i));
                }
            }
            else
            {
                for (int i = 0; i < _customNodeIndexes.Length; i++)
                {
                    yield return StartCoroutine(QueuedUnlock(_customNodeIndexes[i]));
                }
            }
            //演示播放完毕后移动摄像机到远点位置
            //yield return StartCoroutine(MoveCameraRoot(ArtGameManager.instance.MainCameraRoot, 
            //    new Vector3(-5.0f,0.0f,-9.0f), 24f ,1.0f));
        }



        IEnumerator QueuedUnlock(int index)
        {
            //if (index == 83)
            //{
            //    _nodes_cover[0].Find("view1").GetComponent<Animator>().Play("Unlock");
            //    Debug.Log(_nodes_cover[0].name);
            //    yield return new WaitForSeconds(0.5f);
            //}

            Animator animator = GetDecorationNode(_nodes[index]);
            Debug.Log(animator.runtimeAnimatorController.name);

            //需要解锁立即显示的装修节点不能等待摄像机位移再播动画，2者同时进行表现才没问题
            if (animator.runtimeAnimatorController.name.EndsWith("_immediate"))
            {
                StartCoroutine(MoveCameraRoot(GameCameraAdapter.CurrentCamera.transform.parent,
                                                                          animator.transform.position, 16f, 0.001f));
            }
            else
            {
                yield return StartCoroutine(MoveCameraRoot(GameCameraAdapter.CurrentCamera.transform.parent,
                                                                                           animator.transform.position, 16f, 0.001f));
            }

            if (animator.transform.TryGetComponent(out DecorationAnim _decorationAnim))
            {
                _decorationAnim.SetUp();
                delay = _decorationAnim.MyUnlockAnimDuration + 1.0f;
                if(_decorationAnim.transform.parent.name.Contains("_wreck"))
                {
                    ClearDecorAnim clearDecorAnim = _decorationAnim as ClearDecorAnim;
                    delay = clearDecorAnim.MyDelayForCombineNode;
                }
                Debug.Log(animator.transform.parent.name + "-----------" + delay);

            }
            else
            {
                delay = 1.0f;
                //Debug.Log("没有找到Decoration节点");
            }
            

            if (!animator.gameObject.activeSelf)
            {
                animator.gameObject.SetActive(true);
            }
            CheckEnableAnimator(animator);
            yield return null;
            animator.Play("Unlock");
            //DisableAnimatorWhenClipIsDone(animator);

            yield return new WaitForSeconds(delay);
        }

        IEnumerator MoveCameraRoot(Transform from , Vector3 to , float fov , float duration)
        {
            Camera camera = from.GetComponentInChildren<Camera>();
            float st = camera.fieldOfView;
            Vector3 startPos = from.position;
            AnimationCurve curve = ArtUtility.IncreaseLinearCurve;
            if (duration > 0f)
            {
                float timer = 0.0f;
                while (timer <= 1.0f)
                {
                    timer += Time.deltaTime / duration;
                    from.position = Vector3.Lerp(startPos, to , curve.Evaluate(timer));

                    camera.fieldOfView = Mathf.Lerp(st, fov, curve.Evaluate(timer));
                    yield return null;
                }
            }
            else
            {
                from.position = to;
                camera.fieldOfView = fov;
            }
            
        }

        IEnumerator LoadDecorationScene(Action callback)
        {
            var asy = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            while (!asy.isDone)
            {
                yield return null;
            }

            callback?.Invoke();
        }

        IEnumerator UnloadDecorationScene(Scene scene , Action callback = null)
        {
            var asy = SceneManager.UnloadSceneAsync(scene);
            while (!asy.isDone)
            {
                yield return null;
            }

            callback?.Invoke();
        }

        #region 编辑器方法

#if _ARTEST_PRESENTATION
        public void CheckMeshFilterGlitch()
        {
            var list = FindObjectsOfType<MeshFilter>(true);
            foreach (var item in list)
            {
                if (item != null && item.sharedMesh != null)
                {
                    if (item.sharedMesh.name.Contains("Instance"))
                    {
                        if ( item.GetComponentInParent<DecorationAnim>() != null)
                        {
                            string nodeName =  item.GetComponentInParent<DecorationAnim>().transform.parent.name;
                            string viewName =  item.GetComponentInParent<DecorationAnim>().transform.name;
                            Debug.Log(item.name + "--------" + viewName + "-----------" + nodeName + "---此节点Mesh异常引用实例");
                        }
                        else
                        {
                            Debug.Log(item.name + "--------" + "-----------" + "---此节点Mesh异常引用实例");
                        }
                    }
                }
            }
        }

        public void ClearMeshColliderBuffer()
        {
            var list = FindObjectsOfType<NewFurnitureDecorAnim>(true);
            foreach (var item in list)
            {
                if (item.TryGetComponent(out MeshCollider meshCollider))
                {
                    meshCollider.sharedMesh = null;
                }
            }
        }

        /// <summary>
        /// 检查是否有装修挂点重名，view节点是否命名正确，动画状态机剔除模式是否正确
        /// </summary>
        public void CheckRepeatNodeName()
        {
            //获取场景数据
            Initialize();
            List<string> decorNames = new List<string>();
            decorNames.Clear();
            for (int i = 0; i < _nodes.Count; i++)
            {

                var decors = _nodes[i].GetComponentsInChildren<DecorationAnim>(true);
                foreach (var item in decors)
                {
                    item.ClearRendererBuffer();
                }

                string node_name = _nodes[i].name.Split("-")[0];

                if (_nodes[i].name.Contains("三选一"))
                {
                    if (_nodes[i].childCount != 3)
                    {
                        Debug.LogError(_nodes[i].name + "___________此节点view子节点数量不正确");
                        return;
                    }
                }
                else if (!_nodes[i].name.Contains("activity"))
                {
                    if (_nodes[i].childCount != 1)
                    {
                        Debug.LogError(_nodes[i].name + "___________此节点view子节点数量不正确");
                        return;
                    }
                }
                else
                {
                    if (!_nodes[i].name.Contains("2ndversion"))
                    {
                        if (_nodes[i].childCount != _maxActicityDecorCount_Old)
                        {
                            Debug.LogError(_nodes[i].name + "___________此节点view子节点数量不正确");
                            return;
                        }
                    }
                    else
                    {
                        if (_nodes[i].childCount != _maxActicityDecorCount_New)
                        {
                            Debug.LogError(_nodes[i].name + "___________此节点view子节点数量不正确");
                            return;
                        }
                    }
                }

                //检查装修根节点命名是否重名
                if (!decorNames.Contains(node_name))
                {
                    decorNames.Add(node_name);
                }
                else
                {
                    int _index = decorNames.FindIndex((s) => s == node_name);
    # if UNITY_EDITOR
                    EditorGUIUtility.PingObject(_nodes[i].gameObject);
                    UnityEditor.Selection.activeObject = _nodes[i].gameObject;
    #endif
                    Debug.LogError("这两个节点有重名：" + _index.ToString() + "---------" + i.ToString() + "======>" + node_name);
                }

                //Debug.Log(node_name + "__________" + _nodes[i].childCount.ToString());
                //检查子节点view命名是否正确以及localPosition是否归零，动画化状态机剔除模式修正
                for (int s = 0; s < _nodes[i].childCount; s++)
                {
                    if (_nodes[i].GetChild(s).name != "view" + (s + 1).ToString())
                    {
                        Debug.LogError(_nodes[i].name + "此节点" + s.ToString() + "号位子节点命名错误！！！！！");
                    }
                    if (_nodes[i].GetChild(s).localPosition != Vector3.zero)
                    {
                        Debug.LogError(_nodes[i].name + "此节点" + s.ToString() + "号位子节点本地坐标未归零！！！！！");
                    }

                    if (_nodes[i].GetChild(s).GetComponent<Animator>().cullingMode != AnimatorCullingMode.CullUpdateTransforms)
                    {
                        _nodes[i].GetChild(s).GetComponent<Animator>().cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    }
                }

                ////找None
                //var infos = _nodes[i].GetComponentsInChildren<GrowAnimationTypeInfo>(true);
                //for (int z = 0; z < infos.Length; z++)
                //{
                //    if (infos[z].animationType == GrowAnimationType.Pop)
                //    {
                //        Debug.Log(infos[z].gameObject.name);
                //    }
                //}
            }

            //清除节点数据
            _nodes_cover.Clear();
            _nodes_cover = null;
            _nodes.Clear();
            _nodes = null;
        }

        public void SetSpriteRenderersSortingPointToPivot()
        {
            if(_modifyRoot != null)
            {
                var decors = _modifyRoot.GetComponentsInChildren<DecorationAnim>(true).ToList();
                foreach(var decor in decors)
                {
                    decor.SetSortingPoint();
                }
                _modifyRoot = null;
            }
        }

        [SerializeField] float Scalor = 1.0f;
        public void ScalingAllSubRoots()
        {
            if(_modifyRoot != null)
            {
                _modifyRoot.localScale = Vector3.one * Scalor;
                var decors = _modifyRoot.GetComponentsInChildren<DecorationAnim>(true).ToList();
                List<Transform> roots = new List<Transform>();
                roots.Clear();
                for (int a = 0; a < decors.Count; a++)
                {
                    var root = decors[a].transform.Find("root");
                    roots.Add(root);
                    root.SetParent(_modifyRoot.parent);
                }
                _modifyRoot.localScale = Vector3.one;
                for (int b = 0; b < roots.Count; b++)
                {
                    roots[b].SetParent(decors[b].transform);
                }

                _modifyRoot = null;
            }
        }
        public void SetEditorModeSpineAniamtion(string clipName , SkeletonAnimation skeletonAniamtion)
        {
            skeletonAniamtion.AnimationName = clipName;
            //spine默认有0.2s 过渡动画，这里需要直接更新到第0.2s，才能完全显示正确
            skeletonAniamtion.Update(0.2f);
            //EditorUtility.SetDirty(skeletonAniamtion.skeletonDataAsset);
        }

        public void AddDecorationScene()
        {
            var sc = EditorSceneManager.GetActiveScene();
            if(sc.name == MyDecorationSceneName)
            {
                Debug.Log($"{MyDecorationSceneName} 场景已打开");
                return;
            }
            var uids = AssetDatabase.FindAssets(MyDecorationSceneName, new string[]{"Assets"});
            var obj_path = AssetDatabase.GUIDToAssetPath(uids[0]);
            var targetScene = EditorSceneManager.OpenScene(obj_path , OpenSceneMode.Additive);
            if(targetScene != null)
            {
                EditorSceneManager.SetActiveScene(targetScene);
                EditorUtility.SetDirty(this);
            }
            
        }
#endif

        #endregion
    }
} 