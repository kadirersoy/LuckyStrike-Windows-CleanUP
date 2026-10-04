@echo off
title LuckyStrike-Windows-CleanUP v3.0 Pro - Gelismis Windows Bakimi
color 0B
chcp 65001 >nul 2>&1

:: ANSI Renk Kodlarini Aktif Et (Windows 10/11 icin)
for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do (
    set "ESC=%%b"
)
set "C_RESET=%ESC%[0m"
set "C_GREEN=%ESC%[1;32m"
set "C_YELLOW=%ESC%[1;33m"
set "C_CYAN=%ESC%[1;36m"
set "C_RED=%ESC%[1;31m"
set "C_MAGENTA=%ESC%[1;35m"
set "C_WHITE=%ESC%[1;37m"
set "C_LINE=%ESC%[38;5;39m"

:: Log klasörünü ve master log dosyasını hazırla
SET "LOG_DIR=%APPDATA%\LuckyCleanTemp"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" >nul 2>&1
SET "MASTER_LOG=%LOG_DIR%\LuckyStrike-Windows-CleanUP_Log.txt"

:: ==========================================
:: PARAMETRELER (18 ADIM)
:: ==========================================
SET ENABLE_LOGGING=ON
SET LOG_LEVEL=5
SET CLEAN_TEMP=ON
SET CLEAN_UPDATE=ON
SET CLEAN_EXPLORER_RECENT=ON
SET CLEAN_RECYCLE=ON
SET CLEAN_LOGS=ON
SET CLEAN_DNS=ON
SET SET_EXPLORER_HISTORY_OFF=OFF
SET HIDE_RECENT_APPS=OFF
SET CLEAN_CRASH_DUMPS=ON
SET CLEAN_DELIVERY_OPT=ON
SET CLEAN_SHADER_CACHE=ON
SET CLEAN_THUMB_ICON=ON
SET CLEAN_PREFETCH=ON
SET CLEAN_NET_CACHE=ON
SET CLEAN_DEFENDER=OFF
SET CLEAN_DISM=ON
SET CLEAN_BROWSERS=ON
SET CLEAN_WIN_OLD=ON
:: ==========================================

:MAIN_MENU
cls
echo %C_LINE%================================================================================%C_RESET%
echo.
echo %C_LINE%  +--------------------------------------------------------------------------+%C_RESET%
echo %C_LINE%  ^|                                                                          ^|%C_RESET%
echo %C_WHITE%  ^|         L U C K Y S T R I K E   -   C L E A N   U P   P R O              ^|%C_RESET%
echo %C_LINE%  ^|             Gelismis Windows Bakim ve Temizleme Araci                    ^|%C_RESET%
echo %C_LINE%  ^|                         Surum v3.0 Pro (18 Modul)                        ^|%C_RESET%
echo %C_LINE%  ^|                                                                          ^|%C_RESET%
echo %C_LINE%  +--------------------------------------------------------------------------+%C_RESET%
echo.
echo %C_WHITE%                                  A N A  M E N U                                 %C_RESET%
echo %C_LINE%================================================================================%C_RESET%
echo.
echo    %C_GREEN%[1]  Temizlik Islemlerini Baslat (18 Adimli Akilli Temizlik)%C_RESET%
echo.
echo    %C_CYAN%[2]  Sistem Geri Yukleme Noktasi Olustur (Yedek Al)%C_RESET%
echo.
echo    %C_YELLOW%[3]  Uygulamalari Guncelle (Winget Upgrade --all)%C_RESET%
echo.
echo    %C_MAGENTA%[4]  Bilgisayari Yeniden Baslat%C_RESET%
echo.
echo    %C_RED%[5]  Bilgisayari Kapat%C_RESET%
echo.
echo    %C_WHITE%[*]  Cikis icin herhangi baska bir tusa basiniz...%C_RESET%
echo.
echo %C_LINE%================================================================================%C_RESET%
echo %C_WHITE% Lutfen yapmak istediginiz islemin tusuna basiniz:%C_RESET%

for /f "delims=" %%k in ('PowerShell -NoProfile -Command "$k = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown'); $k.Character"') do set "KEY=%%k"

