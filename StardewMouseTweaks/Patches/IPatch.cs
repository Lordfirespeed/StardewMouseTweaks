using StardewModdingAPI;

namespace StardewMouseTweaks.Patches;

public interface IPatch
{
    public void Initialize(IModHelper helper, IMonitor monitor);
}
