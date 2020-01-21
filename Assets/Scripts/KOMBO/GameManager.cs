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
        private float _scoreReference = 66.6f;

        private TargetsManager targetsManager;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void SetScore(float score_) {
            StartCoroutine(DisplayScores(score_));
        }

        IEnumerator DisplayScores(float newScore_)
        {
            // Launch score gauge player animation
            GameObject gaugPlayer_ = Instantiate(_scoreGaugePrefab, this.gameObject.transform) as GameObject;
            gaugPlayer_.SendMessage("SetPlayerScore", newScore_);

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(1.5f);

            // Launche score gauge reference animation
            GameObject gaugReference_ = Instantiate(_scoreGaugePrefab, this.gameObject.transform) as GameObject;
            gaugReference_.SendMessage("SetPlayerScore", _scoreReference);
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