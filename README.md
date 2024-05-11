# Stardew Mouse Tweaks

## Notes

Pseudocode for the listener to `Game1.hooks.OnGame1_UpdateControlInput` registered by `Game1.UpdateControlInput`
with respect to `IClickableScreen.receiveLeftClick` and `IClickableScreen.receiveRightClick`:

```text
if (Game1.panMode) return;
if (releasing slingshot) return;
if ((use tool pressed on kbm OR action pressed on gamepad) AND mouse was visible this frame AND Game1.pauseTime <= 0) {
    for each visible screen {
        if ((HUD is displayed OR screen is chatbox) AND not chatting AND cursor is within screen bounds) {
            screen.receiveLeftClick();
            return;
        }
        if (screen is chatbox AND gamepad controls AND chatting) {
            return;
        }
    }
}
if (chatting or Game1.player.freezePause > 0) return;
if (Game1.paused or Game1.HostPaused) return;
if (action button pressed) {
    for each visible screen {
        if (mouse was visible this frame AND (HUD is displayed or screen is chatbox) AND cursor is within screen bounds) {
            screen.receiveRightClick();
        }
    }
}
```

Which, for the purposes of this mod, may be simplified:
```text
if (game1.panMode) return;
if (releasing slingshot) return;
if (mouse was not visible this frame) return;
if (HUD is not displayed) return;
if (Game1.paused or Game1.HostPaused) return;
if (chatting) return;

if (use tool pressed on kbm OR action pressed on gamepad) {
    for each visible screen {
        if (cursor is within screen bounds) {
            screen.receiveLeftClick();
            return;
        }
    }
}

if (Game1.player.freezePause > 0) return;

if (action button pressed) {
    for each visible screen {
        if (cursor is within screen bounds) {
            screen.receiveRightClick();
            return;
        }
    }
}
```

Where the following (unverified) assumptions are made:
- `Game1.paused` $\lor$ `Game1.HostPaused` $\Leftrightarrow$ `Game1.pauseTime` > 0

This was used to inform the input detection implementation for this mod.
