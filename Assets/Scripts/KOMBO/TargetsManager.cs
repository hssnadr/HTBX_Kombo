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

        public Color[] _colors
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
        { _lifeTime = lifeTime_; _scale = scale_; _transSpeed = transSpeed_; _rotSpeed = rotSpeed_; }
    }

    public class TargetsManager : MonoBehaviour
    {
        public GameObject targetPrefab;
        private List<GameObject> targetsList;			// list of current targets
        private List<Color> _reachTargetsColor;			// colors of reached targets
        private List<Vector3> _reachTargetsPosition;	// position of reached targets

        /// <summary>
        /// Here are specified the caracteristics for each target level
        /// </summary>
        [SerializeField]
        private Color _colorLvl1 = Color.white;
        [SerializeField]
        private TargetProperties _targetPropLvl1;    /// LEVEL 1
        [SerializeField]
        private Color[] _colorsLevel2 = new Color[3] {Color.red, Color.green, Color.blue};
        [SerializeField]
        private TargetProperties _targetPropLvl2;   /// LEVEL 2
        [SerializeField]
        private Color _colorLvl3 = Color.white;
        [SerializeField]
        private TargetProperties _targetPropLvl3;   ///  LEVEL 3

        private Camera _hitboxCamera;

        [SerializeField]
        private float _scr = 0f ; /// ! --> REMOVE THIS <-- ! ///
        private float _score = 0f;
        private int _comboMultiply = 1 ;

        private void Awake()
        {
            _hitboxCamera = this.gameObject.GetComponentInParent<Camera>();
            targetsList = new List<GameObject>();
            _reachTargetsColor = new List<Color>();
            _reachTargetsPosition = new List<Vector3>();

            /// Initialize target propertie levels
            _targetPropLvl1 = new TargetProperties(new Color[]{_colorLvl1}, 3.0f, 40.0f, 85.0f, 0.0f);      /// LEVEL 1
            _targetPropLvl2 = new TargetProperties(_colorsLevel2, 2.0f, 30.0f, 120.0f, 300.0f);             /// LEVEL 2
            _targetPropLvl3 = new TargetProperties(new Color[]{_colorLvl3}, 1.0f, 30.0f, 200.0f, -300.0f);  /// LEVEL 3
        }

        private void Start()
        {
            SetTargetsLvl1(this.transform.position);

            _score += 1 * _comboMultiply;
            _comboMultiply *= 2;
        }

		public void GetImpact(Vector2 position2D_)
		{
			/// Get position and set raycast vector
			Vector3 position3D_ = new Vector3(position2D_.x, position2D_.y, this.gameObject.transform.position.z + 50f);
			Vector3 cameraForward = _hitboxCamera.transform.forward;
			Debug.DrawRay(position2D_, cameraForward * 10000, Color.yellow, 10.0f);

			/// Raycast target and action
			Color targetColor_ = Color.black;
			if (targetsList.Count > 0)
			{
				RaycastHit hit;
				if (Physics.Raycast(position2D_, cameraForward, out hit))
				{
					if (hit.collider != null && hit.transform.tag == "target")
					{
                        int targetLvl_ = hit.collider.GetComponent<TargetBehavior>().TargetLevel;
                        switch (targetLvl_) {
                            case 1:
                                HitTargetLvl1(hit.collider.gameObject);
                                break;
                            case 2:
                                HitTargetLvl2(hit.collider.gameObject);
                                break;
                            case 3:
                                HitTargetLvl3(hit.collider.gameObject);
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
        /// TARGET HIT ACTION
        /// </summary>
        private void HitTargetLvl1(GameObject target_) { 
        }
        private void HitTargetLvl2(GameObject target_)
        {
        }
        private void HitTargetLvl3(GameObject target_)
        {
        }

        /// <summary>
        /// TARGET LEVEL GENERATOR
        /// </summary>
        private void SetTargetsLvl1(Vector3 pos_) {
            SetCrownTargets(pos_, _targetPropLvl1); // LEVEL 1
        }        
        private void SetTargetLvl2(Vector3 pos_)
        {
            SetCrownTargets(pos_, _targetPropLvl2); // LEVEL 2
        }        
        private void SetTargetsLvl3(Vector3 pos_)
        {
            SetCrownTargets(pos_, _targetPropLvl3); // LEVEL 3
        }

        private void SetCrownTargets(Vector3 position_, TargetProperties targetProp_)
        {
            Color[] colTargets_ = targetProp_._colors;
            int angle0_ = Random.Range(0, 360);
            int nTargets_ = colTargets_.Length;

            /// Generate a beautiful crown of colorized targets all over the impact  ( *-*( m )
            for (int i = 0; i < nTargets_; i++)
            {
                SetTarget(position_, colTargets_[i], angle0_ + i * (float)360 / nTargets_, targetProp_);
            }
        }

        private void SetTarget(Vector3 position_, Color colTarget_, float angleDirection_, TargetProperties targetProp_)
        {
            // Instantiate target
            GameObject target_ = Instantiate(targetPrefab, position_, Quaternion.identity) as GameObject;

            // Set target properties
            target_.GetComponent<TargetBehavior>().LifeTime = targetProp_.lifeTime;
            target_.GetComponent<TargetBehavior>().Angle = angleDirection_;
            target_.GetComponent<TargetBehavior>().TargetColor = colTarget_;
            target_.GetComponent<TargetBehavior>().TranslationSpeed = targetProp_.transSpeed;
            target_.GetComponent<TargetBehavior>().RotationAxis = position_;
            target_.GetComponent<TargetBehavior>().RotationSpeed = targetProp_.rotSpeed;
            target_.GetComponent<TargetBehavior>().SetScale(targetProp_.scale);
        }

private void Update()
        {
            for (int i = 0; i < targetsList.Count; i++) {
                if (targetsList[i] == null)
                    targetsList.RemoveAt(i);

                if (targetsList.Count == 0)
                {
                    Debug.Log("Score = " + _score);
                    _score = 0;
                    _comboMultiply = 1;

                    _reachTargetsColor.Clear();
                    _reachTargetsPosition.Clear();

                    //serialController.EndGame();

                    this.gameObject.GetComponentInParent<GameManager>().SetScore(_scr);

                    Destroy(this.gameObject); // good bye
                }
            }
        }
    }
}