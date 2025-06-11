using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AboloLib
{
    /// <summary>
    /// Animation 组件需要绑在root上，不要绑在根节点
    /// </summary>
    public class FxWithAnimation : FxAnim
    {

        protected Animation mAnimation;

        void GetAnimation()
        {
            if (mAnimation == null)
            {
                mAnimation = root.GetComponentInChildren<Animation>(true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override  void Play()
        {
            base.Play();
            GetAnimation();
            if (mAnimation != null)
            {
                if (mAnimation.isPlaying) mAnimation.Stop();
                mAnimation[StartAnimeName].time = 0;
                    mAnimation.Play(StartAnimeName);
            }
        }

        public override void Stop(Action callback)
        {
            base.Stop(callback);
            GetAnimation();
            if (mAnimation != null && mAnimation.GetClipCount() > 1)
            {
                if(mAnimation.GetClip(StopAnimeName) != null)
                    mAnimation.Play(StopAnimeName);
            }
        }

    }

}