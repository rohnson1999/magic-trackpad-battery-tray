新增开机自启动 / Per-user startup at Windows sign-in.

- 右键托盘图标，勾选或取消“开机自启动”。无需管理员权限。
- 保存带引号的绝对路径，支持包含空格的安装目录。
- 建议将 EXE 放在固定目录，再开启自启动；迁移后可重新启用以更新路径。
- 用户已手动验证原有托盘操作和正常运行。

Download `MagicTrackpadBattery-v1.1.0-windows-x64.zip`, extract it to a permanent folder, and run `MagicTrackpadBattery.exe`. Startup is optional and can be toggled in the tray menu. The standalone EXE and SHA-256 checksums are also attached.

Battery reading, low-battery policy and startup command tests pass. Registration can be verified without rebooting; actual launch after a Windows restart has not been exercised. Existing compatibility limits apply: Windows x64, .NET Framework 4.5+, Lightning Magic Trackpad PID `0265` over Bluetooth. Binaries are not Authenticode signed.
