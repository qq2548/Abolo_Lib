using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArtUtils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class UI_Transformer : AboloSingleton<UI_Transformer>
    {
        public class TransformNode
        {
            public Transform transform;
            public UI_CustomFx customFx;
            [UIBind("root/bg")]
            public RectTransform bg;
            [UIBind("root/bg/bg_sub")]
            RectTransform bg_sub;
            [UIBind("root/bg/titleBg")]
            RectTransform title_bg;
            [UIBind("root/bg/OkBtn")]
            RectTransform okBtn;
            [UIBind("root/bg/CloseBtn")]
            RectTransform closeBtn;
            private List<RectTransform> rects;
            public List<RectTransform> MyRects
            {
                get
                {
                    if(rects == null||rects.Count == 0)
                    {
                        rects = new (){bg , bg_sub , title_bg , okBtn, closeBtn};
                    }
                    return rects;
                }
            }
            public void Setup()
            {
                if(rects == null||rects.Count == 0)
                {
                    rects = new (){bg , bg_sub , title_bg , okBtn, closeBtn};
                }
            }
            public RectTransform GetRT(string name)
            {
                return MyRects.FirstOrDefault(x => x.gameObject.name == name);
            }
        }
        public struct RectData
        {
            public Vector3 position;
            public Vector2 sizeDelta;

            public Color color;
            public RectData(RectTransform rectTransform)
            {
                this.position = rectTransform != null ? rectTransform.position : Vector3.zero;
                this.sizeDelta = rectTransform != null ? rectTransform.sizeDelta : Vector2.zero;
                Color color = rectTransform != null&&rectTransform.TryGetComponent(out Image img_f)? 
                    img_f.color : Color.white;
                this.color = color;
            }
        }
        public class TransfromPair
        {
            public RectData from;

            public RectData to;

            public TransfromPair(RectTransform from,RectTransform to)
            {
                this.from = new RectData(from );

                this.to = new RectData(to);
            }
        }


        [UIBind("root/testBtn")]
        Button testBtn;
        public Dictionary<string, TransfromPair> TransfromPairs;
        List<TransformNode> transformNodes = new List<TransformNode>();

        List<RectData[]> rectDatas= new List<RectData[]>();


        public static List<string> subUINames = new()
        {
            "form_0",  
            "form_1",  
            "form_2",  
            "form_3",  
            "form_4",  
            "form_5",  
        };
        public static List<string> PagePathConfig = new()
        {
            "root/form_0",  
            "root/form_1",  
            "root/form_2",  
            "root/form_3",  
            "root/form_4",  
            "root/form_5",  
        };

        public static List<UI_Base> uI_Bases = new List<UI_Base>();
        public override void Init()
        {
            base.Init();
            DestroyImmediate(transform.GetComponent<UI_CustomFx>());
            for (int a = 0; a < uI_Bases.Count; a++)
            {
                var node = new TransformNode();
                node.transform = uI_Bases[a].transform;
                UI_Base.SetUIElementsBinding(node , node.transform);

                if(a == 0)
                {
                    var btn =  node.transform.AddComponent<Button>();
                    btn.onClick.AddListener(() => TransformUI(0 , 1));
                }

                transformNodes.Add(node);

                if(!node.transform.TryGetComponent(out UI_CustomFx custom_ui))
                {
                    custom_ui = node.transform.AddComponent<UI_CustomFx>();
                }

                custom_ui.Interval = 0.05f;
                node.customFx = custom_ui;
                Debug.Log($"{node.transform.name}---------------------{node.MyRects.Count}");
                RectData[] datas = new RectData[node.MyRects.Count];
                for (int b = 0; b < node.MyRects.Count; b++)
                {
                    Debug.Log($"{b}++++++++++++++{node.transform.name}");
                    datas[b] = new RectData(node.MyRects[b]);
                }
                rectDatas.Add(datas);
            }

            // for (int i = 0; i < transformNodes.Count; i++)
            // {
            //     transformNodes[i].transform.gameObject.SetActive(i == index );
            //     transformNodes[i].customFx.ResetSubAnimations();
            // }

            //testBtn.onClick.AddListener(TestFunc);
        }

        // public override void Show(int canvasIndex = 3 , bool mute = false)
        // {
        //     base.Show(canvasIndex, mute);
        //     for (int i = 0; i < transformNodes.Count; i++)
        //     {
        //         transformNodes[i].transform.gameObject.SetActive(i == index );
        //         transformNodes[i].customFx.ResetSubAnimations();
        //     }
        //     // int fromIndex = Index != 0 ? Index - 1 : subUINames.Count -1; 
        //     // int toIndex = Index;
        //     // SetDestineyToStartCondition(transformNodes[toIndex] , fromIndex);
        // }

        public void TestFunc()
        {
            int fromIndex = Index != 0 ? Index - 1 : subUINames.Count -1; 
            int toIndex = Index;
            TransformUI(fromIndex , toIndex);
            Index ++;
        }
        int index;
        public int Index
        {
            get => index;
            set
            {
                if(value < subUINames.Count)
                {
                    index = value;
                }
                else
                {
                    index = 0;
                }
            }
        }


        void SetDestineyToStartCondition(TransformNode target , int fromIndex)
        {
            var from = rectDatas[fromIndex];
            var to = target;
            if(to != null)
            {
                //UI的默认Canvas层级一定要改，这里写死非常不合理
                to.transform.SetParent(UIManager.GetCanvas(3));
                if(!to.transform.gameObject.activeSelf) to.transform.gameObject.SetActive(true);
                 to.customFx.StopQueuedActive();
                to.customFx.ResetSubAnimations();
                for (int i = 0; i < to.MyRects.Count; i++)
                {
                    if(to.MyRects[i] != null)
                    {
                        to.MyRects[i].position = from[i].position;
                        to.MyRects[i].sizeDelta = from[i].sizeDelta;
                        if(to.MyRects[i].TryGetComponent(out Image img))
                        {
                            img.color = from[i].color;
                        }
                    }
                }
            }
            else
            {

            }
        }

        public void TransformUI(string fromName , string toName)
        {
            int fromIndex = subUINames.IndexOf(fromName);
            int toIndex = subUINames.IndexOf(toName);
            TransformUI(fromIndex , toIndex);
        }

        public void TransformUI(int fromIndex , int toIndex)
        {
            // for (int i = 0; i < transformNodes.Count; i++)
            // {
            //     transformNodes[i].transform.gameObject.SetActive(i == index);
            //     transformNodes[i].customFx.ResetSubAnimations();
            //     transformNodes[i].customFx.ResetLayout();
            // }
            //Debug.Log();
            SetDestineyToStartCondition(transformNodes[toIndex] , fromIndex);
            List<RectTransform> rects = transformNodes[toIndex].MyRects;
            ScheduleAdapter.DoAnimation(0.5f , (value) =>
                {
                    for (int i = 0; i < rects.Count; i++)
                    {
                        if(rects[i] != null)
                        {
                            float fac = CustomAssetManager.AnimCurveDic[CurveFactory.CurveType.SlowSteady].Evaluate(value);
                            rects[i].position = Vector3.Lerp(rectDatas[fromIndex][i].position , rectDatas[toIndex][i].position , fac);
                            rects[i].sizeDelta = Vector2.Lerp(rectDatas[fromIndex][i].sizeDelta , rectDatas[toIndex][i].sizeDelta , fac);
                            if(rects[i].TryGetComponent(out Image image))
                            {
                                image.color = Color.Lerp(rectDatas[fromIndex][i].color,rectDatas[toIndex][i].color , fac);
                            }
                        }
                    }
                } 
                //Todo：这里跟动画事件帧的激活动画有冲突，待修复
                //, () => transformNodes[toIndex].customFx.PlayQueuedActive()
            );
        }
    }
}
