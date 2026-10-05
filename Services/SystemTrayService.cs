using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using WinPurifyPro.ViewModels;

namespace WinPurifyPro.Services
{
    public enum TrayNotificationType
    {
        Info = 1,
        Warning = 2,
        Error = 3,
        Success = 4
    }

    /// <summary>
    /// Windows 시스템 트레이(Notification Area / Taskbar Tray) 통합 관리 서비스
    /// - Win32 Shell_NotifyIconW 기반 초저지연 트레이 아이콘 및 풍선/토스트 알림
    /// - 실시간 작업 진행률(Progress) 툴팁 동적 갱신 및 마일스톤 알림
    /// - 시스템 트레이 최소화(Minimize to Tray) & 닫기 시 트레이 상주(Close to Tray)
    /// - 트레이 우클릭 컨텍스트 메뉴 (빠른 정화, RAM 압축, 스케줄러, 설정, 종료)
    /// </summary>
    public sealed class SystemTrayService : IDisposable
    {
        private static readonly Lazy<SystemTrayService> _instance = new(() => new SystemTrayService());
        public static SystemTrayService Instance => _instance.Value;

        private const int WM_APP = 0x8000;
        public const int WM_TRAYICON_CALLBACK = WM_APP + 288;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_CONTEXTMENU = 0x007B;

        private const uint NIM_ADD = 0x00000000;
        private const uint NIM_MODIFY = 0x00000001;
        private const uint NIM_DELETE = 0x00000002;
        private const uint NIM_SETVERSION = 0x00000004;

        private const uint NIF_MESSAGE = 0x00000001;
        private const uint NIF_ICON = 0x00000002;
        private const uint NIF_TIP = 0x00000004;
        private const uint NIF_STATE = 0x00000008;
        private const uint NIF_INFO = 0x00000010;
        private const uint NIF_SHOWTIP = 0x00000080;

        private const uint NOTIFYICON_VERSION_4 = 4;

