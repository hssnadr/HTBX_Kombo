// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CRI.HitBoxTemplate.Serial
{
    /// <summary>
    /// Event Payload
    /// </summary>
    public struct ImpactPointControlEventArgs
    {
        /// <summary>
        /// The position of the impact that initiated the event.
        /// </summary>
        public Vector2 impactPosition { get; }
        /// <summary>
        /// Accelerometer value.
        /// </summary>
        public Vector3 accelerometer { get; }
        /// <summary>
        /// The index of the player that initiated the event.
        /// </summary>
        public int playerIndex { get; }

        public ImpactPointControlEventArgs(Vector2 impactPosition, Vector2 accelerometer, int playerIndex)
        {
            this.impactPosition = impactPosition;
            this.accelerometer = accelerometer;
            this.playerIndex = playerIndex;
        }
    }

    /// <summary>
    /// Event Payload
    /// </summary>
    /// <param name="sender">this object</param>
    /// <param name="e"><see cref="ImpactPointControlEventArgs"/></param>
    public delegate void ImpactPointControlEventHandler(object sender, ImpactPointControlEventArgs e);

    /// <summary>
    /// Check if there's an impact on the boxing bag.
    /// </summary>
    public class ImpactPointControl : MonoBehaviour
    {
        /// <summary>
        /// This event is fired whenever an impact is detected.
        /// </summary>
        public static event ImpactPointControlEventHandler onImpact;

        /// <summary>
        /// The point grid.
        /// </summary>
        private GameObject[] _pointGrid;
        /// <summary>
        /// An instance of a serial touch controller of the same player index used to get the accelerometer data.
        /// </summary>
        [Tooltip("An instance of a serial touch controller of the same player index used to get the accelerometer data.")]
        public SerialTouchController serialTouchController;
        /// <summary>
        /// An instance of a serial led controller of the same player index used to get the color data.
        /// </summary>
        [Tooltip("An instance of a serial led controller of the same player index used to get the color data.")]
        public SerialLedController serialLedController;
        /// <summary>
        /// If true, the impact point controll will ignore all impacts over a black background.
        /// </summary>
        [Tooltip("If true, the impact point control will ignore all impacts over a black background.")]
        public bool ignoreBlackBackground;
        /// <summary>
        /// X coordinate of current impact.
        /// </summary>
        private float _xG = 0f;
        /// <summary>
        /// Y coordinate of the current impact.
        /// </summary>
        private float _yG = 0f;
        /// <summary>
        /// Total pressure of current impact.
        /// </summary>
        private float _totG = 0;

        /// <summary>
        /// Min value to detect impact.
        /// </summary>
        [Tooltip("Min value to detect impact.")]
        public float threshImpact = 20;
        /// <summary>
        /// Minimum time (in ms) between 2 impacts to be validated (minimum 50ms <=> maximum 50 hits/s)
        /// </summary>
        [Tooltip("Minimum time (in ms) between 2 impacts to be validated (minimum 50ms <=> maximum 50 hits/s)")]
        public int delayOffHit = 50;
        /// <summary>
        /// Time of the last valid impact.
        /// </summary>
        private float _timerOffHit0 = 0;
        /// <summary>
        /// Position of the impact.
        /// </summary>
        private Vector3 _position;

        /// <summary>
        /// Position of the impact.
        /// </summary>
        public Vector3 position { get { return _position; } }

        /// <summary>
        /// The index of the player.
        /// </summary>
        [Tooltip("The index of the player.")]
        public int playerIndex = 0;
        /// <summary>
        /// Number of hit.
        /// </summary>
        private int _countHit = 0;

        private void Start()
        {
            _pointGrid = GameObject.FindGameObjectsWithTag("datapoint").Where(x => x.GetComponent<DatapointControl>().playerIndex == playerIndex).ToArray();
            serialLedController = GameObject.FindObjectsOfType<SerialLedController>().First(x => x.playerIndex == playerIndex);
            _timerOffHit0 = Time.time;
        }

        internal void OnImpact(Vector2 position)
        {
            if (onImpact != null)
                onImpact(this, new ImpactPointControlEventArgs(_position, serialTouchController.acceleration, playerIndex));
                
        }

        private void Update()
        {
            // Get instant center of pressure
            float totG_ = 0.0f;   // instant total pressure
            float xG_ = 0f;       // instant X coordinate of center of pressure
            float yG_ = 0f;       // instant Y coordinate of center of pressure;

            for (int i = 0; i < _pointGrid.Length; i++)
            {
                var datapoint = _pointGrid[i];
                var dpc = datapoint.GetComponent<DatapointControl>();
                if (dpc.curDerivVal > this.threshImpact && (!ignoreBlackBackground || IsColored(dpc.transform.position)))
                {
                    /////////////////////////////////////////////////////////////////////////////////////
                    /// /////////////////////////////////////////////////////////////////////////////////////
                    dpc.threshImpact = (int)this.threshImpact;   // TO REMOVE
                                                                                                        /////////////////////////////////////////////////////////////////////////////////////
                                                                                                        /// /////////////////////////////////////////////////////////////////////////////////////

                    totG_ += dpc.curRemapVal;
                    xG_ += dpc.curRemapVal * datapoint.transform.position.x;
                    yG_ += dpc.curRemapVal * datapoint.transform.position.y;
                }
            }

            // Get current impact
            if (1000 * (Time.time - _timerOffHit0) > this.delayOffHit && _totG != 0f)
            {
                // Get current impact positon
                _xG /= _totG;   // get X coordinate of current impact
                _yG /= _totG;   // get Y coordinate of current impact

                _position = new Vector3(_xG, _yG, 0);
                OnImpact(_position);

                _xG = 0;     // reset X coordinate of current impact
                _yG = 0;     // reset Y coordinate of current impact
                _totG = 0;   // reset pressure of current impact

                _countHit++;   // increment number of hit
                _timerOffHit0 = Time.time;
            }
            else
            {
                _xG += xG_;
                _yG += yG_;
                _totG += totG_;
            }
        }

        private bool IsColored(Vector3 position)
        {
            Vector3 point = serialLedController.playerCamera.WorldToScreenPoint(position);
            return serialLedController.cameraTexture.GetPixel((int)point.x, (int)point.y) != Color.black;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ImpactPointControl))]
    public class ImpactPointControlEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var ipc = (ImpactPointControl)target;

            if (Application.isPlaying && GUILayout.Button("Impact"))
                ipc.OnImpact(Vector2.zero);
        }
    }
#endif
}