# Project Emptiness

> *"The stars don't care about you. Build an empire anyway."*

A 2D top-down space sandbox — Starsector's combat and aesthetic, X4 Foundations' living economy and empire building, The Last Starship's clarity. Built in Godot 4 + C#.

---

## Concept

You start with a single ship and 50,000 credits in a procedurally generated galaxy of 64 star systems, divided between four rival factions. From there:

- Trade goods between stations, exploiting price differences
- Land on planets — explore, extract resources, interact with colonies
- Join a faction, earn a commission, fly their flag
- Build your own fleet — from a lone freighter to a capital ship armada
- Establish stations and trade routes, build an economic empire
- Navigate complex diplomacy — forge alliances, broker peace, declare war
- Found your own faction and carve out territory in the void

The universe runs without you. Factions expand, trade, and go to war. Prices fluctuate. A peaceful system may be under siege when you return.

---

## Possible Scope Expansions

These are active design directions currently under consideration. They are not all locked for the next build, but they describe where the project could grow beyond the current galaxy-map-first prototype.

### 1. System Map as a Playable Local Space Layer
Instead of remaining a mostly informational scene, the **System View** could evolve into a more dynamic local-space layer with **Mount & Blade: Bannerlord-style movement**:
- fleets, traders, pirates, patrols, and stations moving in real time on the system map
- the player traveling freely inside the system instead of only clicking static objects
- encounters triggered by proximity, pursuit, interception, or escort behavior
- system geography becoming strategically relevant, not just decorative

### 2. Modular Station Construction
Station building could become a major empire feature:
- build stations in their own construction/management instance
- finished stations appear physically on the system map
- stations assembled **module by module** rather than as one fixed prefab
- modules could include cargo, refinery, habitation, defense, ship services, trade, research, and production
- station layout would affect both function and visual identity

### 3. Stations as Walkable / Boardable Spaces
Stations may eventually be enterable in person via **spacesuit / EVA / docked traversal**:
- board stations and move through them directly
- gameplay tone could lean toward **roguelike** or **Quasimorph-style** exploration
- station interiors could support combat, looting, quests, faction events, sabotage, survival pressure, or social interaction
- this would add a strong on-foot layer to what is currently a strategic space sandbox

### 4. Planet Landing and On-Foot Gameplay
Planets are planned to become more than resource nodes:
- land on planets and move around as a character
- possible presentation: **2D RPG-style exploration**, **Warsim-/combat-heavy encounters**, or a hybrid of both
- colonized worlds, ruins, hostile fauna, raiders, and faction activity could all become gameplay spaces
- planetary visits could tie together narrative, resources, survival, and combat

### 5. Planetary Base / Colony Management
Planet-side empire building is also on the table:
- establish and upgrade a planetary base
- choose individual buildings rather than only abstract percentages
- use a more classic **kingdom-management / colony-management** approach
- buildings could affect income, production, population, defense, logistics, and political stability
- this would connect local on-planet gameplay with broader faction and economic strategy

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
- **Workflow:** Claude writes all code, Manuel tests and gives feedback → 1-2 days per feature

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

**Day 3 complete — Station Trade Screen**

### What's playable
- 64 procedurally generated star systems across 4 faction territories
- Galaxy map: pan, zoom, click to select systems, jump via hyperlanes
- **System view:** enter any system — see orbiting planets (animated) and stations
- Click planets → type, population, resources
- Click stations → faction, goods inventory with live prices
- **Station trade screen:** dock at any station — buy and sell goods, live prices, cargo tracking
- Trade arbitrage: buy cheap in one system, sell expensive in another
- Live economy: station prices fluctuate daily with supply and demand
- HUD: credits, current day, current location

### What it looks like
- Nebula background (painterly deep space) in all scenes
- Stars colored by type: yellow, orange, red, blue, white, neutron (purple)
- Faction territory shown as colored glows around stars
- Hyperlane network connecting all systems
- AI-generated planet sprites (barren, terran, ice) — transparent PNGs composited cleanly
- Player ship sprite (freighter class) — transparent PNG
- Remaining planet types fall back to barren sprite until Aras delivers them

