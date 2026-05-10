#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectEmptiness.Core;
using ProjectEmptiness.Data;

namespace ProjectEmptiness.Scenes;

public partial class StationTrade : Node2D
{
    private static readonly PackedScene SystemViewScene =
        GD.Load<PackedScene>("res://scenes/SystemView/SystemView.tscn");

    private static readonly Dictionary<string, string> GoodNames = new()
    {
        ["food"]        = "Food Supplies",
        ["metal"]       = "Refined Metals",
        ["fuel"]        = "Hyperium Fuel",
        ["machinery"]   = "Industrial Machinery",
        ["electronics"] = "Electronics",
        ["luxuries"]    = "Luxury Goods",
        ["weapons"]     = "Weapons Cache",
        ["narcotics"]   = "Stimulants",
        ["ore"]         = "Raw Ore",
        ["medical"]     = "Medical Supplies",
    };

    private static readonly Color DimColor   = new(0.55f, 0.55f, 0.6f);
    private static readonly Color ValueColor = new(0.7f, 0.95f, 0.7f);

    private Label         _lblStation  = null!;
    private Label         _lblCargo    = null!;
    private Label         _lblCredits  = null!;
    private VBoxContainer _buyList     = null!;
    private VBoxContainer _sellList    = null!;
    private Station       _station     = null!;

    public override void _Ready()
    {
        _station = GameState.Instance.CurrentStation!;

        GetNode<TextureRect>("Background/BgRect").Texture =
            GD.Load<Texture2D>("res://assets/backgrounds/bg-nebula-blue.png");

        _lblStation = GetNode<Label>("UI/Root/TopBar/HBox/StationTitle");
        _lblCargo   = GetNode<Label>("UI/Root/TopBar/HBox/CargoLabel");
        _lblCredits = GetNode<Label>("UI/Root/TopBar/HBox/CreditsLabel");
        _buyList    = GetNode<VBoxContainer>("UI/Root/Body/BuyPanel/BuyVBox/BuyScroll/BuyList");
        _sellList   = GetNode<VBoxContainer>("UI/Root/Body/SellPanel/SellVBox/SellScroll/SellList");

        var faction = GameState.Instance.GetFaction(_station.FactionId);
        _lblStation.Text = $"{_station.Name}  —  {faction?.Name ?? "Independent"}";

        GetNode<Button>("UI/Root/TopBar/HBox/BackBtn").Pressed += OnBackPressed;

        GameState.Instance.CreditsChanged += OnCreditsChanged;
        GameState.Instance.CargoChanged   += OnCargoChanged;

        RefreshAll();
    }

    public override void _ExitTree()
    {
        GameState.Instance.CreditsChanged -= OnCreditsChanged;
        GameState.Instance.CargoChanged   -= OnCargoChanged;
    }

    private void OnCreditsChanged(long _) => RefreshHUD();
    private void OnCargoChanged()         => RefreshAll();

    private void RefreshAll()
    {
        RefreshHUD();
        BuildBuyList();
        BuildSellList();
    }

    private void RefreshHUD()
    {
        var ship = GameState.Instance.PlayerShip;
        _lblCredits.Text = $"Credits: {ship.Credits:N0}";
        _lblCargo.Text   = $"Cargo: {ship.UsedCargoSpace} / {ship.CargoCapacity}";
    }

