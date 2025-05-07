using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class ChartWorker_List : ChartWorker
    {
        private static readonly IEnumerable<ChartOption> options = new ChartOption[]
        {
            new ChartOption_Enum<ListStyle>(() => VisibleWealthSettings.ListStyle, option => VisibleWealthSettings.ListStyle = option, "VisibleWealth_ListStyle".Translate(), option => option.GetLabel(), option => option.GetIcon()),
            ChartOption.CollapseAll, 
            new ChartOption_Enum<SortBy>(() => VisibleWealthSettings.SortBy, option => VisibleWealthSettings.SortBy = option, "VisibleWealth_SortBy".Translate(), option => option.GetLabel(), option => option.GetIcon()), 
            new ChartOption_Toggle(() => VisibleWealthSettings.SortAscending, option => VisibleWealthSettings.SortAscending = option, "VisibleWealth_SortDirectionAscending".Translate(), SortByUtility.SortDirectionIcon), 
            ChartOption.PercentOf,
            ChartOption.RaidPointMode
        };

        private Dictionary<long, IEnumerable<WealthNode>> cache = new Dictionary<long, IEnumerable<WealthNode>>();

        public override IEnumerable<ChartOption> Options => options;

        public override void Draw(Rect outRect, Rect viewRect, ref float y, IEnumerable<WealthNode> rootNodes)
        {
            if (VisibleWealthSettings.ListStyle == ListStyle.Nested)
            {
                foreach (WealthNode node in rootNodes)
                {
                    node.Draw(viewRect.width, ref y);
                }
            }
            else if (VisibleWealthSettings.ListStyle == ListStyle.Flat)
            {
                long state = GetState(rootNodes);
                IEnumerable<WealthNode> sortedNodes;
                if (!cache.ContainsKey(state))
                {
                    sortedNodes = VisibleWealthSettings.SortBy.Sorted(WealthNode.GetAllNodes(Dialog_WealthBreakdown.Current.rootNodes).Where(n => n.IsLeafNode), VisibleWealthSettings.SortAscending);
                    cache[state] = sortedNodes;
                }
                else
                {
                    sortedNodes = cache[state];
                }
                foreach (WealthNode node in sortedNodes)
                {
                    node.Draw(viewRect.width, ref y, false);
                }
            }
        }

        public override void Cleanup()
        {
            cache.Clear();
        }

        private static long GetState(IEnumerable<WealthNode> nodes) => nodes.Sum(n => GetNodeState(n)) + (VisibleWealthSettings.SortAscending ? 7903 : 146) + (int)VisibleWealthSettings.SortBy * 1685 + Dialog_WealthBreakdown.Search.filter.Text.GetHashCode() + (VisibleWealthSettings.RaidPointMode ? -2172368 : 123);

        private static long GetNodeState(WealthNode node)
        {
            long state = node.GetHashCode();
            foreach (WealthNode child in node.Children)
            {
                state += GetNodeState(child);
            }
            return state;
        }
    }
}
