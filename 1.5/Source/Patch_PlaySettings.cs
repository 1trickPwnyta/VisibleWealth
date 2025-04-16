using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PlaySettings))]
    [HarmonyPatch(nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    public static class Patch_PlaySettings
    {
        private static Texture2D WealthBreakdownIcon = ContentFinder<Texture2D>.Get("UI/Buttons/WealthBreakdown");

        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (!worldView)
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
        }
    }
}
