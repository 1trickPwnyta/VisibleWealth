using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VisibleWealth
{
    public class Dialog_WealthBreakdown : Window
    {
        private static Vector2 scrollPosition;
        private static float y;
        public static QuickSearchWidget Search = new QuickSearchWidget();

        private readonly Map map;
        private readonly WealthNode itemsNode;
        private readonly WealthNode buildingsNode;
        private readonly WealthNode pawnsNode;
        private readonly WealthNode pocketMapsNode;

        public override Vector2 InitialSize => new Vector2(WealthNode.Size.x + 20f + Window.StandardMargin * 2, 600f);

        public Dialog_WealthBreakdown()
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = true;
            absorbInputAroundWindow = true;

            WealthNode_Floor.TerrainCache.Clear();

            map = Find.CurrentMap;
            map.wealthWatcher.ForceRecount();
            itemsNode = new WealthNode_WealthCategory(map, 0, WealthCategory.Items);
            buildingsNode = new WealthNode_WealthCategory(map, 0, WealthCategory.Buildings);
            pawnsNode = new WealthNode_WealthCategory(map, 0, WealthCategory.Pawns);
            pocketMapsNode = new WealthNode_WealthCategory(map, 0, WealthCategory.PocketMaps);

            Search.Reset();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 45f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "VisibleWealth_WealthBreakdown".Translate());
            Text.Font = GameFont.Small;

            Rect totalWealthRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, 30f);
            Widgets.Label(totalWealthRect, "VisibleWealth_TotalWealth".Translate(map.wealthWatcher.WealthTotal.ToString("F0")));

            Rect searchRect = new Rect(inRect.xMax - Window.QuickSearchSize.x, inRect.y + 45f, Window.QuickSearchSize.x, Window.QuickSearchSize.y);
            Search.OnGUI(searchRect);

            Rect outRect = new Rect(inRect.x, inRect.y + 45f + 30f + 10f, inRect.width, inRect.height - 45f - 30f - 10f - 24f - 10f - Window.CloseButSize.y - 10f);
            Rect viewRect = new Rect(0f, 0f, WealthNode.Size.x, y);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            y = 0f;

            itemsNode.Draw(ref y);
            buildingsNode.Draw(ref y);
            pawnsNode.Draw(ref y);
            pocketMapsNode.Draw(ref y);

            Widgets.EndScrollView();

            Rect optionsRect = new Rect(inRect.x, inRect.yMax - Window.CloseButSize.y - 10f - 24f, inRect.width, 24f);
            Rect sortByRect = new Rect(optionsRect.x, optionsRect.y, 24f, optionsRect.height);
            if (Widgets.ButtonImage(sortByRect, WealthNode.SortBy.GetIcon(), true, "VisibleWealth_SortBy".Translate(WealthNode.SortBy.GetLabel())))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (SortBy sortBy in typeof(SortBy).GetEnumValues())
                {
                    options.Add(new FloatMenuOption("VisibleWealth_SortBy".Translate(sortBy.GetLabel()), () =>
                    {
                        WealthNode.SortBy = sortBy;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            Rect sortDirectionRect = new Rect(optionsRect.x + 24f, optionsRect.y, 24f, optionsRect.height);
            if (Widgets.ButtonImage(sortDirectionRect, SortByUtility.SortDirectionIcon, true, WealthNode.SortAscending ? "VisibleWealth_SortDirectionAscending".Translate() : "VisibleWealth_SortDirectionDescending".Translate()))
            {
                WealthNode.SortAscending = !WealthNode.SortAscending;
                (WealthNode.SortAscending ? SoundDefOf.Tick_High : SoundDefOf.Tick_Low).PlayOneShot(null);
            }
            Rect sortDirectionCheckRect = new Rect(sortDirectionRect.x + sortDirectionRect.width / 2, sortDirectionRect.y, sortDirectionRect.width / 2, sortDirectionRect.height / 2);
            GUI.DrawTexture(sortDirectionCheckRect, WealthNode.SortAscending ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
        }
    }
}
