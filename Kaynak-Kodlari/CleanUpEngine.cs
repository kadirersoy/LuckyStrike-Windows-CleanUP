using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace LuckyStrikeCleanUp
{
    public enum StepStatus
    {
        Idle,
        Running,
        Completed,
        Warning,
        Error,
        Skipped
    }

    public class CleanStepItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        private bool _isSelected = true;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged("IsSelected"); }
        }

        private StepStatus _status = StepStatus.Idle;
        public StepStatus Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); OnPropertyChanged("StatusText"); }
        }

        private string _duration = "";
        public string Duration
        {
            get { return _duration; }
            set { _duration = value; OnPropertyChanged("Duration"); }
        }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case StepStatus.Running: return "İşleniyor...";
                    case StepStatus.Completed: return string.IsNullOrEmpty(Duration) ? "Tamamlandı" : string.Format("Tamamlandı ({0})", Duration);
                    case StepStatus.Warning: return string.IsNullOrEmpty(Duration) ? "Uyarı" : string.Format("Uyarı ({0})", Duration);
                    case StepStatus.Error: return string.IsNullOrEmpty(Duration) ? "Hata" : string.Format("Hata ({0})", Duration);
                    case StepStatus.Skipped: return "Atlandı";
                    default: return "Bekliyor";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class CleanUpEngine
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);

        const uint SHERB_NOCONFIRMATION = 0x00000001;
        const uint SHERB_NOPROGRESSUI = 0x00000002;
        const uint SHERB_NOSOUND = 0x00000004;

        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        static extern UInt32 DnsFlushResolverCache();

        public string LogDirectory { get; private set; }
        public string MasterLogFile { get; private set; }

        public Action<string, string> LogCallback { get; set; }
        public Action<int, string> ProgressCallback { get; set; }

        public CleanUpEngine()
        {
            LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuckyCleanTemp");
            MasterLogFile = Path.Combine(LogDirectory, "LuckyStrike-Windows-CleanUP_Log.txt");
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
            }
            catch { }
        }

        public static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public void RestartAsAdmin()
        {
            try
            {
                var proc = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(proc);
                Environment.Exit(0);
            }
            catch { }
        }

        public void WriteLog(string level, string message)
        {
            string line = string.Format("[{0:HH:mm:ss}] [{1}] {2}", DateTime.Now, level, message);
            try
            {
                File.AppendAllText(MasterLogFile, line + Environment.NewLine, Encoding.UTF8);
            }
            catch { }

            if (LogCallback != null)
            {
                LogCallback(level, message);
            }
        }

        public long GetFreeDiskSpaceBytes(string driveLetter = "C")
        {
            try
            {
                var drive = new DriveInfo(driveLetter);
                if (drive.IsReady)
                    return drive.AvailableFreeSpace;
            }
            catch { }
            return 0;
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 0) bytes = 0;
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n2} {1}", number, suffixes[counter]);
        }

        public async Task<bool> CreateRestorePointAsync()
        {
            return await Task.Run(() =>
            {
                WriteLog("INFO", "Sistem Geri Yükleme Noktası oluşturuluyor...");
                try
                {
                    int res = RunCommandSync("powershell.exe",
                        "-NoProfile -Command \"Checkpoint-Computer -Description 'LuckyStrike_CleanUP_Backup' -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop\"");
                    if (res == 0)
                    {
                        WriteLog("SUCCESS", "Sistem Geri Yükleme Noktası başarıyla oluşturuldu.");
                        return true;
                    }
                    else
                    {
                        WriteLog("WARN", "Geri yükleme noktası oluşturulamadı (Sistem koruması kapalı olabilir).");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("ERROR", "Geri yükleme noktası hatası: " + ex.Message);
                    return false;
                }
            });
        }

        public async Task<string> ExecuteStepsAsync(List<CleanStepItem> steps, CancellationToken token)
        {
            WriteLog("INFO", "================================================================================");
            WriteLog("INFO", string.Format("LuckyStrike-Windows-CleanUP v3.0 Temizlik Başlatıldı. Yönetici: {0}", IsAdministrator()));
            WriteLog("INFO", "================================================================================");

            long initialFree = GetFreeDiskSpaceBytes("C");
            var stopwatchTotal = Stopwatch.StartNew();
            int selectedCount = steps.Count(s => s.IsSelected);
            int currentStepIndex = 0;

            for (int i = 0; i < steps.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    WriteLog("WARN", "Kullanıcı tarafından temizlik durduruldu.");
                    break;
                }

                var step = steps[i];
                if (!step.IsSelected)
                {
                    step.Status = StepStatus.Skipped;
                    step.Duration = "";
                    continue;
                }

                currentStepIndex++;
                int percent = (int)((double)currentStepIndex / selectedCount * 100);
                if (ProgressCallback != null)
                {
                    ProgressCallback(percent, string.Format("[{0}/{1}] {2}", step.Id, steps.Count, step.Title));
                }

                step.Status = StepStatus.Running;
                WriteLog("INFO", string.Format("Adım {0}/{1} Başlatıldı: {2}", step.Id, steps.Count, step.Title));

                var stepStopwatch = Stopwatch.StartNew();
                StepStatus resultStatus = StepStatus.Completed;

                try
                {
                    await Task.Run(() =>
                    {
                        ExecuteStepById(step.Id, out resultStatus);
                    }, token);
                }
                catch (OperationCanceledException)
                {
                    step.Status = StepStatus.Skipped;
                    break;
                }
                catch (Exception ex)
                {
                    resultStatus = StepStatus.Error;
                    WriteLog("ERROR", string.Format("Adım {0} sırasında hata: {1}", step.Id, ex.Message));
                }

                stepStopwatch.Stop();
                step.Duration = string.Format("{0:F2} sn", stepStopwatch.Elapsed.TotalSeconds);
                step.Status = resultStatus;

                WriteLog(resultStatus == StepStatus.Completed ? "SUCCESS" : (resultStatus == StepStatus.Warning ? "WARN" : "ERROR"),
                    string.Format("Adım {0}/{1} Bitti ({2}): {3} [{4}]", step.Id, steps.Count, step.Duration, step.Title, step.StatusText));
            }

            stopwatchTotal.Stop();
            long finalFree = GetFreeDiskSpaceBytes("C");
            long freedBytes = finalFree - initialFree;
            string freedSpaceStr = freedBytes > 0 ? FormatBytes(freedBytes) : "Önbellek temizlendi (kayda değer boş alan kazanımı)";

            string totalTimeStr = stopwatchTotal.Elapsed.TotalMinutes >= 1
                ? string.Format("{0} dk {1} sn", (int)stopwatchTotal.Elapsed.TotalMinutes, stopwatchTotal.Elapsed.Seconds)
                : string.Format("{0:F2} sn", stopwatchTotal.Elapsed.TotalSeconds);

            if (ProgressCallback != null)
            {
                ProgressCallback(100, "Tüm işlemler tamamlandı!");
            }

            WriteLog("INFO", "================================================================================");
            WriteLog("SUCCESS", string.Format("Tüm işlemler tamamlandı! Toplam Süre: {0}", totalTimeStr));
            if (freedBytes > 0)
            {
                WriteLog("SUCCESS", string.Format("Kazanılan Disk Alanı (C:): {0}", freedSpaceStr));
            }
            WriteLog("INFO", string.Format("Log Dosyası: {0}", MasterLogFile));
            WriteLog("INFO", "================================================================================");

            return freedBytes > 0 ? freedSpaceStr : "";
        }

        private void ExecuteStepById(int stepId, out StepStatus status)
        {
            status = StepStatus.Completed;
            bool hasWarning = false;

            switch (stepId)
            {
                case 1: // Windows ve Kullanıcı Temp Klasörleri
                    SmartDeleteDirectory(@"C:\Windows\Temp", ref hasWarning);
                    SmartDeleteDirectory(Path.GetTempPath(), ref hasWarning);
                    break;

                case 2: // Windows Update Ön Belleği (wuauserv, bits, dosvc güvenli durdurulup açılır)
                    RunCommandSync("net", "stop wuauserv");
                    RunCommandSync("net", "stop bits");
                    RunCommandSync("net", "stop dosvc");
                    SmartDeleteDirectory(@"C:\Windows\SoftwareDistribution\Download", ref hasWarning);
                    RunCommandSync("net", "start wuauserv");
                    RunCommandSync("net", "start bits");
                    RunCommandSync("net", "start dosvc");
                    break;

                case 3: // Dosya Gezgini Geçmişi ve RunMRU
                    string recent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Recent");
                    SmartDeleteDirectory(recent, ref hasWarning);
                    try
                    {
                        using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU", true))
                        {
                            if (key != null)
                            {
                                foreach (var val in key.GetValueNames())
                                    key.DeleteValue(val, false);
                            }
                        }
                    }
                    catch (Exception ex) { WriteLog("WARN", "RunMRU uyarısı: " + ex.Message); hasWarning = true; }
                    break;

                case 4: // Tüm Disklerdeki Geri Dönüşüm Kutuları
                    try
                    {
                        SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);
                    }
                    catch (Exception ex)
                    {
                        WriteLog("WARN", "RecycleBin API uyarısı, PowerShell deneniyor: " + ex.Message);
                        RunCommandSync("powershell.exe", "-NoProfile -Command Clear-RecycleBin -Force -ErrorAction SilentlyContinue");
                    }
                    break;

                case 5: // Windows Olay Günlükleri (Hızlı .NET EventLog API / Tek komut PowerShell)
                    try
                    {
                        WriteLog("INFO", "Olay günlükleri hızlı motorla temizleniyor...");
                        int exitCode = RunCommandSync("powershell.exe",
                            "-NoProfile -Command \"Get-WinEvent -ListLog * -EA 0 | ForEach-Object { try { [System.Diagnostics.Eventing.Reader.EventLogSession]::GlobalSession.ClearLog($_.LogName) } catch {} }\"");
                        if (exitCode != 0) hasWarning = true;
                    }
                    catch (Exception ex) { WriteLog("WARN", "Olay günlükleri uyarısı: " + ex.Message); hasWarning = true; }
                    break;

                case 6: // DNS Ön Belleği
                    try
                    {
                        DnsFlushResolverCache();
                    }
                    catch { }
                    RunCommandSync("ipconfig", "/flushdns");
                    break;

                case 7: // [Gizlilik] Son Açılan Dosya Geçmişi Reg
                    try
                    {
                        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer"))
                        {
                            if (key != null)
                            {
                                key.SetValue("ShowRecent", 0, RegistryValueKind.DWord);
                                key.SetValue("ShowFrequent", 0, RegistryValueKind.DWord);
                            }
                        }
                    }
                    catch (Exception ex) { WriteLog("WARN", "ShowRecent reg uyarısı: " + ex.Message); hasWarning = true; }
                    break;

                case 8: // [Gizlilik] Başlat Menüsü Son Kurulan Uygulamalar
                    try
                    {
                        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                        {
                            if (key != null)
                            {
                                key.SetValue("Start_TrackProgs", 0, RegistryValueKind.DWord);
                            }
                        }
                    }
                    catch (Exception ex) { WriteLog("WARN", "Start_TrackProgs reg uyarısı: " + ex.Message); hasWarning = true; }
                    break;

                case 9: // Hata Raporları ve Bellek Dökümleri
                    SmartDeleteFile(@"C:\Windows\MEMORY.DMP", ref hasWarning);
                    SmartDeleteDirectory(@"C:\Windows\Minidump", ref hasWarning);
                    string localWer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\WER");
                    SmartDeleteDirectory(localWer, ref hasWarning);
                    SmartDeleteDirectory(@"C:\ProgramData\Microsoft\Windows\WER", ref hasWarning);
                    break;

                case 10: // Teslim İyileştirme (Delivery Optimization)
                    RunCommandSync("net", "stop dosvc");
                    SmartDeleteDirectory(@"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache", ref hasWarning);
                    RunCommandSync("net", "start dosvc");
                    break;

                case 11: // DirectX ve Ekran Kartı Shader Ön Belleği (NVIDIA + AMD + Intel + D3D)
                    string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    SmartDeleteDirectory(Path.Combine(localApp, "D3DSCache"), ref hasWarning);
                    SmartDeleteDirectory(Path.Combine(localApp, @"NVIDIA\GLCache"), ref hasWarning);
                    SmartDeleteDirectory(Path.Combine(localApp, @"NVIDIA Corporation\NV_Cache"), ref hasWarning);
                    SmartDeleteDirectory(Path.Combine(localApp, @"AMD\DXCache"), ref hasWarning);
                    SmartDeleteDirectory(Path.Combine(localApp, @"Intel\ShaderCache"), ref hasWarning);
                    SmartDeleteDirectory(Path.Combine(localApp, @"Microsoft\DirectX Shader Cache"), ref hasWarning);
                    break;

                case 12: // Küçük Resim ve Simge Ön Belleği (Explorer anlık güvenli yenilenir)
                    try
                    {
                        WriteLog("INFO", "Simge veritabanı kilitlerini açmak için Explorer geçici olarak yenileniyor...");
                        RunCommandSync("taskkill", "/f /im explorer.exe");
                        Thread.Sleep(500);

                        string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        SmartDeleteFile(Path.Combine(localData, "IconCache.db"), ref hasWarning);

                        string explorerCache = Path.Combine(localData, @"Microsoft\Windows\Explorer");
                        if (Directory.Exists(explorerCache))
                        {
                            var files = Directory.GetFiles(explorerCache, "thumbcache_*.db");
                            foreach (var f in files)
                            {
                                SmartDeleteFile(f, ref hasWarning);
                            }
                        }

                        // Explorer'ı hemen geri başlat
                        Process.Start("explorer.exe");
                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        WriteLog("WARN", "Simge temizliği uyarısı: " + ex.Message);
                        hasWarning = true;
                        try { Process.Start("explorer.exe"); } catch { }
                    }
                    break;

                case 13: // Windows Prefetch
                    SmartDeleteDirectory(@"C:\Windows\Prefetch", ref hasWarning);
                    break;

                case 14: // ARP Tablosu ve Ağ Ön Belleği
                    RunCommandSync("arp", "-d *");
                    RunCommandSync("nbtstat", "-R");
                    RunCommandSync("nbtstat", "-RR");
                    break;

                case 15: // Windows Defender Tarama Geçmişi
                    try
                    {
                        RunCommandSync("takeown", "/f \"C:\\ProgramData\\Microsoft\\Windows Defender\\Scans\\History\\Service\\DetectionHistory\" /r /d y");
                        RunCommandSync("icacls", "\"C:\\ProgramData\\Microsoft\\Windows Defender\\Scans\\History\\Service\\DetectionHistory\" /grant administrators:F /t");
                        SmartDeleteDirectory(@"C:\ProgramData\Microsoft\Windows Defender\Scans\History\Service\DetectionHistory", ref hasWarning);
                        SmartDeleteDirectory(@"C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\Quick", ref hasWarning);
                        SmartDeleteDirectory(@"C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\Resource", ref hasWarning);
                        RunCommandSync("wevtutil.exe", "cl \"Microsoft-Windows-Windows Defender/Operational\"");
                    }
                    catch (Exception ex) { WriteLog("WARN", "Defender uyarısı: " + ex.Message); hasWarning = true; }
                    break;

                case 16: // DISM Bileşen Temizliği (Açıklayıcı konsol uyarısı ile)
                    WriteLog("INFO", "DISM WinSxS bileşen deposu temizleniyor. Bu işlem sistem hızınıza bağlı olarak 2-5 dakika sürebilir, lütfen bekleyiniz...");
                    int dismExit = RunCommandSync("dism.exe", "/online /cleanup-image /startcomponentcleanup /quiet");
                    if (dismExit != 0)
                    {
                        WriteLog("WARN", string.Format("DISM çıkış kodu: {0}", dismExit));
                        hasWarning = true;
                    }
                    break;

                case 17: // Web Tarayıcı Önbellekleri (Chrome, Edge, Brave, Opera - Sadece Cache_Data)
                    CleanBrowserCaches(ref hasWarning);
                    break;

                case 18: // Windows Eski Yükseltme Dosyaları (Windows.old, $Windows.~BT)
                    CleanWindowsOld(ref hasWarning);
                    break;
            }

            if (hasWarning)
                status = StepStatus.Warning;
        }

        private void CleanBrowserCaches(ref bool hasWarning)
        {
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Chrome
            string chromeCache = Path.Combine(localApp, @"Google\Chrome\User Data\Default\Cache");
            SmartDeleteDirectory(chromeCache, ref hasWarning);
            SmartDeleteDirectory(Path.Combine(localApp, @"Google\Chrome\User Data\Default\Code Cache"), ref hasWarning);
            SmartDeleteDirectory(Path.Combine(localApp, @"Google\Chrome\User Data\Default\GPUCache"), ref hasWarning);

            // Microsoft Edge
            string edgeCache = Path.Combine(localApp, @"Microsoft\Edge\User Data\Default\Cache");
            SmartDeleteDirectory(edgeCache, ref hasWarning);
            SmartDeleteDirectory(Path.Combine(localApp, @"Microsoft\Edge\User Data\Default\Code Cache"), ref hasWarning);
            SmartDeleteDirectory(Path.Combine(localApp, @"Microsoft\Edge\User Data\Default\GPUCache"), ref hasWarning);

            // Brave
            string braveCache = Path.Combine(localApp, @"BraveSoftware\Brave-Browser\User Data\Default\Cache");
            SmartDeleteDirectory(braveCache, ref hasWarning);
            SmartDeleteDirectory(Path.Combine(localApp, @"BraveSoftware\Brave-Browser\User Data\Default\Code Cache"), ref hasWarning);

            // Opera
            string operaCache = Path.Combine(appData, @"Opera Software\Opera Stable\Cache");
            SmartDeleteDirectory(operaCache, ref hasWarning);
        }

        private void CleanWindowsOld(ref bool hasWarning)
        {
            string winOld = @"C:\Windows.old";
            string winBt = @"C:\$Windows.~BT";
            string winWs = @"C:\$Windows.~WS";

            if (Directory.Exists(winOld))
            {
                WriteLog("INFO", "C:\\Windows.old tespit edildi, temizleniyor...");
                RunCommandSync("takeown", "/F \"C:\\Windows.old\" /A /R /D Y");
                RunCommandSync("icacls", "\"C:\\Windows.old\" /grant *S-1-5-32-544:F /T /C /Q");
                SmartDeleteDirectory(winOld, ref hasWarning);
            }
            if (Directory.Exists(winBt))
            {
                SmartDeleteDirectory(winBt, ref hasWarning);
            }
            if (Directory.Exists(winWs))
            {
                SmartDeleteDirectory(winWs, ref hasWarning);
            }
        }

        public void SmartDeleteDirectory(string path, ref bool hasWarning)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            try
            {
                var dirInfo = new DirectoryInfo(path);
                foreach (var file in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        file.Attributes = FileAttributes.Normal;
                        file.Delete();
                    }
                    catch (IOException)
                    {
                        hasWarning = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        hasWarning = true;
                    }
                    catch { }
                }

                foreach (var subDir in dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories).OrderByDescending(d => d.FullName.Length))
                {
                    try
                    {
                        subDir.Delete(true);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                hasWarning = true;
                WriteLog("DEBUG", string.Format("Dizin atlandı ({0}): {1}", path, ex.Message));
            }
        }

        public void SmartDeleteFile(string path, ref bool hasWarning)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            try
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }
            catch (IOException)
            {
                hasWarning = true;
            }
            catch (UnauthorizedAccessException)
            {
                hasWarning = true;
            }
            catch { }
        }

        public int RunCommandSync(string fileName, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var p = Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                WriteLog("DEBUG", string.Format("Komut çalıştırma hatası ({0} {1}): {2}", fileName, args, ex.Message));
                return -1;
            }
        }

        public async Task RunWingetUpgradeAsync(Action<string> lineCallback)
        {
            await Task.Run(() =>
            {
                WriteLog("INFO", "Winget güncellemesi başlatılıyor (winget upgrade --all)...");
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "upgrade --all --accept-source-agreements --accept-package-agreements",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };

                    using (var p = Process.Start(psi))
                    {
                        p.OutputDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data) && lineCallback != null)
                            {
                                lineCallback(e.Data);
                            }
                        };
                        p.ErrorDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data) && lineCallback != null)
                            {
                                lineCallback("[WINGET HATA] " + e.Data);
                            }
                        };

                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                        p.WaitForExit();
                        WriteLog("SUCCESS", string.Format("Winget tamamlandı. Çıkış kodu: {0}", p.ExitCode));
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("ERROR", "Winget çalıştırılamadı: " + ex.Message);
                    if (lineCallback != null)
                    {
                        lineCallback("[HATA] Winget çalıştırılamadı: " + ex.Message);
                    }
                }
            });
        }
    }
}
