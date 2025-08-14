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
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            var sgo = transform.Find("root/selected");
            if (sgo != null) sgo.gameObject.SetActive(true);
            if (_animation != null)
            {
                if (_animation.isPlaying) _animation.Stop();
                _animation.Play("ani_selectable_enter");
            }
            OnEnter?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            var sgo = transform.Find("root/selected");
            if (sgo != null) sgo.gameObject.SetActive(false);
            if (_animation != null)
            {
                if (_animation.isPlaying) _animation.Stop();
                _animation.Play("ani_selectable_exit");
                _animation.PlayQueued("ani_selectable_idle");
            }
            OnExit?.Invoke();
        }
    }
}
