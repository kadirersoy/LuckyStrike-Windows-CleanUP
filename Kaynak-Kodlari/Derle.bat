@echo off
chcp 65001 >nul
echo ===================================================
echo [INFO] LuckyStrike CleanUP v3.0 Derleniyor...
echo ===================================================

cd /d "%~dp0"
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" "LuckyStrikeCleanUp.csproj" /p:Configuration=Release /p:Platform=x64 /v:minimal

if %ERRORLEVEL% EQU 0 (
    if not exist "..\Uygulama" mkdir "..\Uygulama"
    copy /y "bin\Release\LuckyStrike-CleanUP.exe" "..\Uygulama\LuckyStrike-CleanUP.exe" >nul
    echo.
    echo ===================================================
    echo [BASARILI] Yeni surum basariyla olusturuldu!
    echo Konum: Uygulama\LuckyStrike-CleanUP.exe
    echo ===================================================
) else (
    echo.
    echo [HATA] Derleme basarisiz oldu.
)

echo.
pause
