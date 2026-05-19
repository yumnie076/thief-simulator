using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    private float _shakeDuration;
    private float _shakeMagnitude;

    public void Shake(float duration, float magnitude)
    {
        _shakeDuration = duration;
        _shakeMagnitude = magnitude;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;

        if (_shakeDuration > 0)
        {
            desiredPosition += (Vector3)Random.insideUnitCircle * _shakeMagnitude;
            _shakeDuration -= Time.unscaledDeltaTime;
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.unscaledDeltaTime);
    }
}
