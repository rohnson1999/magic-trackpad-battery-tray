# Magic Trackpad Battery Tray

[English](README.en.md) · [下载 Windows 版](https://github.com/rohnson1999/magic-trackpad-battery-tray/releases/latest) · [MIT License](LICENSE)

轻量 Windows 托盘程序：显示 Apple Magic Trackpad 电量，并在低于 10% 时提醒充电。

## 功能

- 托盘图标直接显示电量数字，悬停查看电量和充电状态。
- 每 60 秒自动刷新；左键单击立即刷新。
- 电量低于 **10%**（9% 及以下）且未充电时弹出系统提醒。
- 同一段低电量期间只提醒一次，重启程序也会保留提醒状态；充电或恢复至至少 10% 后重置。
- 未连接、休眠或读取失败时显示灰色 `?`，不会把失败显示成 0%。
- 单实例、无主窗口、无网络请求、无第三方运行库，无需 Python 或管理员权限。

## 下载与使用

1. 在 [Releases](https://github.com/rohnson1999/magic-trackpad-battery-tray/releases) 下载 `MagicTrackpadBattery-v1.0.0-windows-x64.zip`。
2. 解压并双击 `MagicTrackpadBattery.exe`。
3. 在右下角系统托盘找到电量数字图标；如被折叠，可从隐藏图标区域将其拖出来。
4. 悬停查看电量；右键菜单可刷新、查看最后更新时间或退出。

程序不自动添加开机启动项。Windows 通知设置或勿扰模式可能抑制提醒。

## 兼容性

| 项目 | 支持范围 |
| --- | --- |
| 系统 | Windows x64；已在 Windows 11 实测 |
| 运行环境 | .NET Framework 4.5 或更新版本；当前实测环境为 4.8 |
| 设备 | Lightning Magic Trackpad，PID `0265`，蓝牙连接 |
| 驱动 | 已在 imbushuo 的 mac-precision-touchpad 驱动环境验证 |
| USB / USB-C 型号 / ARM64 | 当前版本未验证；USB-C PID `0324` 不在设备筛选范围 |

程序读取已有 HID 接口，不安装或替换驱动，不调整手势，也不会把电量添加到 Windows 蓝牙设置页。需要先确保触控板正常连接，辅助 HID 接口可被当前用户访问。

## 构建与测试

使用 Windows 自带 .NET Framework 编译器，无需下载 NuGet 包：

```powershell
.\scripts\build.ps1 -RunTests
```

输出在 `artifacts/`。生成便携包和 SHA-256 校验文件：

```powershell
.\scripts\package.ps1
```

如果当前 PowerShell 策略不允许运行脚本，也可以用系统 C# 编译器直接构建：

```powershell
New-Item -ItemType Directory -Path artifacts -Force
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /codepage:65001 /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:artifacts\MagicTrackpadBattery.exe src\*.cs
```

源码按设备读取、报告解析、提醒策略、图标绘制和托盘界面拆分。测试覆盖报告校验、10% 边界、重复提醒、充电、断连、恢复和重启抑制。

## 隐私与移除

程序不联网。正常运行仅在以下位置保存一个提醒状态文件：

```text
%LOCALAPPDATA%\MagicTrackpadBattery\low-battery-alert.txt
```

右键退出后删除 EXE 即可移除，也可删除上述状态目录。可选诊断参数见 [实现说明](docs/architecture.md)。

## 已知限制

- 当前版本仅处理一台匹配的 Lightning Magic Trackpad；多台同时连接时采用首个成功读取的接口。
- 通知规则已通过自动测试；首版发布时尚未以设备真实低于 10% 的状态验证 Windows 通知送达。
- 若 HID 查询长时间不返回，托盘会显示超时并保持界面响应；不会并发堆积查询。此时可重新连接设备，或退出后重开程序。
- 实际托盘悬停提示与菜单尚未通过界面自动化验证。
- Release EXE 没有商业代码签名证书。

## 致谢与许可证

- [imbushuo/mac-precision-touchpad](https://github.com/imbushuo/mac-precision-touchpad)：Windows 精确式触控板驱动。
- [vitoplantamura/MagicTrackpad2ForWindows](https://github.com/vitoplantamura/MagicTrackpad2ForWindows)：公开的电量读取实现帮助确认报告 `0x90` 的电量字段。
- [微软 HidD_GetInputReport 文档](https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/hidsdi/nf-hidsdi-hidd_getinputreport)。

本仓库不打包上述驱动。程序源码采用 [MIT License](LICENSE)。
