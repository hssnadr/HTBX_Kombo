using CRI.HitBoxTemplate.Example;
using System.Collections;
using UnityEngine;

namespace INSEP
{
	public class SessionManager : MonoBehaviour
	{
		public ExampleSerialController serialController;

		public bool _isPlaying = false;

		private int _score = 0;
		private float _curCadence = 0.0f;
		public int _curSessionTime = 0;
		public bool _onAttack = false;

		private float _sessionTimer0 = 0.0f;
		private float _actionTimer0 = 0.0f;
		public int _nAttackAction = 0;

		public GameObject targetGenerator;

		[SerializeField]
		private SessionTime[] _sessionTimes;

		private void OnEnable()
		{
			TargetController.onHit += Count;
		}

		private void OnDisable()
		{
			TargetController.onHit -= Count;
		}

		void Count(object sender, TargetControllerHitEventArgs e)
		{
			if (_curSessionTime == 0 && _score == 0)
			{
				// Start session on first hit
				_sessionTimer0 = Time.time;
				_curSessionTime = 0;
				_actionTimer0 = Time.time;
				_isPlaying = true;
			}

			_nAttackAction++;
			_score++;

			if (_nAttackAction >= _sessionTimes[_curSessionTime].nAttack)
			{
				_nAttackAction = 0;

				// Start defense coroutine
				float _elapsCurTimeAction = Time.time - _actionTimer0;
				float dlyDefense_ = 60.0f / _sessionTimes[_curSessionTime].rythm - _elapsCurTimeAction;
				StartCoroutine(TrigDefenseAction(dlyDefense_, _sessionTimes[_curSessionTime].nDefense));
			}
		}

		private void Start()
		{
			_curSessionTime = 0;
			targetGenerator.GetComponent<TargetGenerator>().SetRandomAttackTarget(_sessionTimes[_curSessionTime].nAttack);
		}

		void Update()
		{
			if (_isPlaying)
			{
				_curCadence = _score / (Time.time - _sessionTimer0);

				//------------------
				//-------ACTIVE-----
				//------------------
				if (Time.time - _sessionTimer0 < _sessionTimes[_curSessionTime].activeTime)
				{
					_onAttack = true;
				}
				else
				{
					//------------------
					//-----RECORVERY----
					//------------------
					if (Time.time - _sessionTimer0 < _sessionTimes[_curSessionTime].activeTime + _sessionTimes[_curSessionTime].recoveryTime)
					{
						if (_onAttack)
						{
							targetGenerator.GetComponent<TargetGenerator>().SwitchAllToDefense();

							if (serialController != null)
								serialController.ScreenSaver();
							// --> disable Unity screen to Hitbox
						}
						_onAttack = false;
					}
					else
					{
						_curSessionTime++;
						_sessionTimer0 = Time.time;
						_actionTimer0 = Time.time;
						_score = 0;

						if (_curSessionTime >= _sessionTimes.Length)
						{
							if (_isPlaying)
							{
								if (serialController != null)
									serialController.EndGame();
								// --> disable Unity screen to Hitbox
								// then wait for automatic save mode
							}
							_isPlaying = false;
						}
						else
						{
							targetGenerator.GetComponent<TargetGenerator>().SetRandomAttackTarget(_sessionTimes[_curSessionTime].nAttack);
						}
					}
				}
			}
		}

		private IEnumerator TrigDefenseAction(float time_, int n_)
		{
			float timePerDef_ = time_ / (float)(n_ + 1);
			if (timePerDef_ < 0)
				timePerDef_ = 0;
			Debug.Log(timePerDef_);

			var targetGenerator_ = targetGenerator.GetComponent<TargetGenerator>();

			yield return new WaitForSeconds(timePerDef_);

			if (n_ > 0)
			{
				for (int i = 0; i < n_; i++)
				{
					targetGenerator_.DefenseAnimation();
					yield return new WaitForSeconds(timePerDef_);
				}
			}

			if (_onAttack)
			{
				targetGenerator_.SetRandomAttackTarget(_sessionTimes[_curSessionTime].nAttack);
			}

			_actionTimer0 = Time.time;
		}
	}
}