using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class DecorationParticle : MonoBehaviour,IFxAnimCtrl
    {
        [Header("权重1是最大值，对应100颗粒子")]
        public ParticleWithSpawnWeight[] _particleSystems;

        Transform _root;
        Matrix4x4 matrix;
        void Awake()
        {
            matrix = transform.worldToLocalMatrix;
        }


        public void EmitDecorationParticle(Transform root)
        {
            Mesh cm = ArtUtility.CreateCombinedMesh(root , matrix);
            foreach (var ps in _particleSystems)
            {
                ArtUtility.ConfigurePaticleSystem(ps.particleSystem, cm, ps.weight);
                ps.particleSystem.Play();
            }
        }

        public void Play()
        {
            _root = transform.parent;
            EmitDecorationParticle(_root);
        }

        public void Stop(Action callback=null)
        {
            foreach (var ps in _particleSystems)
            {
                var emit = ps.particleSystem.emission;
                emit.enabled = false;
            }

            callback?.Invoke();
        }
    }

}