---

## Roadmap

### Month 1 — Gameplay Foundation
- [x] Day 1: Galaxy map, navigation, factions, economy simulation
- [x] Day 2: System view — orbiting planets and stations, click info
- [x] Day 3: Station trade screen — buy/sell goods, cargo tracking, live prices
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
<<<<<<< HEAD
- [ ] Day 15: Planet landing — explore, extract resources, colony interaction *(may expand into full on-foot RPG/combat layer)*
- [ ] Day 16: First graphics pass — remaining ship classes, faction color variants, and asset production pipeline (Aras / GPT Image 2)
- [ ] Day 17-18: Weapon effects and explosions (shaders + particles)
- [ ] Day 19: Ship upgrade system
- [ ] Day 20: Combat polish — flux system, shields, tactics

### Longer-Term Expansion Paths
- [ ] System-view free movement with roaming fleets and interception gameplay
- [ ] Modular station construction visible directly on the system map
- [ ] Walkable stations / EVA boarding with roguelike or Quasimorph-like loops
- [ ] Planet-side base building with individual structures and colony management

### Month 3 — Content & Polish
- [ ] Day 21-22: More ship classes (8+), more goods, random events
- [ ] Day 23: Sound and music integration
- [ ] Day 24: Ship interior (The Last Starship cross-section style)
- [ ] Day 25-26: UI/UX pass, sci-fi font
- [ ] Day 27-28: Save/load system
- [ ] Day 29-30: Balance pass + beta prep

---

## Controls

### Galaxy Map
| Input | Action |
|---|---|
| Right-click drag | Pan camera |
| Scroll wheel | Zoom in/out |
| Left-click | Select system |
| Jump Here button | Travel to selected system |
| Enter System button | Open system view (current system only) |

### System View
| Input | Action |
|---|---|
| Scroll wheel | Zoom in/out |
| Left-click planet | Show planet info |
| Left-click station | Show station goods |
| ◀ Galaxy Map button | Return to galaxy map |

---

## Update Log

### 2026-05-10 — Day 3: Station Trade Screen
- Dock button in System View navigates to trade screen
- Two-panel layout: Station Stock (buy) + Your Cargo (sell)
- Buy 1 / Max buttons with credit + cargo space validation
- Sell 1 / Sell All with total value preview
- Live price updates via existing economy simulation
- CargoChanged signal added to GameState for live UI refresh

### 2026-05-10 — Asset Integration (final)
- Aras delivered proper RGBA transparent PNGs (1024×1024): barren, terran, ice planets + freighter, frigate, destroyer ships
- All shader workarounds removed — sprites composite cleanly onto backgrounds
- 3 backgrounds live: `bg-nebula-blue` (main), `bg-nebula-warm`, `bg-deep-space`
- Remaining planet types (desert, ocean, volcanic, gas giant, toxic) pending — fall back to barren sprite
- Alpha edge feathering slightly rough — Aras to refine in next batch

### 2026-05-10 — Asset Integration (first pass)
- Nebula background integrated into Galaxy Map and System View
- Planet sprites and ship visible in System View via GLSL shader workaround (black/white background removal)
- Asset brief created at `assets/FOR_ARAS.md`: specs, filenames, transparent PNG requirement
- **Scope decision:** Sprites generated by Aras (GPT Image 2), not Midjourney

### 2026-05-10 — Day 2: System View
- System view scene implemented: animated orbiting planets + stations
- Planet generation added (type, population, resources per planet)
- Clickable planets and stations with info panel
- "Enter System" button on galaxy map info panel
- "◀ Galaxy Map" back button in system view
- **Scope decision:** Planet landing and interaction added to roadmap (Day 15, gameplay TBD)

### 2026-05-10 — Day 1: Galaxy Map
- Initial release
- 64-system procedural galaxy with spiral distribution
- 4 main factions + independent territory via flood-fill
- Hyperlane network (MST-guaranteed connectivity)
- Pan/zoom/click navigation, system jump
- Tick-based economy: supply/demand price fluctuation
- HUD: credits, day counter, location
