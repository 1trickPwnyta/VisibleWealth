using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    [HarmonyPatch(typeof(GlobalControls))]
    [HarmonyPatch(nameof(GlobalControls.GlobalControlsOnGUI))]
    public static class Patch_GlobalControls
    {
        private static float wealthTotal;
        private static float lastRecountTick;

        private static float WealthTotal
        {
            get
            {
                float tick = Find.TickManager.TicksGame;
                if (lastRecountTick < 1f || tick - lastRecountTick > VisibleWealthSettings.WealthGlobalControlCacheTicks)
                {
                    WealthWatcher watcher = Find.CurrentMap.wealthWatcher;
                    watcher.ForceRecount();
                    wealthTotal = watcher.WealthTotal;
                    WealthItems = watcher.WealthItems;
                    WealthBuildings = watcher.WealthBuildings;
                    WealthPawns = watcher.WealthPawns;
                    lastRecountTick = tick;
                }
                return wealthTotal;
            }
        }
        private static float WealthItems;
        private static float WealthBuildings;
        private static float WealthPawns;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();

            int index = instructionsList.FindIndex(i => i.Calls(typeof(GlobalControlsUtility).Method(nameof(GlobalControlsUtility.DoTimespeedControls))));
            instructionsList.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldc_R4, 200f),
                new CodeInstruction(OpCodes.Ldloca_S, 1),
                new CodeInstruction(OpCodes.Call, typeof(WealthOverlay).Method(nameof(WealthOverlay.DoGlobalControls)))
            });

            index = instructionsList.FindIndex(i => i.Calls(typeof(GlobalControlsUtility).Method(nameof(GlobalControlsUtility.DoDate))));
            instructionsList.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldc_R4, 200f),
                new CodeInstruction(OpCodes.Ldloca_S, 1),
                new CodeInstruction(OpCodes.Call, typeof(Patch_GlobalControls).Method(nameof(DoWealth)))
            });

            return instructionsList;
        }

        private static void DoWealth(float leftX, float width, ref float curBaseY)
        {
            if (VisibleWealthSettings.WealthGlobalControl && Find.CurrentMap != null)
            {
                string wealth = WealthTotal.ToStringMoney();
                float textWidth = Text.CalcSize(wealth).x;
                Rect rect = new Rect(leftX + width - textWidth - 14f, curBaseY - 26f, textWidth + 14f, 26f);
                using (new TextBlock(TextAnchor.MiddleCenter)) Widgets.Label(rect, "MoneyFormat".Translate(WealthTotal.ToString("F0")).Colorize(VisibleWealthSettings.WealthGlobalControlColor == Color.clear ? GUI.color : VisibleWealthSettings.WealthGlobalControlColor));
                Widgets.DrawHighlightIfMouseover(rect);
                TooltipHandler.TipRegion(rect, "VisibleWealth_WealthSummary".Translate(WealthTotal.ToStringMoney(), WealthItems.ToStringMoney(), WealthBuildings.ToStringMoney(), WealthPawns.ToStringMoney()));
                if (Widgets.ButtonInvisible(rect))
                {
                    Dialog_WealthBreakdown.Open();
                }
                curBaseY -= rect.height + 2f;
            }
        }
    }
}
