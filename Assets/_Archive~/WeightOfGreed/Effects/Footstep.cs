using UnityEngine;

public class Footstep : MonoBehaviour
{
    public float lifetime = 2f;
    private SpriteRenderer _sr;
    private Color _startColor;
    private float _timer;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _startColor = _sr.color;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        float t = _timer / lifetime;
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
        else
        {
            Color c = _startColor;
            c.a = Mathf.Lerp(_startColor.a, 0f, t);
            _sr.color = c;
        }
    }
}
