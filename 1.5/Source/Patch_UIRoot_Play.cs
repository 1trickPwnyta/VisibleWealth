using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VisibleWealth
{
    [HarmonyPatch(typeof(UIRoot_Play))]
    [HarmonyPatch(nameof(UIRoot_Play.UIRootUpdate))]
    public static class Patch_UIRoot_Play_UIRootUpdate
    {
        public static void Postfix()
        {
            if (Find.CurrentMap != null && !WorldRendererUtility.WorldRenderedNow)
            {
                if (KeyBindingUtility.WealthBreakdown.JustPressed || (KeyBindingUtility.WealthBreakdownShift.JustPressed && KeyBindingUtility.IsShiftHeld()))
                {
                    Dialog_WealthBreakdown.Open();
                }
            }
        }
    }
}
