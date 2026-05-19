using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "ThiefSim/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int    value;
    public int    weight;
    public Sprite icon;
}
