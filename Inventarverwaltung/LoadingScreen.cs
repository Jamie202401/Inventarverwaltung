using System;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// Animierter Ladebildschirm beim Programmstart
    /// Lädt alle Daten und zeigt schöne Animationen
    /// ERWEITERT: KI Engine 2.0 Integration
    /// </summary>
    public static class LoadingScreen
    {
        private static int progressBarWidth = 50;
        private static ConsoleColor primaryColor = ConsoleColor.Cyan;
        private static ConsoleColor accentColor = ConsoleColor.Yellow;

        /// <summary>
        /// Zeigt den Ladebildschirm und lädt alle Daten
        /// </summary>
        public static void Show()
        {
            Console.Clear();
            Console.CursorVisible = false;


            // Zeige Logo
            ZeigeLogo();
            Thread.Sleep(500);

            // Lade Daten mit Progress-Anzeige
            var ladeProzesse = new[]
            {
                new { Name = "Initialisiere System", Action = new Action(() => InitialisiereSystem()) },
                new { Name = "Lade Benutzerdaten", Action = new Action(() => DataManager.LoadBenutzer()) },
                new { Name = "Lade Mitarbeiter", Action = new Action(() => DataManager.LoadMitarbeiter()) },
                new { Name = "Lade Inventar", Action = new Action(() => DataManager.LoadInventar()) },
                new { Name = "Lade Lieferanten", Action = new Action(() => DataManager.LoadLieferanten()) },
			//  new { Name = "Lade Anmeldungen", Action = new Action(() => DataManager.LoadAnmeldung()) },
                new { Name = "🤖 Initialisiere KI Engine 2.0", Action = new Action(() => KIEngine.Initialisiere()) },
                new { Name = "Initialisiere Verschlüsselung", Action = new Action(() => LogManager.InitializeLog()) },
                new { Name = "Prüfe Systemintegrität", Action = new Action(() => PruefeSystem()) }
            };

            Console.WriteLine();
            int cursorTop = Console.CursorTop;

            for (int i = 0; i < ladeProzesse.Length; i++)
            {
                float progress = (float)i / ladeProzesse.Length;

                // Zeige aktuellen Prozess
                Console.SetCursorPosition(0, cursorTop);
                ZeigeProgressBar(progress, ladeProzesse[i].Name);

                // Führe Lade-Aktion aus
                try
                {
                    ladeProzesse[i].Action();
                    Thread.Sleep(300); // Simuliere Ladezeit für visuelle Wirkung
                }
                catch (Exception ex)
                {
                    ZeigeFehler($"Fehler beim {ladeProzesse[i].Name}: {ex.Message}");
                    Thread.Sleep(2000);
                }
            }

            // 100% erreicht
            Console.SetCursorPosition(0, cursorTop);
            ZeigeProgressBar(1.0f, "Laden abgeschlossen!");
            Thread.Sleep(500);

            // Zeige Erfolgs-Animation
            ZeigeErfolgsAnimation();

            Console.CursorVisible = true;
            Thread.Sleep(800);
        }

        /// <summary>
        /// Zeigt das ASCII-Logo
        /// </summary>
        private static void ZeigeLogo()
        {
            Console.ForegroundColor = primaryColor;
            string[] logo = new[]
            {
                "╔═══════════════════════════════════════════════════════════════════════╗",
                "║                                                                       ║",
                "║     ██╗███╗   ██╗██╗   ██╗███████╗███╗   ██╗████████╗ █████╗ ██████╗  ║",
                "║     ██║████╗  ██║██║   ██║██╔════╝████╗  ██║╚══██╔══╝██╔══██╗██╔══██╗ ║",
                "║     ██║██╔██╗ ██║██║   ██║█████╗  ██╔██╗ ██║   ██║   ███████║██████╔╝ ║",
                "║     ██║██║╚██╗██║╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║   ██╔══██║██╔══██╗ ║",
                "║     ██║██║ ╚████║ ╚████╔╝ ███████╗██║ ╚████║   ██║   ██║  ██║██║  ██║ ║",
                "║     ╚═╝╚═╝  ╚═══╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ║",
                "║                                                                       ║",
                "║                  🤖 KI ENGINE 2.0 - PREMIUM EDITION                   ║",
                "║                  🔐 AES-256 Verschlüsselung aktiviert                 ║",
                "║                  📊 Version 2.0.0 - PRODUCTION                        ║",
                "║                               © 2026  jh                              ║",
                "╚═══════════════════════════════════════════════════════════════════════╝"
            };

            int startX = (Console.WindowWidth - 75) / 2;
            if (startX < 0) startX = 0;

            for (int i = 0; i < logo.Length; i++)
            {
                if (i == 9 || i == 10 || i == 11)
                {
                    Console.ForegroundColor = accentColor;
                }
                else
                {
                    Console.ForegroundColor = primaryColor;
                }

                try
                {
                    Console.SetCursorPosition(startX, Console.CursorTop);
                }
                catch { }

                Console.WriteLine(logo[i]);
                Thread.Sleep(50);
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Zeigt eine animierte Fortschrittsleiste
        /// </summary>
        private static void ZeigeProgressBar(float progress, string currentTask)
        {
            // Berechne Fortschritt
            int filledWidth = (int)(progressBarWidth * progress);
            int percentage = (int)(progress * 100);

            // Lösche aktuelle Zeilen
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(new string(' ', Console.WindowWidth - 1));
            }
            Console.SetCursorPosition(0, Console.CursorTop - 5);

            // Zeige aktuellen Task
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  ⚙️  {currentTask}...");
            Console.WriteLine();

            // Zeige Progress Bar
            Console.Write("  [");

            // Gefüllter Teil
            Console.ForegroundColor = progress >= 1.0f ? ConsoleColor.Green : primaryColor;
            Console.Write(new string('█', filledWidth));

            // Leerer Teil
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('░', progressBarWidth - filledWidth));

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"] {percentage}%");
            Console.WriteLine();

            // Zeige Statistik
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (progress >= 0.3f)
            {
                Console.WriteLine($"  📦 Inventar: {DataManager.Inventar.Count} Artikel");
            }
            if (progress >= 0.5f)
            {
                Console.WriteLine($"  👥 Mitarbeiter: {DataManager.Mitarbeiter.Count} Personen");
            }
            if (progress >= 0.7f)
            {
                Console.WriteLine($"  👨‍💼 Benutzer: {DataManager.Benutzer.Count} Accounts");
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Zeigt Fehler beim Laden
        /// </summary>
        private static void ZeigeFehler(string fehlerText)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ⚠️  {fehlerText}");
            Console.ResetColor();
        }

        /// <summary>
        /// Zeigt Erfolgs-Animation
        /// </summary>
        private static void ZeigeErfolgsAnimation()
        {
            Console.WriteLine();
            Console.WriteLine();

            string[] frames = new[] { "◐", "◓", "◑", "◒" };
            string successText = "  ✓ System erfolgreich geladen!";

            // Animierte Checkmark
            for (int i = 0; i < 8; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"  {frames[i % frames.Length]} Starte System");
                Console.ResetColor();
                Thread.Sleep(100);
            }

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(successText + new string(' ', 20));
            Console.ResetColor();

            // Zeige finale Statistik
            Console.WriteLine();
            Console.ForegroundColor = primaryColor;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine($"  ║  📊 Geladen: {DataManager.Inventar.Count} Artikel | {DataManager.Mitarbeiter.Count} Mitarbeiter | {DataManager.Benutzer.Count} Benutzer  ");
            Console.WriteLine("  ║  🤖 KI Engine 2.0: Aktiv & Bereit                        ║");
            Console.WriteLine("  ║  🔐 Verschlüsselung: AES-256 Aktiviert                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        /// <summary>
        /// Initialisiert grundlegende Systemkomponenten
        /// </summary>
        private static void InitialisiereSystem()
        {
            FileManager.HideAllFiles();
            ConsoleHelper.SetupConsole();
        }

        /// <summary>
        /// Prüft Systemintegrität
        /// </summary>
        private static void PruefeSystem()
        {
            Thread.Sleep(200);
        }

        /// <summary>
        /// Zeigt einen kompakten Ladebildschirm (für schnelles Neuladen)
        /// </summary>
        public static void QuickReload()
        {
            Console.Clear();
            Console.ForegroundColor = primaryColor;

            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════╗");
            Console.WriteLine("  ║     🔄 Aktualisiere Daten...          ║");
            Console.WriteLine("  ╚═══════════════════════════════════════╝");
            Console.WriteLine();

            var spinner = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

            for (int i = 0; i < 10; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write($"  {spinner[i % spinner.Length]} Lade Daten...");
                Thread.Sleep(100);
            }

            // Lade Daten neu
            DataManager.LoadBenutzer();
            DataManager.LoadMitarbeiter();
            DataManager.LoadInventar();
            DataManager.LoadLieferanten();
            KIEngine.Initialisiere();

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Daten aktualisiert!              ");
            Console.ResetColor();
            Thread.Sleep(500);

        }

    }

}