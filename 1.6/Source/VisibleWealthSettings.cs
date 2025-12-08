using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
        public static bool PlaySettingsButton = false;
        public static bool MainButton = false;
        public static bool WealthOverlay = true;
        public static bool WealthOverlayPause = true;
        public static bool WealthGlobalControl = true;
        public static Color WealthGlobalControlColor = Color.clear;
        public static int WealthGlobalControlCacheTicks = 5000;

        private static readonly MainButtonDef mainButtonDef = DefDatabase<MainButtonDef>.GetNamed("VisibleWealth_MainButton");
        private static string wealthGlobalControlCacheTicksEditBuffer;
        private static readonly List<Color> wealthColorOptions = Enumerable.Range(0, 32).Select(i => Color.HSVToRGB(i / 32f, 0.7f, 0.9f)).Prepend(ColoredText.CurrencyColor).Prepend(Color.clear).ToList();

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();

            listing.Begin(inRect);

            listing.CheckboxLabeled("VisibleWealth_PlaySettingsButton".Translate(), ref PlaySettingsButton);
            listing.CheckboxLabeled("VisibleWealth_MainButton".Translate(), ref MainButton);
            mainButtonDef.buttonVisible = MainButton;

            listing.Gap(24f);

            listing.CheckboxLabeled("VisibleWealth_WealthGlobalControl".Translate(), ref WealthGlobalControl);
            if (WealthGlobalControl)
            {
                Rect colorRect = listing.GetRect(30f);
                string colorText = "VisibleWealth_WealthGlobalControlColor".Translate();
                using (new TextBlock(TextAnchor.MiddleLeft)) Widgets.Label(colorRect, colorText);
                colorRect.xMin += Text.CalcSize(colorText).x;
                string defaultText = "Default".Translate();
                if (WealthGlobalControlColor == Color.clear)
                {
                    colorRect.width = Text.CalcSize(defaultText).x + 6f;
                    using (new TextBlock(TextAnchor.MiddleCenter)) Widgets.Label(colorRect, defaultText);
                    Widgets.DrawHighlightIfMouseover(colorRect);
                }
                else
                {
                    colorRect.width = colorRect.height;
                    Widgets.DrawRectFast(colorRect, WealthGlobalControlColor);
                }
                if (Widgets.ButtonInvisible(colorRect))
                {
                    Find.WindowStack.Add(new Dialog_ChooseColor("VisibleWealth_WealthGlobalControlColorTitle".Translate(), WealthGlobalControlColor, wealthColorOptions, c =>
                    {
                        WealthGlobalControlColor = c;
                    }));
                }

                Rect cacheRect = listing.GetRect(30f);
                Widgets.Label(cacheRect.LeftHalf(), "VisibleWealth_WealthGlobalControlCacheTicks".Translate());
                Widgets.IntEntry(cacheRect.RightHalf(), ref WealthGlobalControlCacheTicks, ref wealthGlobalControlCacheTicksEditBuffer, 100);
                if (WealthGlobalControlCacheTicks < 60)
                {
                    WealthGlobalControlCacheTicks = 60;
                    wealthGlobalControlCacheTicksEditBuffer = WealthGlobalControlCacheTicks.ToString();
                }
            }

            listing.Gap(24f);

            listing.CheckboxLabeled("VisibleWealth_WealthOverlay".Translate(), ref WealthOverlay);
            listing.CheckboxLabeled("VisibleWealth_WealthOverlayPause".Translate(), ref WealthOverlayPause, "VisibleWealth_WealthOverlayPauseTip".Translate());

            listing.Gap(24f);
            
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
            Scribe_Values.Look(ref WealthGlobalControl, "WealthGlobalControl", true);
            Scribe_Values.Look(ref WealthGlobalControlColor, "WealthGlobalControlColor", Color.clear);
            Scribe_Values.Look(ref WealthGlobalControlCacheTicks, "WealthGlobalControlCacheTicks", 5000);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                mainButtonDef.buttonVisible = MainButton;
            }
        }
    }
}
