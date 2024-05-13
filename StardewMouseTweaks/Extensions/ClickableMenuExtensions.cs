using System;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewValley.Menus;

namespace StardewMouseTweaks.Extensions;

public static class ClickableMenuExtensions
{
    public static bool TryGetChildMenu(this IClickableMenu menu, [MaybeNullWhen(false)] out IClickableMenu childMenu)
    {
        childMenu = menu.GetChildMenu();
        return childMenu != null;
    }

    public static bool TryGetParentMenu(this IClickableMenu menu, [MaybeNullWhen(false)] out IClickableMenu parentMenu)
    {
        parentMenu = menu.GetParentMenu();
        return parentMenu != null;
    }

    public static IClickableMenu GetMostDeeplyNestedChild(this IClickableMenu menu)
    {
        var current = menu;
        while (current.TryGetChildMenu(out var next)) {
            current = next;
        }
        return current;
    }

    public static bool IsWithinBounds(this IClickableMenu menu, ICursorPosition cursorPosition)
    {
        return menu.isWithinBounds((int)cursorPosition.ScreenPixels.X, (int)cursorPosition.ScreenPixels.Y);
    }
}
