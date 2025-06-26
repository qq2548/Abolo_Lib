using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    [CreateAssetMenu(menuName = "ArtUtils/CurveFactory")]
    public class CurveFactory : ScriptableObject
    {
        public enum CurveType
        {
            Pop,
            OnTapPop,
            Shrink,
            FlyPosition,
            FlyScale,
            MergeScale,
            MatchScale,
            SlowSteady,
            ExitPop,
            EnterPopX,
            EnterPopY,
            EnterPosition,
            CollectScale,
            TextPopScale,
            OnBoardPop,
            FloatingPosition,
            DecorationPop,
            Shake,
            Spring,
            FlyAccelerate,
        }
        public List<AnimationCurvePreset> animationCurvePresets;
        /// <summary>
        /// 合成特效延迟，例如灰尘道具合成后延迟0.1f播放清除灰尘特效，Item位置修正缓冲时长
        /// </summary>
        public float durationPreset1 = 0.3f;
        /// <summary>
        /// 0.4---Item从出生开始飞行到播放落地特效的延迟，Item销毁或回收缩放动画时长
        /// </summary>
        public float durationPreset2 = 0.4f;
        /// <summary>
        /// 0.5---合成新Item缩放动画时长
        /// </summary>
        public float durationPreset3 = 0.5f;
        /// <summary>
        /// 1.0---Item点击缩放动画时长
        /// </summary>
        public float durationPreset4 = 1.0f;
        /// <summary>
        /// 1.0---生产机点击缩放动画时长，生产Item飞行时长，默认特效时长
        /// </summary>
        public float durationPreset5 = 1.0f;
        /// <summary>
        /// 1.5---文字tips动画时长
        /// </summary>
        public float durationPreset6 = 1.5f;
        /// <summary>
        /// 2.0---装修特效销毁延迟
        /// </summary>
        public float durationPreset7 = 2.0f;
        /// <summary>
        /// 2.5---装修特效销毁延迟
        /// </summary>
        public float durationPreset8 = 2.5f;
        /// <summary>
        /// 0.75---合成道具收集到订单位置的飞行时长
        /// </summary>
        public float durationPreset9 = 0.75f;

        public AnimationCurvePreset GetCurve(CurveType mType)


        {
            for (int i = 0; i < animationCurvePresets.Count; i++)
            {
                if (animationCurvePresets[i].mType == mType)
                    return animationCurvePresets[i];
            }
            return null;
        }
    }
    
    [System.Serializable]
    public class AnimationCurvePreset
    {
        public CurveFactory.CurveType mType;
        public AnimationCurve curve;
    }
}
