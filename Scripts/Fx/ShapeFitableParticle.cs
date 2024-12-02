
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AboloLib
{
    public class ShapeFitableParticle : FxAnim, IFxAnimCtrl
    {
        [SerializeField] RendererType _rendererType;
        [Header("实例化完成后是否自动播放")]
        [SerializeField] bool _playOnStart;
        [SerializeField] bool _usePool = false;
        //用于计算粒子多少
        //private Vector2 _size;

        Transform _fitTarget;
        public Transform FitTarget
        {
            set
            {
                _fitTarget = value;
                SetParticleEmitShape();
            }
        }

        private ParticleSystem[] _particleSystems;


        public override void Start()
        {
            Init();
            if (_playOnStart)
            {
                Play();
            }
        }

        public override void Init()
        {
            base.Init();
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            FitTarget = transform.parent;
            transform.localPosition = Vector3.zero;
        }

        void SetParticleEmitShape()
        {
            var length = _particleSystems.Length;
            if (length<=0)
            {
#if _ARTEST_PRESENTATION
                Debug.LogError(ArtUtility.WarningLog + name + " --- 没有可播放的粒子!!!!!!!!!");
#endif
                return;
            }
            transform.position = _fitTarget.position;
            for (int i = 0; i < length; i++)
            {
                var main = _particleSystems[i].main;
                var emit = _particleSystems[i].emission;
                emit.enabled = true;
                var shape = _particleSystems[i].shape;
                float weight = 0f;
                switch (_rendererType)
                {
                    case RendererType.Image:
                        shape.sprite = _fitTarget.GetComponentInChildren<Image>(true)?.sprite;
                        FixNullSprite(shape);
#if _ARTEST_PRESENTATION
                        Debug.Log(shape.sprite.textureRect);
#endif
                        break;
                    case RendererType.SpriteRenderer:
                        shape.sprite = _fitTarget.GetComponentInChildren<SpriteRenderer>(true)?.sprite;
                        FixNullSprite(shape);

#if _ARTEST_PRESENTATION
                        Debug.Log(shape.sprite.textureRect);
#endif
                        break;
                }
                weight = shape.sprite.textureRect.width * shape.sprite.textureRect.height / 39527f;
                main.maxParticles = Mathf.FloorToInt(weight * main.maxParticles) + 5;
                var rate = emit.rateOverTime;
                rate.constant = main.maxParticles * 0.6f;
                emit.rateOverTime = rate;
            }
        }


        void FixNullSprite(ParticleSystem.ShapeModule shape)
        {
            if (shape.sprite == null)
            {
                Debug.Log(ArtUtility.WarningLog + _fitTarget.name + " 需要适配形状的图片引用为空，检查资源");
                //shape资源为空的时候，动态生成一个70px的sprite
                shape.sprite = ArtUtility.CreateSprite(new Texture2D(70, 70), Vector2.one * 70);
            }
        }

        public override void Play()
        {
            base.Play();
            //每次播放需要重新初始化，有可能回收再获取之后更换了父物体
            if (!_usePool)
            {
                Init();
            }
            else
            {
                if (_fitTarget == null)
                {
                    Debug.LogWarning(name + "特效对应的目标未赋值，请将目标Renderer的transform赋值给我的 ”FitTarget“ 再 Play");
                }
            }
            //非空或者数组长度大于零的判断
            if (_particleSystems is not {Length: > 0}) return;

            foreach (var item in _particleSystems)
            {
                item.Play();
            }
        }


        public override void Stop(Action callback = null)
        {
            base.Stop(callback);//基类Stop方法会在delay时间后disable掉main_root

            //非空或者数组长度大于零的判断
            if (_particleSystems is not { Length: > 0 }) return;

            foreach (var item in _particleSystems)
            {
                var emit = item.emission;
                emit.enabled = false;
            }
        }

        public void SetColor(Color c)
        {
            //非空或者长度大于零的判断
            if (_particleSystems is not {Length: > 0}) return;
            foreach (var item in _particleSystems)
            {
                ParticleSystem.MainModule mainModule = item.main;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(c, c);
            }
        }


        //public void SetOrderLayerId(int layerId, int order = 999)
        //{
        //    foreach (var item in _particleSystems)
        //    {
        //        item.GetComponent<ParticleSystemRenderer>().sortingLayerID = layerId;
        //        item.GetComponent<ParticleSystemRenderer>().sortingOrder = order;
        //    }
        //}

        //IEnumerator CorStop(Action afterStop, float delay, bool useDisolve = false)
        //{
        //    for (int i = 0; i < _particleSystems.Length; i++)
        //    {
        //        var emit = _particleSystems[i].emission;
        //        emit.enabled = false;

        //        if (useDisolve)
        //        {
        //            int number = _particleSystems[i].particleCount;
        //            var _particles = new ParticleSystem.Particle[number];
        //            _particleSystems[i].GetParticles(_particles);
        //            for (int a = 0; a < _particles.Length; a++)
        //            {

        //                _particles[a].startLifetime = 1.5f;
        //                _particles[a].remainingLifetime = 1.5f;
        //                yield return new WaitForSeconds(0.05f);
        //            }
        //            _particleSystems[i].SetParticles(_particles);
        //        }
        //    }

        //    if (useDisolve)
        //    {
        //        yield return new WaitForSeconds(delay);
        //    }

        //    afterStop?.Invoke();
        //}


    }
}

