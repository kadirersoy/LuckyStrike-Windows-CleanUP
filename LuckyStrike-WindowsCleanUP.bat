@echo off
title Lucky-CleanUP v2.0 - Gelismis Sistem Bakimi
color 0B
chcp 65001 >nul 2>&1

:: Log klasörünü ve master log dosyasını hazırla
SET "LOG_DIR=%APPDATA%\LuckyCleanTemp"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" >nul 2>&1
SET "MASTER_LOG=%LOG_DIR%\LuckyCleanUP_Log.txt"

:: ==========================================
:: PARAMETRELER
:: ==========================================
SET ENABLE_LOGGING=ON
:: ON = Log yazar, OFF = Log dosyası oluşturmaz

SET LOG_LEVEL=5
:: 1=FATAL, 2=ERROR, 3=WARNING, 4=INFO, 5=DEBUG

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
:: ==========================================

:: Baslangic zamanini kaydet
set "t=%TIME: =0%"
set "START_CLOCK=%t:~0,8%"
for /f "tokens=1-4 delims=:,." %%a in ("%t%") do (
    set /a "TOTAL_S_SEC=(((1%%a-100)*60)+(1%%b-100))*60+(1%%c-100)"
    set /a "TOTAL_S_CSEC=1%%d-100"
)

:: Master log dosyasini baslat
if /I "%ENABLE_LOGGING%"=="ON" (
    echo ================================================================================ > "%MASTER_LOG%" 2>nul
    echo [INFO] [%DATE% %START_CLOCK%] Lucky-CleanUP v2.0 Baslatildi (Log Level: %LOG_LEVEL%) >> "%MASTER_LOG%" 2>nul
    echo ================================================================================ >> "%MASTER_LOG%" 2>nul
)

echo ================================================================================
echo.
echo   +--------------------------------------------------------------------------+
echo   ^|                                                                          ^|
echo   ^|                  L U C K Y   -   C L E A N   U P                         ^|
echo   ^|             Gelismis Sistem Bakim ve Temizleme Araci                     ^|
echo   ^|                            Surum v2.0                                    ^|
echo   ^|                                                                          ^|
echo   +--------------------------------------------------------------------------+
echo.
echo ================================================================================
echo           [LUCKY-CLEANUP] TEMIZLIK ISLEMLERI BASLIYOR...
echo           Baslangic Saati: %START_CLOCK%
echo ================================================================================
echo.

:STEP1
<nul set /p="[1/16] Windows ve Kullanici Temp Klasorleri ................ "
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
<nul set /p="[2/16] Windows Update Onbellek Dosyalari ................... "
IF /I "%CLEAN_UPDATE%" NEQ "ON" goto SKIP2
call :TIMER_START "Windows Update Onbellek Dosyalari" 2>nul
(
    net stop wuauserv >nul 2>&1
    call :SMART_DELETE "C:\Windows\SoftwareDistribution\Download"
    net start wuauserv >nul 2>&1
) > "%LOG_DIR%\s2.out" 2> "%LOG_DIR%\s2.err"
call :EVALUATE_STEP "s2" 2>nul
goto STEP3
:SKIP2
call :LOG_SKIP "Windows Update Onbellek Dosyalari" 2>nul

:STEP3
<nul set /p="[3/16] Dosya Gezgini Gecmisi ve Baslat Menusu Onerileri .... "
IF /I "%CLEAN_EXPLORER_RECENT%" NEQ "ON" goto SKIP3
call :TIMER_START "Dosya Gezgini Gecmisi ve Baslat Menusu Onerileri" 2>nul
(
    call :SMART_DELETE "%APPDATA%\Microsoft\Windows\Recent"
    reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU" /va /f >nul 2>&1
) > "%LOG_DIR%\s3.out" 2> "%LOG_DIR%\s3.err"
call :EVALUATE_STEP "s3" 2>nul
goto STEP4
:SKIP3
call :LOG_SKIP "Dosya Gezgini Gecmisi ve Baslat Menusu Onerileri" 2>nul

