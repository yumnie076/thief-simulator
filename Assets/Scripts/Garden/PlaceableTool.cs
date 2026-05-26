using UnityEngine;

/// <summary>
/// Static data class for garden tools.
/// Defines tool types and costs for the freeform ecosystem.
/// </summary>
public static class PlaceableTool
{
    public enum ToolType
    {
        RemoveTile,
        Flower,
        Bush,
        Tree,
        Pond,
        LeafPile,
        HedgehogHouse,
        Sunflower
    }

    /// <summary>Returns the action-point cost for a given tool.</summary>
    public static int GetCost(ToolType t)
    {
        switch (t)
        {
            case ToolType.Pond:          return 2;
            case ToolType.HedgehogHouse: return 2;
            default:                     return 1;
        }
    }
}
