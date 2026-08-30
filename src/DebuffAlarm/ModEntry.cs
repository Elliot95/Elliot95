using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace DebuffAlarm;

[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    public const string ModId = "debuff_alarm";

    public static void Initialize()
    {
        new Harmony(ModId).PatchAll(typeof(ModEntry).Assembly);
    }
}
