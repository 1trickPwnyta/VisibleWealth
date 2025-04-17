using HarmonyLib;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class VisibleWealthMod : Mod
    {
        public const string PACKAGE_ID = "visiblewealth.1trickPwnyta";
        public const string PACKAGE_NAME = "Visible Wealth";

        public static VisibleWealthMod Mod;
        public static VisibleWealthSettings Settings;

        public VisibleWealthMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Mod = this;

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }

        public override string SettingsCategory() => PACKAGE_NAME;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            VisibleWealthSettings.DoSettingsWindowContents(inRect);
        }
    }
}
