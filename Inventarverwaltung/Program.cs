using System;

namespace Inventarverwaltung
{
    /// <summary>
    /// Hauptprogramm der Inventarverwaltung
    /// ERWEITERT: Neue Funktionen für Bestandsverwaltung + Dashboard
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Animierter Ladebildschirm mit Dateninitialisierung
            LoadingScreen.Show();

            // Benutzeranmeldung
            AuthManager.Anmeldung();

            // Willkommensnachricht anzeigen
            ConsoleHelper.PrintWelcome();

            // Hauptschleife: Menü anzeigen und Benutzeraktionen verarbeiten
            bool running = true;
            while (running)
            {
                MenuManager.ShowMenu();
                string auswahl = Console.ReadLine()?.Trim();

                switch (auswahl)
                {
                    // ═══════════════════════════════════════════════════
                    // DASHBOARD (NEU!)
                    // ═══════════════════════════════════════════════════
                    case "99":
                        DashboardManager.ZeigeDashboard();
                        break;

                    // ═══════════════════════════════════════════════════
                    // INVENTARVERWALTUNG
                    // ═══════════════════════════════════════════════════
                    case "1":
                        InventoryManager.NeuenArtikelErstellen();
                        break;

                    case "4":
                        InventoryManager.ZeigeInventar();
                        break;

                    case "14":
                        InventoryManager.ZeigeArtikelDetails();
                        break;

                    // ═══════════════════════════════════════════════════
                    // BESTANDSVERWALTUNG
                    // ═══════════════════════════════════════════════════
                    case "11":
                        InventoryManager.BestandErhoehen();
                        break;

                    case "12":
                        InventoryManager.BestandVerringern();
                        break;

                    case "13":
                        InventoryManager.MindestbestandAendern();
                        break;

                    case "15":
                        InventoryManager.ZeigeArtikelUnterMindestbestand();
                        break;

                    // ═══════════════════════════════════════════════════
                    // MITARBEITERVERWALTUNG
                    // ═══════════════════════════════════════════════════
                    case "2":
                        EmployeeManager.NeuenMitarbeiterHinzufuegen();
                        break;

                    case "3":
                        EmployeeManager.ZeigeMitarbeiter();
                        break;

                    // ═══════════════════════════════════════════════════
                    // BENUTZERVERWALTUNG
                    // ═══════════════════════════════════════════════════
                    case "5":
                        UserManager.NeuerBenutzer();
                        break;

                    case "6":
                        UserManager.ZeigeBenutzer();
                        break;

                    // ═══════════════════════════════════════════════════
                    // SYSTEMFUNKTIONEN
                    // ═══════════════════════════════════════════════════
                    case "7":
                        LogManager.ZeigeLogDatei();
                        break;

                    case "8":
                        LogManager.ErstelleTagesReport();
                        ConsoleHelper.PressKeyToContinue();
                        break;

                    case "9":
                        EncryptionManager.ZeigeVerschluesselungsInfo();
                        break;
                    // ═══════════════════════════════════════════════════
                    // Extrafunktioen
                    // ═══════════════════════════════════════════════════

                    case "16":
                        CSVImportManager.ZeigeImportMenu();
                        break;
                    case "17":  // Bearbeiten
                        Editmanager.ZeigeBearbeitungsMenu();
                        break;

                    case "18":  // Löschen
                        DeleteManager.ZeigeLöschMenu();
                        break;

                    // ═══════════════════════════════════════════════════
                    // PROGRAMM BEENDEN
                    // ═══════════════════════════════════════════════════
                    case "0":
                        running = false;
                        break;

                    // ═══════════════════════════════════════════════════
                    // UNGÜLTIGE EINGABE
                    // ═══════════════════════════════════════════════════
                    default:
                        Console.Clear();
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  💡 Verfügbare Optionen:");
                        Console.WriteLine("     • Dashboard: 99");
                        Console.WriteLine("     • Inventarverwaltung: 1, 4, 14");
                        Console.WriteLine("     • Bestandsverwaltung: 11, 12, 13, 15");
                        Console.WriteLine("     • Mitarbeiter: 2, 3");
                        Console.WriteLine("     • Benutzer: 5, 6");
                        Console.WriteLine("     • System: 7, 8, 9");
                        Console.WriteLine("     • Extra: 16, 17, 18");
                        Console.WriteLine("     • Beenden: 0");
                        Console.ResetColor();
                        ConsoleHelper.PressKeyToContinue();
                        break;
                }
            }

            // Programmende protokollieren
            LogManager.LogProgrammEnde();

            // Abschiedsnachricht
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║     ✓ VIELEN DANK FÜR DIE NUTZUNG!                               ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║     📦 Inventarverwaltung mit Bestandsführung                     ║");
            Console.WriteLine("  ║     🤖 KI-gestützt & 🔐 AES-256 verschlüsselt                     ║");
            Console.WriteLine("  ║     📊 Jetzt mit Dashboard-Funktion!                              ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.ResetColor();

            System.Threading.Thread.Sleep(2000);
        }
    }
}