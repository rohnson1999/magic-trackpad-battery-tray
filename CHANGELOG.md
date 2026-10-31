# Changelog

## 1.2.0 — 2026-10-31

- Replace registry Run startup with a per-user interactive logon task delayed by 15 seconds.
- Permit battery operation and unlimited runtime, with up to three one-minute failure retries.
- Migrate legacy Run registration only after the replacement task has been verified.
- Add bounded local startup/error logs and suppress blocking error dialogs during scheduled startup.
- Validate Windows-normalized task account names as well as SID-form trigger users.

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
