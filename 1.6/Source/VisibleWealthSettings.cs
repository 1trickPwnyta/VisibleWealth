using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class VisibleWealthSettings : ModSettings
    {
        public static ChartDef ChartType = ChartDefOf.List;
        public static SortBy SortBy = SortBy.Value;
        public static bool SortAscending = false;
        public static PercentOf PercentOf = PercentOf.Total;
        public static ListStyle ListStyle = ListStyle.Nested;
        public static PieStyle PieStyle = PieStyle.Nested;
        public static bool RaidPointMode = false;
        public static bool PlaySettingsButton = true;
        public static bool MainButton = false;
        public static bool WealthOverlay = true;
        public static bool WealthOverlayPause = true;

        private static readonly MainButtonDef mainButtonDef = DefDatabase<MainButtonDef>.GetNamed("VisibleWealth_MainButton");

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();

            listing.Begin(inRect);

            listing.CheckboxLabeled("VisibleWealth_PlaySettingsButton".Translate(), ref PlaySettingsButton);
            listing.CheckboxLabeled("VisibleWealth_MainButton".Translate(), ref MainButton);
            mainButtonDef.buttonVisible = MainButton;

            listing.Gap();

            listing.CheckboxLabeled("VisibleWealth_WealthOverlayPause".Translate(), ref WealthOverlayPause, "VisibleWealth_WealthOverlayPauseTip".Translate());
            listing.CheckboxLabeled("VisibleWealth_WealthOverlay".Translate(), ref WealthOverlay);

            listing.Gap();
            
            if (listing.ButtonText("VisibleWealth_OpenKeyBindings".Translate(), null, 0.35f))
            {
                Dialog_KeyBindings dialog = new Dialog_KeyBindings();
                typeof(Dialog_KeyBindings).Field("scrollPosition").SetValue(dialog, new Vector2(0f, GetKeyBindingScrollPosition()));
                Find.WindowStack.Add(dialog);
            }

            listing.End();
        }

        private static float GetKeyBindingScrollPosition()
        {
            float scrollPosition = 0f;
            KeyBindingCategoryDef keyBindingCategoryDef = null;
            foreach (KeyBindingDef keyBindingDef in DefDatabase<KeyBindingDef>.AllDefs)
            {
                if (keyBindingCategoryDef != keyBindingDef.category)
                {
                    keyBindingCategoryDef = keyBindingDef.category;
                    if (keyBindingCategoryDef.defName == "VisibleWealth")
                    {
                        break;
                    }
                    scrollPosition += 44f;
                }
                scrollPosition += 34f;
            }
            return scrollPosition;
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref ChartType, "ChartType");
            Scribe_Values.Look(ref SortBy, "SortBy", SortBy.Value);
            Scribe_Values.Look(ref SortAscending, "SortAscending", false);
            Scribe_Values.Look(ref PercentOf, "PercentOf", PercentOf.Total);
            Scribe_Values.Look(ref ListStyle, "ListStyle", ListStyle.Nested);
            Scribe_Values.Look(ref PieStyle, "PieStyle", PieStyle.Nested);
            Scribe_Values.Look(ref RaidPointMode, "RaidPointMode", false);
            Scribe_Values.Look(ref PlaySettingsButton, "PlaySettingsButton", true);
            Scribe_Values.Look(ref MainButton, "MainButton", false);
            Scribe_Values.Look(ref WealthOverlay, "WealthOverlay", true);
            Scribe_Values.Look(ref WealthOverlayPause, "WealthOverlayPause", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                mainButtonDef.buttonVisible = MainButton;
            }
        }
    }
}
