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
        HedgehogHouse
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
        }
        return Resources.Load<Sprite>($"EgelGame/{spriteName}");
    }
}
