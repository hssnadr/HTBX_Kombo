﻿using CRI.HitBoxTemplate.Serial;
using CRI.HitBoxTemplate.Example;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Hitbox.Kombo
{
    public class ImpactManager : MonoBehaviour
    {
        public ExampleSerialController serialController;
        private Camera _hitboxCamera;
        [SerializeField]
        private Camera _debugCamera;

        [SerializeField]
        private GameObject _hitPrefabs;

        private void OnEnable()
        {
            ImpactPointControl.onImpact += OnImpact;
        }

        private void OnDisable()
        {
            ImpactPointControl.onImpact -= OnImpact;
        }

        private void Awake()
        {
            _hitboxCamera = this.gameObject.GetComponent<Camera>();
        }

        private void OnImpact(object sender, ImpactPointControlEventArgs e)
        {
            Debug.Log(string.Format("Impact: Player [{0}], Position [{1}], Accelerometer [{2}]",
                e.playerIndex,
                e.impactPosition,
                e.accelerometer));
        }

        private void SetImpact(Vector2 position2D_)
        {
            // Display a mark where impacts are detected
            Vector3 pos3DSprite_ = new Vector3(position2D_.x, position2D_.y, this.gameObject.transform.position.z + 10f); // set sprite in front of Hitbox camera
            Instantiate(_hitPrefabs, pos3DSprite_, Quaternion.identity, this.gameObject.transform);
        }

#if UNITY_EDITOR
        void OnMouseDown()
        {
            Vector3 mousePosition = Input.mousePosition;
            Debug.Log(mousePosition);
            if (!_hitboxCamera.orthographic)
                mousePosition.z = this.transform.position.z;
            SetImpact(_debugCamera.ScreenToWorldPoint(mousePosition));
        }
        private void Update()
        {
            // Need to be into Update loop to be trigger without clicking on specific object
            if (Input.GetMouseButtonDown(0))
                OnMouseDown();
#endif
        }
    }
}
