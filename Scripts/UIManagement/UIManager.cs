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

        public static PrefabFactory UI_Prefabs;
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
            if (UI_Prefabs != null)
            {
                if (UI_Prefabs.MyPrefabs != null)
                {
                    GameUIDic = new Dictionary<Type, UI_Base>();

                    foreach (var item in UI_Prefabs.MyPrefabs)
                    {
                        string winName = $"{CustomDataRegister.CustomNamespace}.{item.name}";
                        var win_obj = Instantiate(item.prefab, GetCanvas(0));
                        var ui_base = win_obj.gameObject.AddComponent(Type.GetType(winName)) as UI_Base;
                        //隐藏所有界面
                        win_obj.gameObject.SetActive(false);
                        //测试特性反射start  貌似成功
                        UI_Base.SetUIElementsBinding(ui_base, win_obj.transform);
                        //测试特性反射end
                        ui_base.Setup();
                        win_obj.transform.localScale = Vector3.one;
                        win_obj.transform.localPosition = Vector3.zero;
                        GameUIDic.Add(ui_base.GetType(), ui_base);
                    }

                    Initialized = true;
                }
                else
                {
#if _ARTEST_PRESENTATION
                    Debug.LogError("UI_Prefabs.MyPrefabs 赋值为空");
#endif
                }

            }
            else
            {
#if _ARTEST_PRESENTATION
                Debug.LogError("UI_Prefabs 赋值为空");
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
