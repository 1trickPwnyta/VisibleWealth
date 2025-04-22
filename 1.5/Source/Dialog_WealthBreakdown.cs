using RimWorld;
using System;
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
            DoDefOption(chartTypeRect, VisibleWealthSettings.ChartType, c => c.Icon, c => VisibleWealthSettings.ChartType = c);

            Rect sortByRect = new Rect(optionsRect.x + 48f, optionsRect.y, 24f, optionsRect.height);
            DoEnumOption(sortByRect, VisibleWealthSettings.SortBy, s => s.GetIcon(), s => "VisibleWealth_SortBy".Translate(s.GetLabel()), s => VisibleWealthSettings.SortBy = s);

            Rect sortDirectionRect = new Rect(optionsRect.x + 48f + 24f, optionsRect.y, 24f, optionsRect.height);
            if (Widgets.ButtonImage(sortDirectionRect, SortByUtility.SortDirectionIcon, true, VisibleWealthSettings.SortAscending ? "VisibleWealth_SortDirectionAscending".Translate() : "VisibleWealth_SortDirectionDescending".Translate()))
            {
                VisibleWealthSettings.SortAscending = !VisibleWealthSettings.SortAscending;
                (VisibleWealthSettings.SortAscending ? SoundDefOf.Tick_High : SoundDefOf.Tick_Low).PlayOneShot(null);
            }
            Rect sortDirectionCheckRect = new Rect(sortDirectionRect.x + sortDirectionRect.width / 2, sortDirectionRect.y, sortDirectionRect.width / 2, sortDirectionRect.height / 2);
            GUI.DrawTexture(sortDirectionCheckRect, VisibleWealthSettings.SortAscending ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);

            Rect percentOfTotalRect = new Rect(optionsRect.x + 48f + 24f + 24f, optionsRect.y, 24f, optionsRect.height);
            DoEnumOption(percentOfTotalRect, VisibleWealthSettings.PercentOf, p => p.GetIcon(), p => p.GetLabel(), p => VisibleWealthSettings.PercentOf = p);
        }

        private void DoDefOption<T>(Rect rect, T option, Func<T, Texture2D> icon, Action<T> callback) where T : Def
        {
            if (Widgets.ButtonImage(rect, icon(option), true, option.LabelCap))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (T choice in DefDatabase<T>.AllDefsListForReading)
                {
                    options.Add(new FloatMenuOption(choice.LabelCap, () => callback(choice), icon(choice), Color.white));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        private void DoEnumOption<T>(Rect rect, T option, Func<T, Texture2D> icon, Func<T, string> label, Action<T> callback) where T : Enum
        {
            if (Widgets.ButtonImage(rect, icon(option), true, label(option)))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (T choice in typeof(T).GetEnumValues())
                {
                    options.Add(new FloatMenuOption(label(choice), () => callback(choice), icon(choice), Color.white));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
        }
    }
}
