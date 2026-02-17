using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// Steuert das Konsolenfenster: Vollbild beim Ladebildschirm,
    /// anschließend sauberes Normal-Fenster mit optimaler Größe.
    /// Funktioniert nur unter Windows — auf anderen Plattformen
    /// werden alle Aufrufe still ignoriert.
    /// </summary>
    internal static class WindowManager
    {
        // ═══════════════════════════════════════════════════════════
        // WIN32 IMPORTS
        // ═══════════════════════════════════════════════════════════

        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // Font-API direkt eingebettet (kein Aufruf auf ConsoleFont nötig)
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetCurrentConsoleFontEx(
            IntPtr hConsoleOutput, bool bMaximumWindow,
            ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetCurrentConsoleFontEx(
            IntPtr hConsoleOutput, bool bMaximumWindow,
            ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CONSOLE_FONT_INFO_EX
        {
            public uint cbSize;
            public uint nFont;
            public COORD dwFontSize;
            public int FontFamily;
            public int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD { public short X; public short Y; }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        private const int STD_OUTPUT_HANDLE = -11;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        // ═══════════════════════════════════════════════════════════
        // ÖFFENTLICHE API
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Maximiert das Konsolenfenster auf Vollbild.
        /// Schriftgröße 14 für maximale Zeichenanzahl im Ladebildschirm.
        /// </summary>
        public static void EnterFullscreen()
        {
            if (!IsWindows()) return;

            try
            {
                IntPtr hwnd = GetConsoleWindow();
                if (hwnd == IntPtr.Zero) return;

                SetFont("Consolas", 14);
                Thread.Sleep(80);

                ShowWindow(hwnd, SW_MAXIMIZE);
                Thread.Sleep(120);

                SyncBufferToWindow();
            }
            catch { }
        }

        /// <summary>
        /// Stellt das normale Arbeitsfenster wieder her:
        /// 160 × 50 Zeichen, Schriftgröße 16, zentriert auf dem Bildschirm.
        /// </summary>
        public static void ExitFullscreen()
        {
            if (!IsWindows()) return;

            try
            {
                IntPtr hwnd = GetConsoleWindow();
                if (hwnd == IntPtr.Zero) return;

                SetFont("Consolas", 16);
                Thread.Sleep(80);

                ShowWindow(hwnd, SW_RESTORE);
                Thread.Sleep(80);

                const int cols = 160;
                const int rows = 50;

                try
                {
                    Console.SetBufferSize(cols, rows + 500);
                    Console.SetWindowSize(cols, rows);
                    Console.SetBufferSize(cols, rows);
                }
                catch { }

                CenterWindow(hwnd);
                Thread.Sleep(100);
                Console.Clear();
            }
            catch { }
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELFER
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Setzt Schriftart und -größe des Konsolenfensters direkt
        /// via Win32 — ohne Abhängigkeit zu ConsoleFont.cs.
        /// </summary>
        private static void SetFont(string faceName, short sizeY)
        {
            try
            {
                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd == IntPtr.Zero) return;

                CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
                info.cbSize = (uint)Marshal.SizeOf(info);

                if (!GetCurrentConsoleFontEx(hnd, false, ref info)) return;

                info.FaceName = faceName;
                info.dwFontSize = new COORD { X = 0, Y = sizeY };
                info.FontFamily = 54;   // TrueType
                info.FontWeight = 400;  // Normal

                SetCurrentConsoleFontEx(hnd, false, ref info);
            }
            catch { }
        }

        private static void SyncBufferToWindow()
        {
            try
            {
                int w = Console.LargestWindowWidth;
                int h = Console.LargestWindowHeight;

                if (w > 1 && h > 1)
                {
                    Console.SetBufferSize(w, h);
                    Console.SetWindowSize(w, h);
                }
            }
            catch { }
        }

        private static void CenterWindow(IntPtr hwnd)
        {
            try
            {
                int screenW = GetSystemMetrics(SM_CXSCREEN);
                int screenH = GetSystemMetrics(SM_CYSCREEN);

                if (!GetWindowRect(hwnd, out RECT r)) return;

                int winW = r.Right - r.Left;
                int winH = r.Bottom - r.Top;

                int x = Math.Max(0, (screenW - winW) / 2);
                int y = Math.Max(0, (screenH - winH) / 2);

                SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0,
                    SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            catch { }
        }

        private static bool IsWindows()
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}