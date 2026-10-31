using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyTitle("Touch Monitor")]
[assembly: System.Reflection.AssemblyProduct("Touch Monitor")]
[assembly: System.Reflection.AssemblyDescription("Blocks touch-generated mouse input so the cursor is never affected.")]
[assembly: System.Reflection.AssemblyVersion("1.6.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.6.0.0")]

namespace TouchMonitor
{
    internal static class Native
    {
        public const int WH_MOUSE_LL = 14;
        public const int WM_CLOSE = 0x0010;
        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int WM_POINTERACTIVATE = 0x024B;
        public const int WM_POINTERDOWN = 0x0246;
        public const int WM_TOUCH = 0x0240;
        public const int MA_NOACTIVATE = 3;
        public const int WM_HOTKEY = 0x0312;
        public const int WM_QUIT = 0x0012;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_XBUTTONDOWN = 0x020B;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const int SW_RESTORE = 9;
        public const long SignatureMask = 0xFFFFFF00L;
        public const long TouchSignature = 0xFF515700L;
        public const long TouchFlag = 0x80L;
        public const int GWL_EXSTYLE = -20;
        public const long WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_POPUP = unchecked((int)0x80000000);
        public const uint LWA_ALPHA = 0x00000002;
        public const int SW_HIDE = 0;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint MONITOR_DEFAULTTONEAREST = 2;
        public const int DWMWA_CLOAKED = 14;
        public const int EVENT_SYSTEM_FOREGROUND = 0x0003;
        public const int EVENT_OBJECT_SHOW = 0x8002;
        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
        public const int OBJID_WINDOW = 0;
        public const uint GA_ROOT = 2;
        public const int WM_DISPLAYCHANGE = 0x007E;
        public const int WM_INPUT = 0x00FF;
        public const uint LLMHF_INJECTED = 0x00000001;
        public const uint RIDEV_INPUTSINK = 0x00000100;
        public const ushort HID_USAGE_PAGE_DIGITIZER = 0x0D;
        public const ushort HID_USAGE_DIGITIZER_TOUCHSCREEN = 0x04;
        public const int POINTER_DEVICE_TYPE_INTEGRATED_PEN = 1;
        public const int POINTER_DEVICE_TYPE_EXTERNAL_PEN = 2;
        public const int POINTER_DEVICE_TYPE_TOUCH = 3;
        public const int WS_EX_TOPMOST = 0x00000008;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTER_DEVICE_INFO
        {
            public uint displayOrientation;
            public IntPtr device;
            public int pointerDeviceType;
            public IntPtr monitor;
            public uint startingCursorId;
            public ushort maxActiveContacts;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 520)]
            public string productString;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        public delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwmsEventThread, uint dwmsEventTime);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, [MarshalAs(UnmanagedType.Bool)] bool fAttach);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPointerDevices(ref uint deviceCount, [In, Out] POINTER_DEVICE_INFO[] devices);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        public static bool ForceForeground(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;
            IntPtr foreground = GetForegroundWindow();
            uint pid;
            uint foregroundThread = GetWindowThreadProcessId(foreground, out pid);
            uint currentThread = GetCurrentThreadId();
            bool attached = false;
            if (foregroundThread != 0 && foregroundThread != currentThread)
            {
                attached = AttachThreadInput(currentThread, foregroundThread, true);
            }
            BringWindowToTop(hWnd);
            bool ok = SetForegroundWindow(hWnd);
            if (attached) AttachThreadInput(currentThread, foregroundThread, false);
            return ok;
        }
    }

    internal static class DebugLog
    {
        public static volatile bool Enabled;
        private static readonly object Sync = new object();
        private static StreamWriter writer;

        public static void Write(string message)
        {
            if (!Enabled) return;
            try
            {
                lock (Sync)
                {
                    if (writer == null)
                    {
                        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TouchMonitor");
                        Directory.CreateDirectory(dir);
                        FileStream stream = new FileStream(Path.Combine(dir, "debug.log"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        writer = new StreamWriter(stream);
                        writer.AutoFlush = true;
                    }
                    writer.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " " + message);
                }
            }
            catch
            {
            }
        }

        public static string Hex(long value)
        {
            return "0x" + ((ulong)value).ToString("X16");
        }
    }

    internal static class Settings
    {
        private const string KeyPath = @"Software\TouchMonitor";

        public static void Load()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath))
                {
                    if (key == null) return;
                    HookEngine.Enabled = GetBool(key, "Enabled", true);
                    HookEngine.BlockPen = GetBool(key, "BlockPen", false);
                    FocusGuard.FocusLockEnabled = GetBool(key, "FocusGuard", true);
                    FocusGuard.NoActivateEnabled = GetBool(key, "NoActivate", true);
                    FocusGuard.ShieldEnabled = GetBool(key, "Shield", true);
                    object allow = key.GetValue("AllowProcesses");
                    if (allow != null)
                    {
                        FocusGuard.ClearAllowedProcesses();
                        string[] parts = allow.ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < parts.Length; i++)
                        {
                            FocusGuard.AddAllowedProcess(parts[i]);
                        }
                    }
                    object device = key.GetValue("TouchMonitorDevice");
                    if (device != null) FocusGuard.SetTouchMonitorDevice(device.ToString());
                }
            }
            catch
            {
            }
        }

        public static void Save()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath))
                {
                    key.SetValue("Enabled", HookEngine.Enabled ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("BlockPen", HookEngine.BlockPen ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("FocusGuard", FocusGuard.FocusLockEnabled ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("NoActivate", FocusGuard.NoActivateEnabled ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("Shield", FocusGuard.ShieldEnabled ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("AllowProcesses", string.Join(",", FocusGuard.AllowedProcesses.ToArray()), RegistryValueKind.String);
                    string device = FocusGuard.GetTouchMonitorDevice();
                    if (!string.IsNullOrEmpty(device)) key.SetValue("TouchMonitorDevice", device, RegistryValueKind.String);
                }
            }
            catch
            {
            }
        }

        private static bool GetBool(RegistryKey key, string name, bool defaultValue)
        {
            object value = key.GetValue(name);
            if (value == null) return defaultValue;
            try
            {
                return Convert.ToInt32(value) != 0;
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    internal static class HookEngine
    {
        private static readonly object Sync = new object();
        private static readonly ManualResetEvent InstallDone = new ManualResetEvent(false);

        private static IntPtr hookHandle = IntPtr.Zero;
        private static uint hookThreadId;
        private static Thread hookThread;
        private static Native.LowLevelMouseProc hookProc;

        public static volatile bool Started;
        public static volatile bool HookInstalled;
        public static volatile bool Enabled = true;
        public static volatile bool BlockPen;
        public static volatile string LastError = "";

        public static int LastTouchTick;
        public static int LastRealMouseDownTick;
        public static int LastRealMouseX;
        public static int LastRealMouseY;

        private static int lastAnyEventTick;
        private static int lastMaintenanceTick;

        public static long AllEvents;
        public static long TotalSwallowed;
        public static long TouchCount;
        public static long PenCount;
        public static long LastExtraSeen;

        public static bool Start()
        {
            Started = true;
            InstallDone.Reset();
            lastAnyEventTick = Environment.TickCount;
            StartHookThread();
            InstallDone.WaitOne(2000);
            return HookInstalled;
        }

        public static void Stop()
        {
            Started = false;
            HookInstalled = false;
            IntPtr handle = hookHandle;
            hookHandle = IntPtr.Zero;
            if (handle != IntPtr.Zero) Native.UnhookWindowsHookEx(handle);
            uint tid = hookThreadId;
            hookThreadId = 0;
            if (tid != 0) Native.PostThreadMessage(tid, Native.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            if (hookThread != null && hookThread.IsAlive)
            {
                hookThread.Join(1000);
            }
            hookThread = null;
        }

        private static void StartHookThread()
        {
            hookProc = HookCallback;
            hookThread = new Thread(HookThreadProc);
            hookThread.IsBackground = true;
            hookThread.Name = "TouchMonitorHook";
            hookThread.Start();
        }

        private static void HookThreadProc()
        {
            hookThreadId = Native.GetCurrentThreadId();
            IntPtr myHandle = Native.SetWindowsHookEx(Native.WH_MOUSE_LL, hookProc, Native.GetModuleHandle(null), 0);
            if (myHandle == IntPtr.Zero)
            {
                LastError = "SetWindowsHookEx failed (win32 error " + Marshal.GetLastWin32Error() + ")";
                HookInstalled = false;
            }
            else
            {
                hookHandle = myHandle;
                HookInstalled = true;
                LastError = "";
            }
            InstallDone.Set();
            if (myHandle == IntPtr.Zero) return;

            Native.MSG msg;
            while (Native.GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
            {
                Native.TranslateMessage(ref msg);
                Native.DispatchMessage(ref msg);
            }

            Native.UnhookWindowsHookEx(myHandle);
            Interlocked.CompareExchange(ref hookHandle, IntPtr.Zero, myHandle);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Volatile.Write(ref lastAnyEventTick, Environment.TickCount);

                Native.MSLLHOOKSTRUCT info = Marshal.PtrToStructure<Native.MSLLHOOKSTRUCT>(lParam);
                long extra = info.dwExtraInfo.ToInt64();

                if (DebugLog.Enabled)
                {
                    Interlocked.Increment(ref AllEvents);
                    Interlocked.Exchange(ref LastExtraSeen, extra);
                }

                if ((extra & Native.SignatureMask) == Native.TouchSignature)
                {
                    bool isTouch = (extra & Native.TouchFlag) != 0;
                    if (isTouch || BlockPen)
                    {
                        Volatile.Write(ref LastTouchTick, Environment.TickCount);
                        FocusGuard.NotifyTouchEvent(info.pt.X, info.pt.Y);

                        if (Enabled)
                        {
                            if (DebugLog.Enabled)
                            {
                                if (isTouch) Interlocked.Increment(ref TouchCount);
                                else Interlocked.Increment(ref PenCount);
                                Interlocked.Increment(ref TotalSwallowed);
                            }
                            return (IntPtr)1;
                        }
                    }
                }
                else
                {
                    int message = (int)wParam.ToInt64();
                    if (message == Native.WM_LBUTTONDOWN || message == Native.WM_RBUTTONDOWN ||
                        message == Native.WM_MBUTTONDOWN || message == Native.WM_XBUTTONDOWN)
                    {
                        Volatile.Write(ref LastRealMouseDownTick, Environment.TickCount);
                    }
                    if ((info.flags & Native.LLMHF_INJECTED) == 0)
                    {
                        Volatile.Write(ref LastRealMouseX, info.pt.X);
                        Volatile.Write(ref LastRealMouseY, info.pt.Y);
                    }
                }
            }
            return Native.CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        public static void Maintenance()
        {
            if (!Started) return;
            if (!HookInstalled)
            {
                int now = Environment.TickCount;
                if (unchecked(now - lastMaintenanceTick) > 5000)
                {
                    lastMaintenanceTick = now;
                    DebugLog.Write("hook not installed, retrying: " + LastError);
                    InstallDone.Reset();
                    StartHookThread();
                }
                return;
            }
            if (unchecked(Environment.TickCount - Volatile.Read(ref lastAnyEventTick)) > 300000)
            {
                DebugLog.Write("no mouse events for 5 minutes, reinstalling hook");
                Reinstall();
            }
        }

        private static void Reinstall()
        {
            lock (Sync)
            {
                HookInstalled = false;
                IntPtr old = hookHandle;
                hookHandle = IntPtr.Zero;
                if (old != IntPtr.Zero) Native.UnhookWindowsHookEx(old);
                uint tid = hookThreadId;
                hookThreadId = 0;
                if (tid != 0) Native.PostThreadMessage(tid, Native.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                Volatile.Write(ref lastAnyEventTick, Environment.TickCount);
                InstallDone.Reset();
                StartHookThread();
            }
        }
    }

    internal static class FocusGuard
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<IntPtr, IntPtr> originalStyles = new Dictionary<IntPtr, IntPtr>();
        private static readonly StringBuilder classNameBuffer = new StringBuilder(256);
        private static readonly List<string> allowedProcesses = new List<string>();
        private static readonly Dictionary<uint, string> processNames = new Dictionary<uint, string>();

        public static volatile bool FocusLockEnabled = true;
        public static volatile bool NoActivateEnabled = true;
        public static volatile bool ShieldEnabled = true;

        private static readonly TouchShield Shield = new TouchShield();

        private static Native.WinEventProc winEventProc;
        private static IntPtr foregroundHook = IntPtr.Zero;
        private static IntPtr showHook = IntPtr.Zero;
        private static uint ourPid;

        private static int inSequence;
        private static int sequencePending;
        private static int lastTouchX;
        private static int lastTouchY;
        private static IntPtr capturedTouchForeground = IntPtr.Zero;
        private static IntPtr protectedWindow = IntPtr.Zero;
        private static IntPtr previousForeground = IntPtr.Zero;
        private static IntPtr currentForeground = IntPtr.Zero;
        private static int lastForegroundChangeTick;
        private static IntPtr touchMonitor = IntPtr.Zero;
        private static string touchMonitorDevice = "";
        private static bool pointerDetectionWorked;
        private static int lastSweepTick;
        private static int lastAllowedRefreshTick;
        private static int lastPointerRefreshTick;
        private static int lastProcessCacheClear;
        private static int lastCursorRestoreLogTick;
        private static int lastRawTouchLogTick;

        private static readonly string[] ExcludedClasses = new string[]
        {
            "Shell_TrayWnd",
            "tooltips_class32",
            "Windows.Internal.Shell.TabProxyWindow"
        };

        private static readonly string[] DesktopClasses = new string[]
        {
            "Progman",
            "WorkerW"
        };

        public static void Start()
        {
            ourPid = Native.GetCurrentProcessId();
            currentForeground = Native.GetForegroundWindow();
            Native.POINT cursor;
            if (Native.GetCursorPos(out cursor))
            {
                HookEngine.LastRealMouseX = cursor.X;
                HookEngine.LastRealMouseY = cursor.Y;
            }
            winEventProc = OnWinEvent;
            foregroundHook = Native.SetWinEventHook((uint)Native.EVENT_SYSTEM_FOREGROUND, (uint)Native.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winEventProc, 0, 0, Native.WINEVENT_OUTOFCONTEXT | Native.WINEVENT_SKIPOWNPROCESS);
            showHook = Native.SetWinEventHook((uint)Native.EVENT_OBJECT_SHOW, (uint)Native.EVENT_OBJECT_SHOW, IntPtr.Zero, winEventProc, 0, 0, Native.WINEVENT_OUTOFCONTEXT | Native.WINEVENT_SKIPOWNPROCESS);
            RefreshPointerMonitorFromDevice();
            if (touchMonitor == IntPtr.Zero) touchMonitor = FindMonitorByDevice(touchMonitorDevice);
            lastPointerRefreshTick = Environment.TickCount;
            DebugLog.Write("focus guard started. foregroundHook=" + (foregroundHook != IntPtr.Zero) + " showHook=" + (showHook != IntPtr.Zero) + " touchMonitor=" + touchMonitor.ToInt64().ToString("X"));
        }

        public static void Stop()
        {
            if (foregroundHook != IntPtr.Zero) { Native.UnhookWinEvent(foregroundHook); foregroundHook = IntPtr.Zero; }
            if (showHook != IntPtr.Zero) { Native.UnhookWinEvent(showHook); showHook = IntPtr.Zero; }
            Shield.Hide();
            Shield.DestroyHandle();
            RestoreStyles();
        }

        public static void NotifyTouchEvent(int x, int y)
        {
            Volatile.Write(ref lastTouchX, x);
            Volatile.Write(ref lastTouchY, y);
            if (Volatile.Read(ref sequencePending) == 1 || Volatile.Read(ref inSequence) == 1) return;
            capturedTouchForeground = Native.GetForegroundWindow();
            Volatile.Write(ref sequencePending, 1);
        }

        public static void OnRawTouch()
        {
            Volatile.Write(ref HookEngine.LastTouchTick, Environment.TickCount);
            Native.POINT point;
            if (Native.GetCursorPos(out point))
            {
                NotifyTouchEvent(point.X, point.Y);
            }
            RestoreCursorToMouse();
            if (DebugLog.Enabled && unchecked(Environment.TickCount - lastRawTouchLogTick) > 1000)
            {
                lastRawTouchLogTick = Environment.TickCount;
                DebugLog.Write("raw touch input received");
            }
        }

        private static void ResolvePendingSequence()
        {
            if (Volatile.Read(ref sequencePending) != 1) return;
            lock (Sync)
            {
                if (sequencePending == 0) return;
                sequencePending = 0;
                if (inSequence == 1) return;
                inSequence = 1;
                LearnTouchMonitor(Volatile.Read(ref lastTouchX), Volatile.Read(ref lastTouchY));
                IntPtr foreground = capturedTouchForeground;
                IntPtr target = foreground;
                bool race = false;
                bool recentActivation = previousForeground != IntPtr.Zero &&
                    unchecked(Environment.TickCount - lastForegroundChangeTick) < 400;
                bool realMouseRecent = unchecked(Environment.TickCount - Volatile.Read(ref HookEngine.LastRealMouseDownTick)) < 250;
                if (recentActivation && !realMouseRecent)
                {
                    target = previousForeground;
                    race = true;
                }
                protectedWindow = target;
                DebugLog.Write("touch sequence start: foreground=" + foreground.ToInt64().ToString("X") + " previous=" + previousForeground.ToInt64().ToString("X") + " protected=" + target.ToInt64().ToString("X") + " raceCorrection=" + race);
            }
        }

        public static void Tick()
        {
            ResolvePendingSequence();

            bool sequenceActive = Volatile.Read(ref inSequence) == 1;
            if (sequenceActive && unchecked(Environment.TickCount - Volatile.Read(ref HookEngine.LastTouchTick)) > 300)
            {
                Volatile.Write(ref inSequence, 0);
                sequenceActive = false;
                IntPtr target;
                lock (Sync) { target = protectedWindow; }
                if (FocusLockEnabled && target != IntPtr.Zero && Native.IsWindow(target) && Native.GetForegroundWindow() != target)
                {
                    Restore(target);
                    DebugLog.Write("focus guard: restored " + target.ToInt64().ToString("X") + " after touch sequence");
                }
                RestoreCursorToMouse();
            }

            if (sequenceActive && FocusLockEnabled)
            {
                IntPtr target;
                lock (Sync) { target = protectedWindow; }
                if (target != IntPtr.Zero && Native.IsWindow(target) && Native.GetForegroundWindow() != target)
                {
                    DebugLog.Write("focus lock: restoring " + target.ToInt64().ToString("X") + " (foreground=" + Native.GetForegroundWindow().ToInt64().ToString("X") + ")");
                    Restore(target);
                    DebugLog.Write("focus lock: after restore foreground=" + Native.GetForegroundWindow().ToInt64().ToString("X") + " target=" + target.ToInt64().ToString("X"));
                }
            }

            if (sequenceActive)
            {
                RestoreCursorToMouse();
            }

            if (NoActivateEnabled && touchMonitor != IntPtr.Zero && unchecked(Environment.TickCount - lastSweepTick) > 3000)
            {
                lastSweepTick = Environment.TickCount;
                Sweep();
            }

            int now = Environment.TickCount;
            if (unchecked(now - lastProcessCacheClear) > 60000)
            {
                lastProcessCacheClear = now;
                if (processNames.Count > 0) processNames.Clear();
            }

            UpdateShield();
        }

        public static IntPtr TouchMonitor
        {
            get { return touchMonitor; }
        }

        private static void UpdateShield()
        {
            int now = Environment.TickCount;
            if (unchecked(now - lastPointerRefreshTick) > 30000)
            {
                lastPointerRefreshTick = now;
                RefreshPointerMonitorFromDevice();
            }

            if (!ShieldEnabled || touchMonitor == IntPtr.Zero)
            {
                Shield.Hide();
                return;
            }
            IntPtr foreground = Native.GetForegroundWindow();
            bool fullscreen = IsFullscreenWindow(foreground);
            if (fullscreen)
            {
                bool wasShown = Shield.Shown;
                if (!wasShown) RefreshPointerMonitorFromDevice();
                Shield.ShowOn(touchMonitor);
                if (!wasShown || unchecked(now - lastAllowedRefreshTick) > 500)
                {
                    lastAllowedRefreshTick = now;
                    Shield.SetAllowed(GetAllowedWindows());
                }
            }
            else
            {
                Shield.Hide();
            }
        }

        public static List<string> AllowedProcesses
        {
            get { return allowedProcesses; }
        }

        public static void AddAllowedProcess(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            string normalized = name.ToLowerInvariant();
            if (normalized.EndsWith(".exe")) normalized = normalized.Substring(0, normalized.Length - 4);
            if (!allowedProcesses.Contains(normalized)) allowedProcesses.Add(normalized);
            processNames.Clear();
        }

        public static void ClearAllowedProcesses()
        {
            allowedProcesses.Clear();
            processNames.Clear();
        }

        public static string GetProcessNameFor(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return "";
            uint pid;
            Native.GetWindowThreadProcessId(hWnd, out pid);
            return GetProcessName(pid);
        }

        private static string GetProcessName(uint pid)
        {
            string name;
            if (processNames.TryGetValue(pid, out name)) return name;
            try
            {
                using (Process p = Process.GetProcessById((int)pid))
                {
                    name = p.ProcessName;
                }
            }
            catch
            {
                name = "";
            }
            processNames[pid] = name;
            return name;
        }

        private static List<IntPtr> GetAllowedWindows()
        {
            List<IntPtr> result = new List<IntPtr>();
            if (allowedProcesses.Count == 0 || touchMonitor == IntPtr.Zero) return result;
            Native.EnumWindows(delegate(IntPtr hWnd, IntPtr lParam)
            {
                if (!Native.IsWindowVisible(hWnd)) return true;
                if (Native.GetAncestor(hWnd, Native.GA_ROOT) != hWnd) return true;
                string name = GetClassName(hWnd);
                if (IsDesktopClass(name)) return true;
                for (int i = 0; i < ExcludedClasses.Length; i++)
                {
                    if (string.Equals(name, ExcludedClasses[i], StringComparison.OrdinalIgnoreCase)) return true;
                }
                if (Native.MonitorFromWindow(hWnd, Native.MONITOR_DEFAULTTONEAREST) != touchMonitor) return true;
                uint pid;
                Native.GetWindowThreadProcessId(hWnd, out pid);
                if (pid == ourPid) return true;
                string process = GetProcessName(pid);
                if (allowedProcesses.Contains(process)) result.Add(hWnd);
                return true;
            }, IntPtr.Zero);
            return result;
        }

        private static bool IsFullscreenWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;
            if (hWnd == Shield.Handle) return false;
            if (Native.IsIconic(hWnd)) return false;
            uint pid;
            Native.GetWindowThreadProcessId(hWnd, out pid);
            if (pid == ourPid) return false;
            string name = GetClassName(hWnd);
            if (IsDesktopClass(name)) return false;
            for (int i = 0; i < ExcludedClasses.Length; i++)
            {
                if (string.Equals(name, ExcludedClasses[i], StringComparison.OrdinalIgnoreCase)) return false;
            }
            Native.RECT windowRect;
            if (!Native.GetWindowRect(hWnd, out windowRect)) return false;
            IntPtr monitor = Native.MonitorFromWindow(hWnd, Native.MONITOR_DEFAULTTONEAREST);
            if (monitor == IntPtr.Zero) return false;
            Native.MONITORINFOEX info = new Native.MONITORINFOEX();
            info.cbSize = Marshal.SizeOf(typeof(Native.MONITORINFOEX));
            if (!Native.GetMonitorInfo(monitor, ref info)) return false;
            return windowRect.Left <= info.rcMonitor.Left && windowRect.Top <= info.rcMonitor.Top &&
                   windowRect.Right >= info.rcMonitor.Right && windowRect.Bottom >= info.rcMonitor.Bottom;
        }

        public static void SweepNow()
        {
            lastSweepTick = Environment.TickCount;
            if (touchMonitor == IntPtr.Zero) touchMonitor = FindMonitorByDevice(touchMonitorDevice);
            Sweep();
        }

        private static void Sweep()
        {
            if (touchMonitor == IntPtr.Zero) return;
            Native.EnumWindows(delegate(IntPtr hWnd, IntPtr lParam)
            {
                ApplyNoActivate(hWnd);
                return true;
            }, IntPtr.Zero);
        }

        private static void ApplyNoActivate(IntPtr hWnd)
        {
            try
            {
                if (hWnd == IntPtr.Zero || !Native.IsWindowVisible(hWnd)) return;
                if (Native.GetAncestor(hWnd, Native.GA_ROOT) != hWnd) return;
                if (IsExcluded(hWnd)) return;
                string name = GetClassName(hWnd);
                if (!IsDesktopClass(name) && Native.MonitorFromWindow(hWnd, Native.MONITOR_DEFAULTTONEAREST) != touchMonitor) return;
                IntPtr style = Native.GetWindowLongPtr(hWnd, Native.GWL_EXSTYLE);
                long value = style.ToInt64();
                if ((value & Native.WS_EX_NOACTIVATE) != 0) return;
                if (!originalStyles.ContainsKey(hWnd)) originalStyles[hWnd] = style;
                Native.SetWindowLongPtr(hWnd, Native.GWL_EXSTYLE, new IntPtr(value | Native.WS_EX_NOACTIVATE));
                DebugLog.Write("no-activate set on " + hWnd.ToInt64().ToString("X") + " class=" + name);
            }
            catch
            {
            }
        }

        private static bool IsDesktopClass(string name)
        {
            for (int i = 0; i < DesktopClasses.Length; i++)
            {
                if (string.Equals(name, DesktopClasses[i], StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static void RestoreStyles()
        {
            lock (Sync)
            {
                foreach (KeyValuePair<IntPtr, IntPtr> entry in originalStyles)
                {
                    if (Native.IsWindow(entry.Key))
                    {
                        Native.SetWindowLongPtr(entry.Key, Native.GWL_EXSTYLE, entry.Value);
                    }
                }
                originalStyles.Clear();
            }
        }

        private static bool IsExcluded(IntPtr hWnd)
        {
            uint pid;
            Native.GetWindowThreadProcessId(hWnd, out pid);
            if (pid == ourPid) return true;
            if (IsCloaked(hWnd)) return true;
            string name = GetClassName(hWnd);
            for (int i = 0; i < ExcludedClasses.Length; i++)
            {
                if (string.Equals(name, ExcludedClasses[i], StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static bool IsCloaked(IntPtr hWnd)
        {
            int cloaked;
            return Native.DwmGetWindowAttribute(hWnd, Native.DWMWA_CLOAKED, out cloaked, 4) == 0 && cloaked != 0;
        }

        private static string GetClassName(IntPtr hWnd)
        {
            classNameBuffer.Length = 0;
            Native.GetClassName(hWnd, classNameBuffer, classNameBuffer.Capacity);
            return classNameBuffer.ToString();
        }

        private static void OnWinEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwmsEventThread, uint dwmsEventTime)
        {
            if (eventType == (uint)Native.EVENT_SYSTEM_FOREGROUND)
            {
                IntPtr toRestore = IntPtr.Zero;
                lock (Sync)
                {
                    previousForeground = currentForeground;
                    currentForeground = hwnd;
                    lastForegroundChangeTick = Environment.TickCount;
                    if (FocusLockEnabled && previousForeground != IntPtr.Zero && previousForeground != hwnd)
                    {
                        if (inSequence == 1 && protectedWindow != IntPtr.Zero && hwnd != protectedWindow)
                        {
                            toRestore = protectedWindow;
                        }
                        else if (inSequence == 0)
                        {
                            bool realMouseRecent = unchecked(Environment.TickCount - Volatile.Read(ref HookEngine.LastRealMouseDownTick)) < 250;
                            bool fromOutside = !IsOnTouchMonitor(previousForeground);
                            bool toTouchArea = IsOnTouchMonitor(hwnd) || IsDesktopClass(GetClassName(hwnd));
                            if (!realMouseRecent && fromOutside && toTouchArea)
                            {
                                toRestore = previousForeground;
                            }
                        }
                    }
                }
                if (DebugLog.Enabled)
                {
                    uint fpid;
                    Native.GetWindowThreadProcessId(hwnd, out fpid);
                    IntPtr monitor = Native.MonitorFromWindow(hwnd, Native.MONITOR_DEFAULTTONEAREST);
                    DebugLog.Write("foreground: hwnd=" + hwnd.ToInt64().ToString("X") + " pid=" + fpid + " class=" + GetClassName(hwnd) + " monitor=" + (monitor == touchMonitor ? "touch" : "other") + " inSequence=" + Volatile.Read(ref inSequence) + " restore=" + (toRestore != IntPtr.Zero));
                }
                if (NoActivateEnabled && touchMonitor != IntPtr.Zero && Native.MonitorFromWindow(hwnd, Native.MONITOR_DEFAULTTONEAREST) == touchMonitor)
                {
                    ApplyNoActivate(hwnd);
                }
                if (toRestore != IntPtr.Zero && Native.IsWindow(toRestore))
                {
                    Restore(toRestore);
                    DebugLog.Write("focus lock: restored " + toRestore.ToInt64().ToString("X") + " (activation of " + hwnd.ToInt64().ToString("X") + " was rejected)");
                }
            }
            else if (eventType == (uint)Native.EVENT_OBJECT_SHOW)
            {
                if (idObject == Native.OBJID_WINDOW && idChild == 0 && NoActivateEnabled)
                {
                    ApplyNoActivate(hwnd);
                }
            }
        }

        private static void Restore(IntPtr target)
        {
            try
            {
                if (Native.IsIconic(target)) Native.ShowWindow(target, Native.SW_RESTORE);
                Native.ForceForeground(target);
            }
            catch
            {
            }
        }

        private static void RestoreCursorToMouse()
        {
            if (!HookEngine.Enabled) return;
            int mx = Volatile.Read(ref HookEngine.LastRealMouseX);
            int my = Volatile.Read(ref HookEngine.LastRealMouseY);
            Native.POINT p;
            if (!Native.GetCursorPos(out p)) return;
            if (p.X == mx && p.Y == my) return;
            Native.SetCursorPos(mx, my);
            if (DebugLog.Enabled && unchecked(Environment.TickCount - lastCursorRestoreLogTick) > 1000)
            {
                lastCursorRestoreLogTick = Environment.TickCount;
                DebugLog.Write("cursor restored to " + mx + "," + my + " (was " + p.X + "," + p.Y + ")");
            }
        }

        private static bool IsOnTouchMonitor(IntPtr hWnd)
        {
            if (touchMonitor == IntPtr.Zero || hWnd == IntPtr.Zero) return false;
            return Native.MonitorFromWindow(hWnd, Native.MONITOR_DEFAULTTONEAREST) == touchMonitor;
        }

        public static void RefreshPointerMonitorFromDevice()
        {
            try
            {
                uint count = 0;
                Native.GetPointerDevices(ref count, null);
                if (count == 0) return;
                Native.POINTER_DEVICE_INFO[] devices = new Native.POINTER_DEVICE_INFO[count];
                if (!Native.GetPointerDevices(ref count, devices)) return;
                IntPtr touch = IntPtr.Zero;
                IntPtr pen = IntPtr.Zero;
                for (int i = 0; i < count; i++)
                {
                    if (devices[i].monitor == IntPtr.Zero) continue;
                    if (devices[i].pointerDeviceType == Native.POINTER_DEVICE_TYPE_TOUCH)
                    {
                        touch = devices[i].monitor;
                        break;
                    }
                    if (pen == IntPtr.Zero &&
                        (devices[i].pointerDeviceType == Native.POINTER_DEVICE_TYPE_INTEGRATED_PEN ||
                         devices[i].pointerDeviceType == Native.POINTER_DEVICE_TYPE_EXTERNAL_PEN))
                    {
                        pen = devices[i].monitor;
                    }
                }
                IntPtr target = touch != IntPtr.Zero ? touch : pen;
                if (target == IntPtr.Zero)
                {
                    pointerDetectionWorked = false;
                    return;
                }
                pointerDetectionWorked = true;
                if (target == touchMonitor) return;
                touchMonitor = target;
                touchMonitorDevice = GetDeviceName(target);
                lastSweepTick = 0;
                DebugLog.Write("touch monitor detected via pointer device: " + touchMonitorDevice);
                Settings.Save();
            }
            catch
            {
            }
        }

        private static void LearnTouchMonitor(int x, int y)
        {
            if (pointerDetectionWorked) return;
            Native.POINT point;
            point.X = x;
            point.Y = y;
            IntPtr monitor = Native.MonitorFromPoint(point, Native.MONITOR_DEFAULTTONEAREST);
            if (monitor == IntPtr.Zero || monitor == touchMonitor) return;
            touchMonitor = monitor;
            touchMonitorDevice = GetDeviceName(monitor);
            lastSweepTick = 0;
            DebugLog.Write("touch monitor learned: " + touchMonitor.ToInt64().ToString("X") + " device=" + touchMonitorDevice);
            Settings.Save();
        }

        public static void SetTouchMonitorDevice(string device)
        {
            if (string.IsNullOrEmpty(device)) return;
            touchMonitorDevice = device;
        }

        public static string GetTouchMonitorDevice()
        {
            return touchMonitorDevice;
        }

        private static string GetDeviceName(IntPtr monitor)
        {
            Native.MONITORINFOEX info = new Native.MONITORINFOEX();
            info.cbSize = Marshal.SizeOf(typeof(Native.MONITORINFOEX));
            if (Native.GetMonitorInfo(monitor, ref info)) return info.szDevice;
            return "";
        }

        private static IntPtr FindMonitorByDevice(string device)
        {
            if (string.IsNullOrEmpty(device)) return IntPtr.Zero;
            IntPtr found = IntPtr.Zero;
            Native.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate(IntPtr hMonitor, IntPtr hdc, ref Native.RECT rect, IntPtr data)
            {
                if (string.Equals(GetDeviceName(hMonitor), device, StringComparison.OrdinalIgnoreCase))
                {
                    found = hMonitor;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }
    }

    internal sealed class TouchShield : NativeWindow
    {
        public bool Shown { get; private set; }

        private readonly List<IntPtr> raisedWindows = new List<IntPtr>();
        private readonly List<bool> raisedWasTopmost = new List<bool>();
        private readonly List<bool> raisedAddedNoActivate = new List<bool>();
        private int lastX;
        private int lastY;
        private int lastWidth;
        private int lastHeight;
        private bool positioned;
        private int lastTouchLogTick;

        private void EnsureCreated()
        {
            if (Handle != IntPtr.Zero) return;
            CreateParams cp = new CreateParams();
            cp.Caption = "TouchMonitorShield";
            cp.Style = Native.WS_POPUP;
            cp.ExStyle = (int)(Native.WS_EX_NOACTIVATE | Native.WS_EX_TOOLWINDOW | Native.WS_EX_LAYERED);
            CreateHandle(cp);
            Native.SetLayeredWindowAttributes(Handle, 0, 1, Native.LWA_ALPHA);
        }

        public void ShowOn(IntPtr monitor)
        {
            if (monitor == IntPtr.Zero) return;
            EnsureCreated();
            Native.MONITORINFOEX info = new Native.MONITORINFOEX();
            info.cbSize = Marshal.SizeOf(typeof(Native.MONITORINFOEX));
            if (!Native.GetMonitorInfo(monitor, ref info)) return;
            int x = info.rcMonitor.Left;
            int y = info.rcMonitor.Top;
            int width = info.rcMonitor.Right - x;
            int height = info.rcMonitor.Bottom - y;
            if (Shown && positioned && x == lastX && y == lastY && width == lastWidth && height == lastHeight) return;
            Native.SetWindowPos(Handle, Native.HWND_TOPMOST, x, y, width, height, Native.SWP_NOACTIVATE | Native.SWP_SHOWWINDOW);
            if (!Shown) DebugLog.Write("shield: shown at " + x + "," + y + " " + width + "x" + height);
            lastX = x;
            lastY = y;
            lastWidth = width;
            lastHeight = height;
            positioned = true;
            Shown = true;
        }

        public void SetAllowed(List<IntPtr> allowedWindows)
        {
            if (allowedWindows == null) return;
            if (Shown && Handle != IntPtr.Zero)
            {
                Native.SetWindowPos(Handle, Native.HWND_TOPMOST, 0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
            }
            for (int i = raisedWindows.Count - 1; i >= 0; i--)
            {
                if (!allowedWindows.Contains(raisedWindows[i]))
                {
                    if (Native.IsWindow(raisedWindows[i]))
                    {
                        if (raisedAddedNoActivate[i])
                        {
                            IntPtr style = Native.GetWindowLongPtr(raisedWindows[i], Native.GWL_EXSTYLE);
                            Native.SetWindowLongPtr(raisedWindows[i], Native.GWL_EXSTYLE, new IntPtr(style.ToInt64() & ~Native.WS_EX_NOACTIVATE));
                        }
                        if (!raisedWasTopmost[i])
                        {
                            Native.SetWindowPos(raisedWindows[i], Native.HWND_NOTOPMOST, 0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
                        }
                    }
                    raisedWindows.RemoveAt(i);
                    raisedWasTopmost.RemoveAt(i);
                    raisedAddedNoActivate.RemoveAt(i);
                }
            }
            for (int i = allowedWindows.Count - 1; i >= 0; i--)
            {
                IntPtr hWnd = allowedWindows[i];
                if (raisedWindows.Contains(hWnd))
                {
                    Native.SetWindowPos(hWnd, Native.HWND_TOPMOST, 0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
                    continue;
                }
                IntPtr style = Native.GetWindowLongPtr(hWnd, Native.GWL_EXSTYLE);
                bool wasTopmost = (style.ToInt64() & Native.WS_EX_TOPMOST) != 0;
                bool hadNoActivate = (style.ToInt64() & Native.WS_EX_NOACTIVATE) != 0;
                if (!hadNoActivate)
                {
                    Native.SetWindowLongPtr(hWnd, Native.GWL_EXSTYLE, new IntPtr(style.ToInt64() | Native.WS_EX_NOACTIVATE));
                }
                Native.SetWindowPos(hWnd, Native.HWND_TOPMOST, 0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
                raisedWindows.Add(hWnd);
                raisedWasTopmost.Add(wasTopmost);
                raisedAddedNoActivate.Add(!hadNoActivate);
            }
        }

        public void Hide()
        {
            if (Handle == IntPtr.Zero) return;
            if (Shown)
            {
                Native.ShowWindow(Handle, Native.SW_HIDE);
                DebugLog.Write("shield: hidden");
            }
            Shown = false;
            positioned = false;
            for (int i = 0; i < raisedWindows.Count; i++)
            {
                if (!Native.IsWindow(raisedWindows[i])) continue;
                if (raisedAddedNoActivate[i])
                {
                    IntPtr style = Native.GetWindowLongPtr(raisedWindows[i], Native.GWL_EXSTYLE);
                    Native.SetWindowLongPtr(raisedWindows[i], Native.GWL_EXSTYLE, new IntPtr(style.ToInt64() & ~Native.WS_EX_NOACTIVATE));
                }
                if (!raisedWasTopmost[i])
                {
                    Native.SetWindowPos(raisedWindows[i], Native.HWND_NOTOPMOST, 0, 0, 0, 0, Native.SWP_NOMOVE | Native.SWP_NOSIZE | Native.SWP_NOACTIVATE);
                }
            }
            raisedWindows.Clear();
            raisedWasTopmost.Clear();
            raisedAddedNoActivate.Clear();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_MOUSEACTIVATE || m.Msg == Native.WM_POINTERACTIVATE)
            {
                m.Result = (IntPtr)Native.MA_NOACTIVATE;
                return;
            }
            if (m.Msg == Native.WM_POINTERDOWN || m.Msg == Native.WM_TOUCH)
            {
                int now = Environment.TickCount;
                if (unchecked(now - lastTouchLogTick) > 1000)
                {
                    lastTouchLogTick = now;
                    DebugLog.Write("shield: touch input captured");
                }
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }
    }

    internal static class TaskHelper
    {
        private const string TaskName = "TouchMonitor";

        public static bool IsInstalled()
        {
            return Run("schtasks.exe", "/Query /TN \"" + TaskName + "\"") == 0;
        }

        public static bool Install()
        {
            string exe = Application.ExecutablePath;
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            string args = "/Create /F /SC ONLOGON /RL HIGHEST /IT /RU \"" + user + "\" /TN \"" + TaskName + "\" /TR \"\\\"" + exe + "\\\"\"";
            return Run("schtasks.exe", args) == 0;
        }

        public static bool Uninstall()
        {
            return Run("schtasks.exe", "/Delete /F /TN \"" + TaskName + "\"") == 0;
        }

        private static int Run(string file, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(file, args);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                using (Process p = Process.Start(psi))
                {
                    p.StandardOutput.ReadToEnd();
                    p.StandardError.ReadToEnd();
                    p.WaitForExit(10000);
                    return p.HasExited ? p.ExitCode : -1;
                }
            }
            catch
            {
                return -1;
            }
        }
    }

    internal sealed class MessageWindow : NativeWindow
    {
        public event EventHandler HotkeyPressed;
        public event EventHandler ShieldHotkeyPressed;
        public event EventHandler ExitRequested;

        public MessageWindow()
        {
            CreateParams cp = new CreateParams();
            cp.Caption = "TouchMonitorMessageWindow";
            CreateHandle(cp);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_HOTKEY)
            {
                if (m.WParam.ToInt64() == TrayContext.ShieldHotkeyId)
                {
                    EventHandler shieldHandler = ShieldHotkeyPressed;
                    if (shieldHandler != null) shieldHandler(this, EventArgs.Empty);
                }
                else
                {
                    EventHandler handler = HotkeyPressed;
                    if (handler != null) handler(this, EventArgs.Empty);
                }
            }
            else if (m.Msg == Native.WM_CLOSE)
            {
                EventHandler handler = ExitRequested;
                if (handler != null) handler(this, EventArgs.Empty);
                return;
            }
            else if (m.Msg == Native.WM_DISPLAYCHANGE)
            {
                FocusGuard.RefreshPointerMonitorFromDevice();
            }
            else if (m.Msg == Native.WM_INPUT)
            {
                FocusGuard.OnRawTouch();
            }
            base.WndProc(ref m);
        }
    }

    internal sealed class TrayContext : ApplicationContext
    {
        private const int HotkeyId = 0x7A01;
        public const int ShieldHotkeyId = 0x7A02;
        private const string HotkeyText = "Ctrl+Alt+T";
        private const string ShieldHotkeyText = "Ctrl+Alt+S";

        private readonly NotifyIcon tray;
        private readonly ContextMenuStrip menu;
        private readonly ToolStripMenuItem itemEnabled;
        private readonly ToolStripMenuItem itemPen;
        private readonly ToolStripMenuItem itemFocus;
        private readonly ToolStripMenuItem itemNoActivate;
        private readonly ToolStripMenuItem itemShield;
        private readonly ToolStripMenuItem itemAllowApp;
        private readonly ToolStripMenuItem itemAllowedList;
        private readonly ToolStripMenuItem itemClearAllowed;
        private readonly ToolStripMenuItem itemStartup;
        private readonly MessageWindow messages;
        private readonly System.Windows.Forms.Timer timer;
        private bool disposed;
        private bool startupCheckDone;
        private readonly int startTick;

        private long lastLogAll;
        private long lastLogSwallowed;
        private int lastLogTick;

        public TrayContext()
        {
            startTick = Environment.TickCount;
            FocusGuard.Start();

            messages = new MessageWindow();
            messages.HotkeyPressed += OnHotkey;
            messages.ExitRequested += OnExit;

            Native.RAWINPUTDEVICE[] rawDevices = new Native.RAWINPUTDEVICE[1];
            rawDevices[0].usUsagePage = Native.HID_USAGE_PAGE_DIGITIZER;
            rawDevices[0].usUsage = Native.HID_USAGE_DIGITIZER_TOUCHSCREEN;
            rawDevices[0].dwFlags = Native.RIDEV_INPUTSINK;
            rawDevices[0].hwndTarget = messages.Handle;
            if (!Native.RegisterRawInputDevices(rawDevices, 1, (uint)Marshal.SizeOf(typeof(Native.RAWINPUTDEVICE))))
            {
                DebugLog.Write("raw input registration failed (win32 error " + Marshal.GetLastWin32Error() + ")");
            }
            else
            {
                DebugLog.Write("raw input registered for touch screen");
            }

            itemEnabled = new ToolStripMenuItem("Block touch as mouse", null, OnToggleEnabled);
            itemEnabled.Checked = HookEngine.Enabled;

            itemPen = new ToolStripMenuItem("Also block pen input", null, OnTogglePen);
            itemPen.Checked = HookEngine.BlockPen;

            itemFocus = new ToolStripMenuItem("Keep game focused (Focus Lock)", null, OnToggleFocus);
            itemFocus.Checked = FocusGuard.FocusLockEnabled;

            itemNoActivate = new ToolStripMenuItem("Touch screen windows never activate", null, OnToggleNoActivate);
            itemNoActivate.Checked = FocusGuard.NoActivateEnabled;

            itemShield = new ToolStripMenuItem("Shield touch screen in fullscreen apps", null, OnToggleShield);
            itemShield.Checked = FocusGuard.ShieldEnabled;

            itemAllowApp = new ToolStripMenuItem("Allow app under cursor on touch screen", null, OnAllowAppUnderCursor);
            itemAllowedList = new ToolStripMenuItem("Allowed apps: (none)");
            itemAllowedList.Enabled = false;
            itemClearAllowed = new ToolStripMenuItem("Clear allowed apps", null, OnClearAllowed);

            ToolStripMenuItem itemHotkey = new ToolStripMenuItem("Hotkeys: " + HotkeyText + " / " + ShieldHotkeyText);
            itemHotkey.Enabled = false;

            itemStartup = new ToolStripMenuItem("Start with Windows", null, OnToggleStartup);
            itemStartup.Checked = false;

            ToolStripMenuItem itemExit = new ToolStripMenuItem("Exit", null, OnExit);

            menu = new ContextMenuStrip();
            menu.Items.Add(itemEnabled);
            menu.Items.Add(itemPen);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemFocus);
            menu.Items.Add(itemNoActivate);
            menu.Items.Add(itemShield);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemAllowApp);
            menu.Items.Add(itemAllowedList);
            menu.Items.Add(itemClearAllowed);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemHotkey);
            menu.Items.Add(itemStartup);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemExit);

            tray = new NotifyIcon();
            tray.ContextMenuStrip = menu;
            tray.Visible = true;
            tray.DoubleClick += OnToggleEnabled;
            menu.Opening += delegate(object s, System.ComponentModel.CancelEventArgs e) { UpdateAllowedLabel(); };
            UpdateTray();

            if (!Native.RegisterHotKey(messages.Handle, HotkeyId, Native.MOD_CONTROL | Native.MOD_ALT, 0x54))
            {
                DebugLog.Write("hotkey " + HotkeyText + " could not be registered");
            }
            if (!Native.RegisterHotKey(messages.Handle, ShieldHotkeyId, Native.MOD_CONTROL | Native.MOD_ALT, 0x53))
            {
                DebugLog.Write("hotkey " + ShieldHotkeyText + " could not be registered");
            }

            messages.ShieldHotkeyPressed += OnToggleShield;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += OnTick;
            timer.Start();

            if (FocusGuard.NoActivateEnabled) FocusGuard.SweepNow();
        }

        private void UpdateTray()
        {
            tray.Text = "Touch Monitor - " + (HookEngine.Enabled ? "active" : "paused");
            tray.Icon = HookEngine.Enabled ? SystemIcons.Shield : SystemIcons.Application;
        }

        private void OnToggleEnabled(object sender, EventArgs e)
        {
            HookEngine.Enabled = !HookEngine.Enabled;
            itemEnabled.Checked = HookEngine.Enabled;
            Settings.Save();
            UpdateTray();
        }

        private void OnTogglePen(object sender, EventArgs e)
        {
            HookEngine.BlockPen = !HookEngine.BlockPen;
            itemPen.Checked = HookEngine.BlockPen;
            Settings.Save();
        }

        private void OnToggleFocus(object sender, EventArgs e)
        {
            FocusGuard.FocusLockEnabled = !FocusGuard.FocusLockEnabled;
            itemFocus.Checked = FocusGuard.FocusLockEnabled;
            Settings.Save();
        }

        private void OnToggleNoActivate(object sender, EventArgs e)
        {
            FocusGuard.NoActivateEnabled = !FocusGuard.NoActivateEnabled;
            itemNoActivate.Checked = FocusGuard.NoActivateEnabled;
            Settings.Save();
            if (FocusGuard.NoActivateEnabled) FocusGuard.SweepNow();
            else FocusGuard.RestoreStyles();
        }

        private void OnToggleShield(object sender, EventArgs e)
        {
            FocusGuard.ShieldEnabled = !FocusGuard.ShieldEnabled;
            itemShield.Checked = FocusGuard.ShieldEnabled;
            Settings.Save();
        }

        private void OnAllowAppUnderCursor(object sender, EventArgs e)
        {
            Native.POINT point;
            if (!Native.GetCursorPos(out point)) return;
            IntPtr hWnd = Native.WindowFromPoint(point);
            IntPtr root = Native.GetAncestor(hWnd, Native.GA_ROOT);
            string name = FocusGuard.GetProcessNameFor(root);
            if (string.IsNullOrEmpty(name))
            {
                tray.ShowBalloonTip(3000, "Touch Monitor", "No app found under the cursor.", ToolTipIcon.Warning);
                return;
            }
            FocusGuard.AddAllowedProcess(name);
            Settings.Save();
            UpdateAllowedLabel();
            tray.ShowBalloonTip(3000, "Touch Monitor", "Allowed on the touch screen while shielded: " + name, ToolTipIcon.Info);
        }

        private void OnClearAllowed(object sender, EventArgs e)
        {
            FocusGuard.ClearAllowedProcesses();
            Settings.Save();
            UpdateAllowedLabel();
        }

        private void UpdateAllowedLabel()
        {
            System.Collections.Generic.List<string> list = FocusGuard.AllowedProcesses;
            itemAllowedList.Text = list.Count == 0 ? "Allowed apps: (none)" : "Allowed apps: " + string.Join(", ", list.ToArray());
        }

        private void OnToggleStartup(object sender, EventArgs e)
        {
            bool installed = TaskHelper.IsInstalled();
            bool ok = installed ? TaskHelper.Uninstall() : TaskHelper.Install();
            if (!ok)
            {
                tray.ShowBalloonTip(4000, "Touch Monitor", "Could not update the startup task. Run install-autostart.ps1 as administrator.", ToolTipIcon.Warning);
                itemStartup.Checked = TaskHelper.IsInstalled();
                return;
            }
            itemStartup.Checked = !installed;
            tray.ShowBalloonTip(3000, "Touch Monitor", itemStartup.Checked ? "Will start with Windows." : "Autostart disabled.", ToolTipIcon.Info);
        }

        private void OnHotkey(object sender, EventArgs e)
        {
            OnToggleEnabled(sender, e);
        }

        private void OnTick(object sender, EventArgs e)
        {
            FocusGuard.Tick();
            HookEngine.Maintenance();
            if (!startupCheckDone && unchecked(Environment.TickCount - startTick) > 2000)
            {
                startupCheckDone = true;
                itemStartup.Checked = TaskHelper.IsInstalled();
            }
            if (DebugLog.Enabled) LogDebug();
        }

        private void LogDebug()
        {
            long all = HookEngine.AllEvents;
            long swallowed = HookEngine.TotalSwallowed;
            if (all == lastLogAll && swallowed == lastLogSwallowed) return;

            int now = Environment.TickCount;
            bool swallowedChanged = swallowed != lastLogSwallowed;
            if (!swallowedChanged && unchecked(now - lastLogTick) < 1000) return;

            lastLogTick = now;
            lastLogAll = all;
            lastLogSwallowed = swallowed;
            DebugLog.Write("events=" + all + " swallowed=" + swallowed + " touch=" + HookEngine.TouchCount + " pen=" + HookEngine.PenCount + " lastExtra=" + DebugLog.Hex(HookEngine.LastExtraSeen));
        }

        private void OnExit(object sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                FocusGuard.Stop();
                if (messages != null)
                {
                    Native.UnregisterHotKey(messages.Handle, HotkeyId);
                    Native.UnregisterHotKey(messages.Handle, ShieldHotkeyId);
                    messages.DestroyHandle();
                }
                if (tray != null)
                {
                    tray.Visible = false;
                    tray.Dispose();
                }
                if (menu != null) menu.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            foreach (string argument in args)
            {
                if (string.Equals(argument, "--debug", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLog.Enabled = true;
                }
            }

            bool createdNew;
            using (Mutex mutex = new Mutex(true, "TouchMonitor_SingleInstance", out createdNew))
            {
                if (!createdNew) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
                {
                    DebugLog.Write("UI exception: " + e.Exception);
                };
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
                {
                    DebugLog.Write("fatal exception: " + e.ExceptionObject);
                };

                Settings.Load();

                bool hookOk = HookEngine.Start();
                DebugLog.Write("started. hookInstalled=" + hookOk + " error=" + HookEngine.LastError + " elevated=" + IsElevated());

                if (!hookOk)
                {
                    if (!DebugLog.Enabled)
                    {
                        MessageBox.Show("Could not install the global mouse hook." + Environment.NewLine + Environment.NewLine + HookEngine.LastError, "Touch Monitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }

                using (TrayContext context = new TrayContext())
                {
                    Application.Run(context);
                }

                HookEngine.Stop();
                DebugLog.Write("stopped");
                GC.KeepAlive(mutex);
            }
        }

        private static bool IsElevated()
        {
            try
            {
                System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
