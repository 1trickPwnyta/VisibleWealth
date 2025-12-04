using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VisibleWealth
{
    [HarmonyPatch(typeof(MapInterface))]
    [HarmonyPatch(nameof(MapInterface.MapInterfaceUpdate))]
    public static class Patch_MapInterface_MapInterfaceUpdate
    {
        public static void Postfix()
        {
            if (Find.CurrentMap != null && WorldRendererUtility.DrawingMap)
            {
                WealthOverlay.ForMap(Find.CurrentMap).WealthOverlayUpdate();
            }
        }
    }

    [HarmonyPatch(typeof(MapInterface))]
    [HarmonyPatch(nameof(MapInterface.MapInterfaceOnGUI_BeforeMainTabs))]
    public static class Patch_MapInterface_MapInterfaceOnGUI_BeforeMainTabs
    {
        public static void Postfix()
        {
            if (Find.CurrentMap != null && WorldRendererUtility.DrawingMap)
            {
                WealthOverlay.ForMap(Find.CurrentMap).WealthOverlayOnGUI();
            }
        }
    }
}
