# CLAUDE.md — Project Emptiness

Claude-spezifische Anweisungen. Diese Datei wird zu Beginn jeder Session gelesen.
Für Roadmap, Update Log und Current State → README.md.

---

## Kern-Vision

2D Top-Down Space Sandbox. Mit einem Schiff starten, Handel treiben, kämpfen, Imperium aufbauen, eigene Fraktion gründen. Lebendiges Universum mit Wirtschaft und Fraktions-KI.

**Referenzspiele:** Starsector (Kampf, Stil), The Last Starship (UI, Innenraum), X4 Foundations (Wirtschaft, Empire)

---

## Tech Stack

| Bereich | Technologie |
|---|---|
| Engine | **Godot 4.6.2 .NET** |
| Sprache | **C#** (.NET 8) |
| Namespace | `ProjectEmptiness` |
| Nullable | `#nullable enable` per-File, nie via .csproj |
| Platform | Windows |

**Godot-Gotchas:**
- `.csproj` wird von Godot verwaltet → nie manuell ersetzen
- `#nullable enable` per-File setzen (nicht global in .csproj)
- `.tscn` Format 3 ohne UIDs (UIDs verursachten "format too new" Fehler)
- `Dictionary<K,V>.GetValueOrDefault()` existiert nicht → explizites `ContainsKey` verwenden

---

## Architektur

```
Rendering Layer     → Godot Scenes (.tscn + .cs Node-Scripts)
Simulation Layer    → Pure C# Klassen, tick-basiert (kein Godot-Inheritance)
Data Layer          → JSON-Dateien in /data/, geladen beim Start
```

**Autoloads (Singletons):**
- `GameState` (`src/Core/GameState.cs`) — zentraler Zustand: Galaxie, Spieler, Fraktionen, Signals
- `SimulationManager` (`src/Simulation/SimulationManager.cs`) — Tick-Engine (1 Tag = 24 Sek)

**Szenen-Modi:**
| Szene | Status | Beschreibung |
|---|---|---|
| `GalaxyMap` | ✅ Fertig | Galaxie-Übersicht, Pan/Zoom, Navigation, Info-Panel, HUD |
| `SystemView` | ✅ Fertig | Planeten + Stationen in einem System, animiert, klickbar |
| `StationTrade` | ✅ Fertig | Handelscreen: kaufen/verkaufen, Preise, Lager, Live-Update |
| `Combat` | 🔲 Next | Echtzeit-Kampf (Starsector-Stil), Waffen, Schilde, Flux |
| `PlanetView` | 🔲 Geplant | Planet-Landung, Interaktion (Gameplay TBD) |
| `StationInterior` | 🔲 Später | Schiffs-Innenraum (The Last Starship Querschnitt) |

---

## Datei-Struktur

```
Project Emptiness/
├── project.godot
├── Project Emptiness.csproj   ← Godot-generiert, nicht ersetzen
├── scenes/
│   ├── Main/                  ← Entry Point (lädt erste Szene)
│   ├── GalaxyMap/             ← ✅ Fertig
│   ├── SystemView/            ← ✅ Fertig
│   ├── StationTrade/          ← ✅ Fertig
│   ├── Combat/                ← 🔲 Next
│   └── StationInterior/       ← 🔲 Später
├── src/
│   ├── Core/GameState.cs      ← Singleton, Signals, Spielzustand
│   ├── Data/
│   │   ├── Enums.cs           ← StarType, ShipClass, FactionStance, PlanetType, ...
│   │   ├── StarSystem.cs      ← StarSystem, Planet, Station
│   │   ├── Faction.cs         ← Faction + Reputationslogik
│   │   ├── Ship.cs            ← PlayerShip, ShipTemplate
│   │   └── TradeGood.cs
│   ├── Generation/
│   │   └── GalaxyGenerator.cs ← 64 Systeme, Planeten, Stationen, Fraktionen
│   └── Simulation/
│       └── SimulationManager.cs ← Wirtschaft + Diplomatie-Drift
├── assets/
│   ├── FOR_ARAS.md            ← Asset-Brief für Aras (Specs, Dateinamen, Format)
│   ├── concepts/              ← KI-generierte Konzept-Referenzen (nicht final)
│   ├── planets/               ← ✅ barren, terran, ice — 🔲 desert, ocean, volcanic, gasgiant, toxic
│   ├── ships/                 ← ✅ freighter, frigate, destroyer — 🔲 shuttle, cruiser, battlecruiser, carrier, dreadnought
│   └── backgrounds/           ← ✅ bg-nebula-blue, bg-nebula-warm, bg-deep-space — 🔲 bg-combat
└── data/
    ├── factions.json           ← 5 Fraktionen
    └── goods.json              ← 10 Handelswaren
```

