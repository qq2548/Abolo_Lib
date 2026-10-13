using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AboloLib
{
    public class UI_Selectable : MonoBehaviour, 
        IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler, IPointerUpHandler , IBeginDragHandler
    {
        public Action PointerDown;
        public Action PointerEnter;
        public Action PointerExit;
        public Action PointerClick;
        public Action PointerUp;
        public Action BeginDrag;

        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDrag?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDown?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExit?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUp?.Invoke();
        }
    }
}
