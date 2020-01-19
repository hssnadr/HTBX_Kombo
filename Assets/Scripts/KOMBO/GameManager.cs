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

        private TargetsManager targetsManager;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void SetScore() {
            // Launch score animation
            Vector3 gaugePosition_ = this.gameObject.transform.position;
            gaugePosition_.z += 100;

            GameObject go = Instantiate(_scoreGaugePrefab, gaugePosition_, Quaternion.identity, this.gameObject.transform) as GameObject;
            go.SendMessage("SetScore", 28);
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