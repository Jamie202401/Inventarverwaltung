using System;
using System.Runtime.InteropServices;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet echten Vollbildmodus für die Konsole.
    /// Entfernt Titelleiste, Rahmen, und alle Fensterkontrollen.
    /// Nur unter Windows verfügbar.
    /// </summary>
    public static class FullscreenManager
    {
        // ═══════════════════════════════════════════════════════════
        // WIN32 IMPORTS
        // ═══════════════════════════════════════════════════════════

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Konstanten
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;

        private const int WS_CAPTION = 0x00C00000;   // Titelleiste
        private const int WS_THICKFRAME = 0x00040000;   // Größenänderungsrahmen
        private const int WS_MINIMIZE = 0x20000000;   // Minimieren-Button
        private const int WS_MAXIMIZE = 0x01000000;   // Maximieren-Button
        private const int WS_SYSMENU = 0x00080000;   // Systemmenü (X-Button)

        private const int WS_EX_DLGMODALFRAME = 0x00000001;
        private const int WS_EX_TOPMOST = 0x00000008;

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private const int SW_MAXIMIZE = 3;

        // ═══════════════════════════════════════════════════════════
        // ÖFFENTLICHE API
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Aktiviert echten Vollbildmodus:
        /// - Entfernt Titelleiste
        /// - Entfernt Rahmen
        /// - Entfernt Minimize/Maximize/Close Buttons
        /// - Füllt gesamten Bildschirm
        /// </summary>
        public static void AktiviereVollbild()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            try
            {
                IntPtr hwnd = GetConsoleWindow();
                if (hwnd == IntPtr.Zero) return;

                // Hole aktuellen Style
                int style = GetWindowLong(hwnd, GWL_STYLE);
                int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

                // Entferne alle Fensterelemente
                style &= ~WS_CAPTION;      // Titelleiste weg
                style &= ~WS_THICKFRAME;   // Rahmen weg
                style &= ~WS_MINIMIZE;     // Minimieren-Button weg
                style &= ~WS_MAXIMIZE;     // Maximieren-Button weg
                style &= ~WS_SYSMENU;      // X-Button weg

                // Setze neuen Style
                SetWindowLong(hwnd, GWL_STYLE, style);

                // Optional: Als Topmost setzen (immer im Vordergrund)
                // exStyle |= WS_EX_TOPMOST;
                // SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);

                // Hole Bildschirmgröße
                int screenW = GetSystemMetrics(SM_CXSCREEN);
                int screenH = GetSystemMetrics(SM_CYSCREEN);

                // Positioniere Fenster auf (0,0) und mache es bildschirmfüllend
                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, screenW, screenH, SWP_FRAMECHANGED);

                // Konsolenpuffer anpassen
                SyncBufferToWindow();
            }
            catch
            {
                // Silent fail
            }
        }

        /// <summary>
        /// Stellt normales Fenster wieder her (mit Titelleiste und Buttons).
        /// Rufe dies auf wenn das Programm beendet wird.
        /// </summary>
        public static void DeaktiviereVollbild()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            try
            {
                IntPtr hwnd = GetConsoleWindow();
                if (hwnd == IntPtr.Zero) return;

                // Standard-Style wiederherstellen
                int style = GetWindowLong(hwnd, GWL_STYLE);
                style |= WS_CAPTION;
                style |= WS_THICKFRAME;
                style |= WS_MINIMIZE;
                style |= WS_MAXIMIZE;
                style |= WS_SYSMENU;

                SetWindowLong(hwnd, GWL_STYLE, style);

                // Fenster zentrieren und auf normale Größe setzen
                SetWindowPos(hwnd, IntPtr.Zero, 100, 100, 1200, 800, SWP_FRAMECHANGED);
            }
            catch
            {
                // Silent fail
            }
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
            catch
            {
                // Silent fail
            }
        }
    }
}