        private const uint NIIF_NONE = 0x00000000;
        private const uint NIIF_INFO = 0x00000001;
        private const uint NIIF_WARNING = 0x00000002;
        private const uint NIIF_ERROR = 0x00000003;
        private const uint NIIF_USER = 0x00000004;
        private const uint NIIF_LARGE_ICON = 0x00000020;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public uint dwState;
            public uint dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public uint dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ExtractIconW(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadImageW(IntPtr hInst, string name, uint type, int cx, int cy, uint fuLoad);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadIconW(IntPtr hInstance, IntPtr lpIconName);

        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;
        private const uint LR_DEFAULTSIZE = 0x00000040;
        private const int IDI_APPLICATION = 32512;

        private Window? _mainWindow;
        private MainViewModel? _viewModel;
        private IntPtr _windowHandle = IntPtr.Zero;
        private HwndSource? _hwndSource;
        private IntPtr _hIcon = IntPtr.Zero;
        private bool _isCreated = false;
        private readonly object _lockObj = new();

        // Tray Configuration Options
        public bool MinimizeToTray { get; set; } = true;
        public bool CloseToTray { get; set; } = true;
        public bool NotificationsEnabled { get; set; } = true;
        public bool MilestoneNotificationsEnabled { get; set; } = true;
        public bool LiveTooltipProgressEnabled { get; set; } = true;

        private int _lastNotifiedMilestone = -1;

        private SystemTrayService() { }

        public void Initialize(Window mainWindow, MainViewModel viewModel)
        {
            if (_isCreated) return;

            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            var helper = new WindowInteropHelper(_mainWindow);
            _windowHandle = helper.Handle;

            if (_windowHandle == IntPtr.Zero)
            {
                _mainWindow.SourceInitialized += (s, e) =>
                {
                    _windowHandle = new WindowInteropHelper(_mainWindow).Handle;
                    SetupHookAndTray();
                };
            }
            else
            {
                SetupHookAndTray();
            }

            // Window minimize / state changed hook
            _mainWindow.StateChanged += (s, e) =>
            {
                if (_mainWindow.WindowState == WindowState.Minimized && MinimizeToTray)
                {
                    _mainWindow.Hide();
                    ShowTrayNotification(
                        "WinPurify Pro 최소화됨",
                        "시스템 트레이로 안전하게 최소화되었습니다. 트레이 아이콘을 더블 클릭하면 다시 열립니다.",
                        TrayNotificationType.Info
                    );
                }
            };

            // Window close hook
            _mainWindow.Closing += (s, e) =>
            {
                if (CloseToTray)
                {
                    e.Cancel = true;
                    _mainWindow.Hide();
                    ShowTrayNotification(
                        "백그라운드 트레이 상주 중",
                        "WinPurify Pro가 백그라운드 트레이에서 대기 중입니다. 자동 스케줄러 및 실시간 감시가 계속 작동합니다.",
                        TrayNotificationType.Info
                    );
                }
            };
        }

        public void Initialize(Window mainWindow, IMainViewModel viewModel)
        {
            if (viewModel is MainViewModel mvm)
            {
                Initialize(mainWindow, mvm);
            }
        }

        private void SetupHookAndTray()
        {
            try
            {
                _hwndSource = HwndSource.FromHwnd(_windowHandle);
                _hwndSource?.AddHook(WndProc);

                LoadTrayIcon();
                CreateTrayIcon();
                UpdateTooltip($"WinPurify Pro v{AppEnvironment.AppVersion}\n상태: 준비 완료 (대기 중)");
            }
            catch (Exception ex)
            {
                LoggerService.Instance.LogException("SystemTrayService.SetupHookAndTray", ex);
            }
        }

        private void LoadTrayIcon()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string icoPath = Path.Combine(baseDir, "WinPurify.ico");

                if (File.Exists(icoPath))
                {
                    _hIcon = LoadImageW(IntPtr.Zero, icoPath, IMAGE_ICON, 32, 32, LR_LOADFROMFILE | LR_DEFAULTSIZE);
                }

                if (_hIcon == IntPtr.Zero)
                {
                    string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                    {
                        _hIcon = ExtractIconW(IntPtr.Zero, exePath, 0);
                    }
                }

                if (_hIcon == IntPtr.Zero)
                {
                    _hIcon = LoadIconW(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
                }
            }
            catch
            {
                try
                {
                    _hIcon = LoadIconW(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
                }
                catch { }
            }
        }

        private void CreateTrayIcon()
        {
            lock (_lockObj)
            {
                if (_windowHandle == IntPtr.Zero) return;

                var nid = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                    hWnd = _windowHandle,
                    uID = 1001,
                    uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                    uCallbackMessage = WM_TRAYICON_CALLBACK,
                    hIcon = _hIcon,
                    szTip = $"WinPurify Pro v{AppEnvironment.AppVersion} (준비 완료)"
                };

                _isCreated = Shell_NotifyIcon(NIM_ADD, ref nid);

                // Set Version 4 behavior
                var nidVersion = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                    hWnd = _windowHandle,
                    uID = 1001,
                    uTimeoutOrVersion = NOTIFYICON_VERSION_4
                };
                Shell_NotifyIcon(NIM_SETVERSION, ref nidVersion);
            }
        }

