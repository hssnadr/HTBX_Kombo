//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Kombo
{
    [System.Serializable]
    public struct TargetProperties
    {
        private int _level;
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

        public int Level
        {
            set
            {
                _level = value;
            }
            get
            {
                return _level;
            }
        }

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

        public TargetProperties(int targetLevel_, Color[] colors_, float lifeTime_, float scale_, float transSpeed_, float rotSpeed_)
        { _level = targetLevel_; _colors = colors_; _lifeTime = lifeTime_; _scale = scale_; _transSpeed = transSpeed_; _rotSpeed = rotSpeed_; }
    }

    public class TargetsManager : MonoBehaviour
    {
        public GameObject targetPrefab;
        private List<GameObject> _targetsList;			// list of current targets

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
        [SerializeField]
        private float _coefScore = 1f;
        private float _damageReduce;
        [SerializeField]
        private float _coefDamageReduce = 3f;

        // On Hit level 1
        private int[,] _trgtsKomboLvl2 = new int[4,6];
        private System.Random _random = new System.Random();
        private int[] _curKomboSequence;
        [SerializeField]
        private int _komboLinks;     // Number of targets per kombo init
        private int _komboLvl;       // Current Kombo level
        private int _komboTrgt;      // Current Kombo hit apparition
        private int _curKomboSerie;  // Current Kombo serie

        // On hit level 2
        private Vector3[,] _posHitTrgtsLvl2; // Hit positions of targets level 2 into a single kombo

        // On hit level 3
        private int _nHitLvl3 = 0;
        private int _totHitLvl3 = 2;

        private void Awake()
        {
            _hitboxCamera = this.gameObject.GetComponentInParent<Camera>();
            _targetsList = new List<GameObject>();

            /// Initialize target propertie levels
            //_targetPropLvl1 = new TargetProperties(new Color[] { Color.white }, 3.0f, 40.0f, 85.0f, 0.0f);      /// LEVEL 1
            //_targetPropLvl2 = new TargetProperties(new Color[] { Color.white }, 2.0f, 30.0f, 120.0f, 300.0f);   /// LEVEL 2
            //_targetPropLvl3 = new TargetProperties(new Color[] { Color.white }, 1.0f, 30.0f, 200.0f, -300.0f);  /// LEVEL 3
            _targetPropLvl1.Level = 1;
            _targetPropLvl2.Level = 2;
            _targetPropLvl3.Level = 3;

            // Initialize Level 2 /// Set Kombo
            _trgtsKomboLvl2 = new int[,]
            {
                {0, 1, 0, 2, 1, 2},
                {1, 2, 1, 0, 0, 2},
                {0, 0, 1, 2, 1, 2},
                {0, 1, 0, 1, 2, 2}
            };
            _curKomboSequence = new int[6];

            // Initialize Level 3
        }

        private void Start()
        {
            _komboLvl = 0; // First Kombo level
            _komboLinks = 2;
            _score = 0f;
            _damageReduce = 1f;
            SetTargetsLvl1(this.transform.position);
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
                        _score += _coefScore * _coefDamageReduce * _damageReduce; // Update score

                        int targetLvl_ = hit.collider.GetComponent<TargetBehavior>().GetTargetLevel();
                        switch (targetLvl_)
                        {
                            case 1:
                                SetTargetsLvl2(position3D_);
                                break;
                            case 2:
                                SetTargetsLvl3(hit.collider.gameObject, position3D_);
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

        void Shuffle(int[] array)
        {
            int p = array.Length;
            for (int n = p - 1; n > 0; n--)
            {
                int r = _random.Next(0, n);
                int t = array[r];
                array[r] = array[n];
                array[n] = t;
            }
        }

        /// <summary>
        /// TARGET LEVEL GENERATOR
        /// </summary>
        private void SetTargetsLvl1(Vector3 pos_)
        {
            _komboLvl++;

            // Reset Kombo parameters
            _curKomboSerie = 0;
            _komboTrgt = 0;
            int nTargetsLvl2_ = _targetPropLvl2.colors.Length;
            int komboCombination_ = _trgtsKomboLvl2.GetLength(1) / nTargetsLvl2_;
            // Update damage reduce
            _damageReduce *= 1f / _komboLvl;
            _damageReduce = Mathf.Clamp(_damageReduce, 0f, 1f);

            // Set current kombo
            if (_komboLvl < _trgtsKomboLvl2.GetLength(0))
            {
                for (int i = 0; i < _curKomboSequence.Length; i++)
                {
                    _curKomboSequence[i] = _trgtsKomboLvl2[_komboLvl, i];
                }
            }
            else
            {
                Shuffle(_curKomboSequence); // Shuffle last kombo sequence
            }

            _posHitTrgtsLvl2 = new Vector3[komboCombination_, nTargetsLvl2_];

            Debug.Log("---------------------------------------------------------------------------------------------------------");
            Debug.Log("CURRENT LEVEL = " + _komboLvl);
            Debug.Log("CURRENT SCORE = " + _score);
            Debug.Log("Damage Reduce = " + _damageReduce);

            int angle0_ = Random.Range(0, 360);
            int nTargets_ = _targetPropLvl1.colors.Length;

            // Generate a beautiful crown of colorized targets all over the impact  ( *-*( m )
            for (int i = 0; i < nTargets_; i++)
            {
                SetTarget(pos_, i, angle0_ + i * (float)360 / nTargets_, _targetPropLvl1);
            }
        }
        private void SetTargetsLvl2(Vector3 pos_)
        {
            int angle0_ = Random.Range(0, 360);
            int nTargets_ = _komboLinks;

            // Generate a beautiful crown of colorized targets all over the impact  ( *-*( m )
            for (int i = _komboTrgt; i < _komboTrgt + _komboLinks; i++)
            {
                if(i < _curKomboSequence.Length)
                {
                    int trgtType_;
                    trgtType_ = _curKomboSequence[i];

                    SetTarget(pos_, trgtType_, angle0_ + i * (float)360 / nTargets_, _targetPropLvl2);
                }
            }
            _komboTrgt += _komboLinks;
        }
        private void SetTargetsLvl3(GameObject trgt_, Vector3 pos_)
        {
            int indType_ = trgt_.GetComponent<TargetBehavior>().GetTargetType();
            for (int i = 0; i < _posHitTrgtsLvl2.GetLength(0); i++) {
                if (_posHitTrgtsLvl2[i, indType_].Equals(Vector3.zero)) {
                    _posHitTrgtsLvl2[i, indType_] = pos_;   // update value
                    break; // update only once
                }
            }

            // FOR DEBUG
            //print("VVVVVVVVVVVVVV");
            //for (int i = 0; i < _posHitTrgtsLvl2.GetLength(0); i++)
            //{
            //    string _print = "";
            //    for (int j = 0; j < _posHitTrgtsLvl2.GetLength(1); j++)
            //    {
            //        _print += _posHitTrgtsLvl2[i, j].Equals(Vector3.zero);
            //        _print += " ";
            //    }
            //    print(_print);
            //}
            //print("--------------");

            // Check current sub Kombo combinaison
            float xG_ = 0f;
            float yG_ = 0f;
            for (int j = 0; j < _posHitTrgtsLvl2.GetLength(1); j++)
            {
                if (!_posHitTrgtsLvl2[_curKomboSerie, j].Equals(Vector3.zero))
                {
                    xG_ += _posHitTrgtsLvl2[_curKomboSerie, j].x / _targetPropLvl2.colors.Length;
                    yG_ += _posHitTrgtsLvl2[_curKomboSerie, j].y / _targetPropLvl2.colors.Length;

                    if (j == _posHitTrgtsLvl2.GetLength(1) - 1) {
                        Vector3 newPosLvl3_ = new Vector3(xG_, yG_, pos_.z);
                        SetTarget(newPosLvl3_, 0, 0, _targetPropLvl3);   // create target level 3
                        _curKomboSerie++;

                        Debug.Log("KOMBO " + _curKomboSerie);
                    }
                }
                else {
                    break; // cancel level 3 if kombo not complete
                }
            }
        }

        private void ResetKombo(Vector3 pos_)
        {
            _nHitLvl3++;
            if (_nHitLvl3 >= _totHitLvl3) {
                _nHitLvl3 = 0;
                SetTargetsLvl1(pos_);                
            }
        }

        /// <summary>
        /// TARGET GENERATOR
        /// </summary>
        private void SetTarget(Vector3 position_, int targetType_, float angleDirection_, TargetProperties targetProp_)
        {
            // Instantiate target
            _targetsList.Add((GameObject)Instantiate(targetPrefab, position_, Quaternion.identity, this.gameObject.transform));

            // Set target properties
            GameObject target_ = _targetsList[_targetsList.Count - 1];                            // get last target which correspond to the current one

            // Set target properties
            target_.GetComponent<TargetBehavior>().SetTargetLevel(targetProp_.Level);
            target_.GetComponent<TargetBehavior>().SetTargetType(targetType_);
            target_.GetComponent<TargetBehavior>().SetLifeTime(targetProp_.lifeTime);
            target_.GetComponent<TargetBehavior>().SetAngleDirection(angleDirection_);
            target_.GetComponent<TargetBehavior>().SetColor(targetProp_.colors[targetType_]);
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
                    this.gameObject.GetComponentInParent<GameManager>().SetScore(_score);

                    //_score = 0;
                    //_damageReduce = 1;
                    Destroy(this.gameObject); // good bye
                }
            }
        }
    }
}