# Magic Trackpad Battery Tray

[中文](README.md) · [Windows releases](https://github.com/rohnson1999/magic-trackpad-battery-tray/releases/latest)

A small Windows tray application that shows the battery percentage of a Lightning Apple Magic Trackpad and warns below 10%.

## Features

- Battery percentage on the tray icon; hover for the percentage and charging state.
- Refreshes every 60 seconds. Left-click for an immediate refresh; right-click to see status or exit.
- Optional per-user launch approximately 15 seconds after Windows sign-in through a scheduled task. Toggle it using "开机自启动" in the tray menu; no administrator privileges required.
- Warns at 9% or below while not charging. One warning per low-battery episode, persisted across application restarts.
- Charging or recovery to 10% or above resets the warning. Disconnection does not reset it.
- Shows a gray `?` when the reading is unavailable rather than treating a read failure as 0%.
- Single instance, no main window, no network access, no Python or third-party packages, and no administrator privileges required.

## Download

Download the latest `windows-x64.zip` from [Releases](https://github.com/rohnson1999/magic-trackpad-battery-tray/releases), extract it, and run `MagicTrackpadBattery.exe`. If hidden, move the numbered icon out of the tray overflow area.

Keep the EXE in a permanent folder before enabling startup. If you move it, enable startup again from the new location to update the path. Exiting stops the current instance; unchecking the startup menu item disables future launch at sign-in. Windows notification settings and Do Not Disturb can suppress alerts.

## Compatibility

- Windows x64; tested on Windows 11 with .NET Framework 4.8. Requires .NET Framework 4.5 or later.
- Lightning Magic Trackpad with PID `0265`, connected over Bluetooth, tested with the imbushuo mac-precision-touchpad driver.
- USB operation and other drivers are not verified. USB-C PID `0324` and native ARM64 builds are outside this release.

This application reads an existing auxiliary HID interface. It does not install drivers, change gestures, or add a percentage to the Windows Bluetooth Settings page.

## Build

Use the Windows .NET Framework C# compiler; no NuGet restore is needed:

```powershell
.\scripts\build.ps1 -RunTests
.\scripts\package.ps1
```

Build outputs, the portable ZIP, and SHA-256 checksums are placed in `artifacts/`. GitHub Actions builds and tests the same source on Windows.

## State and removal

The alert state and bounded startup logs are stored under `%LOCALAPPDATA%\MagicTrackpadBattery\`. Enabling startup registers the per-user `MagicTrackpadBattery` logon task with a 15-second delay, interactive/non-elevated execution, battery operation enabled, and up to three one-minute failure retries. No password is stored. A verified replacement removes the legacy Run value. Disable startup, exit through the tray menu, and delete the executable to uninstall. The state/log directory can also be removed.

## Limitations

- One matching device is monitored. Multiple connected trackpads are not individually tracked.
- Low-battery policy is tested; actual low-battery notification delivery has not yet been tested with a device below 10%.
- A stalled HID query shows a timeout while keeping the UI responsive. Reconnect the device or restart the application if it does not recover.
- The user has manually verified tray operation and normal runtime. The new scheduled task has actually launched the installed tray, with successful initialization recorded in its log. Logon triggering after a full Windows restart has not yet been verified.
- Release binaries are not Authenticode signed.

See [architecture and diagnostics](docs/architecture.md) and [validation](docs/validation.md).

## Acknowledgements and license

[mac-precision-touchpad](https://github.com/imbushuo/mac-precision-touchpad) provides the tested driver environment. The public battery implementation in [MagicTrackpad2ForWindows](https://github.com/vitoplantamura/MagicTrackpad2ForWindows) helped confirm the battery field in report `0x90`. Neither driver is bundled here.

[MIT License](LICENSE), copyright 2026 rohnson1999.
