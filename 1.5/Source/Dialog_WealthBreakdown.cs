using RimWorld;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class Dialog_WealthBreakdown : Window
    {
        private static Vector2 scrollPosition;
        private static float y;
        public static QuickSearchWidget Search = new QuickSearchWidget();

        private readonly WealthNode itemsNode;
        private readonly WealthNode buildingsNode;
        private readonly WealthNode pawnsNode;

        public override Vector2 InitialSize => new Vector2(WealthNode.Size.x + 20f + Window.StandardMargin * 2, 600f);

        public Dialog_WealthBreakdown() : base() 
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = true;

            Map map = Find.CurrentMap;
            itemsNode = new WealthNode_WealthCategory(map, 0, WealthCategory.Items);
            buildingsNode = new WealthNode_WealthCategory(map, 0, WealthCategory.Buildings);
            //pawnsNode = new WealthNode(map);

            Search.Reset();
            Search.Focus();
        }

        private float GetTotalValue()
        {
            return itemsNode.Value + buildingsNode.Value;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 45f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "VisibleWealth_WealthBreakdown".Translate());
            Text.Font = GameFont.Small;

            Rect totalWealthRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, 30f);
            Widgets.Label(totalWealthRect, "VisibleWealth_TotalWealth".Translate(GetTotalValue().ToString("F0")));

            Rect searchRect = new Rect(inRect.xMax - Window.QuickSearchSize.x, inRect.y + 45f, Window.QuickSearchSize.x, Window.QuickSearchSize.y);
            Search.OnGUI(searchRect);

            Rect outRect = new Rect(inRect.x, inRect.y + 45f + 30f + 10f, inRect.width, inRect.height - 45f - 30f - 10f - Window.CloseButSize.y);
            Rect viewRect = new Rect(0f, 0f, WealthNode.Size.x, y);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            y = 0f;

            itemsNode.Draw(ref y);
            buildingsNode.Draw(ref y);
            //pawnsNode.Draw(ref y);

            Widgets.EndScrollView();
        }
    }
}
