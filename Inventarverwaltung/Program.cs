using System;
using Inventarverwaltung.Manager.UI;
using Inventarverwaltung.Manager.Auth;

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
            // UTF-8 muss allererste Zeile sein (Emojis, Umlaute, Box-Zeichen)
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Inventarverwaltung";

            // ── [1] Vollbild für Ladebildschirm ─────────────────────
            WindowManager.EnterFullscreen();

            // ── [2] Cinematic Loading Screen ────────────────────────
            LoadingScreen.Show();

            // ── [3] Zurück zum Normal-Fenster ────────────────────────
            WindowManager.ExitFullscreen();

            // ── [4] Anmeldung ────────────────────────────────────────
            AuthManager.Anmeldung();
            ConsoleHelper.PrintWelcome();

            // ── [5] Hauptschleife ────────────────────────────────────
            // Kehrt zurück wenn der Benutzer [0] drückt.
            AppSetup.Build().Run();

            // ── [6] Abschluss ────────────────────────────────────────
            LogManager.LogProgrammEnde();
            Verabschiedung();
        }

        private static void Verabschiedung()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║     ✓  VIELEN DANK FÜR DIE NUTZUNG!                              ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║     📦 Inventarverwaltung  ·  🖨️  Hardware-Ausgabe                ║");
            Console.WriteLine("  ║     🤖 KI-gestützt  ·  🔐 AES-256 verschlüsselt                   ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.ResetColor();
            System.Threading.Thread.Sleep(2000);
        }
    }
}