using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Kombo
{
    public class TargetBehavior : MonoBehaviour
    {
        private float _time0;                            // time born from game start
        private Renderer render;

        /// TARGET LEVEL
        private int _targetLevel = 0;                       // target family level (level 1, 2, 3...)
        public int TargetLevel
        {
            get => _targetLevel;
            set
            {
                _targetLevel = value;
            }
        }

        /// TARGET TYPE
        private int _targetType = 0;                        // target type from family level properties
        public int TargetType
        {
            get => _targetType;
            set
            {
                _targetType = value;
            }
        }

        /// TARGET DIRECTION
        private Vector3 _targetDirection = Vector3.right;   // target direction in world space
        public Vector3 TargetDirection
        {
            get => _targetDirection;
            set
            {
                _targetDirection = value;
            }
        }

        /// ROTATION AXIS
        private Vector3 _rotationAxis = Vector3.forward;
        public Vector3 RotationAxis
        {
            get => _rotationAxis;
            set
            {
                _rotationAxis = value;
            }
        }

        /// TARGET ANGLE
        private float _angle = 0.0f;                        // default angle direction
        public float Angle
        {
            get => _angle;
            set
            {
                _angle = value * Mathf.Deg2Rad;             // convert function input in degree to local variable in radian
                TargetDirection = new Vector3(Mathf.Cos(_angle), Mathf.Sin(_angle), 0);
            }
        }

        /// TRANSLATION SPEED
        private float _translationSpeed = 50.0f;              // default translation speed value
        public float TranslationSpeed
        {
            get => _translationSpeed;
            set
            {
                _translationSpeed = value;
            }
        }

        /// ROTATION SPEED
        private float rotationSpeed = 0.0f;                  // default rotation speed value
        public float RotationSpeed
        {
            get => _translationSpeed;
            set
            {
                _translationSpeed = value;
            }
        }

        // LIFE TIME
        private float _lifeTime = 5.0f;                     // default lifetime in second
        public float LifeTime
        {
            get => _lifeTime;
            set
            {
                _lifeTime = value;
            }
        }

        // TARGET COLOR
        public Color TargetColor
        {
            set
            {
                render.material.SetColor("_Color", value);
            }
        }

        /// <summary>
        /// Prefab of the feedback object.
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab of the feedback object.")]
        private HitFeedbackAnimator _hitFeedbackPrefab = null;

        void Awake()
        {
            render = GetComponent<Renderer>();
            _time0 = Time.time;
        }

        

        public void SetScale(float scale_)
        {
            this.gameObject.transform.localScale = new Vector3(scale_, scale_, scale_);
        }

        void Update()
        {
            if (rotationSpeed != 0.0f)
            {
                this.gameObject.transform.RotateAround(_rotationAxis, Vector3.forward, rotationSpeed * Time.deltaTime);
            }
            this.gameObject.transform.Translate(_translationSpeed * _targetDirection * Time.deltaTime, Space.World);

            if (Time.time - _time0 > _lifeTime)
            {
                destroyTarget();
            }
        }

        public void SetHit()
        {
            // Trigger explose animation = instantiate impact explosion
            if (_hitFeedbackPrefab != null)
            {
                var go = Instantiate(_hitFeedbackPrefab, this.transform.position, Quaternion.identity);
                go.gameObject.layer = this.gameObject.layer;
            }

            // Destroy target
            this.destroyTarget();
        }

        void OnBecameInvisible()
        {
            this.destroyTarget();
        }

        private void destroyTarget()
        {
            // Destroy current target
            Destroy(this.gameObject);
        }
    }
}
