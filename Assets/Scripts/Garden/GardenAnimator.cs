using UnityEngine;

/// <summary>
/// Adds idle animations to garden objects based on their type.
/// Attach to any GardenObject to bring it to life.
/// </summary>
public class GardenAnimator : MonoBehaviour
{
    public enum AnimType { None, Sway, Breathe, Wave, Wobble }

    private AnimType _animType = AnimType.None;
    private float _timeOffset;
    private Vector3 _baseScale;
    private Quaternion _baseRotation;
    private SpriteRenderer _sr;
    private Color _baseColor;

    // Animation parameters
    private float _swayAmount = 3f;      // degrees
    private float _swaySpeed = 1.5f;
    private float _breatheAmount = 0.03f;
    private float _breatheSpeed = 1.2f;
    private float _waveSpeed = 2f;

    public void Setup(AnimType type)
    {
        _animType = type;
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
        _baseScale = transform.localScale;
        _baseRotation = transform.rotation;
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _baseColor = _sr.color;

        // Customize per type
        switch (type)
        {
            case AnimType.Sway: // Flowers
                _swayAmount = Random.Range(3f, 6f);
                _swaySpeed = Random.Range(1.2f, 2f);
                break;
            case AnimType.Wobble: // Trees — slower, bigger sway
                _swayAmount = Random.Range(1.5f, 3f);
                _swaySpeed = Random.Range(0.6f, 1f);
                break;
            case AnimType.Breathe: // LeafPile
                _breatheAmount = 0.025f;
                _breatheSpeed = Random.Range(0.8f, 1.5f);
                break;
            case AnimType.Wave: // Water/Pond
                _waveSpeed = Random.Range(1.5f, 2.5f);
                break;
        }
    }

    private void Update()
    {
        float t = Time.time + _timeOffset;

        switch (_animType)
        {
            case AnimType.Sway:
                float angle = Mathf.Sin(t * _swaySpeed) * _swayAmount;
                transform.rotation = _baseRotation * Quaternion.Euler(0, 0, angle);
                break;

            case AnimType.Wobble:
                float treeAngle = Mathf.Sin(t * _swaySpeed) * _swayAmount;
                transform.rotation = _baseRotation * Quaternion.Euler(0, 0, treeAngle);
                break;

            case AnimType.Breathe:
                float scale = 1f + Mathf.Sin(t * _breatheSpeed) * _breatheAmount;
                transform.localScale = _baseScale * scale;
                break;

            case AnimType.Wave:
                if (_sr != null)
                {
                    // Subtle color pulse between light and dark blue
                    float pulse = Mathf.Sin(t * _waveSpeed) * 0.5f + 0.5f;
                    Color light = new Color(0.4f, 0.7f, 0.95f);
                    Color dark = new Color(0.25f, 0.5f, 0.8f);
                    _sr.color = Color.Lerp(dark, light, pulse);
                }
                // Gentle scale wave
                float wScale = 1f + Mathf.Sin(t * _waveSpeed * 1.3f) * 0.015f;
                transform.localScale = _baseScale * wScale;
                break;
        }
    }
}
