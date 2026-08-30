# Debuff Alarm (Slay the Spire 2 mod)

Adds a small indicator near the top of the screen that lights up and pulses
whenever **any player in the run — including co-op teammates — currently has
Vulnerable or Weak**. It's a passive HUD alert only; it doesn't touch cards,
combat math, or anything else (`affects_gameplay: false` in the manifest).

## Status: unverified against a live install

This was built in an environment without Slay the Spire 2 or its assemblies
installed, so it could not be compiled or run against the real game. Every
API it calls is taken from public, real STS2 modding sources (linked below)
rather than guessed, but STS2's mod API is unofficial/reverse-engineered and
has shifted between patches before, so **treat this as a scaffold to build
against your own game version, not a drop-in binary**. The two spots most
likely to need a tweak for your exact version:

1. **`DebuffAlarm.csproj`** — the `<Reference Include="Sts2.Core">` HintPath
   assumes the game's C# assembly is named `Sts2.Core.dll` next to your
   install. Confirm the real filename (open your install folder, or check
   what your mod loader already references) and fix the path.
2. **`AlarmIcon.cs`** — draws itself as an independent screen-space overlay
   near the top of the screen rather than reusing the native top-bar
   container, precisely so it doesn't depend on a HUD scene path I couldn't
   verify. `Diameter`/`TopOffset` are eyeballed; nudge them once you see it
   in-game to line up with the real top bar.

Everything else — the manifest shape, the mod entry point, the `NRun._Ready`
attach pattern, and the `CombatState`/`Creature.Powers` query — matches real,
published STS2 mod source (see Sources).

## How it works

- `ModEntry.cs` — the mod's entry point (`[ModInitializer]`), registers a
  Harmony instance and patches everything in the assembly.
- `Patches/CombatStatePatches.cs` — postfixes `CombatState.AddCreature` /
  `RemoveCreature` (these fire whenever a player or monster, including a
  co-op teammate, joins or leaves combat) to keep `DebuffAlarmState` pointed
  at the live `CombatState`.
- `DebuffWatcher.cs` — pure check: walks `combat.Players`, and for each
  player's `Creature.Powers`, looks for `VulnerablePower` / `WeakPower` with
  `Amount > 0`. Add more type names here (e.g. `"FrailPower"`,
  `"PoisonPower"`) to widen what trips the alarm.
- `Patches/RunPatches.cs` — postfixes `NRun._Ready` to attach one
  `AlarmIcon` per run, the same way the community tutorial's custom-GUI
  example attaches its own controller.
- `AlarmIcon.cs` — a `CanvasLayer` with a small circular `Panel`: dim/grey
  when idle, filled red and pulsing (via a looping `Tween`) when
  `DebuffAlarmState.HasAnyTrackedDebuff()` is true. Polled once per frame in
  `_Process`, which is cheap for a 2-4 player roster.

## Build

1. Install the .NET 9 SDK.
2. Point the project at your game install, either:
   - `export STS2_GAME_DIR="/path/to/Slay the Spire 2"` (or the Windows
     PowerShell equivalent), or
   - copy `src/DebuffAlarm/GameDir.props.example` to
     `src/DebuffAlarm/GameDir.props` and edit the path in there.
3. Fix the `Sts2.Core.dll` filename in `DebuffAlarm.csproj` if yours differs.
4. `dotnet build src/DebuffAlarm/DebuffAlarm.csproj -c Release`

## Install (local testing)

Copy `DebuffAlarm.json` and the built `DebuffAlarm.dll` into a
`DebuffAlarm/` folder under your game's `mods/` directory, then enable it
from the in-game mod list. **Do this and confirm it actually works in-game
before publishing** — nothing here has been run against the real game yet.

## Publish to Steam Workshop

STS2 has an official uploader tool for this:
[megacrit/sts2-mod-uploader](https://github.com/megacrit/sts2-mod-uploader).

1. Download `ModUploader.exe` from that repo's releases and run it once —
   it generates a `NewModWorkspace` folder next to itself.
2. Rename that folder (e.g. `DebuffAlarmWorkspace`) and populate it:
   - `content/mods/DebuffAlarm/DebuffAlarm.json` and `DebuffAlarm.dll` —
     mirroring the same `mods/<id>/` layout used for local install, per the
     packaging guidance in the modding tutorial's `docs/09` chapter. If the
     uploader's own generated workspace README describes a different
     `content/` layout, follow that instead — it's the authoritative source,
     this is a best-effort based on the general packaging convention.
   - `workshop.json` — metadata for the Workshop listing. This repo's
     `src/DebuffAlarm/workshop/workshop.json` has starter values (title,
     description, tags); the uploader generates its own copy with the real
     expected fields on first run, so treat this repo's version as a draft
     to copy values *from*, not a file to drop in as-is.
   - `image.png` — preview image, must stay under 1MB. Use
     `src/DebuffAlarm/workshop/image.png` (a generated placeholder) or swap
     in your own.
3. From the command line: `ModUploader.exe upload -w <workspace-folder>`.
4. It writes a `mod_id.txt` into the workspace — keep that around, it's what
   makes the *next* run of the same command update this Workshop item
   instead of creating a new one.

This tool appears to be Windows-only based on available guides — if you're
on macOS/Linux you may need Wine/Proton or a Windows machine for this step.

## Sources

Built against the following real, public STS2 modding material:

- [fresh-milkshake/Modding-Tutorial](https://github.com/fresh-milkshake/Modding-Tutorial) —
  manifest/mod-loader shape, `[ModInitializer]` + Harmony entry point pattern,
  and the `NRun._Ready` controller-attach pattern (`docs/14-custom-gui.md`).
- [jiegec/STS2FirstMod](https://github.com/jiegec/STS2FirstMod) — example
  mod project layout (manifest + csproj + entry point).
- [S0ul3r/BoberInSpire](https://github.com/S0ul3r/BoberInSpire) —
  `CombatState.AddCreature`/`RemoveCreature` as the hook points for capturing
  live combat state, and `Creature.Powers` / power `Amount` / type-name
  lookup (`VulnerablePower`, `WeakPower`, ...) for reading status effects.
- [Alchyr/BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) — a more
  mature community modding library (custom powers, UI helpers) worth
  depending on instead of raw Harmony patches if you extend this further.
- [megacrit/sts2-mod-uploader](https://github.com/megacrit/sts2-mod-uploader) —
  the official Steam Workshop upload tool (see "Publish to Steam Workshop").

If any of the above has moved on to a newer game patch by the time you build
this, re-check the exact method/property names against your own decompile
(`docs/04-reverse-engineering-the-game.md` in the tutorial above covers how).
