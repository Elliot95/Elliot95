# Debuff Alarm (Slay the Spire 2 mod)

Adds an indicator docked next to the top bar's energy counter that lights up
and pulses whenever **any player in the run — including co-op teammates —
currently has Vulnerable or Weak**. It's a passive HUD alert only; it doesn't
touch cards, combat math, or anything else (`affects_gameplay: false` in the
manifest).

**v1 → v2:** v1 (still in git history) drew the icon as an independent
overlay at a guessed screen position. v2 instead finds the game's own native
energy-counter widget in the scene tree at runtime and docks next to it, so
it actually reads as part of the top bar. See "Status" below for why this
doesn't depend on BaseLib-StS2 despite BaseLib being the more idiomatic way
to build native-looking widgets.

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
2. **`TopBarAnchor.cs`** — looks up `MegaCrit.Sts2.Core.Nodes.NEnergyCounter`
   by type while walking the scene tree. That class name is confirmed real
   (BaseLib-StS2's `NEnergyCounterFactory : NodeFactory<NEnergyCounter>`),
   but its namespace is inferred from sibling native UI types (`NRun`,
   `NOverlayStack`, ...) rather than confirmed directly — fix the `using` if
   it doesn't compile. If the widget it finds isn't actually the energy
   counter in your version, `AlarmIcon` still falls back to a fixed
   top-center position, so the mod stays visible either way.

Why not just depend on BaseLib-StS2 for this? I looked into it, but my
research tooling only returns short paraphrased snippets of GitHub source,
not full files — not enough to safely reproduce the exact abstract method
signatures its generic `NodeFactory<T>` base class requires. Referencing the
base game's own `NEnergyCounter` type directly gets the same "docked in the
top bar" result without depending on an unverified generic contract.

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
- `TopBarAnchor.cs` — recursively searches the scene tree for the live
  `NEnergyCounter` node.
- `AlarmIcon.cs` — a `CanvasLayer` with a small circular `Panel`: dim/grey
  when idle, filled red and pulsing (via a looping `Tween`) when
  `DebuffAlarmState.HasAnyTrackedDebuff()` is true. Re-runs `TopBarAnchor`
  once a second (the energy counter is torn down/recreated between fights)
  and docks just to the right of whatever it finds, falling back to a fixed
  top-center position otherwise. Polled once per frame in `_Process`, which
  is cheap for a 2-4 player roster.

## Build

1. Install the .NET 9 SDK.
2. Point the project at your game install, either:
   - `export STS2_GAME_DIR="/path/to/Slay the Spire 2"` (or the Windows
     PowerShell equivalent), or
   - copy `src/DebuffAlarm/GameDir.props.example` to
     `src/DebuffAlarm/GameDir.props` and edit the path in there.
3. Fix the `Sts2.Core.dll` filename in `DebuffAlarm.csproj` if yours differs.
4. `dotnet build src/DebuffAlarm/DebuffAlarm.csproj -c Release`

## Install

Copy `DebuffAlarm.json` and the built `DebuffAlarm.dll` into a
`DebuffAlarm/` folder under your game's `mods/` directory, then enable it
from the in-game mod list.

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
  mature community modding library (custom powers, UI helpers). Not a
  dependency here (see "Status" above for why), but its
  `NEnergyCounterFactory : NodeFactory<NEnergyCounter>` is what confirms
  `NEnergyCounter` is a real type, and it's worth depending on directly
  instead of raw Harmony patches if you extend this mod further.

If any of the above has moved on to a newer game patch by the time you build
this, re-check the exact method/property names against your own decompile
(`docs/04-reverse-engineering-the-game.md` in the tutorial above covers how).
