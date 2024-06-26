using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewMouseTweaks.Extensions;
using StardewValley;
using StardewValley.Menus;
using Rectangle = System.Drawing.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace StardewMouseTweaks.Patches;

[HarmonyPatch(typeof(ItemGrabMenu))]
public class ItemGrabMenuPatches : IPatch
{
    private static IModHelper _helper = null!;
    private static IMonitor _monitor = null!;

    public void Initialize(IModHelper helper, IMonitor monitor)
    {
        _helper = helper;
        _monitor = monitor;
    }

    #region 'drawing' patches

    [HarmonyPatch(nameof(ItemGrabMenu.draw))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> PatchDrawShippingBin(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.shippingBin))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse),
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Game1), nameof(Game1.getFarm))),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Farm), nameof(Farm.lastItemShipped))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse)
        );
        matcher.Advance(3);
        matcher.RemoveInstructions(3);

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Game1), nameof(Game1.getFarm))),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Farm), nameof(Farm.lastItemShipped))),
            new CodeMatch(OpCodes.Ldarg_1),

            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.lastShippedHolder))),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(ClickableComponent), nameof(ClickableComponent.bounds))),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Rectangle), nameof(Rectangle.X))),
            new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)16),
            new CodeMatch(OpCodes.Add),
            new CodeMatch(OpCodes.Conv_R4),

            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.lastShippedHolder))),
            new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(ClickableComponent), nameof(ClickableComponent.bounds))),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Rectangle), nameof(Rectangle.Y))),
            new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)16),
            new CodeMatch(OpCodes.Add),
            new CodeMatch(OpCodes.Conv_R4),

            new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(Vector2), [typeof(float), typeof(float)])),
            new CodeMatch(OpCodes.Ldc_R4, 1f),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Item), nameof(Item.drawInMenu), [typeof(SpriteBatch), typeof(Vector2), typeof(float)]))
        );

        var label = generator.DefineLabel();

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Game1), nameof(Game1.getFarm))),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Farm), nameof(Farm.lastItemShipped))),
            new CodeInstruction(OpCodes.Brfalse, label)
        );
        matcher.Advance(20);
        matcher.Labels.Add(label);

        return matcher.InstructionEnumeration();
    }

    #endregion

    #region left-click patches

    public static void LeftClickShippingBinSlot(ItemGrabMenu menu)
    {
        if (menu.heldItem is null) TakeItemFromSlot();
        else PlaceItemInSlot();
        return;

        void PlaceItemInSlot()
        {
            menu.behaviorFunction(menu.heldItem, Game1.player);
            menu.heldItem = null;
        }

        void TakeItemFromSlot()
        {
            if (Game1.getFarm().lastItemShipped is not { } lastItemShipped)
                return;

            Game1.getFarm().getShippingBin(Game1.player).Remove(lastItemShipped);

            if (_helper.Input.IsDown(SButton.LeftShift)) {
                if (!Game1.player.addItemToInventoryBool(lastItemShipped)) {
                    Game1.getFarm().getShippingBin(Game1.player).Add(lastItemShipped);
                    return;
                }

                Game1.playSound("coin");
                Game1.getFarm().lastItemShipped = null;
                if (Game1.player.ActiveObject == null)
                    return;
                Game1.player.showCarrying();
                Game1.player.Halt();
                return;
            }

            Game1.playSound("coin");
            Game1.getFarm().lastItemShipped = null;
            menu.heldItem = lastItemShipped;
        }
    }

    /**
     * Changes behaviour when left-clicking on a shipping bin slot.
     * Usually <c>player.addItemToInventoryBool</c> is attempted immediately, and <c>menu.heldItem</c>
     * is assigned <c>null</c>.
     * This patch invokes <c>player.addItemToInventoryBool</c> only when the shift key is held, and <c>menu.heldItem</c>
     * persists because shipping menus are not 'swapped' when inventories are updated.
     */
    [HarmonyPatch(nameof(ItemGrabMenu.receiveLeftClick))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> PatchLeftClickShippingBinSlot(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions);

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.shippingBin))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.lastShippedHolder))),
            new CodeMatch(OpCodes.Ldarg_1),
            new CodeMatch(OpCodes.Ldarg_2),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ClickableComponent), nameof(ClickableComponent.containsPoint), [typeof(int), typeof(int)])),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse)
        );

        var jumpToLabel = (Label)matcher.Operand;
        matcher.Advance(1);

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ItemGrabMenuPatches), nameof(LeftClickShippingBinSlot))),
            new CodeInstruction(OpCodes.Ret)
        );

        var deleteFromIndex = matcher.Pos;
        matcher.MatchStartForward(
            new CodeMatch(instr => instr.labels.Contains(jumpToLabel))
        );
        matcher.RemoveInstructionsInRange(deleteFromIndex, matcher.Pos - 1);

        return matcher.InstructionEnumeration();
    }

    public static void LeftClickReceivingMenu(ItemGrabMenu menu)
    {
        Debug.Assert(menu.showReceivingMenu);

        if (_helper.Input.IsDown(SButton.LeftShift)) {
            if (!Game1.player.addItemToInventoryBool(menu.heldItem)) return;

            menu.heldItem = null;
            Game1.playSound("coin");
        }
    }

    /**
     * Changes behaviour when left-clicking on an item in the 'receiving menu' (e.g. a chest's inventory).
     * Usually <c>player.addItemToInventoryBool</c> is invoked immediately and the menu is
     * 'swapped' to a fresh instance with `menu.heldItem` <c>null</c>.
     * This patch invokes <c>player.addItemToInventoryBool</c> only when the shift key is held, and an event handler
     * persists <c>menu.heldItem</c> across the 'swap'.
     * <seealso cref="DragOperationManager.OnMenuChanged"/>
     */
    [HarmonyPatch(nameof(ItemGrabMenu.receiveLeftClick))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> PatchLeftClickReceivingMenu(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(MenuWithInventory), nameof(MenuWithInventory.heldItem))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brtrue),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse)
        );
        if (matcher.IsInvalid) throw new InvalidOperationException();

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(MenuWithInventory), nameof(MenuWithInventory.heldItem))),
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse)
        );
        if (matcher.IsInvalid) throw new InvalidOperationException();

        var labelsToMove = matcher.Labels;

        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldarg_0) { labels = labelsToMove },
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ItemGrabMenuPatches), nameof(LeftClickReceivingMenu)))
        );

        var deleteFromIndex = matcher.Pos;
        matcher.MatchStartForward(
            new CodeMatch(instr => instr.opcode == OpCodes.Br_S)
        );
        if (matcher.IsInvalid) throw new InvalidOperationException();
        matcher.RemoveInstructionsInRange(deleteFromIndex, matcher.Pos - 1);

        _monitor.LogInstructionsFrom(matcher, LogLevel.Info);

        return matcher.InstructionEnumeration();
    }

    public static void LeftClickInventory(ItemGrabMenu menu)
    {
        if (_helper.Input.IsDown(SButton.LeftShift)) {
            menu.behaviorFunction(menu.heldItem, Game1.player);
            menu.heldItem = null;
        }
    }

    /**
     * Changes behaviour when left-clicking on an item in the player's inventory.
     * Usually <c>menu.BehaviourFunction</c> is invoked immediately, `menu.heldItem` is <b>not</b> set to <c>null</c>, and the menu is
     * 'swapped' to a fresh instance with `menu.heldItem` <c>null</c>.
     * This patch invokes <c>menu.BehaviourFunction</c> only when the shift key is held, and an event handler
     * persists <c>menu.heldItem</c> across the 'swap'.
     * <seealso cref="DragOperationManager.OnMenuChanged"/>
     */
    [HarmonyPatch(nameof(ItemGrabMenu.receiveLeftClick))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> PatchLeftClickInventory(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        _monitor.LogInstructionsFrom(matcher, LogLevel.Info);

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(MenuWithInventory), nameof(MenuWithInventory.heldItem))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brtrue),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse)
        );
        if (matcher.IsInvalid) throw new InvalidOperationException();

        matcher.MatchEndForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.reverseGrab))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brtrue_S),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.behaviorFunction))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldarg_1),
            new CodeMatch(OpCodes.Ldarg_2),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.isWithinBounds))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse_S)
        );
        if (matcher.IsInvalid) throw new InvalidOperationException();

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.behaviorFunction))),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(MenuWithInventory), nameof(MenuWithInventory.heldItem))),
            new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ItemGrabMenu.behaviorOnItemSelect), nameof(ItemGrabMenu.behaviorOnItemSelect.Invoke)))
        );
        if (matcher.IsInvalid) throw new InvalidOperationException();

        matcher.Advance(1);
        matcher.InsertAndAdvance(
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ItemGrabMenuPatches), nameof(LeftClickInventory)))
        );
        matcher.RemoveInstructions(5);

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.destroyItemOnClick))),
            new CodeMatch(instr => instr.opcode == OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldnull),
            new CodeMatch(OpCodes.Call, AccessTools.PropertySetter(typeof(MenuWithInventory), nameof(MenuWithInventory.heldItem))),
            new CodeMatch(OpCodes.Ret)
        );
        matcher.SetAndAdvance(OpCodes.Nop, null);
        matcher.RemoveInstructions(6);

        return matcher.InstructionEnumeration();
    }

    #endregion

    #region right-click patches

    public static void RightClickReceivingMenu(ItemGrabMenu menu)
    {
        Debug.Assert(menu.showReceivingMenu);
    }

    [HarmonyPatch(nameof(ItemGrabMenu.receiveRightClick))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> PatchRightClickReceivingMenu(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        return matcher.InstructionEnumeration();
    }

    public static void RightClickInventory(ItemGrabMenu menu)
    {

    }

    [HarmonyPatch(nameof(ItemGrabMenu.receiveRightClick))]
    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> PatchRightClickInventory(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        return matcher.InstructionEnumeration();
    }

    #endregion
}