:STEP4
<nul set /p="[4/16] Tum Disklerdeki Geri Donusum Kutulari ............... "
IF /I "%CLEAN_RECYCLE%" NEQ "ON" goto SKIP4
call :TIMER_START "Tum Disklerdeki Geri Donusum Kutulari" 2>nul
PowerShell.exe -NoProfile -Command "Clear-RecycleBin -Force -ErrorAction SilentlyContinue" > "%LOG_DIR%\s4.out" 2> "%LOG_DIR%\s4.err"
call :EVALUATE_STEP "s4" 2>nul
goto STEP5
:SKIP4
call :LOG_SKIP "Tum Disklerdeki Geri Donusum Kutulari" 2>nul

:STEP5
<nul set /p="[5/16] Windows Olay Gunlukleri (Loglar) .................... "
IF /I "%CLEAN_LOGS%" NEQ "ON" goto SKIP5
call :TIMER_START "Windows Olay Gunlukleri" 2>nul
(for /F "tokens=*" %%G in ('wevtutil.exe el') DO wevtutil.exe cl "%%G" >nul 2>&1) > "%LOG_DIR%\s5.out" 2> "%LOG_DIR%\s5.err"
call :EVALUATE_STEP "s5" 2>nul
goto STEP6
:SKIP5
call :LOG_SKIP "Windows Olay Gunlukleri" 2>nul

:STEP6
<nul set /p="[6/16] DNS Onbellegi ....................................... "
IF /I "%CLEAN_DNS%" NEQ "ON" goto SKIP6
call :TIMER_START "DNS Onbellegi" 2>nul
ipconfig /flushdns > "%LOG_DIR%\s6.out" 2> "%LOG_DIR%\s6.err"
call :EVALUATE_STEP "s6" 2>nul
goto STEP7
:SKIP6
call :LOG_SKIP "DNS Onbellegi" 2>nul

:STEP7
<nul set /p="[7/16] Klasor Gezgininde Son Acilan Dosya Gecmisi .......... "
IF /I "%CLEAN_EXPLORER_HISTORY%" NEQ "ON" goto SKIP7
call :TIMER_START "Klasor Gezgininde Son Acilan Dosya Gecmisi" 2>nul
(
    reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer" /v "ShowRecent" /t REG_DWORD /d 0 /f >nul 2>&1
    reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer" /v "ShowFrequent" /t REG_DWORD /d 0 /f >nul 2>&1
) > "%LOG_DIR%\s7.out" 2> "%LOG_DIR%\s7.err"
call :EVALUATE_STEP "s7" 2>nul
goto STEP8
:SKIP7
call :LOG_SKIP "Klasor Gezgininde Son Acilan Dosya Gecmisi" 2>nul

:STEP8
<nul set /p="[8/16] Baslat Menusu Son Kurulan Uygulamalar Gizleme ....... "
IF /I "%HIDE_RECENT_APPS%" NEQ "ON" goto SKIP8
call :TIMER_START "Baslat Menusu Son Kurulan Uygulamalar Gizleme" 2>nul
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced" /v "Start_TrackProgs" /t REG_DWORD /d 0 /f > "%LOG_DIR%\s8.out" 2> "%LOG_DIR%\s8.err"
call :EVALUATE_STEP "s8" 2>nul
goto STEP9
:SKIP8
call :LOG_SKIP "Baslat Menusu Son Kurulan Uygulamalar Gizleme" 2>nul

:STEP9
<nul set /p="[9/16] Windows Hata Raporlari ve Bellek Dokumleri .......... "
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
<nul set /p="[10/16] Teslim Iyilestirme (Delivery Opt.) Onbellegi ....... "
IF /I "%CLEAN_DELIVERY_OPT%" NEQ "ON" goto SKIP10
call :TIMER_START "Teslim Iyilestirme Onbellegi" 2>nul
call :SMART_DELETE "C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache" > "%LOG_DIR%\s10.out" 2> "%LOG_DIR%\s10.err"
call :EVALUATE_STEP "s10" 2>nul
goto STEP11
:SKIP10
call :LOG_SKIP "Teslim Iyilestirme Onbellegi" 2>nul

