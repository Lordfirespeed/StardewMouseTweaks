using JetBrains.Annotations;
using StardewModdingAPI;
using HarmonyLib;
using StardewMouseTweaks.Patches;

namespace StardewMouseTweaks;

[UsedImplicitly]
internal sealed class Entrypoint : Mod
{
    internal new static IMonitor Monitor { get; private set; } = null!;

    private DragOperationManager _dragOperationManager = null!;
    private Harmony _harmony = null!;

    private static readonly IPatch[] Patches = [
        new InventoryMenuPatches(),
        new ItemGrabMenuPatches(),
    ];

    /// <summary>
    /// Entrypoint.
    /// </summary>
    /// <seealso cref="StardewValley.Menus.InventoryMenu"/>
    /// <param name="helper"></param>
    public override void Entry(IModHelper helper)
    {
        Monitor = base.Monitor;
        _harmony = new Harmony(ModManifest.UniqueID);
        foreach (var patch in Patches) {
            patch.Initialize(helper, Monitor);
            _harmony.CreateClassProcessor(patch.GetType()).Patch();
        }
        _dragOperationManager = new DragOperationManager(helper, Monitor);
    }
}
