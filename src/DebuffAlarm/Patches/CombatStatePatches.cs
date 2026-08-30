using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;

namespace DebuffAlarm.Patches;

/// <summary>
/// Keeps DebuffAlarmState pointed at the live CombatState. AddCreature/RemoveCreature
/// fire whenever a player or monster enters/leaves combat (including co-op teammates
/// joining), which makes them a reliable place to (re)capture the CombatState
/// reference without needing a dedicated "combat started" hook.
/// </summary>
[HarmonyPatch(typeof(CombatState))]
public static class CombatStatePatches
{
    [HarmonyPatch(nameof(CombatState.AddCreature))]
    [HarmonyPostfix]
    private static void AfterAddCreature(CombatState __instance)
    {
        DebuffAlarmState.SetCombatState(__instance);
    }

    [HarmonyPatch(nameof(CombatState.RemoveCreature))]
    [HarmonyPostfix]
    private static void AfterRemoveCreature(CombatState __instance)
    {
        if (__instance?.Players == null || __instance.Players.Count == 0)
        {
            DebuffAlarmState.ClearCombatState();
        }
        else
        {
            DebuffAlarmState.SetCombatState(__instance);
        }
    }
}
