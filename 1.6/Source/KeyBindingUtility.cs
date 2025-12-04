using UnityEngine;
using Verse;

namespace VisibleWealth
{
    [StaticConstructorOnStartup]
    public static class KeyBindingUtility
    {
        public static KeyBindingDef WealthBreakdown = KeyBindingDef.Named("VisibleWealth_WealthBreakdown");
        public static KeyBindingDef WealthBreakdownShift = KeyBindingDef.Named("VisibleWealth_WealthBreakdownShift");
        public static KeyBindingDef WealthOverlay = KeyBindingDef.Named("VisibleWealth_WealthOverlay");

        public static bool IsShiftHeld()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
    }
}
