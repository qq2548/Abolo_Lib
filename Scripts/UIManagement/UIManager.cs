using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AboloLib;

namespace AboloLib
{
    public class UIManager : AboloSingleton<UIManager>
    {
        const int MAX_CANVAS_COUNT = 5;

        public static Dictionary<Type, UI_Base> GameUIDic;

        public static Transform UI_Root;
        public static Camera UI_Camera;
        public static UIFactory UI_Factory;
        private bool _initialized;
        public static bool Initialized
        {
            get => _instance._initialized;
            set => _instance._initialized = value;
        }


        public override void Init()
        {
            base.Init();
            UI_Root = UICanvasAdapter.CurrentCanvas.transform.parent;
            UI_Camera = UICanvasAdapter.CurrentCanvas.worldCamera;
            if (UI_Factory != null)
            {
                if (UI_Factory.UI_Bases != null)
                {
                    GameUIDic = new Dictionary<Type, UI_Base>();
                    foreach (var item in UI_Factory.UI_Bases)
                    {
                        var ui = Instantiate(item , GetCanvas(0));
                        //隐藏所有界面
                        ui.gameObject.SetActive(false);
                        //测试特性反射start  貌似成功
                        UI_Base.SetUIElementsBinding(ui , ui.transform);
                        //测试特性反射end
                        ui.Setup();
                        ui.transform.localScale = Vector3.one;
                        ui.transform.localPosition = Vector3.zero;
                        GameUIDic.Add(ui.GetType() , ui);
                    }

                    Initialized = true;
                }
                else
                {
#if _ARTEST_PRESENTATION
                    Debug.LogError("UIFactory.UI_Base 赋值为空");
#endif
                }

            }
            else
            {
#if _ARTEST_PRESENTATION
                Debug.LogError("UIFactory 赋值为空");
#endif
            }
        }

        public static Transform GetCanvas(int index)
        {
            if (index >= 0 && index <= MAX_CANVAS_COUNT)
            {
                return UI_Root.Find("CanvasRoot").GetChild(index);
            }
            else
            {
                Debug.LogError("index 参数的值大于最大限制，无法获取到目标Canvas");
                return null;
            }
        }

        public static T GetUI<T>() where T :UI_Base
        {
            //if (GameController._instance.CurrentWin != null)
            //{
            //    GameController._instance.CurrentWin.IsReuse = false;
            //}
            if (Initialized)
            {
                T ui = GameUIDic[typeof(T)] as T;

                if (ui != null)
                {
                    return ui ;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                _instance.Init();
                return GetUI<T>();
            }
        }

    }
}
