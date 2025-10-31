using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AboloLib;

namespace AboloLib
{
    public class UICurrencyCollect : ArtAnimation
    {
        public static string BoomPaticlePath = "particle_boom";
        public static string FlyParticlePath = "particle_boom/particle_fly";

        [Header("[Boomb Param]")]
        public float range; //炸出范围
        public float timeToBoom;  //飞出时间
        public float timeToStop;  //飞出后停滞的时间
        public float timeToCollect; //飞去固定位置的时间 
        public float delay;//Icon动画延迟
        [Header("[UnityObj]"), SerializeField]
        GameObject shadowObj;
        //public Image prefab; //复制的预设 
        
        public int maxSortingLayer;


        public Vector3 startPos
        {
            get { return _startPos; }
            set { this._startPos = value; }
        }

        //--------------- private -------------
        private Vector3 _startPos;
        private int currencyIndex;
        //private bool canSet;

        [SerializeField] AnimationCurve _blastCurve;
        [SerializeField] AnimationCurve _moveCurve;
        [SerializeField] AnimationCurve _scaleCurve;
        [SerializeField] AnimationCurve _offsetCurve;

        [SerializeField] List<Currency> currencies;
        public List<Currency> Currencies
        {
            get => currencies;
        }

        public static void SetCurrenciesSprite(List<Currency> currencies ,int id)
        {
             Debug.Log($"id is ===========>{id}");
            Sprite sprite = ArtUtility.InstantiateSpriteFromResource($"Sprites/MergeItems/Icon_{id.ToString("D3")}", Vector2.one * 0.5f);
            foreach (var item in currencies)
            {
                item.transform.Find("particle_boom").GetComponent<ParticleSystem>().textureSheetAnimation.SetSprite(0, sprite);
                item.transform.Find("particle_boom/particle_fly").GetComponent<ParticleSystem>().textureSheetAnimation.SetSprite(0, sprite);
            }
        }
        public override void Start()
        {
            //startPos = this.transform.position;
        }

        private void OnDisable()
        {
            StopAnimation();
        }

        //返回从爆发到第一个金币飞到目标点的总时间
        public float GetDelayFromStartToHit(int count)
        {
            return count * (delay * timeToBoom) + timeToBoom + timeToCollect;
        }

        //返回撞击目标的时间间隔
        public float GetHitingInterval()
        {
            return delay * timeToCollect;
        }

        public void ParticlesLifeConfiguration(List<Currency> currencyObj)
        {
            for (int i = 0; i < currencyObj.Count; i++)
            {
                ParticleSystem ps_boom = currencyObj[i].transform.Find(BoomPaticlePath).GetComponent<ParticleSystem>();
                if (ps_boom != null)
                    ArtUtility.ConfigurePaticleSystem(ps_boom, i, timeToBoom + delay * i * timeToCollect + timeToStop, maxSortingLayer);

                ParticleSystem ps_fly = currencyObj[i].transform.Find(FlyParticlePath).GetComponent<ParticleSystem>();
                if (ps_fly != null)
                    ArtUtility.ConfigurePaticleSystem(ps_fly, i, timeToCollect + delay * (currencyObj.Count - i) * timeToBoom, maxSortingLayer);
            }
        }

        //--------------- function ------------
        /// <summary>
        /// 自定义喷发发射位置，飞行道具 ，回调执行的时机不是飞行完精准碰撞的那一刻（会延迟一点），需要精确时机用 GetDelayFromStartToHit 获取延迟
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <param name="from"></param>
        /// <param name="ShootPos"></param>
        /// <param name="to"></param>
        /// <param name="action"></param>
        public List<Currency> BoombToCollectCurrency(int id, int count, Vector3 from, Vector3 ShootPos ,Vector3 to , bool castShadow = false ,Action action = null)
        {
            List<Currency> cs = new List<Currency>();
            cs.Clear();
            for (int i = 0; i < count; i++)
            {
                var cur = CreateCurrencyObj(id);
                cs.Add(cur);
            }
            ani = StartCoroutine(_BoombToCollectCurrency(cs, from, ShootPos, to, castShadow, action));
            return cs;
        }
        /// <summary>
        /// 默认范围，随机方向发射，飞行道具，回调执行的时机不是飞行完精准碰撞的那一刻（会延迟一点），需要精确时机用 GetDelayFromStartToHit 获取延迟
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="action"></param>
        public List<Currency> BoombToCollectCurrency(int id , int count , Transform from ,Vector3 to , Action action= null)
        {
            List<Currency> cs = new List<Currency>();
            cs.Clear();
            for (int i = 0; i < count; i++)
            {
                var cur = CreateCurrencyObj(id);
                cs.Add(cur);
            }
            ani = StartCoroutine(_BoombToCollectCurrency(cs, from , to, action));
            return cs;
        }
        /// <summary>
        /// 自定义范围，随机方向发射，飞行道具，回调执行的时机不是飞行完精准碰撞的那一刻（会延迟一点），需要精确时机用 GetDelayFromStartToHit 获取延迟
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="boomRange"></param>
        /// <param name="action"></param>
        public List<Currency> BoombToCollectCurrency(int id, int count, Transform from, Vector3 to, float boomRange , Action action = null)
        {
            range = boomRange;
            List<Currency> cs = new List<Currency>();
            cs.Clear();
            for (int i = 0; i < count; i++)
            {
                var cur = CreateCurrencyObj(id);
                cs.Add(cur);
            }
            ani = StartCoroutine(_BoombToCollectCurrency(cs , from , to , action));
            return cs;
        }


