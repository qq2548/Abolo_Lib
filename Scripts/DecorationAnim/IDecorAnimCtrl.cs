using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
   public interface IDecorAnimCtrl
    {
        /// <summary>
        /// 播放脚本装修动画
        /// </summary>
        void Play();

        public abstract float MyUnlockAnimDuration { get; set; }
    }
}