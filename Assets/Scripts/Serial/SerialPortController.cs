// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace CRI.HitBoxTemplate.Serial
{
    /// <summary>
    /// This class represent a connection with Arduino through serial.
    /// </summary>
    public abstract class SerialPortController : MonoBehaviour
    {
        /// <summary>
        /// The serial port of the arduino connection.
        /// </summary>
        protected SerialPort _serialPort;
        /// <summary>
        /// A locker for the serial port lockers. Prevent the read and write operations to be done at the same time.
        /// </summary>
        private static readonly object _serialPortLocker = new object();
        /// <summary>
        /// The serial thread that will run independently.
        /// </summary>
        protected Thread _serialThread;
        /// <summary>
        /// If true, the game is running. If false, the thread stops.
        /// </summary>
        protected volatile bool _gameRunning = true;
        /// <summary>
        /// If true, this instance will automatically send messages at the start and the end of the connection.
        /// </summary>
        protected bool _sendMessages = true;

        protected SerialPort OpenSerialPort(
            string name,
            int baudRate,
            int readTimeout,
            Handshake handshake
            )
        {
            _serialPort = new SerialPort(name, baudRate);
            Debug.Log("Connection started");
            try
            {
                _serialPort.Open();
                _serialPort.ReadTimeout = readTimeout;
                _serialPort.Handshake = handshake;

                Debug.Log("Serial Port " + _serialPort.PortName);

                _serialThread = new Thread(() => ThreadUpdate());
                _serialThread.Start();

                if (_sendMessages)
                    SendSerialMessage("connect");
                Debug.Log("Port Opened!");
            }
            catch (System.Exception e)
            {
                throw e;
            }
            return _serialPort;
        }

        /// <summary>
        /// The thread method called after the serial port is opened.
        /// </summary>
        protected abstract void ThreadUpdate();

        /// <summary>
        /// Sends a serial message. Thread-safe.
        /// </summary>
        /// <param name="mess_">The message that will be sent.</param>
        public void SendSerialMessage(string mess_)
        {
            lock (_serialPortLocker)
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Write(mess_ + '_');
                }
            }
        }

        /// <summary>
        /// Read a byte from the serial port. Thread-safe.
        /// </summary>
        /// <returns>The byte read.</returns>
        public byte ReadSerialByte()
        {
            lock (_serialPortLocker)
            {
                return (byte)_serialPort.ReadByte();
            }
        }

        /// <summary>
        /// Checks if the serial port is open. Thread-safe.
        /// </summary>
        /// <returns>True if the serial port is open.</returns>
        public bool IsSerialOpen()
        {
            lock (_serialPortLocker)
            {
                return _serialPort != null && _serialPort.IsOpen;
            }
        }

        /// <summary>
        /// Closes the serial port. Thread-safe.
        /// </summary>
        public void CloseSerialPort()
        {
            lock (_serialPortLocker)
            {
                _serialPort.Close();
            }
        }

        /// <summary>
        /// Called when the application quits.
        /// </summary>
        private void OnApplicationQuit()
        {
            _gameRunning = false;

            if (_serialPort != null && _serialPort.IsOpen)
            {
                if (_sendMessages)
                    SendSerialMessage("disconnect");
                print("closing serial port");
                CloseSerialPort();
                print("serial port closed");
            }
        }
    }
}
