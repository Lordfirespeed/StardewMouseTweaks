using System;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewMouseTweaks.Extensions;
using StardewValley;
using StardewValley.Menus;

namespace StardewMouseTweaks;

public static class MenuUtils
{
    public static IClickableMenu? GetCurrentActiveMenu()
    {
        IClickableMenu? candidate = Game1.activeClickableMenu;
        if (candidate is null) return null;
        if (!candidate.IsActive()) return null;
        return candidate.GetMostDeeplyNestedChild();
    }

    public static bool TryGetCurrentActiveMenu([MaybeNullWhen(false)] out IClickableMenu menu)
    {
        menu = GetCurrentActiveMenu();
        return menu != null;
    }

    public static bool ClickableMenusCanReceivePrimaryButtonPresses()
    {
        if (Game1.panMode) return false;
        if (Game1.player.usingSlingshot) return false;
        if (!Game1.wasMouseVisibleThisFrame) return false;
        if (!Game1.displayHUD) return false;
        if (Game1.paused || Game1.HostPaused) return false;
        if (Game1.IsChatting) return false;
        return true;
    }

    public static bool ClickableMenusCanReceiveSecondaryButtonPresses()
    {
        if (!ClickableMenusCanReceivePrimaryButtonPresses()) return false;
        if (Game1.player.freezePause > 0) return false;
        return true;
    }

    public static bool TryGetHoveredClickableMenu(ICursorPosition cursorPosition, [MaybeNullWhen(false)] out IClickableMenu menu)
    {
        menu = null;
        if (!Game1.wasMouseVisibleThisFrame) return false;
        if (TryGetCurrentActiveMenu(out menu)) return true;

        foreach (var onScreenMenu in Game1.onScreenMenus) {
            if (onScreenMenu is LevelUpMenu { informationUp: false }) continue;
            if (!onScreenMenu.IsWithinBounds(cursorPosition)) continue;

            if (ReferenceEquals(onScreenMenu, Game1.chatBox)) {
                menu = onScreenMenu;
                return true;
            }

            if (Game1.displayHUD) {
                menu = onScreenMenu;
                return true;
            }
        }

        return false;
    }

    public static bool TryGetHoveredInventoryMenu(ICursorPosition cursorPosition, [MaybeNullWhen(false)] out InventoryMenu menu)
    {
        menu = null;
        if (!TryGetHoveredClickableMenu(cursorPosition, out var clickableMenu)) return false;

        if (clickableMenu is GameMenu gameMenu ) {
            if (gameMenu.currentTab == GameMenu.inventoryTab) {
                menu = (gameMenu.pages[GameMenu.inventoryTab] as InventoryPage)!.inventory;
                return true;
            }

            if (gameMenu.currentTab == GameMenu.craftingTab) {
                menu = (gameMenu.pages[GameMenu.craftingTab] as CraftingPage)!.inventory;
                return true;
            }

            return false;
        }

        if (clickableMenu is MenuWithInventory menuWithInventory) {
            if (menuWithInventory.inventory.IsWithinBounds(cursorPosition)) {
                menu = menuWithInventory.inventory;
                return true;
            }

            if (menuWithInventory is ItemGrabMenu itemGrabMenu && itemGrabMenu.ItemsToGrabMenu.IsWithinBounds(cursorPosition)) {
                menu = itemGrabMenu.ItemsToGrabMenu;
                return true;
            }

            if (menuWithInventory is QuestContainerMenu questContainerMenu && questContainerMenu.ItemsToGrabMenu.IsWithinBounds(cursorPosition)) {
                menu = questContainerMenu.ItemsToGrabMenu;
                return true;
            }

            return false;
        }

        if (clickableMenu is JunimoNoteMenu junimoNoteMenu && junimoNoteMenu.inventory.IsWithinBounds(cursorPosition)) {
            menu = junimoNoteMenu.inventory;
            return true;
        }

        return false;
    }

    public static Item? CursorSlotItem {
        get {
            if (!TryGetCurrentActiveMenu(out var menu))
                return Game1.player.CursorSlotItem;

            if (menu is GameMenu gameMenu ) {
                if (gameMenu.currentTab == GameMenu.inventoryTab)
                    return Game1.player.CursorSlotItem;

                if (gameMenu.currentTab == GameMenu.craftingTab)
                    return (gameMenu.pages[GameMenu.craftingTab] as CraftingPage)!.heldItem;

                throw new InvalidOperationException();
            }

            if (menu is MenuWithInventory menuWithInventory) {
                return menuWithInventory.heldItem;
            }

            if (menu is JunimoNoteMenu junimoNoteMenu) {
                return junimoNoteMenu.heldItem;
            }

            return Game1.player.CursorSlotItem;
        }
        set {
            if (!TryGetCurrentActiveMenu(out var menu)) {
                Game1.player.CursorSlotItem = value;
                return;
            }

            if (menu is GameMenu gameMenu) {
                if (gameMenu.currentTab == GameMenu.inventoryTab) {
                    Game1.player.CursorSlotItem = value;
                    return;
                }

                if (gameMenu.currentTab == GameMenu.craftingTab) {
                    (gameMenu.pages[GameMenu.craftingTab] as CraftingPage)!.heldItem = value;
                    return;
                }

                throw new InvalidOperationException();
            }

            if (menu is MenuWithInventory menuWithInventory) {
                menuWithInventory.heldItem = value;
                return;
            }

            if (menu is JunimoNoteMenu junimoNoteMenu) {
                junimoNoteMenu.heldItem = value;
                return;
            }

            Game1.player.CursorSlotItem = value;
        }
    }
}
