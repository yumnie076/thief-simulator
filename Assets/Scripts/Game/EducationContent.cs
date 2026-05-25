using System.Collections.Generic;

/// <summary>
/// Static educational content for Egel op Expeditie.
/// All text is in Dutch as required by the Avans project brief.
/// </summary>
public static class EducationContent
{
    /// <summary>Facts shown as popup after the player places an element.</summary>
    public static readonly Dictionary<string, string> Facts = new()
    {
        ["RemoveTile"] = "Minder verharding = meer ruimte voor planten en regenwater dat de bodem in kan.",
        ["Flower"]     = "Wilde bloemen trekken bijen, vlinders en kevers — belangrijk voedsel voor egels.",
        ["Bush"]       = "Struiken bieden schuilplek voor egels, kleine vogels en insecten.",
        ["Tree"]       = "Bomen verkoelen je tuin, vangen CO₂ op en huisvesten vogels en insecten.",
        ["Pond"]       = "Een vijver met schuine kant geeft egels en vogels drinkwater — zonder dat ze verdrinken.",
        ["LeafPile"]   = "Een bladhoop onder een struik is dé winterslaap-plek voor de egel.",
        ["House"]      = "Een egelhuis biedt veiligheid tegen rovers, kou en regen.",
    };

    /// <summary>Tips shown on the result screen at the end of the game.</summary>
    public static readonly Dictionary<string, string> EndTips = new()
    {
        ["FenceGap"]   = "Tip: maak een gat van 13×13 cm in je schutting — een egelpoort. Zo kunnen egels tussen tuinen reizen.",
        ["NoPellets"]  = "Gebruik geen slakkenkorrels. Die doden 200.000 egels per jaar in Nederland.",
        ["NoMowing"]   = "Check altijd je grasveld voordat je maait — vooral 's avonds en in lang gras.",
        ["WaterBowl"]  = "Geen vijver? Een ondiepe waterbak (max 5cm) helpt egels en vogels in droge zomers.",
        ["Native"]     = "Plant inheemse soorten — die voeden meer insecten dan exotische tuinplanten.",
    };
}
