using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public enum FlyDirType
    {
        Skew,
        Left,
        Right,
        Up,
        Down,
        Horizontal,
        Vertical,
    }
    [System.Serializable]
    public class FlyData
    {
        public FlyDirType MyFlyDirType;
        public bool DisableRandomShootPosition = false;
        //发射阶段位移周期
        public float ShootDuraiotn = 0.5f;
        //发射完成到开始飞行延迟
        public float FlyDelay = 0.0f;
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
        /// 多个 item 发射周期常量
        /// </summary>
        public const float MULTI_FLY_INTERVAL = 0.15f;
        public const float MULTI_SHOOT_DURATION = 0.30f;
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


        private static Vector3 GetOffsetScalor(Vector3 position , FlyDirType dirType)
        {
            Vector3 result = Vector3.zero;
            int scalor = 1;
            Vector3 screen_pos = UICanvasAdapter.CurrentCanvas.worldCamera.WorldToScreenPoint(position);
            switch (dirType)
            {
                case FlyDirType.Skew:
                    result =  Vector3.one;

                break;
                case FlyDirType.Right:
                    result =  Vector3.right;

                break;
                case FlyDirType.Left:
                    result =  Vector3.left;

                break;
                case FlyDirType.Up:
                    result =  Vector3.up;

                break;
                case FlyDirType.Down:
                    result =  Vector3.down;

                break;
                case FlyDirType.Horizontal:
                    result =  Vector3.right;

                break;
                case FlyDirType.Vertical:
                    result =  Vector3.up;

                break;
            }
            if(dirType == FlyDirType.Skew || dirType == FlyDirType.Horizontal || dirType == FlyDirType.Vertical)
            {
                if (screen_pos.x > Screen.width * 0.8f)
                {
                    scalor = -1;
                }
                else if (screen_pos.x < Screen.width * 0.2f)
                {
                    scalor = 1;
                }
                else
                {
                    scalor = UnityEngine.Random.Range(-1, 2);
                }
            }
            return result * scalor;
        }

        public static float GetDelayFromStartToHit(List<FlyItem> flyItems)
        {
            float result = 0.0f;
            int count = flyItems.Count - 1;
            result = MULTI_SHOOT_DURATION*count/(count + 1) + flyItems[0].MyFlyData.ShootDuraiotn + flyItems[0].MyFlyData.FlyDuraiotn;
                //flyItems[0].MyFlyData.ShootDuraiotn + flyItems[0].MyFlyData.FlyDuraiotn + flyItems[0].MyFlyData.FlyDelay;
            return result;
        }

        public static float GetHitingInterval(int count)
        {
            float result = MULTI_FLY_INTERVAL - MULTI_SHOOT_DURATION/(float)count;
            return result >= 0f ? result : 0.0f;
        }

        private static Vector3  FlyShootOut(FlyItem item  , Vector3 from , Vector3 to , Action callback = null)
        {
            item.OnStartShootOut();
            Vector3 toscale = item.transform.localScale;
            Action<float> _delta = (value) =>
            {
                item.transform.position = Vector3.Lerp(from , to , item.MyFlyData.ShootPosCurve.Evaluate(value));
                item.transform.localScale = Vector3.one * item.MyFlyData.ShootScaleCurve.Evaluate(value);
            };
            ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoAnimation(item.MyFlyData.ShootDuraiotn , _delta , callback));
            return to;
        }

        private static void FlyToPosition(FlyItem item , Vector3 from , Vector3 to , Action callback = null)
        {
            Vector3 offset = GetOffsetScalor(from , item.MyFlyData.MyFlyDirType);
            Vector3 fromscale = item.transform.localScale;
            //Vector3 toscale = item.transform.localScale * 3.0f;
            Action<float> _delta = (value) =>
            {
                Vector3 pos = Vector3.Lerp(from, to, item.MyFlyData.FlyPosCurve.Evaluate(value));
                pos += offset * item.MyFlyData.FlyPosOffsetCurve.Evaluate(value);
                item.transform.position = pos;
                item.transform.localScale = fromscale * item.MyFlyData.FlyScaleCurve.Evaluate(value);//Vector3.Lerp(fromscale, toscale, item.MyFlyData.FlyScaleCurve.Evaluate(value));

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
            ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(item.MyFlyData.FlyDelay , () =>
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

        public static void MultipleFlyProccedual(List<FlyItem> items 
             ,Vector3 from , Vector3 to , List<Action> singleFlyDoneActions, Action callback = null)
        {   
            int count = items.Count - 1;
            if(count > 0)
            {
                
                for(int i = 0; i <= count; i++) 
                {
                    if(items[i].gameObject.activeInHierarchy && items[i].gameObject.activeSelf)
                    {
                        items[i].gameObject.SetActive(false);
                    }
                    items[i].MyFlyData.FlyDelay =  
                        MULTI_SHOOT_DURATION*((count - i)/(count + 1)) + MULTI_FLY_INTERVAL * (count - i) ;
                }
            }
            ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoActionWithInterval(items.Count , MULTI_SHOOT_DURATION/items.Count , (index) =>
            {
                //items[index].MyFlyData.FlyDelay = (shootInterval) * (items.Count - index) + items[index].MyFlyData.ShootDuraiotn * ((items.Count - index) /(float)items.Count);
                FlyProccedual(items[index] , from , to , singleFlyDoneActions[index] += () => {items[index].MyFlyData.FlyDelay =0.0f; });
            } , callback));
        }

        public static void MultipleFlyProccedual(List<FlyItem> items  
            ,Vector3 from , Vector3 to , List<Action> singleFlyDoneActions ,float timeoffset , Action callback = null)
        {
            float amount = 0.0f;
            for(int i = 0; i < items.Count; i++)  
            {
                amount += timeoffset * i;
                
                var img = items[i].TargetGraphic;
                if(img != null)
                    img.color = new Color(amount, 1f, 1f, 1f); 
            }
            MultipleFlyProccedual(items  , from , to ,singleFlyDoneActions, callback);
        }

        public static void MultipleFlyProccedual(List<FlyItem> items 
            , Vector3 from, Vector3 to, List<Action> singleFlyDoneActions, Vector2 randomRange, Action callback = null)
        {
            for(int i = 0; i < items.Count; i++) 
            {
                var img = items[i].TargetGraphic;
                if(img != null)
                    img.color = new Color(UnityEngine.Random.Range(randomRange.x , randomRange.y), 1f, 1f, 1f); 
            }
            MultipleFlyProccedual(items  , from , to , singleFlyDoneActions , callback);
        }
    }
}
