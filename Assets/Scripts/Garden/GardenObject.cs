using UnityEngine;

/// <summary>
/// A single placed object in the freeform garden ecosystem.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class GardenObject : MonoBehaviour
{
    public enum ObjectType
    {
        Paved,
        Flower,
        Bush,
        Tree,
        Pond,
        LeafPile,
        HedgehogHouse,
        Trash
    }

    [SerializeField] private ObjectType _type;
    public ObjectType Type
    {
        get => _type;
        private set
        {
            _type = value;
            ApplyVisuals();
        }
    }

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = false; // Player should collide with trees/houses, but maybe not flowers.
    }

    public void Initialize(ObjectType newType)
    {
        Type = newType;
        
        var col = GetComponent<BoxCollider2D>();
        // Walkable items
        if (Type == ObjectType.Flower || Type == ObjectType.LeafPile || Type == ObjectType.Paved)
        {
            col.isTrigger = true;
        }
        else
        {
            col.isTrigger = false;
        }
        
        // Adjust collider size based on type
        if (Type == ObjectType.Tree) col.size = new Vector2(0.5f, 0.5f);
        else col.size = new Vector2(0.8f, 0.8f);
    }

    private void ApplyVisuals()
    {
        if (_sr == null) return;
        _sr.sprite = GetSpriteForType(_type);
        
        if (_sr.sprite == null && _type == ObjectType.Trash)
        {
            // Fallback for Trash: crumpled grey bag shape
            var tex = new Texture2D(16, 16);
            tex.filterMode = FilterMode.Point;
            Color clear = Color.clear;
            Color bag = new Color(0.35f, 0.35f, 0.38f);
            Color bagDark = new Color(0.22f, 0.22f, 0.25f);
            Color bagLight = new Color(0.5f, 0.48f, 0.45f);
            Color tie = new Color(0.6f, 0.15f, 0.1f);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    tex.SetPixel(x, y, clear);

            // Bag body (rounded shape)
            for (int y = 1; y <= 11; y++)
            {
                for (int x = 3; x <= 12; x++)
                {
                    float dx = (x - 7.5f) / 5f;
                    float dy = (y - 6f) / 6f;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        // wrinkle pattern
                        if ((x + y) % 4 == 0) tex.SetPixel(x, y, bagDark);
                        else if ((x + y) % 3 == 0) tex.SetPixel(x, y, bagLight);
                        else tex.SetPixel(x, y, bag);
                    }
                }
            }

            // Tied top
            tex.SetPixel(7, 12, tie);
            tex.SetPixel(8, 12, tie);
            tex.SetPixel(7, 13, tie);
            tex.SetPixel(8, 13, tie);
            tex.SetPixel(6, 13, bag);
            tex.SetPixel(9, 13, bag);

            tex.Apply();
            _sr.sprite = Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(0.5f,0.5f), 16f);
        }

        _sr.color = Color.white;
        
        // Sorting order based on Y position for fake depth
        _sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 10f);
    }

    public static Sprite GetSpriteForType(ObjectType type)
    {
        string spriteName = "";
        switch (type)
        {
            case ObjectType.Paved:         spriteName = "tile_paved"; break;
            case ObjectType.Flower:        spriteName = "tile_flower"; break;
            case ObjectType.Bush:          spriteName = "tile_bush"; break;
            case ObjectType.Tree:          spriteName = "tile_tree"; break;
            case ObjectType.Pond:          spriteName = "tile_water"; break;
            case ObjectType.LeafPile:      spriteName = "tile_leafpile"; break;
            case ObjectType.HedgehogHouse: spriteName = "tile_hedgehoghouse"; break;
            case ObjectType.Trash:         spriteName = "tile_trash"; break;
        }
        return Resources.Load<Sprite>($"EgelGame/{spriteName}");
    }
}