        public void UpdateTooltip(string tooltip)
        {
            if (!_isCreated || _windowHandle == IntPtr.Zero) return;

            lock (_lockObj)
            {
                string safeTip = tooltip.Length > 125 ? tooltip.Substring(0, 125) : tooltip;

                var nid = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                    hWnd = _windowHandle,
                    uID = 1001,
                    uFlags = NIF_TIP,
                    szTip = safeTip
                };

                Shell_NotifyIcon(NIM_MODIFY, ref nid);
            }
        }

        public void UpdateProgress(int current, int total, string currentTaskName)
        {
            if (total <= 0) return;

            int percent = (int)Math.Clamp(Math.Round(((double)current / total) * 100.0), 0, 100);

            if (LiveTooltipProgressEnabled)
            {
                string tip = $"WinPurify Pro 최적화 진행 중 ({percent}%)\n처리 중: {current}/{total}\n항목: {currentTaskName}";
                UpdateTooltip(tip);
            }

            // Milestone Notifications (25%, 50%, 75%)
            if (MilestoneNotificationsEnabled && NotificationsEnabled)
            {
                if ((percent == 50 || percent == 25 || percent == 75) && _lastNotifiedMilestone != percent)
                {
                    _lastNotifiedMilestone = percent;
                    ShowTrayNotification(
                        $"⚡ 최적화 진행 ({percent}%)",
                        $"{current}/{total} 모듈 처리 완료: {currentTaskName}",
                        TrayNotificationType.Info
                    );
                }
            }
        }

        public void ResetProgressMilestone()
        {
            _lastNotifiedMilestone = -1;
        }

        public void ShowTrayNotification(string title, string message, TrayNotificationType type = TrayNotificationType.Info)
        {
            if (!NotificationsEnabled || !_isCreated || _windowHandle == IntPtr.Zero) return;

            lock (_lockObj)
            {
                uint infoFlags = type switch
                {
                    TrayNotificationType.Warning => NIIF_WARNING,
                    TrayNotificationType.Error => NIIF_ERROR,
                    TrayNotificationType.Success => NIIF_INFO,
                    _ => NIIF_INFO
                } | NIIF_LARGE_ICON;

                string safeTitle = title.Length > 63 ? title.Substring(0, 63) : title;
                string safeMsg = message.Length > 255 ? message.Substring(0, 255) : message;

                var nid = new NOTIFYICONDATA
                {
                    cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                    hWnd = _windowHandle,
                    uID = 1001,
                    uFlags = NIF_INFO,
                    szInfoTitle = safeTitle,
                    szInfo = safeMsg,
                    dwInfoFlags = infoFlags
                };

                Shell_NotifyIcon(NIM_MODIFY, ref nid);
            }
        }

        public void RestoreMainWindow()
        {
            if (_mainWindow == null) return;

            _mainWindow.Dispatcher.Invoke(() =>
            {
                if (!_mainWindow.IsVisible)
                {
                    _mainWindow.Show();
                }

                if (_mainWindow.WindowState == WindowState.Minimized)
                {
                    _mainWindow.WindowState = WindowState.Normal;
                }

                _mainWindow.Activate();
                _mainWindow.Focus();
                SetForegroundWindow(new WindowInteropHelper(_mainWindow).Handle);
            });
        }

        public void ToggleMainWindow()
        {
            if (_mainWindow == null) return;

            _mainWindow.Dispatcher.Invoke(() =>
            {
                if (_mainWindow.IsVisible && _mainWindow.WindowState != WindowState.Minimized)
                {
                    _mainWindow.WindowState = WindowState.Minimized;
                    if (MinimizeToTray) _mainWindow.Hide();
                }
                else
                {
                    RestoreMainWindow();
                }
            });
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_TRAYICON_CALLBACK)
            {
                int eventId = lParam.ToInt32() & 0xFFFF;

                switch (eventId)
                {
                    case WM_LBUTTONDBLCLK:
                    case WM_LBUTTONDOWN:
                        RestoreMainWindow();
                        handled = true;
                        break;

                    case WM_RBUTTONUP:
                    case WM_CONTEXTMENU:
                        ShowContextMenu();
                        handled = true;
                        break;
                }
            }

            return IntPtr.Zero;
        }

        private void ShowContextMenu()
        {
            if (_mainWindow == null || _viewModel == null) return;

            _mainWindow.Dispatcher.Invoke(() =>
            {
                var contextMenu = new ContextMenu
                {
                    Style = null // Use clean WPF system styling with dark theme touches
                };

                // Header / Version Info Item (disabled)
                var headerItem = new MenuItem
                {
                    Header = $"🚀 WinPurify Pro v{AppEnvironment.AppVersion}",
                    FontWeight = FontWeights.Bold,
                    IsEnabled = false
                };
                contextMenu.Items.Add(headerItem);

                // Status info item (disabled)
                var statusItem = new MenuItem
                {
                    Header = $"상태: {_viewModel.StatusText} (CPU: {_viewModel.CpuUsage:F0}% / RAM: {_viewModel.RamUsagePercent:F0}%)",
                    IsEnabled = false
                };
                contextMenu.Items.Add(statusItem);

                contextMenu.Items.Add(new Separator());

                // Restore / Open Window
                var openItem = new MenuItem { Header = "🖥️ WinPurify Pro 열기", FontWeight = FontWeights.SemiBold };
                openItem.Click += (s, e) => RestoreMainWindow();
                contextMenu.Items.Add(openItem);

                // Quick Optimize
                var optimizeItem = new MenuItem
                {
                    Header = $"⚡ 선택 항목 즉시 정화 ({_viewModel.SelectedCount}개)",
                    IsEnabled = !_viewModel.IsBusy && _viewModel.SelectedCount > 0
                };
                optimizeItem.Click += async (s, e) => await _viewModel.OptimizeSelectedAsync();
                contextMenu.Items.Add(optimizeItem);

                // Quick Scan
                var scanItem = new MenuItem
                {
                    Header = "🔍 초고속 전체 스캔 (Robocopy)",
                    IsEnabled = !_viewModel.IsBusy
                };
                scanItem.Click += async (s, e) => await _viewModel.ScanAllAsync();
                contextMenu.Items.Add(scanItem);

                // Fast RAM Trim
                var ramItem = new MenuItem
                {
                    Header = "🧠 RAM 즉시 압축 (Trim Working Set)",
                    IsEnabled = !_viewModel.IsBusy
                };
                ramItem.Click += async (s, e) => await _viewModel.CompressRamAsync();
                contextMenu.Items.Add(ramItem);

                contextMenu.Items.Add(new Separator());

                // Open Task Scheduler Modal
                var scheduleItem = new MenuItem { Header = "⏰ 태스크 스케줄러 관리자 열기" };
                scheduleItem.Click += (s, e) =>
                {
                    RestoreMainWindow();
                    _viewModel.OpenScheduleModalCommand.Execute(null);
                };
                contextMenu.Items.Add(scheduleItem);

                // Tray & Notification Settings
                var settingsItem = new MenuItem { Header = "🔔 트레이 및 알림 설정..." };
                settingsItem.Click += (s, e) =>
                {
                    RestoreMainWindow();
                    _viewModel.IsTraySettingsModalOpen = true;
                };
                contextMenu.Items.Add(settingsItem);

                // Open Log Directory
                var logItem = new MenuItem { Header = "📂 작업 로그 폴더 열기" };
                logItem.Click += (s, e) => _viewModel.OpenLogFolderCommand.Execute(null);
                contextMenu.Items.Add(logItem);

                contextMenu.Items.Add(new Separator());

                // Exit Application
                var exitItem = new MenuItem { Header = "🚪 WinPurify Pro 종료 (Exit)", Foreground = System.Windows.Media.Brushes.Crimson };
                exitItem.Click += (s, e) =>
                {
                    CloseToTray = false; // Bypass cancel logic
                    Dispose();
                    System.Windows.Application.Current.Shutdown(0);
                };
                contextMenu.Items.Add(exitItem);

                // Bring main window to foreground to ensure context menu closes properly on outside click
                SetForegroundWindow(new WindowInteropHelper(_mainWindow).Handle);
                contextMenu.IsOpen = true;
            });
        }

        public void Dispose()
        {
            lock (_lockObj)
            {
                if (_isCreated && _windowHandle != IntPtr.Zero)
                {
                    var nid = new NOTIFYICONDATA
                    {
                        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                        hWnd = _windowHandle,
                        uID = 1001
                    };
                    Shell_NotifyIcon(NIM_DELETE, ref nid);
                    _isCreated = false;
                }

                if (_hwndSource != null)
                {
                    _hwndSource.RemoveHook(WndProc);
                    _hwndSource = null;
                }
            }
        }
    }
}
