using System;

namespace Inventarverwaltung
{
    /// <summary>
    /// Hilfsfunktionen für die Konsolen-Ausgabe.
    /// SetupConsole() wird nach ExitFullscreen() aufgerufen —
    /// Größe muss zur WindowManager-Konfiguration passen (160×50).
    /// </summary>
    public static class ConsoleHelper
    {
        // Arbeits-Fensterbreite — muss mit WindowManager.ExitFullscreen() übereinstimmen
        public const int WorkWidth = 160;
        public const int WorkHeight = 50;

        // ── Konsole einrichten ────────────────────────────────────
        public static void SetupConsole()
        {
            Console.Title = "INVENTARVERWALTUNG  ·  KI Edition 2.0";

            try
            {
                // Puffer ohne vertikale Scrollbar (exakt = Fenstergröße)
                Console.SetBufferSize(WorkWidth, WorkHeight);
                Console.SetWindowSize(WorkWidth, WorkHeight);
            }
            catch
            {
                // Auf Terminals die keine feste Größe unterstützen → still ignorieren
            }

            Console.CursorVisible = true;
        }

        // ── Hauptüberschrift ──────────────────────────────────────
        public static void PrintHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║      INVENTARVERWALTUNG SYSTEM             ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        // ── Willkommensnachricht ──────────────────────────────────
        public static void PrintWelcome()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║      INVENTARVERWALTUNG SYSTEM             ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine("\n  Willkommen! System wurde erfolgreich gestartet.\n");
            System.Threading.Thread.Sleep(1500);
        }

        // ── Menüpunkt ─────────────────────────────────────────────
        public static void PrintMenuItem(string key, string text)
        {
            Console.WriteLine($"  [{key}]  {text}");
        }

        // ── Erfolg (grün) ─────────────────────────────────────────
        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✓ {message}");
            Console.ResetColor();
        }

        // ── Fehler (rot) ──────────────────────────────────────────
        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ {message}");
            Console.ResetColor();
        }

        // ── Warnung (gelb) ────────────────────────────────────────
        public static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n⚠ {message}");
            Console.ResetColor();
        }

        // ── Info (cyan) ───────────────────────────────────────────
        public static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nℹ {message}");
            Console.ResetColor();
        }

        // ── Abschnitt-Überschrift ─────────────────────────────────
        public static void PrintSectionHeader(string title, ConsoleColor color = ConsoleColor.Blue)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("\n╔" + new string('═', title.Length + 4) + "╗");
            Console.WriteLine($"║  {title}  ║");
            Console.WriteLine("╚" + new string('═', title.Length + 4) + "╝");
            Console.ResetColor();
        }

        // ── Eingabe ───────────────────────────────────────────────
        public static string GetInput(string prompt)
        {
            Console.Write($"\n▶ {prompt}: ");
            return Console.ReadLine()?.Trim();
        }

        // ── Taste drücken ─────────────────────────────────────────
        public static void PressKeyToContinue()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n[Drücken Sie eine beliebige Taste zum Fortfahren...]");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        // ── Trennlinie ────────────────────────────────────────────
        public static void PrintSeparator()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('─', WorkWidth - 2));
            Console.ResetColor();
        }

        // ── Tabellenkopf ──────────────────────────────────────────
        public static void PrintTableHeader(params string[] headers)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            string header = "  ";
            foreach (var h in headers)
                header += $"{h,-20} ";
            Console.WriteLine(header);
            Console.WriteLine("  " + new string('─', WorkWidth - 4));
            Console.ResetColor();
        }
    }
}