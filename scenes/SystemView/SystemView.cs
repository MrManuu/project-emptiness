#nullable enable
using Godot;
using System;
using System.Linq;
using System.Text;
using ProjectEmptiness.Core;
using ProjectEmptiness.Data;

namespace ProjectEmptiness.Scenes;

public partial class SystemView : Node2D
{
    private static readonly PackedScene GalaxyMapScene =
        GD.Load<PackedScene>("res://scenes/GalaxyMap/GalaxyMap.tscn");

    private const float StarRadius       = 40f;
    private const float PlanetSpriteHalf = 20f;
    private const float StationHalf      = 8f;
    private const float ClickThreshold   = 22f;

    private static readonly Rect2 RockyRegion = new(0f,   512f, 512f, 512f);
    private static readonly Rect2 BlueRegion  = new(512f, 512f, 512f, 512f);

    private static readonly float[] OrbitRadii  = { 130f, 210f, 300f, 400f, 510f, 630f };
    private static readonly float[] OrbitSpeeds = { 0.18f, 0.13f, 0.09f, 0.065f, 0.048f, 0.036f };

    private static readonly Color OrbitColor = new(1f, 1f, 1f, 0.06f);
    private static readonly Color TextColor  = new(0.75f, 0.8f, 0.95f, 0.65f);

    // Textures
    private Texture2D _planetSheet1 = null!;
    private Texture2D _planetSheet2 = null!;

    // State
    private Camera2D   _camera        = null!;
    private StarSystem _system        = null!;
    private float[]    _planetAngles  = Array.Empty<float>();
    private float[]    _stationAngles = Array.Empty<float>();
    private int        _selectedPlanet  = -1;
    private int        _selectedStation = -1;

    // UI
    private Label   _lblSystemTitle = null!;
    private Control _infoPanel      = null!;
    private Label   _lblObjectName  = null!;
    private Label   _lblDetails     = null!;
    private Button  _btnDock        = null!;
    private Label   _lblCredits     = null!;
    private Label   _lblDay         = null!;

    public override void _Ready()
    {
        _camera = GetNode<Camera2D>("Camera2D");
        _system = GameState.Instance.CurrentSystem!;

        // Background nebula
        var bgRect = GetNode<TextureRect>("Background/BgRect");
        bgRect.Texture  = GD.Load<Texture2D>("res://assets/concepts/gpt-image-2/pack-02-painterly-probe/background-01.png");
        bgRect.Modulate = new Color(0.6f, 0.55f, 0.7f, 1f);

        // Planet sheets (1024x1024, 2x2 grid — planets in bottom half)
        _planetSheet1 = GD.Load<Texture2D>("res://assets/concepts/gpt-image-2/pack-01-clean-tactical/planet-01.png");
        _planetSheet2 = GD.Load<Texture2D>("res://assets/concepts/gpt-image-2/pack-01-clean-tactical/planet-02.png");

        // Player ship
        var ship = GetNode<Sprite2D>("PlayerShip");
        ship.Texture = GD.Load<Texture2D>("res://assets/concepts/gpt-image-2/pack-01-clean-tactical/ship-01.png");
        ship.Scale   = new Vector2(0.055f, 0.055f);

        int p = _system.Planets.Count;
        int s = _system.Stations.Count;
        _planetAngles  = new float[p];
        _stationAngles = new float[s];

        for (int i = 0; i < p; i++)
            _planetAngles[i] = p > 1 ? i * (Mathf.Tau / p) : 0f;
        for (int i = 0; i < s; i++)
            _stationAngles[i] = (s > 1 ? i * (Mathf.Tau / s) : 0f) + 1.2f;

        _lblSystemTitle = GetNode<Label>("UI/HUD/TopBar/HBoxContainer/SystemTitle");
        _infoPanel      = GetNode<Control>("UI/InfoPanel");
        _lblObjectName  = GetNode<Label>("UI/InfoPanel/VBox/ObjectName");
        _lblDetails     = GetNode<Label>("UI/InfoPanel/VBox/Details");
        _btnDock        = GetNode<Button>("UI/InfoPanel/VBox/DockBtn");
        _lblCredits     = GetNode<Label>("UI/HUD/TopBar/HBoxContainer/Credits");
        _lblDay         = GetNode<Label>("UI/HUD/TopBar/HBoxContainer/Day");

        GetNode<Button>("UI/HUD/TopBar/HBoxContainer/BackBtn").Pressed += OnBackPressed;
        _btnDock.Pressed += OnDockPressed;
        _infoPanel.Visible = false;

        GameState.Instance.DayPassed      += OnDayPassed;
        GameState.Instance.CreditsChanged += OnCreditsChanged;

        _lblSystemTitle.Text = $"{_system.Name}  —  {_system.StarType} Star";
        RefreshHUD();
    }

