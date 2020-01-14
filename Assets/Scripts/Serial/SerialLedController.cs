// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using UnityEditor;

namespace CRI.HitBoxTemplate.Serial
{
    /// <summary>
    /// This class represents the arduino connection through serial to manage the leds.
    /// </summary>
    public class SerialLedController : SerialPortController
    {
        /// <summary>
        /// The number of rows for the led control. The data will be automatically read from the Game Settings.
        /// </summary>
        private int _rows = 30;
        /// <summary>
        /// The number of columns for the led control. The data will be automatically read from the Game Settings.
        /// </summary>
        private int _cols = 60;
        /// <summary>
        /// Stores the led data (in RGB).
        /// </summary>
        private Color[] _leds;
        /// <summary>
        /// Stores the led data (in RGB).
        /// </summary>
        private Color[] _newLedColor;
        /// <summary>
        /// Locker for the leds for thread-safe operations.
        /// </summary>
        private static readonly object _ledsLocker = new object();

        private Texture2D _cameraTexture;

        /// <summary>
        /// Current texture generated from the camera.
        /// </summary>
        public Texture2D cameraTexture
        {
            get
            {
                lock (_ledsLocker)
                {
                    return _cameraTexture;
                }
            }
        }

        /// <summary>
        /// Index of the player. The led display will depend on the screen of that player.
        /// </summary>
        public int playerIndex
        {
            get;
            private set;
        }
        /// <summary>
        /// The camera of the player.
        /// </summary>
        public Camera playerCamera
        {
            get;
            private set;
        }

        /// <summary>
        /// Initialize the led control.
        /// </summary>
        /// <param name="playerIndex">The index of the player corresponding to this serial port connection.</param>
        /// <param name="ledControlGridRows">The number of rows of leds.</param>
        /// <param name="ledControlGridCols">The number of columns of leds.</param>
        /// <param name="serialPortName">Name of the serial port.</param>
        /// <param name="baudRate">Baud rate of the serial port.</param>
        /// <param name="readTimeout">Read timeout of the serial port.</param>
        /// <param name="handshake">Handshake of the serial port.</param>
        /// <param name="playerCamera">The camera corresponding to the player.
        /// This camera needs to have a render texture to correctly display what it sees to the leds.</param>
        public void Init(int playerIndex,
            int ledControlGridRows,
            int ledControlGridCols,
            string serialPortName,
            int baudRate,
            int readTimeout,
            Handshake handshake,
            Camera playerCamera)
        {
            // The led controller needs to send the "connect" and "disconnect" messages. So we set the send message field to true.
            _sendMessages = true;

            // Initialize leds array to store color values
            this.playerIndex = playerIndex;
            this.playerCamera = playerCamera;
            _rows = ledControlGridRows;
            _cols = ledControlGridCols;
            _leds = new Color[_rows * _cols];
            _newLedColor = new Color[_rows * _cols];
            if (playerCamera.targetTexture != null)
                _cameraTexture = new Texture2D(playerCamera.targetTexture.width,
                    playerCamera.targetTexture.height,
                    TextureFormat.ARGB32,
                    false);
            for (int i = 0; i < _cols; i++)
            {
                for (int j = 0; j < _rows; j++)
                {
                    _leds[GetLedIndex(i, j)] = Color.red;
                    _newLedColor[GetLedIndex(i, j)] = Color.black;
                }
            }
            // Serial port initialization
            OpenSerialPort(serialPortName, baudRate, readTimeout, handshake);
        }

        private void Update()
        {
            if (playerCamera.targetTexture != null && IsSerialOpen())
            {
                playerCamera.targetTexture.GetRTPixels(ref _cameraTexture);
                SetPixelColor(_cameraTexture);
            }
        }

        /// <summary>
        /// Gets the color of a led at a specific index. Thread-safe.
        /// </summary>
        /// <param name="index">The index of the led.</param>
        /// <returns>The color of the led.</returns>
        private Color GetLedColor(int index)
        {
            lock (_ledsLocker)
            {
                return _newLedColor[index];
            }
        }

        /// <summary>
        /// Sets the color of a led at a specific index. Thread-safe.
        /// </summary>
        /// <param name="index">The index of the led.</param>
        /// <param name="value">The new color of the led.</param>
        private void SetLedColor(int index, Color value)
        {
            lock (_ledsLocker)
            {
                _newLedColor[index] = value;
            }
        }

        /// <summary>
        /// For each pixel in a given texture, call the SetTheLedColor function to set the color of each led.
        /// </summary>
        private void SetPixelColor(Texture2D cameraTexture)
        {
            // get pixel color to drive leds pannel
            int offsetX = (int)(cameraTexture.width / 2f - cameraTexture.height / 2f);
            for (int i = 0; i < _cols; i++)
            {
                for (int j = 0; j < _rows; j++)
                {
                    int gx = (int)(cameraTexture.height * (i / ((float)_cols))) + offsetX;
                    int gy = (int)(cameraTexture.height * ((j / ((float)_rows)) * 0.95f + 0.025f));
                    SetLedColor(GetLedIndex(i, j), cameraTexture.GetPixel(gx, gy));
                }
            }
        }

        protected override void ThreadUpdate()
        {
            int length = _newLedColor.Length;
            while (_gameRunning)
            {
                for (int i = 0; i < length; i++)
                {
                    ThreadWriteLedColor(i, GetLedColor(i));
                }
            }
        }

        /// <summary>
        /// Sends to the leds a message to display the screensaver.
        /// </summary>
        public void ScreenSaver()
        {
            try
            {
                SendSerialMessage("savemode");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// Sends to the leds a message to turn off.
        /// </summary>
        public void ShutDown()
        {
            try
            {
                SendSerialMessage("turnoff");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// Sends to the leds a message to display the grid.
        /// </summary>
        public void DisplayGrid()
        {
            try
            {
                SendSerialMessage("dispgrid");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// Sends to the leds a message to start the hit animation.
        /// </summary>
        public void Hit()
        {
            try
            {
                SendSerialMessage("hit");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        
        /// <summary>
        /// Sends to the leds a message to start the "end game" animation.
        /// </summary>
        public void EndGame()
        {
            try
            {
                SendSerialMessage("endgame");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// Gets the index of the led with a given x and y.
        /// </summary>
        /// <param name="x">The x coordinate of the led.</param>
        /// <param name="y">The y coordinate of the led.</param>
        /// <returns></returns>
        private int GetLedIndex(int x, int y)
        {
            // Convert X and Y coordinate into the led index
            x = Mathf.Clamp(x, 0, _cols - 1);
            y = Mathf.Clamp(y, 0, _rows - 1);
            if (y % 2 == 0)
            {
                x = _cols - 1 - x;
            }
            y = _rows - 1 - y;
            int ipix = y * _cols + x;
            return ipix;
        }

        /// <summary>
        /// Function called by the thread. Call the write function to send the color of the led at a given index.
        /// </summary>
        /// <param name="ipix">Index of the led.</param>
        /// <param name="col">Color that will be written.</param>
        private void ThreadWriteLedColor(int ipix, Color col)
        {
            string ledSerialData = "";
            // Do not send color value to the led if it_s already the same
            if (_leds[ipix] != col)
            {
                int r = (int)(255 * col.r);
                int g = (int)(255 * col.g);
                int b = (int)(255 * col.b);

                // Create data string to send and send it to serial port
                ledSerialData += ipix.ToString("X3") + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + "_";   // decimal format

                // Send new color to leds pannel
                if (IsSerialOpen())
                {
                    SendSerialMessage(ledSerialData);
                    _leds[ipix] = col; // update led color values
                }
            }
        }
    }
}