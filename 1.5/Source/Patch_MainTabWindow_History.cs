using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VisibleWealth
{
    [HarmonyPatch(typeof(MainTabWindow_History))]
    [HarmonyPatch("DoStatisticsPage")]
    public static class Patch_MainTabWindow_History
    {
        public static void Postfix(Rect rect)
        {
            Rect buttonRect = new Rect(rect.x, rect.yMax - 30f, 200f, 30f);
            if (Widgets.ButtonText(buttonRect, "VisibleWealth_WealthBreakdown".Translate()))
            {
                Find.WindowStack.Add(new Dialog_WealthBreakdown());
                SoundDefOf.Click.PlayOneShot(null);
            }
        }
    }
}
