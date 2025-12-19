using HarmonyLib;
using RimWorld;
using Verse;

namespace VisibleWealth
{
    [HarmonyPatch(typeof(WealthWatcher))]
    [HarmonyPatch(nameof(WealthWatcher.ForceRecount))]
    public static class Patch_WealthWatcher
    {
        public static void Postfix(WealthWatcher __instance, Map ___map)
        {
            WealthGlobalControl control = WealthGlobalControl.ForMap(___map);
            if (control != null)
            {
                control.wealthTotal = __instance.WealthTotal;
                control.WealthItems = __instance.WealthItems;
                control.WealthBuildings = __instance.WealthBuildings;
                control.WealthPawns = __instance.WealthPawns;
                control.lastRecountTick = Find.TickManager.TicksGame;
            }
        }
    }
}
