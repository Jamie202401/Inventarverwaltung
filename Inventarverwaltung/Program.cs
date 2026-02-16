using System;

namespace Inventarverwaltung
{
    /// <summary>
    /// Einstiegspunkt des Programms.
    /// Nur noch 5 Zeilen Logik — der Rest steckt in:
    ///   AppSetup.cs  → Menükonfiguration
    ///   Core/        → Router, UI, Interface
    ///   Commands/    → Einzelne Aktionen
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            LoadingScreen.Show();          // Ladebildschirm + alle Daten laden
            AuthManager.Anmeldung();       // Benutzeranmeldung
            ConsoleHelper.PrintWelcome();  // Willkommensbildschirm

            // Router aufbauen und Hauptschleife starten.
            // Kehrt zurück wenn der Benutzer [0] drückt.
            AppSetup.Build().Run();

            // Abschluss
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