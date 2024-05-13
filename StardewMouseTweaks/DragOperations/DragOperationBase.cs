using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMouseTweaks.Extensions;

namespace StardewMouseTweaks.DragOperations;

public abstract class DragOperationBase : IDisposable
{
    private IModHelper Helper { get; }

    protected InventoryMenuExtensions.InventorySlot? HoveredSlot;

    public required InventoryMenuExtensions.InventorySlot? InitialHoveredSlot {
        init {
            HoveredSlot = value;
            HoveredSlotChanged();
        }
    }

    public DragOperationBase(IModHelper helper)
    {
        Helper = helper;
        Helper.Events.Input.CursorMoved += OnCursorMoved;
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs args)
    {
        if (!MenuUtils.TryGetHoveredInventoryMenu(args.NewPosition, out var menu)) return;
        if (!menu.TryGetHoveredItemSlot(args.NewPosition, out var slot)) {
            HoveredSlot = null;
            return;
        }
        if (HoveredSlot == slot) return;
        HoveredSlot = slot;
        HoveredSlotChanged();
    }

    protected abstract void HoveredSlotChanged();

    public virtual void Complete()
    {
        Completed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs>? Completed;

    public virtual void Dispose()
    {
        Helper.Events.Input.CursorMoved -= OnCursorMoved;
    }
}
