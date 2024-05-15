using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMouseTweaks.DragOperations;
using StardewMouseTweaks.Extensions;
using StardewValley;

namespace StardewMouseTweaks;

public class DragOperationManager
{
    private IModHelper Helper { get; }
    private IMonitor Monitor { get; }

    private DragOperationBase? _ongoingDragOperation = null;
    private SButton? _ongoingDragOperationTrigger = null;

    public DragOperationManager(IModHelper helper, IMonitor monitor)
    {
        Helper = helper;
        Monitor = monitor;

        Listen();
    }

    private void Listen()
    {
        Helper.Events.Input.ButtonPressed += OnButtonPressed;
        Helper.Events.Input.ButtonReleased += OnButtonReleased;
        Helper.Events.Display.MenuChanged += OnMenuChanged;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs args)
    {
        if (args.Button is SButton.MouseRight) {
            OnSecondaryButtonPressed();
            return;
        }

        if (args.Button is SButton.MouseLeft) {
            OnPrimaryButtonPressed();
            return;
        }

        if (Game1.options.gamepadControls) {
            if (args.Button is SButton.ControllerA) {
                OnPrimaryButtonPressed();
                return;
            }

            if (args.Button is SButton.ControllerX) {
                OnSecondaryButtonPressed();
                return;
            }
        }

        if (args.Button.TryGetStardewInput(out var inputButton)) {
            if (Game1.options.actionButton.Contains(inputButton)) {
                OnSecondaryButtonPressed();
                return;
            }

            if (Game1.options.useToolButton.Contains(inputButton)) {
                OnPrimaryButtonPressed();
                return;
            }
        }

        return;

        bool BeginDragOperationConditionsAreMet([NotNullWhen(true)] out InventoryMenuExtensions.InventorySlot? slot)
        {
            slot = null;
            if (!MenuUtils.ClickableMenusCanReceiveSecondaryButtonPresses()) return false;
            if (!MenuUtils.TryGetHoveredInventoryMenu(args.Cursor, out var menu)) return false;
            if (!menu.TryGetHoveredItemSlot(args.Cursor, out slot)) return false;
            if (MenuUtils.CursorSlotItem is null) return false;
            if (_ongoingDragOperation is not null) return false;
            return true;
        }

        void OnSecondaryButtonPressed()
        {
            if (!BeginDragOperationConditionsAreMet(out var slot)) return;

            _ongoingDragOperationTrigger = args.Button;
            _ongoingDragOperation = new BreadcrumbOperation(Helper, Monitor) {
                InitialHoveredSlot = slot,
            };
            _ongoingDragOperation.Completed += OnOngoingDragOperationCompleted;
        }

        void OnPrimaryButtonPressed()
        {
            if (!BeginDragOperationConditionsAreMet(out var slot)) return;

            _ongoingDragOperationTrigger = args.Button;
            _ongoingDragOperation = new DistributeOperation(Helper, Monitor) {
                InitialHoveredSlot = slot,
            };
            _ongoingDragOperation.Completed += OnOngoingDragOperationCompleted;
        }
    }

    private void OnButtonReleased(object? sender, ButtonReleasedEventArgs args)
    {
        if (args.Button is SButton.MouseRight) {
            OnSecondaryButtonReleased();
            return;
        }

        if (args.Button is SButton.MouseLeft) {
            OnPrimaryButtonReleased();
            return;
        }

        if (Game1.options.gamepadControls) {
            if (args.Button is SButton.ControllerA) {
                OnSecondaryButtonReleased();
                return;
            }

            if (args.Button is SButton.ControllerX) {
                OnPrimaryButtonReleased();
                return;
            }
        }

        if (args.Button.TryGetStardewInput(out var inputButton)) {
            if (Game1.options.actionButton.Contains(inputButton)) {
                OnSecondaryButtonReleased();
                return;
            }

            if (Game1.options.useToolButton.Contains(inputButton)) {
                OnPrimaryButtonReleased();
                return;
            }
        }

        return;

        void OnSecondaryButtonReleased()
        {
            if (_ongoingDragOperationTrigger != args.Button) return;
            if (_ongoingDragOperation is not BreadcrumbOperation breadcrumbOperation) return;

            breadcrumbOperation.Complete();
        }

        void OnPrimaryButtonReleased()
        {
            if (_ongoingDragOperationTrigger != args.Button) return;
            if (_ongoingDragOperation is not DistributeOperation distributeOperation) return;

            distributeOperation.Complete();
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs args)
    {
        if (args.OldMenu is null && args.NewMenu is null) return;

        if (args.OldMenu is null && args.NewMenu is not null) {
            OnMenuOpened();
            return;
        }

        if (args.OldMenu is not null && args.NewMenu is null) {
            OnMenuClosed();
            return;
        }

        OnMenuSwapped();
        return;

        void OnMenuClosed()
        {
            Monitor.Log("Menu closed", LogLevel.Debug);
            ResetOngoingDragOperation();
        }

        void OnMenuSwapped()
        {
            Monitor.Log("Menu swapped", LogLevel.Debug);
            ResetOngoingDragOperation();
        }

        void OnMenuOpened()
        {
            Monitor.Log("Menu opened", LogLevel.Debug);
        }
    }

    private void OnOngoingDragOperationCompleted(object? sender, EventArgs args)
    {
        ResetOngoingDragOperation();
    }

    private void ResetOngoingDragOperation()
    {
        if (_ongoingDragOperation is null) return;
        _ongoingDragOperation.Completed -= OnOngoingDragOperationCompleted;

        _ongoingDragOperation.Dispose();
        _ongoingDragOperation = null;
        _ongoingDragOperationTrigger = null;
    }
}
