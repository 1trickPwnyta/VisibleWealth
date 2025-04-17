using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public abstract class ChartWorker
    {
        public abstract void Draw(Rect outRect, Rect viewRect, ref float y, IEnumerable<WealthNode> rootNodes);

        public abstract void OnClick(Vector2 pos);

        public abstract void Cleanup();

        public static void CleanupAll()
        {
            foreach (ChartDef def in DefDatabase<ChartDef>.AllDefsListForReading)
            {
                def.Worker.Cleanup();
            }
        }
    }
}
