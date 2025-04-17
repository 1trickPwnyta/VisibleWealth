using System.Runtime;
using Verse;

namespace VisibleWealth
{
    [StaticConstructorOnStartup]
    public static class VisibleWealthInitializer
    {
        static VisibleWealthInitializer()
        {
            VisibleWealthMod.Settings = VisibleWealthMod.Mod.GetSettings<VisibleWealthSettings>();
        }
    }
}