---

## Coding-Konventionen

- **Namespace:** Immer `ProjectEmptiness.XYZ`
- **Node-Scripts:** `partial class` extends Godot-Typ
- **Simulation-Klassen:** Pure C#, kein Godot-Inheritance
- **Signals:** In `GameState` zentralisiert, in `_ExitTree()` wieder abmelden
- **Kein GDScript** — alles C#
- **Keine Kommentare** außer bei nicht-offensichtlichem Verhalten
- **JSON** für alle Spieldaten

---

## Asset-Pipeline

Sprites werden von **Aras** (GPT Image 2) generiert. Alle Sprites sind RGBA 1024×1024, Hintergründe opaque.

| Status | Beschreibung |
|---|---|
| ✅ In Betrieb | `bg-nebula-blue.png` — Haupthintergrund in Galaxy Map + System View |
| ✅ In Betrieb | `bg-nebula-warm.png`, `bg-deep-space.png` — geliefert, noch nicht per System zugewiesen |
| ✅ In Betrieb | `ship-freighter.png` — Spielerschiff in System View, kein Shader |
| ✅ In Betrieb | `planet-barren.png`, `planet-terran.png`, `planet-ice.png` — live, auto-geladen nach PlanetType |
| ⚠️ Fallback | `planet-desert/ocean/volcanic/gasgiant/toxic` → zeigen barren-Sprite bis Aras liefert |
| 🔲 Ausstehend | `ship-shuttle/cruiser/battlecruiser/carrier/dreadnought.png` |
| 🔲 Ausstehend | `bg-combat.png` (erst für Day 4-5 Combat benötigt) |
| 🔲 Ausstehend | Alpha-Kanten verfeinern (leichter dunkler Rand sichtbar, Aras-Feedback) |

**Wenn Aras neue Assets liefert:**
1. Dateien in `assets/planets/`, `assets/ships/`, `assets/backgrounds/` ablegen (exakte Dateinamen lt. Brief)
2. Kein Code-Umbau nötig — `GetPlanetPath()` in `SystemView.cs` lädt automatisch per `PlanetType`
3. `ResourceLoader.Exists()` Fallback auf `planet-barren.png` ist bereits aktiv

---

## Design-Entscheidungen

| Thema | Entscheidung | Begründung |
|---|---|---|
| Engine | Godot 4.6 + C# | Claude schreibt alles, Manuel testet |
| Wirtschaft | Abstrahiert (Formel-basiert) | X4-Feeling ohne vollständige Simulation |
| KI-Simulation | Tick-basiert (1 Tag = 24 Sek) | Performance, kein Frame-Tracking |
| 3D | Nein, komplett 2D | Top-Down |
| Grafik-Timing | Erst Mechaniken, dann Sprites | Sprites von Aras (GPT Image 2), parallel zu Mechaniken |
| Innenraum | The Last Starship Querschnitt-Stil | Monat 3, nach Kampf + Handel |
| Planet Landing | Geplant für Monat 2 (Tag 15) | Gameplay-Scope noch TBD |
| **Echtes Fliegen** | **Geplant — nach Core-Loop** | SystemView soll echte Schiffsbewegung bekommen (WASD/Physik auf der System-Map mit Planeten). Aktuell nur Klick-Menü — das ist Placeholder. Kampf später auch direkt in der System-Map möglich. |

---

## Visueller Stil

- **Hintergrund:** `Color(0.02, 0.02, 0.055)` — fast schwarz, Blaustich
- **Sternfarben:** Yellow `#FFD938`, Orange `#FF8C1F`, Red `#EB3A2E`, Blue `#4794FF`, White `#EDF0FF`, Neutron `#AE47FF`
- **Planeten:** RGBA transparent PNGs von Aras (1024×1024), barren/terran/ice live — Rest Fallback auf barren
- **Fraktionsfarben:** Terran `#4577FF`, Syndicate `#FF4D26`, Void `#A640FF`, Alliance `#33E573`
- **UI:** Dunkle Panels, helle Labels, Sci-Fi-minimalistisch
- **Font:** Godot FallbackFont jetzt → Sci-Fi Monospace Font in Monat 3
