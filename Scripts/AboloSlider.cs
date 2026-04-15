using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class AboloSlider : Slider
    {
        float currentValue = 0.0f;
        protected override void Awake()
        {
            base.Awake();
            onValueChanged.RemoveAllListeners();
            onValueChanged.AddListener((v) =>
            {   
                StopAllCoroutines();
                if(Application.isPlaying)
                {
                    StartCoroutine(ArtAnimation.DoAnimation(0.33f , (f) =>
                    {
                        this.value = Mathf.Lerp(currentValue , v , f);
                    } , () => currentValue = v));
                }
            });
        }
    }
}
