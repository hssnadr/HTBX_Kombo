using UnityEngine;

namespace INSEP
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(MeshRenderer))]
    public class HitFeedbackAnimator : MonoBehaviour
    {
        /// <summary>
        /// Color of the material
        /// </summary>
        [SerializeField]
        [Tooltip("Color of the material.")]
        private Color _color = Color.white;

        private MeshRenderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            _renderer.material.SetColor("_Color", _color);
        }

        public void OnAnimationEvent()
        {
            Destroy(this);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Destroy(_renderer.material);
        }
    }
}