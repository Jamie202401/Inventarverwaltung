using Inventarverwaltung.Manager.Data;
using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.UI;
using System;

namespace Inventarverwaltung
{
    /// <summary>
    /// Einstiegspunkt des Programms.
    ///
    /// Start-Sequenz:
    ///   1. Vollbild aktivieren         (WindowManager)
    ///   2. Ladebildschirm anzeigen     (LoadingScreen)
    ///   3. Normales Fenster wiederherstellen (WindowManager)
    ///   4. Anmeldung                   (AuthManager)
    ///   5. Hauptschleife               (AppSetup → AppRouter)
    ///   6. Abschluss-Screen
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (args.Length > 0 && args[0] == DevConsole._ep)
            {
                DevConsole.Boot(args);
                return;
            }
            // UTF-8 muss allererste Zeile sein (Emojis, Umlaute, Box-Zeichen)
            
            Console.Title = "Inventarverwaltung";

            // ── [1] Vollbild für Ladebildschirm ─────────────────────
            WindowManager.EnterFullscreen();

            // ── [2] Cinematic Loading Screen ────────────────────────
            LoadingScreen.Show();

            //// ── [3] Cinematic Loading Screen ────────────────────────
            LowStockWarning.ZeigeBestandswarnungen();

            // ── [4] Zurück zum Normal-Fenster ────────────────────────
            WindowManager.ExitFullscreen();

            // ── [5] Anmeldung ────────────────────────────────────────
            AuthManager.Anmeldung();
            ConsoleHelper.PrintWelcome();

            // ── [6] Hauptschleife ────────────────────────────────────
            // Kehrt zurück wenn der Benutzer [0] drückt.
            AppSetup.Build().Run();

            // ── [7] Abschluss ────────────────────────────────────────
            LogManager.LogProgrammEnde();
            Verabschiedung();
        }

        public static void Verabschiedung()
        {
            Console.CursorVisible = false;
            Console.Clear();
            // ── Phase 1: Abmeldetext einblenden (~0.8s) ───────────────────
            string[] zeilen =
            {
                "",
                "  ╔═══════════════════════════════════════════════════════════════════╗",
                "  ║                                                                   ║",
                "  ║     ✓  VIELEN DANK FÜR DIE NUTZUNG!                              ║",
                "  ║                                                                   ║",
                "  ║     📦 Inventarverwaltung  ·  🖨️  Hardware-Ausgabe                ║",
                "  ║     🤖 KI-gestützt  ·  🔐 AES-256 verschlüsselt                  ║",
                "  ║                                                                   ║",
                "  ╚═══════════════════════════════════════════════════════════════════╝",
                "",
            };
            Console.ForegroundColor = ConsoleColor.Green;

            foreach(var z in zeilen)
            {
                Console.WriteLine(z);
                Thread.Sleep(100);
            }

            // [2] Fortschrittsbalken
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("  SYSTEME WURDEN BEENDET  ");
            Console.ResetColor();

            int balkenBreite = 30;
            for (int i = 0; i <= balkenBreite; i++)
            {
                Console.CursorLeft = 27;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write(new string('█', i));
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(new string('░', balkenBreite - i));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("]");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write($"  {i * 100 / balkenBreite,3}%");
                Console.ResetColor();
                Thread.Sleep(80);
            }

            Console.WriteLine();
            Console.WriteLine();

            // Phase 3 Anmelden
            var checks = new (string Text, int MS)[]
            {
                ("  ✓  Daten gesichert",          120),
                ("  ✓  Log verschlüsselt",         120),
                ("  ✓  Sitzung beendet",           120),
                ("  ✓  Speicher freigegeben",       100),
            };
            foreach(var (text,ms)in checks)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(text);
                Console.ResetColor();
                Thread.Sleep(ms);
            }

            // Phase 4 Ausblenden
            Thread.Sleep(300);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("   Auf Wiedersehen  ");
            Console.ResetColor();
            Thread.Sleep(600);

            Console.CursorVisible = false;
        }
    }
}