    public override void _ExitTree()
    {
        GameState.Instance.DayPassed      -= OnDayPassed;
        GameState.Instance.CreditsChanged -= OnCreditsChanged;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        for (int i = 0; i < _planetAngles.Length; i++)
            _planetAngles[i] += OrbitSpeeds[Math.Min(i, OrbitSpeeds.Length - 1)] * dt;
        for (int i = 0; i < _stationAngles.Length; i++)
        {
            int slot = _system.Planets.Count + i;
            _stationAngles[i] += OrbitSpeeds[Math.Min(slot, OrbitSpeeds.Length - 1)] * 0.7f * dt;
        }
        QueueRedraw();
    }

    // ── Drawing ────────────────────────────────────────────────────────────────
    public override void _Draw()
    {
        if (_system == null) return;
        DrawStar();
        DrawPlanetsAndStations();
    }

    private void DrawStar()
    {
        var col = StarColor(_system.StarType);
        DrawCircle(Vector2.Zero, StarRadius * 1.6f, new Color(col.R, col.G, col.B, 0.15f));
        DrawCircle(Vector2.Zero, StarRadius, col);
    }

    private void DrawPlanetsAndStations()
    {
        for (int i = 0; i < _system.Planets.Count; i++)
            DrawArc(Vector2.Zero, OrbitRadii[Math.Min(i, OrbitRadii.Length - 1)],
                0, Mathf.Tau, 64, OrbitColor, 1f, true);

        for (int i = 0; i < _system.Stations.Count; i++)
        {
            int slot = _system.Planets.Count + i;
            DrawArc(Vector2.Zero, OrbitRadii[Math.Min(slot, OrbitRadii.Length - 1)],
                0, Mathf.Tau, 64, new Color(0.8f, 0.8f, 0.5f, 0.05f), 1f, true);
        }

        for (int i = 0; i < _system.Planets.Count; i++)
        {
            float r   = OrbitRadii[Math.Min(i, OrbitRadii.Length - 1)];
            var   pos = new Vector2(MathF.Cos(_planetAngles[i]) * r, MathF.Sin(_planetAngles[i]) * r);

            if (i == _selectedPlanet)
                DrawArc(pos, PlanetSpriteHalf + 5f, 0, Mathf.Tau, 24, new Color(1f, 1f, 1f, 0.6f), 1.5f, true);

            var (sheet, region) = GetPlanetSprite(_system.Planets[i].Type);
            var destRect = new Rect2(pos - new Vector2(PlanetSpriteHalf, PlanetSpriteHalf),
                                     new Vector2(PlanetSpriteHalf * 2f, PlanetSpriteHalf * 2f));
            DrawTextureRectRegion(sheet, destRect, region);

            DrawString(ThemeDB.FallbackFont, pos + new Vector2(PlanetSpriteHalf + 3f, 3f),
                _system.Planets[i].Name, HorizontalAlignment.Left, -1, 10, TextColor);
        }

        for (int i = 0; i < _system.Stations.Count; i++)
        {
            int   slot = _system.Planets.Count + i;
            float r    = OrbitRadii[Math.Min(slot, OrbitRadii.Length - 1)];
            var   pos  = new Vector2(MathF.Cos(_stationAngles[i]) * r, MathF.Sin(_stationAngles[i]) * r);

            if (i == _selectedStation)
                DrawArc(pos, StationHalf + 6f, 0, Mathf.Tau, 24, new Color(1f, 1f, 0.6f, 0.7f), 1.5f, true);

            DrawPolygon(new Vector2[]
            {
                pos + new Vector2(0f,         -StationHalf),
                pos + new Vector2(StationHalf,  0f),
                pos + new Vector2(0f,           StationHalf),
                pos + new Vector2(-StationHalf, 0f),
            }, new Color[] { new Color(0.9f, 0.9f, 0.6f, 0.9f) });

            DrawString(ThemeDB.FallbackFont, pos + new Vector2(StationHalf + 4f, 3f),
                _system.Stations[i].Name, HorizontalAlignment.Left, -1, 10,
                new Color(0.9f, 0.9f, 0.6f, 0.6f));
        }
    }

