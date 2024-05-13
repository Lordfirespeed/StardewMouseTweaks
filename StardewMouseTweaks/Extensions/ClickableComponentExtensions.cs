using StardewModdingAPI;
using StardewValley.Menus;

namespace StardewMouseTweaks.Extensions;

public static class ClickableComponentExtensions
{
    public static bool ContainsPoint(this ClickableComponent component, ICursorPosition position)
    {
        return component.containsPoint((int)position.ScreenPixels.X, (int)position.ScreenPixels.Y);
    }
}
