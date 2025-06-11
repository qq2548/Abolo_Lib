using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AboloLib
{
    public class CurrencyCollectTest : MonoBehaviour
    {
        //[SerializeField]UICurrencyCollect currencyCollect;
        public Transform target;
        public int count;
        [SerializeField] int id;
        [SerializeField] TMP_Text number;
        [SerializeField] bool overrideBoomRange;
        [SerializeField] float range;
        [SerializeField] bool _playAutomatically = false;
        [SerializeField] UICurrencyCollect spawnerInstance;
        /// <summary>
        /// 0 : Animation, 1: Slider
        /// </summary>
        [SerializeField]int _targetAnimationState = 0;

        private void Awake()
        {
            if (number == null)
            {
                number = target.GetComponentInChildren<TMP_Text>();
            }
        }

        private void OnEnable()
        {
            if (_playAutomatically)
            {
                PlayAnimation();
            }
        }

        public void PlayAnimation()
        {
            //target.localScale = Vector3.zero;
            if (!target.gameObject.activeInHierarchy)
            {
                target.gameObject.SetActive(true);
            }

            ///收集动画调用开始
            if (spawnerInstance == null)
            {
                spawnerInstance = (CurrencyCollectManager.CollectSpawmer);
            }
            //生成器位置设置到出生点
            //spawnerInstance.transform.position = transform.position;
            //调用生成器实例的播放动画方法
            if (!overrideBoomRange)//是否重新设置喷发范围
            {
                spawnerInstance.BoombToCollectCurrency(id, count, transform, target.position );
            }
            else
            {
                spawnerInstance.BoombToCollectCurrency(id, count, transform, target.position , range );
            }
            ///收集动画调用结束

            //击中目标的联动动画效果，目前仅作为配合展示
            if (number != null)
            {
                number.text = "156";
            }
            StartCoroutine(PlayTargetAnimation(count, spawnerInstance.GetHitingInterval()));
            StartCoroutine(Fadeout(spawnerInstance));
        }

        IEnumerator Fadeout(UICurrencyCollect collect)
        {
            if (TryGetComponent(out Image image))
            {
                image.enabled = false;
                yield return new WaitForSeconds(collect.GetDelayFromStartToHit(count));
                image.enabled = true;
            }
        }

        private void TargetAnimation()
        {
            switch (_targetAnimationState)
            {
                case 0:
                    if (target.TryGetComponent(out Animation animation))
                    {
                        if (!target.gameObject.activeInHierarchy)
                        {
                            target.gameObject.SetActive(true);
                        }
                        if (target.localScale == Vector3.zero)
                        {
                            target.localScale = Vector3.one;
                        }

                        if (animation.isPlaying)
                        {
                            animation.Stop();
                        }
                        animation.Play();
                        Debug.Log("ani_played");
                    }
                    break;
                case 1:
                    if (target.TryGetComponent(out Slider slider))
                    {
                        slider.value += 0.01f;

                        var ani = target.Find("Fill Area/mix_ui_slider_highlight").GetComponent<Animation>();
                        var img = target.Find("Fill Area/mix_ui_slider_highlight").GetComponent<Image>();
                        img.fillAmount = slider.value;
                        if (ani != null)
                        {
                            ani.Play();
                        }
                    }
                    break;

            }

        }
        IEnumerator PlayTargetAnimation(int count , float interval , Action action = null)
        {
            yield return new WaitForSeconds(spawnerInstance.GetDelayFromStartToHit(count));
            for (int i = 0; i < count; i++)
            {
                TargetAnimation();
                if (number != null)
                {
                    number.text = (156 + i + 1).ToString();
                }
#if _ARTEST_PRESENTATION
                Debug.Log(i);
#endif
                yield return new WaitForSeconds(interval);
            }
            yield return null;
            action?.Invoke();
        }
    }

}