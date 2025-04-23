using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VisibleWealth
{
    [StaticConstructorOnStartup]
    public class Dialog_WealthBreakdown : Window
    {
        private static readonly float chartWidth = 600f;
        private static Vector2 scrollPosition;
        private static float y;
        private static readonly ChartOption chartTypeOption = new ChartOption_Def<ChartDef>(() => VisibleWealthSettings.ChartType, option => VisibleWealthSettings.ChartType = option, option => option.Icon);
        public static QuickSearchWidget Search = new QuickSearchWidget();

        public static void Open()
        {
            LongEventHandler.QueueLongEvent(() => Find.WindowStack.Add(new Dialog_WealthBreakdown()), "VisibleWealth_Calculating", false, null);
        }

        private readonly Map map;
        private readonly WealthNode itemsNode;
        private readonly WealthNode buildingsNode;
        private readonly WealthNode pawnsNode;
        private readonly WealthNode pocketMapsNode;

        public override Vector2 InitialSize => new Vector2(chartWidth + 20f + StandardMargin * 2, 600f);

        private Dialog_WealthBreakdown()
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = true;
            absorbInputAroundWindow = true;

            WealthNode_Floor.TerrainCache.Clear();

            map = Find.CurrentMap;
            map.wealthWatcher.ForceRecount();
            itemsNode = new WealthNode_WealthCategory(null, map, 0, WealthCategory.Items);
            buildingsNode = new WealthNode_WealthCategory(null, map, 0, WealthCategory.Buildings);
            pawnsNode = new WealthNode_WealthCategory(null, map, 0, WealthCategory.Pawns);
            pocketMapsNode = new WealthNode_WealthCategory(null, map, 0, WealthCategory.PocketMaps);

            Search.Reset();
        }

        public override void PostOpen()
        {
            SoundDefOf.TabOpen.PlayOneShot(null);
        }

        public override void PostClose()
        {
            VisibleWealthMod.Settings.Write();
            ChartWorker.CleanupAll();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 45f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "VisibleWealth_WealthBreakdown".Translate());
            Text.Font = GameFont.Small;

            Rect totalWealthRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, 30f);
            Widgets.Label(totalWealthRect, "VisibleWealth_TotalWealth".Translate(map.wealthWatcher.WealthTotal.ToString("F0")));

            Rect searchRect = new Rect(inRect.xMax - QuickSearchSize.x, inRect.y + 45f, QuickSearchSize.x, QuickSearchSize.y);
            Search.OnGUI(searchRect);

            Rect outRect = new Rect(inRect.x, inRect.y + 45f + 30f + 10f, inRect.width, inRect.height - 45f - 30f - 10f - 24f - 10f - CloseButSize.y - 10f);
            Rect viewRect = new Rect(0f, 0f, chartWidth, y);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            y = 0f;
            IEnumerable<WealthNode> rootNodes = new[] { itemsNode, buildingsNode, pawnsNode, pocketMapsNode };
            VisibleWealthSettings.ChartType.Worker.Draw(outRect, viewRect, ref y, rootNodes);
            Vector2 mousePos = GUIUtility.ScreenToGUIPoint(UI.MousePositionOnUI);
            mousePos.y = outRect.height - mousePos.y;
            if (Mouse.IsOver(viewRect))
            {
                VisibleWealthSettings.ChartType.Worker.OnMouseOver(mousePos, outRect, viewRect, rootNodes);
            }
            if (Widgets.ButtonInvisible(viewRect, false))
            {
                VisibleWealthSettings.ChartType.Worker.OnClick(mousePos, outRect, viewRect, rootNodes);
            }
            Widgets.EndScrollView();

            Rect optionsRect = new Rect(inRect.x, inRect.yMax - CloseButSize.y - 10f - 24f, inRect.width, 24f);

            Rect chartTypeRect = new Rect(optionsRect.x, optionsRect.y, 24f, optionsRect.height);
            chartTypeOption.DoOption(chartTypeRect);

            Widgets.DrawLine(new Vector2(chartTypeRect.xMax + 12f, optionsRect.y), new Vector2(chartTypeRect.xMax + 12f, optionsRect.yMax), Color.gray, 1f);

            Rect optionRect = new Rect(optionsRect.x + 48f, optionsRect.y, 24f, optionsRect.height);
            foreach (ChartOption option in VisibleWealthSettings.ChartType.Worker.Options)
            {
                option.DoOption(optionRect);
                optionRect.x += optionRect.width;
            }
        }
    }
}
