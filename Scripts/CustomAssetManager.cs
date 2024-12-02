using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class CustomAssetManager : AboloSingleton<CustomAssetManager>
    {
        [SerializeField]
        protected CurveFactory curveFactory;
        public CurveFactory CurveFactory
        {
            get => curveFactory;
            set => curveFactory = value;
        }
        protected Dictionary<CurveFactory.CurveType, AnimationCurve> animCurveDic;
        /// <summary>
        /// 曲线字典
        /// </summary>
        public Dictionary<CurveFactory.CurveType, AnimationCurve> AnimCurveDic
        {
            get => animCurveDic;
            set => animCurveDic = value;
        }
        [SerializeField]
        protected AudioFactory audioFactory;
        protected Dictionary<string, AudioClip> audioDic;
        /// <summary>
        /// 音效字典
        /// </summary>
        public Dictionary<string, AudioClip> AudioDic
        {
            get => audioDic;
            set => audioDic = value;
        }

        public override void Init()
        {
            base.Init();
            //加载动画曲线预设资源
            if (curveFactory == null)
            {
                curveFactory = Resources.Load<CurveFactory>("CustomAssets/AnimationCurvePresets");
            }
            //动画曲线字典赋值
            if (AnimCurveDic == null)
            {
                AnimCurveDic = new Dictionary<CurveFactory.CurveType, AnimationCurve>();
                AnimCurveDic.Clear();
                foreach (var item in curveFactory.animationCurvePresets)
                {
                    AnimCurveDic.Add(item.mType, item.curve);
                }
#if _ARTEST_PRESENTATION
                Debug.Log("---------------CurveFactory 字典赋值完毕");
#endif
            }
            //曲线转接器初始化
            CurveAdapter.Init(() => { return AnimCurveDic; } , () => { return CurveFactory; });
            //加载音效预设资源
            if (audioFactory == null)
            {
                audioFactory = Resources.Load<AudioFactory>("CustomAssets/AudioPresets");
            }
            //音效字典赋值
            if (AudioDic == null && audioFactory != null)
            {
                AudioDic = new Dictionary<string, AudioClip>();
                AudioDic.Clear();
                foreach (var item in audioFactory.AudioPresets)
                {
                    AudioDic.Add(item._mName.ToString(), item.mAudioClip);
                }
#if _ARTEST_PRESENTATION
                Debug.Log("---------------AudioFactory 字典赋值完毕");
#endif
            }
            //音效转接器初始化
            AudioPlayerAdapter.Init(AudioPlayer._instance, () => AudioDic); ;
        }

    }
}
