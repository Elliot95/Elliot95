using System.Linq;
using MegaCrit.Sts2.Core.Combat;

namespace DebuffAlarm;

/// <summary>
/// Pure query over a CombatState: is any player's creature currently
/// carrying one of the tracked debuffs? Add more power type names here
/// (e.g. "FrailPower", "PoisonPower") to widen what trips the alarm.
/// </summary>
public static class DebuffWatcher
{
    private static readonly string[] TrackedPowers = { "VulnerablePower", "WeakPower" };

    public static bool AnyPlayerHasTrackedDebuff(CombatState combat)
    {
        if (combat?.Players == null)
        {
            return false;
        }

        foreach (var player in combat.Players)
        {
            var powers = player?.Creature?.Powers;
            if (powers == null)
            {
                continue;
            }

            foreach (var power in powers)
            {
                if (power == null)
                {
                    continue;
                }

                if (power.Amount > 0 && TrackedPowers.Contains(power.GetType().Name))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