if "%KEY%"=="1" goto CLEANUP_PROCESS
if "%KEY%"=="2" goto RESTORE_POINT_PROCESS
if "%KEY%"=="3" goto WINGET_PROCESS
if "%KEY%"=="4" shutdown /r /t 0
if "%KEY%"=="5" shutdown /s /t 0

goto :eof

:RESTORE_POINT_PROCESS
cls
echo %C_LINE%================================================================================%C_RESET%
echo %C_CYAN% [SISTEM] Sistem Geri Yukleme Noktasi Olusturuluyor...%C_RESET%
echo %C_LINE%================================================================================%C_RESET%
PowerShell -NoProfile -Command "Checkpoint-Computer -Description 'LuckyStrike_CleanUP_Backup' -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop" >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo %C_GREEN% [BASARILI] Geri yukleme noktasi basariyla olusturuldu!%C_RESET%
) else (
    echo %C_YELLOW% [UYARI] Geri yukleme noktasi olusturulamadi (Sistem Korumasi kapali olabilir veya yonetici yetkisi gereklidir).%C_RESET%
)
echo %C_LINE%--------------------------------------------------------------------------------%C_RESET%
echo %C_YELLOW% Ana menuye donmek icin herhangi bir tusa basiniz...%C_RESET%
echo %C_LINE%================================================================================%C_RESET%
pause >nul
goto MAIN_MENU

:WINGET_PROCESS
cls
echo %C_LINE%================================================================================%C_RESET%
echo %C_WHITE% [WINGET] Uygulamalar guncelleniyor...%C_RESET%
echo %C_LINE%================================================================================%C_RESET%
winget upgrade --all --accept-source-agreements --accept-package-agreements
echo.
echo %C_LINE%================================================================================%C_RESET%
echo %C_GREEN% [WINGET] Guncelleme islemi tamamlandi.%C_RESET%
echo %C_LINE%--------------------------------------------------------------------------------%C_RESET%
echo %C_YELLOW% Ana menuye donmek icin herhangi bir tusa basiniz...%C_RESET%
echo %C_LINE%================================================================================%C_RESET%
pause >nul
goto MAIN_MENU

:CLEANUP_PROCESS
cls
set "t=%TIME: =0%"
set "START_CLOCK=%t:~0,8%"
for /f "tokens=1-4 delims=:,." %%a in ("%t%") do (
    set /a "TOTAL_S_SEC=(((1%%a-100)*60)+(1%%b-100))*60+(1%%c-100)"
    set /a "TOTAL_S_CSEC=1%%d-100"
)

:: Temizlik oncesi C: bos alanini al
for /f "delims=" %%B in ('PowerShell -NoProfile -Command "([System.IO.DriveInfo]'C').AvailableFreeSpace"') do set "INITIAL_FREE_BYTES=%%B"
for /f "delims=" %%B in ('PowerShell -NoProfile -Command "[string][math]::Round(%INITIAL_FREE_BYTES% / 1GB, 2) + ' GB Bos'"') do set "INITIAL_FREE_GB=%%B"

if /I "%ENABLE_LOGGING%"=="ON" (
    echo ================================================================================ > "%MASTER_LOG%" 2>nul
    echo [INFO] [%DATE% %START_CLOCK%] LuckyStrike-Windows-CleanUP v3.0 Pro Baslatildi >> "%MASTER_LOG%" 2>nul
    echo ================================================================================ >> "%MASTER_LOG%" 2>nul
)

echo ================================================================================
echo.
echo   +--------------------------------------------------------------------------+
echo   ^|                                                                          ^|
echo   ^|         L U C K Y S T R I K E   -   C L E A N   U P   P R O              ^|
echo   ^|             Gelismis Windows Bakim ve Temizleme Araci                    ^|
echo   ^|                         Surum v3.0 Pro (18 Modul)                        ^|
echo   ^|                                                                          ^|
echo   +--------------------------------------------------------------------------+
echo.
:: YONETICI KONTROLU
fltmc >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    powershell -NoProfile -Command "Write-Host ' [UYARI] TAM FONKSIYON ICIN YONETICI HAKLARI GEREKLIDIR!' -ForegroundColor Red; Write-Host ' Tum adimlarin (DISM, Servisler, Loglar) hatasiz calismasi icin lutfen Yonetici Olarak calistirin.' -ForegroundColor Yellow"
    echo ================================================================================
)
echo ================================================================================
echo           [LuckyStrike-Windows-CleanUP] TEMIZLIK ISLEMLERI BASLIYOR...
echo           Baslangic Saati : %START_CLOCK%
echo           Baslangic Alani : %INITIAL_FREE_GB% GB Bos
echo ================================================================================
echo.

