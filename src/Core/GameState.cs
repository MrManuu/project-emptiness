#nullable enable
using Godot;
using System.Collections.Generic;
using System.Linq;
using ProjectEmptiness.Data;
using ProjectEmptiness.Generation;

namespace ProjectEmptiness.Core;

public partial class GameState : Node
{
    public static GameState Instance { get; private set; } = null!;

    public List<StarSystem> Galaxy { get; private set; } = new();
    public List<Faction> Factions { get; private set; } = new();
    public PlayerShip PlayerShip { get; private set; } = new();
    public StarSystem CurrentSystem { get; set; } = null!;
    public string PlayerFactionId { get; set; } = "";

    // Game time
    public int Day { get; set; } = 1;

    // Signals
    [Signal] public delegate void SystemChangedEventHandler(int newSystemId);
    [Signal] public delegate void CreditsChangedEventHandler(long newAmount);
    [Signal] public delegate void DayPassedEventHandler(int day);

    public override void _Ready()
    {
        Instance = this;
        InitFactions();
        InitGalaxy();
        InitPlayer();
    }

    private void InitFactions()
    {
        Factions = new List<Faction>
        {
            new()
            {
                Id = "terran",
                Name = "Terran Confederation",
                Description = "A democratic coalition of core worlds. Values stability and trade.",
                Color = new Color(0.27f, 0.47f, 1f),
                PlayerReputation = 10f,
                Relations = new() { ["syndicate"] = -20f, ["void_collective"] = 5f, ["free_alliance"] = 40f, ["independent"] = 0f }
            },
            new()
            {
                Id = "syndicate",
                Name = "Iron Syndicate",
                Description = "A corporate military state. Controls the most powerful fleet.",
                Color = new Color(1f, 0.3f, 0.15f),
                PlayerReputation = 0f,
                Relations = new() { ["terran"] = -20f, ["void_collective"] = -50f, ["free_alliance"] = -30f, ["independent"] = -10f }
            },
            new()
            {
                Id = "void_collective",
                Name = "Void Collective",
                Description = "Mysterious technocrats from the outer rim. Advanced but isolationist.",
                Color = new Color(0.65f, 0.25f, 1f),
                PlayerReputation = -5f,
                Relations = new() { ["terran"] = 5f, ["syndicate"] = -50f, ["free_alliance"] = 15f, ["independent"] = 20f }
            },
            new()
            {
                Id = "free_alliance",
                Name = "Free Planets Alliance",
                Description = "A loose coalition of traders and rebels. No central authority.",
                Color = new Color(0.2f, 0.9f, 0.45f),
                PlayerReputation = 15f,
                Relations = new() { ["terran"] = 40f, ["syndicate"] = -30f, ["void_collective"] = 15f, ["independent"] = 30f }
            },
            new()
            {
                Id = "independent",
                Name = "Independent",
                Description = "Unaffiliated systems. Pirates, traders, and frontier settlers.",
                Color = new Color(0.55f, 0.55f, 0.6f),
                PlayerReputation = 0f,
                Relations = new()
            }
        };
    }

    private void InitGalaxy()
    {
        var generator = new GalaxyGenerator();
        Galaxy = generator.Generate(seed: 1337, systemCount: 64);
    }

    private void InitPlayer()
    {
        var startSystem = Galaxy.FirstOrDefault(s => s.IsStartingSystem)
            ?? Galaxy[0];

        CurrentSystem = startSystem;

        PlayerShip = new PlayerShip
        {
            Name = "Wanderer",
            Class = ShipClass.Freighter,
            CurrentSystemId = startSystem.Id,
            Credits = 50_000,
            CargoCapacity = 200,
        };
    }

    public Faction? GetFaction(string id) =>
        Factions.FirstOrDefault(f => f.Id == id);

    public StarSystem? GetSystem(int id) =>
        Galaxy.FirstOrDefault(s => s.Id == id);

    public bool CanJumpTo(StarSystem target) =>
        CurrentSystem.IsConnectedTo(target.Id);

    public void JumpToSystem(StarSystem target)
    {
        if (!CanJumpTo(target)) return;
        CurrentSystem = target;
        PlayerShip.CurrentSystemId = target.Id;
        EmitSignal(SignalName.SystemChanged, target.Id);
    }

    public void AddCredits(long amount)
    {
        PlayerShip.Credits += amount;
        EmitSignal(SignalName.CreditsChanged, PlayerShip.Credits);
    }
}
