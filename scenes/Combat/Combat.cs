#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using ProjectEmptiness.Core;

namespace ProjectEmptiness.Scenes;

public partial class Combat : Node2D
{
    private static readonly PackedScene SystemViewScene =
        GD.Load<PackedScene>("res://scenes/SystemView/SystemView.tscn");

    // ── Tuning ─────────────────────────────────────────────────────────────────
    private const float ShipScale      = 0.06f;
    private const float PlayerAccel    = 620f;
    private const float PlayerDrag     = 3.5f;
    private const float FireCooldown   = 0.22f;
    private const float FluxPerShot    = 8f;
    private const float FluxDissipate  = 38f;
    private const float FluxPerHit     = 22f;
    private const float ShieldRegen    = 7f;
    private const float OverloadTime   = 3f;

    private const float EnemySpeed     = 145f;
    private const float EnemyDrag      = 3.5f;
    private const float EnemyRotSpeed  = 1.9f;
    private const float EnemyPreferDist = 235f;
    private const float EnemyFireRate  = 1.15f;

    private const float ProjSpeed      = 460f;
    private const float ProjDamage     = 35f;
    private const float ProjMaxAge     = 2.3f;
    private const float HitRadius      = 28f;
    private const float ProjDrawRadius = 4f;

    private const float VictoryLoot    = 2_000f;
    private const float DeathPenalty   = 0.25f;

    // ── Nodes ──────────────────────────────────────────────────────────────────
    private Sprite2D _playerSprite = null!;
    private Sprite2D _enemySprite  = null!;
    private Camera2D _camera       = null!;

    // ── Player state ───────────────────────────────────────────────────────────
    private Vector2 _playerVel;
    private float   _playerAngle;
    private float   _pHull, _pMaxHull;
    private float   _pShields, _pMaxShields;
    private float   _pFlux, _pMaxFlux;
    private bool    _overloaded;
    private float   _overloadTimer;
    private float   _fireCooldown;

    // ── Enemy state ────────────────────────────────────────────────────────────
    private Vector2 _enemyVel;
    private float   _enemyAngle;
    private float   _eHull, _eMaxHull;
    private float   _eShields, _eMaxShields;
    private float   _eFireCooldown;

    // ── Projectiles ────────────────────────────────────────────────────────────
    private class Proj { public Vector2 Pos; public Vector2 Dir; public float Age; }
    private readonly List<Proj> _playerProjs = new();
    private readonly List<Proj> _enemyProjs  = new();

    // ── HUD ────────────────────────────────────────────────────────────────────
    private ProgressBar  _pHullBar = null!, _pShieldBar = null!, _pFluxBar = null!;
    private ProgressBar  _eHullBar = null!, _eShieldBar = null!;
    private Control      _resultPanel  = null!;
    private Label        _resultTitle  = null!;
    private Label        _resultDetails = null!;
    private StyleBoxFlat _fluxStyle    = null!;

