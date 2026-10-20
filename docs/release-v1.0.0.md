首个 Windows x64 便携版 / Initial Windows x64 portable release.

- 托盘数字显示电量；悬停查看电量和充电状态。
- 每分钟自动刷新，左键立即刷新，右键菜单退出。
- 电量低于 10% 且未充电时提醒；记录提醒状态，避免重复弹出。
- 无需 Python、额外 NuGet 包或管理员权限。

Download `MagicTrackpadBattery-v1.0.0-windows-x64.zip`, extract it, and run `MagicTrackpadBattery.exe`. The standalone EXE is also attached. `SHA256SUMS.txt` includes checksums for both files.

**Compatibility:** Windows x64, .NET Framework 4.5+, Lightning Magic Trackpad PID `0265` over Bluetooth. Tested with the mac-precision-touchpad driver. USB-C PID `0324` and ARM64 are not supported by this release.

**Validation:** report reading and alert-policy tests passed. Real below-10% notification delivery and tray UI interaction have not yet been independently validated. Windows notification settings may suppress alerts. Binaries are not Authenticode signed.

源码和 MIT 许可证见仓库。驱动不包含在下载文件中。
