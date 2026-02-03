using System;

namespace Inventarverwaltung
{
    /// <summary>
    /// Hauptprogramm der Inventarverwaltung
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Log-System initialisieren
            LogManager.InitializeLog();
            LogManager.LogProgrammStart();

            // Dateien vorbereiten und verstecken
            FileManager.HideAllFiles();

            // Benutzer anmelden
            AuthManager.Anmeldung();

            // Konsole einrichten
            ConsoleHelper.SetupConsole();

            // Alle gespeicherten Daten laden
            DataManager.LoadBenutzer();
            DataManager.LoadMitarbeiter();
            DataManager.LoadInventar();

            // KI-Assistent initialisieren (lernt aus vorhandenen Daten)
            IntelligentAssistant.IniializeAI();

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
                    case "1":
                        InventoryManager.NeuenArtikelErstellen();
                        break;
                    case "2":
                        EmployeeManager.NeuenMitarbeiterHinzufuegen();
                        break;
                    case "3":
                        EmployeeManager.ZeigeMitarbeiter();
                        break;
                    case "4":
                        InventoryManager.ZeigeInventar();
                        break;
                    case "5":
                        UserManager.NeuerBenutzer();
                        break;
                    case "6":
                        UserManager.ZeigeBenutzer();
                        break;
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
                    case "0":
                        running = false;
                        break;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl! Bitte wählen Sie eine Zahl von 0-9.");
                        ConsoleHelper.PressKeyToContinue();
                        break;
                }
            }

            // Programmende protokollieren
            LogManager.LogProgrammEnde();

            // Abschiedsnachricht
            Console.Clear();
            ConsoleHelper.PrintSuccess("\n✓ Danke für die Nutzung des Inventarverwaltungssystems!\n");
            System.Threading.Thread.Sleep(1500);
        }
    }
}