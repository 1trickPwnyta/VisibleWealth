using RimWorld;

namespace VisibleWealth
{
    public class MainButtonWorker_WealthBreakdown : MainButtonWorker
    {
        public override void Activate()
        {
            Dialog_WealthBreakdown.Open();
        }
    }
}
