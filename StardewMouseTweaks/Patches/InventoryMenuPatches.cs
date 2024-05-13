using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using StardewValley;
using StardewValley.Menus;

namespace StardewMouseTweaks.Patches;

[HarmonyPatch(typeof(InventoryMenu))]
public class InventoryMenuPatches
{
    [HarmonyPatch(nameof(InventoryMenu.rightClick))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> PatchRightClick(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher.Start();
        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))),
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Callvirt, AccessToolsPolyfill.DeclaredIndexerGetter(typeof(IList<Item>), [typeof(int)])),
            new CodeMatch(OpCodes.Ldarg_3),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Item), nameof(Item.canStackWith))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse)
        );

        var moveLabelsFrom = matcher.Instruction!;
        var deleteFromIndex = matcher.Pos;

        matcher.MatchStartForward(
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse)
        );

        var jumpToLabel = (Label)matcher.Operand;

        matcher.MatchStartForward(
            new CodeMatch(instr => instr.labels.Contains(jumpToLabel))
        );
        var deleteToIndex = matcher.Pos - 1;

        moveLabelsFrom.MoveLabelsTo(matcher.Instruction);
        matcher.RemoveInstructionsInRange(deleteFromIndex, deleteToIndex);

        return matcher.InstructionEnumeration();
    }
}
