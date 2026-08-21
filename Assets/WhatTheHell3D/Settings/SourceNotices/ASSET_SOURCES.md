# Asset Sources

This project currently builds the redesigned campaign levels from assets already present in the repository plus procedural Godot meshes.

## Repository assets used by the campaign levels

- `assets/platformer_kit/`: coins, gems, key, heart, bridge, trees, rocks, spikes, saw hazard, checkpoint/goal flag.
- `assets/environment/props/`: fences, crates, rope, ladders, utility props.
- `assets/environment/nature/`: trees, rocks, grass and mushrooms.
- `assets/medieval_buildings/`: towers, walls, banners, doors and castle props.
- `assets/characters/enemies/`: goblin, zombie and witch enemy models.
- `assets/characters/player/`: knight player model.
- `assets/animations/player/Dying.fbx`: player death animation provided by the user from Mixamo for the main knight.
- `assets/animations/player/Standing_Melee_Attack_Horizontal.fbx`: player sword combo animation provided by the user from Mixamo.
- `assets/animations/player/Stable_Sword_Inward_Slash.fbx`: player heavy sword combo animation provided by the user from Mixamo.
- `assets/animations/player/Sprinting_Forward_Roll.fbx`: player sprint-jump roll animation provided by the user from Mixamo.
- `assets/audio/`: existing music, ambience, dialogue and SFX.

## Procedural content

The forest, mines and castle layouts are generated in `scripts/game/campaign_level.gd` with Godot primitives for platforms, blockout silhouettes, lighting markers, danger markers and decorative accents.

No new third-party models, textures, sounds or images were downloaded by Codex for the current level redesign pass.
