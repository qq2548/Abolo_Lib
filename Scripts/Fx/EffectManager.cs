using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class EffectManager : AboloSingleton<EffectManager>
    {
        public PrefabFactory EffectFactory;
        public MyObjectFactory FlyItemFactory;
        public static Dictionary<string, GameObject> EffectDic;
        //public static Dictionary<int, MyObject> FlyItemDic;
        public override void Init()
        {
            base.Init();
            EffectDic = new Dictionary<string, GameObject>();
            foreach (var item in EffectFactory.MyPrefabs)
            {
                EffectDic.Add(item.name, item.prefab);
            }

            // FlyItemDic = new Dictionary<int, MyObject>();
            // foreach (var item in FlyItemFactory.prefabs)
            // {
            //     FlyItemDic.Add(item.Id, item);
            // }
        }
        /// <summary>
        /// 播放指定名称的特效，并在一定延迟后销毁
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="name">特效资源名称</param>
        /// <param name="delay">播放后延迟销毁的时间</param>
        /// <returns></returns>
        public static GameObject PlayFx(Vector3 position, string name, float delay = 1.0f)
        {
            var fx = EffectDic.ContainsKey(name)? Instantiate(EffectDic[name], position, Quaternion.identity) : null;
            if (fx != null)
            {
                if(fx.TryGetComponent( out IFxAnimCtrl fxAnim)) fxAnim.Play();
            }
            ScheduleAdapter.DoSchedual(delay, () => { if (fx != null) DestroyImmediate(fx); });
            return fx;
        }

        public static FlyItem FlyCurrency(Vector3 from , Vector3 to , int id , Action callback = null)
        {
            int key = 3;
            switch (id)
            {
                case 999:
                    key = 0;
                break;
                case 648:
                    key = 1;
                break;
                case 998:
                    key = 2;
                break;
            }
            bool isCurrency = ArtGameManager.IsCurrency(id);
            //string key = isCurrency ? id.ToString() : "SingleItem";
            var item = _instance.FlyItemFactory.Get(key) as FlyItem;
            if(!isCurrency)
            {
                var img = item.transform.Find("root/Art").GetComponent<Image>();
                img.sprite = ArtUtility.InstantiateSpriteFromResource(GlobalText.ItemSpritePath(id));
            }
            item.transform.SetParent(UICanvasAdapter.CurrentCanvas.transform.GetChild(4), true);
            item.transform.localScale = Vector3.one;
            item.OnItemFlyDone = () =>
            {
                item.transform.SetParent(null);
                _instance.FlyItemFactory.Reclaim(item);
            } ;            
            callback += () =>
            {

            };
            FlyItemUtility.FlyProccedual(item , from , to , callback);
            return item;
        }

        public static List<FlyItem> FlyMultipleCurrency(int count , Vector3 from , Vector3 to , int id , Action callback = null)
        {
            int key = 3;
            switch (id)
            {
                case 999:
                    key = 0;
                break;
                case 648:
                    key = 1;
                break;
                case 998:
                    key = 2;
                break;
            }
            bool isCurrency = ArtGameManager.IsCurrency(id);
            List<FlyItem> items = new List<FlyItem>();
            items.Clear();
            List<Action> actions = new List<Action>();
            actions.Clear();
            //string key = isCurrency ? id.ToString() : "SingleItem";
            for(int i = 0; i < count; i++) 
            {
                var item = _instance.FlyItemFactory.Get(key) as FlyItem;
                if(!isCurrency)
                {
                    var img = item.transform.Find("root/Art").GetComponent<Image>();
                    img.sprite = ArtUtility.InstantiateSpriteFromResource(GlobalText.ItemSpritePath(id));
                }
                item.transform.SetParent(UICanvasAdapter.CurrentCanvas.transform.GetChild(4), true);
                item.transform.localScale = Vector3.one;
                item.OnItemFlyDone = () =>
                {
                    item.transform.SetParent(null);
                    _instance.FlyItemFactory.Reclaim(item);
                } ;
                actions.Add(() =>
                {
                    
                });
                // callback += () =>
                // {
      
                // };   
                items.Add(item);
            }
            if(id == ArtGameManager.GoldId)
            {
                FlyItemUtility.MultipleFlyProccedual(items , from , to , actions ,new Vector2(0.0f , 1.0f) ,callback);
            }
            else
            {
                FlyItemUtility.MultipleFlyProccedual(items , from , to , actions , callback);
            }
            return items;
        }
    }
}
