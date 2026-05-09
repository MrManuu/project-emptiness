# Project Emptiness

> *"The stars don't care about you. Build an empire anyway."*

A 2D top-down space sandbox — Starsector's combat and aesthetic, X4 Foundations' living economy and empire building, The Last Starship's clarity. Built in Godot 4 + C#.

---

## Concept

You start with a single ship and 50,000 credits in a procedurally generated galaxy of 64 star systems, divided between four rival factions. From there:

- Trade goods between stations, exploiting price differences
- Join a faction, earn a commission, fly their flag
- Build your own fleet — from a lone freighter to a capital ship armada
- Establish stations and trade routes, build an economic empire
- Navigate complex diplomacy — forge alliances, broker peace, declare war
- Found your own faction and carve out territory in the void

The universe runs without you. Factions expand, trade, and go to war. Prices fluctuate. A peaceful system may be under siege when you return.

---

## Inspirations

| Game | What we take |
|---|---|
| **Starsector** | Visual aesthetic, real-time ship combat, flux/shield system, faction commissions |
| **The Last Starship** | Ship cross-section interior, UI clarity, schematic style |
| **X4 Foundations** | Living economy, faction AI, empire building, diplomacy depth |

---

## Tech Stack

- **Engine:** Godot 4.6.2 (.NET / C#)
- **Language:** C# for all game logic and simulation
- **Platform:** Windows (cross-platform export later)

---

## Setup

### Requirements
- [Godot 4.6+ .NET version](https://godotengine.org/download/windows/)
- .NET SDK 8.0+

### Run
1. Clone / download this repo
2. Open **Godot 4** → **Import** → select `project.godot`
3. `Projekt → Tools → C# → Create C# solution` (first time only)
4. Click the **Build** button (hammer icon)
5. Press **F5**

---

## Current State

**Version 0.1 — Galaxy Map (Tag 1)**

### What's playable
- 64 procedurally generated star systems across 4 faction territories
- Pan (right-click drag) and zoom (scroll wheel) the galaxy
- Click any system → info panel (faction, star type, planets, security level)
- Jump between connected systems via hyperlanes
- Pulsing indicator shows your current location
- Live economy: station prices fluctuate daily with supply and demand
- HUD: credits, current day, current location

### What it looks like
- Black space background with ambient star field
- Stars colored by type: yellow, orange, red, blue, white, neutron (purple)
- Faction territory shown as colored glows around stars
- Hyperlane network connecting all systems

---

## Roadmap

Development pace: Claude writes all code, Manuel tests and gives feedback → **1-2 days per feature** instead of weeks.

### Month 1 — Gameplay Foundation
- [x] Day 1: Galaxy map, navigation, factions, economy simulation
- [ ] Day 2: System view — enter a system, see planets and stations
- [ ] Day 3: Station trade screen — buy and sell goods
- [ ] Day 4-5: Real-time combat — first ship battle
- [ ] Day 6: Fleet management — buy and command 2-5 ships
- [ ] Day 7: Faction reputation — consequences for your actions
- [ ] Day 8: Mission system — trade runs, bounties, escorts
- [ ] Day 9: Station ownership — buy or build outposts
- [ ] Day 10: Diplomacy screen — alliances, treaties, tribute

### Month 2 — Empire & AI
- [ ] Day 11-12: Faction AI — autonomous expansion, trade, war declarations
- [ ] Day 13: Found your own faction
- [ ] Day 14: Empire overview — your systems, income, fleets
- [ ] Day 15: First graphics pass — AI-generated ship sprites (Midjourney)
- [ ] Day 16-17: Weapon effects and explosions (shaders + particles)
- [ ] Day 18: Ship upgrade system
- [ ] Day 19-20: Combat polish — flux system, shields, tactics

### Month 3 — Content & Polish
- [ ] Day 21-22: More ship classes (8+), more goods, random events
- [ ] Day 23: Sound and music integration
- [ ] Day 24: Ship interior (The Last Starship cross-section style)
- [ ] Day 25-26: UI/UX pass, sci-fi font
- [ ] Day 27-28: Save/load system
- [ ] Day 29-30: Balance pass + beta prep

---

## Project Structure

```
Project Emptiness/
├── scenes/          — Godot scenes (GalaxyMap, SystemView, Combat, ...)
├── src/
│   ├── Core/        — GameState singleton
│   ├── Data/        — Models: StarSystem, Faction, Ship, TradeGood
│   ├── Generation/  — Procedural galaxy generator
│   └── Simulation/  — Tick-based economy and faction AI
└── data/            — JSON: factions, goods, ship templates
```

---

## Controls

| Input | Action |
|---|---|
| Right-click drag | Pan camera |
| Scroll wheel | Zoom in/out |
| Left-click | Select system |
| Jump Here button | Travel to selected system |
