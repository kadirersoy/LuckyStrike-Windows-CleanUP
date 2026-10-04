using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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

        private static readonly object _logLock = new object();

        private long _totalDeletedBytes = 0;
        private int _totalFilesDeleted = 0;

        public long TotalDeletedBytes { get { return Interlocked.Read(ref _totalDeletedBytes); } }
        public int TotalFilesDeleted { get { return _totalFilesDeleted; } }

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
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
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
            lock (_logLock)
            {
                try
                {
                    File.AppendAllText(MasterLogFile, line + Environment.NewLine, Encoding.UTF8);
                }
                catch { }
            }

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

        public Tuple<long, long> GetDriveSpaceStats(string driveLetter = "C")
        {
            try
            {
                var drive = new DriveInfo(driveLetter);
                if (drive.IsReady)
                    return Tuple.Create(drive.AvailableFreeSpace, drive.TotalSize);
            }
            catch { }
            return Tuple.Create(0L, 0L);
        }

        public string GetDriveSpaceFormatted(string driveLetter = "C")
        {
            var stats = GetDriveSpaceStats(driveLetter);
            if (stats.Item2 > 0)
            {
                return string.Format("{0} Boş / {1}", FormatBytes(stats.Item1), FormatBytes(stats.Item2));
            }
            return "";
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 B";
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024m) >= 1 && counter < suffixes.Length - 1)
            {
                number = number / 1024m;
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
                        WriteLog("WARN", "Geri yükleme noktası oluşturulamadı (Sistem koruması kapalı olabilir veya izin verilmedi).");
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
            Interlocked.Exchange(ref _totalDeletedBytes, 0);
            Interlocked.Exchange(ref _totalFilesDeleted, 0);

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
                        ExecuteStepById(step.Id, out resultStatus, token);
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
            long deletedBytes = TotalDeletedBytes;
            int deletedFiles = TotalFilesDeleted;

            string totalTimeStr = stopwatchTotal.Elapsed.TotalMinutes >= 1
                ? string.Format("{0} dk {1} sn", (int)stopwatchTotal.Elapsed.TotalMinutes, stopwatchTotal.Elapsed.Seconds)
                : string.Format("{0:F2} sn", stopwatchTotal.Elapsed.TotalSeconds);

            if (ProgressCallback != null)
            {
                ProgressCallback(100, "Tüm işlemler tamamlandı!");
            }

            string resultSummary = "";
            if (freedBytes > 0)
            {
                resultSummary = string.Format("{0} boş alan (Silinen: {1}, {2:N0} dosya)", FormatBytes(freedBytes), FormatBytes(deletedBytes), deletedFiles);
            }
            else if (deletedBytes > 0)
            {
                resultSummary = string.Format("{0} önbellek temizlendi ({1:N0} dosya)", FormatBytes(deletedBytes), deletedFiles);
            }
            else
            {
                resultSummary = "Önbellekler temizlendi";
            }

            WriteLog("INFO", "================================================================================");
            WriteLog("SUCCESS", string.Format("Tüm işlemler tamamlandı! Toplam Süre: {0}", totalTimeStr));
            WriteLog("SUCCESS", string.Format("Temizlik Özeti: {0} (Silinen dosya sayısı: {1:N0})", resultSummary, deletedFiles));
            WriteLog("INFO", string.Format("Log Dosyası: {0}", MasterLogFile));
            WriteLog("INFO", "================================================================================");

            return resultSummary;
        }

        private void ExecuteStepById(int stepId, out StepStatus status, CancellationToken token)
        {
            status = StepStatus.Completed;
            bool hasWarning = false;

            switch (stepId)
            {
                case 1: // Windows ve Kullanıcı Temp Klasörleri
                    SmartDeleteDirectory(@"C:\Windows\Temp", ref hasWarning, token);
                    SmartDeleteDirectory(Path.GetTempPath(), ref hasWarning, token);
                    break;

                case 2: // Windows Update Ön Belleği (wuauserv, bits, dosvc güvenli durdurulup açılır)
                    try
                    {
                        RunCommandSync("net", "stop wuauserv", token);
                        RunCommandSync("net", "stop bits", token);
                        RunCommandSync("net", "stop dosvc", token);
                        SmartDeleteDirectory(@"C:\Windows\SoftwareDistribution\Download", ref hasWarning, token);
                    }
                    finally
                    {
                        RunCommandSync("net", "start wuauserv", token);
                        RunCommandSync("net", "start bits", token);
                        RunCommandSync("net", "start dosvc", token);
                    }
                    break;

                case 3: // Dosya Gezgini Geçmişi ve RunMRU
                    string recent = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Recent");
                    SmartDeleteDirectory(recent, ref hasWarning, token);
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
                        foreach (var drive in DriveInfo.GetDrives())
                        {
                            if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
                            {
                                try
                                {
                                    SHEmptyRecycleBin(IntPtr.Zero, drive.Name, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);
                                }
                                catch { }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog("WARN", "RecycleBin API uyarısı, PowerShell deneniyor: " + ex.Message);
                        RunCommandSync("powershell.exe", "-NoProfile -Command Clear-RecycleBin -Force -ErrorAction SilentlyContinue", token);
                    }
                    break;

                case 5: // Windows Olay Günlükleri (Hızlı Native .NET EventLog API)
                    try
                    {
                        WriteLog("INFO", "Olay günlükleri yerel .NET motoruyla temizleniyor...");
                        using (var session = new EventLogSession())
                        {
                            var logNames = session.GetLogNames();
                            foreach (var logName in logNames)
                            {
                                if (token.IsCancellationRequested) break;
                                try
                                {
                                    session.ClearLog(logName);
                                }
                                catch { }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog("WARN", "Olay günlükleri uyarısı: " + ex.Message);
                        hasWarning = true;
                    }
                    break;

                case 6: // DNS Ön Belleği
                    try
                    {
                        DnsFlushResolverCache();
                    }
                    catch { }
                    RunCommandSync("ipconfig", "/flushdns", token);
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
                    SmartDeleteDirectory(@"C:\Windows\Minidump", ref hasWarning, token);
                    SmartDeleteDirectory(@"C:\Windows\LiveKernelReports", ref hasWarning, token);
                    string localWer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\WER");
                    SmartDeleteDirectory(localWer, ref hasWarning, token);
                    SmartDeleteDirectory(@"C:\ProgramData\Microsoft\Windows\WER", ref hasWarning, token);
                    break;

                case 10: // Teslim İyileştirme (Delivery Optimization)
                    try
                    {
                        RunCommandSync("net", "stop dosvc", token);
                        SmartDeleteDirectory(@"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache", ref hasWarning, token);
                    }
                    finally
                    {
                        RunCommandSync("net", "start dosvc", token);
                    }
                    break;

                case 11: // DirectX ve Ekran Kartı Shader Ön Belleği (NVIDIA + AMD + Intel + D3D)
                    string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    SmartDeleteDirectory(Path.Combine(localApp, "D3DSCache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(localApp, @"NVIDIA\GLCache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(localApp, @"NVIDIA\DXCache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(localApp, @"NVIDIA Corporation\NV_Cache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(localApp, @"AMD\DXCache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(localApp, @"AMD\GLCache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(localApp, @"Intel\ShaderCache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(localApp, @"Microsoft\DirectX Shader Cache"), ref hasWarning, token);
                    break;

                case 12: // Küçük Resim ve Simge Ön Belleği (Explorer güvenli yenilenir)
                    try
                    {
                        WriteLog("INFO", "Simge veritabanı kilitlerini açmak için Explorer geçici olarak yenileniyor...");
                        RunCommandSync("taskkill", "/f /im explorer.exe", token);
                        Thread.Sleep(400);

                        string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        SmartDeleteFile(Path.Combine(localData, "IconCache.db"), ref hasWarning);

                        string explorerCache = Path.Combine(localData, @"Microsoft\Windows\Explorer");
                        if (Directory.Exists(explorerCache))
                        {
                            try
                            {
                                var files = Directory.GetFiles(explorerCache, "thumbcache_*.db");
                                foreach (var f in files)
                                {
                                    SmartDeleteFile(f, ref hasWarning);
                                }
                                var iconFiles = Directory.GetFiles(explorerCache, "iconcache_*.db");
                                foreach (var f in iconFiles)
                                {
                                    SmartDeleteFile(f, ref hasWarning);
                                }
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog("WARN", "Simge temizliği uyarısı: " + ex.Message);
                        hasWarning = true;
                    }
                    finally
                    {
                        // Explorer'ın çalıştığından kesinlikle emin ol
                        if (Process.GetProcessesByName("explorer").Length == 0)
                        {
                            try
                            {
                                Process.Start("explorer.exe");
                            }
                            catch { }
                        }
                    }
                    break;

                case 13: // Windows Prefetch
                    SmartDeleteDirectory(@"C:\Windows\Prefetch", ref hasWarning, token);
                    break;

                case 14: // ARP Tablosu ve Ağ Ön Belleği
                    RunCommandSync("arp", "-d *", token);
                    RunCommandSync("nbtstat", "-R", token);
                    RunCommandSync("nbtstat", "-RR", token);
                    break;

                case 15: // Windows Defender Tarama Geçmişi
                    try
                    {
                        RunCommandSync("takeown", "/f \"C:\\ProgramData\\Microsoft\\Windows Defender\\Scans\\History\\Service\\DetectionHistory\" /r /d y", token);
                        RunCommandSync("icacls", "\"C:\\ProgramData\\Microsoft\\Windows Defender\\Scans\\History\\Service\\DetectionHistory\" /grant administrators:F /t", token);
                        SmartDeleteDirectory(@"C:\ProgramData\Microsoft\Windows Defender\Scans\History\Service\DetectionHistory", ref hasWarning, token);
                        SmartDeleteDirectory(@"C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\Quick", ref hasWarning, token);
                        SmartDeleteDirectory(@"C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\Resource", ref hasWarning, token);
                        RunCommandSync("wevtutil.exe", "cl \"Microsoft-Windows-Windows Defender/Operational\"", token);
                    }
                    catch (Exception ex) { WriteLog("WARN", "Defender uyarısı: " + ex.Message); hasWarning = true; }
                    break;

                case 16: // DISM Bileşen Temizliği
                    WriteLog("INFO", "DISM WinSxS bileşen deposu temizleniyor (sistem hızına bağlı olarak 2-5 dakika sürebilir)...");
                    int dismExit = RunCommandSync("dism.exe", "/online /cleanup-image /startcomponentcleanup /quiet", token);
                    if (dismExit != 0 && dismExit != -1)
                    {
                        WriteLog("WARN", string.Format("DISM çıkış kodu: {0}", dismExit));
                        hasWarning = true;
                    }
                    break;

                case 17: // Web Tarayıcı Önbellekleri (Chrome, Edge, Brave, Firefox, Opera, Vivaldi)
                    CleanBrowserCaches(ref hasWarning, token);
                    break;

                case 18: // Windows Eski Yükseltme Dosyaları (Windows.old, $Windows.~BT, $Windows.~WS)
                    CleanWindowsOld(ref hasWarning, token);
                    break;
            }

            if (hasWarning)
                status = StepStatus.Warning;
        }

        private void CleanBrowserCaches(ref bool hasWarning, CancellationToken token)
        {
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // 1. Google Chrome (Tüm Profiller)
            string chromeUser = Path.Combine(localApp, @"Google\Chrome\User Data");
            CleanChromiumBase(chromeUser, ref hasWarning, token);

            // 2. Microsoft Edge (Tüm Profiller)
            string edgeUser = Path.Combine(localApp, @"Microsoft\Edge\User Data");
            CleanChromiumBase(edgeUser, ref hasWarning, token);

            // 3. Brave Browser (Tüm Profiller)
            string braveUser = Path.Combine(localApp, @"BraveSoftware\Brave-Browser\User Data");
            CleanChromiumBase(braveUser, ref hasWarning, token);

            // 4. Vivaldi Browser
            string vivaldiUser = Path.Combine(localApp, @"Vivaldi\User Data");
            CleanChromiumBase(vivaldiUser, ref hasWarning, token);

            // 5. Opera & Opera GX
            SmartDeleteDirectory(Path.Combine(localApp, @"Opera Software\Opera Stable\Cache"), ref hasWarning, token);
            SmartDeleteDirectory(Path.Combine(appData, @"Opera Software\Opera Stable\Cache"), ref hasWarning, token);
            SmartDeleteDirectory(Path.Combine(localApp, @"Opera Software\Opera GX Stable\Cache"), ref hasWarning, token);
            SmartDeleteDirectory(Path.Combine(appData, @"Opera Software\Opera GX Stable\Cache"), ref hasWarning, token);

            // 6. Mozilla Firefox (Tüm Profiller)
            string ffProfiles = Path.Combine(localApp, @"Mozilla\Firefox\Profiles");
            if (Directory.Exists(ffProfiles))
            {
                try
                {
                    foreach (var pDir in Directory.GetDirectories(ffProfiles))
                    {
                        if (token.IsCancellationRequested) break;
                        SmartDeleteDirectory(Path.Combine(pDir, "cache2"), ref hasWarning, token);
                        SmartDeleteDirectory(Path.Combine(pDir, "jumpListCache"), ref hasWarning, token);
                        SmartDeleteDirectory(Path.Combine(pDir, "startupCache"), ref hasWarning, token);
                    }
                }
                catch { }
            }

            // 7. Microsoft Edge WebView2 (Uygulamaların gömülü web motoru önbellekleri)
            string webView2User = Path.Combine(localApp, @"Microsoft\EdgeWebView\User Data");
            CleanChromiumBase(webView2User, ref hasWarning, token);
        }

        private void CleanChromiumBase(string userDataPath, ref bool hasWarning, CancellationToken token)
        {
            if (!Directory.Exists(userDataPath)) return;

            try
            {
                // Root caches
                SmartDeleteDirectory(Path.Combine(userDataPath, "ShaderCache"), ref hasWarning, token);
                SmartDeleteDirectory(Path.Combine(userDataPath, "GrShaderCache"), ref hasWarning, token);

                // Default & Profile dirs
                var profileDirs = Directory.GetDirectories(userDataPath, "*Profile*")
                    .Concat(Directory.GetDirectories(userDataPath, "Default"))
                    .Distinct();

                foreach (var profile in profileDirs)
                {
                    if (token.IsCancellationRequested) break;
                    SmartDeleteDirectory(Path.Combine(profile, "Cache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(profile, "Code Cache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(profile, "GPUCache"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(profile, @"Service Worker\CacheStorage"), ref hasWarning, token);
                    SmartDeleteDirectory(Path.Combine(profile, @"Service Worker\ScriptCache"), ref hasWarning, token);
                }
            }
            catch { }
        }

        private void CleanWindowsOld(ref bool hasWarning, CancellationToken token)
        {
            string winOld = @"C:\Windows.old";
            string winBt = @"C:\$Windows.~BT";
            string winWs = @"C:\$Windows.~WS";
            string winCurrent = @"C:\$GetCurrent";

            if (Directory.Exists(winOld))
            {
                WriteLog("INFO", "C:\\Windows.old tespit edildi, izinler alınıp temizleniyor...");
                RunCommandSync("takeown", "/F \"C:\\Windows.old\" /A /R /D Y", token);
                RunCommandSync("icacls", "\"C:\\Windows.old\" /grant *S-1-5-32-544:F /T /C /Q", token);
                SmartDeleteDirectory(winOld, ref hasWarning, token);
            }
            if (Directory.Exists(winBt))
            {
                WriteLog("INFO", "C:\\$Windows.~BT tespit edildi, temizleniyor...");
                RunCommandSync("takeown", "/F \"C:\\$Windows.~BT\" /A /R /D Y", token);
                RunCommandSync("icacls", "\"C:\\$Windows.~BT\" /grant *S-1-5-32-544:F /T /C /Q", token);
                SmartDeleteDirectory(winBt, ref hasWarning, token);
            }
            if (Directory.Exists(winWs))
            {
                WriteLog("INFO", "C:\\$Windows.~WS tespit edildi, temizleniyor...");
                RunCommandSync("takeown", "/F \"C:\\$Windows.~WS\" /A /R /D Y", token);
                RunCommandSync("icacls", "\"C:\\$Windows.~WS\" /grant *S-1-5-32-544:F /T /C /Q", token);
                SmartDeleteDirectory(winWs, ref hasWarning, token);
            }
            if (Directory.Exists(winCurrent))
            {
                WriteLog("INFO", "C:\\$GetCurrent tespit edildi, temizleniyor...");
                RunCommandSync("takeown", "/F \"C:\\$GetCurrent\" /A /R /D Y", token);
                RunCommandSync("icacls", "\"C:\\$GetCurrent\" /grant *S-1-5-32-544:F /T /C /Q", token);
                SmartDeleteDirectory(winCurrent, ref hasWarning, token);
            }
        }

        public void SmartDeleteDirectory(string path, ref bool hasWarning, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

            try
            {
                var dirInfo = new DirectoryInfo(path);
                DeleteDirectoryInternal(dirInfo, ref hasWarning, token);
            }
            catch (Exception ex)
            {
                hasWarning = true;
                WriteLog("DEBUG", string.Format("Dizin atlandı ({0}): {1}", path, ex.Message));
            }
        }

        private void DeleteDirectoryInternal(DirectoryInfo dir, ref bool hasWarning, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            // Sembolik bağ veya junction noktalarını atla (başka klasörlere zarar vermemek için)
            if ((dir.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
            {
                try
                {
                    dir.Delete();
                }
                catch { }
                return;
            }

            // 1. Önce bu dizindeki dosyaları sil
            FileInfo[] files = null;
            try
            {
                files = dir.GetFiles();
            }
            catch (Exception)
            {
                hasWarning = true;
                return;
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    if (token.IsCancellationRequested) return;
                    try
                    {
                        long len = 0;
                        try { len = file.Length; } catch { }
                        file.Attributes = FileAttributes.Normal;
                        file.Delete();
                        Interlocked.Add(ref _totalDeletedBytes, len);
                        Interlocked.Increment(ref _totalFilesDeleted);
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
            }

            // 2. Alt dizinleri yinelemeli tara ve boşalanları sil
            DirectoryInfo[] subDirs = null;
            try
            {
                subDirs = dir.GetDirectories();
            }
            catch (Exception)
            {
                hasWarning = true;
                return;
            }

            if (subDirs != null)
            {
                foreach (var sub in subDirs)
                {
                    if (token.IsCancellationRequested) return;
                    DeleteDirectoryInternal(sub, ref hasWarning, token);
                    try
                    {
                        sub.Attributes = FileAttributes.Normal;
                        sub.Delete();
                    }
                    catch { }
                }
            }
        }

        public void SmartDeleteFile(string path, ref bool hasWarning)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            try
            {
                long len = 0;
                try
                {
                    var fi = new FileInfo(path);
                    len = fi.Length;
                }
                catch { }

                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
                Interlocked.Add(ref _totalDeletedBytes, len);
                Interlocked.Increment(ref _totalFilesDeleted);
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

        public int RunCommandSync(string fileName, string args, CancellationToken token = default(CancellationToken))
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
                    if (p == null) return -1;

                    // Boruların dolup kilitlenmesini (pipe deadlock) önlemek için asenkron boşalt
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    while (!p.WaitForExit(150))
                    {
                        if (token.IsCancellationRequested)
                        {
                            try { p.Kill(); } catch { }
                            return -1;
                        }
                    }
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

