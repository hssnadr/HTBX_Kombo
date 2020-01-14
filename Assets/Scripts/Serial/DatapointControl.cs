// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using System.Collections.Generic;

namespace CRI.HitBoxTemplate.Serial
{
    public class DatapointControl : MonoBehaviour
    {
        /// <summary>
        /// Array list to store each new incoming raw data.
        /// </summary>
        private List<float> _rawVals = new List<float>();
        /// <summary>
        /// Size of the raw data list.
        /// </summary>
        [Tooltip("Size of the raw data list.")]
        public int N = 15;
        /// <summary>
        /// Current smooth data = mean of the current raw data list.
        /// </summary>
        private float _curSmoothVal = 0.0f;
        /// <summary>
        /// Last smooth data.
        /// </summary>
        private float _oldSmoothVal = 0.0f;
        /// <summary>
        /// Difference between current and previous smooth data.
        /// </summary>
        private float _curDerivVal = 0.0f;
        /// <summary>
        /// Reference data value.
        /// </summary>
        private float _smoothValOffset = 0.0f;
        /// <summary>
        /// CurSmoothVal offset from smoothValOffset (curSRelativeVal = curSmoothVal - smoothValOffset).
        /// </summary>
        [Tooltip("CurSmoothVal offset from smoothValOffset (curSRelativeVal = curSmoothVal - smoothValOffset).")]
        public float curSRelativeVal = 0.0f;
        /// <summary>
        /// Remap data based on the entire row, range between 0.0 and 1.0.
        /// </summary>
        [Tooltip("Remap data based on the entire row, range between 0.0 and 1.0.")]
        public float curRemapVal = 0.0f;

        public SerialTouchController touchSurface;

        /// <summary>
        /// Difference between current and previous smooth data.
        /// </summary>
        public float curDerivVal
        {
            get { return _curDerivVal; }
        }

        /// <summary>
        /// The corresponding player index.
        /// </summary>
        [Tooltip("The corresponding player index.")]
        public int playerIndex = 0;

        /////////////////////////////////////////////////////////////////////////////////////////
        /// /////////////////////////////////////////////////////////////////////////////////////////
        public int threshImpact = 0; // TO REMOVE /////////////////////////////////////////////////////////////////////////////////////////
                                     /////////////////////////////////////////////////////////////////////////////////////////
                                     /// /////////////////////////////////////////////////////////////////////////////////////////

        // Colors
        //private Color _curCol = Color.white;
        private Color _red = new Color(245f / 255f, 91f / 255f, 85f / 255f);
        private Color _blue = new Color(125f / 255f, 222f / 255f, 227f / 255f);
        private Color _yellow = new Color(243f / 255f, 240f / 255f, 114f / 255f);
        private Color _purple = new Color(73f / 255f, 81f / 255f, 208f / 255f);

        public int maxRadius = 8;
        private Vector3 _oldAcceleration = Vector3.zero;

        private void Update()
        {
            if (Input.GetKeyDown("space"))
                SetOffsetValue();

            ShiftRawVal();

            //		this.curCol = getLerpColor(this.curRemapVal);
           // _curCol = GetLerpColor(this.curDerivVal);

            //this.gameObject.GetComponent<Renderer>().material.color = _curCol;

            // DIPLAY POINT
            //this.gameObject.transform.localScale = new Vector3 (this.maxRadius*this.curRemapVal,this.maxRadius*this.curRemapVal,this.maxRadius*this.curRemapVal);
            if (this.curDerivVal > this.threshImpact)
            {
                this.gameObject.transform.localScale = new Vector3(this.maxRadius * this.curDerivVal, this.maxRadius * this.curDerivVal, this.maxRadius * this.curDerivVal);
            }
            else
            {
                this.gameObject.transform.localScale = Vector3.zero;
            }

            // shift position based on acceleration vector
            this.gameObject.transform.position -= _oldAcceleration;
            Vector3 curAcceleration_ = touchSurface.acceleration;
            this.gameObject.transform.position += curAcceleration_;
            _oldAcceleration = curAcceleration_;
        }

        public void PusNewRawVal(float rawVal_)
        {
            // Add a new raw data value in the list
            if (rawVal_ > 0)
            {
                _rawVals.Add(rawVal_); // add new raw value
            }
            else
            {
                _rawVals.Add(0);       // value can not be negative, so default = 0
            }

            while (_rawVals.Count > this.N)
            {
                _rawVals.RemoveAt(0);       // remove older data from the list
            }

            if (_rawVals.Count > 0)
            {
                this.UpdateDataVals();        // update all data vals based on new incoming raw data 
            }
        }

        //----------------------------------------------------------------------------

        private void UpdateDataVals()
        {
            this.SetSmoothVal();          // call function to smooth raw data
            this.SetSmoothRelativeVal();  // call function to get the smooth relative data
            this.SetDerivativeVal();           // call function to get derivative of current data
        }

        private void SetSmoothVal()
        {
            // Compute mean of last N incoming data
            int meanVal_ = 0;
            foreach (int i in _rawVals)
            {
                meanVal_ += i;
            }
            _curSmoothVal = meanVal_ / ((float)_rawVals.Count);
        }

        private void SetSmoothRelativeVal()
        {
            this.curSRelativeVal = _curSmoothVal - _smoothValOffset; // offset data value
        }

        private void SetDerivativeVal()
        {
            _curDerivVal = _curSmoothVal - _oldSmoothVal;
            _oldSmoothVal = _curSmoothVal;
        }

        //----------------------------------------------------------------------------

        internal void SetOffsetValue()
        {
            _smoothValOffset = _curSmoothVal; // set current value as reference value
        }

        //----------------------------------------------------------------------------

        internal void ShiftRawVal()
        {
            // Update data list to keep a stable data flow
            if (_rawVals.Count >= N)
            {
                _rawVals.Add(_rawVals[_rawVals.Count - 1]); // duplicate last value
                _rawVals.RemoveAt(0); // remove first value
                                      // Call functions to update values
                this.UpdateDataVals();        // update all data vals based on new incoming raw data
            }
        }

        //----------------------------------------------------------------------------

        private Color GetLerpColor(float amt_)
        {
            // Set the color of the data point based on its value
            Color newColor_ = _purple;
            if (amt_ > 0.5)
            {
                // shade from yellow to red if value between .5 and 1.0
                newColor_ = Color.Lerp(_yellow, _red, 2 * (amt_ - 0.5f));
            }
            else
            {
                // shade from blue to yellow if value between .0 and .5
                newColor_ = Color.Lerp(_blue, _yellow, 2 * amt_);
            }
            return newColor_;
        }
    }
}