:STEP1
<nul set /p="[1/18] Windows ve Kullanici Temp Klasorleri ................ "
call :TIMER_START "Windows ve Kullanici Temp Klasorleri" 2>nul
(
    call :SMART_DELETE "C:\Windows\Temp"
    call :SMART_DELETE "%temp%"
) > "%LOG_DIR%\s1.out" 2> "%LOG_DIR%\s1.err"
call :EVALUATE_STEP "s1" 2>nul
goto STEP2
:SKIP1
call :LOG_SKIP "Windows ve Kullanici Temp Klasorleri" 2>nul

:STEP2
<nul set /p="[2/18] Windows Update Onbellek Dosyalari ................... "
IF /I "%CLEAN_UPDATE%" NEQ "ON" goto SKIP2
call :TIMER_START "Windows Update Onbellek Dosyalari" 2>nul
(
    net stop wuauserv >nul 2>&1
    net stop bits >nul 2>&1
    net stop dosvc >nul 2>&1
    call :SMART_DELETE "C:\Windows\SoftwareDistribution\Download"
    net start wuauserv >nul 2>&1
    net start bits >nul 2>&1
    net start dosvc >nul 2>&1
) > "%LOG_DIR%\s2.out" 2> "%LOG_DIR%\s2.err"
call :EVALUATE_STEP "s2" 2>nul
goto STEP3
:SKIP2
call :LOG_SKIP "Windows Update Onbellek Dosyalari" 2>nul

:STEP3
<nul set /p="[3/18] Dosya Gezgini Gecmisi ve Calistir MRU ............... "
IF /I "%CLEAN_EXPLORER_RECENT%" NEQ "ON" goto SKIP3
call :TIMER_START "Dosya Gezgini Gecmisi ve Calistir MRU" 2>nul
(
    call :SMART_DELETE "%APPDATA%\Microsoft\Windows\Recent"
    reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU" /va /f >nul 2>&1
) > "%LOG_DIR%\s3.out" 2> "%LOG_DIR%\s3.err"
call :EVALUATE_STEP "s3" 2>nul
goto STEP4
:SKIP3
call :LOG_SKIP "Dosya Gezgini Gecmisi ve Calistir MRU" 2>nul

:STEP4
<nul set /p="[4/18] Tum Disklerdeki Geri Donusum Kutulari ............... "
IF /I "%CLEAN_RECYCLE%" NEQ "ON" goto SKIP4
call :TIMER_START "Tum Disklerdeki Geri Donusum Kutulari" 2>nul
PowerShell.exe -NoProfile -Command "Clear-RecycleBin -Force -ErrorAction SilentlyContinue" > "%LOG_DIR%\s4.out" 2> "%LOG_DIR%\s4.err"
call :EVALUATE_STEP "s4" 2>nul
goto STEP5
:SKIP4
call :LOG_SKIP "Tum Disklerdeki Geri Donusum Kutulari" 2>nul

:STEP5
<nul set /p="[5/18] Windows Olay Gunlukleri (Hizli Motor) ................ "
IF /I "%CLEAN_LOGS%" NEQ "ON" goto SKIP5
call :TIMER_START "Windows Olay Gunlukleri" 2>nul
PowerShell.exe -NoProfile -Command "Get-WinEvent -ListLog * -EA 0 | ForEach-Object { try { [System.Diagnostics.Eventing.Reader.EventLogSession]::GlobalSession.ClearLog($_.LogName) } catch {} }" > "%LOG_DIR%\s5.out" 2> "%LOG_DIR%\s5.err"
call :EVALUATE_STEP "s5" 2>nul
goto STEP6
:SKIP5
call :LOG_SKIP "Windows Olay Gunlukleri" 2>nul

