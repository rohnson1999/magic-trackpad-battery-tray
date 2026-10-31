# Validation

The initial implementation was tested on Windows 11 x64 with .NET Framework 4.8 and a Lightning Magic Trackpad over Bluetooth using mac-precision-touchpad.

- Actual auxiliary HID battery reads succeeded and agreed between the Python proof of concept and C# implementation.
- The tray process ran successfully, updated its diagnostic status, and refreshed automatically after one minute.
- A second launch exited successfully while the first instance remained active.
- The icon was rendered and visually inspected.
- The user has manually verified system tray operation and normal runtime.
- Startup command tests cover spaces, case-insensitive comparison, missing/stale registrations and rejection of relative paths.
- Version 1.1.0 was deployed to a permanent local folder, queried the actual device successfully, registered itself through `--autostart enable`, and the saved current-user Run value was read back and matched the quoted installed EXE path. The deployed tray instance started successfully.
- On 2026-10-31 the user reported that v1.1.0 had not started after Windows restart/sign-in. The Run entry and EXE still existed; read-back alone had not validated execution.
- Version 1.2.0 replaced that entry with an interactive delayed logon task. Executing the installed task actually launched the tray, and its startup log recorded `Process started (login task)` and `Tray initialized successfully`.
- A diagnostic task in the same execution context successfully read the actual battery. Temporary diagnostic tasks were removed afterward.
- Task XML tests cover normalized account names, XML escaping, trigger user matching, disabled triggers, delay, interactive least-privilege execution, battery operation and unlimited runtime.
- Hardware-independent tests cover valid/invalid report decoding, charging bits, ambiguous zero data, truncated reports, exactly 10%, first warning below 10%, repeated low readings, disconnection, charging, recovery, unplugging while low, and a persisted alert latch.

The initial portable prototype occupied about 34 MB of working-set memory. Resource usage varies by Windows version and device state; it is not a guaranteed memory limit.

## Not yet validated

- Real low-battery Windows notification delivery: the connected device was not below the threshold during testing.
- The v1.2.0 logon trigger after an actual Windows restart; demand-starting the same registered task has been verified without restarting the user's session.
- USB, USB-C devices, native ARM64, multiple trackpads and other driver combinations.

GitHub Actions runs the hardware-independent tests, compiles the executable and packages the portable Windows release payload. Hardware access is not tested on hosted runners.
