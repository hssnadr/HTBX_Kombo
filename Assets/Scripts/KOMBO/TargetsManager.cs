//using System;
using System.Collections;
using System.Collections.Generic;
using CRI.HitBoxTemplate.Serial;
using CRI.HitBoxTemplate.Example;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hitbox.Kombo
{
    [System.Serializable]
    public struct TargetProperties
    {
        [SerializeField]
        private float _rotSpeed;
        [SerializeField]
        private float _transSpeed;
        [SerializeField]
        private float _lifeTime;
        [SerializeField]
        private float _scale;

        public float rotSpeed
        {
            set {
                _rotSpeed = value ;
            }
            get{
                return _rotSpeed;
            }
        }

        public float transSpeed
        {
            set
            {
                _transSpeed = value;
            }
            get
            {
                return _transSpeed;
            }
        }

        public float lifeTime
        {
            set
            {
                _lifeTime = value;
            }
            get
            {
                return _lifeTime;
            }
        }

        public float scale
        {
            set
            {
                _scale = value;
            }
            get
            {
                return _scale;
            }
        }

        public TargetProperties(float lifeTime_, float scale_, float transSpeed_, float rotSpeed_)
        { _lifeTime = lifeTime_; _scale = scale_; _transSpeed = transSpeed_; _rotSpeed = rotSpeed_; }
    }

    public class TargetsManager : MonoBehaviour
    {
        public GameObject targetPrefab ;
        List<GameObject> targetsList;       // list of current targets
        List<Color> _reachTargetsColor;      // colors of reached targets
        List<Vector3> _reachTargetsPosition;   // position of reached targets

        [SerializeField]
        private TargetProperties _targetPropLvl1 = new TargetProperties(3.0f, 40.0f, 85.0f, 0.0f);      // RGB
        [SerializeField]
        private TargetProperties _targetPropLvl2 = new TargetProperties(2.0f, 30.0f, 120.0f, 300.0f);   // CMJ
        [SerializeField]
        private TargetProperties _targetPropLvl3 = new TargetProperties(1.0f, 30.0f, 200.0f, -300.0f);  // White

        public GameObject impact;           // prefab to show where the impacts are detected
        private float delayOffHit = 0.2f ;
        private float timerOffHit0 = 0;

        private Camera _hitboxCamera;

        private int _score = 0 ;
        private int _comboMultiply = 1 ;
        public Text scoreText ;

        private bool isWaiting = false;

        private void Awake()
        {
            _hitboxCamera = this.gameObject.GetComponent<Camera>();
            targetsList = new List<GameObject>();
            _reachTargetsColor = new List<Color>();
            _reachTargetsPosition = new List<Vector3>();

            scoreText.text = "";
        }

        public void GetImpact(Vector2 position2D_)
        {
            Vector3 position3D_ = new Vector3(position2D_.x, position2D_.y, this.gameObject.transform.position.z + 50f);

            Vector3 cameraForward = _hitboxCamera.transform.forward;
            Debug.DrawRay(position2D_, cameraForward * 10000, Color.yellow, 10.0f);

            bool isTrigger_ = false ;
            Color targetColor_ = Color.black;
            if (targetsList.Count > 0)
            {
                RaycastHit hit;
                if (Physics.Raycast(position2D_, cameraForward, out hit))
                {
                    if (hit.collider != null && hit.transform.tag == "target")
                    {
                        targetColor_ = hit.collider.GetComponent<TargetBehavior>().GetColor();
                        hit.collider.GetComponent<TargetBehavior>().destroyTarget();
                        isTrigger_ = true;
                    }
                }
            }
            else {
                isTrigger_ = true;
            }

            if (isTrigger_)
            {
                Color[] colTargets_;

                if (targetsList.Count == 0)
                {
                    colTargets_ = new Color[3];
                    colTargets_[0] = Color.red;
                    colTargets_[1] = Color.green;
                    colTargets_[2] = Color.blue;
                    SetCrownTargets(position3D_, colTargets_, _targetPropLvl1);

                    _score += 1 * _comboMultiply;
                    _comboMultiply *= 2;
                }
                else
                {
                    _reachTargetsColor.Add(targetColor_);   // update reached target colors
                    _reachTargetsPosition.Add(position3D_); // update reached target positions

                    if (targetColor_ == Color.red)
                    {
                        colTargets_ = new Color[2];
                        colTargets_[0] = Color.magenta;
                        colTargets_[1] = Color.yellow;
                        SetCrownTargets(position3D_, colTargets_, _targetPropLvl2);

                        _score += 2 * _comboMultiply;
                        _comboMultiply *= 4;
                    }

                    if (targetColor_ == Color.green)
                    {
                        colTargets_ = new Color[2];
                        colTargets_[0] = Color.yellow;
                        colTargets_[1] = Color.cyan;
                        SetCrownTargets(position3D_, colTargets_, _targetPropLvl2);

                        _score += 2 * _comboMultiply;
                        _comboMultiply *= 4;
                    }

                    if (targetColor_ == Color.blue)
                    {
                        colTargets_ = new Color[2];
                        colTargets_[0] = Color.cyan;
                        colTargets_[1] = Color.magenta;
                        SetCrownTargets(position3D_, colTargets_, _targetPropLvl2);

                        _score += 2 * _comboMultiply;
                        _comboMultiply *= 4;
                    }

                    if (targetColor_ == Color.white)
                    {
                        _score += 4 * _comboMultiply;
                        _comboMultiply *= 8;
                    }

                    if (targetColor_ == Color.yellow || targetColor_ == Color.cyan || targetColor_ == Color.magenta)
                    {
                        int[] indexCMJ_ = new int[3];
                        bool isYellow_ = false;
                        bool isCyan_ = false;
                        bool isMagenta_ = false;
                        for (int i = 0; i < _reachTargetsColor.Count; i++) {
                            if (_reachTargetsColor[i] == Color.yellow && !isYellow_) {
                                indexCMJ_[0] = i;                                       // get yellow index
                                isYellow_ = true;
                            }

                            if (_reachTargetsColor[i] == Color.cyan && !isCyan_){
                                indexCMJ_[1] = i;                                       // get cyan index
                                isCyan_ = true;
                            }

                            if (_reachTargetsColor[i] == Color.magenta && !isMagenta_) {
                                indexCMJ_[2] = i;                                       // get magenta index
                                isMagenta_ = true;
                            }                                

                            if (isYellow_ && isCyan_ && isMagenta_) {
                                float xG_ = 0.0f;
                                for (int j = 0; j < 3; j++)
                                {
                                    xG_ += _reachTargetsPosition[indexCMJ_[j]].x;
                                }
                                xG_ /= 3.0f; // get X pos of white target

                                float yG_ = 0.0f;
                                for (int j = 0; j < 3; j++)
                                {
                                    yG_ += _reachTargetsPosition[indexCMJ_[j]].y;
                                }
                                yG_ /= 3.0f;  // get Y pos of white target

                                float zG_ = 0.0f;
                                for (int j = 0; j < 3; j++)
                                {
                                    zG_ += _reachTargetsPosition[indexCMJ_[j]].z;
                                }
                                zG_ /= 3.0f;  // get Z pos of white target

                                // Set white target
                                Vector3 posWhite_ = new Vector3(xG_, yG_, zG_);
                                SetTarget(posWhite_, Color.white, 0.0f, _targetPropLvl3);

                                // Destroyed reached targets
                                System.Array.Sort(indexCMJ_);
                                for (int j = 2; j >= 0; j--) {
                                    _reachTargetsColor.RemoveAt(indexCMJ_[j]);
                                    _reachTargetsPosition.RemoveAt(indexCMJ_[j]);                                    
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void SetTarget(Vector3 position_, Color colTarget_, float angleDirection_, TargetProperties targetProp_)
        {   
            // Instantiate target
            targetsList.Add((GameObject)Instantiate(targetPrefab, position_, Quaternion.identity));

            // Set target properties
            GameObject target_ = targetsList[targetsList.Count - 1];                            // get last target which correspond to the current one
            target_.GetComponent<TargetBehavior>().SetLifeTime(targetProp_.lifeTime);
            target_.GetComponent<TargetBehavior>().SetAngleDirection(angleDirection_);
            target_.GetComponent<TargetBehavior>().SetColor(colTarget_);
            target_.GetComponent<TargetBehavior>().SetTranslationSpeed(targetProp_.transSpeed);
            target_.GetComponent<TargetBehavior>().SetRotationAxis(position_);
            target_.GetComponent<TargetBehavior>().SetRotationSpeed(targetProp_.rotSpeed);
            target_.GetComponent<TargetBehavior>().setScale(targetProp_.scale);
        }

        private void SetCrownTargets(Vector3 position_, Color[] colTargets_, TargetProperties targetProp_)
        {
            int angle0_ = Random.Range(0, 360);
            int nTargets_ = colTargets_.Length;

            for (int i = 0; i < nTargets_; i++)
            {
                SetTarget(position_, colTargets_[i], angle0_ + i * (float)360 / nTargets_, targetProp_);
            }
        }



        private IEnumerator WaitForEnd()
        {
            isWaiting = true;
            yield return new WaitForSeconds(5.0f);
            scoreText.text = "";
            //serialController.ScreenSaver();
            isWaiting = false;
        }


        private void Update()
        {
            for (int i = 0; i < targetsList.Count; i++) {
                if (targetsList[i] == null)
                    targetsList.RemoveAt(i);

                if (targetsList.Count == 0 && !isWaiting)
                {
                    scoreText.text = _score.ToString();
                    Debug.Log("Score = " + _score);
                    _score = 0;
                    _comboMultiply = 1;

                    _reachTargetsColor.Clear();
                    _reachTargetsPosition.Clear();

                    //serialController.EndGame();

                    StartCoroutine(WaitForEnd());
                }
                else {
                    scoreText.text = ""; 
                }
            }
        }
    }
}