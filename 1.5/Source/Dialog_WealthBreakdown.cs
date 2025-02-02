using RimWorld;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class Dialog_WealthBreakdown : Window
    {
        private static Vector2 scrollPosition;
        private static float y;

        private WealthNode itemsNode;
        private WealthNode buildingsNode;
        private WealthNode pawnsNode;

        public override Vector2 InitialSize => new Vector2(WealthNode.Size.x + 20f + Window.StandardMargin * 2, 600f);

        public Dialog_WealthBreakdown() : base() 
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = true;

            Map map = Find.CurrentMap;
            itemsNode = new WealthNode_WealthCategory(map, 0, WealthCategory.Items);
            //buildingsNode = new WealthNode(map);
            //pawnsNode = new WealthNode(map);
        }

        private float GetTotalValue()
        {
            return itemsNode.Value;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 45f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "VisibleWealth_WealthBreakdown".Translate());
            Text.Font = GameFont.Small;

            Rect totalWealthRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, 30f);
            Widgets.Label(totalWealthRect, "VisibleWealth_TotalWealth".Translate(GetTotalValue().ToString("F0")));

            Rect outRect = new Rect(inRect.x, inRect.y + 45f + 30f + 10f, inRect.width, inRect.height - 45f - 30f - 10f - Window.CloseButSize.y);
            Rect viewRect = new Rect(0f, 0f, WealthNode.Size.x, y);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            y = 0f;

            itemsNode.Draw(ref y);
            //buildingsNode.Draw(ref y);
            //pawnsNode.Draw(ref y);

            Widgets.EndScrollView();
        }
    }
}
