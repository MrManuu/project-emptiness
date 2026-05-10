#nullable enable
using Godot;
using System;
using System.Linq;
using ProjectEmptiness.Core;
using ProjectEmptiness.Data;

namespace ProjectEmptiness.Scenes;

public partial class GalaxyMap : Node2D
{
    private static readonly PackedScene SystemViewScene =
        GD.Load<PackedScene>("res://scenes/SystemView/SystemView.tscn");
    // ── Visual constants ───────────────────────────────────────────────────────
    private const float StarRadius       = 7f;
    private const float GlowRadius       = 22f;
    private const float ClickThreshold   = 24f;

    private static readonly Color HyperlaneColor   = new(1f, 1f, 1f, 0.08f);
    private static readonly Color TextColor         = new(0.75f, 0.8f, 0.95f, 0.65f);
    private static readonly Color PlayerRingColor   = new(1f, 1f, 1f, 0.9f);
    private static readonly Color SelectedRingColor = new(1f, 1f, 1f, 0.6f);
    private static readonly Color BgColor           = new(0.031f, 0.031f, 0.07f);

    // ── State ──────────────────────────────────────────────────────────────────
    private Camera2D _camera = null!;
    private StarSystem? _selected;
    private StarSystem? _hovered;
    private bool _isDragging;
    private Vector2 _dragOriginScreen;
    private Vector2 _cameraOriginPos;
    private float _pulse;

    // ── UI nodes ───────────────────────────────────────────────────────────────
    private Control  _infoPanel     = null!;
    private Label    _lblSystemName  = null!;
    private Label    _lblStarType    = null!;
    private Label    _lblFaction     = null!;
    private Label    _lblPlanets     = null!;
    private Label    _lblSecurity    = null!;
    private Button   _btnJump        = null!;
    private Button   _btnEnter       = null!;
    private Label    _lblCredits     = null!;
    private Label    _lblDay         = null!;
    private Label    _lblLocation    = null!;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    public override void _Ready()
    {
        _camera = GetNode<Camera2D>("Camera2D");

        // Position camera on player's starting system
        var gs = GameState.Instance;
        if (gs?.CurrentSystem != null)
            _camera.Position = gs.CurrentSystem.MapPosition;

        // UI refs
        _infoPanel    = GetNode<Control>("UI/InfoPanel");
        _lblSystemName = GetNode<Label>("UI/InfoPanel/VBox/SystemName");
        _lblStarType   = GetNode<Label>("UI/InfoPanel/VBox/StarType");
        _lblFaction    = GetNode<Label>("UI/InfoPanel/VBox/Faction");
        _lblPlanets    = GetNode<Label>("UI/InfoPanel/VBox/Planets");
        _lblSecurity   = GetNode<Label>("UI/InfoPanel/VBox/Security");
        _btnJump       = GetNode<Button>("UI/InfoPanel/VBox/JumpBtn");
        _btnEnter      = GetNode<Button>("UI/InfoPanel/VBox/EnterBtn");
        _lblCredits    = GetNode<Label>("UI/HUD/TopBar/HBoxContainer/Credits");
        _lblDay        = GetNode<Label>("UI/HUD/TopBar/HBoxContainer/Day");
        _lblLocation   = GetNode<Label>("UI/HUD/TopBar/HBoxContainer/Location");

        GetNode<TextureRect>("Background/BgRect").Texture =
            GD.Load<Texture2D>("res://assets/backgrounds/bg-nebula-blue.png");
        GetNode<TextureRect>("Background/BgRect").Modulate = new Color(0.7f, 0.7f, 0.85f, 1f);

        _btnJump.Pressed  += OnJumpPressed;
        _btnEnter.Pressed += OnEnterPressed;
        _infoPanel.Visible = false;

        GameState.Instance.SystemChanged += OnSystemChanged;
        GameState.Instance.CreditsChanged += OnCreditsChanged;
        GameState.Instance.DayPassed += OnDayPassed;

        RefreshHUD();
    }

