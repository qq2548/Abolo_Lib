using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    [System.Serializable]
    public class FlyData
    {
        public bool DisableRandomShootPosition = false;
        //发射阶段位移周期
        public float ShootDuraiotn = 0.5f;
        //发射完成到开始飞行延迟
        public float FlyDlay = 0.0f;
        //飞行阶段位移周期
        public float FlyDuraiotn = 0.5f;
        public float Range = 0.5f;
        public AnimationCurve ShootPosCurve;
        public AnimationCurve ShootScaleCurve;
        public AnimationCurve FlyPosCurve;
        public AnimationCurve FlyScaleCurve;
        public AnimationCurve FlyPosOffsetCurve;
        public float MinDegree = 0f;
        public float MaxDegree = 0f;
        public FxAnim OnDoneFx;

        public float TotalDuration
        {
            get => ShootDuraiotn + FlyDuraiotn;
        }
        
        public Vector3 ShootPosition(Vector3 from)
        {
            float len = DisableRandomShootPosition? Range : UnityEngine.Random.Range(0f, Range);
            float degree = UnityEngine.Random.Range(MinDegree, MaxDegree);
            Vector3 to = from + FlyItemUtility.VectorRotate(new Vector3(len, 0, 0), Mathf.Deg2Rad * (- degree));
            return to;
        }
    }
    public static class FlyItemUtility
    {
        /// <summary>
        /// 顺时针旋转向量
        /// </summary>
        /// <param name="oriVec"></param>
        /// <param name="redius">旋转弧度</param>
        /// <returns></returns>
        public static Vector3 VectorRotate(Vector3 oriVec, float redius)
        {
            float x = oriVec.x * Mathf.Cos(redius) + oriVec.y * Mathf.Sin(redius);
            float y = oriVec.y * Mathf.Cos(redius) - oriVec.x * Mathf.Sin(redius);
            return new Vector3(x, y, 0);
        }


        public static int GetOffsetDirection(Vector3 position)
        {
            Vector3 screen_pos = UICanvasAdapter.CurrentCanvas.worldCamera.WorldToScreenPoint(position);
            if (screen_pos.x > Screen.width * 0.8f)
            {
                return -1;
            }
            else if (screen_pos.x < Screen.width * 0.2f)
            {
                return 1;
            }
            else
            {
                return UnityEngine.Random.Range(-1, 2);
            }
        }

        public static Vector3  FlyShootOut(FlyItem item  , Vector3 from , Vector3 to , Action callback = null)
        {
            item.OnStartShootOut();
            Vector3 toscale = item.transform.localScale;
            Action<float> _delta = (value) =>
            {
                item.transform.position = Vector3.Lerp(from , to , item.MyFlyData.ShootPosCurve.Evaluate(value));
                item.transform.localScale = Vector3.Lerp(Vector3.zero , toscale, item.MyFlyData.ShootScaleCurve.Evaluate(value));
            };
            ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoAnimation(item.MyFlyData.ShootDuraiotn , _delta , callback));
            return to;
        }

        public static void FlyToPosition(FlyItem item , Vector3 from , Vector3 to , Action callback = null)
        {
            int offset_dir = GetOffsetDirection(from);
            Vector3 fromscale = item.transform.localScale;
            Vector3 toscale = item.transform.localScale * 3.0f;
            Action<float> _delta = (value) =>
            {
                Vector3 pos = Vector3.Lerp(from, to, item.MyFlyData.FlyPosCurve.Evaluate(value));
                pos += Vector3.one * item.MyFlyData.FlyPosOffsetCurve.Evaluate(value) * offset_dir;
                item.transform.position = pos;
                item.transform.localScale = Vector3.Lerp(fromscale, toscale, item.MyFlyData.FlyScaleCurve.Evaluate(value));

                //shadow fly
                if (item.Shadow != null)
                {
                    float delta = 0.0f;
                    Vector3 asix = to - from;
                    Vector3 v1 = pos - from;
                    float fac = (Vector3.Distance(from, to) * Vector3.Distance(from, pos));
                    if (fac != 0f)
                    {
                        delta = v1.magnitude * Vector3.Dot(asix, v1) / fac;
                    }
                    else
                    {
                        delta = 0f;
                    }
                    Vector3 sPos = Vector3.LerpUnclamped(from, to, (delta / asix.magnitude));
                    item.Shadow.transform.position = sPos;
                    Vector3 shadow_scale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, item.MyFlyData.FlyScaleCurve.Evaluate(value));
                    item.Shadow.transform.localScale = shadow_scale;
                }
                //end shadow fly
            };

            var enumerator = ArtAnimation.DoAnimation(item.MyFlyData.FlyDuraiotn, _delta, callback);
            ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(item.MyFlyData.FlyDlay , () =>
            {
                ScheduleAdapter.Schedual.StartCoroutine(enumerator);
            }));
        }

        public static void FlyProccedual(FlyItem item, Vector3 from , Vector3 to , Action callback = null)
        {
            if(!item.gameObject.activeSelf)
            {
                item.gameObject.SetActive(true);
            }
            var shootPos = item.MyFlyData.ShootPosition(from);
            FlyShootOut(item , from , shootPos, () =>
            {
                FlyToPosition(item , shootPos , to ,() =>
                {
                    item.OnFlyDone();
                    callback?.Invoke();
                });
            });
        }

        public static void MultipleFlyProccedual(List<FlyItem> items , float shootInterval ,Vector3 from , Vector3 to , Action callback = null)
        {
            foreach (var item in items)
            {
                if (item.gameObject.activeInHierarchy && item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(false);
                }
            }
            ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoActionWithInterval(items.Count , shootInterval , (index) =>
            {
                items[index].MyFlyData.FlyDlay = (shootInterval) * (items.Count - index) + items[index].MyFlyData.ShootDuraiotn * ((items.Count - index) /(float)items.Count);
                FlyProccedual(items[index] , from , to , () => GameObject.DestroyImmediate(items[index].gameObject));
            } , callback));
        }
    }
}
