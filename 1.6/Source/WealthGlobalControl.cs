using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthGlobalControl
    {
        private static Dictionary<Map, WealthGlobalControl> instances = new Dictionary<Map, WealthGlobalControl>();

        public static void RegisterMap(Map map)
        {
            if (!instances.ContainsKey(map))
            {
                instances[map] = new WealthGlobalControl(map);
            }
        }

        public static WealthGlobalControl ForMap(Map map) => instances.ContainsKey(map) ? instances[map] : null;

        public static WealthGlobalControl ForCurrentMap() => ForMap(Find.CurrentMap);

        private Map map;
        public float lastRecountTick;
        public float wealthTotal;

        private float WealthTotal
        {
            get
            {
                if (Find.CurrentMap == map && (lastRecountTick < 1f || Find.TickManager.TicksGame - lastRecountTick > VisibleWealthSettings.WealthGlobalControlCacheTicks))
                {
                    map.wealthWatcher.ForceRecount();
                }
                return wealthTotal;
            }
        }
        public float WealthItems;
        public float WealthBuildings;
        public float WealthPawns;

        public WealthGlobalControl(Map map)
        {
            this.map = map;
        }

        public void DoWealth(float leftX, float width, ref float curBaseY)
        {
            if (VisibleWealthSettings.WealthGlobalControl && Find.CurrentMap == map)
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
