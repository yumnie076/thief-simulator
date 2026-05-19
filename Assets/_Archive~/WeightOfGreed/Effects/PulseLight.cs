using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PulseLight : MonoBehaviour
{
    private Light2D _light;
    public float minIntensity = 0.2f;
    public float maxIntensity = 0.5f;
    public float speed = 2f;

    private void Awake()
    {
        _light = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (_light != null)
        {
            float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
            _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        }
    }
}