        //----------- boomb ani -----------------

        IEnumerator _BoombToCollectCurrency(List<Currency> cs, Transform from ,Vector3 to , Action action = null)
        {
            //-----shadow_test

            List<GameObject> shadows = new List<GameObject>();
            shadows.Clear();

            //-----shadow_test


            //delay = timeToBoom / count;
            for (int i = 0; i < cs.Count; i++)
            {
                //ArtUtility.ConfigurePaticleSystem(currencies[i].transform.GetComponent<ParticleSystem>() , 0 , 1.0f , 100);

                //-----shadow_test
                //shadows.Add(Instantiate(shadowObj));

                //-----shadow_test
            }
            yield return StartCoroutine(RandomShootOut(cs , from , shadows));
            //canSet = false;
            yield return new WaitForSeconds(timeToStop);
            yield return StartCoroutine(FlyToPos(cs, to , shadows));
            action?.Invoke();
            yield return null;
            RemoveShadows(shadows);
            ///暂时不用Pop动画，直接写死延迟0.5s销毁，让最后的撞击粒子有时间播放完毕
            yield return new WaitForSeconds(4.0f);

            RemoveCurrencies(cs);
            ani = null;
        }

        IEnumerator _BoombToCollectCurrency(List<Currency> cs, Vector3 from, Vector3 shootPos ,Vector3 to, bool castShadow = false , Action action = null)
        {
            //-----shadow_test

            List<GameObject> shadows = new List<GameObject>();
            shadows.Clear();

            //-----shadow_test


            //delay = timeToBoom / count;
            for (int i = 0; i < cs.Count; i++)
            {
                //-----shadow_test
                if(castShadow)
                 shadows.Add(Instantiate(shadowObj));
                //-----shadow_test
            }
            yield return StartCoroutine(TargetPositionShootOut(cs, from, shootPos , shadows));
            yield return new WaitForSeconds(timeToStop);
            yield return StartCoroutine(FlyToPos(cs, to, shadows));
            action?.Invoke();
            yield return null;
            RemoveShadows(shadows);
            ///暂时不用Pop动画，直接写死延迟0.5s销毁，让最后的撞击粒子有时间播放完毕
            yield return new WaitForSeconds(4.0f);

            RemoveCurrencies(cs);
            ani = null;
        }



        IEnumerator RandomShootOut(List<Currency> currencyObj , Transform from , List<GameObject> shadows = null)
        {
            List<Vector3> toPos = new List<Vector3>();
            List<Vector3> fromPositions = new List<Vector3>();

#if _ARTEST_PRESENTATION
            //Debug.Log(from.lossyScale);
#endif
            foreach (Currency obj in currencyObj)
            {
                Vector3 pos = from.TransformPoint(UnityEngine.Random.insideUnitCircle * range * (1.0f / from.lossyScale.x));

                toPos.Add(pos);
                fromPositions.Add(from.position);
            }

            //配置粒子系统生命周期
            ParticlesLifeConfiguration(currencyObj);

            yield return StartCoroutine(ShootOut(currencyObj , fromPositions , toPos , shadows));
        }

        IEnumerator TargetPositionShootOut(List<Currency> currencyObj, Vector3 fromPos , Vector3 toPos, List<GameObject> shadows = null)
        {
            List<Vector3> toPositions = new List<Vector3>();
            List<Vector3> fromPositions = new List<Vector3>();

            foreach (Currency obj in currencyObj)
            {
                Vector3 pos = fromPos + toPos * ArtUtility.UISpaceWorldPositionScalor(UICanvasAdapter.CurrentCanvas);

                toPositions.Add(pos);
                fromPositions.Add(fromPos);
            }

            //配置粒子系统生命周期
            ParticlesLifeConfiguration(currencyObj);

            yield return StartCoroutine(ShootOut(currencyObj, fromPositions, toPositions, shadows));
        }

