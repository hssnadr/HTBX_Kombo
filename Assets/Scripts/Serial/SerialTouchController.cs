// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Linq;
using System;

namespace CRI.HitBoxTemplate.Serial
{
    public class SerialTouchController : SerialPortController
    {
        public bool printSerialDataRate = false;
        // Thread variables
        /// <summary>
        /// Queue of temporarily stored data. Is used by the thread.
        /// </summary>
        private Queue<string> _dataQueue = new Queue<string>();
        /// <summary>
        /// Locker for all the data queue operations.
        /// </summary>
        private static readonly object _dataQueueLocker = new object();
        /// <summary>
        /// Counts the number of data read each second.
        /// </summary>
        private int _dataCounter = 0;

        // Sensor grid variables
        /// <summary>
        /// The prefab of the datapoint control. The datapoint will be cloned for each sensor in the grid.
        /// </summary>
        [SerializeField]
        [Tooltip("The prefab of the datapoint control. The datapoint will be cloned for each sensor in the grid.")]
        protected DatapointControl _datapointPrefab;
        /// <summary>
        /// The prefab of the impact point. The impact point manages all the datapoints.
        /// </summary>
        [SerializeField]
        [Tooltip("The prefab of the impact point. The impact point manages all the datapoints.")]
        protected ImpactPointControl _impactPointControlPrefab;
        /// <summary>
        /// A grid of datapoints.
        /// </summary>
        private DatapointControl[,] _pointGrid;
 
        // Sensor grid setup.
        /// <summary>
        /// The number of rows of the sensor grid. Will be automatically replaced by the Game Settings.
        /// </summary>
        private int _rows = 24;
        /// <summary>
        /// The number of columns of the sensor grid. Will be automatically replaced by the Game Settings.
        /// </summary>
        private int _cols = 24;

        // Physics values.
        private Vector3 _acceleration;

        private static readonly object _accelerationLocker = new object();
        /// <summary>
        /// Acceleration values.
        /// </summary>
        internal Vector3 acceleration
        {
            get
            {
                lock(_accelerationLocker)
                {
                    return _acceleration;
                }
            }
        }
        /// <summary>
        /// Stores acceleration data to compe moving mean.
        /// </summary>
        private List<Vector3> _accCollection = new List<Vector3>();
        /// <summary>
        /// Max sac of accCollection.
        /// </summary>
        private const int nAcc = 5; // max size of accCollection (size of filter)

