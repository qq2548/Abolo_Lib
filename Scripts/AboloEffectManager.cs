using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class AboloEffectManager : AboloSingleton<AboloEffectManager>
    {
        public static FlyItem InitFlyItemInstance(int id , Transform parent = null)
        {
            FlyItem itemRes = Resources.Load<FlyItem>("Prefabs/FlyItems/CardFlyItem_"+ id.ToString());
            FlyItem item = Instantiate(itemRes , parent);
            item.transform.Find("root/Art").GetComponent<Image>().sprite =
                Resources.Load<Sprite>("Sprites/MergeItems/Icon_" + id.ToString("D3"));
            return item;
        }

        public static void RecailmFxObject(float delay , GameObject go)
        {
            ScheduleAdapter.DoSchedual(delay, () =>
            {
                GameObject.DestroyImmediate(go);
            });
        }
    }
}
