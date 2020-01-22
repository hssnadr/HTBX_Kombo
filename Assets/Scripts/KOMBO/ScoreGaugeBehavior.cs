using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Kombo
{
    public class ScoreGaugeBehavior : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve _curve;
        [SerializeField]
        private AnimationCurve _curveDefeat;

        private float _pFinalScore = 100f;
        [SerializeField]
        private float _maxScore = 100f;

        private Camera _hitboxCamera;
        [SerializeField]
        private float _coefSpeedGaug = 0.5f;

        [SerializeField]
        private Color _colorVictory;
        [SerializeField]
        private Color _colorDefeat;

        private Renderer _gaugRenderer;

        void Awake() {
            _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            _gaugRenderer = this.gameObject.GetComponent<Renderer>();
            _hitboxCamera = this.GetComponentInParent<Camera>();
        }
        
        void Start()
        {
            SetAlpha(0f);
        }

        public void SetGaugeColor(Color col_)
        {
            _gaugRenderer.material.color = col_;
        }

        private void SetAlpha(float a_) {
            Color newCol_ = _gaugRenderer.material.color;
            newCol_.a = a_;
            _gaugRenderer.material.color = newCol_;
        }

        public void SetPlayerScore(float score_) {
            _pFinalScore = score_;
        }

        void SetPositionFromScore(float score_)
        {
            float scoreRatio_ = score_ / _maxScore;
            float valY_ = _hitboxCamera.rect.height * _hitboxCamera.orthographicSize * (2 * scoreRatio_ - 1);
            Vector3 newPos_ = new Vector3(0, valY_, 100);
            transform.localPosition = newPos_;
        }

        public IEnumerator EntranceAnimation()
        {
            float timerStart_ = Time.time;
            float deltaTime_ = 0f;
            
            while (deltaTime_ * _coefSpeedGaug <= 1f){
                deltaTime_ = Time.time - timerStart_;
                float progressEntrance_ = _pFinalScore * _curve.Evaluate(deltaTime_ * _coefSpeedGaug);
                SetPositionFromScore(progressEntrance_);

                SetAlpha(_curve.Evaluate(0.6f * deltaTime_));

                yield return null;
            }
        }

        public IEnumerator VictoryAnimation()
        {
            float timerStart_ = Time.time;
            float deltaTime_ = 0f;
            float animSpeed_ = 0.8f;
            Color color0_ = _gaugRenderer.material.color;
            Vector3 scale0_ = transform.localScale; // inital scale
            float dScaleY_ = _hitboxCamera.rect.height * _hitboxCamera.orthographicSize + Mathf.Abs(transform.localPosition.y);
            dScaleY_ -= scale0_.y / 2f;
            dScaleY_ *= 2f;

            while (deltaTime_ <= 1f)
            {
                deltaTime_ = animSpeed_ * (Time.time - timerStart_);

                SetGaugeColor(Color.Lerp(color0_, _colorVictory, _curve.Evaluate(1.5f * deltaTime_)));

                /// Rotation
                //Quaternion newRot_ = transform.localRotation;
                //newRot_.z = _curve.Evaluate(deltaTime_) * 180f;
                //transform.localRotation = newRot_;

                /// Anim alpha
                float progAlpha_ = 1f - _curve.Evaluate(deltaTime_);
                SetAlpha(progAlpha_);

                /// Anim scale
                Vector3 newScale_ = scale0_;
                newScale_.y += _curve.Evaluate(deltaTime_) * dScaleY_;
                transform.localScale = newScale_;

                yield return null;
            }
        }

        public IEnumerator DefeatAnimation()
        {
            float timerStart_ = Time.time;
            float deltaTime_ = 0f;
            float animSpeed_ = 0.8f;
            Color color0_ = _gaugRenderer.material.color;
            Vector3 scale0_ = transform.localScale; // inital scale
            float dScaleY_ = 8f * scale0_.y;
            Vector3 pos0_ = transform.localPosition;
            float dPosY_ = _hitboxCamera.rect.height * _hitboxCamera.orthographicSize + pos0_.y;
            Debug.Log("new scale Y = " + dScaleY_);

            while (deltaTime_ <= 1f)
            {
                deltaTime_ = animSpeed_ * (Time.time - timerStart_);

                SetGaugeColor(Color.Lerp(color0_, _colorDefeat, _curve.Evaluate(1.5f * deltaTime_)));

                /// Anim alpha
                if (deltaTime_ > 0.35)
                {
                    float progAlpha_ = 1f - _curve.Evaluate(deltaTime_);
                    SetAlpha(progAlpha_);
                }

                /// Anim scale
                Vector3 newScale_ = scale0_;
                newScale_.y += _curve.Evaluate(deltaTime_) * dScaleY_;
                transform.localScale = newScale_;

                /// Anim position
                Vector3 newPos_ = this.transform.localPosition;
                newPos_.y = _curveDefeat.Evaluate(deltaTime_) * dPosY_ - _hitboxCamera.rect.height * _hitboxCamera.orthographicSize;
                transform.localPosition = newPos_;

                yield return null;
            }
        }

        public IEnumerator FadeOutAnimation(float scaleFactor_)
        {
            float timerStart_ = Time.time;
            float deltaTime_ = 0f;
            float animSpeed_ = 0.8f;
            Color color0_ = _gaugRenderer.material.color;
            Vector3 scale0_ = transform.localScale; // inital scale
            float dScaleY_ = scaleFactor_ * scale0_.y;

            while (deltaTime_ <= 1f)
            {
                deltaTime_ = animSpeed_ * (Time.time - timerStart_);

                /// Anim alpha
                float progAlpha_ = 1f - _curve.Evaluate(deltaTime_);
                SetAlpha(progAlpha_);

                /// Anim scale
                Vector3 newScale_ = scale0_;
                newScale_.y += _curve.Evaluate(deltaTime_) * dScaleY_;
                transform.localScale = newScale_;

                yield return null;
            }
        }
    }
}
