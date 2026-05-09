# CLAUDE.md — Project Emptiness

Projektspezifische Anweisungen für Claude. Diese Datei wird zu Beginn jeder Session gelesen.

---

## Was wir bauen

Ein **2D Top-Down Space Sandbox** — visuell inspiriert von Starsector + The Last Starship, gameplay-technisch nahe an X4 Foundations, aber in 2D.

**Kern-Vision:** Mit einem einzigen Schiff starten, Handel treiben, Fraktionen beitreten oder bekämpfen, eine eigene Fraktion gründen, und schließlich ein Imperium aus Schiffen, Stationen und Territorien aufbauen — alles in einem lebendigen Universum mit echter Wirtschaft und Fraktions-KI.

**Referenzspiele:**
- **Starsector** (Fractal Softworks) — Visueller Stil, Echtzeit-Kampf, Fraktions-System, Kolonie-Mechanik
- **The Last Starship** (Introversion) — Schiffs-Innenraum als Querschnitt, UI-Stil, Klarheit
- **X4 Foundations** (Egosoft) — Wirtschaftssystem, Empire Building, Diplomatie, Fraktions-KI

---

## Tech Stack

| Bereich | Technologie |
|---|---|
| Engine | **Godot 4.6.2 .NET** |
| Sprache | **C#** (.NET 8) für alle Simulation/Logik |
| Namespace | `ProjectEmptiness` |
| .csproj | `Project Emptiness.csproj` (von Godot generiert, nicht überschreiben) |
| Nullable | `#nullable enable` am Anfang jeder .cs Datei die Nullable-Annotations nutzt |
| Platform | Windows (später cross-platform via Godot Export) |

**Wichtig:** Das `.csproj` wird von Godot verwaltet — nie manuell ersetzen. Stattdessen `#nullable enable` per-File setzen.

---

## Architektur — Drei Schichten

```
Rendering Layer     → Godot Scenes (.tscn + .cs Node-Scripts)
Simulation Layer    → Pure C# Klassen, tick-basiert (kein Godot-Inheritance)
Data Layer          → JSON-Dateien in /data/, geladen beim Start
```

### Autoloads (Singletons)
- `GameState` (`src/Core/GameState.cs`) — zentraler Zustand: Galaxie, Spieler, Fraktionen
- `SimulationManager` (`src/Simulation/SimulationManager.cs`) — Tick-Engine (1 Tag = 24 Sek)

### Szenen-Modi
| Szene | Status | Beschreibung |
|---|---|---|
| `GalaxyMap` | ✅ Fertig | Galaxie-Übersicht, Pan/Zoom, Navigation, Info-Panel, HUD |
| `SystemView` | 🔲 Next | System-Ansicht: Planeten, Stationen, lokale Flotten |
| `StationTrade` | 🔲 Geplant | Handelscreen: kaufen/verkaufen, Preise, Lager |
| `Combat` | 🔲 Geplant | Echtzeit-Kampf (Starsector-Stil), Waffen, Schilde, Flux |
| `StationInterior` | 🔲 Später | Schiffs-Innenraum (The Last Starship Querschnitt) |

---

## Datei-Struktur

```
Project Emptiness/
├── project.godot
├── Project Emptiness.csproj   ← Godot-generiert, nicht ersetzen
├── scenes/
│   ├── Main/                  ← Entry Point
│   ├── GalaxyMap/             ← ✅ Fertig
│   ├── SystemView/            ← 🔲 Next
│   ├── Combat/                ← 🔲 Geplant
│   └── StationInterior/       ← 🔲 Später
├── src/
│   ├── Core/GameState.cs      ← Singleton, Signals, Spielzustand
│   ├── Data/
│   │   ├── Enums.cs           ← StarType, ShipClass, FactionStance, ...
│   │   ├── StarSystem.cs      ← StarSystem, Planet, Station
│   │   ├── Faction.cs         ← Faction + Reputationslogik
│   │   ├── Ship.cs            ← PlayerShip, ShipTemplate
│   │   └── TradeGood.cs
│   ├── Generation/
│   │   └── GalaxyGenerator.cs ← 64 Systeme, Spiral, MST, Flood-Fill
│   └── Simulation/
│       └── SimulationManager.cs ← Wirtschaft + Diplomatie-Drift
└── data/
    ├── factions.json           ← 5 Fraktionen
    └── goods.json              ← 10 Handelswaren
```

---

## Aktueller Stand (Tag 1 — 10. Mai 2026)

### Fertig implementiert
- **Galaxie-Generator** — 64 prozedurale Systeme, Spiral-Verteilung, MST-Konnektivität, Fraktionszuweisung per Flood-Fill
- **Fraktionen** — 5 Fraktionen (Terran, Syndicate, Void Collective, Free Alliance, Independent), Farben, Relations-Matrix, Reputationssystem
- **Galaxie-Karte** — Pan/Zoom, Klick-Auswahl, Info-Panel, Jump über Hyperlanes, pulsierender Spieler-Indikator
- **HUD** — Credits, Day-Counter, aktuelle Location
- **Wirtschafts-Tick** — Stationen mit Lagerbeständen + Preisschwankung (Angebot/Nachfrage)
- **Diplomatie-Grundlage** — Relations-Dictionary, langsame Drift per Tag
- **Hintergrund** — Schwarzer Weltraum, Sternfeld, Fraktions-Glow auf Systemen

