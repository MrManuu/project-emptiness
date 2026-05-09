#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectEmptiness.Data;

namespace ProjectEmptiness.Generation;

public class GalaxyGenerator
{
    private Random _rng = null!;
    private readonly HashSet<string> _usedNames = new();

    private static readonly string[] StarNames =
    {
        "Arcturus", "Procyon", "Rigel", "Aldebaran", "Antares", "Pollux", "Regulus",
        "Spica", "Sirius", "Vega", "Deneb", "Altair", "Canopus", "Fomalhaut",
        "Achernar", "Hadar", "Mimosa", "Gacrux", "Acrux", "Alioth", "Dubhe",
        "Merak", "Phecda", "Megrez", "Alcor", "Mizar", "Alkaid", "Thuban",
        "Rastaban", "Eltanin", "Kochab", "Pherkad", "Alderamin", "Caph", "Schedar",
        "Ruchbah", "Segin", "Navi", "Mirfak", "Algol", "Almach", "Hamal",
        "Menkar", "Diphda", "Ankaa", "Sadalsuud", "Sadalmelik", "Markab"
    };

    private static readonly string[] Prefixes =
        { "Sol", "Alpha", "Beta", "Gamma", "Delta", "Eta", "Tau", "Zeta", "Kappa", "Sigma", "Nova", "Ara" };

    private static readonly string[] Suffixes =
        { "Prime", "Minor", "Major", "VI", "VII", "IX", "XII", "A", "B", "II", "III", "IV" };

    public List<StarSystem> Generate(int seed, int systemCount = 64)
    {
        _rng = new Random(seed);
        _usedNames.Clear();

        var systems = new List<StarSystem>();

        for (int i = 0; i < systemCount; i++)
        {
            var system = PlaceSystem(i, systems);
            systems.Add(system);
        }

        ConnectSystems(systems);
        AssignFactions(systems);

        // Mark starting system: closest to center in Terran territory
        var startingSystem = systems
            .Where(s => s.FactionId == "terran")
            .OrderBy(s => s.MapPosition.Length())
            .First();
        startingSystem.IsStartingSystem = true;

        GenerateStations(systems);

        return systems;
    }

    private StarSystem PlaceSystem(int id, List<StarSystem> existing)
    {
        Vector2 pos;
        int attempts = 0;
        const float minDistance = 65f;
        const float galaxyRadius = 820f;

        do
        {
            // Spiral arm distribution
            float t = (float)_rng.NextDouble();
            float angle = (float)(_rng.NextDouble() * MathF.PI * 2f);
            float r = MathF.Sqrt(t) * galaxyRadius;

            // Add arm winding
            float armAngle = angle + r * 0.003f;
            float spread = (float)((_rng.NextDouble() - 0.5) * 180f * (r / galaxyRadius));

            pos = new Vector2(
                MathF.Cos(armAngle) * r + spread,
                MathF.Sin(armAngle) * r * 0.75f + spread * 0.5f
            );
            attempts++;
        }
        while (attempts < 60 && existing.Any(s => s.MapPosition.DistanceTo(pos) < minDistance));

        return new StarSystem
        {
            Id = id,
            Name = GenerateName(),
            MapPosition = pos,
            StarType = RollStarType(),
            PlanetCount = _rng.Next(0, 8),
            Security = (SecurityLevel)_rng.Next(0, 5),
            FactionId = "independent"
        };
    }

    private StarType RollStarType()
    {
        float roll = (float)_rng.NextDouble();
        return roll switch
        {
            < 0.35f => StarType.Red,
            < 0.58f => StarType.Yellow,
            < 0.74f => StarType.Orange,
            < 0.86f => StarType.White,
            < 0.94f => StarType.Blue,
            _ => StarType.Neutron
        };
    }

    private void ConnectSystems(List<StarSystem> systems)
    {
        // Connect each system to its 2-3 nearest neighbors
        foreach (var system in systems)
        {
            int connectionCount = _rng.Next(2, 4);
            var nearest = systems
                .Where(s => s.Id != system.Id)
                .OrderBy(s => s.MapPosition.DistanceTo(system.MapPosition))
                .Take(connectionCount + 2);

            int added = 0;
            foreach (var neighbor in nearest)
            {
                if (added >= connectionCount) break;
                if (system.ConnectedSystemIds.Contains(neighbor.Id)) continue;

                system.ConnectedSystemIds.Add(neighbor.Id);
                neighbor.ConnectedSystemIds.Add(system.Id);
                added++;
            }
        }

        // Ensure full connectivity via union-find
        EnsureConnected(systems);
    }

