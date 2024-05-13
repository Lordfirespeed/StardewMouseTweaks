using StardewModdingAPI;

namespace StardewMouseTweaks.DragOperations;

public class BreadcrumbOperation : DragOperationBase
{
    private IMonitor Monitor { get; }

    public BreadcrumbOperation(IModHelper helper, IMonitor monitor) : base(helper)
    {
        Monitor = monitor;
        Monitor.Log("Started breadcrumb operation", LogLevel.Info);
    }

    protected override void HoveredSlotChanged()
    {

    }

    public override void Complete()
    {
        Monitor.Log("Completed breadcrumb operation", LogLevel.Info);
        base.Complete();
    }
}