        IEnumerator ShootOut(List<Currency> currencyObj, List<Vector3> fromPositions, List<Vector3> toPositions, List<GameObject> shadows = null)
        {
            float timer = 0.0f;

            while (timer < 1.0f + currencyObj.Count * delay)
            {
                timer += Time.deltaTime / timeToBoom;
                for (int i = 0; i < currencyObj.Count; i++)
                {
                    //index越靠后的物体越晚开始位移
                    float t = timer - delay * i;

                    //开始位移时才能激活物体，真正开始消耗粒子生命，不然计算的结果有误差
                    if (t >= 0.0f && !currencyObj[i].gameObject.activeSelf)
                    {
                        currencyObj[i].gameObject.SetActive(true);
                    }

                    Vector3 fromPos = fromPositions[i];
                    Vector3 to = toPositions[i];
                    Currency currency = currencyObj[i];
                    Vector3 pos = Vector3.Lerp(fromPos, to, _blastCurve.Evaluate(t));

                    //------------shadow_test
                    if (shadows != null && shadows.Count > 0)
                    {
                        shadows[i].transform.position = pos;
                    }
                    //------------shadow_test

                    currency.transform.position = pos;
                }

                yield return null;

            }
        }

        IEnumerator FlyToPos(List<Currency> currencyObj, Vector3 to , List<GameObject> shadows = null)
        {
            List<Vector3> fromPos = new List<Vector3>();

            foreach (Currency obj in currencyObj)
                fromPos.Add(obj.transform.position);

            float timer = 0.0f;

            
            //currencyObj.Reverse();
            while (timer < 1.0f + currencyObj.Count * delay)
            {
                yield return null;
                timer += Time.deltaTime / timeToCollect;

                for (int i = 0; i < currencyObj.Count; i++)
                {
                    float t = timer - delay * i;

                    Vector3 from = fromPos[i];
                    Currency currency = currencyObj[i];
                    Vector3 pos = Vector3.Lerp(from, to, _moveCurve.Evaluate(t));
                    pos += Vector3.one * _offsetCurve.Evaluate(t);
                    Vector3 scale = Vector3.Lerp(Vector3.one, Vector3.one*3.0f, _scaleCurve.Evaluate(t));



                    currency.transform.position = pos;
                    currency.transform.localScale = scale;

                    //------------shadow_test

                    if (shadows != null && shadows.Count > 0)
                    {
                        float delta = 0.0f;
                        Vector3 asix = to - from;
                        Vector3 v1 = pos - from;
                        float fac = (Vector3.Distance(from, to) * Vector3.Distance(from, pos));
                        if (fac != 0f)
                        {
                            delta = v1.magnitude * Vector3.Dot(asix, v1) / fac;
                        }
                        else
                        {
                            delta = 0f;
                        }
                        Vector3 sPos = Vector3.LerpUnclamped(from, to, (delta / asix.magnitude));
                        //from +  v1 * delta;

                        //Debug.Log(delta);
                        //Vector3 shadowPos = sPos;//Vector3.Lerp(from, to, ArtUtility.IncreaseLinearCurve.Evaluate(t));
                        shadows[i].transform.position = sPos;
                        Vector3 shadow_scale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, _scaleCurve.Evaluate(t));
                        shadows[i].transform.localScale = shadow_scale;
                    }
                        

                    //------------shadow_test
                }
                
            }

            Debug.Log("Fly Done!!!!!!!!!!!!!!!!!!!!");
        }

        Currency CreateCurrencyObj(int id)
        {
            Currency currency = (Currency)CurrencyCollectManager.instance.currencyFactory.Get(id);
           currency.gameObject.SetActive(false);
            currency.transform.position = startPos;
            currency.transform.localScale = Vector3.one;
            return currency;
        }

        private void RemoveCurrencies(List<Currency> currencies)
        {
            if (currencies.Count > 0)
            {
                for (int i = 0; i < currencies.Count; i++)
                {
                    CurrencyCollectManager.instance.currencyFactory.Reclaim(currencies[i]);
                }
                currencies.Clear();
            }
        }
        //------------shadow_test
        void RemoveShadows(List<GameObject> shadows)
        {
            foreach (var item in shadows)
            {
                DestroyImmediate(item);
            }
            shadows.Clear();
            shadows = null;
        }
        //------------shadow_test
    }

}