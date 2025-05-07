using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public abstract class ChartWorker
    {
        public abstract void Draw(Rect outRect, Rect viewRect, ref float y, IEnumerable<WealthNode> rootNodes);

        public virtual IEnumerable<ChartOption> Options => new ChartOption[0];

        public virtual void OnMouseOver(Vector2 pos, Rect outRect, Rect viewRect, IEnumerable<WealthNode> rootNodes)
        {

        }

        public virtual void OnMouseNotOver(IEnumerable<WealthNode> rootNodes)
        {

        }

        public virtual void OnClick(Vector2 pos, Rect outRect, Rect viewRect, IEnumerable<WealthNode> rootNodes)
        {

        }

        public virtual void Cleanup()
        {

        }

        public static void CleanupAll()
        {
            foreach (ChartDef def in DefDatabase<ChartDef>.AllDefsListForReading)
            {
                def.Worker.Cleanup();
            }
        }
    }
}
