using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace VisibleWealth
{
    [HarmonyPatch(typeof(GlobalControls))]
    [HarmonyPatch(nameof(GlobalControls.GlobalControlsOnGUI))]
    public static class Patch_GlobalControls
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();

            int index = instructionsList.FindIndex(i => i.Calls(typeof(GlobalControlsUtility).Method(nameof(GlobalControlsUtility.DoTimespeedControls))));
            instructionsList.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldc_R4, 200f),
                new CodeInstruction(OpCodes.Ldloca_S, 1),
                new CodeInstruction(OpCodes.Call, typeof(WealthOverlay).Method(nameof(WealthOverlay.DoGlobalControls)))
            });

            index = instructionsList.FindIndex(i => i.Calls(typeof(GlobalControlsUtility).Method(nameof(GlobalControlsUtility.DoDate))));
            instructionsList.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Call, typeof(WealthGlobalControl).Method(nameof(WealthGlobalControl.ForCurrentMap))),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldc_R4, 200f),
                new CodeInstruction(OpCodes.Ldloca_S, 1),
                new CodeInstruction(OpCodes.Call, typeof(WealthGlobalControl).Method(nameof(WealthGlobalControl.DoWealth)))
            });

            return instructionsList;
        }
    }
}
