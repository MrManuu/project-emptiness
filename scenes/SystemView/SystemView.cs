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
    private const float PlanetSpriteHalf = 22f;
    private const float StationHalf      = 8f;
    private const float ClickThreshold   = 24f;

    private static readonly float[] OrbitRadii  = { 130f, 210f, 300f, 400f, 510f, 630f };
    private static readonly float[] OrbitSpeeds = { 0.18f, 0.13f, 0.09f, 0.065f, 0.048f, 0.036f };

    private static readonly Color OrbitColor = new(1f, 1f, 1f, 0.06f);
    private static readonly Color TextColor  = new(0.75f, 0.8f, 0.95f, 0.65f);

    private const string FallbackPlanetPath = "res://assets/planets/planet-barren.png";

    // Planet Sprite2D nodes (positioned each frame)
    private Sprite2D[] _planetSprites = Array.Empty<Sprite2D>();

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

        // Background
        var bgRect = GetNode<TextureRect>("Background/BgRect");
        bgRect.Texture  = GD.Load<Texture2D>("res://assets/backgrounds/bg-nebula-blue.png");
        bgRect.Modulate = new Color(1f, 1f, 1f, 1f);

        // Planet sprites — load individual PNGs, fall back to barren
        float planetScale = PlanetSpriteHalf * 2f / 1024f;
        _planetSprites = new Sprite2D[_system.Planets.Count];
        for (int i = 0; i < _system.Planets.Count; i++)
        {
            string path = GetPlanetPath(_system.Planets[i].Type);
            var sprite = new Sprite2D
            {
                Texture = GD.Load<Texture2D>(path),
                Scale   = new Vector2(planetScale, planetScale),
                ZIndex  = 1
            };
            AddChild(sprite);
            _planetSprites[i] = sprite;
        }

        // Player ship — transparent PNG, no shader needed
        var ship = GetNode<Sprite2D>("PlayerShip");
        ship.Texture = GD.Load<Texture2D>("res://assets/ships/ship-freighter.png");
        ship.Scale   = new Vector2(0.05f, 0.05f);

        // Orbit angles
        int p = _system.Planets.Count;
        int s = _system.Stations.Count;
        _planetAngles  = new float[p];
        _stationAngles = new float[s];
        for (int i = 0; i < p; i++) _planetAngles[i]  = p > 1 ? i * (Mathf.Tau / p) : 0f;
        for (int i = 0; i < s; i++) _stationAngles[i] = (s > 1 ? i * (Mathf.Tau / s) : 0f) + 1.2f;

        // UI
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
        {
            _planetAngles[i] += OrbitSpeeds[Math.Min(i, OrbitSpeeds.Length - 1)] * dt;
            float r = OrbitRadii[Math.Min(i, OrbitRadii.Length - 1)];
            _planetSprites[i].Position = new Vector2(MathF.Cos(_planetAngles[i]) * r, MathF.Sin(_planetAngles[i]) * r);
        }
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
        DrawOrbitsLabelsStations();
    }

    private void DrawStar()
    {
        var col = StarColor(_system.StarType);
        DrawCircle(Vector2.Zero, StarRadius * 1.6f, new Color(col.R, col.G, col.B, 0.15f));
        DrawCircle(Vector2.Zero, StarRadius, col);
    }

    private void DrawOrbitsLabelsStations()
    {
        // Orbit rings
        for (int i = 0; i < _system.Planets.Count; i++)
            DrawArc(Vector2.Zero, OrbitRadii[Math.Min(i, OrbitRadii.Length - 1)],
                0, Mathf.Tau, 64, OrbitColor, 1f, true);

        for (int i = 0; i < _system.Stations.Count; i++)
        {
            int slot = _system.Planets.Count + i;
            DrawArc(Vector2.Zero, OrbitRadii[Math.Min(slot, OrbitRadii.Length - 1)],
                0, Mathf.Tau, 64, new Color(0.8f, 0.8f, 0.5f, 0.05f), 1f, true);
        }

        // Planet selection ring + label
        for (int i = 0; i < _system.Planets.Count; i++)
        {
            var pos = _planetSprites[i].Position;
            if (i == _selectedPlanet)
                DrawArc(pos, PlanetSpriteHalf + 5f, 0, Mathf.Tau, 24, new Color(1f, 1f, 1f, 0.6f), 1.5f, true);
            DrawString(ThemeDB.FallbackFont, pos + new Vector2(PlanetSpriteHalf + 3f, 3f),
                _system.Planets[i].Name, HorizontalAlignment.Left, -1, 10, TextColor);
        }

        // Stations
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
            if (worldPos.DistanceTo(_planetSprites[i].Position) < ClickThreshold)
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
    private void OnDayPassed(int _day)       => RefreshHUD();
    private void OnCreditsChanged(long _amt) => RefreshHUD();

    private void RefreshHUD()
    {
        var gs = GameState.Instance;
        _lblCredits.Text = $"Credits: {gs.PlayerShip.Credits:N0}";
        _lblDay.Text     = $"Day {gs.Day}";
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private static string GetPlanetPath(PlanetType type)
    {
        string path = type switch
        {
            PlanetType.Barren   => "res://assets/planets/planet-barren.png",
            PlanetType.Desert   => "res://assets/planets/planet-desert.png",
            PlanetType.Terran   => "res://assets/planets/planet-terran.png",
            PlanetType.Ocean    => "res://assets/planets/planet-ocean.png",
            PlanetType.Ice      => "res://assets/planets/planet-ice.png",
            PlanetType.Volcanic => "res://assets/planets/planet-volcanic.png",
            PlanetType.GasGiant => "res://assets/planets/planet-gasgiant.png",
            PlanetType.Toxic    => "res://assets/planets/planet-toxic.png",
            _                   => FallbackPlanetPath
        };
        return ResourceLoader.Exists(path) ? path : FallbackPlanetPath;
    }

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
