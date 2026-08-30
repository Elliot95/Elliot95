using MegaCrit.Sts2.Core.Combat;

namespace DebuffAlarm;

/// <summary>
/// Holds the most recently seen CombatState, fed by Patches/CombatStatePatches.cs.
/// AlarmIcon polls HasAnyTrackedDebuff() every frame rather than reacting to a
/// single "power changed" event, since powers can be added/removed through
/// several different game paths and polling is cheap for a handful of players.
/// </summary>
public static class DebuffAlarmState
{
    private static CombatState _current;

    public static void SetCombatState(CombatState combat)
    {
        _current = combat;
    }

    public static void ClearCombatState()
    {
        _current = null;
    }

    public static bool HasAnyTrackedDebuff()
    {
        return DebuffWatcher.AnyPlayerHasTrackedDebuff(_current);
    }
}
