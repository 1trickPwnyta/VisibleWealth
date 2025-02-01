using HarmonyLib;
using Verse;

namespace VisibleWealth
{
    public class VisibleWealthMod : Mod
    {
        public const string PACKAGE_ID = "visiblewealth.1trickPwnyta";
        public const string PACKAGE_NAME = "Visible Wealth";

        public VisibleWealthMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}
