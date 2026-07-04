# LuckyStrike-WindowsCleanUP v2.0 🚀

An advanced, smart, and highly configurable system maintenance and automation script for Windows 10 and Windows 11. Built with robust error handling, multi-level logging, and intelligent file locking bypass mechanisms.

![Windows 10/11](https://img.shields.io/badge/OS-Windows%2010%20%7C%2011-0078D6?style=flat&logo=windows)
![Version](https://img.shields.io/badge/Version-2.0-00B2FF?style=flat)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)

> [!IMPORTANT]
> **ADMINISTRATOR PRIVILEGES REQUIRED:** This script performs deep system maintenance (DISM component cleanup, Windows Defender scan history purge, and Windows Service management). You **must run this script as Administrator** for it to function correctly.

---

## 📸 Screenshots

| Execution & Progress | Summary & Interactive Exit |
| :---: | :---: |
| ![Main Screen](assets/main_screen.png) | ![Completion](assets/completion.png) |

---

## ✨ Key Features

- **🛡️ Run As Administrator Requirement:** Deep-cleans system directories and services safely with elevated permissions.
- **🧠 Smart Deletion Algorithm:** Logs exact file paths being deleted or bypassed due to active system locks without crashing or spamming the console.
- **📊 5-Level Parametric Logging:** Fully configurable log levels ranging from silent critical errors (`FATAL`) to full file-by-file deletion tracking (`DEBUG`).
- **🔐 Windows Defender History Purge:** Safely takes ownership (`takeown` & `icacls`) of protected system folders to completely wipe old threat detection logs.
- **⚡ Background Execution:** Utilizes native Windows tools (`DISM`, `wevtutil`, `ipconfig`, `PowerShell`) silently in isolated sub-processes.
- **🎮 Interactive Exit Options:** Built-in prompt at completion allowing an instant System Reboot (`R`), Shutdown (`S`), or safe clean exit.

---

## 🚀 How to Use

1. Download the latest `LuckyStrike-Windows-CleanUP.bat` file from this repository.
2. **Right-click** on `LuckyStrike-Windows-CleanUP.bat` and select **"Run as administrator"** *(Crucial for Step 15 & 16)*.
3. Sit back and let the automated cleanup process run.
4. When all 16 steps are completed, press:
   - `R` to **Reboot** the PC immediately.
   - `S` to **Shutdown** the PC immediately.
   - **Any other key** to close the window cleanly.

---

## 🧹 What Does It Clean? (16 Steps)

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
