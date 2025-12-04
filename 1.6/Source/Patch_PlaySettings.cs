using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PlaySettings))]
    [HarmonyPatch("DoMapControls")]
    public static class Patch_PlaySettings
    {
        private static Texture2D WealthBreakdownIcon = ContentFinder<Texture2D>.Get("UI/Buttons/WealthBreakdown");
        private static Texture2D WealthOverlayIcon = ContentFinder<Texture2D>.Get("UI/Buttons/WealthOverlay");

        public static void Postfix(WidgetRow row)
        {
            if (VisibleWealthSettings.PlaySettingsButton)
            {
                string keyCodeText = "";
                KeyCode keyCode = KeyPrefs.KeyPrefsData.GetBoundKeyCode(KeyBindingUtility.WealthBreakdown, KeyPrefs.BindingSlot.A);
                KeyCode keyCodeShift = KeyPrefs.KeyPrefsData.GetBoundKeyCode(KeyBindingUtility.WealthBreakdownShift, KeyPrefs.BindingSlot.A);
                if (keyCode != KeyCode.None)
                {
                    keyCodeText += "HotKeyTip".Translate() + ": " + keyCode.ToStringReadable() + "\n\n";
                }
                else if (keyCodeShift != KeyCode.None)
                {
                    keyCodeText += "HotKeyTip".Translate() + ": " + KeyCode.LeftShift.ToStringReadable() + " + " + keyCodeShift.ToStringReadable() + "\n\n";
                }

                if (row.ButtonIcon(WealthBreakdownIcon, keyCodeText + "VisibleWealth_WealthBreakdown".Translate()))
                {
                    Dialog_WealthBreakdown.Open();
                }
            }

            if (VisibleWealthSettings.WealthOverlay)
            {
                bool visible = WealthOverlay.Visible;
                row.ToggleableIcon(ref visible, WealthOverlayIcon, "VisibleWealth_WealthOverlayTip".Translate(), SoundDefOf.Mouseover_ButtonToggle);
                if (visible != WealthOverlay.Visible)
                {
                    WealthOverlay.Visible = visible;
                }
            }
        }
    }
}
