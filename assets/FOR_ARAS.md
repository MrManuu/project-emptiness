# Asset Brief — Project Emptiness

Hi Aras. This file describes exactly what game assets are needed, in what format, and where to put them.
Read this before generating anything new.

---

## Critical Format Rules

| Requirement | Specification |
|---|---|
| **File format** | PNG with **transparent background** (RGBA, not RGB) |
| **One asset per file** | Never multiple sprites on one image (no grids, no sheets) |
| **Resolution** | See per-category specs below |
| **Orientation** | Top-down view (camera looks straight down) |
| **Style** | Dark sci-fi, Starsector-inspired — realistic but stylized, not cartoon |

**Why transparent background matters:** The game engine (Godot 4) composites sprites onto animated backgrounds. Black, white, or any colored background causes ugly rectangles around assets. Every sprite must have a **transparent alpha channel** (checkerboard pattern in your image editor) outside the object — not white, not black, not grey. If you open the PNG in an image editor and see a solid color behind the object, it is wrong.

**How to verify:** Open the file in Photoshop, GIMP, or any PNG viewer. The area outside the planet/ship should show a grey-and-white checkerboard pattern. If it shows a solid color, the transparency is missing.

---

## Visual Style Reference

- Overall feel: **Starsector** (the PC game) — gritty, military sci-fi, dark color palette
- Ships: angular, utilitarian, metal hull with colored engine glow (blue/orange/white)
- Planets: photorealistic from orbit, lit from one side (not flat-shaded)
- Backgrounds: deep space nebulae — dark, vast, with subtle color variation
- Color palette: dark blues, cool grays, accent colors per faction (see factions below)

---

## Planets

**Folder:** `assets/planets/`  
**Resolution:** `512 × 512 px`  
**Notes:** Planet centered, filling ~80% of canvas. Transparent background outside the sphere. Lit from upper-left (as if the system's star is top-left). No atmosphere rings needed yet.

| Filename | Status | Description |
|---|---|---|
| `planet-barren.png` | ✅ Done | Rocky, cratered, grey-brown. Dead moon. No atmosphere. |
| `planet-desert.png` | 🔲 Needed | Sandy/ochre surface, some rock formations. Thin haze. |
| `planet-terran.png` | ✅ Done | Earth-like. Blue oceans, green/brown landmasses, white clouds. |
| `planet-ocean.png` | 🔲 Needed | Mostly water. Deep blue with white storm swirls. |
| `planet-ice.png` | ✅ Done | White/pale blue. Frozen surface, cracks, some ice caps. |
| `planet-volcanic.png` | 🔲 Needed | Dark rock, orange lava cracks glowing, thin red atmosphere. |
| `planet-gasgiant.png` | 🔲 Needed | Large gas giant, bands of brown/orange/cream, no solid surface visible. |
| `planet-toxic.png` | 🔲 Needed | Sickly yellow-green, dense cloudy atmosphere, acid haze. |

**Current state:** Individual transparent PNGs are now live in the game. Shader workarounds removed. Keep delivering the remaining types in the same style.

---

## Ships

**Folder:** `assets/ships/`  
**Resolution:** `512 × 512 px`  
**Notes:** Ship centered, pointing **upward** (toward top of canvas). Top-down orthographic view — no perspective foreshortening. Transparent background. Engine glow visible at the bottom (engine exhausts point down). Ships should look like they belong to different size classes.

| Filename | Status | Ship Class | Size hint | Description |
|---|---|---|---|---|
| `ship-shuttle.png` | 🔲 Needed | Shuttle | Tiny | Small, simple, 2-seat craft. Civilian. |
| `ship-freighter.png` | ✅ Done | Freighter | Medium | Blocky cargo hauler. Wide, slow-looking. |
| `ship-frigate.png` | ✅ Done | Frigate | Medium | Fast, lightly armed escort. Sleek. |
| `ship-destroyer.png` | ✅ Done | Destroyer | Medium-large | Military, 2-3 weapon mounts visible. |
| `ship-cruiser.png` | 🔲 Needed | Cruiser | Large | Heavy warship, thick armor plating. |
| `ship-battlecruiser.png` | 🔲 Needed | Battlecruiser | Very large | Imposing. Mix of speed and firepower. |
| `ship-carrier.png` | 🔲 Needed | Carrier | Very large | Flat deck, launch bays visible on sides. |
| `ship-dreadnought.png` | 🔲 Needed | Dreadnought | Massive | The biggest. Bristling with weapon batteries. |

**Current state:** freighter/frigate/destroyer are live in the codebase. Continue in the same style for the remaining classes.

---

## Faction Color Coding (for ship variants later)

Ships will eventually come in faction color variants. For now generate neutral/generic versions. The faction palette for reference:

| Faction | Primary Color | Feel |
|---|---|---|
| Terran Confederation | `#4577FF` blue | Military, disciplined, standard |
| Syndicate | `#FF4D26` red-orange | Corporate, aggressive, sharp edges |
| Void Collective | `#A640FF` purple | Mysterious, alien-influenced, organic shapes |
| Free Alliance | `#33E573` green | Patched together, worn, improvised |

---

## Backgrounds

**Folder:** `assets/backgrounds/`  
**Resolution:** `1920 × 1080 px` (16:9)  
**Notes:** Full scene backgrounds. These tile behind the game world. No alpha needed (full opaque images). Should be dark enough that UI text and game objects remain readable on top.

| Filename | Status | Description |
|---|---|---|
| `bg-nebula-blue.png` | ✅ Done | Cold blue/purple deep space nebula. Our main background. |
| `bg-nebula-warm.png` | ✅ Done | Warm orange/amber nebula. For systems near active stars. |
| `bg-deep-space.png` | ✅ Done | Near-black, minimal. Just stars. For void/empty regions. |
| `bg-combat.png` | 🔲 Needed | Slightly denser, more dramatic. Used during combat scenes. |

**Current state:** Three backgrounds delivered and wired up. bg-combat needed once combat scene is implemented (Month 2).

---

## Stations (Future — not urgent)

**Folder:** `assets/stations/`  
**Resolution:** `256 × 256 px`  
**Notes:** Not needed yet. Will be requested in Month 2. Space stations, top-down view, angular and industrial. Each faction will have a visual variant.

---

## Delivery

1. Place files in the folders listed above (create them if they don't exist)
2. Use the exact filenames specified — the game code references them by name
3. One asset per PR or commit, labeled clearly
4. If you generate a variant or alternative, suffix with `-v2`, `-alt`, etc. and note it in the commit message

---

## What's Live in the Game

| File | Notes |
|---|---|
| `backgrounds/bg-nebula-blue.png` | ✅ Active — Galaxy Map + System View main background |
| `backgrounds/bg-nebula-warm.png` | ✅ Delivered — will be used for warm-star systems |
| `backgrounds/bg-deep-space.png` | ✅ Delivered — will be used for void/independent systems |
| `planets/planet-barren.png` | ✅ Active — default fallback + Barren type |
| `planets/planet-terran.png` | ✅ Active — auto-loaded for Terran planets |
| `planets/planet-ice.png` | ✅ Active — auto-loaded for Ice planets |
| `ships/ship-freighter.png` | ✅ Active — player ship in System View |
| `ships/ship-frigate.png` | ✅ Delivered — will be used for NPC ships |
| `ships/ship-destroyer.png` | ✅ Delivered — will be used for NPC ships |
