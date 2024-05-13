using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace StardewMouseTweaks.DragOperations;

public abstract class DragOperationBase : IDisposable
{
    private IModHelper Helper { get; }

    public required ICursorPosition InitialCursorPosition {
        init {
            // todo : call HoveredSlotChanged
        }
    }

    public DragOperationBase(IModHelper helper)
    {
        Helper = helper;
        Helper.Events.Input.CursorMoved += OnCursorMoved;
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs args)
    {

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