    private bool _combatEnded;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    public override void _Ready()
    {
        GetNode<TextureRect>("Background/BgRect").Texture =
            GD.Load<Texture2D>("res://assets/backgrounds/bg-nebula-blue.png");

        _camera       = GetNode<Camera2D>("Camera2D");
        _playerSprite = GetNode<Sprite2D>("PlayerShip/ShipSprite");
        _enemySprite  = GetNode<Sprite2D>("EnemyShip/ShipSprite");

        _playerSprite.Texture = GD.Load<Texture2D>("res://assets/ships/ship-freighter.png");
        _playerSprite.Scale   = new Vector2(ShipScale, ShipScale);
        _enemySprite.Texture  = GD.Load<Texture2D>("res://assets/ships/ship-frigate.png");
        _enemySprite.Scale    = new Vector2(ShipScale, ShipScale);

        _playerSprite.Position = new Vector2(0, 300);
        _enemySprite.Position  = new Vector2(0, -300);
        _playerAngle = 0f;
        _enemyAngle  = MathF.PI;

        var ship = GameState.Instance.PlayerShip;
        _pMaxHull    = ship.MaxHull;
        _pHull       = ship.CurrentHull;
        _pMaxShields = ship.MaxShields;
        _pShields    = ship.CurrentShields;
        _pMaxFlux    = ship.MaxFlux;
        _pFlux       = 0f;

        _eMaxHull    = 600f;
        _eHull       = 600f;
        _eMaxShields = 300f;
        _eShields    = 300f;

        // HUD
        _pHullBar    = GetNode<ProgressBar>("UI/HUD/PlayerHUD/PVBox/PHullRow/PHullBar");
        _pShieldBar  = GetNode<ProgressBar>("UI/HUD/PlayerHUD/PVBox/PShieldRow/PShieldBar");
        _pFluxBar    = GetNode<ProgressBar>("UI/HUD/PlayerHUD/PVBox/PFluxRow/PFluxBar");
        _eHullBar    = GetNode<ProgressBar>("UI/HUD/EnemyHUD/EVBox/EHullRow/EHullBar");
        _eShieldBar  = GetNode<ProgressBar>("UI/HUD/EnemyHUD/EVBox/EShieldRow/EShieldBar");
        _resultPanel = GetNode<Control>("UI/HUD/ResultPanel");
        _resultTitle = GetNode<Label>("UI/HUD/ResultPanel/RVBox/ResultTitle");
        _resultDetails = GetNode<Label>("UI/HUD/ResultPanel/RVBox/ResultDetails");

        GetNode<Label>("UI/HUD/PlayerHUD/PVBox/PName").Text = ship.Name.ToUpper();
        GetNode<Button>("UI/HUD/ResultPanel/RVBox/BackBtn").Pressed += OnBackPressed;

        PaintBar(_pHullBar,   new Color(0.2f,  0.85f, 0.3f));
        PaintBar(_pShieldBar, new Color(0.25f, 0.55f, 1f));
        PaintBar(_eHullBar,   new Color(0.9f,  0.25f, 0.2f));
        PaintBar(_eShieldBar, new Color(0.6f,  0.3f,  1f));
        _fluxStyle = PaintBar(_pFluxBar, new Color(0.15f, 0.85f, 0.85f));

        RefreshHUD();
    }

    // ── Per-frame ──────────────────────────────────────────────────────────────
    public override void _Process(double delta)
    {
        if (_combatEnded) return;
        float dt = (float)delta;

        HandlePlayerInput(dt);
        TickEnemyAI(dt);
        TickProjectiles(dt);
        TickFlux(dt);

        _camera.Position = _playerSprite.Position;
        RefreshHUD();
        QueueRedraw();
    }

    // ── Player input ───────────────────────────────────────────────────────────
    private void HandlePlayerInput(float dt)
    {
        Vector2 toMouse = GetGlobalMousePosition() - _playerSprite.GlobalPosition;
        if (toMouse.LengthSquared() > 1f)
            _playerAngle = MathF.Atan2(toMouse.X, -toMouse.Y);
        _playerSprite.Rotation = _playerAngle - MathF.PI / 2f;

        // Movement relative to ship facing: W=forward, S=back, A/D=strafe
        Vector2 fwd   = FwdVec(_playerAngle);
        Vector2 right = new Vector2(fwd.Y, -fwd.X);
        var move = Vector2.Zero;
        if (Input.IsKeyPressed(Key.W)) move += fwd;
        if (Input.IsKeyPressed(Key.S)) move -= fwd;
        if (Input.IsKeyPressed(Key.A)) move -= right;
        if (Input.IsKeyPressed(Key.D)) move += right;
        if (move.LengthSquared() > 0) move = move.Normalized();

        _playerVel += move * PlayerAccel * dt;
        _playerVel *= MathF.Max(0f, 1f - PlayerDrag * dt);
        _playerSprite.Position += _playerVel * dt;

        _fireCooldown = MathF.Max(0f, _fireCooldown - dt);
        if (Input.IsMouseButtonPressed(MouseButton.Left) && _fireCooldown <= 0 && !_overloaded)
        {
            _playerProjs.Add(new Proj { Pos = _playerSprite.GlobalPosition, Dir = FwdVec(_playerAngle) });
            _pFlux += FluxPerShot;
            _fireCooldown = FireCooldown;
        }
    }

