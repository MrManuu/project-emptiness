using Godot;
using System;
using System.Linq;
using ProjectEmptiness.Core;

namespace ProjectEmptiness.Simulation;

public partial class SimulationManager : Node
{
    public static SimulationManager Instance { get; private set; } = null!;

    // 1 real second = 1 in-game hour. A day ticks every 24 real seconds.
    private const float HoursPerSecond = 1f;
    private const float HoursPerDay = 24f;

    private float _hourAccumulator = 0f;
    private Random _rng = new(42);

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        _hourAccumulator += (float)delta * HoursPerSecond;

        if (_hourAccumulator >= HoursPerDay)
        {
            _hourAccumulator -= HoursPerDay;
            OnDayTick();
        }
    }

    private void OnDayTick()
    {
        var state = GameState.Instance;
        if (state == null) return;

        state.Day++;
        TickEconomy(state);
        TickFactionRelations(state);

        state.EmitSignal(GameState.SignalName.DayPassed, state.Day);
    }

    private void TickEconomy(GameState state)
    {
        foreach (var system in state.Galaxy)
        {
            foreach (var station in system.Stations)
            {
                // Simulate consumption and production
                foreach (var goodId in station.Stock.Keys.ToList())
                {
                    // Random consumption
                    int consumption = _rng.Next(0, 15);
                    station.Stock[goodId] = Math.Max(0, station.Stock[goodId] - consumption);

                    // Random production / restocking
                    int production = _rng.Next(0, 12);
                    station.Stock[goodId] = Math.Min(500, station.Stock[goodId] + production);

                    // Price adjusts with supply: low stock → higher price
                    if (station.Prices.TryGetValue(goodId, out float basePrice))
                    {
                        float supplyFactor = station.Stock[goodId] / 300f;
                        float priceMod = 1f + (0.5f - supplyFactor) * 0.6f;
                        float noise = (float)(_rng.NextDouble() - 0.5) * 0.05f;
                        station.Prices[goodId] = basePrice * Mathf.Clamp(priceMod + noise, 0.5f, 2.5f);
                    }
                }
            }
        }
    }

    private void TickFactionRelations(GameState state)
    {
        // Very slow random drift of faction relations
        if (_rng.NextDouble() > 0.05) return; // only 5% chance per day

        var factions = state.Factions;
        if (factions.Count < 2) return;

        int idxA = _rng.Next(factions.Count);
        int idxB = _rng.Next(factions.Count);
        if (idxA == idxB) return;

        var a = factions[idxA];
        var b = factions[idxB];
        if (b.Id == "independent") return;

        float delta = (float)(_rng.NextDouble() - 0.5) * 4f;
        if (a.Relations.ContainsKey(b.Id))
        {
            a.Relations[b.Id] = Mathf.Clamp(a.Relations[b.Id] + delta, -100f, 100f);
            float bVal = b.Relations.ContainsKey(a.Id) ? b.Relations[a.Id] : 0f;
            b.Relations[a.Id] = Mathf.Clamp(bVal + delta, -100f, 100f);
        }
    }
}
