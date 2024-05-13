using JetBrains.Annotations;
using StardewModdingAPI;
using HarmonyLib;
using StardewMouseTweaks.Patches;

namespace StardewMouseTweaks;

[UsedImplicitly]
internal sealed class Entrypoint : Mod
{
    internal static new IMonitor Monitor { get; private set; } = null!;

    private DragOperationManager _dragOperationManager = null!;
    private Harmony _harmony = null!;

    /// <summary>
    /// Entrypoint.
    /// </summary>
    /// <seealso cref="StardewValley.Menus.InventoryMenu"/>
    /// <param name="helper"></param>
    public override void Entry(IModHelper helper)
    {
        Monitor = base.Monitor;
        _harmony = new Harmony(ModManifest.UniqueID);
        _harmony.CreateClassProcessor(typeof(InventoryMenuPatches)).Patch();
        _dragOperationManager = new DragOperationManager(helper, Monitor);
    }
}