    // ── Input ──────────────────────────────────────────────────────────────────
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp)   ZoomCamera(1.12f);
            if (mb.ButtonIndex == MouseButton.WheelDown) ZoomCamera(1f / 1.12f);
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
                TrySelect(GetGlobalMousePosition());
        }
    }

    private void ZoomCamera(float factor)
    {
        float x = Mathf.Clamp(_camera.Zoom.X * factor, 0.3f, 4f);
        _camera.Zoom = new Vector2(x, x);
    }

    private void TrySelect(Vector2 worldPos)
    {
        for (int i = 0; i < _system.Planets.Count; i++)
        {
            float r   = OrbitRadii[Math.Min(i, OrbitRadii.Length - 1)];
            var   pos = new Vector2(MathF.Cos(_planetAngles[i]) * r, MathF.Sin(_planetAngles[i]) * r);
            if (worldPos.DistanceTo(pos) < ClickThreshold)
            {
                _selectedPlanet  = i;
                _selectedStation = -1;
                ShowPlanetInfo(_system.Planets[i]);
                return;
            }
        }

        for (int i = 0; i < _system.Stations.Count; i++)
        {
            int   slot = _system.Planets.Count + i;
            float r    = OrbitRadii[Math.Min(slot, OrbitRadii.Length - 1)];
            var   pos  = new Vector2(MathF.Cos(_stationAngles[i]) * r, MathF.Sin(_stationAngles[i]) * r);
            if (worldPos.DistanceTo(pos) < ClickThreshold)
            {
                _selectedStation = i;
                _selectedPlanet  = -1;
                ShowStationInfo(_system.Stations[i]);
                return;
            }
        }

        _selectedPlanet  = -1;
        _selectedStation = -1;
        _infoPanel.Visible = false;
    }

    // ── Info Panel ─────────────────────────────────────────────────────────────
    private void ShowPlanetInfo(Planet p)
    {
        _lblObjectName.Text = p.Name;
        var sb = new StringBuilder();
        sb.AppendLine($"Type:       {p.Type}");
        sb.AppendLine($"Population: {(p.Population > 0 ? $"{p.Population / 1_000_000f:0.#}M" : "Uninhabited")}");
        if (p.Resources.Count > 0)
            sb.AppendLine($"Resources:  {string.Join(", ", p.Resources)}");
        _lblDetails.Text   = sb.ToString().TrimEnd();
        _btnDock.Visible   = false;
        _infoPanel.Visible = true;
    }

    private void ShowStationInfo(Station s)
    {
        var faction = GameState.Instance.GetFaction(s.FactionId);
        _lblObjectName.Text = s.Name;
        var sb = new StringBuilder();
        sb.AppendLine($"Faction: {faction?.Name ?? "Independent"}");
        sb.AppendLine();
        sb.AppendLine("— Goods available —");
        bool anyStock = false;
        foreach (var kv in s.Stock.OrderBy(kv => kv.Key))
        {
            if (kv.Value <= 0) continue;
            anyStock = true;
            float price = s.Prices.ContainsKey(kv.Key) ? s.Prices[kv.Key] : 0f;
            sb.AppendLine($"  {Capitalize(kv.Key),-14} {kv.Value,4} u  {price:0} cr");
        }
        if (!anyStock) sb.AppendLine("  (no stock)");
        _lblDetails.Text   = sb.ToString().TrimEnd();
        _btnDock.Visible   = true;
        _infoPanel.Visible = true;
    }

    private void OnDockPressed() { /* Day 3: open StationTrade */ }

    private void OnBackPressed() =>
        GetTree().Root.GetNode<Main>("Main").LoadScene(GalaxyMapScene);

    // ── HUD ────────────────────────────────────────────────────────────────────
    private void OnDayPassed(int _day)      => RefreshHUD();
    private void OnCreditsChanged(long _amt) => RefreshHUD();

    private void RefreshHUD()
    {
        var gs = GameState.Instance;
        _lblCredits.Text = $"Credits: {gs.PlayerShip.Credits:N0}";
        _lblDay.Text     = $"Day {gs.Day}";
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private (Texture2D sheet, Rect2 region) GetPlanetSprite(PlanetType type) => type switch
    {
        PlanetType.Terran   => (_planetSheet1, BlueRegion),
        PlanetType.Ocean    => (_planetSheet2, BlueRegion),
        PlanetType.Ice      => (_planetSheet1, BlueRegion),
        PlanetType.Barren   => (_planetSheet1, RockyRegion),
        PlanetType.Desert   => (_planetSheet2, RockyRegion),
        PlanetType.Volcanic => (_planetSheet2, RockyRegion),
        PlanetType.GasGiant => (_planetSheet1, RockyRegion),
        PlanetType.Toxic    => (_planetSheet2, RockyRegion),
        _                   => (_planetSheet1, RockyRegion)
    };

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];

    private static Color StarColor(StarType t) => t switch
    {
        StarType.Yellow  => new Color(1f,    0.87f, 0.22f),
        StarType.Orange  => new Color(1f,    0.55f, 0.12f),
        StarType.Red     => new Color(0.92f, 0.22f, 0.18f),
        StarType.Blue    => new Color(0.28f, 0.58f, 1f),
        StarType.White   => new Color(0.92f, 0.94f, 1f),
        StarType.Neutron => new Color(0.68f, 0.28f, 1f),
        _                => new Color(1f,    1f,    1f)
    };

}
