using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class EffectManager : AboloSingleton<EffectManager>
    {
        public PrefabFactory EffectFactory;
        public static Dictionary<string, GameObject> EffectDic;
        public override void Init()
        {
            base.Init();
            EffectDic = new Dictionary<string, GameObject>();
            foreach (var item in EffectFactory.MyPrefabs)
            {
                EffectDic.Add(item.name, item.prefab);
            }
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
    }
}
