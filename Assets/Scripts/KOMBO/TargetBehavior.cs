using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Kombo
{
    public class TargetBehavior : MonoBehaviour
    {
        private int _targetLevel = 0;                       // target family level (level 1, 2, 3...)
        private int _targetType = 0;                        // target type from family level properties
        private Vector3 _direction = Vector3.right;   // target direction in world space
        private float _angle = 0.0f;                        // default angle direction
        private float _translationSpeed_ = 50.0f;              // default translation speed value
        private float _rotationSpeed = 0.0f;                  // default rotation speed value
        private Vector3 _rotationAxis = Vector3.forward;
        private Renderer render;
        private float _lifeTime = 5.0f;                     // default lifetime in second
        private float _timeBorn;                            // time born from game start

        /// TARGET LEVEL
        public void SetTargetLevel(int targetLevel_)
        {
            _targetLevel = targetLevel_;
        }

        public int GetTargetLevel()
        {
            return _targetLevel;
        }

        /// TARGET TYPE
        public void SetTargetType(int targetType_)
        {
            _targetType = targetType_;
        }

        public int GetTargetType()
        {
            return _targetType;
        }

        public void SetLifeTime(float lifeTime_)
        {
            _lifeTime = lifeTime_;
        }

        public void SetAngleDirection(float angleDeg_)
        {
            _angle = angleDeg_ * Mathf.Deg2Rad;             // convert function input in degree to local variable in radian
            _direction = new Vector3(Mathf.Cos(_angle), Mathf.Sin(_angle), 0);
        }

        public void SetTranslationSpeed(float speedTranslate_)
        {
            _translationSpeed_ = speedTranslate_;
        }

        public void SetRotationSpeed(float speedRotate_)
        {
            _rotationSpeed = speedRotate_;
        }

        public void SetRotationAxis(Vector3 axRotate_)
        {
            _rotationAxis = axRotate_;
        }

        public void SetColor(Color col_)
        {
            render.material.SetColor("_Color", col_);
        }

        public Color GetColor()
        {
            return render.material.GetColor("_Color");
        }

        public void setScale(float scale_)
        {
            this.gameObject.transform.localScale = new Vector3(scale_, scale_, scale_);
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
            _timeBorn = Time.time;
        }


        public void SetScale(float scale_)
        {
            this.gameObject.transform.localScale = new Vector3(scale_, scale_, scale_);
        }

        void Update()
        {
            if (_rotationSpeed != 0.0f)
            {
                this.gameObject.transform.RotateAround(_rotationAxis, Vector3.forward, _rotationSpeed * Time.deltaTime);
            }
            this.gameObject.transform.Translate(_translationSpeed_ * _direction * Time.deltaTime, Space.World);

            if (Time.time - _timeBorn > _lifeTime)
            {
                destroyTarget();
            }
        }

        public void SetHit()
        {
            // Trigger explose animation = instantiate impact explosion
            if (_hitFeedbackPrefab != null && _targetLevel == 3)
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