:STEP11
<nul set /p="[11/16] DirectX ve Ekran Karti Shader Onbellegi ............ "
IF /I "%CLEAN_SHADER_CACHE%" NEQ "ON" goto SKIP11
call :TIMER_START "DirectX ve Ekran Karti Shader Onbellegi" 2>nul
(
    call :SMART_DELETE "%LOCALAPPDATA%\D3DSCache"
    call :SMART_DELETE "%LOCALAPPDATA%\NVIDIA\GLCache"
    call :SMART_DELETE "%LOCALAPPDATA%\NVIDIA Corporation\NV_Cache"
    call :SMART_DELETE "%LOCALAPPDATA%\AMD\DXCache"
) > "%LOG_DIR%\s11.out" 2> "%LOG_DIR%\s11.err"
call :EVALUATE_STEP "s11" 2>nul
goto STEP12
:SKIP11
call :LOG_SKIP "DirectX ve Ekran Karti Shader Onbellegi" 2>nul

:STEP12
<nul set /p="[12/16] Kucuk Resim ve Simge Onbellegi ..................... "
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
<nul set /p="[13/16] Windows Prefetch Onbellegi ......................... "
IF /I "%CLEAN_PREFETCH%" NEQ "ON" goto SKIP13
call :TIMER_START "Windows Prefetch Onbellegi" 2>nul
call :SMART_DELETE "C:\Windows\Prefetch" > "%LOG_DIR%\s13.out" 2> "%LOG_DIR%\s13.err"
call :EVALUATE_STEP "s13" 2>nul
goto STEP14
:SKIP13
call :LOG_SKIP "Windows Prefetch Onbellegi" 2>nul

:STEP14
<nul set /p="[14/16] ARP Tablosu ve Ag Onbellegi ........................ "
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
<nul set /p="[15/16] Windows Defender Tarama Gecmisi .................... "
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
<nul set /p="[16/16] DISM Bilesen Temizligi ............................. "
IF /I "%CLEAN_DISM%" NEQ "ON" goto SKIP16
call :TIMER_START "DISM Bilesen Temizligi" 2>nul
dism /online /cleanup-image /startcomponentcleanup /quiet > "%LOG_DIR%\s16.out" 2> "%LOG_DIR%\s16.err"
call :EVALUATE_STEP "s16" 2>nul
goto END
:SKIP16
call :LOG_SKIP "DISM Bilesen Temizligi" 2>nul

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

if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 4 echo [INFO] [%DATE% %END_CLOCK%] Tum Islemler Tamamlandi. Toplam Sure: %TOTAL_TIME% >> "%MASTER_LOG%" 2>nul

:: Gecici adim loglarini temizle
del /f /q "%LOG_DIR%\s*.out" "%LOG_DIR%\s*.err" "%LOG_DIR%\step_*.log" >nul 2>&1

echo ================================================================================
echo           [LUCKY-CLEANUP] TUM ISLEMLER TAMAMLANDI!
echo           Bitis Saati : %END_CLOCK%
echo           Toplam Sure : %TOTAL_TIME%
if /I "%ENABLE_LOGGING%"=="ON" (
    echo           Log Konumu  : %MASTER_LOG%
) else (
    echo           Log Durumu  : Kapali
)
echo ================================================================================
echo.
echo Press [R] to Reboot, [S] to Shutdown, or any other key to Exit...
PowerShell -NoProfile -Command "$k = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown'); if ($k.Character -eq 'R' -or $k.Character -eq 'r') { exit 82 } elseif ($k.Character -eq 'S' -or $k.Character -eq 's') { exit 83 } else { exit 0 }"

if %ERRORLEVEL% EQU 82 (
    if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 4 echo [INFO] [%TIME:~0,8%] Kullanici istegiyle sistem yeniden baslatiliyor... >> "%MASTER_LOG%" 2>nul
    shutdown /r /t 0
    goto :eof
)
if %ERRORLEVEL% EQU 83 (
    if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 4 echo [INFO] [%TIME:~0,8%] Kullanici istegiyle sistem kapatiliyor... >> "%MASTER_LOG%" 2>nul
    shutdown /s /t 0
    goto :eof
)
goto :eof

:: ==========================================
:: ALT PROGRAMLAR (SÜRE, AKILLI SİLME & LOG)
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
if /I "%ENABLE_LOGGING%"=="ON" if %LOG_LEVEL% GEQ 3 echo [WARNING] [%TIME:~0,8%] Atlandi (Parametre Kapali): %~1 >> "%MASTER_LOG%" 2>nul
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