:STEP6
<nul set /p="[6/18] DNS Onbellegi ....................................... "
IF /I "%CLEAN_DNS%" NEQ "ON" goto SKIP6
call :TIMER_START "DNS Onbellegi" 2>nul
ipconfig /flushdns > "%LOG_DIR%\s6.out" 2> "%LOG_DIR%\s6.err"
call :EVALUATE_STEP "s6" 2>nul
goto STEP7
:SKIP6
call :LOG_SKIP "DNS Onbellegi" 2>nul

:STEP7
<nul set /p="[7/18] [Gizlilik] Son Acilan Dosyalari Kapat .............. "
IF /I "%SET_EXPLORER_HISTORY_OFF%" NEQ "ON" goto SKIP7
call :TIMER_START "Son Acilan Dosyalari Kapat" 2>nul
(
    reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer" /v "ShowRecent" /t REG_DWORD /d 0 /f >nul 2>&1
    reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer" /v "ShowFrequent" /t REG_DWORD /d 0 /f >nul 2>&1
) > "%LOG_DIR%\s7.out" 2> "%LOG_DIR%\s7.err"
call :EVALUATE_STEP "s7" 2>nul
goto STEP8
:SKIP7
call :LOG_SKIP "Son Acilan Dosyalari Kapat (Gizlilik)" 2>nul

:STEP8
<nul set /p="[8/18] [Gizlilik] Baslat Menusu Uygulama Takibi ........... "
IF /I "%HIDE_RECENT_APPS%" NEQ "ON" goto SKIP8
call :TIMER_START "Baslat Menusu Uygulama Takibi" 2>nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "Start_TrackProgs" /t REG_DWORD /d 0 /f > "%LOG_DIR%\s8.out" 2> "%LOG_DIR%\s8.err"
call :EVALUATE_STEP "s8" 2>nul
goto STEP9
:SKIP8
call :LOG_SKIP "Baslat Menusu Uygulama Takibi (Gizlilik)" 2>nul

:STEP9
<nul set /p="[9/18] Windows Hata Raporlari ve Bellek Dokumleri .......... "
IF /I "%CLEAN_CRASH_DUMPS%" NEQ "ON" goto SKIP9
call :TIMER_START "Windows Hata Raporlari ve Bellek Dokumleri" 2>nul
(
    call :SMART_DELETE_FILE "C:\Windows\MEMORY.DMP"
    call :SMART_DELETE "C:\Windows\Minidump"
    call :SMART_DELETE "%LOCALAPPDATA%\Microsoft\Windows\WER"
    call :SMART_DELETE "C:\ProgramData\Microsoft\Windows\WER"
) > "%LOG_DIR%\s9.out" 2> "%LOG_DIR%\s9.err"
call :EVALUATE_STEP "s9" 2>nul
goto STEP10
:SKIP9
call :LOG_SKIP "Windows Hata Raporlari ve Bellek Dokumleri" 2>nul

:STEP10
<nul set /p="[10/18] Teslim Iyilestirme (Delivery Opt.) Onbellegi ...... "
IF /I "%CLEAN_DELIVERY_OPT%" NEQ "ON" goto SKIP10
call :TIMER_START "Teslim Iyilestirme Onbellegi" 2>nul
(
    net stop dosvc >nul 2>&1
    call :SMART_DELETE "C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache"
    net start dosvc >nul 2>&1
) > "%LOG_DIR%\s10.out" 2> "%LOG_DIR%\s10.err"
call :EVALUATE_STEP "s10" 2>nul
goto STEP11
:SKIP10
call :LOG_SKIP "Teslim Iyilestirme Onbellegi" 2>nul