    public override void _Process(double delta)
    {
        _pulse += (float)delta;
        QueueRedraw();
    }

    // ── Drawing ────────────────────────────────────────────────────────────────
    public override void _Draw()
    {
        var gs = GameState.Instance;
        if (gs?.Galaxy == null) return;

        DrawBackground();
        DrawHyperlanes(gs);
        DrawSystems(gs);
        DrawPlayerIndicator(gs);
    }

    private void DrawBackground()
    {
        // Subtle ambient star field via cheap noise — 200 fixed dots
        var rng = new Random(999);
        for (int i = 0; i < 200; i++)
        {
            var pos = new Vector2(
                (float)(rng.NextDouble() * 2400 - 1200),
                (float)(rng.NextDouble() * 2000 - 1000));
            float brightness = (float)(rng.NextDouble() * 0.25 + 0.05);
            DrawCircle(pos, (float)(rng.NextDouble() * 1.2 + 0.3), new Color(1f, 1f, 1f, brightness));
        }
    }

    private void DrawHyperlanes(GameState gs)
    {
        foreach (var sys in gs.Galaxy)
        {
            foreach (var neighborId in sys.ConnectedSystemIds)
            {
                if (neighborId <= sys.Id) continue; // draw once per pair
                var neighbor = gs.GetSystem(neighborId);
                if (neighbor == null) continue;
                DrawLine(sys.MapPosition, neighbor.MapPosition, HyperlaneColor, 1f, true);
            }
        }
    }

    private void DrawSystems(GameState gs)
    {
        foreach (var sys in gs.Galaxy)
        {
            var faction = gs.GetFaction(sys.FactionId);
            var starColor = StarColor(sys.StarType);

            // Faction territory glow
            if (faction != null && faction.Id != "independent")
            {
                var glow = new Color(faction.Color.R, faction.Color.G, faction.Color.B, 0.25f);
                DrawCircle(sys.MapPosition, GlowRadius, glow);
            }

            // Hover ring
            if (sys == _hovered && sys != _selected)
                DrawArc(sys.MapPosition, StarRadius + 6f, 0, Mathf.Tau, 24,
                    new Color(1f, 1f, 1f, 0.35f), 1.2f);

            // Selected ring
            if (sys == _selected)
                DrawArc(sys.MapPosition, StarRadius + 8f, 0, Mathf.Tau, 32,
                    SelectedRingColor, 1.5f);

            // Reachable indicator: dashed-ish blue ring
            if (_selected != null && gs.CurrentSystem == _selected)
            {
                /* skip — handled by jump button */
            }

            // Star disc
            DrawCircle(sys.MapPosition, StarRadius, starColor);

            // Station dot
            if (sys.Stations.Count > 0)
                DrawCircle(sys.MapPosition + new Vector2(StarRadius + 3f, 0), 2.5f,
                    new Color(0.9f, 0.9f, 0.6f, 0.8f));

            // Name label
            DrawString(ThemeDB.FallbackFont,
                sys.MapPosition + new Vector2(StarRadius + 4f, 3f),
                sys.Name,
                HorizontalAlignment.Left, -1, 10, TextColor);
        }
    }

    private void DrawPlayerIndicator(GameState gs)
    {
        if (gs.CurrentSystem == null) return;
        float wave = (Mathf.Sin(_pulse * 2.8f) + 1f) * 0.5f;
        float r = StarRadius + 10f + wave * 5f;
        float alpha = 0.5f + wave * 0.5f;
        DrawArc(gs.CurrentSystem.MapPosition, r, 0, Mathf.Tau, 36,
            new Color(PlayerRingColor.R, PlayerRingColor.G, PlayerRingColor.B, alpha), 2f);
    }

