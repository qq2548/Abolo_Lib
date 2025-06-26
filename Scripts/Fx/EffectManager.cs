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

        public static GameObject PlayFx(Vector3 position, string name, float delay = 1.0f)
        {
            var fx = Instantiate(EffectDic[name], position, Quaternion.identity);
            fx.GetComponent<IFxAnimCtrl>().Play();
            ScheduleAdapter.DoSchedual(delay, () => DestroyImmediate(fx));
            return fx;
        }
    }
}