### Bekannte TODOs
- System-Namen bei einigen Systemen zu lang → Label-Clipping
- Kein Save/Load System noch
- Kamera springt nicht smooth zu neuem System nach Jump

---

## Entwicklungs-Tempo

Claude schreibt den gesamten Code, Nutzer (Manuel) testet und gibt Feedback. Dadurch ist das Tempo deutlich schneller als bei Solo-Entwicklung:
- **1-2 Tage** pro Feature statt 1-2 Wochen
- Gameplay-Systeme: in ~2 Monaten vollständig
- Grafik-Pass: ab Monat 2, wenn Mechaniken stabil sind

---

## Zeitplan (tagesbasiert)

### Monat 1 — Gameplay-Fundament
```
Tag 1   ✅ Galaxy Map + Navigation + Wirtschafts-Tick + Fraktionen
Tag 2      System View (Planeten + Stationen in einem System)
Tag 3      Stations-Handelscreen (kaufen/verkaufen)
Tag 4-5    Basis-Kampf (Echtzeit, Platzhalter-Schiffe)
Tag 6      Flotte — 2-5 Schiffe kaufen + befehligen
Tag 7      Fraktions-Reputation (Konsequenzen: Preise, Feindseligkeit)
Tag 8      Missions-System (Handelsrouten, Kopfgelder, Eskorten)
Tag 9      Station kaufen/errichten
Tag 10     Diplomatie-Screen (Allianzen, Verträge, Tribute)
```

### Monat 2 — Empire & KI
```
Tag 11-12  Fraktions-KI (baut, handelt, erklärt Kriege autonom)
Tag 13     Eigene Fraktion gründen
Tag 14     Empire-Übersicht (eigene Systeme, Einnahmen, Flotten)
Tag 15     Erster Grafik-Pass — AI-Sprites für Schiffe (Midjourney)
Tag 16-17  Waffen-Effekte, Explosionen (Shader + Partikel, kein Sprite nötig)
Tag 18     Schiffs-Upgrade-System
Tag 19-20  Kampf-Polish (Flux-System, Schilde, Taktik)
```

### Monat 3 — Content & Polish
```
Tag 21-22  Mehr Schiffsklassen (8+), mehr Waren, Events
Tag 23     Sound + Musik-Integration
Tag 24     Schiffs-Innenraum (The Last Starship Stil)
Tag 25-26  UI/UX finaler Pass, Sci-Fi Font
Tag 27-28  Save/Load System
Tag 29-30  Balance + Beta-Vorbereitung
```

---

## Grafik-Strategie

**Phase 1 (Monat 1):** Nur Platzhalter — geometrische Formen, Farben, Partikel
**Phase 2 (Monat 2):** AI-generierte Sprites via **Midjourney** (beste Qualität für Top-Down Sci-Fi Schiffe)
**Phase 3 (Monat 3):** Shader-Effekte für Waffen/Explosionen (100% Code, kein Sprite)

Claude kann keine Sprite-Assets erstellen. Manuel generiert Sprites mit Midjourney wenn Mechaniken stabil sind.

---

## Design-Entscheidungen (festgelegt)

| Thema | Entscheidung | Begründung |
|---|---|---|
| Engine | Godot 4.6 + C# | Claude schreibt alles, Manuel testet |
| Scope | ~3 Monate für spielbaren Core | Tagesbasiertes Tempo mit AI-Entwickler |
| Wirtschaft | Abstrahiert (Formel-basiert) | X4-Feeling ohne vollständige Simulation |
| KI-Simulation | Tick-basiert (1 Tag = 24 Sek) | Performance, kein Frame-Tracking |
| 3D | Nein, komplett 2D | Top-Down |
| Grafik-Timing | Erst Mechaniken, dann Sprites | Sprites nicht vor stabilem Gameplay |
| Innenraum | The Last Starship Querschnitt-Stil | Q2, nach Kampf + Handel |

---

## Coding-Konventionen

- **Namespace:** Immer `ProjectEmptiness.XYZ`
- **Node-Scripts:** `partial class` extends Godot-Typ
- **Simulation-Klassen:** Pure C#, kein Godot-Inheritance
- **Nullable:** `#nullable enable` per-File, nie via .csproj
- **Signals:** In `GameState` zentralisiert
- **Kein GDScript** — alles C#
- **Keine Kommentare** außer bei nicht-offensichtlichem Verhalten
- **JSON** für alle Spieldaten

---

## Visueller Stil

- **Hintergrund:** `Color(0.02, 0.02, 0.055)` — fast schwarz, Blaustich
- **Sternfarben:** Yellow `#FFD938`, Orange `#FF8C1F`, Red `#EB3A2E`, Blue `#4794FF`, White `#EDF0FF`, Neutron `#AE47FF`
- **Fraktionsfarben:** Terran `#4577FF`, Syndicate `#FF4D26`, Void `#A640FF`, Alliance `#33E573`
- **UI:** Dunkle Panels, helle Labels, Sci-Fi-minimalistisch
- **Font:** Godot FallbackFont jetzt → Sci-Fi Monospace Font in Monat 3