:STEP11
<nul set /p="[11/18] DirectX ve Ekran Karti Shader Onbellegi ............ "
IF /I "%CLEAN_SHADER_CACHE%" NEQ "ON" goto SKIP11
call :TIMER_START "DirectX ve Ekran Karti Shader Onbellegi" 2>nul
(
    call :SMART_DELETE "%LOCALAPPDATA%\D3DSCache"
    call :SMART_DELETE "%LOCALAPPDATA%\NVIDIA\GLCache"
    call :SMART_DELETE "%LOCALAPPDATA%\NVIDIA Corporation\NV_Cache"
    call :SMART_DELETE "%LOCALAPPDATA%\AMD\DXCache"
    call :SMART_DELETE "%LOCALAPPDATA%\Intel\ShaderCache"
    call :SMART_DELETE "%LOCALAPPDATA%\Microsoft\DirectX Shader Cache"
) > "%LOG_DIR%\s11.out" 2> "%LOG_DIR%\s11.err"
call :EVALUATE_STEP "s11" 2>nul
goto STEP12
:SKIP11
call :LOG_SKIP "DirectX ve Ekran Karti Shader Onbellegi" 2>nul

:STEP12
<nul set /p="[12/18] Kucuk Resim ve Simge Onbellegi (Explorer Yenile) ... "
IF /I "%CLEAN_THUMB_ICON%" NEQ "ON" goto SKIP12
call :TIMER_START "Kucuk Resim ve Simge Onbellegi" 2>nul
(
    taskkill /f /im explorer.exe >nul 2>&1
    call :SMART_DELETE_FILE "%LOCALAPPDATA%\IconCache.db"
    for /f "delims=" %%F in ('dir /b "%LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db" 2^>nul') do call :SMART_DELETE_FILE "%LOCALAPPDATA%\Microsoft\Windows\Explorer\%%F"
    start explorer.exe >nul 2>&1
) > "%LOG_DIR%\s12.out" 2> "%LOG_DIR%\s12.err"
call :EVALUATE_STEP "s12" 2>nul
goto STEP13
:SKIP12
call :LOG_SKIP "Kucuk Resim ve Simge Onbellegi" 2>nul

:STEP13
<nul set /p="[13/18] Windows Prefetch Onbellegi ........................ "
IF /I "%CLEAN_PREFETCH%" NEQ "ON" goto SKIP13
call :TIMER_START "Windows Prefetch Onbellegi" 2>nul
call :SMART_DELETE "C:\Windows\Prefetch" > "%LOG_DIR%\s13.out" 2> "%LOG_DIR%\s13.err"
call :EVALUATE_STEP "s13" 2>nul
goto STEP14
:SKIP13
call :LOG_SKIP "Windows Prefetch Onbellegi" 2>nul

:STEP14
<nul set /p="[14/18] ARP Tablosu ve Ag Onbellegi ....................... "
IF /I "%CLEAN_NET_CACHE%" NEQ "ON" goto SKIP14
call :TIMER_START "ARP Tablosu ve Ag Onbellegi" 2>nul
(
    arp -d * >nul 2>&1
    nbtstat -R >nul 2>&1
    nbtstat -RR >nul 2>&1
) > "%LOG_DIR%\s14.out" 2> "%LOG_DIR%\s14.err"
call :EVALUATE_STEP "s14" 2>nul
goto STEP15
:SKIP14
call :LOG_SKIP "ARP Tablosu ve Ag Onbellegi" 2>nul

:STEP15
<nul set /p="[15/18] Windows Defender Tarama Gecmisi ................... "
IF /I "%CLEAN_DEFENDER%" NEQ "ON" goto SKIP15
call :TIMER_START "Windows Defender Tarama Gecmisi" 2>nul
(
    takeown /f "C:\ProgramData\Microsoft\Windows Defender\Scans\History\Service\DetectionHistory" /r /d y >nul 2>&1
    icacls "C:\ProgramData\Microsoft\Windows Defender\Scans\History\Service\DetectionHistory" /grant administrators:F /t >nul 2>&1
    call :SMART_DELETE "C:\ProgramData\Microsoft\Windows Defender\Scans\History\Service\DetectionHistory"
    call :SMART_DELETE "C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\Quick"
    call :SMART_DELETE "C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\Resource"
    wevtutil.exe cl "Microsoft-Windows-Windows Defender/Operational" >nul 2>&1
) > "%LOG_DIR%\s15.out" 2> "%LOG_DIR%\s15.err"
call :EVALUATE_STEP "s15" 2>nul
goto STEP16
:SKIP15
call :LOG_SKIP "Windows Defender Tarama Gecmisi" 2>nul

