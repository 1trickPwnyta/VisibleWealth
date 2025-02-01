using RimWorld;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class Dialog_WealthBreakdown : Window
    {
        public override Vector2 InitialSize => new Vector2(600f, 600f);

        public Dialog_WealthBreakdown() : base() 
        {
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = true;
            optionalTitle = "VisibleWealth_WealthBreakdown".Translate();
        }

        public override void DoWindowContents(Rect inRect)
        {
            
        }
    }
}