    private void EnsureConnected(List<StarSystem> systems)
    {
        var parent = new Dictionary<int, int>();
        foreach (var s in systems) parent[s.Id] = s.Id;

        int Find(int id)
        {
            while (parent[id] != id) id = parent[id] = parent[parent[id]];
            return id;
        }

        void Union(int a, int b) => parent[Find(a)] = Find(b);

        foreach (var s in systems)
            foreach (var neighborId in s.ConnectedSystemIds)
                Union(s.Id, neighborId);

        // Connect isolated components
        var components = systems.GroupBy(s => Find(s.Id)).ToList();
        for (int i = 1; i < components.Count; i++)
        {
            var a = components[i].OrderBy(s => s.MapPosition.Length()).First();
            var b = components[0]
                .OrderBy(s => s.MapPosition.DistanceTo(a.MapPosition))
                .First();

            a.ConnectedSystemIds.Add(b.Id);
            b.ConnectedSystemIds.Add(a.Id);
        }
    }

    private void AssignFactions(List<StarSystem> systems)
    {
        string[] mainFactions = { "terran", "syndicate", "void_collective", "free_alliance" };

        // Pick well-separated capitals
        var capitals = new List<StarSystem>();
        var shuffled = systems.OrderBy(_ => _rng.Next()).ToList();

        foreach (var factionId in mainFactions)
        {
            StarSystem? capital = shuffled.FirstOrDefault(s =>
                !capitals.Any(c => c.MapPosition.DistanceTo(s.MapPosition) < 300f));

            capital ??= shuffled.First(s => !capitals.Contains(s));
            capital.FactionId = factionId;
            capitals.Add(capital);
        }

        // Flood-fill faction ownership from capitals
        foreach (var system in systems.Where(s => s.FactionId == "independent"))
        {
            var nearest = capitals
                .OrderBy(c => c.MapPosition.DistanceTo(system.MapPosition))
                .First();

            float dist = nearest.MapPosition.DistanceTo(system.MapPosition);
            system.FactionId = dist < 320f ? nearest.FactionId : "independent";
        }
    }

    private void GenerateStations(List<StarSystem> systems)
    {
        foreach (var system in systems)
        {
            // Most faction systems have a station, independent rarely do
            bool hasStation = system.FactionId != "independent"
                ? _rng.NextDouble() < 0.8
                : _rng.NextDouble() < 0.25;

            if (!hasStation) continue;

            system.Stations.Add(new Station
            {
                Id = $"station_{system.Id}_0",
                Name = $"{system.Name} Station",
                FactionId = system.FactionId,
                Stock = GenerateInitialStock(),
                Prices = GenerateInitialPrices()
            });
        }
    }

    private Dictionary<string, int> GenerateInitialStock()
    {
        return new Dictionary<string, int>
        {
            ["food"] = _rng.Next(0, 500),
            ["metal"] = _rng.Next(0, 300),
            ["fuel"] = _rng.Next(50, 400),
            ["machinery"] = _rng.Next(0, 150),
            ["electronics"] = _rng.Next(0, 100),
            ["luxuries"] = _rng.Next(0, 80),
        };
    }

    private Dictionary<string, float> GenerateInitialPrices()
    {
        float variance() => 0.8f + (float)_rng.NextDouble() * 0.4f;
        return new Dictionary<string, float>
        {
            ["food"] = 40f * variance(),
            ["metal"] = 120f * variance(),
            ["fuel"] = 80f * variance(),
            ["machinery"] = 350f * variance(),
            ["electronics"] = 600f * variance(),
            ["luxuries"] = 900f * variance(),
        };
    }

    private string GenerateName()
    {
        string name;
        int attempts = 0;
        do
        {
            name = _rng.NextDouble() < 0.65 && StarNames.Length > _usedNames.Count
                ? StarNames[_rng.Next(StarNames.Length)]
                : $"{Prefixes[_rng.Next(Prefixes.Length)]} {Suffixes[_rng.Next(Suffixes.Length)]}";
            attempts++;
        }
        while (_usedNames.Contains(name) && attempts < 30);

        _usedNames.Add(name);
        return name;
    }
}
