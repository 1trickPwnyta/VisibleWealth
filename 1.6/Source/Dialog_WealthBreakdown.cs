using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly ChartOption chartTypeOption = new ChartOption_Def<ChartDef>(() => VisibleWealthSettings.ChartType, option => VisibleWealthSettings.ChartType = option, null, option => option.Icon);
        public static QuickSearchWidget Search = new QuickSearchWidget();

        public static void Open()
        {
            if (Current == null)
            {
                LongEventHandler.QueueLongEvent(() =>
                {
                    if (Current == null)
                    {
                        Find.WindowStack.Add(new Dialog_WealthBreakdown());
                    }
                }, "VisibleWealth_Calculating", false, null);
            }
        }

        public static Dialog_WealthBreakdown Current { get; private set; }

        private readonly Map map;
        public readonly List<WealthNode> rootNodes;

        public float TotalWealth => VisibleWealthSettings.RaidPointMode ? map.wealthWatcher.WealthTotal - map.wealthWatcher.WealthBuildings / 2f : map.wealthWatcher.WealthTotal;

        public override Vector2 InitialSize => new Vector2(chartWidth + 20f + StandardMargin * 2, 600f);

        private Dialog_WealthBreakdown()
        {
            Current = this;

            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = true;
            absorbInputAroundWindow = true;

            WealthNode_Floor.TerrainCache.Clear();

            map = Find.CurrentMap;
            map.wealthWatcher.ForceRecount();
            rootNodes = WealthNode.MakeRootNodes(map).ToList();

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
            Current = null;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Current == this)
            {
                Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 45f);
                Text.Font = GameFont.Medium;
                Widgets.Label(titleRect, "VisibleWealth_WealthBreakdown".Translate());
                Text.Font = GameFont.Small;

                Rect totalWealthRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, 30f);
                Widgets.Label(totalWealthRect, "VisibleWealth_TotalWealth".Translate(TotalWealth.ToString("F0")));

                Rect searchRect = new Rect(inRect.xMax - QuickSearchSize.x, inRect.y + 45f, QuickSearchSize.x, QuickSearchSize.y);
                Search.OnGUI(searchRect);

                Rect outRect = new Rect(inRect.x, inRect.y + 45f + 30f + 10f, inRect.width, inRect.height - 45f - 30f - 10f - 24f - 10f - CloseButSize.y - 10f);
                Rect viewRect = new Rect(0f, 0f, chartWidth, y);
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
                y = 0f;
                VisibleWealthSettings.ChartType.Worker.Draw(outRect, viewRect, ref y, rootNodes);
                Vector2 mousePos = GUIUtility.ScreenToGUIPoint(Input.mousePosition);
                mousePos.y = outRect.height - mousePos.y;
                if (Mouse.IsOver(viewRect))
                {
                    VisibleWealthSettings.ChartType.Worker.OnMouseOver(mousePos, outRect, viewRect, rootNodes);
                }
                else
                {
                    VisibleWealthSettings.ChartType.Worker.OnMouseNotOver(rootNodes);
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
}
