using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace AboloLib
{
    public class FlyFxItemCtrl : MonoBehaviour,IBeginDragHandler, IDragHandler,IEndDragHandler
    {
        [Header("飞行起点，可拖拽移动")]
        [SerializeField] Transform from;
        [Header("飞行终点，可拖拽移动")]
        [SerializeField] Transform to;

        [Header("单次发射道具数量")]
        [SerializeField] int itemCounts = 1;
        [Header("发射范围")]
        [SerializeField] float blastRange = 1.0f;

        Camera uiCamera;
        Transform dragTarget = null;
        [PrefabSelect("Assets/ArtWorkSpace/CustomAssets/currency_factory.asset")] 
        [SerializeField] string itemName;
        //int itemIndex = 0;
        private void Start()
        {
            if (dragTarget != null)
            {
                dragTarget = null;
            }

            uiCamera = UICanvasAdapter.CurrentCanvas.worldCamera;//ArtGameManager.instance.UICamera;

            RefreshCountText();

            from.GetComponent<Button>().onClick.AddListener(() => ShootFlyItem());
            from.Find("plusBtn").GetComponent<Button>().onClick.AddListener(()=> 
            {
                itemCounts++;
                RefreshCountText();
            });
            from.Find("minusBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                itemCounts--;
                if (itemCounts < 0)
                {
                    itemCounts = 0;
                }
                RefreshCountText();
            });
        }

        public void ShootFlyItem()
        {
            string indexStr = itemName.Split("|")[0];
            from.GetComponent<CanvasGroup>().alpha = 0.0f;
            CurrencyCollectManager.CollectSpawmer.BoombToCollectCurrency
                (int.Parse(indexStr) , itemCounts , from , to.position, blastRange);
            float delay = CurrencyCollectManager.CollectSpawmer.GetDelayFromStartToHit(itemCounts);
            StartCoroutine(PlayTargetAnimation(itemCounts, CurrencyCollectManager.CollectSpawmer.GetHitingInterval() , to));
            StartCoroutine(ArtAnimation.ArtAnimDelayCoroutine(0.6f , ()=> { from.GetComponent<CanvasGroup>().alpha = 1.0f; }));
        }

        void RefreshCountText()
        {
            from.Find("count_text").GetComponent<TMP_Text>().text = itemCounts.ToString();
        }

        public static void TargetAnimation(Transform target)
        {
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
            }
        }
        IEnumerator PlayTargetAnimation(int count, float interval, Transform target ,Action action = null)
        {
            yield return new WaitForSeconds(CurrencyCollectManager.CollectSpawmer.GetDelayFromStartToHit(count));
            for (int i = 0; i < count; i++)
            {
                TargetAnimation(target);
#if _ARTEST_PRESENTATION
                Debug.Log(i);
#endif
                yield return new WaitForSeconds(interval);
            }
            yield return null;
            action?.Invoke();
        }
    

    public void OnDrag(PointerEventData eventData)
        {
            Move();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var rect1 = from.GetComponent<RectTransform>();
            var rect2 = to.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(rect1, eventData.position, uiCamera))
            {
                dragTarget = from;
            }
            if (RectTransformUtility.RectangleContainsScreenPoint(rect2, eventData.position, uiCamera))
            {
                dragTarget = to;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragTarget = null;
        }

        void Move()
        {
            if (dragTarget != null)
            {
                Vector3 pos = uiCamera.ScreenToWorldPoint(Input.mousePosition);
                dragTarget.position = new Vector3(pos.x, pos.y, 0.0f);
            }
        }
    }

}
