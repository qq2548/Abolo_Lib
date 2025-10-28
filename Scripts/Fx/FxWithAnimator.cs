using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AboloLib
{
    public class FxWithAnimator : FxAnim
    {
        public static string State_Start = "ani_start";
        public static string State_Idle = "ani_idle";
        public static string State_Exit = "ani_exit";
        [SerializeField]
        string _boolname;

        private Animator mAnimator;

        void GetAnimator()
        {
            mAnimator = root.GetComponentInChildren<Animator>(true);
        }
        public override void Play()
        {
            StopAnimation();
            base.Play();
            GetAnimator();
            mAnimator?.SetBool(_boolname, true);
            PlayParticles();
        }

        public override void Stop(Action callback)
        {
            base.Stop(callback);
            GetAnimator();
            mAnimator?.SetBool(_boolname, false);
            StopParticles();
        }

    }

}