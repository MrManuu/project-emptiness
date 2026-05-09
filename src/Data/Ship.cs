#nullable enable
using System.Collections.Generic;

namespace ProjectEmptiness.Data;

public class PlayerShip
{
    public string Name { get; set; } = "Wanderer";
    public ShipClass Class { get; set; } = ShipClass.Freighter;
    public int CurrentSystemId { get; set; }
    public long Credits { get; set; } = 50_000;

    // Stats
    public float MaxHull { get; set; } = 1000f;
    public float CurrentHull { get; set; } = 1000f;
    public float MaxShields { get; set; } = 500f;
    public float CurrentShields { get; set; } = 500f;
    public float MaxFlux { get; set; } = 800f;
    public float CurrentFlux { get; set; } = 0f;
    public float Speed { get; set; } = 120f;
    public int CargoCapacity { get; set; } = 200;

    // Cargo
    public Dictionary<string, int> Cargo { get; set; } = new();
    public int UsedCargoSpace => GetUsedCargo();

    // Weapons slots (weapon id or null)
    public List<string?> WeaponSlots { get; set; } = new() { "laser_pulse", null, null, null };

    private int GetUsedCargo()
    {
        int total = 0;
        foreach (var amount in Cargo.Values)
            total += amount;
        return total;
    }

    public bool CanCarry(int amount) => UsedCargoSpace + amount <= CargoCapacity;
}

public class ShipTemplate
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public ShipClass Class { get; set; }
    public string Description { get; set; } = "";
    public long BaseCost { get; set; }
    public float MaxHull { get; set; }
    public float MaxShields { get; set; }
    public float MaxFlux { get; set; }
    public float Speed { get; set; }
    public int CargoCapacity { get; set; }
    public int WeaponSlots { get; set; }
    public string SpritePath { get; set; } = "";
}
