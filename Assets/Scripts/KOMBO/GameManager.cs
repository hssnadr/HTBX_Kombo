using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Kombo
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _komboPrefab;
        [SerializeField]
        private GameObject _scoreGaugePrefab;

        [SerializeField]
        private float _scoreReference = 16.6f;
        [SerializeField]
        private Color _playerColor = Color.green;
        [SerializeField]
        private Color _referenceColor = Color.blue;

        private TargetsManager targetsManager;

        public void SetScore(float newScore_) {
            StartCoroutine(DisplayScores(newScore_));
        }

        IEnumerator DisplayScores(float scorePlayer_)
        {
            // Launch score gauge player animation
            GameObject gaugPlayer_ = Instantiate(_scoreGaugePrefab, this.gameObject.transform) as GameObject;
            gaugPlayer_.SendMessage("SetGaugeColor", _playerColor);
            gaugPlayer_.SendMessage("SetPlayerScore", scorePlayer_);
            var ply_ = gaugPlayer_.GetComponent<ScoreGaugeBehavior>();
            StartCoroutine(ply_.EntranceAnimation());

            // Delay reference display
            yield return new WaitForSeconds(0.75f);

            // Launche score gauge reference animation
            GameObject gaugReference_ = Instantiate(_scoreGaugePrefab, this.gameObject.transform) as GameObject;
            gaugReference_.SendMessage("SetGaugeColor", _referenceColor);
            gaugReference_.SendMessage("SetPlayerScore", _scoreReference);
            var ref_ = gaugReference_.GetComponent<ScoreGaugeBehavior>();
            yield return StartCoroutine(ref_.EntranceAnimation());

            //// Animation Victory / Defeat
            print("FINAL ANIM");
            if (scorePlayer_ > _scoreReference)
            {
                StartCoroutine(ref_.FadeOutAnimation(-0.8f));
                yield return StartCoroutine(ply_.VictoryAnimation());
            }
            else {
                StartCoroutine(ref_.FadeOutAnimation(+1.5f));
                yield return StartCoroutine(ply_.DefeatAnimation());
            }

            // Destroy gauges and update score
            print("DONE");
            //GameObject.Destroy(gaugPlayer_);
            //GameObject.Destroy(gaugReference_);
            _scoreReference = scorePlayer_;
        }

        public void GetInteractPoint(Vector2 pos2D_)
        {
            Debug.Log("Interact point at" + pos2D_);

            if (targetsManager == null)
            {
                // Start KOMBO game
                Vector3 gamePosition_ = new Vector3(pos2D_.x, pos2D_.y, 10);
                gamePosition_.z += this.gameObject.transform.position.z;

                var _game = Instantiate(_komboPrefab, gamePosition_, Quaternion.identity, this.gameObject.transform);
                targetsManager = _game.GetComponent<TargetsManager>();
            }
            else {
                // Kombo game is running
                targetsManager.GetImpact(pos2D_);
            }            
        }
    }
}