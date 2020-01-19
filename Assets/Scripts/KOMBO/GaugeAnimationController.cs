using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaugeAnimationController : MonoBehaviour
{
    public AnimationCurve curve;

    [SerializeField]
    private int _score = 100 ;

    void Start()
    {
        curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        curve.preWrapMode = WrapMode.PingPong;
        curve.postWrapMode = WrapMode.PingPong;
    }

    void Update()
    {
        Vector3 newPos_ = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        newPos_.y += curve.Evaluate(Time.time) * _score;

        transform.position = newPos_;
    }
}
