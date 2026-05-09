using Godot;
using System.Collections.Generic;

namespace ProjectEmptiness.Data;

public class StarSystem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Vector2 MapPosition { get; set; }
    public StarType StarType { get; set; }
    public string FactionId { get; set; } = "independent";
    public List<int> ConnectedSystemIds { get; set; } = new();
    public List<Planet> Planets { get; set; } = new();
    public int PlanetCount { get; set; }
    public SecurityLevel Security { get; set; }
    public bool IsStartingSystem { get; set; }
    public bool IsPlayerOwned { get; set; }
    public List<Station> Stations { get; set; } = new();

    public bool IsConnectedTo(int systemId) => ConnectedSystemIds.Contains(systemId);
}

public class Planet
{
    public string Name { get; set; } = "";
    public PlanetType Type { get; set; }
    public int Population { get; set; }
    public List<string> Resources { get; set; } = new();
}

public class Station
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string FactionId { get; set; } = "";
    public string OwnerId { get; set; } = "";
    public bool IsPlayerOwned { get; set; }
    public Dictionary<string, int> Stock { get; set; } = new();
    public Dictionary<string, float> Prices { get; set; } = new();
}
