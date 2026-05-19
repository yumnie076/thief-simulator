using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerFlashlight : MonoBehaviour
{
    private Light2D _light;
    private Vector2 _lastDir = Vector2.down;

    private void Awake()
    {
        _light = GetComponent<Light2D>();
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(h, v);

        if (dir.sqrMagnitude > 0.01f)
        {
            _lastDir = dir.normalized;
        }

        // Rotate the flashlight transform to face the direction
        float angle = Mathf.Atan2(_lastDir.y, _lastDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
