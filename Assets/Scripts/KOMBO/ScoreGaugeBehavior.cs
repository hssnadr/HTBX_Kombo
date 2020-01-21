using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Kombo
{
    public class ScoreGaugeBehavior : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve _curve;

        private float _pFinalScore = 100f;
        [SerializeField]
        private float _maxScore = 100f;

        private Camera _hitboxCamera;
        [SerializeField]
        private float _coefSpeedGaug = 0.5f;
        private float _timerStart;

        private Renderer _gaugRenderer;

        void Awake() {
            _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            _gaugRenderer = this.gameObject.GetComponent<Renderer>();

            _hitboxCamera = this.GetComponentInParent<Camera>();
        }
        
        void Start()
        {
            SetAlpha(0f);
            SetPositionFromScore(0f);
            _timerStart = Time.time;
        }

        public void SetGaugeColor(Color col_)
        {
            _gaugRenderer.material.color = col_;
        }

        private void SetAlpha(float a_) {
            Color newCol_ = _gaugRenderer.material.color;
            newCol_.a = a_;
            Debug.Log("alpha = " + newCol_.a);
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

            newPos_ += _hitboxCamera.transform.position; // position from parent to world
            transform.position = newPos_;
        }

        void Update()
        {
            float animRelativScore_ = _pFinalScore * _curve.Evaluate((Time.time - _timerStart) * _coefSpeedGaug);
            SetPositionFromScore(animRelativScore_);

            SetAlpha(_curve.Evaluate(0.6f*(Time.time - _timerStart)));
        }
    }
}
