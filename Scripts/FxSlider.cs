using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class FxSlider : Slider
    {
        [SerializeField]
        Slider parentSlider;
        protected override void Awake()
        {
            base.Awake();
            parentSlider = transform.parent.GetComponent<Slider>();
            parentSlider.onValueChanged.AddListener((f) =>
            {
                StopAllCoroutines();
                float from = value;
                StartCoroutine(ArtAnimation.DoAnimation(0.5f , (t) =>
                {
                    this.value = Mathf.Lerp(from , f , t * t);
                }));
            });
        }
    }
}
