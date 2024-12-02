using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public enum GrowAnimationType
    {
        Pop = 0,
        Fadein = 1,
        None = 2
    };
    /// <summary>
    /// 给renderer物体添加动画类型标记
    /// </summary>
    public class ComplexGrowNewDecoration : NewFurnitureDecorAnim
    {
        [SerializeField] float _groupInterval = 0.33f;
        [SerializeField] int _maxDurationGroupIndex;
        [SerializeField] float _maxGroupDuration;
        //string groupNodePath = "root/anim_items";
        //List<float> _growIntervals;
        List<GrowAnimationTypeInfo[]> AnimationGroups;
        //Transform animRoot;
        private void Awake()
        {

#if _ARTEST_PRESENTATION
            //GrowAnimationTypeInfo[] grows = GetComponentsInChildren<GrowAnimationTypeInfo>(true);
            //for (int i = 0; i < grows.Length; i++)
            //{
            //    var name = i.ToString() + "_" + grows[i].name;
            //    grows[i].name = name;
            //}
#endif
        }

        public override void ResetSubItems(float factor)
        {
            base.ResetSubItems(factor);
            GrowAnimationTypeInfo[] grows = GetComponentsInChildren<GrowAnimationTypeInfo>(true);

            foreach (var item in grows)
            {
                
                if (factor == 0f)
                {
                    if (item.AnimType == GrowAnimationType.Pop)
                    {
                        item.TryGetComponent(out SpriteRenderer spriteRenderer);
                        if(spriteRenderer != null)
                            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1.0f);
                        item.transform.localScale = Vector3.zero;
                    }
                }
                else
                {
                    item.transform.localScale = Vector3.one;
                }  
            }
        }


        public override void SetUp()
        {
            base.SetUp();
            //AnimationGroups = new List<GrowAnimationTypeInfo[]>();
            //AnimationGroups.Clear();

            //for (int i = 0; i < spr_root.childCount; i++)
            //{
            //    var infos = spr_root.GetChild(i).GetComponentsInChildren<GrowAnimationTypeInfo>(true);
            //    AnimationGroups.Add(infos);
            //}
        }

        /// <summary>
        ///新增类装修动画周期赋值
        /// </summary>
        public override void SetUnlockDuration()
        {
            //int lastIndex = animRoot.childCount - 1;

            //float lastGroupduration = CurveFactoryAdapter.CurveFactory.durationPreset3 * 
            //                        (1.0f + animRoot.GetChild(lastIndex).GetComponentsInChildren<Renderer>(true).Length * _interval);
            //float offsetGroupsDuration = (animRoot.childCount - 1) * _groupInterval;


            _maxGroupDuration = GetMaxDurationGroupDuration(out _maxDurationGroupIndex);

            //这个1.5应该是为了粒子动画播放完毕预留的延迟
            _myUnlockDuration = 1.5f + _maxGroupDuration
                + CurveAdapter.CurveFactory.durationPreset3 * (1.0f + _popItems.Length * _interval);
        }

        float GetMaxDurationGroupDuration(out int index)
        {
            int groupIndex = 0;
            float duration = 0.0f;
            int[] counts = new int[spr_root.childCount];
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = spr_root.GetChild(i).GetComponentsInChildren<Renderer>().Length;
                
                float t = CurveAdapter.CurveFactory.durationPreset3 * (1.0f + counts[i] * _interval) + (i) * _groupInterval;
                if (duration < t)
                {
                    duration = t;
                    groupIndex = i;
                }
            }
            index = groupIndex;

            //duration += index * _groupInterval;
            return duration;
        }

        public override void Play()
        {
            //播放一个BranNew音效
            if (_playMyAudio)
            {
                AudioPlayerAdapter.PlayAudio("hotspot_BrandNew");
            }

            Action _fadeincallback = new Action(PlayDecorDoneFx);
            StartCoroutine(ComplexGrowAnimation(GetComponentsInChildren<Renderer>(true),
                        CurveAdapter.CurveFactory.durationPreset3, _fadeincallback));
        }

        IEnumerator ComplexGrowAnimation(Renderer[] graphics, float duration, Action callback = null)
        {
            //animRoot 子节点分批次播放自己的子节点Renderer入场动画
            for (int i = 0; i < spr_root.childCount; i++)
            {
                Renderer[] renderers = spr_root.GetChild(i).GetComponentsInChildren<Renderer>(true);
                if (renderers != null && renderers.Length > 0)
                    StartCoroutine(GroupGrow(renderers, duration));
                yield return new WaitForSeconds(_groupInterval);
            }

            yield return new WaitForSeconds(Mathf.Abs(_maxGroupDuration - (spr_root.childCount ) * _groupInterval));

            //pop_items 子节点缩放动画
            if (pop_root != null && _popItems != null && _popItems.Length > 0)
            {
                if (!pop_root.gameObject.activeInHierarchy) pop_root.gameObject.SetActive(true);
                yield return StartCoroutine(DoAnimationWithInterval(_popItems.Length, CurveAdapter.CurveFactory.durationPreset3,
                    _interval , MultiPopDeltaAnimation(_popItems , _interval ,true)));
            }
            callback?.Invoke();
        }

        IEnumerator GroupGrow(Renderer[] graphics, float duration , Action callback = null)
        {
            float timer = 0.0f;
            Vector3[] startPositions = new Vector3[graphics.Length];
            for (int i = 0; i < graphics.Length; i++)
            {
                startPositions[i] = graphics[i].transform.position;
            }

            while (timer <= 1.0f + graphics.Length * _interval)
            {
                timer += Time.deltaTime / duration;
                for (int i = 0; i < graphics.Length; i++)
                {
                    float t = Mathf.Clamp01(timer - _interval * i);
                    //Renderers 通过什么效果入场，由 GrowAnimationTypeInfo 标记
                    if (graphics[i].TryGetComponent( out GrowAnimationTypeInfo aniInfo))
                    {
                       
                        switch (aniInfo.AnimType)
                        {
                            case GrowAnimationType.Pop:
                                graphics[i].transform.localScale = ArtUtility.Pop(timer: t, 
                                    curve: CurveAdapter.AnimCurveDic[CurveFactory.CurveType.DecorationPop]);
                                break;
                            case GrowAnimationType.Fadein:
                                FadeDelta(graphics[i] , startPositions[i] , t);
                                break;
                            case GrowAnimationType.None:
                                graphics[i].transform.localScale = Vector3.one;
                                graphics[i].transform.localPosition = Vector3.zero;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        FadeDelta(graphics[i], startPositions[i], t);
#if _ARTEST_PRESENTATION
                        Debug.Log(ArtUtility.WarningLog + $" {transform.name} 的 {graphics[i].name} 节点没有绑定 GrowAnimationTypeInfo 脚本，如果是有意设置，忽略此条");
#endif
                    }
                }
                yield return null;
            }
            callback?.Invoke();
        }


#if _ARTEST_PRESENTATION
        #region 编辑器方法
        /// <summary>
        /// 添加动画类型标记
        /// </summary>
        public void AddTypeInfoToRenderers()
        {

            Renderer[] renderers = transform.Find("root/anim_items").GetComponentsInChildren<Renderer>(true);
            foreach (var item in renderers)
            {
                if (!item.TryGetComponent(out GrowAnimationTypeInfo growAnimationTypeInfo))
                {
                    item.gameObject.AddComponent<GrowAnimationTypeInfo>();
                }
            }
        }
        /// <summary>
        /// 清除动画类型标记
        /// </summary>
        public void RemoveTypeInfoOfRenderers()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var item in renderers)
            {
                if (item.TryGetComponent(out GrowAnimationTypeInfo growAnimationTypeInfo))
                {
                    DestroyImmediate(growAnimationTypeInfo);
                }
            }
        }
        /// <summary>
        /// 子节点Renderer反序
        /// </summary>
        public void ReverseAnimRootRenderersQueue()
        {
            if (spr_root == null)
            {
                spr_root = transform.Find("root/anim_items");
            }
            for (int a = 0; a < spr_root.childCount; a++)
            {
                var rs = spr_root.GetChild(a).GetComponentsInChildren<Renderer>(true);
                //List<Renderer> lrs = new List<Renderer>();
                for (int b = 0; b < rs.Length; b++)
                {
                    rs[b].transform.SetAsFirstSibling();
                }
            }
        }
        #endregion
#endif

    }
}
