using UnityEngine;
using TMPro;

public class ItemLabel : MonoBehaviour
{
    private TextMeshPro _text;
    private ItemPickup _item;

    private void Start()
    {
        _item = GetComponentInParent<ItemPickup>();
        if (_item == null || _item.itemData == null) return;

        var go = new GameObject("LabelText");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0, 0.7f, 0);
        go.transform.localRotation = Quaternion.identity;

        _text = go.AddComponent<TextMeshPro>();
        _text.alignment = TextAlignmentOptions.Center;
        _text.fontSize = 3f;
        _text.color = Color.white;
        _text.outlineWidth = 0.2f;
        _text.outlineColor = new Color32(0, 0, 0, 255);
        
        var mr = _text.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerName = "Default";
            mr.sortingOrder = 15;
        }

        // Generate text
        string emoji = GetEmoji(_item.itemData.itemName);
        int val = _item.itemData.value;
        int weight = _item.itemData.weight;
        
        string dots = "";
        for (int i = 0; i < weight; i++) dots += "•";
        
        // Add spaces between dots for readability
        dots = string.Join(" ", dots.ToCharArray());

        _text.text = $"{emoji} {val}\n<color=#ff3333><size=2>{dots}</size></color>";

        // Pulsing scale
        StartCoroutine(PulseRoutine(go.transform));
    }

    private string GetEmoji(string itemName)
    {
        switch (itemName)
        {
            case "Coin": return "🪙";
            case "Ring": return "💍";
            case "Vase": return "🏺";
            case "Painting": return "🖼️";
            case "Crown": return "👑";
            case "Necklace": return "📿";
            case "Statue": return "🗿";
            default: return "[?]";
        }
    }

    private System.Collections.IEnumerator PulseRoutine(Transform t)
    {
        Vector3 orig = Vector3.one;
        float time = 0;
        while (true)
        {
            time += Time.deltaTime;
            float scale = 1.0f + 0.05f * Mathf.Sin(time * Mathf.PI * 2f / 1.5f);
            t.localScale = orig * scale;
            yield return null;
        }
    }
}
