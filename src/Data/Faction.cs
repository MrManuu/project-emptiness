using Godot;
using System.Collections.Generic;

namespace ProjectEmptiness.Data;

public class Faction
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Color Color { get; set; }

    // Relations with other factions: key = other faction id, value = -100 (war) to 100 (ally)
    public Dictionary<string, float> Relations { get; set; } = new();

    // Player's relation with this faction
    public float PlayerReputation { get; set; } = 0f;

    public FactionStance GetStanceToward(string otherFactionId)
    {
        if (!Relations.TryGetValue(otherFactionId, out float rel))
            return FactionStance.Neutral;

        return rel switch
        {
            >= 75f => FactionStance.Ally,
            >= 30f => FactionStance.Friendly,
            >= -10f => FactionStance.Neutral,
            >= -40f => FactionStance.Unfriendly,
            >= -70f => FactionStance.Hostile,
            _ => FactionStance.AtWar
        };
    }

    public FactionStance GetPlayerStance() => PlayerReputation switch
    {
        >= 75f => FactionStance.Ally,
        >= 30f => FactionStance.Friendly,
        >= -10f => FactionStance.Neutral,
        >= -40f => FactionStance.Unfriendly,
        >= -70f => FactionStance.Hostile,
        _ => FactionStance.AtWar
    };
}