    // ── Enemy AI ───────────────────────────────────────────────────────────────
    private void TickEnemyAI(float dt)
    {
        Vector2 toPlayer = _playerSprite.Position - _enemySprite.Position;
        float   dist     = toPlayer.Length();
        float   targetAngle = dist > 0.5f ? MathF.Atan2(toPlayer.X, -toPlayer.Y) : _enemyAngle;

        float diff = AngleDiff(_enemyAngle, targetAngle);
        _enemyAngle += MathF.Sign(diff) * MathF.Min(MathF.Abs(diff), EnemyRotSpeed * dt);

        float approachFactor = (dist - EnemyPreferDist) / EnemyPreferDist;
        _enemyVel += FwdVec(_enemyAngle) * approachFactor * EnemySpeed * dt;
        _enemyVel *= MathF.Max(0f, 1f - EnemyDrag * dt);
        _enemySprite.Position += _enemyVel * dt;
        _enemySprite.Rotation  = _enemyAngle - MathF.PI / 2f;

        _eFireCooldown = MathF.Max(0f, _eFireCooldown - dt);
        if (_eFireCooldown <= 0 && MathF.Abs(diff) < 0.18f)
        {
            _enemyProjs.Add(new Proj { Pos = _enemySprite.GlobalPosition, Dir = FwdVec(_enemyAngle) });
            _eFireCooldown = EnemyFireRate;
        }
    }

    // ── Projectiles ────────────────────────────────────────────────────────────
    private void TickProjectiles(float dt)
    {
        AdvanceProjs(_playerProjs, _enemySprite.Position,  DealDamageToEnemy,  dt);
        AdvanceProjs(_enemyProjs,  _playerSprite.Position, DealDamageToPlayer, dt);
    }

    private static void AdvanceProjs(List<Proj> projs, Vector2 target, Action<float> onHit, float dt)
    {
        for (int i = projs.Count - 1; i >= 0; i--)
        {
            var p = projs[i];
            p.Pos += p.Dir * ProjSpeed * dt;
            p.Age += dt;
            bool hit = p.Pos.DistanceTo(target) < HitRadius;
            if (hit) onHit(ProjDamage);
            if (hit || p.Age > ProjMaxAge) projs.RemoveAt(i);
        }
    }

    // ── Damage ─────────────────────────────────────────────────────────────────
    private void DealDamageToPlayer(float dmg)
    {
        if (_combatEnded) return;
        if (_pShields > 0)
        {
            float absorb = MathF.Min(_pShields, dmg);
            _pShields -= absorb;
            dmg       -= absorb;
            _pFlux    += FluxPerHit;
        }
        _pHull -= dmg;
        if (_pHull <= 0) OnPlayerDied();
    }

    private void DealDamageToEnemy(float dmg)
    {
        if (_combatEnded) return;
        if (_eShields > 0)
        {
            float absorb = MathF.Min(_eShields, dmg);
            _eShields -= absorb;
            dmg       -= absorb;
        }
        _eHull -= dmg;
        if (_eHull <= 0) OnEnemyDied();
    }

    // ── Flux ───────────────────────────────────────────────────────────────────
    private void TickFlux(float dt)
    {
        if (_overloaded)
        {
            _overloadTimer -= dt;
            if (_overloadTimer <= 0) { _overloaded = false; _pFlux = 0; }
        }
        else
        {
            _pFlux = MathF.Max(0f, _pFlux - FluxDissipate * dt);
            if (_pFlux >= _pMaxFlux) { _overloaded = true; _overloadTimer = OverloadTime; _pShields = 0; }
        }

        if (!_overloaded && _pFlux < _pMaxFlux * 0.4f)
            _pShields = MathF.Min(_pMaxShields, _pShields + ShieldRegen * dt);
    }

