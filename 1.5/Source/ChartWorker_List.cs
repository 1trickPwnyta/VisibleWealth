using System.Collections.Generic;
using UnityEngine;

namespace VisibleWealth
{
    public class ChartWorker_List : ChartWorker
    {
        private static readonly IEnumerable<ChartOption> options = new ChartOption[]
        {
            new ChartOption_Enum<SortBy>(() => VisibleWealthSettings.SortBy, option => VisibleWealthSettings.SortBy = option, option => option.GetLabel(), option => option.GetIcon()), 
            new ChartOption_Toggle(() => VisibleWealthSettings.SortAscending, option => VisibleWealthSettings.SortAscending = option, option => option ? SortByUtility.SortDirectionAscending : SortByUtility.SortDirectionDescending, SortByUtility.SortDirectionIcon), 
            new ChartOption_Enum<PercentOf>(() => VisibleWealthSettings.PercentOf, option => VisibleWealthSettings.PercentOf = option, option => option.GetLabel(), option => option.GetIcon())
        };

        public override IEnumerable<ChartOption> Options => options;

        public override void Draw(Rect outRect, Rect viewRect, ref float y, IEnumerable<WealthNode> rootNodes)
        {
            foreach (WealthNode node in rootNodes)
            {
                node.Draw(viewRect.width, ref y);
            }
        }
    }
}
