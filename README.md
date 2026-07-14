# LuckyStrike-Windows-CleanUP v3.0 🚀

An advanced, smart, and highly configurable system maintenance, cleanup, and application automation script for Windows 10 and Windows 11. Built with an interactive main menu, robust error handling, multi-level logging, intelligent file-locking bypass mechanisms, and one-click package upgrades via Winget.

![Windows 10/11](https://img.shields.io/badge/OS-Windows%2010%20%7C%2011-0078D6?style=flat&logo=windows)
![Version](https://img.shields.io/badge/Version-3.0-00B2FF?style=flat)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)

> [!IMPORTANT]
> **ADMINISTRATOR PRIVILEGES REQUIRED:** This script performs deep system maintenance (DISM component cleanup, Windows Defender scan history purge, Windows Service management, and system-wide application updates). You **must run this script as Administrator** for it to function correctly.

---

## 📸 Screenshots

| Interactive Main Menu & Execution | Summary & Completion |
| :---: | :---: |
| ![Main Screen](assets/main_screen.png) | ![Completion](assets/completion.png) |

---

## ✨ What's New in v3.0?

- **🖥️ Interactive Color-Coded Main Menu:** A brand-new UI with clear, color-coded menu options for easy navigation.
- **⚡ Instant Key Detection:** Powered by a background PowerShell input handler, the script detects your choice (1, 2, 3, or 4) *instantly* without needing to press the Enter key.
- **📦 One-Click App Upgrades (Winget):** Easily update all outdated installed applications on your system with a single keystroke using Windows Package Manager (`winget upgrade --all`).
- **🚪 Quick Exit Mechanism:** Pressing any unassigned key (any key other than 1, 2, 3, or 4) in the main menu instantly closes the script cleanly.

---

## 🚀 How to Use

1. Download the latest `LuckyStrike-Windows-CleanUP.bat` file from this repository.
2. **Right-click** on `LuckyStrike-Windows-CleanUP.bat` and select **"Run as administrator"** *(Crucial for deep cleaning steps and Winget package updates)*.
3. Once the **Main Menu** appears, simply press the corresponding number key on your keyboard (no need to press Enter):
   - `1` : **Start Cleanup Process** (Triggers the 16-step automated system deep clean).
   - `2` : **Update Applications** (Scans and upgrades all outdated software via Winget).
   - `3` : **Reboot Computer** (Restarts Windows immediately).
   - `4` : **Shutdown Computer** (Powers off Windows immediately).
   - **Any other key** : Closes the script instantly.
4. After completing a cleanup or update task, press any key to return directly to the Main Menu.

---

## 📦 Application Upgrades (Winget Integration)

By pressing `2` in the Main Menu, LuckyStrike-Windows-CleanUP utilizes the native **Windows Package Manager (Winget)** to scan all installed software on your machine against official repositories. 

- Automatically identifies outdated applications (e.g., browsers, tools, runtimes).
- Silently downloads and installs the latest stable versions without manual intervention.
- Returns seamlessly to the Main Menu once all upgrades are complete.

---

## 🧹 What Does It Clean? (16 Steps)

When you select Option `1`, the script executes the following 16-step maintenance pipeline:

1. **Windows & User Temp Folders** (`C:\Windows\Temp` & `%TEMP%`)
2. **Windows Update Cache** (`SoftwareDistribution\Download`)
3. **File Explorer Recent Files & Start Menu MRU**
4. **All Recycle Bins Across All Local Drives**
5. **Windows Event Logs** (System, Application, Security, etc.)
6. **DNS Resolver Cache** (`ipconfig /flushdns`)
7. **Explorer File Access & Frequent Folder History**
8. **Start Menu Recently Installed Apps Tracking**
9. **Crash Reports & Memory Dumps** (`MEMORY.DMP`, `Minidump`, `WER`)
10. **Delivery Optimization Cache**
11. **DirectX & GPU Shader Caches** (NVIDIA, AMD, D3D)
12. **Thumbnail & Icon Caches** (`IconCache.db`, `thumbcache_*.db`)
13. **Windows Prefetch Cache**
14. **ARP & Network Caches** (`NBTSTAT`)
15. **Windows Defender Protection & Scan History**
16. **DISM Component Store Cleanup** (`/StartComponentCleanup`)

---

## ⚙️ Configuration & Parameters

You can easily customize the script behavior by editing the variables at the top of the `LuckyStrike-Windows-CleanUP.bat` file with any text editor (e.g., Notepad):

### Enable / Disable Modules
Set any step to `ON` or `OFF`:
```batch
SET CLEAN_TEMP=ON
SET CLEAN_UPDATE=ON
SET CLEAN_EXPLORER_RECENT=ON
SET CLEAN_RECYCLE=ON
SET CLEAN_LOGS=ON
SET CLEAN_DNS=ON
SET CLEAN_EXPLORER_HISTORY=ON
SET HIDE_RECENT_APPS=ON
SET CLEAN_CRASH_DUMPS=ON
SET CLEAN_DELIVERY_OPT=ON
SET CLEAN_SHADER_CACHE=OFF
SET CLEAN_THUMB_ICON=ON
SET CLEAN_PREFETCH=ON
SET CLEAN_NET_CACHE=ON
SET CLEAN_DEFENDER=OFF
SET CLEAN_DISM=ON