    // ── Win / Lose ─────────────────────────────────────────────────────────────
    private void OnEnemyDied()
    {
        _combatEnded = true;
        _eHull = 0;
        long loot = (long)VictoryLoot;
        GameState.Instance.AddCredits(loot);
        GameState.Instance.PlayerShip.CurrentHull    = MathF.Max(1f, _pHull);
        GameState.Instance.PlayerShip.CurrentShields = _pShields;
        _resultTitle.Text   = "VICTORY";
        _resultDetails.Text = $"Enemy destroyed.\n+{loot:N0} credits salvaged.";
        _resultPanel.Visible = true;
    }

    private void OnPlayerDied()
    {
        _combatEnded = true;
        _pHull = 0;
        long penalty = (long)(GameState.Instance.PlayerShip.Credits * DeathPenalty);
        GameState.Instance.AddCredits(-penalty);
        GameState.Instance.PlayerShip.CurrentHull    = GameState.Instance.PlayerShip.MaxHull * 0.25f;
        GameState.Instance.PlayerShip.CurrentShields = 0;
        _resultTitle.Text   = "DESTROYED";
        _resultDetails.Text = $"Ship lost.\n-{penalty:N0} credits in damages.";
        _resultPanel.Visible = true;
    }

    private void OnBackPressed() =>
        GetTree().Root.GetNode<Main>("Main").LoadScene(SystemViewScene);

    // ── Drawing ────────────────────────────────────────────────────────────────
    public override void _Draw()
    {
        foreach (var p in _playerProjs)
            DrawCircle(p.Pos, ProjDrawRadius, new Color(0.4f, 0.9f, 1f, 0.9f));
        foreach (var p in _enemyProjs)
            DrawCircle(p.Pos, ProjDrawRadius, new Color(1f, 0.45f, 0.1f, 0.9f));

        if (_overloaded && (_overloadTimer % 0.4f) < 0.2f)
            DrawCircle(_playerSprite.Position, 34f, new Color(1f, 0.3f, 0f, 0.3f));
    }

    // ── HUD update ─────────────────────────────────────────────────────────────
    private void RefreshHUD()
    {
        _pHullBar.Value   = _pHull    / _pMaxHull    * 100.0;
        _pShieldBar.Value = _pShields / _pMaxShields * 100.0;
        _pFluxBar.Value   = _pFlux    / _pMaxFlux    * 100.0;
        _eHullBar.Value   = _eHull    / _eMaxHull    * 100.0;
        _eShieldBar.Value = _eShields / _eMaxShields * 100.0;

        float ratio = _pFlux / _pMaxFlux;
        _fluxStyle.BgColor = ratio > 0.75f ? new Color(1f, 0.35f, 0.1f) :
                             ratio > 0.5f  ? new Color(1f, 0.8f,  0.1f) :
                                             new Color(0.15f, 0.85f, 0.85f);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private static Vector2 FwdVec(float angle) => new(MathF.Sin(angle), -MathF.Cos(angle));

    private static float AngleDiff(float from, float to)
    {
        float d = (to - from) % (MathF.PI * 2f);
        if (d >  MathF.PI) d -= MathF.PI * 2f;
        if (d < -MathF.PI) d += MathF.PI * 2f;
        return d;
    }

    private static StyleBoxFlat PaintBar(ProgressBar bar, Color color)
    {
        var style = new StyleBoxFlat
        {
            BgColor = color,
            CornerRadiusTopLeft     = 2,
            CornerRadiusTopRight    = 2,
            CornerRadiusBottomLeft  = 2,
            CornerRadiusBottomRight = 2,
        };
        bar.AddThemeStyleboxOverride("fill", style);
        return style;
    }
}
