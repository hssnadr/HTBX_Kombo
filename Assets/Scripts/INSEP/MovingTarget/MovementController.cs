// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace INSEP
{
    public class MovementController : MonoBehaviour
    {
        /// <summary>
        /// The rotation speed of the movement controller when hit by a force.
        /// </summary>
        [Tooltip("The rotation speed of the movement controller when hit by a force.")]
        [SerializeField]
        private float _rotationSpeed = 50.0f;
        /// <summary>
        /// Z Rotation speed of the movement controller.
        /// </summary>
        [Tooltip("Z Rotation speed of the movement controller.")]
        [SerializeField]
        private float _zRotationSpeed = 20.0f;
        /// <summary>
        /// The max angular velocity of the rigidbody.
        /// </summary>
        [Tooltip("The max angular velocity of the rigidbody.")]
        [SerializeField]
        private float maxAngularVelocity = 3.0f;

        [SerializeField]
        private float elasticityFactor = 0.8f;

        private Rigidbody _rigidbody;

#if UNITY_EDITOR
        /// <summary>
        /// (EDITOR-ONLY) The index of the player when a mouse click occurs.
        /// </summary>
        [Tooltip("(EDITOR-ONLY) The index of the player when a mouse click occurs.")]
        [SerializeField]
        private int _mousePlayerIndex = 0;

        public int mousePlayerIndex
        {
            get
            {
                return _mousePlayerIndex;
            }
        }
#endif

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            _rigidbody.maxAngularVelocity = maxAngularVelocity;
            transform.RotateAround(transform.position, Vector3.forward, _zRotationSpeed * Time.fixedDeltaTime);

            transform.rotation = new Quaternion(this.transform.rotation.x * elasticityFactor, this.transform.rotation.y * elasticityFactor, this.transform.rotation.z, this.transform.rotation.w);
        }

        public void Hit(Vector3 cameraForward, RaycastHit hit)
        {
            //_rigidbody.AddForceAtPosition(cameraForward * _rotationSpeed, hit.point, ForceMode.Impulse);
            _rigidbody.AddForceAtPosition(cameraForward * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}