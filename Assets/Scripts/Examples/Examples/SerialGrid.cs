using System;
using UnityEngine;

namespace CRI.HitBoxTemplate.Example
{
    /// <summary>
    /// Settings for a grid of leds or a grid of sensors.
    /// </summary>
    [Serializable]
    public struct SerialGrid
    {
        /// <summary>
        /// The number of rows.
        /// </summary>
        [Tooltip("The number of rows")]
        public int rows;
        /// <summary>
        /// The number of columns.
        /// </summary>
        [Tooltip("The number of columns.")]
        public int cols;

        public SerialGrid(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
        }
    }
}