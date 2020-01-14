using CRI.HitBoxTemplate.Serial;
using UnityEngine;

namespace CRI.HitBoxTemplate.Example
{
    public class ExampleDebugTextController : MonoBehaviour
    {
        public ExampleSerialController serialController;

        private void OnEnable()
        {
            ImpactPointControl.onImpact += OnImpact;
        }

        private void OnDisable()
        {
            ImpactPointControl.onImpact -= OnImpact;
        }

        private void OnImpact(object sender, ImpactPointControlEventArgs e)
        {
            Debug.Log(string.Format("Impact: Player [{0}], Position [{1}], Accelerometer [{2}]",
                e.playerIndex,
                e.impactPosition,
                e.accelerometer));
        }

        private void Update()
        {
            Vector3[] accelerations = serialController.accelerations;
            for (int i = 0; i < accelerations.Length; i++)
            {
                Debug.Log(string.Format("Acceleration Player {0}", accelerations[i]));
            }
        }
    }
}
