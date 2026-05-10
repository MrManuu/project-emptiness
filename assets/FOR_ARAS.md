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

**Why transparent background matters:** The game engine (Godot 4) composites sprites onto animated backgrounds. Black or colored backgrounds cause ugly rectangles around assets. Every sprite must have a transparent alpha channel outside the object.

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

| Filename | Description |
|---|---|
| `planet-barren.png` | Rocky, cratered, grey-brown. Dead moon. No atmosphere. |
| `planet-desert.png` | Sandy/ochre surface, some rock formations. Thin haze. |
| `planet-terran.png` | Earth-like. Blue oceans, green/brown landmasses, white clouds. |
| `planet-ocean.png` | Mostly water. Deep blue with white storm swirls. |
| `planet-ice.png` | White/pale blue. Frozen surface, cracks, some ice caps. |
| `planet-volcanic.png` | Dark rock, orange lava cracks glowing, thin red atmosphere. |
| `planet-gasgiant.png` | Large gas giant, bands of brown/orange/cream, no solid surface visible. |
| `planet-toxic.png` | Sickly yellow-green, dense cloudy atmosphere, acid haze. |

**Current state:** `assets/concepts/gpt-image-2/pack-01-clean-tactical/` contains planet sheets (2x2 grids, black background) — these are being used with a shader workaround. Individual transparent PNGs would remove the need for the shader.

---

## Ships

**Folder:** `assets/ships/`  
**Resolution:** `512 × 512 px`  
**Notes:** Ship centered, pointing **upward** (toward top of canvas). Top-down orthographic view — no perspective foreshortening. Transparent background. Engine glow visible at the bottom (engine exhausts point down). Ships should look like they belong to different size classes.

| Filename | Ship Class | Size hint | Description |
|---|---|---|---|
| `ship-shuttle.png` | Shuttle | Tiny | Small, simple, 2-seat craft. Civilian. |
| `ship-freighter.png` | Freighter | Medium | Blocky cargo hauler. Wide, slow-looking. |
| `ship-frigate.png` | Frigate | Medium | Fast, lightly armed escort. Sleek. |
| `ship-destroyer.png` | Destroyer | Medium-large | Military, 2-3 weapon mounts visible. |
| `ship-cruiser.png` | Cruiser | Large | Heavy warship, thick armor plating. |
| `ship-battlecruiser.png` | Battlecruiser | Very large | Imposing. Mix of speed and firepower. |
| `ship-carrier.png` | Carrier | Very large | Flat deck, launch bays visible on sides. |
| `ship-dreadnought.png` | Dreadnought | Massive | The biggest. Bristling with weapon batteries. |

**Current state:** `assets/concepts/gpt-image-2/pack-01-clean-tactical/ship-01.png` is a good freighter/frigate-scale reference. Continue in that style.

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

| Filename | Description |
|---|---|
| `bg-nebula-blue.png` | Cold blue/purple deep space nebula. Our main background. |
| `bg-nebula-warm.png` | Warm orange/amber nebula. For systems near active stars. |
| `bg-deep-space.png` | Near-black, minimal. Just stars. For void/empty regions. |
| `bg-combat.png` | Slightly denser, more dramatic. Used during combat scenes. |

**Current state:** `assets/concepts/gpt-image-2/pack-02-painterly-probe/background-01.png` is a 1024×1024 painterly nebula, used as-is but stretched. A proper 1920×1080 version in the same style would be ideal.

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

## What's Already Usable

| File | Status | Notes |
|---|---|---|
| `concepts/gpt-image-2/pack-01-clean-tactical/ship-01.png` | ✅ In use | Good freighter. Keep this style. |
| `concepts/gpt-image-2/pack-01-clean-tactical/planet-01.png` | ⚠️ Workaround | 2x2 grid, shader applied. Replace with individual files. |
| `concepts/gpt-image-2/pack-01-clean-tactical/planet-02.png` | ⚠️ Workaround | Same issue. |
| `concepts/gpt-image-2/pack-02-painterly-probe/background-01.png` | ✅ In use | Good nebula. 1920×1080 version needed. |