:STEP16
<nul set /p="[16/18] DISM Bilesen Temizligi (WinSxS - 2-5 dk surebilir) .. "
IF /I "%CLEAN_DISM%" NEQ "ON" goto SKIP16
call :TIMER_START "DISM Bilesen Temizligi" 2>nul
dism /online /cleanup-image /startcomponentcleanup /quiet > "%LOG_DIR%\s16.out" 2> "%LOG_DIR%\s16.err"
call :EVALUATE_STEP "s16" 2>nul
goto STEP17
:SKIP16
call :LOG_SKIP "DISM Bilesen Temizligi" 2>nul

:STEP17
<nul set /p="[17/18] Web Tarayici Onbellekleri (Chrome, Edge, Brave) ... "
IF /I "%CLEAN_BROWSERS%" NEQ "ON" goto SKIP17
call :TIMER_START "Web Tarayici Onbellekleri" 2>nul
(
    call :SMART_DELETE "%LOCALAPPDATA%\Google\Chrome\User Data\Default\Cache"
    call :SMART_DELETE "%LOCALAPPDATA%\Google\Chrome\User Data\Default\Code Cache"
    call :SMART_DELETE "%LOCALAPPDATA%\Google\Chrome\User Data\Default\GPUCache"
    call :SMART_DELETE "%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Cache"
    call :SMART_DELETE "%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Code Cache"
    call :SMART_DELETE "%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\GPUCache"
    call :SMART_DELETE "%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\Cache"
    call :SMART_DELETE "%APPDATA%\Opera Software\Opera Stable\Cache"
) > "%LOG_DIR%\s17.out" 2> "%LOG_DIR%\s17.err"
call :EVALUATE_STEP "s17" 2>nul
goto STEP18
:SKIP17
call :LOG_SKIP "Web Tarayici Onbellekleri" 2>nul

:STEP18
<nul set /p="[18/18] Eski Windows Surum Kalintilari (Windows.old) ...... "
IF /I "%CLEAN_WIN_OLD%" NEQ "ON" goto SKIP18
call :TIMER_START "Eski Windows Surum Kalintilari" 2>nul
(
    if exist "C:\Windows.old" (
        takeown /F "C:\Windows.old" /A /R /D Y >nul 2>&1
        icacls "C:\Windows.old" /grant *S-1-5-32-544:F /T /C /Q >nul 2>&1
        call :SMART_DELETE "C:\Windows.old"
    )
    call :SMART_DELETE "C:\$Windows.~BT"
    call :SMART_DELETE "C:\$Windows.~WS"
) > "%LOG_DIR%\s18.out" 2> "%LOG_DIR%\s18.err"
call :EVALUATE_STEP "s18" 2>nul
goto END
:SKIP18
call :LOG_SKIP "Eski Windows Surum Kalintilari" 2>nul

:END
set "t=%TIME: =0%"
set "END_CLOCK=%t:~0,8%"
for /f "tokens=1-4 delims=:,." %%a in ("%t%") do (
    set /a "E_SEC=(((1%%a-100)*60)+(1%%b-100))*60+(1%%c-100)"
    set /a "E_CSEC=1%%d-100"
)
set /a "D_SEC=E_SEC-TOTAL_S_SEC"
set /a "D_CSEC=E_CSEC-TOTAL_S_CSEC"
if %D_CSEC% lss 0 (
    set /a "D_SEC-=1"
    set /a "D_CSEC+=100"
)
if %D_SEC% lss 0 set /a "D_SEC+=86400"
set /a "MIN=D_SEC/60"
set /a "SEC=D_SEC%%60"
if %D_CSEC% lss 10 (set "CSEC_STR=0%D_CSEC%") else (set "CSEC_STR=%D_CSEC%")
if %MIN% equ 0 (set "TOTAL_TIME=%SEC%.%CSEC_STR% sn") else (set "TOTAL_TIME=%MIN% dk %SEC%.%CSEC_STR% sn")

