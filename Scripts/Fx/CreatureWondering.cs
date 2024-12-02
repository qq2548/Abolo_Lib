using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    public class CreatureWondering : ArtAnimation
    {
        Animator animator;
        [SerializeField] float _wonderingRange;
        [SerializeField] float _walkingSpeed = 1f;
         float duration;
        AnimationClip[] _myClips;

        Transform root;
        int _state = 0;
        public int State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public void Awake()
        {
            animator = GetComponentInChildren<Animator>(true);
            _myClips = animator.runtimeAnimatorController.animationClips;
            root = transform.Find("main_root");
            //Debug.Log(this.name +  root.position);
        }



        private void OnEnable()
        {
            if (_initialized)
            {
                ResetState();
            }
            else
            {
                StartCoroutine(ArtAnimDelayCoroutine(0.05f , () => ResetState()));
            }
        }


        Vector3 GetTargetPosition()
        {
            Vector2 pos = UnityEngine.Random.insideUnitCircle * _wonderingRange;
            return new Vector3(pos.x, 0f, pos.y);
        }

        void RandomAnimationBehavior()
        {
            
            string _clipName = _myClips[_state].name;
            animator.Play(_clipName);
            if (!_clipName.EndsWith("_walk"))

            {
                duration = _myClips[_state].length;
                StartCoroutine(ArtAnimDelayCoroutine(duration, () => ResetState()));
            }
            else
            {
                Vector3 from = root.localPosition;
                Vector3 to = GetTargetPosition();
                duration = Vector3.Distance(from, to) /_walkingSpeed;
                if (_initialized)
                {
                    Action<float> _deltaAnimation = (value) => { root.localPosition = ArtUtility.Move(from, to, value , 
                        CurveAdapter.AnimCurveDic[CurveFactory.CurveType.SlowSteady]); };
                    StartCoroutine(DoAnimation(duration , _deltaAnimation , () => ResetState()));
                }
                else
                {
                    base.Init();
                }
            }
        }

        void ResetState()
        {
            State = UnityEngine.Random.Range(0, _myClips.Length);
            RandomAnimationBehavior();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            float radius = _wonderingRange;
            int segments = 100;
            float deltaAngle = 360f / segments;
            Vector3 forward = transform.forward;

            Vector3[] vertices = new Vector3[segments];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 pos = Quaternion.Euler(0f, deltaAngle * i, 0f) * forward * radius + transform.position;
                vertices[i] = pos;
            }
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                Gizmos.DrawLine(vertices[i], vertices[i + 1]);
            }
            Gizmos.DrawLine(vertices[0], vertices[vertices.Length - 1]);
        }
#endif

    }

}