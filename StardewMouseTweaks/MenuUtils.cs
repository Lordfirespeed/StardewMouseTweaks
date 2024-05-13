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
}
