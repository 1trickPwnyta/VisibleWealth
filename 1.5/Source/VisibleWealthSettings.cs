using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class VisibleWealthSettings : ModSettings
    {
        public static ChartDef ChartType = ChartDefOf.List;
        public static SortBy SortBy = SortBy.Value;
        public static bool SortAscending = false;
        public static PercentOf PercentOf = PercentOf.Category;
        public static PieStyle PieStyle = PieStyle.Nested;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            //listingStandard.CheckboxLabeled("IdeologyPatch_DisableHumanFoodPlantThought".Translate(), ref DisableHumanFoodPlantThought);

            listingStandard.End();
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref ChartType, "ChartType");
            Scribe_Values.Look(ref SortBy, "SortBy", SortBy.Value);
            Scribe_Values.Look(ref SortAscending, "SortAscending", false);
            Scribe_Values.Look(ref PercentOf, "PercentOf", PercentOf.Category);
            Scribe_Values.Look(ref PieStyle, "PieStyle", PieStyle.Nested);
        }
    }
}
