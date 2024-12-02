using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class ArtAnimation : MonoBehaviour
    {
        /// <summary>
        /// 默认动画开启clip名称
        /// </summary>
        public static string StartAnimeName = "ani_open";
        /// <summary>
        /// 默认动画关闭clip名称
        /// </summary>
        public static string StopAnimeName = "ani_close";


        /// <summary>
        /// 用于存储需要中断的协程动画
        /// </summary>
        internal Coroutine ani;
        public static bool _initialized = false;
        //--------------- status --------------
        internal bool IsPlaying { get { return ani != null; } }

        public virtual void Start()
        {
            Init();
        }

        public virtual void Init()
        {
            _initialized = true;
        }


        public void StopAnimation()
        {
            if (IsPlaying)
            {
                StopCoroutine(ani);
                ani = null;
            }
        }

        /// <summary>
        /// 颜色透明渐变动画变化片段
        /// </summary>
        /// <param name="renderer">需要动画效果的Renderer</param>
        /// <param name="startPosition">动画起始位置</param>
        /// <param name="proccess">动画进度</param>
        /// <param name="isFadeIn">是否为淡入效果，false则是淡出效果</param>
        public static void FadeDelta(Renderer renderer, Vector3 startPosition, float proccess, bool isFadeIn = true)
        {
            SpriteRenderer sp;
            renderer.TryGetComponent(out sp);
            if (!isFadeIn)
            {
                Vector3 fromPos = startPosition;
                Vector3 toPos = startPosition + Vector3.up;
                if (sp != null)
                {
                    Color oriColor = sp.color;
                    sp.color = ArtUtility.FadeOut(oriColor, proccess);
                }
                renderer.transform.position = ArtUtility.Move(fromPos, toPos, proccess);
                renderer.transform.localScale = ArtUtility.Pop(proccess, 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.ExitPop]);
            }
            else
            {
                Vector3 fromPos = startPosition + Vector3.up;
                Vector3 toPos = startPosition;
                if (sp != null)
                {
                    Color oriColor = sp.color;
                    sp.color = ArtUtility.FadeIn(oriColor, proccess);
                }
                renderer.transform.position = ArtUtility.Move(fromPos, toPos, proccess, 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.EnterPosition]);
                renderer.transform.localScale = ArtUtility.Pop(proccess, 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.EnterPopX], 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.EnterPopY]);
            }
        }
        /// <summary>
        /// UI渲染的透明过渡动画
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="startPosition"></param>
        /// <param name="proccess"></param>
        /// <param name="isFadeIn"></param>
        public static void FadeDelta(Graphic graphic, Vector3 startPosition, float proccess, bool isFadeIn = true)
        {
            Graphic gh;
            graphic.TryGetComponent(out gh);
            if (!isFadeIn)
            {
                Vector3 fromPos = startPosition;
                Vector3 toPos = startPosition + Vector3.up;
                if (gh != null)
                {
                    Color oriColor = gh.color;
                    gh.color = ArtUtility.FadeOut(oriColor, proccess);
                }
                graphic.transform.position = ArtUtility.Move(fromPos, toPos, proccess);
                graphic.transform.localScale = ArtUtility.Pop(proccess, 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.ExitPop]);
            }
            else
            {
                Vector3 fromPos = startPosition + Vector3.up;
                Vector3 toPos = startPosition;
                if (gh != null)
                {
                    Color oriColor = gh.color;
                    gh.color = ArtUtility.FadeIn(oriColor, proccess);
                }
                graphic.transform.position = ArtUtility.Move(fromPos, toPos, proccess, 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.EnterPosition]);
                graphic.transform.localScale = ArtUtility.Pop(proccess, 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.EnterPopX], 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.EnterPopY]);
            }
        }

        /// <summary>
        /// 多物体颜色透明渐变动画片段
        /// </summary>
        /// <param name="renderers">需要动画效果的Renderers</param>
        /// <param name="startPositions">动画起始位置</param>
        /// <param name="isFadeIn">是否为淡入效果，false则是淡出效果</param>
        /// <returns></returns>
        public static Action<float> MultiFadeDeltaAnimation(Renderer[] renderers, Vector3[] startPositions, float interval, bool isFadeIn = true)
        {

            Action<float> _deltaAnimation = (value) =>
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    float t = Mathf.Clamp01(value - interval * i);
                    FadeDelta(renderers[i], startPositions[i], t, isFadeIn);
                }
            };
            return _deltaAnimation;
        }

        /// <summary>
        /// 缩放变形动画片段
        /// </summary>
        /// <param name="item">需要动画效果的物体</param>
        /// <param name="proccess">动画进度</param>
        /// <param name="isPopIn">是否为放大入场效果，false则是缩小出场效果</param>
        public static void PopDelta(Transform item, float proccess, bool isPopIn)
        {
            if (isPopIn)
            {
                item.transform.localScale = ArtUtility.Pop(proccess, 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.DecorationPop]);
            }
            else
            {
                item.transform.localScale = ArtUtility.Pop(proccess, 
                    CurveAdapter.AnimCurveDic[CurveFactory.CurveType.Shrink]);
            }
        }

        /// <summary>
        /// 多物体变形动画片段
        /// </summary>
        /// <param name="renderers">需要动画效果的Renderers</param>
        /// <param name="IsPopIn">是否为放大入场效果，false则是缩小出场效果</param>
        /// <returns></returns>
        public static Action<float> MultiPopDeltaAnimation<T>(T[] renderers, float interval, bool IsPopIn) where T : Component
        {
            Action<float> _deltaAnimation = (value) =>
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    float t = Mathf.Clamp01(value - interval * i);
                    PopDelta(renderers[i].transform, t, IsPopIn);
                }
            };
            return _deltaAnimation;
        }


        public static IEnumerator DoAnimation(float duration , Action<float> deltaAnimation , Action callback = null)
        {
            float timer = 0.0f;
            while (timer <= 1.0f)
            {
                timer += Time.deltaTime / duration;
                deltaAnimation?.Invoke(timer);
                yield return null;
            }
            callback?.Invoke();
        }

        /// <summary>
        /// 多物体带间隔的动画循环
        /// </summary>
        /// <param name="count">物体数量</param>
        /// <param name="duration">动画周期</param>
        /// <param name="deltaAnimation">动画效果片段方法</param>
        /// <param name="callback">动画结束时回调</param>
        /// <returns></returns>
        public static IEnumerator DoAnimationWithInterval(int count, float duration, float interval, Action<float> deltaAnimation, Action callback = null)
        {
            float timer = 0.0f;
            while (timer <= 1.0f + count * interval)
            {
                timer += Time.deltaTime / duration;
                deltaAnimation?.Invoke(timer);
                yield return null;
            }
            callback?.Invoke();
        }

        public static IEnumerator ArtAnimDelayCoroutine(float delay , Action callback)
        {
            yield return new WaitForSeconds(delay);

            callback?.Invoke();
        }

    }

}