    // ── Input ──────────────────────────────────────────────────────────────────
    public override void _UnhandledInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton mb:
                HandleMouseButton(mb);
                break;
            case InputEventMouseMotion mm:
                HandleMouseMotion(mm);
                break;
        }
    }

    private void HandleMouseButton(InputEventMouseButton mb)
    {
        if (mb.ButtonIndex == MouseButton.WheelUp)
            ZoomCamera(1.12f);
        else if (mb.ButtonIndex == MouseButton.WheelDown)
            ZoomCamera(1f / 1.12f);
        else if (mb.ButtonIndex == MouseButton.Right)
        {
            _isDragging = mb.Pressed;
            if (_isDragging)
            {
                _dragOriginScreen = mb.Position;
                _cameraOriginPos  = _camera.Position;
            }
        }
        else if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            TrySelect(GetGlobalMousePosition());
        }
    }

    private void HandleMouseMotion(InputEventMouseMotion mm)
    {
        if (_isDragging)
        {
            var delta = (mm.Position - _dragOriginScreen) / _camera.Zoom;
            _camera.Position = _cameraOriginPos - delta;
        }
        _hovered = NearestSystem(GetGlobalMousePosition(), ClickThreshold * 1.5f);
    }

    private void ZoomCamera(float factor)
    {
        float x = Mathf.Clamp(_camera.Zoom.X * factor, 0.25f, 3.5f);
        _camera.Zoom = new Vector2(x, x);
    }

    private void TrySelect(Vector2 worldPos)
    {
        var sys = NearestSystem(worldPos, ClickThreshold);
        _selected = sys;
        if (sys != null) ShowInfo(sys);
        else             HideInfo();
    }

    private StarSystem? NearestSystem(Vector2 worldPos, float threshold)
    {
        var gs = GameState.Instance;
        StarSystem? best = null;
        float bestDist = threshold;
        foreach (var sys in gs.Galaxy)
        {
            float d = worldPos.DistanceTo(sys.MapPosition);
            if (d < bestDist) { bestDist = d; best = sys; }
        }
        return best;
    }

    // ── Info Panel ─────────────────────────────────────────────────────────────
    private void ShowInfo(StarSystem sys)
    {
        var gs = GameState.Instance;
        var faction = gs.GetFaction(sys.FactionId);

        _lblSystemName.Text = sys.Name;
        _lblStarType.Text   = $"Star:     {sys.StarType}";
        _lblFaction.Text    = $"Faction:  {faction?.Name ?? "Independent"}";
        _lblPlanets.Text    = $"Planets:  {sys.PlanetCount}";
        _lblSecurity.Text   = $"Security: {sys.Security}";

        bool reachable     = gs.CanJumpTo(sys);
        bool isCurrent     = sys == gs.CurrentSystem;
        _btnJump.Text      = isCurrent ? "[ current location ]" : reachable ? "▶  Jump Here" : "✕  Not in range";
        _btnJump.Disabled  = isCurrent || !reachable;
        _btnEnter.Disabled = !isCurrent;

        _infoPanel.Visible = true;
    }

    private void HideInfo() => _infoPanel.Visible = false;

    private void OnJumpPressed()
    {
        if (_selected == null) return;
        GameState.Instance.JumpToSystem(_selected);
        ShowInfo(_selected);
    }

    private void OnEnterPressed()
    {
        GetTree().Root.GetNode<Main>("Main").LoadScene(SystemViewScene);
    }

    // ── HUD refresh ────────────────────────────────────────────────────────────
    private void RefreshHUD()
    {
        var gs = GameState.Instance;
        _lblCredits.Text  = $"Credits: {gs.PlayerShip.Credits:N0}";
        _lblDay.Text      = $"Day {gs.Day}";
        _lblLocation.Text = $"Location: {gs.CurrentSystem?.Name ?? "Unknown"}";
    }

    private void OnSystemChanged(int _id)
    {
        if (_selected != null) ShowInfo(_selected);
        RefreshHUD();
    }

    private void OnCreditsChanged(long _amount) => RefreshHUD();
    private void OnDayPassed(int _day) => RefreshHUD();

    // ── Helpers ────────────────────────────────────────────────────────────────
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
