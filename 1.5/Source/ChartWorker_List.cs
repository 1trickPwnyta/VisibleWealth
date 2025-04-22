using System;
using System.Collections.Generic;
using UnityEngine;

namespace VisibleWealth
{
    public class ChartWorker_List : ChartWorker
    {
        public override void Draw(Rect outRect, Rect viewRect, ref float y, IEnumerable<WealthNode> rootNodes)
        {
            foreach (WealthNode node in rootNodes)
            {
                node.Draw(viewRect.width, ref y);
            }
        }
    }
}