        // Game values.
        /// <summary>
        /// The player index of the touch surface. All the data read will affect that player's gameplay.
        /// </summary>
        public int playerIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Initialize the touch surface.
        /// </summary>
        /// <param name="playerIndex">The index of the player corresponding to this serial port connection.</param>
        /// <param name="touchSurfaceGridRows">The number of rows of datapoints.</param>
        /// <param name="touchSurfaceGridCols">The number of columns of datapoints.</param>
        /// <param name="serialPortName">Name of the serial port.</param>
        /// <param name="baudRate">Baud rate of the serial port.</param>
        /// <param name="readTimeout">Read timeout of the serial port.</param>
        /// <param name="handshake">Handshake of the serial port.</param>
        /// <param name="playerCamera">The camera corresponding to the player.</param>
        /// <param name="impactThreshold">Threshold of impact</param>
        /// <param name="displayDataPoints">Display the data points.</param>
        /// <param name="ignoreBlackBackground">Whether or not the impact on black background should be ignored.</param>
        public void Init(int playerIndex,
            int touchSurfaceGridRows,
            int touchSurfaceGridCols,
            string name,
            int baudRate,
            int readTimeout,
            Handshake handshake,
            Camera playerCamera,
            float impactThreshold,
            int delayOffHit,
            bool displayDataPoints,
            bool ignoreBlackBackground = false)
        {
            // Prevents the touch surface to send messages.
            _sendMessages = false;

            // Initialize point grid as gameobjects
            _rows = touchSurfaceGridRows;
            _cols = touchSurfaceGridCols;
            _pointGrid = new DatapointControl[_rows, _cols];

            int count = 0;

            // The grid is positionned as a child of a camera.
            // We need the grid to move whenever the camera moves to keep an accurate representation of the touch data.
            var grid = new GameObject("Player" + (playerIndex + 1) + " Grid");
            grid.transform.parent = playerCamera.transform;
            Bounds bounds = playerCamera.GetBounds();
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    DatapointControl dpc = null;
                    // The positioning of the datapoints is different whether the camera is orthographic or not.
                    if (playerCamera.orthographic)
                    {
                        float x = i * ((float)1.0f / _cols);
                        float y = j * ((float)1.0f / _rows);
                        dpc = GameObject.Instantiate(_datapointPrefab, playerCamera.ViewportToWorldPoint(new Vector3(x, y, playerCamera.nearClipPlane + 100.0f)), Quaternion.identity, grid.transform);
                        dpc.name = "Datapoint " + count + " " + playerIndex + " (" + x + ";" + y + ")";
                    }
                    else
                    {
                        float x = bounds.min.x + i * ((bounds.extents.x * 2.0f) / _cols);
                        float y = bounds.min.y + j * ((bounds.extents.y * 2.0f) / _rows);
                        dpc = GameObject.Instantiate(_datapointPrefab, new Vector3(x, y, 100.0f), Quaternion.identity, grid.transform);
                        dpc.name = "Datapoint " + count + " " + playerIndex + " (" + x + ";" + y + ")";
                    }
                    dpc.gameObject.layer = 8 + playerIndex;
                    count++;
                    dpc.playerIndex = playerIndex;
                    dpc.GetComponentInChildren<MeshRenderer>().enabled = displayDataPoints;
                    dpc.touchSurface = this;
                    _pointGrid[i, _cols - j - 1] = dpc;
                }
            }
            // Imapact point initialization
            var ipc = GameObject.Instantiate(_impactPointControlPrefab, this.transform);
            ipc.threshImpact = impactThreshold;
            ipc.playerIndex = playerIndex;
            ipc.ignoreBlackBackground = ignoreBlackBackground;
            ipc.serialTouchController = this;
            // Serial port initialization
            try
            {
                OpenSerialPort(name, baudRate, readTimeout, handshake);
                if (printSerialDataRate)
                    StartCoroutine(PrintSerialDataRate(1f));
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        protected override void ThreadUpdate()
        {
            string data = "";
            byte tmp = 0;
            tmp = ReadSerialByte();
            while (tmp != 255 && _gameRunning)
            {
                tmp = ReadSerialByte();
                // We read characters one by one until we find a "q" character.
                // If we do, we enqueue the data and initialize the next data string.
                if (tmp != 'q')
                {
                    data += ((char)tmp);
                }
                else
                {
                    Enqueue(data);
                    data = "";
                }
            }
        }

        /// <summary>
        /// Enqueue a data in the data queue. Thread-safe.
        /// </summary>
        /// <param name="data">The data that will be enqueued.</param>
        private void Enqueue(string data)
        {
            lock (_dataQueueLocker)
            {
                _dataQueue.Enqueue(data);
            }
        }

        /// <summary>
        /// Dequeue a data at the top of the data queue. Thread-safe.
        /// </summary>
        /// <returns>The data dequeued.</returns>
        private string Dequeue()
        {
            lock (_dataQueueLocker)
            {
                return _dataQueue.Dequeue();
            }
        }

        /// <summary>
        /// The length of the data queue.
        /// </summary>
        /// <returns>The length of the data queue.</returns>
        private int QueueLength()
        {
            lock (_dataQueueLocker)
            {
                return _dataQueue.Count;
            }
        }

        private void Update()
        {
            if (_serialPort.IsOpen)
            {
                // Get serial data from second thread
                int count = QueueLength();
                for (int i = 0; i < count; i++)
                {
                    string rawDataStr = Dequeue();
                    if (rawDataStr != null && rawDataStr.Length > 1)
                    {
                        ParseSerialData(rawDataStr);
                    }
                }


                // Remap and display data points
                for (int i = 0; i < _rows; i++)
                {
                    // Get row data range
                    float minRow = 1000.0f;
                    float maxRow = -1000.0f;
                    float sumRow = 0.0f;
                    for (int j = 0; j < _cols; j++)
                    {
                        float curSRelativeVal = _pointGrid[i, j].curSRelativeVal;
                        sumRow += curSRelativeVal;

                        if (minRow > curSRelativeVal)
                            minRow = curSRelativeVal;
                        if (maxRow < curSRelativeVal)
                            maxRow = curSRelativeVal;
                    }

                    // Get remap values for the current row and display data point
                    for (int j = 0; j < _cols; j++)
                    {
                        if (maxRow - minRow != 0)
                        {
                            float curRemapVal = _pointGrid[i, j].curRemapVal;
                            curRemapVal = (_pointGrid[i, j].curSRelativeVal - minRow) / (maxRow - minRow);
                            curRemapVal *= sumRow;
                            curRemapVal /= 1024.0f; // 1024 = max analog range
                            _pointGrid[i, j].curRemapVal = Mathf.Clamp(curRemapVal, 0.0f, 1.0f);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parse the serial data.
        /// </summary>
        /// <param name="serialData">The serial data that will be parsed.</param>
        private void ParseSerialData(string serialData)
        {
            serialData = serialData.Trim();

            // First character of the string is an adress
            char adr_ = serialData.ToCharArray()[0]; // get address character
            serialData = serialData.Split(adr_)[1]; // remove adress from the string to get the message content

            switch (adr_)
            {
                case 'z':
                    // GET COORDINATES
                    if (serialData != null)
                    {
                        if (serialData.Length == 2 + 4 * _cols)
                        {
                            //int[] rawdat_ = serialdata_.Split ('x').Select (str => int.Parse (str)).ToArray (); // get 
                            int[] rawdat_ = serialData.Split('x').Select(str => int.Parse(str, System.Globalization.NumberStyles.HexNumber)).ToArray();
                            //print (rawdat_.Length);
                            //print (COLS+1);
                            if (rawdat_.Length == _cols + 1)
                            { // COLS + 1 ROW
                                int j = rawdat_[0];
                                for (int k = 1; k < rawdat_.Length; k++)
                                {
                                    _pointGrid[j, k - 1].GetComponent<DatapointControl>().PusNewRawVal(rawdat_[k]);
                                }
                            }
                            _dataCounter++;
                        }
                    }
                    break;

                case 'a':
                    // GET ACCELERATION
                    if (serialData != null)
                    {
                        if (serialData.Length == 3 * 3 + 2)
                        {
                            int[] acc_ = serialData.Split('c').Select(str => int.Parse(str, System.Globalization.NumberStyles.HexNumber)).ToArray();
                            if (acc_.Length == 3)
                            {
                                _accCollection.Add(new Vector3(acc_[0], acc_[1], acc_[2]));
                                while (_accCollection.Count > nAcc)
                                {
                                    _accCollection.RemoveAt(0);
                                }

                                // Compute moving mean filter
                                Vector3 smoothAcc_ = Vector3.zero;
                                foreach (Vector3 curAcc_ in _accCollection)
                                {
                                    smoothAcc_ += curAcc_;
                                }
                                smoothAcc_ /= (float)_accCollection.Count;

                                _acceleration = smoothAcc_;
                                _acceleration /= 10000f; // map acceleration TO CHANGE
                                _dataCounter++;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                    if (_pointGrid != null && _pointGrid[i, j])
                    {
                        Destroy(_pointGrid[i, j].gameObject);
                        _pointGrid[i, j] = null;
                    }
            }
        }

        /// <summary>
        /// Coroutine called each <paramref name="waitTime"/> to print the serial data rate.
        /// </summary>
        /// <param name="waitTime">The wait time between each print.</param>
        /// <returns></returns>
        private IEnumerator PrintSerialDataRate(float waitTime)
        {
            while (true)
            {
                yield return new WaitForSeconds(waitTime);
#if UNITY_EDITOR
                print("Serial data rate = " + _dataCounter / waitTime + " data/s");
#endif
                _dataCounter = 0;
            }
        }
    }
}