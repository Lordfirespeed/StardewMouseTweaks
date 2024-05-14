using System;
using StardewModdingAPI;
using StardewValley;

namespace StardewMouseTweaks.DragOperations;

public class BreadcrumbOperation : DragOperationBase
{
    private IMonitor Monitor { get; }

    public BreadcrumbOperation(IModHelper helper, IMonitor monitor) : base(helper)
    {
        Monitor = monitor;
        Monitor.Log("Started breadcrumb operation", LogLevel.Debug);
    }

    protected override void HoveredSlotChanged()
    {
        if (HoveredSlot is null) return;
        if (Game1.player.CursorSlotItem is not { } cursorSlotItem) throw new InvalidOperationException();

        var singleItem = cursorSlotItem.getOne();
        if (HoveredSlot.Item is { } hoveredItem && !singleItem.canStackWith(hoveredItem)) return;
        var remainder = HoveredSlot.AddItem(singleItem);
        if (remainder is not null) return;

        cursorSlotItem.Stack -= 1;
        if (cursorSlotItem.Stack == 0) {
            Game1.player.CursorSlotItem = null;
            Complete();
        }
    }

    public override void Complete()
    {
        Monitor.Log("Completed breadcrumb operation", LogLevel.Debug);
        base.Complete();
    }
}
