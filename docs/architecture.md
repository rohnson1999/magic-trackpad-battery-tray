# Implementation and diagnostics

## Device access

`HidBattery` enumerates Windows Raw Input HID collections and selects Apple Lightning Magic Trackpad PID `0265`. It prefers the auxiliary `Col02` collection and opens it with shared query access. If zero desired access is insufficient, it tries shared read access.

The reader calls `HidD_GetInputReport` with report ID `0x90`, using the HID collection's reported input length. The third byte is interpreted as percentage; status bits 1 and 2 of the second byte indicate charging and fully charged. Truncated reports, invalid IDs, values above 100, and an ambiguous all-zero payload are treated as unavailable. No streaming `ReadFile`, feature writes, driver installation, registry changes or Bluetooth pairing operations are performed.

## Tray and alerts

`BatteryTray` owns a Windows Forms `NotifyIcon` and context menu. A worker task performs the synchronous device query; the UI stays responsive, with at most one query in flight. A one-second UI timer schedules device queries at least 60 seconds apart and detects queries exceeding ten seconds. A stalled worker is not replaced by additional blocked workers.

`LowBatteryPolicy` warns only below 10%, while neither charging nor fully charged. A persisted latch suppresses repeated alerts across process restarts. Recovery to at least 10% or charging clears the latch. Unknown readings preserve it. This is a single-device application.

`TrayArt` draws the number icon locally with GDI+. Green means a normal reading, blue means charging, red means below 10% while not charging, and gray means unavailable. The application uses a per-session named mutex to prevent duplicate tray instances.

## Layout

| Path | Purpose |
| --- | --- |
| `src/Reading.cs` | Report validation and decoding |
| `src/HidBattery.cs` | Read-only Windows HID access |
| `src/LowBatteryPolicy.cs` | Alert decision and reset rules |
| `src/TrayArt.cs` | Icon drawing and native icon cleanup |
| `src/BatteryTray.cs` | Polling, tooltips, menu and notifications |
| `src/Program.cs` | Entry point, single instance and diagnostic commands |
| `src/StartupRegistration.cs` | Interactive logon task, validation and legacy migration |
| `src/StartupLog.cs` | Bounded local startup and error log |
| `tests/LogicTests.cs` | Hardware-independent tests |
| `scripts/` | Build, test and release packaging |

## Optional diagnostic commands

These are explicit command-line options; normal double-click startup produces no diagnostic file:

```powershell
# Query once and write a short result, without starting the tray.
.\artifacts\MagicTrackpadBattery.exe --read-once .\artifacts\device-reading.txt

# Run the tray and update a JSON status file after each query.
.\artifacts\MagicTrackpadBattery.exe --status-file .\artifacts\status.json

# Render an example icon to a PNG; no device access.
.\artifacts\MagicTrackpadBattery.exe --preview .\artifacts\icon.png
```

The single-instance guard also applies to diagnostic tray startup. Exit an existing tray instance first when checking a new build. No device addresses or device paths are written to the diagnostic result.

## Startup

The tray menu's "开机自启动" checkbox adds or removes the `MagicTrackpadBattery` scheduled task through Windows Task Scheduler COM. Its logon trigger targets the current user's SID and delays execution 15 seconds. The action uses the absolute EXE path, `--startup`, and the install directory as its working directory. It uses `InteractiveToken` and `LeastPrivilege`, stores no password, permits battery operation and has no execution time limit. Nonzero startup failure can retry three times at one-minute intervals. Normal user exit returns success and is not automatically restarted.

The setting is re-read when the menu opens. Validation accepts either SID or the account name Windows normalizes into the saved trigger. Moving the executable requires enabling startup from its new location. Exiting the tray does not disable startup. Legacy Run registration is migrated on normal launch and removed only after the replacement task has been verified. Headless deployment commands return exit code 0 on success:

```powershell
.\MagicTrackpadBattery.exe --autostart enable
.\MagicTrackpadBattery.exe --autostart disable
```

`startup.log` records manual/task launch, successful tray initialization and errors under the local application-data directory. It rotates after approximately 64 KiB, retaining one previous log. Scheduled startup reports an initialization error with a nonzero exit instead of a blocking message box so Task Scheduler can retry. Logs stay local.

## Development reference

- [HidD_GetInputReport](https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/hidsdi/nf-hidsdi-hidd_getinputreport)
- [NotifyIcon](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.notifyicon)
- [Public report 0x90 battery implementation](https://github.com/vitoplantamura/MagicTrackpad2ForWindows/blob/master/AmtPtpHidFilter/ControlDevice.c)
