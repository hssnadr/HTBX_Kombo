using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaugeAnimationController : MonoBehaviour
{
    public AnimationCurve curve;

    [SerializeField]
    private int _pFinalScore = 100 ;

    private Camera _hitboxCamera;
    private float _coefSpeedGaug = 0.5f;

    void Start()
    {
        curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        curve.preWrapMode = WrapMode.PingPong;
        curve.postWrapMode = WrapMode.PingPong;

        _hitboxCamera = this.GetComponentInParent<Camera>();
        SetPositionFromRelativeScore100(0f); // place at bottom of screen
    }

    void SetPositionFromRelativeScore100(float score100_) {
        Vector3 curPosScreen_ = _hitboxCamera.WorldToViewportPoint(this.transform.position);

        //Vector3 newPos_ = new Vector3(curPosScreen_.x, 0, curPosScreen_.z);
        Vector3 newPos_ = curPosScreen_;
        newPos_.y -= _hitboxCamera.rect.y * _hitboxCamera.orthographicSize / 2;

        float _scorePercent = score100_ / _pFinalScore;
        newPos_.y += _scorePercent * _hitboxCamera.rect.y * _hitboxCamera.orthographicSize;

        if (!_hitboxCamera.orthographic)
            newPos_.z = this.transform.position.z;

        transform.position = _hitboxCamera.ScreenToWorldPoint(newPos_);
    }

    void Update()
    {
        float animRelativScore_ = _pFinalScore * curve.Evaluate(Time.time) * _coefSpeedGaug;
        Debug.Log(animRelativScore_);
        SetPositionFromRelativeScore100(animRelativScore_);
    }
}
