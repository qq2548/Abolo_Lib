#define USE_SPINE
#define USE_TMPRO

//根据项目情况注释
//#undef USE_SPINE
//#undef USE_TMPRO

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if USE_SPINE
using Spine;
using Spine.Unity;
#endif
#if USE_TMPRO
using TMPro;
#endif

namespace AboloLib
{
    public static class TransformExtension
    {
        /// <summary>
        /// transform 世界坐标位移动画扩展方法
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="from">起点位置坐标</param>
        /// <param name="to">终点位置坐标</param>
        /// <param name="duration">动画时长</param>
        /// <param name="curve">动画采样曲线</param>
        /// <param name="callback">回调</param>
        /// <returns>动画协程</returns>
        public static Coroutine MoveTo(this Transform transform , 
            Vector3 from , Vector3 to , float duration , AnimationCurve curve = null ,Action callback = null)
        {
            if (curve == null) curve = ArtUtility.IncreaseLinearCurve;
            Action<float> _deltaAnim = (value) =>
            {
                transform.position = Vector3.Lerp(from, to, curve.Evaluate(value));
            };
            return ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoAnimation(duration , _deltaAnim , callback));
        }

        /// <summary>
        /// transform 本地坐标位移动画扩展方法
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="from">起点位置坐标</param>
        /// <param name="to">终点位置坐标</param>
        /// <param name="duration">动画时长</param>
        /// <param name="curve">动画采样曲线</param>
        /// <param name="callback">回调</param>
        /// <returns>动画协程</returns>
        public static Coroutine LocalMoveTo(this Transform transform ,
            Vector3 from, Vector3 to, float duration, AnimationCurve curve = null, Action callback = null)
        {
            if (curve == null) curve = ArtUtility.IncreaseLinearCurve;
            Action<float> _deltaAnim = (value) =>
            {
                transform.localPosition = Vector3.Lerp(from, to, curve.Evaluate(value));
            };
            return ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoAnimation(duration, _deltaAnim, callback));
        }

        /// <summary>
        /// transform 本地坐标位移动画扩展方法
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="from">起始缩放大小</param>
        /// <param name="to">结束缩放大小</param>
        /// <param name="duration">动画时长</param>
        /// <param name="curve">动画采样曲线</param>
        /// <param name="callback">回调</param>
        /// <returns>动画协程</returns>
        public static Coroutine ScaleTo(this Transform transform , 
            Vector3 from, Vector3 to, float duration, AnimationCurve curve = null, Action callback = null)
        {
            if (curve == null) curve = ArtUtility.IncreaseLinearCurve;
            Action<float> _deltaAnim = (value) =>
            {
                transform.localScale = Vector3.Lerp(from, to, curve.Evaluate(value));
            };
            return ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoAnimation(duration, _deltaAnim, callback));
        }

        /// <summary>
        /// transform 子节点带间隔时间的位移动画扩展方法
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="interval">动画开始的间隔</param>
        /// <param name="froms">起点坐标数组</param>
        /// <param name="tos">终点坐标数组</param>
        /// <param name="duration">动画时长</param>
        /// <param name="curve">动画采样曲线</param>
        /// <param name="callback">回调</param>
        /// <returns>动画协程</returns>
        public static Coroutine ChildrenMoveToWithInterval(this Transform transform , float interval , 
            Vector3[] froms, Vector3[] tos, float duration, AnimationCurve curve = null, Action callback = null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                float delay = interval * i;
                ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(delay , () => 
                {
                    transform.GetChild(i).MoveTo(froms[i] , tos[i] , duration , curve);
                }));
            }
            float totalDuration = duration + interval * (transform.childCount - 1);
            return ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(totalDuration , callback));
        }

        /// <summary>
        /// transform 子节点带间隔时间的缩放动画扩展方法
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="interval">动画开始的间隔</param>
        /// <param name="froms">起点缩放大小数组</param>
        /// <param name="tos">终点缩放大小数组</param>
        /// <param name="duration">动画时长</param>
        /// <param name="curve">动画采样曲线</param>
        /// <param name="callback">回调</param>
        /// <returns>延迟协程</returns>
        public static Coroutine ChildrenScaleToWithInterval(this Transform transform, float interval,
            Vector3[] froms, Vector3[] tos, float duration, AnimationCurve curve = null, Action callback = null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                float delay = interval * i;
                ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(delay, () =>
                {
                    transform.GetChild(i).ScaleTo(froms[i], tos[i], duration, curve);
                }));
            }
            float totalDuration = duration + interval * (transform.childCount - 1);
            return ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(totalDuration, callback));
        }

        /// <summary>
        /// transform 子节点带间隔时间的缩放动画扩展方法，带子动画回调的重写
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="interval">动画开始的间隔</param>
        /// <param name="froms">起点缩放大小数组</param>
        /// <param name="tos">终点缩放大小数组</param>
        /// <param name="duration">动画时长</param>
        /// <param name="subCallbacks">子节点动画完成时回调数组</param>
        /// <param name="curve">动画采样曲线</param>
        /// <param name="callback">回调</param>
        /// <returns>延迟协程</returns>
        public static Coroutine ChildrenScaleToWithInterval(this Transform transform, float interval,
            Vector3[] froms, Vector3[] tos, float duration, Action[] subCallbacks, AnimationCurve curve = null, Action callback = null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                float delay = interval * i;
                ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(delay, () =>
                {
                    transform.GetChild(i).ScaleTo(froms[i], tos[i], duration, curve, subCallbacks[i]);
                }));
            }
            float totalDuration = duration + interval * (transform.childCount - 1);
            return ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(totalDuration, callback));
        }

        public static bool TryPlayAnimation(this Transform transform , string animationName)
        {
            if (transform.TryGetComponent(out UnityEngine.Animation animation))
            {
                if (animation.isPlaying) animation.Stop();
                animation.Play(animationName);
                return true;
            }
            else
            {
#if _ARTEST_PRESENTATION
                Debug.LogWarning($"{transform.gameObject.name}------没有Animation组件，无法播放！");
#endif
                return false;
            }
        }
    }

    public static class GraphicExtension
    {
        /// <summary>
        /// UI图像组件颜色渐变动画扩展方法
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="from">起始颜色值</param>
        /// <param name="to">结束颜色值</param>
        /// <param name="duration">动画时长</param>
        /// <param name="curve">动画采样曲线</param>
        /// <param name="callback">回调</param>
        /// <returns>动画协程</returns>
        public static Coroutine ColorGradient(this Graphic graphic,
           Color from, Color to, float duration, AnimationCurve curve = null, Action callback = null)
        {

            if (curve == null) curve = ArtUtility.IncreaseLinearCurve;
            Action<float> _deltaAnim = (value) =>
            {
                graphic.color = Color.Lerp(from, to, curve.Evaluate(value));
            };
            return ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoAnimation(duration, _deltaAnim, callback));
        }
    }

    public static class SpriteRendererExtension
    {
        /// <summary>
        /// 图片精灵组件颜色渐变动画扩展方法
        /// </summary>
        /// <param name="spriteRenderer"></param>
        /// <param name="from">起始颜色值</param>
        /// <param name="to">结束颜色值</param>
        /// <param name="duration">动画时长</param>
        /// <param name="curve">动画采样曲线</param>
        /// <param name="callback">回调</param>
        /// <returns>动画协程</returns>
        public static Coroutine ColorGradient(this SpriteRenderer spriteRenderer,
            Color from, Color to, float duration, AnimationCurve curve = null, Action callback = null)
        {
            if (curve == null) curve = ArtUtility.IncreaseLinearCurve;
            Action<float> _deltaAnim = (value) =>
            {
                spriteRenderer.color = Color.Lerp(from, to, curve.Evaluate(value));
            };
            return ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoAnimation(duration, _deltaAnim, callback));
            
        }
    }

