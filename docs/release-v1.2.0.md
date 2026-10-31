修复登录后未自动运行的问题 / More reliable launch at Windows sign-in.

- 改为当前用户的交互式登录计划任务，延迟 15 秒启动。
- 使用普通权限，不保存密码；使用电池时也可运行，不设运行时长上限。
- 启动失败后最多每分钟重试一次，共三次；主动退出不会立即重启。
- 新任务验证成功后移除旧注册表启动项；托盘菜单仍可开关自启动。
- 增加本地启动日志，帮助区分“未触发”和“程序启动失败”。

Download `MagicTrackpadBattery-v1.2.0-windows-x64.zip`, extract it to a permanent folder, and replace the old EXE after exiting it. Existing Run registration is migrated on launch; startup can also be enabled from the tray menu.

**Validation:** build and logic tests passed. The installed scheduled task actually launched the tray, with successful initialization recorded in its log, and a task-context query read the real battery successfully. Logon triggering after a full Windows restart remains to be verified. Existing device compatibility limits and unsigned-binary status apply.
