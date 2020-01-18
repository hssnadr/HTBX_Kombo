using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Kombo
{
    public class TargetBehavior : MonoBehaviour
    {
        private Vector3 _targetDirection = Vector3.right;   // target direction in world space
        private float _angle = 0.0f;                        // default angle direction
        private float _speedTranslate = 50.0f;              // default translation speed value
        private float _speedRotate = 0.0f;                  // default rotation speed value
        private Vector3 _rotationAxe = Vector3.forward;
        private Renderer render;
        private float _lifeTime = 5.0f;                     // default lifetime in second
        private float _timeBorn;                            // time born from game start

        /// <summary>
        /// Prefab of the feedback object.
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab of the feedback object.")]
        private ImpactFeedbackAnimator _impactFeedbackPrefab = null;

        void Awake()
        {
            render = GetComponent<Renderer>();
            _timeBorn = Time.time;
        }

        public void SetLifeTime(float lifeTime_)
        {
            _lifeTime = lifeTime_;
        }

        public void SetAngleDirection(float angleDeg_)
        {
            _angle = angleDeg_ * Mathf.Deg2Rad;             // convert function input in degree to local variable in radian
            _targetDirection = new Vector3(Mathf.Cos(_angle), Mathf.Sin(_angle), 0);
        }

        public void SetTranslationSpeed(float speedTranslate_)
        {
            _speedTranslate = speedTranslate_;
        }

        public void SetRotationSpeed(float speedRotate_)
        {
            _speedRotate = speedRotate_;
        }

        public void SetRotationAxis(Vector3 axRotate_)
        {
            _rotationAxe = axRotate_;
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

        void Update()
        {
            if (_speedRotate != 0.0f)
            {
                this.gameObject.transform.RotateAround(_rotationAxe, Vector3.forward, _speedRotate * Time.deltaTime);
            }
            this.gameObject.transform.Translate(_speedTranslate * _targetDirection * Time.deltaTime, Space.World);

            if (Time.time - _timeBorn > _lifeTime)
            {
                destroyTarget();
            }
        }

        public void GetImpact()
        {
            // Trigger explose animation = instantiate impact explosion
            if (_impactFeedbackPrefab != null)
            {
                var go = Instantiate(_impactFeedbackPrefab, this.transform.position, Quaternion.identity);
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
