using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PlaySettings))]
    [HarmonyPatch(nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    public static class Patch_PlaySettings
    {
        private static Texture2D WealthBreakdownIcon = ContentFinder<Texture2D>.Get("UI/Buttons/WealthBreakdown");

        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (!worldView)
            {
                if (row.ButtonIcon(WealthBreakdownIcon, "VisibleWealth_WealthBreakdown".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_WealthBreakdown());
                }
            }
        }
    }
}
