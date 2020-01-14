using CRI.HitBoxTemplate.Serial;
using System;
using System.IO.Ports;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CRI.HitBoxTemplate.Example
{
    public class ExampleSerialController : MonoBehaviour
    {
        [SerializeField]
        private SerialSettings[] _serialSettings = null;

        [SerializeField]
        private GameObject _serialControllerPrefab = null;

        private GameObject[] _serialControllers;

        public Vector3[] accelerations
        {
            get
            {
                return _serialControllers.Select(x => x.GetComponent<SerialTouchController>().acceleration).ToArray();
            }
        }

        public const int touchControlGridRows = 24;
        public const int touchControlGridCols = 24;
        public const int ledControlGridRows = 30;
        public const int ledControlGridCols = 60;
        public const int baudRate = 38400;
        public const int readTimeout = 400;
        public const Handshake handshake = Handshake.None;

        private void Start()
        {
            int lenght = _serialSettings.Length;
            _serialControllers = new GameObject[lenght];
            for (int i = 0; i < lenght; i++)
            {
                _serialControllers[i] = InitSerialController(_serialSettings[i], i);
            }
        }

        private GameObject InitSerialController(SerialSettings serialSettings, int p)
        {
            var go = GameObject.Instantiate(_serialControllerPrefab, this.transform);
            try
            {
                go.name = "Serial Controller" + p;
                go.GetComponent<SerialLedController>().Init(p,
                    ledControlGridRows,
                    ledControlGridCols,
                    serialSettings.ledControlSerialPortName,
                    baudRate,
                    readTimeout,
                    handshake,
                    serialSettings.playerCamera
                    );
                go.GetComponent<SerialTouchController>().Init(p,
                    touchControlGridRows,
                    touchControlGridCols,
                    serialSettings.touchControlSerialPortName,
                    baudRate,
                    readTimeout,
                    handshake,
                    serialSettings.playerCamera,
                    serialSettings.impactThreshold,
                    serialSettings.delayOffHit,
                    serialSettings.displayDatapoints,
                    serialSettings.ignoreBlackBackground
                    );
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.Log("Connection aborted.");
                Destroy(go);
                go = null;
            }
            return go;
        }

        /// <summary>
        /// Plays the End Game animation.
        /// </summary>
        public void EndGame()
        {
            for (int i = 0; i < _serialControllers.Length; i++)
            {
                if(_serialControllers[i] != null) {
                    _serialControllers[i].GetComponent<SerialLedController>().EndGame();
                }
            }
        }

        /// <summary>
        /// Plays the Hit animation.
        /// </summary>
        public void Hit()
        {
            for (int i = 0; i < _serialControllers.Length; i++)
            {
                if (_serialControllers[i] != null)
                {
                    _serialControllers[i].GetComponent<SerialLedController>().Hit();
                }
            }
        }

        /// <summary>
        /// Plays the Display Grid animation.
        /// </summary>
        public void DisplayGrid()
        {
            for (int i = 0; i < _serialControllers.Length; i++)
            {
                if (_serialControllers[i] != null)
                {
                    _serialControllers[i].GetComponent<SerialLedController>().DisplayGrid();
                }
            }
        }

        /// <summary>
        /// Stop all animations and turn the leds off.
        /// </summary>
        public void ShutDown()
        {
            for (int i = 0; i < _serialControllers.Length; i++)
            {
                if (_serialControllers[i] != null)
                {
                    _serialControllers[i].GetComponent<SerialLedController>().ShutDown();
                }
            }
        }

        /// <summary>
        /// Plays the Screen Saver animation.
        /// </summary>
        public void ScreenSaver()
        {
            for (int i = 0; i < _serialControllers.Length; i++)
            {
                if (_serialControllers[i] != null)
                {
                    _serialControllers[i].GetComponent<SerialLedController>().ScreenSaver();
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ExampleSerialController))]
    public class ExampleSerialControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                var exampleController = target as ExampleSerialController;
                EditorGUILayout.LabelField("Send Messages", EditorStyles.centeredGreyMiniLabel);
                if (GUILayout.Button("End Game"))
                    exampleController.EndGame();
                if (GUILayout.Button("Hit"))
                    exampleController.Hit();
                if (GUILayout.Button("Display Grid"))
                    exampleController.DisplayGrid();
                if (GUILayout.Button("Shut Down"))
                    exampleController.ShutDown();
                if (GUILayout.Button("Screen Saver"))
                    exampleController.ScreenSaver();
            }
        }
    }
#endif
}
