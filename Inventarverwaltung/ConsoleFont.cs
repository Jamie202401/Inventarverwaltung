using System;
using System.Runtime.InteropServices;


namespace Inventarverwaltung
{
    internal static class ConsoleFont
    {
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
        private struct COORD
        {
            public short X;
            public short Y;
        }

        private const int STD_OUTPUT_HANDLE = -11;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

        public static void TrySetConsoleFont(string faceName = "Consolas", short sizeY = 18)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Nicht‑Windows: nicht unterstützt
                return;
            }

            try
            {
                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
                info.cbSize = (uint)Marshal.SizeOf(info);

                if (!GetCurrentConsoleFontEx(hnd, false, ref info))
                {
                    return;
                }

                info.FaceName = faceName;
                info.dwFontSize = new COORD { X = 0, Y = sizeY };
                info.FontFamily = 54; // typischer Wert für TrueType
                info.FontWeight = 400;

                SetCurrentConsoleFontEx(hnd, false, ref info);
            }
            catch
            {
                // Fehler still ignorieren — Konsolen können unterschiedliche Einschränkungen haben
            }
        }
    }
}