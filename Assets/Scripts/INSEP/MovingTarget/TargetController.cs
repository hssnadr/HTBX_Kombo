// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CRI.HitBoxTemplate.Serial;
using System.Linq;
using UnityEngine;

namespace INSEP
{
     /// <summary>
     /// Event Payload
     /// </summary>
    public struct TargetControllerHitEventArgs
    {
        /// <summary>
        /// Index of the player that initiated the hit.
        /// </summary>
        public int playerIndex { get; }
        /// <summary>
        /// Position of the hit.
        /// </summary>
        public Vector2 position { get; }
        /// <summary>
        /// Whether the hit was successful or not.
        /// </summary>
        public bool successful { get; }
        /// <summary>
        /// ID of the reached game object
        /// </summary>
        public int gameObjectID { get; }

        public TargetControllerHitEventArgs(int playerIndex, Vector2 position, bool successful, int id)
        {
            this.playerIndex = playerIndex;
            this.position = position;
            this.successful = successful;
            this.gameObjectID = id;
        }
    }

    public delegate void TargetControllerHitEventHandler(object sender, TargetControllerHitEventArgs e);
    [RequireComponent(typeof(AudioSource))]
    public class TargetController : MonoBehaviour
    {
        public static event TargetControllerHitEventHandler onHit;

        public int playerIndex;

        public Camera playerCamera;

        [SerializeField]
        private Target[] _targets;

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
            _targets = GetComponentsInChildren<Target>(true);
        }

        private void OnImpact(object sender, ImpactPointControlEventArgs e)
        {
            OnImpact(e.impactPosition, e.playerIndex);
        }

        private void OnImpact(Vector3 position, int playerIndex)
        {
            if (this.playerIndex == playerIndex)
            {
                // Layer of the player
                LayerMask layerMask = 1 << (8 + 1 + playerIndex);
                Vector3 cameraForward = playerCamera.transform.forward;
                Debug.DrawRay(position, cameraForward * 5000, Color.yellow, 10.0f);
                var hits = Physics.RaycastAll(position, cameraForward, Mathf.Infinity, layerMask);
                if (hits != null && hits.Any(x => x.collider.GetComponent<Target>() != null))
                {
                    var hitTargets = hits
                        .Where(
                            x => x.collider.GetComponent<Target>() != null
                            )
                        .OrderBy(
                            x => x.transform.position.z * cameraForward.z
                        );
                    var first = hitTargets.First();
                    Hit(first, position);
                    bool direction = (first.collider.transform.position.z - first.collider.GetComponent<Target>().zPosition) * cameraForward.z >= 0;
                    GetComponentInParent<MovementController>().Hit(direction ? cameraForward : -cameraForward, first);
                }
            }
        }

        private void Hit(RaycastHit hit, Vector2 position)
        {
            hit.collider.GetComponent<Target>().Hit();
            int id_ = hit.collider.gameObject.GetInstanceID();
            if (onHit != null)
                onHit(this, new TargetControllerHitEventArgs(playerIndex, position, true, id_));
        }

#if UNITY_EDITOR
        private void OnMouseDown()
        {
            if (GetComponentInParent<MovementController>().mousePlayerIndex == playerIndex)
            {
                Vector3 mousePosition = Input.mousePosition;
                if (!playerCamera.orthographic)
                    mousePosition.z = this.transform.position.z;
                OnImpact(playerCamera.ScreenToWorldPoint(mousePosition), playerIndex);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) {
                OnMouseDown();
            }
        }
#endif
    }
}
