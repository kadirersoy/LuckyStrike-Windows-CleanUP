using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LuckyStrikeCleanUp
{
    public partial class MainWindow : Window
    {
        private readonly CleanUpEngine _engine;
        private CancellationTokenSource _cts;
        public ObservableCollection<CleanStepItem> Steps { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _engine = new CleanUpEngine();

            InitializeSteps();
            StepsListControl.ItemsSource = Steps;

            CheckAdminPrivileges();
            WireEngineCallbacks();

            AppendConsoleLine("LuckyStrike-Windows-CleanUP v3.0 Pro Masaüstü Sistemi Hazır.");
            AppendConsoleLine("18 Temizlik modülü yapılandırıldı. 'Temizliği Başlat' butonuna tıklayabilirsiniz.\n");
        }

        private void InitializeSteps()
        {
            Steps = new ObservableCollection<CleanStepItem>
            {
                new CleanStepItem { Id = 1, Title = "Windows ve Kullanıcı Temp Klasörleri", Description = @"C:\Windows\Temp ve %temp% geçici dosyaları", IsSelected = true },
                new CleanStepItem { Id = 2, Title = "Windows Update Ön Bellek Dosyaları", Description = @"SoftwareDistribution\Download (wuauserv, bits, dosvc kontrollü)", IsSelected = true },
                new CleanStepItem { Id = 3, Title = "Dosya Gezgini Geçmişi & Çalıştır MRU", Description = @"Son kullanılan dosyalar ve Çalıştır geçmiş kayıtları", IsSelected = true },
                new CleanStepItem { Id = 4, Title = "Tüm Disklerdeki Geri Dönüşüm Kutuları", Description = @"Tüm sürücülerdeki silinmiş çöp kutusu verileri", IsSelected = true },
                new CleanStepItem { Id = 5, Title = "Windows Olay Günlükleri (Hızlı Motor)", Description = @"Sistem, uygulama ve güvenlik günlükleri (Hızlı .NET API)", IsSelected = true },
                new CleanStepItem { Id = 6, Title = "DNS Ön Belleği (DNS Cache)", Description = @"Eski alan adı çözümleme kayıtları temizlenir", IsSelected = true },
                new CleanStepItem { Id = 7, Title = "[Gizlilik Ayarı] Son Açılan Dosyaları Kapat", Description = @"Gezgin son kullanılan dosya takibini pasifleştirir", IsSelected = false },
                new CleanStepItem { Id = 8, Title = "[Gizlilik Ayarı] Başlat Menüsü Uygulama Takibi", Description = @"Başlat menüsünde sık kullanılan uygulama takibini kapatır", IsSelected = false },
                new CleanStepItem { Id = 9, Title = "Hata Raporları & Bellek Dökümleri", Description = @"MEMORY.DMP, Minidump ve WER hata logları", IsSelected = true },
                new CleanStepItem { Id = 10, Title = "Teslim İyileştirme (Delivery Opt.)", Description = @"Windows Update eşler arası indirme ön belleği", IsSelected = true },
                new CleanStepItem { Id = 11, Title = "DirectX ve GPU Shader Ön Belleği", Description = @"NVIDIA GLCache, AMD DXCache, Intel ve D3DSCache", IsSelected = true },
                new CleanStepItem { Id = 12, Title = "Küçük Resim ve Simge Ön Belleği", Description = @"IconCache.db & thumbcache (Explorer güvenli yenilenir)", IsSelected = true },
                new CleanStepItem { Id = 13, Title = "Windows Prefetch Ön Belleği", Description = @"C:\Windows\Prefetch eski uygulama ön yükleme verileri", IsSelected = true },
                new CleanStepItem { Id = 14, Title = "ARP Tablosu ve Ağ Ön Belleği", Description = @"Adres çözümleme tablosu ve NetBIOS ön belleği", IsSelected = true },
                new CleanStepItem { Id = 15, Title = "Windows Defender Tarama Geçmişi", Description = @"Zararlı tarama logları ve algılama geçmişi (Varsayılan: Kapalı)", IsSelected = false },
                new CleanStepItem { Id = 16, Title = "DISM Bileşen Temizliği (WinSxS)", Description = @"Eski Windows bileşen deposu temizliği (2-5 dk sürebilir)", IsSelected = true },
                new CleanStepItem { Id = 17, Title = "Web Tarayıcı Önbellekleri (Web Cache)", Description = @"Chrome, Edge, Brave, Opera geçici önbellekleri (Şifreler korunur)", IsSelected = true },
                new CleanStepItem { Id = 18, Title = "Eski Windows Sürüm Kalıntıları", Description = @"C:\Windows.old ve yükseltme kalıntıları ($Windows.~BT)", IsSelected = true }
            };
        }

        private void CheckAdminPrivileges()
        {
            bool isAdmin = CleanUpEngine.IsAdministrator();
            if (isAdmin)
            {
                TxtAdminStatus.Text = "Yönetici Modu Aktif";
                TxtAdminStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 118));
                AdminBadgeBorder.Background = new SolidColorBrush(Color.FromArgb(40, 0, 230, 118));
                AdminBadgeBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 230, 118));
                BtnRelaunchAdmin.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtAdminStatus.Text = "Normal Yetki (Bazı Adımlar Sınırlı)";
                TxtAdminStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 171, 0));
                AdminBadgeBorder.Background = new SolidColorBrush(Color.FromArgb(40, 255, 171, 0));
                AdminBadgeBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 171, 0));
                BtnRelaunchAdmin.Visibility = Visibility.Visible;
            }
        }

        private void WireEngineCallbacks()
        {
            _engine.LogCallback = delegate(string level, string msg)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    AppendConsoleLine(string.Format("[{0:HH:mm:ss}] [{1}] {2}", DateTime.Now, level, msg));
                }));
            };

            _engine.ProgressCallback = delegate(int percent, string currentStep)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    MainProgressBar.Value = percent;
                    TxtProgressPercent.Text = string.Format("{0}%", percent);
                    TxtCurrentStep.Text = currentStep;
                }));
            };
        }

        private void AppendConsoleLine(string text)
        {
            TxtConsole.AppendText(text + Environment.NewLine);
            TxtConsole.ScrollToEnd();
        }

        private async void BtnStartCleanup_Click(object sender, RoutedEventArgs e)
        {
            BtnStartCleanup.Visibility = Visibility.Collapsed;
            BtnStopCleanup.Visibility = Visibility.Visible;
            BtnWinget.IsEnabled = false;
            BtnRestorePoint.IsEnabled = false;
            FreedDiskBadge.Visibility = Visibility.Collapsed;

            TxtStatusSummary.Text = "Temizlik işlemleri devam ediyor...";
            MainProgressBar.Value = 0;
            TxtProgressPercent.Text = "0%";

            foreach (var step in Steps)
            {
                if (step.IsSelected)
                {
                    step.Status = StepStatus.Idle;
                    step.Duration = "";
                }
                else
                {
                    step.Status = StepStatus.Skipped;
                }
            }

            _cts = new CancellationTokenSource();

            try
            {
                var stepsList = new List<CleanStepItem>(Steps);
                string freedSpace = await _engine.ExecuteStepsAsync(stepsList, _cts.Token);

                if (!string.IsNullOrEmpty(freedSpace))
                {
                    TxtFreedDisk.Text = string.Format("Kazanılan Alan: {0}", freedSpace);
                    FreedDiskBadge.Visibility = Visibility.Visible;
                    TxtStatusSummary.Text = string.Format("Temizlik başarıyla tamamlandı! Kazanılan Alan: {0}", freedSpace);
                }
                else
                {
                    TxtStatusSummary.Text = "Temizlik başarıyla tamamlandı!";
                }
            }
            catch (Exception ex)
            {
                AppendConsoleLine(string.Format("[HATA] Beklenmeyen hata: {0}", ex.Message));
                TxtStatusSummary.Text = "İşlem sırasında hata meydana geldi.";
            }
            finally
            {
                BtnStartCleanup.Visibility = Visibility.Visible;
                BtnStopCleanup.Visibility = Visibility.Collapsed;
                BtnWinget.IsEnabled = true;
                BtnRestorePoint.IsEnabled = true;
                if (_cts != null)
                {
                    _cts.Dispose();
                    _cts = null;
                }
            }
        }

        private void BtnStopCleanup_Click(object sender, RoutedEventArgs e)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                AppendConsoleLine("[UYARI] Durdurma isteği gönderildi...");
                TxtStatusSummary.Text = "İşlemler durduruluyor...";
            }
        }

        private async void BtnRestorePoint_Click(object sender, RoutedEventArgs e)
        {
            BtnRestorePoint.IsEnabled = false;
            TxtStatusSummary.Text = "Sistem Geri Yükleme Noktası oluşturuluyor...";
            AppendConsoleLine("\n--------------------------------------------------");
            AppendConsoleLine("[SİSTEM] Geri Yükleme Noktası oluşturuluyor...");
            AppendConsoleLine("--------------------------------------------------");

            bool success = await _engine.CreateRestorePointAsync();
            if (success)
            {
                MessageBox.Show("Sistem Geri Yükleme Noktası başarıyla oluşturuldu!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                TxtStatusSummary.Text = "Geri Yükleme Noktası oluşturuldu.";
            }
            else
            {
                MessageBox.Show("Geri yükleme noktası oluşturulamadı.\nWindows Sistem Korumasının (System Protection) açık olduğundan ve yönetici olduğunuzdan emin olun.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtStatusSummary.Text = "Geri yükleme noktası oluşturulamadı.";
            }
            BtnRestorePoint.IsEnabled = true;
        }

        private async void BtnWinget_Click(object sender, RoutedEventArgs e)
        {
            BtnStartCleanup.IsEnabled = false;
            BtnWinget.IsEnabled = false;
            TxtStatusSummary.Text = "Winget uygulamaları güncelleniyor...";

            AppendConsoleLine("\n--------------------------------------------------");
            AppendConsoleLine("[WINGET] Uygulama güncellemeleri kontrol ediliyor...");
            AppendConsoleLine("--------------------------------------------------");

            try
            {
                await _engine.RunWingetUpgradeAsync(delegate(string line)
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        AppendConsoleLine(line);
                    }));
                });
                TxtStatusSummary.Text = "Winget güncelleme tamamlandı.";
            }
            catch (Exception ex)
            {
                AppendConsoleLine("[WINGET HATA] " + ex.Message);
            }
            finally
            {
                BtnStartCleanup.IsEnabled = true;
                BtnWinget.IsEnabled = true;
            }
        }

        private void BtnRestartPC_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "Bilgisayarınız hemen yeniden başlatılacak.\nTüm açık çalışmalarınızı kaydettiğinizden emin olun.\n\nDevam etmek istiyor musunuz?",
                "LuckyStrike - Bilgisayarı Yeniden Başlat",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                Process.Start("shutdown", "/r /t 0");
            }
        }

        private void BtnShutdownPC_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "Bilgisayarınız hemen kapatılacak.\nTüm açık çalışmalarınızı kaydettiğinizden emin olun.\n\nDevam etmek istiyor musunuz?",
                "LuckyStrike - Bilgisayarı Kapat",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                Process.Start("shutdown", "/s /t 0");
            }
        }

        private void BtnRelaunchAdmin_Click(object sender, RoutedEventArgs e)
        {
            _engine.RestartAsAdmin();
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var step in Steps)
                step.IsSelected = true;
        }

        private void BtnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var step in Steps)
                step.IsSelected = false;
        }

        private void BtnSelectDefault_Click(object sender, RoutedEventArgs e)
        {
            foreach (var step in Steps)
            {
                // Defender (15) ve Gizlilik Ayarları (7, 8) varsayılan kapalı; diğerleri açık
                step.IsSelected = (step.Id != 7 && step.Id != 8 && step.Id != 15);
            }
        }

        private void BtnOpenLogFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(_engine.MasterLogFile))
                {
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", _engine.MasterLogFile));
                }
                else if (Directory.Exists(_engine.LogDirectory))
                {
                    Process.Start("explorer.exe", string.Format("\"{0}\"", _engine.LogDirectory));
                }
                else
                {
                    MessageBox.Show("Henüz log dosyası oluşturulmamış.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Log klasörü açılamadı: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCopyLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtConsole.Text);
                MessageBox.Show("Konsol içeriği panoya kopyalandı!", "Kopyalandı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Panoya kopyalanırken hata: " + ex.Message);
            }
        }

        private void BtnClearConsole_Click(object sender, RoutedEventArgs e)
        {
            TxtConsole.Clear();
            AppendConsoleLine(string.Format("[{0:HH:mm:ss}] Konsol temizlendi.", DateTime.Now));
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
