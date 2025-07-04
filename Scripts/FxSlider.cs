using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class FxSlider : Slider
    {
        [SerializeField] bool hideFillOnLowValue = false;
        [SerializeField] Slider parentSlider;
        protected override void Awake()
        {
            base.Awake();
            parentSlider = transform.parent.GetComponent<Slider>();
            parentSlider.onValueChanged.AddListener((f) =>
            {
                StopAllCoroutines();

                //某些特殊效果需要再低值阶段，暂定低于5%隐藏填充条
                if (hideFillOnLowValue)
                {
                    var fill = transform.Find("Fill Area/Fill").gameObject;
                    if (f <= 0.05f)
                    {
                        fill.SetActive(false);
                    }
                    else
                    {
                        if (!fill.activeSelf) fill.SetActive(true);
                    }
                }

                float from = value;
                StartCoroutine(ArtAnimation.DoAnimation(0.5f , (t) =>
                {
                    this.value = Mathf.Lerp(from , f , t * t);
                },() => this.transform.Find("Fill Area/Fill").gameObject.SetActive(this.value != 0)));
            });
        }
    }
}
