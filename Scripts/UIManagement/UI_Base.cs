using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AboloLib;
using static AboloLib.ArtUtility;

namespace AboloLib
{
    [AttributeUsage(AttributeTargets.Field)]
     public class UIBindAttribute :Attribute
    {
        public string mPath;
        public UIBindAttribute(string path)
        {
            this.mPath = path;
        }
    }

    public class UI_Base : MonoBehaviour
    {
        public static string PageOpenAnimName = "ani_page_open";
        public static string PageCloseAnimName = "ani_page_close";
        public static string PopOpenAnimName = "ani_open";
        public static string PopCloseAnimName = "ani_close";
        [UIBind("root/bg/CloseBtn")]
        protected Button CloseBtn = null;
        private UI_CustomFx customFx = null;
        private Coroutine animCoroutine = null;

        Action onOpenAnimationDone;
        public Action OnOpenAniamtionDone
        {
            get => onOpenAnimationDone;
            set => onOpenAnimationDone = value;
        }
        private bool isReuse = false;
        public bool IsReuse
        {
            get => isReuse;
            set => isReuse = value;
        }
        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {

        }

        public virtual void RefreshUI()
        {

        }

        public virtual void Show(int canvasIndex = 3 , bool mute = false)
        {
            gameObject.transform.SetParent(UIManager.GetCanvas(canvasIndex));
            gameObject.transform.localScale = Vector3.one;
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            if (animCoroutine != null)
            {
                ScheduleAdapter.Schedual.StopCoroutine(animCoroutine);
                OnOpenAniamtionDone?.Invoke();
            }
            //播放动画时，屏蔽交互操作 
            if (transform.TryPlayAnimation(PageOpenAnimName))
            {
                RayCastBlock._instance.SetRayCastBlock(true);
                float delay = transform.GetComponent<Animation>().GetClip(PageOpenAnimName).length;
                animCoroutine = ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(delay , () =>
                {
                    RayCastBlock._instance.SetRayCastBlock(false);
                    OnOpenAniamtionDone?.Invoke();
                }));
            }
            RefreshUI();

            if (!mute) AudioPlayerAdapter.PlayAudio("UI_PageOpen_Common");

            Debug.Log($"Show Window {this.name}");
        }

        public virtual void Hide()
        {
            if (animCoroutine != null)
            {
                ScheduleAdapter.Schedual.StopCoroutine(animCoroutine);
            }
            //播放动画时，屏蔽交互操作 
            if (transform.TryPlayAnimation(PageCloseAnimName))
            {
                RayCastBlock._instance.SetRayCastBlock(true);
                float delay = transform.GetComponent<Animation>().GetClip(PageCloseAnimName).length;
                animCoroutine = ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(delay, () =>
                {
                    gameObject.transform.SetParent(UIManager.GetCanvas(0));
                    if (gameObject.activeSelf) gameObject.SetActive(false);
                    RayCastBlock._instance.SetRayCastBlock(false);
                }));
            }
            else
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
            }

            //AudioPlayerAdapter.PlayAudio("Merge_Close");
        }

        public virtual void Setup()
        {
            //ClearAllButtonListener();
            if (CloseBtn != null)
            {
                CloseBtn.onClick.AddListener(delegate { Hide(); });
            }

            //获取UI动画脚本
            if (!TryGetComponent<UI_CustomFx>(out customFx))
            {
                customFx = gameObject.AddComponent<UI_CustomFx>();
            }
        }

        public void ClearAllButtonListener()
        {
            var btns = GetComponentsInChildren<Button>(true);
            foreach (var item in btns)
            {
                item.onClick.RemoveAllListeners();
            }
        }

        public static  void SetProductItem(GameObject itemObj , int productId , int number)
        {
            if (itemObj.activeInHierarchy) itemObj.SetActive(false);
            string path = ResUtility.ProductSpriteResPath + productId.ToString();
            itemObj.transform.Find("icon").GetComponent<Image>().sprite = 
                InstantiateSpriteFromResource(path);
            itemObj.transform.Find("num").GetComponent<TextMeshProUGUI>().text = 
                number.ToString();
            itemObj.SetActive(true);
        }

        /// <summary>
        /// 设置界面需要引用的节点
        /// </summary>
        public static void SetUIElementsBinding(object target , Transform root)
        {
            Type mType = target.GetType();

            Dictionary<string , FieldInfo> filedInfoList = new Dictionary<string, FieldInfo>();
            ArtUtility.GetAllFieldInfo(mType, filedInfoList);
            foreach (var item in filedInfoList)
            {
                if (item.Value.IsDefined(typeof(UIBindAttribute), true))
                {
                    UIBindAttribute uiBind = (UIBindAttribute)item.Value.GetCustomAttribute(typeof(UIBindAttribute), true);
                    if (item.Value.FieldType.FullName == typeof(GameObject).FullName)
                    {
                        var obj = GetChildGameObject(root, uiBind);
                        item.Value.SetValue(target, obj);
                    }
                    else
                    {
                        var component = GetChildElements(root, uiBind , item.Value.FieldType);
                        item.Value.SetValue(target, component);
                    }
                }
            }
            filedInfoList.Clear();
            filedInfoList = null;
        }

        public static Component GetChildElements(Transform root , UIBindAttribute bindAttribute, Type type)
        {
            Transform tran = root.transform.Find(bindAttribute.mPath);
            if (tran != null)
            {
                return tran.GetComponent(type);
            }
            else
            {
                return null;
            }
        }

        public static GameObject GetChildGameObject(Transform root , UIBindAttribute bindAttribute)
        {
            Transform tran = root.transform.Find(bindAttribute.mPath);
            if (tran != null)
            {
                return tran.gameObject;
            }
            else
            {
                return null;
            }

        }
    }
}