:: Temizlik sonrasi C: bos alanini ve kazanilan alani hesapla
for /f "delims=" %%B in ('PowerShell -NoProfile -Command "([System.IO.DriveInfo]'C').AvailableFreeSpace"') do set "FINAL_FREE_BYTES=%%B"
for /f "delims=" %%G in ('PowerShell -NoProfile -Command "$d = ([int64]%FINAL_FREE_BYTES% - [int64]%INITIAL_FREE_BYTES%); if ($d -gt 1073741824) { [string][math]::Round($d / 1GB, 2) + ' GB' } elseif ($d -gt 1048576) { [string][math]::Round($d / 1MB, 2) + ' MB' } elseif ($d -gt 0) { [string][math]::Round($d / 1KB, 2) + ' KB' } else { 'Onbellek Temizlendi' }"') do set "FREED_SPACE_STR=%%G"

if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 4 echo [INFO] [%DATE% %END_CLOCK%] Tum Islemler Tamamlandi. Sure: %TOTAL_TIME%, Kazanc: %FREED_SPACE_STR% >> "%MASTER_LOG%" 2>nul

del /f /q "%LOG_DIR%\s*.out" "%LOG_DIR%\s*.err" "%LOG_DIR%\step_*.log" >nul 2>&1

echo.
echo %C_LINE%================================================================================%C_RESET%
echo %C_GREEN%           [LuckyStrike-Windows-CleanUP] TUM ISLEMLER TAMAMLANDI!          %C_RESET%
echo %C_WHITE%           Bitis Saati    : %END_CLOCK%                                     %C_RESET%
echo %C_WHITE%           Toplam Sure    : %TOTAL_TIME%                                    %C_RESET%
echo %C_GREEN%           Kazanilan Alan : %FREED_SPACE_STR%                               %C_RESET%
if /I "%ENABLE_LOGGING%"=="ON" (
    echo %C_WHITE%           Log Konumu     : %%APPDATA%%\LuckyCleanTemp\LuckyStrike-Windows-CleanUP_Log.txt %C_RESET%
) else (
    echo %C_WHITE%           Log Durumu     : Kapali                                      %C_RESET%
)
echo %C_LINE%--------------------------------------------------------------------------------%C_RESET%
echo %C_YELLOW% Ana menuye donmek icin herhangi bir tusa basiniz...                       %C_RESET%
echo %C_LINE%================================================================================%C_RESET%
pause >nul
goto MAIN_MENU

:: ==========================================
:: ALT PROGRAMLAR
:: ==========================================
:SMART_DELETE
if not exist "%~1" goto :eof
for /f "delims=" %%F in ('dir /b /s /a-d "%~1\*.*" 2^>nul') do (
    del /f /q "%%F" >nul 2>&1
    if errorlevel 1 (
        if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 3 echo [WARNING] Kilitli Dosya Atlandi: %%F >> "%MASTER_LOG%" 2>nul
    ) else (
        if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% EQU 5 echo [DEBUG-DEL] Silindi: %%F >> "%MASTER_LOG%" 2>nul
    )
)
for /f "delims=" %%D in ('dir /b /s /ad "%~1\*.*" 2^>nul ^| sort /r') do rmdir /q "%%D" >nul 2>&1
goto :eof

:SMART_DELETE_FILE
if not exist "%~1" goto :eof
del /f /q "%~1" >nul 2>&1
if errorlevel 1 (
    if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 3 echo [WARNING] Kilitli Dosya Atlandi: %~1 >> "%MASTER_LOG%" 2>nul
) else (
    if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% EQU 5 echo [DEBUG-DEL] Silindi: %~1 >> "%MASTER_LOG%" 2>nul
)
goto :eof

:TIMER_START
set "CUR_STEP=%~1"
set "t=%TIME: =0%"
for /f "tokens=1-4 delims=:,." %%a in ("%t%") do (
    set /a "S_SEC=(((1%%a-100)*60)+(1%%b-100))*60+(1%%c-100)"
    set /a "S_CSEC=1%%d-100"
)
if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 4 echo [INFO] [%TIME:~0,8%] Baslatildi: %CUR_STEP% >> "%MASTER_LOG%" 2>nul
goto :eof

