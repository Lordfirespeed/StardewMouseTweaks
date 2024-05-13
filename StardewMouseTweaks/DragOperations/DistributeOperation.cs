using StardewModdingAPI;

namespace StardewMouseTweaks.DragOperations;

public class DistributeOperation : DragOperationBase
{
    private IMonitor Monitor { get; }

    public DistributeOperation(IModHelper helper, IMonitor monitor) : base(helper)
    {
        Monitor = monitor;
        Monitor.Log("Started distribute operation", LogLevel.Info);
    }

    protected override void HoveredSlotChanged()
    {

    }

    public override void Complete()
    {
        Monitor.Log("Completed distribute operation", LogLevel.Info);
        base.Complete();
    }
}