#if USE_SPINE
    public static class SkeletonAnimationExtension
    {
        /// <summary>
        /// Spine 动画组件播放扩展方法
        /// </summary>
        /// <param name="skeletonAnimation"></param>
        /// <param name="clipName">动画名称</param>
        /// <param name="regenerateMesh">是否重新生成mesh</param>
        /// <param name="loop">循环</param>
        public static void Play(this SkeletonAnimation skeletonAnimation, string clipName, bool regenerateMesh = false, bool loop = false)
        {
            if (regenerateMesh) skeletonAnimation.Initialize(regenerateMesh);
            skeletonAnimation.AnimationState.SetAnimation(0 , clipName , loop);
        }
        /// <summary>
        /// Spine 动画组件添加动画播放扩展方法
        /// </summary>
        /// <param name="skeletonAnimation"></param>
        /// <param name="clipName">动画名称</param>
        /// <param name="loop">循环</param>
        public static void AddClipToPlay(this SkeletonAnimation skeletonAnimation, string clipName, bool loop = false)
        {
            float delay = skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration;
            skeletonAnimation.AnimationState.AddAnimation(0, clipName, loop, delay - skeletonAnimation.skeletonDataAsset.defaultMix);
        }
        /// <summary>
        /// Spine 动画组件播放一组动画的扩展方法
        /// </summary>
        /// <param name="skeletonAnimation"></param>
        /// <param name="clipNames">动画名称数组</param>
        /// <param name="regenerateMesh">初始动画播放前是否重建mesh</param>
        /// <param name="loop">最后一个动画是否循环</param>
        public static void PlaySerial(this SkeletonAnimation skeletonAnimation, string[] clipNames, bool regenerateMesh = false, bool loop = false)
        {
            if (regenerateMesh) skeletonAnimation.Initialize(regenerateMesh);
            skeletonAnimation.AnimationState.SetAnimation(0, clipNames[0], loop);
            for (int i = 1; i < clipNames.Length; i++)
            {
                SkeletonData data = skeletonAnimation.SkeletonDataAsset.GetSkeletonData(false);
                float delay = data.FindAnimation(clipNames[i-1]).Duration;
                bool _loop = i == clipNames.Length - 1 ? loop : false;
                skeletonAnimation.AnimationState.AddAnimation(0, clipNames[i], _loop, delay);
            }
        }
    }

    public static class SkeletonGraphicExtension
    {
        /// <summary>
        /// Spine UI绘制组件动画播放扩展方法
        /// </summary>
        /// <param name="skeletonGraphic"></param>
        /// <param name="clipName">动画名称</param>
        /// <param name="regenerateMesh">是否重新生成mesh</param>
        /// <param name="loop">循环</param>
        public static void Play(this SkeletonGraphic skeletonGraphic, string clipName, bool regenerateMesh = false, bool loop = false)
        {
            if (regenerateMesh) skeletonGraphic.Initialize(regenerateMesh);
            skeletonGraphic.AnimationState.SetAnimation(0, clipName, loop);
        }
        /// <summary>
        ///  Spine UI绘制组件添加动画播放的扩展方法
        /// </summary>
        /// <param name="skeletonGraphic"></param>
        /// <param name="clipName">动画名称</param>
        /// <param name="loop">循环</param>
        public static void AddClipToPlay(this SkeletonGraphic skeletonGraphic, string clipName, bool loop = false)
        {
            float delay = skeletonGraphic.AnimationState.GetCurrent(0).Animation.Duration;
            skeletonGraphic.AnimationState.AddAnimation(0, clipName, loop, delay - skeletonGraphic.skeletonDataAsset.defaultMix);
        }
        /// <summary>
        /// Spine UI绘制组件播放一组动画的扩展方法
        /// </summary>
        /// <param name="skeletonGraphic"></param>
        /// <param name="clipNames">动画名称数组</param>
        /// <param name="regenerateMesh">初始动画播放前是否重建mesh</param>
        /// <param name="loop">最后一个动画是否循环</param>
        public static void PlaySerial(this SkeletonGraphic skeletonGraphic, string[] clipNames, bool regenerateMesh = false, bool loop = false)
        {
            if (regenerateMesh) skeletonGraphic.Initialize(regenerateMesh);
            skeletonGraphic.AnimationState.SetAnimation(0, clipNames[0], loop);
            for (int i = 1; i < clipNames.Length; i++)
            {
                SkeletonData data = skeletonGraphic.SkeletonDataAsset.GetSkeletonData(false);
                float delay = data.FindAnimation(clipNames[i - 1]).Duration;
                bool _loop = i == clipNames.Length - 1 ? loop : false;
                skeletonGraphic.AnimationState.AddAnimation(0, clipNames[i], _loop, delay);
            }
        }
    }
#endif

#if USE_TMPRO
    public static class TextMeshProUGUIExtension
    {
        public static void DoTextAnimation(this TextMeshProUGUI tmp , int from , int to , float duration = 0.5f)
        {
            Action<float> _delta = (value) =>
            {
                int result =Mathf.RoundToInt(Mathf.Lerp(from, to, value));
                tmp.text = result.ToString();
            };
            ScheduleAdapter.Schedual.StartCoroutine(ArtAnimation.DoAnimation(duration , _delta));
        }
    }
#endif
}




