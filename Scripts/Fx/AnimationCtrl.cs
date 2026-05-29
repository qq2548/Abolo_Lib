using System;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace AboloLib
{
    public class AnimationCtrl : MonoBehaviour
    {
        [SerializeField] Transform target;
        public void PlayAniamtion(string animName)
        {
            if(target != null) PlayTargetAnimation(target , animName); 
        }
        //需要兼容的类型 Animator Animation SkeletonAnimation  SkeletonGraphic 
        public static void PlayTargetAnimation(Transform target , string animName)
        {
            var cmpts = target.GetComponents(typeof(Component));
            if(cmpts != null && cmpts.Length > 0)
            {
                foreach (var cmpt in cmpts)
                {
                    if(TryPlayComponetAnimation(cmpt , animName)) break;
                }
            }
        }

        public static bool TryPlayComponetAnimation(Component component , string animName)
        {
            if(component is Animator)
            {
                var anim = component as Animator;
                anim.Play(animName);
                 return true;
            }          
            if(component is Animation)
            {
                var anim = component as Animation;
                anim.Play(animName);
                 return true;
            }
            if(component is SkeletonAnimation)
            {
                var anim = component as SkeletonAnimation;
                anim.Play(animName);
                 return true;
            }
            if(component is SkeletonGraphic)
            {
                var anim = component as SkeletonGraphic;
                anim.Play(animName);
                 return true;
            }
            return false;
        }
    }
}
