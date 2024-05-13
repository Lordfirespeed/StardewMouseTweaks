using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMouseTweaks.DragOperations;
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

        void OnSecondaryButtonPressed()
        {
            if (!MenuUtils.ClickableMenusCanReceiveSecondaryButtonPresses()) return;
            if (!MenuUtils.TryGetHoveredClickableMenu(args.Cursor, out var menu)) return;
            Monitor.Log(menu.GetType().Name, LogLevel.Info);
            if (_ongoingDragOperation is not null) return;

            _ongoingDragOperationTrigger = args.Button;
            _ongoingDragOperation = new BreadcrumbOperation(Helper, Monitor) {
                InitialCursorPosition = args.Cursor,
            };
        }

        void OnPrimaryButtonPressed()
        {
            if (!MenuUtils.ClickableMenusCanReceivePrimaryButtonPresses()) return;
            if (!MenuUtils.TryGetHoveredClickableMenu(args.Cursor, out var menu)) return;
            if (_ongoingDragOperation is not null) return;

            _ongoingDragOperationTrigger = args.Button;
            _ongoingDragOperation = new DistributeOperation(Helper, Monitor) {
                InitialCursorPosition = args.Cursor,
            };
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
            ResetOngoingDragOperation();
        }

        void OnPrimaryButtonReleased()
        {
            if (_ongoingDragOperationTrigger != args.Button) return;
            if (_ongoingDragOperation is not DistributeOperation distributeOperation) return;

            distributeOperation.Complete();
            ResetOngoingDragOperation();
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
            Monitor.Log("Menu closed", LogLevel.Info);
            ResetOngoingDragOperation();
        }

        void OnMenuSwapped()
        {
            Monitor.Log("Menu swapped", LogLevel.Info);
            ResetOngoingDragOperation();
        }

        void OnMenuOpened()
        {
            Monitor.Log("Menu opened", LogLevel.Info);
        }
    }

    private void ResetOngoingDragOperation()
    {
        if (_ongoingDragOperation is null) return;

        _ongoingDragOperation.Dispose();
        _ongoingDragOperation = null;
        _ongoingDragOperationTrigger = null;
    }
}
