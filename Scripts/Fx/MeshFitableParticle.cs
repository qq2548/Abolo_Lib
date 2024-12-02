
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{

    public class MeshFitableParticle : MonoBehaviour
    {
        private MeshFilter _meshFilter = null;
        //用于计算粒子多少
        private Vector2 _size;

        private Color _color = Color.white;
        private ParticleSystem[] _particleSystems;

        [SerializeField]
        private float _particleAmount = 0.8f;

        void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        public void Init(MeshFilter meshFilter, float width = 1f, float height = 1f)
        {
            _color = Color.white;
            _meshFilter = meshFilter;
            _size = new Vector2(width, height);
            Vector3 sfSize = meshFilter.transform.localScale;
            if (_meshFilter != null)
            {
                var length = _particleSystems.Length;
                for (int i = 0; i < length; i++)
                {
                    var emit = _particleSystems[i].emission;
                    emit.enabled = true;
                    var shape = _particleSystems[i].shape;
                    shape.mesh = _meshFilter.mesh;
                    shape.scale = sfSize;
                     var _main = _particleSystems[i].main;
                    int maxCount = _main.maxParticles;
                    _main.maxParticles = Mathf.CeilToInt(_size.x * _size.y * maxCount * _particleAmount);
                    var _curve = emit.rateOverTime;
                    if (_main.loop)
                    {
                        _curve.constant = Mathf.CeilToInt(_size.x * _size.y *maxCount * _particleAmount);
                        emit.rateOverTime = _curve;
                    }
                    else
                    {
                        _curve.constant = _main.maxParticles * (1.0f / _main.duration);
                        emit.rateOverTime = _curve;
                    }
                    _main.startColor = _color;
                }
            }
            else
            {
                Debug.LogError("_mySprite 或者 targetSpriteRenderer 为空!");
            }
        }

        public void InitAndPlay(MeshFilter meshFilter, int width, int height)
        {
            Init(meshFilter, width, height);
            Play();
        }

        public void Play()
        {
            foreach (var item in _particleSystems)
            {
                item.Play();
            }
        }

        public void Stop(Action afterStop, float delay = 0.2f, bool useDisolve = true)
        {
            transform.SetParent(null);
            StartCoroutine(CorStop(afterStop , delay));
        }

        public void SetColor(Color c)
        {
            foreach (var item in _particleSystems)
            {
                ParticleSystem.MainModule mainModule = item.main;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(c, c);
            }
        }

        public void SetOrderLayerId(int layerId, int order = 999)
        {
            foreach (var item in _particleSystems)
            {
                item.GetComponent<ParticleSystemRenderer>().sortingLayerID = layerId;
                item.GetComponent<ParticleSystemRenderer>().sortingOrder = order;
            }
        }

        IEnumerator CorStop(Action afterStop, float delay, bool useDisolve = false)
        {
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var emit = _particleSystems[i].emission;
                emit.enabled = false;

                if (useDisolve)
                {
                    int number = _particleSystems[i].particleCount;
                    var _particles = new ParticleSystem.Particle[number];
                    _particleSystems[i].GetParticles(_particles);
                    for (int a = 0; a < _particles.Length; a++)
                    {

                        _particles[a].startLifetime = 1.5f;
                        _particles[a].remainingLifetime = 1.5f;
                        yield return new WaitForSeconds(0.05f);
                    }
                    _particleSystems[i].SetParticles(_particles);
                }
            }

            if (useDisolve)
            {
                yield return new WaitForSeconds(delay);
            }

            afterStop?.Invoke();
        }
    }
}