:LOG_SKIP
echo [ATLANDI]
if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 3 echo [WARNING] [%TIME:~0,8%] Atlandi: %~1 >> "%MASTER_LOG%" 2>nul
echo --------------------------------------------------------------------------------
goto :eof

:EVALUATE_STEP
set "PREFIX=%~1"
set "t=%TIME: =0%"
for /f "tokens=1-4 delims=:,." %%a in ("%t%") do (
    set /a "E_SEC=(((1%%a-100)*60)+(1%%b-100))*60+(1%%c-100)"
    set /a "E_CSEC=1%%d-100"
)
set /a "D_SEC=E_SEC-S_SEC"
set /a "D_CSEC=E_CSEC-S_CSEC"
if %D_CSEC% lss 0 (
    set /a "D_SEC-=1"
    set /a "D_CSEC+=100"
)
if %D_SEC% lss 0 set /a "D_SEC+=86400"
if %D_CSEC% lss 10 (set "STEP_TIME=%D_SEC%.0%D_CSEC% sn") else (set "STEP_TIME=%D_SEC%.%D_CSEC% sn")

if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% EQU 5 (
    if exist "%LOG_DIR%\%PREFIX%.out" (
        for %%A in ("%LOG_DIR%\%PREFIX%.out") do if %%~zA GTR 0 (
            echo [DEBUG-OUT] --- %CUR_STEP% Ciktilari --- >> "%MASTER_LOG%" 2>nul
            (for /f "usebackq delims=" %%O in ("%LOG_DIR%\%PREFIX%.out") do echo [DEBUG-OUT] %%O >> "%MASTER_LOG%") 2>nul
        )
    )
)

type nul > "%LOG_DIR%\step_real_err.log" 2>nul
type nul > "%LOG_DIR%\step_warn.log" 2>nul

if exist "%LOG_DIR%\%PREFIX%.err" (
    for %%A in ("%LOG_DIR%\%PREFIX%.err") do if %%~zA GTR 0 (
        findstr /i /c:"kullan" /c:"bulam" /c:"Could Not Find" /c:"unable to find" /c:"engellendi" /c:"Access is denied" /c:"Failed to clear" /c:"erisem" /c:"belirtilen" "%LOG_DIR%\%PREFIX%.err" > "%LOG_DIR%\step_warn.log" 2>nul
        findstr /i /v /c:"kullan" /c:"bulam" /c:"Could Not Find" /c:"unable to find" /c:"engellendi" /c:"Access is denied" /c:"Failed to clear" /c:"erisem" /c:"belirtilen" "%LOG_DIR%\%PREFIX%.err" > "%LOG_DIR%\step_real_err.log" 2>nul
    )
)

if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 3 (
    if exist "%LOG_DIR%\step_warn.log" (
        for %%A in ("%LOG_DIR%\step_warn.log") do if %%~zA GTR 0 (
            (for /f "usebackq delims=" %%W in ("%LOG_DIR%\step_warn.log") do echo [WARNING] %%W >> "%MASTER_LOG%") 2>nul
        )
    )
)

set "HAS_ERR=0"
if exist "%LOG_DIR%\step_real_err.log" (
    for %%A in ("%LOG_DIR%\step_real_err.log") do if %%~zA GTR 0 set "HAS_ERR=1"
)

if "%HAS_ERR%"=="1" (
    echo [HATA] (%STEP_TIME%)
    if exist "%LOG_DIR%\step_real_err.log" (
        (for /f "usebackq delims=" %%E in ("%LOG_DIR%\step_real_err.log") do (
            echo        [LOG] -^> %%E
            if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 2 echo [ERROR] [%TIME:~0,8%] %CUR_STEP%: %%E >> "%MASTER_LOG%" 2>nul
        )) 2>nul
    )
) else (
    echo [TAMAMLANDI] (%STEP_TIME%)
    if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 4 echo [INFO] [%TIME:~0,8%] Tamamlandi (%STEP_TIME%): %CUR_STEP% >> "%MASTER_LOG%" 2>nul
)
echo --------------------------------------------------------------------------------
goto :eof
