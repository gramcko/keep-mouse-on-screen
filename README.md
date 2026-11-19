# Keep Mouse

Keeps your touchscreen from moving the mouse cursor, clicking, stealing focus, or kicking your game. Real touch input still works in apps that handle touch/pointer input.

## How it works

Four layers, all built on documented Windows mechanisms:

1. **Block touch as mouse** - a global low-level mouse hook (`WH_MOUSE_LL`) swallows mouse events that Windows synthesizes from touch/pen (tagged in `dwExtraInfo`: `(dwExtraInfo & 0xFFFFFF00) == 0xFF515700`, bit `0x80` = touch). The cursor never moves and no clicks are faked. Physical mouse input passes through untouched, and real touch messages (`WM_POINTER` / `WM_TOUCH`) travel a separate path, so touch-aware apps keep working. Touch is additionally detected at the device level with Raw Input (touch screen usage page `0x0D`), because apps like Chromium consume pointer messages and never trigger mouse synthesis - if a touch still displaces the cursor through any path, it is snapped back to the last real mouse position during and after the touch.

2. **Touch screen windows never activate** - top-level windows on the touchscreen monitor get `WS_EX_NOACTIVATE`, so touching them cannot activate them at all. The desktop (`Progman`/`WorkerW`) is always included, because it spans every monitor. This is what keeps a fullscreen game from minimizing: the game never deactivates. Applied on startup, on new windows, and re-checked every few seconds.

3. **Keep game focused (Focus Lock)** - a `SetWinEventHook(EVENT_SYSTEM_FOREGROUND)` watcher reverts focus steals instantly. Any activation of a touchscreen window or the desktop that was not caused by a real mouse click snaps focus back to the previous window within milliseconds (covers the desktop, taskbar/Start menu, and apps like browsers that activate themselves on touch).

4. **Shield touch screen in fullscreen apps** - while a fullscreen app (e.g. a game) is focused, an invisible, never-activating overlay covers the touchscreen and consumes all touch input. Nothing on the touchscreen can activate, so an exclusive-fullscreen game can never be tabbed out by a touch. The shield disappears automatically when the fullscreen app loses foreground. **Allowed apps** escape the shield: their windows are raised above it, so you can touch them during the game while everything else stays shielded.

The touchscreen monitor is auto-detected from Windows' pointer-device API (`GetPointerDevices` → the display the touch digitizer is mapped to). This is authoritative, works before the first touch, and survives Windows renumbering/reconnecting displays (`WM_DISPLAYCHANGE` triggers a re-check; a 30 s safety check runs too). The old "learn from a touch point" method remains only as a fallback.

## Performance notes

- The low-level mouse hook callback does no file/registry I/O, takes no locks and does not allocate — it only updates a few fields. Everything else (logging, settings, learning) happens on the UI thread's 100 ms timer. This keeps input latency unchanged, including for high-polling-rate mice.
- While a fullscreen app is focused, window scans run every 500 ms and the shield is only repositioned when the monitor geometry actually changes.
- The no-activate sweep runs every 3 s (new windows are handled immediately via show events).

## Build

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

Produces `dist\TouchMonitor.exe` using the .NET Framework compiler built into Windows (no installs needed).

## Autostart (admin, one time)

```powershell
powershell -ExecutionPolicy Bypass -File install-autostart.ps1
```

Creates a Scheduled Task named `TouchMonitor` that starts at logon with highest privileges (needed to block touch input over elevated windows).

To remove:

```powershell
powershell -ExecutionPolicy Bypass -File uninstall-autostart.ps1
```

## Usage

Tray icon menu:

- **Block touch as mouse** - main toggle (cursor never moves from touch)
- **Also block pen input** - also swallow pen-synthesized mouse events
- **Keep game focused (Focus Lock)** - touches can never take focus away from your game
- **Touch screen windows never activate** - windows on the touchscreen can't steal focus at all
- **Shield touch screen in fullscreen apps** - while a fullscreen game is focused, touches on the touchscreen do nothing (nothing can be activated, game stays fullscreen)
- **Allow app under cursor on touch screen** - hover the mouse over an app on the touchscreen and click this: that app keeps receiving touches even while the shield is active (use for your custom control app). While raised, the tool forces `WS_EX_NOACTIVATE` on it so it cannot steal focus, and restores its original style afterwards.
- **Start with Windows** - registers/removes the autostart task (prompts for administrator approval the first time, needed so it also covers elevated apps)
- **Touch mapping (Tablet PC Settings...)** - opens Windows' touch-display mapping dialog (Display tab > Setup... > touch the touchscreen), for when touch lands on the wrong monitor
- **Exit** - restores original window styles and uninstalls the hooks

Hotkeys: `Ctrl+Alt+T` toggles "Block touch as mouse", `Ctrl+Alt+S` toggles the shield. Double-clicking the tray icon toggles blocking.

## Trade-offs

- Apps that only understand mouse messages (old Win32 apps) ignore touch while blocking is on. Use `Ctrl+Alt+T` to pause temporarily.
- With "never activate" or "shield" on, keyboard input stays with your game; to type into an app on the touchscreen, toggle them off (or keep Focus Lock only).
- With the shield on, touches (and mouse clicks) on the touchscreen do nothing while a fullscreen app is focused. This is what guarantees an exclusive-fullscreen game is never tabbed out. To use the touchscreen as a live control panel during gaming, allow your app (tray menu) — its window receives touches while everything else is shielded. The app must not steal focus (WS_EX_NOACTIVATE / return MA_NOACTIVATE from WM_POINTERACTIVATE; the tool forces the no-activate style automatically). Keyboard input stays with the game.
- Exclusive-fullscreen games: with layers 2+3 active the game should never even see a deactivation. If a game still flickers, use borderless windowed mode - that mode never minimizes on focus change.
- The secure desktop (UAC prompts, Ctrl+Alt+Del) cannot be intercepted.
- Elevated apps are only covered while Touch Monitor runs elevated (the scheduled task does).
- Touch visual feedback (the circle) is controlled by Windows settings, not by this tool.

## Debug

Run `dist\TouchMonitor.exe --debug` and check `%LOCALAPPDATA%\TouchMonitor\debug.log`. It shows the `dwExtraInfo` tag of mouse events, what gets swallowed, and every focus/no-activate decision.
