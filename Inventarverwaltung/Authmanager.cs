using System;
using System.Linq;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet die Benutzeranmeldung über Accounts.txt
    /// </summary>
    public static class AuthManager
    {
        public static string AktuellerBenutzer { get; private set; }

        /// <summary>
        /// Führt die Benutzeranmeldung durch - prüft gegen Accounts.txt
        /// </summary>
        public static void Anmeldung()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                           ║");
            Console.WriteLine("  ║           INVENTARVERWALTUNG - ANMELDUNG                  ║");
            Console.WriteLine("  ║                                                           ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.ResetColor();

            // Lade Benutzer aus Accounts.txt
            DataManager.LoadBenutzer();

            // Wenn keine Benutzer existieren, erstelle Standard-Admin
            if (DataManager.Benutzer.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ⚠️  Noch keine Benutzer-Accounts vorhanden!");
                Console.WriteLine();
                Console.WriteLine("  Erstelle Standard-Admin-Account...");
                Console.ResetColor();

                // Erstelle Admin-Account
                Accounts adminAccount = new Accounts("admin", Berechtigungen.Admin);
                DataManager.Benutzer.Add(adminAccount);
                DataManager.SaveBenutzerToFile();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine("  ✓ Admin-Account erstellt!");
                Console.WriteLine("  → Benutzername: admin");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1500);
                Console.WriteLine();
            }

            // Anmeldeschleife
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  👤 Benutzername: ");
                Console.ResetColor();
                string benutzername = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(benutzername))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ✗ Benutzername darf nicht leer sein!");
                    Console.ResetColor();
                    Console.WriteLine();
                    continue;
                }

                // Prüfe ob Benutzer in Accounts.txt existiert
                var benutzer = DataManager.Benutzer.FirstOrDefault(b =>
                    b.Benutzername.Equals(benutzername, StringComparison.OrdinalIgnoreCase));

                if (benutzer == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ Benutzer '{benutzername}' existiert nicht!");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  💡 Tipp: Lassen Sie einen Admin einen Account für Sie anlegen");
                    Console.WriteLine("  💡 Oder verwenden Sie 'admin' für den Standard-Account");
                    Console.ResetColor();
                    Console.WriteLine();
                    continue;
                }

                // Erfolgreiche Anmeldung
                AktuellerBenutzer = benutzer.Benutzername;

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  ✓ Anmeldung erfolgreich!");
                Console.WriteLine($"  → Willkommen, {AktuellerBenutzer}");

                string rollenIcon = benutzer.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";
                Console.WriteLine($"  → Berechtigung: {rollenIcon} {benutzer.Berechtigung}");
                Console.ResetColor();

                // Logging
                LogManager.LogAnmeldungErfolgreich(AktuellerBenutzer);

                System.Threading.Thread.Sleep(1500);
                break;
            }
        }
    }
}