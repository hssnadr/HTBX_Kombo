using CRI.HitBoxTemplate.Example;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INSEP
{
    public class TargetGenerator : MonoBehaviour
    {
        [SerializeField]
        private GameObject _targetAttack;
        [SerializeField]
        private GameObject _targetDefense;
        private List<GameObject> _targetsList;       // list of current targets

        /// <summary>
        /// Prefab of the feedback object.
        /// </summary>
        [SerializeField]
        [Tooltip("Prefab of the feedback object.")]
        private HitFeedbackAnimator _hitFeedbackPrefab = null;

        public LineRenderer lineDefense;
		public Material matLineDefense;
		public Color[] defenseColors;
		private int _curColor = 0;

		[SerializeField]
		private float _radiusTargetCrown = 3.0f;

		private void Awake()
        {
            _targetsList = new List<GameObject>();

            // Instantiate targets
            AddTarget(_targetDefense, new Vector3(_radiusTargetCrown, 0, 0));
            AddTarget(_targetDefense, new Vector3(0, _radiusTargetCrown, 0));
            AddTarget(_targetDefense, new Vector3(-1.0f * _radiusTargetCrown, 0, 0));
            AddTarget(_targetDefense, new Vector3(0, -1.0f * _radiusTargetCrown, 0));
        }

        private void OnEnable()
        {
            TargetController.onHit += UpdateTargets;
        }

        private void OnDisable()
        {
            TargetController.onHit -= UpdateTargets;
        }

        void UpdateTargets(object sender, TargetControllerHitEventArgs e)
        {
            for (int i = 0; i < _targetsList.Count; i++)
            {
                if (int.Equals(e.gameObjectID, _targetsList[i].GetInstanceID()))
                {
                    SwapToTargetType(i, _targetDefense);
                }
            }
        }

        public void AddTarget(GameObject target_, Vector3 position_)
        {
            var newTarget_ = Instantiate(target_, position_, Quaternion.identity);
            newTarget_.transform.parent = gameObject.transform;
            newTarget_.transform.localPosition = position_;
            _targetsList.Add(newTarget_);
        }

        public void SetRandomAttackTarget(int n_)
        {
            List<int> index_ = new List<int>();
            for (int i = 0; i < _targetsList.Count; i++)
            {
                index_.Add(i);
            }

            for (int i = 0; i < n_; i++)
            {
                int randIndx1_ = Mathf.FloorToInt(Random.Range(0, index_.Count));
                int randIndx2_ = index_[randIndx1_];

                SwapToTargetType(randIndx2_, _targetAttack);
                index_.RemoveAt(randIndx1_);
            }
        }

        public void SwitchAllToDefense()
        {
            for(int i=0; i < _targetsList.Count; i++)
            {
                SwapToTargetType(i, _targetDefense);
            }
        }

        public void SwapToTargetType(int targetIndex_, GameObject toTypeTarget_)
        {
            var pos_ = _targetsList[targetIndex_].transform.localPosition;
            RemoveTarget(targetIndex_);

            // Insert new target at position of the previous one inside the list
            var newTarget_ = Instantiate(toTypeTarget_, pos_, Quaternion.identity);
            newTarget_.transform.parent = gameObject.transform;
            newTarget_.transform.localPosition = pos_;
            _targetsList.Insert(targetIndex_, newTarget_);
        }

        public void RemoveTarget(int ind_)
        {
            if (ind_ >= 0 && ind_ < _targetsList.Count)
            {
                // Feedback flash animation
                if (_hitFeedbackPrefab != null)// && _targetsList[ind_].tag == "targetAttack")
                {
                    var pos_ = _targetsList[ind_].transform.localPosition;
                    var go = GameObject.Instantiate(_hitFeedbackPrefab, this.transform);
                    go.transform.localPosition = pos_;
                    go.gameObject.layer = this.gameObject.layer;
                }

                // Destroy target
                Destroy(_targetsList[ind_]);
                _targetsList.RemoveAt(ind_);
            }
        }

        public void DefenseAnimation()
        {
            StartCoroutine(DefenseAnimationCoroutine());
        }

        private IEnumerator DefenseAnimationCoroutine()
        {
			// Set color
			matLineDefense.color = defenseColors[_curColor];
			_curColor++;
			_curColor %= defenseColors.Length;

			// Set line
			lineDefense.positionCount = _targetsList.Count + 1;
            for (int i = 0; i < _targetsList.Count; i++)
            {
                lineDefense.SetPosition(i, _targetsList[i].transform.position);
            }
            lineDefense.SetPosition(_targetsList.Count, _targetsList[0].transform.position); // close shape

            yield return new WaitForSeconds(.2f);
            lineDefense.positionCount = 0;
        }
    }
}
