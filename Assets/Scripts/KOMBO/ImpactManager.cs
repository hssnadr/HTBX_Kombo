using CRI.HitBoxTemplate.Serial;
using CRI.HitBoxTemplate.Example;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Hitbox.Kombo
{
    public class ImpactManager : MonoBehaviour
    {
        private Camera _hitboxCamera;
        [SerializeField]
        private Camera _debugCamera;

        [SerializeField]
        private GameObject _impactPrefabs;

        private void OnEnable()
        {
            ImpactPointControl.onImpact += OnHit;
        }

        private void OnDisable()
        {
            ImpactPointControl.onImpact -= OnHit;
        }

        private void Awake()
        {
            _hitboxCamera = this.gameObject.GetComponent<Camera>();
        }

        private void OnHit(object sender, ImpactPointControlEventArgs e)
        {
            // ----------- A CHECKER IF UTILE OU PAS -----------
            //if (Time.time - timerOffHit0 > delayOffHit)
            //{
            //    SetImpact(e.impactPosition);
            //    timerOffHit0 = Time.time;
            //}
            // -------------------------------------------------

            SetImpact(e.impactPosition);
        }

        private void SetImpact(Vector2 position2D_)
        {
            // Display a mark where impacts are detected
            Vector3 pos3DSprite_ = new Vector3(position2D_.x, position2D_.y, this.gameObject.transform.position.z + 100f); // set sprite in front of Hitbox camera
            Instantiate(_impactPrefabs, pos3DSprite_, Quaternion.identity, this.gameObject.transform);

            this.gameObject.GetComponent<GameManager>().GetInteractPoint(position2D_); 
        }

#if UNITY_EDITOR
        void OnMouseDown()
        {
            Vector3 mousePosition = Input.mousePosition;
            if (!_hitboxCamera.orthographic)
                mousePosition.z = this.transform.position.z;
            SetImpact(_debugCamera.ScreenToWorldPoint(mousePosition));
        }
        private void Update()
        {
            // Need to be into Update loop to be trigger without clicking on specific object
            if (Input.GetMouseButtonDown(0))
                OnMouseDown();
        }
#endif
    }
}