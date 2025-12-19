using HarmonyLib;
using Verse;

namespace VisibleWealth
{
    [HarmonyPatch(typeof(Map))]
    [HarmonyPatch(nameof(Map.ConstructComponents))]
    public static class Patch_Map
    {
        public static void Postfix(Map __instance)
        {
            WealthGlobalControl.RegisterMap(__instance);
            WealthOverlay.RegisterMap(__instance);
        }
    }
}
