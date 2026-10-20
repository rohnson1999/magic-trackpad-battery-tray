# Changelog

## 1.1.0 — 2026-10-20

- Add a per-user "开机自启动" tray menu toggle, with no administrator privileges required.
- Register the absolute executable location with correct quoting for paths containing spaces.
- Add `--autostart enable|disable` for local deployment and removal.
- Record successful user testing of tray operation and normal runtime.

## 1.0.0 — 2026-10-20

- Initial portable Windows x64 release.
- Read Lightning Magic Trackpad battery through its existing auxiliary HID collection.
- Numbered tray icon, hover status, manual refresh and one-minute background polling.
- Warn below 10% while not charging, with persistent duplicate suppression.
- Show unavailable or timeout states without inventing a battery percentage.
- Include build scripts, logic tests, portable packaging and SHA-256 checksums.
