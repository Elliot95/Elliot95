using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace DebuffAlarm.Patches;

/// <summary>
/// Attaches one AlarmIcon to every run, mirroring the "attach a controller to
/// NRun._Ready" pattern from docs/14-custom-gui.md of the community modding
/// tutorial (see README for the link). AlarmIcon draws itself as a screen-space
/// CanvasLayer, so it doesn't need to know NRun's own position in the scene tree.
/// </summary>
[HarmonyPatch(typeof(NRun), nameof(NRun._Ready))]
public static class RunPatches
{
    private const string ControllerName = "DebuffAlarmIcon";

    private static void Postfix(NRun __instance)
    {
        if (__instance.GetNodeOrNull(ControllerName) is not null)
        {
            return;
        }

        __instance.AddChild(new AlarmIcon
        {
            Name = ControllerName
        });
    }
}
