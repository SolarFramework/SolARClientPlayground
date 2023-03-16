using UnityEngine;

public class AnimateObject : MonoBehaviour
{
    [Range(0f, 10f)]
    public float animationDuration = 5f;
    [Range(0f, 1f)]
    public float animationAmplitude = .1f;

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
        float angle = 2f * Mathf.PI * time / animationDuration;
        var delta = animationAmplitude * Mathf.Sin(angle) * Vector3.up;
        transform.position = initialPosition + delta;

        transform.Rotate(Vector3.up, animationDuration / 360f / Time.fixedDeltaTime);
    }
}
