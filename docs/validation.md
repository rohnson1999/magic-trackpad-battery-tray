# Validation

The initial implementation was tested on Windows 11 x64 with .NET Framework 4.8 and a Lightning Magic Trackpad over Bluetooth using mac-precision-touchpad.

- Actual auxiliary HID battery reads succeeded and agreed between the Python proof of concept and C# implementation.
- The tray process ran successfully, updated its diagnostic status, and refreshed automatically after one minute.
- A second launch exited successfully while the first instance remained active.
- The icon was rendered and visually inspected.
- The user has manually verified system tray operation and normal runtime.
- Startup command tests cover spaces, case-insensitive comparison, missing/stale registrations and rejection of relative paths.
- Version 1.1.0 was deployed to a permanent local folder, queried the actual device successfully, registered itself through `--autostart enable`, and the saved current-user Run value was read back and matched the quoted installed EXE path. The deployed tray instance started successfully.
- Hardware-independent tests cover valid/invalid report decoding, charging bits, ambiguous zero data, truncated reports, exactly 10%, first warning below 10%, repeated low readings, disconnection, charging, recovery, unplugging while low, and a persisted alert latch.

The initial portable prototype occupied about 34 MB of working-set memory. Resource usage varies by Windows version and device state; it is not a guaranteed memory limit.

## Not yet validated

- Real low-battery Windows notification delivery: the connected device was not below the threshold during testing.
- Startup after an actual Windows restart; local registration can be verified without restarting the user's session.
- USB, USB-C devices, native ARM64, multiple trackpads and other driver combinations.

GitHub Actions runs the hardware-independent tests, compiles the executable and packages the portable Windows release payload. Hardware access is not tested on hosted runners.
