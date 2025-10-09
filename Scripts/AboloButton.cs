using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AboloLib
{
    public class AboloButton : Button
    {
        [SerializeField] Animation _animation;
        [SerializeField] Animator _animator;
        UnityEvent onPointerEnter = new UnityEvent();
        public UnityEvent OnEnter
        {
            get => onPointerEnter;
            set => onPointerEnter = value;
        }
        UnityEvent onPointerExit = new UnityEvent();
        public UnityEvent OnExit
        {
            get => onPointerExit;
            set => onPointerExit = value;
        }

        protected override void Awake()
        {
            base.Awake();
            transform.TryGetComponent(out _animation);
            transform.TryGetComponent(out _animator);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            var sgo = transform.Find("root/selected");
            if (sgo != null) sgo.gameObject.SetActive(true);

            var sgo_bg = transform.Find("root/selected_bg");
            if (sgo_bg != null) sgo_bg.gameObject.SetActive(true);

            if (_animation != null)
            {
                if (_animation.isPlaying) _animation.Stop();
                _animation.Play("ani_selectable_enter");
            }

            if (_animator != null && !_animator.GetBool("enter"))
            {
                _animator.SetBool("enter" , true);
            }

            OnEnter?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            var sgo = transform.Find("root/selected");
            if (sgo != null) sgo.gameObject.SetActive(false);

            var sgo_bg = transform.Find("root/selected_bg");
            if (sgo_bg != null) sgo_bg.gameObject.SetActive(false);

            if (_animation != null)
            {
                if (_animation.isPlaying) _animation.Stop();
                _animation.Play("ani_selectable_exit");
                _animation.PlayQueued("ani_selectable_idle");
            }

            if (_animator != null && _animator.GetBool("enter"))
            {
                _animator.SetBool("enter", false);
            }

            OnExit?.Invoke();
        }
    }
}
