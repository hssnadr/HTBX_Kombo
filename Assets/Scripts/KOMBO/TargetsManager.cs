//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Kombo
{
    [System.Serializable]
    public struct TargetProperties
    {
        [SerializeField]
        private Color[] _colors;
        [SerializeField]
        private float _rotSpeed;
        [SerializeField]
        private float _transSpeed;
        [SerializeField]
        private float _lifeTime;
        [SerializeField]
        private float _scale;

        public Color[] colors
        {
            set
            {
                _colors = value;
            }
            get
            {
                return _colors;
            }
        }

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

        public TargetProperties(Color[] colors_, float lifeTime_, float scale_, float transSpeed_, float rotSpeed_)
        {_colors = colors_; _lifeTime = lifeTime_; _scale = scale_; _transSpeed = transSpeed_; _rotSpeed = rotSpeed_; }
    }

    public class TargetsManager : MonoBehaviour
    {
        public GameObject targetPrefab;
        private List<GameObject> _targetsList;			// list of current targets
        private List<Vector3> _reachTargetsPosition;	// position of reached targets

        /// <summary>
        /// Here are specified the caracteristics for each target level
        /// </summary>
        [SerializeField]
        private TargetProperties _targetPropLvl1 = new TargetProperties();  /// LEVEL 1
        [SerializeField]
        private TargetProperties _targetPropLvl2 = new TargetProperties();  /// LEVEL 2
        [SerializeField]
        private TargetProperties _targetPropLvl3 = new TargetProperties();  ///  LEVEL 3

        private Camera _hitboxCamera;

        [SerializeField]
        private float _score;
        private float _damageReduce;

        // On Hit level 1
        [SerializeField]
        private int[,] _trgtsKomboLvl2 = new int[3,6];
        [SerializeField]
        private int _komboLinks = 2; // Number of targets per kombo init
        private int _komboLvl = 0;  // Current Kombo level
        private int _komboHit;      // Current Kombo hit progression

        // On hit level 3
        private int _nHitLvl3 = 0;
        private int _totHitLvl3 = 2;

        private void Awake()
        {
            _hitboxCamera = this.gameObject.GetComponentInParent<Camera>();
            _targetsList = new List<GameObject>();
            _reachTargetsPosition = new List<Vector3>();

            /// Initialize target propertie levels
            //_targetPropLvl1 = new TargetProperties(new Color[] { Color.white }, 3.0f, 40.0f, 85.0f, 0.0f);      /// LEVEL 1
            //_targetPropLvl2 = new TargetProperties(new Color[] { Color.white }, 2.0f, 30.0f, 120.0f, 300.0f);   /// LEVEL 2
            //_targetPropLvl3 = new TargetProperties(new Color[] { Color.white }, 1.0f, 30.0f, 200.0f, -300.0f);  /// LEVEL 3

            // Initialize Level 1            

            // Initialize Level 2 /// Set Kombo
            _trgtsKomboLvl2 = new int[,]
            {
                {0, 1, 0, 1, 2, 2},
                {0, 1, 0, 2, 1, 2},
                {1, 2, 0, 2, 1, 1},
            };
            _komboLvl = 0; // First Kombo level
            _komboHit = 0;

            // Initialize Level 3
        }

        private void Start()
        {
            SetTargetsLvl1(this.transform.position);

            _score = 0f;
            _damageReduce = 1f;
    }

        public void GetImpact(Vector2 position2D_)
        {
            /// Get position and set raycast vector
            Vector3 position3D_ = new Vector3(position2D_.x, position2D_.y, this.gameObject.transform.position.z + 50f);
            Vector3 cameraForward = _hitboxCamera.transform.forward;
            Debug.DrawRay(position2D_, cameraForward * 10000, Color.yellow, 10.0f);

            /// Raycast target and action
            if (_targetsList.Count > 0)
            {
                RaycastHit hit;
                if (Physics.Raycast(position2D_, cameraForward, out hit))
                {
                    if (hit.collider != null && hit.transform.tag == "target")
                    {
                        _score += 1f * _damageReduce;

                        int targetLvl_ = hit.collider.GetComponent<TargetBehavior>().GetTargetLevel();
                        switch (targetLvl_)
                        {
                            case 1:
                                SetTargetsLvl2(position3D_);
                                break;
                            case 2:
                                SetTargetsLvl3(position3D_);
                                break;
                            case 3:
                                ResetKombo(position3D_);
                                break;
                            default:
                                break;
                        }

                        hit.collider.GetComponent<TargetBehavior>().SetHit(); // Inform the target she's been touched
                    }
                }
            }
        }

        /// <summary>
        /// TARGET LEVEL GENERATOR
        /// </summary>
        private void SetTargetsLvl1(Vector3 pos_)
        {
            int angle0_ = Random.Range(0, 360);
            int nTargets_ = _targetPropLvl1.colors.Length;

            /// Generate a beautiful crown of colorized targets all over the impact  ( *-*( m )
            for (int i = 0; i < nTargets_; i++)
            {
                SetTarget(pos_, _targetPropLvl1.colors[i], angle0_ + i * (float)360 / nTargets_, _targetPropLvl1, 1);
            }
        }
        private void SetTargetsLvl2(Vector3 pos_)
        {
            int angle0_ = Random.Range(0, 360);
            int nTargets_ = _komboLinks;

            /// Generate a beautiful crown of colorized targets all over the impact  ( *-*( m )
            for (int i = _komboHit; i < _komboHit + _komboLinks; i++)
            {
                int indxColor_ = _trgtsKomboLvl2[_komboLvl, i];
                SetTarget(pos_, _targetPropLvl2.colors[indxColor_], angle0_ + i * (float)360 / nTargets_, _targetPropLvl2, 2);
            }
            _komboHit += _komboLinks;
        }
        private void SetTargetsLvl3(Vector3 pos_)
        {
            
        }

        private void ResetKombo(Vector3 pos_)
        {
            _nHitLvl3++;
            if (_nHitLvl3 >= _totHitLvl3) {
                SetTargetsLvl1(pos_);
                _nHitLvl3 = 0;
            }
        }

        /// <summary>
        /// TARGET GENERATOR
        /// </summary>
        private void SetTarget(Vector3 position_, Color colTarget_, float angleDirection_, TargetProperties targetProp_, int targetLvl_)
        {
            // Instantiate target
            _targetsList.Add((GameObject)Instantiate(targetPrefab, position_, Quaternion.identity, this.gameObject.transform));

            // Set target properties
            GameObject target_ = _targetsList[_targetsList.Count - 1];                            // get last target which correspond to the current one

            // Set target properties
            target_.GetComponent<TargetBehavior>().SetTargetLevel(targetLvl_);
            target_.GetComponent<TargetBehavior>().SetLifeTime(targetProp_.lifeTime);
            target_.GetComponent<TargetBehavior>().SetAngleDirection(angleDirection_);
            target_.GetComponent<TargetBehavior>().SetColor(colTarget_);
            target_.GetComponent<TargetBehavior>().SetTranslationSpeed(targetProp_.transSpeed);
            target_.GetComponent<TargetBehavior>().SetRotationAxis(position_);
            target_.GetComponent<TargetBehavior>().SetRotationSpeed(targetProp_.rotSpeed);
            target_.GetComponent<TargetBehavior>().SetScale(targetProp_.scale);
        }

        private void Update()
        {
            for (int i = 0; i < _targetsList.Count; i++)
            {
                if (_targetsList[i] == null)
                    _targetsList.RemoveAt(i);

                if (_targetsList.Count == 0)
                {
                    Debug.Log("Score = " + _score);
                    _score = 0;
                    _damageReduce = 1;

                    _reachTargetsPosition.Clear();

                    this.gameObject.GetComponentInParent<GameManager>().SetScore(_score);

                    Destroy(this.gameObject); // good bye
                }
            }
        }
    }
}