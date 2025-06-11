using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AboloLib
{
    [DisallowMultipleComponent]
    public class FlyItem : MonoBehaviour
    {
        static string OpenAnimationName = "ani_open";
        static string IdleAnimationName = "ani_idle";
        static string CloseAnimationName = "ani_close";
        public FlyData MyFlyData;
        public Transform Shadow;
        public Action OnItemFlyDone = null;

        
        public void OnStartShootOut()
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            if (TryGetComponent(out Animation ani))
            {
                if (ani.GetClip(OpenAnimationName) != null)
                {
                    ani.Play(OpenAnimationName);
                }

                if (ani.GetClip(IdleAnimationName) != null)
                {
                    ani.PlayQueued(IdleAnimationName);
                }
            }
        }

        public void OnFlyDone()
        {
            OnItemFlyDone?.Invoke();
            if (MyFlyData.OnDoneFx != null)
            {
                var fx = Instantiate(MyFlyData.OnDoneFx, transform.parent);
                fx.transform.position = transform.position;
                fx.Stop(() => GameObject.Destroy(fx.gameObject, 1.5f));
                fx.Play();
            }

            float delay = 0.0f;
            if (TryGetComponent( out Animation ani))
            {
                if (ani.GetClip(CloseAnimationName) != null)
                {
                    ani.Play(CloseAnimationName);
                    delay = ani.GetClip(CloseAnimationName).length;
                }
            }
            ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(delay, () => gameObject.SetActive(false)));
        }
#if UNITY_EDITOR
        static FlyItem()
        {
            ObjectFactory.componentWasAdded += OnComponentWasAdded;
        }
        [ExecuteInEditMode]
        static void OnComponentWasAdded(Component component)
        {
            if (component.GetType() != typeof(FlyItem))
            {
                return;
            }
            else
            {
                FlyItem fly_item = component as FlyItem;
                var data = AssetDatabase.LoadAssetAtPath<FlyDataPreset>("Assets/Art/Scripts/FlyItems/DefaultFlyDataPreset.asset").FlyData;
                fly_item.MyFlyData = new FlyData();
                fly_item.MyFlyData.DisableRandomShootPosition = data.DisableRandomShootPosition;
                fly_item.MyFlyData.ShootDuraiotn = data.ShootDuraiotn;
                fly_item.MyFlyData.FlyDlay = data.FlyDlay;
                fly_item.MyFlyData.FlyDuraiotn = data.FlyDuraiotn;
                fly_item.MyFlyData.Range = data.Range;
                fly_item.MyFlyData.ShootPosCurve = data.ShootPosCurve;
                fly_item.MyFlyData.ShootScaleCurve = data.ShootScaleCurve;
                fly_item.MyFlyData.FlyPosCurve = data.FlyPosCurve;
                fly_item.MyFlyData.FlyScaleCurve = data.FlyScaleCurve;
                fly_item.MyFlyData.FlyPosOffsetCurve = data.FlyPosOffsetCurve;
                fly_item.MyFlyData.MinDegree = data.MinDegree;
                fly_item.MyFlyData.MaxDegree = data.MaxDegree;
                fly_item.MyFlyData.OnDoneFx = data.OnDoneFx;
                data = null;
            }
        }
#endif
    }
}