    // ── Buy panel ──────────────────────────────────────────────────────────────
    private void BuildBuyList()
    {
        foreach (Node child in _buyList.GetChildren()) child.QueueFree();

        var ship = GameState.Instance.PlayerShip;

        foreach (var kv in _station.Stock.OrderBy(kv => kv.Key))
        {
            string id    = kv.Key;
            int    stock = kv.Value;
            float  price = _station.Prices.ContainsKey(id) ? _station.Prices[id] : 0f;
            string name  = GoodNames.ContainsKey(id) ? GoodNames[id] : Capitalize(id);

            var row = MakeRow();

            row.AddChild(MakeLabel(name,          180, HorizontalAlignment.Left, expand: true));
            row.AddChild(MakeLabel(stock > 0 ? $"{stock} u" : "—", 56, HorizontalAlignment.Right));
            row.AddChild(MakeLabel($"{price:0} cr",               72, HorizontalAlignment.Right));

            if (stock > 0)
            {
                int  freeSpace = ship.CargoCapacity - ship.UsedCargoSpace;
                int  maxQty    = Math.Min(stock, freeSpace);
                bool canBuy1   = ship.Credits >= (long)MathF.Ceiling(price) && freeSpace >= 1;
                bool canBuyMax = maxQty > 0 && ship.Credits >= (long)MathF.Ceiling(price * maxQty);

                var btn1   = MakeButton("Buy 1",          canBuy1);
                var btnMax = MakeButton($"Max ({maxQty})", canBuyMax && maxQty > 1);

                // capture loop variable
                string capturedId  = id;
                int    capturedMax = maxQty;
                btn1.Pressed   += () => GameState.Instance.BuyGood(capturedId, 1);
                btnMax.Pressed += () => GameState.Instance.BuyGood(capturedId, capturedMax);

                row.AddChild(btn1);
                row.AddChild(btnMax);
            }
            else
            {
                row.AddChild(MakeLabel("out of stock", 140, HorizontalAlignment.Left, color: DimColor));
            }

            _buyList.AddChild(row);
        }
    }

    // ── Sell panel ─────────────────────────────────────────────────────────────
    private void BuildSellList()
    {
        foreach (Node child in _sellList.GetChildren()) child.QueueFree();

        var ship = GameState.Instance.PlayerShip;

        if (ship.Cargo.Count == 0)
        {
            _sellList.AddChild(MakeLabel("Cargo hold is empty.", -1, HorizontalAlignment.Left, color: DimColor));
            return;
        }

        foreach (var kv in ship.Cargo.OrderBy(kv => kv.Key))
        {
            string id    = kv.Key;
            int    qty   = kv.Value;
            float  price = _station.Prices.ContainsKey(id) ? _station.Prices[id] : 0f;
            string name  = GoodNames.ContainsKey(id) ? GoodNames[id] : Capitalize(id);
            long   total = (long)(price * qty);

            var row = MakeRow();

            row.AddChild(MakeLabel(name,              180, HorizontalAlignment.Left, expand: true));
            row.AddChild(MakeLabel($"{qty} u",          56, HorizontalAlignment.Right));
            row.AddChild(MakeLabel($"{price:0} cr",     72, HorizontalAlignment.Right));
            row.AddChild(MakeLabel($"= {total:N0}",     90, HorizontalAlignment.Right, color: ValueColor));

            bool canSell = price > 0;
            var  btn1    = MakeButton("Sell 1",         canSell);
            var  btnAll  = MakeButton($"Sell All ({qty})", canSell);

            string capturedId  = id;
            int    capturedQty = qty;
            btn1.Pressed   += () => GameState.Instance.SellGood(capturedId, 1);
            btnAll.Pressed += () => GameState.Instance.SellGood(capturedId, capturedQty);

            row.AddChild(btn1);
            row.AddChild(btnAll);

            _sellList.AddChild(row);
        }
    }

    private void OnBackPressed() =>
        GetTree().Root.GetNode<Main>("Main").LoadScene(SystemViewScene);

    // ── Helpers ────────────────────────────────────────────────────────────────
    private static HBoxContainer MakeRow()
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        return row;
    }

    private static Label MakeLabel(string text, float minWidth, HorizontalAlignment align,
        bool expand = false, Color? color = null)
    {
        var lbl = new Label
        {
            Text                = text,
            HorizontalAlignment = align,
            AutowrapMode        = TextServer.AutowrapMode.Off,
            ClipText            = true,
        };
        if (minWidth > 0)
            lbl.CustomMinimumSize = new Vector2(minWidth, 0);
        if (expand)
            lbl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        lbl.AddThemeFontSizeOverride("font_size", 12);
        if (color.HasValue)
            lbl.Modulate = color.Value;
        return lbl;
    }

    private static Button MakeButton(string text, bool enabled)
    {
        var btn = new Button { Text = text, Disabled = !enabled };
        btn.AddThemeFontSizeOverride("font_size", 12);
        return btn;
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
}
