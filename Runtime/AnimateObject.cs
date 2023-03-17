using UnityEngine;

namespace Bcom.SharedPlayground
{
    public class AnimateObject : MonoBehaviour
    {
        public Vector3 translationAxis = Vector3.up;
        public Vector3 rotationAxis = Vector3.up;

        [Range(0f, 10f)]
        public float animationDuration = 5f;
        [Range(0f, 1f)]
        public float animationAmplitude = .1f;

        public AnimationCurve animationCurve;

        private Quaternion initialRotation;
        private Vector3 initialPosition;

        private float time;

        private void OnEnable()
        {
            time = 0f;
            initialRotation = transform.rotation;
            initialPosition = transform.position;
        }

        private void OnDisable()
        {
            transform.rotation = initialRotation;
            transform.position = initialPosition;
        }

        void FixedUpdate()
        {
            time += Time.fixedDeltaTime;
            float value = animationCurve.Evaluate(time / animationDuration);
            transform.position = initialPosition + animationAmplitude * value * translationAxis;

            transform.Rotate(rotationAxis, (.5f * animationDuration / 360f) / Time.fixedDeltaTime);
        }
    }
}
