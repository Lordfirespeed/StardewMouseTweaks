using JetBrains.Annotations;
using StardewModdingAPI;

namespace StardewMouseTweaks;

[UsedImplicitly]
internal sealed class Entrypoint : Mod
{
    private DragOperationManager _dragOperationManager = null!;

    /// <summary>
    /// Entrypoint.
    /// </summary>
    /// <seealso cref="StardewValley.Menus.InventoryMenu"/>
    /// <param name="helper"></param>
    public override void Entry(IModHelper helper)
    {
        _dragOperationManager = new DragOperationManager(helper, Monitor);
    }
}
