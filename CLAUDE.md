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
| `PlanetView` | 🔲 Geplant | Planet-Landung, Interaktion (Gameplay TBD) |
| `StationTrade` | 🔲 Next | Handelscreen: kaufen/verkaufen, Preise, Lager |
| `Combat` | 🔲 Geplant | Echtzeit-Kampf (Starsector-Stil), Waffen, Schilde, Flux |
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
│   ├── StationTrade/          ← 🔲 Next
│   ├── Combat/                ← 🔲 Geplant
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

## Design-Entscheidungen

| Thema | Entscheidung | Begründung |
|---|---|---|
| Engine | Godot 4.6 + C# | Claude schreibt alles, Manuel testet |
| Wirtschaft | Abstrahiert (Formel-basiert) | X4-Feeling ohne vollständige Simulation |
| KI-Simulation | Tick-basiert (1 Tag = 24 Sek) | Performance, kein Frame-Tracking |
| 3D | Nein, komplett 2D | Top-Down |
| Grafik-Timing | Erst Mechaniken, dann Sprites | Sprites ab Monat 2 via Midjourney |
| Innenraum | The Last Starship Querschnitt-Stil | Monat 3, nach Kampf + Handel |
| Planet Landing | Geplant für Monat 2 (Tag 15) | Gameplay-Scope noch TBD |

---

## Visueller Stil

- **Hintergrund:** `Color(0.02, 0.02, 0.055)` — fast schwarz, Blaustich
- **Sternfarben:** Yellow `#FFD938`, Orange `#FF8C1F`, Red `#EB3A2E`, Blue `#4794FF`, White `#EDF0FF`, Neutron `#AE47FF`
- **Planetfarben:** Terran grün, Ocean blau, Desert braun, Ice weiß, Volcanic rot, GasGiant braun-orange, Barren grau, Toxic giftgrün
- **Fraktionsfarben:** Terran `#4577FF`, Syndicate `#FF4D26`, Void `#A640FF`, Alliance `#33E573`
- **UI:** Dunkle Panels, helle Labels, Sci-Fi-minimalistisch
- **Font:** Godot FallbackFont jetzt → Sci-Fi Monospace Font in Monat 3
