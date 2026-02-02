using System;
using System.Linq;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet die Benutzeranmeldung
    /// </summary>
    public static class AuthManager
    {
        /// <summary>
        /// Meldet einen Benutzer am System an oder erstellt ein neues Konto
        /// </summary>
        public static void Anmeldung()
        {
            DataManager.LoadAnmeldung();

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║          🔐 BENUTZERANMELDUNG              ║");
            Console.WriteLine("╚════════════════════════════════════════════╝\n");
            Console.ResetColor();

            // Benutzername eingeben (mit Wiederholung)
            string anmeldename;
            while (true)
            {
                anmeldename = ConsoleHelper.GetInput("Benutzername");

                if (string.IsNullOrWhiteSpace(anmeldename))
                {
                    ConsoleHelper.PrintError("Benutzername darf nicht leer sein!");
                    LogManager.LogAnmeldungFehlgeschlagen("Leerer Benutzername");
                    continue;
                }

                if (anmeldename.Length < 3)
                {
                    ConsoleHelper.PrintError("Benutzername muss mindestens 3 Zeichen lang sein!");
                    LogManager.LogAnmeldungFehlgeschlagen($"Benutzername zu kurz: {anmeldename}");
                    continue;
                }

                break; // Eingabe ist gültig
            }

            // Prüfen ob Benutzer existiert
            Anmelder existierenderBenutzer = DataManager.Anmeldung.FirstOrDefault(a =>
                a.Anmeldename.Equals(anmeldename, StringComparison.OrdinalIgnoreCase));

            if (existierenderBenutzer != null)
            {
                // Benutzer gefunden - erfolgreich angemeldet
                ConsoleHelper.PrintSuccess($"Willkommen zurück, {anmeldename}! 👋");
                LogManager.LogAnmeldungErfolgreich(anmeldename);
            }
            else
            {
                // Neuen Benutzer erstellen
                ConsoleHelper.PrintWarning($"Benutzer '{anmeldename}' wurde nicht gefunden.");

                while (true)
                {
                    Console.WriteLine("\n  Möchten Sie ein neues Konto erstellen?");
                    Console.WriteLine("  [1] Ja, neues Konto erstellen");
                    Console.WriteLine("  [0] Nein, Anmeldung abbrechen");

                    string eingabe = ConsoleHelper.GetInput("Ihre Wahl");

                    if (eingabe == "1")
                    {
                        Anmelder neuerBenutzer = new Anmelder(anmeldename);
                        DataManager.Anmeldung.Add(neuerBenutzer);
                        DataManager.SaveIntoNewAccounts();

                        ConsoleHelper.PrintSuccess($"Konto für '{anmeldename}' wurde erfolgreich erstellt! 🎉");
                        LogManager.LogNeuesKontoErstellt(anmeldename);
                        break;
                    }
                    else if (eingabe == "0")
                    {
                        ConsoleHelper.PrintWarning("Anmeldung abgebrochen.");
                        LogManager.LogAnmeldungFehlgeschlagen("Benutzer hat Anmeldung abgebrochen");
                        System.Threading.Thread.Sleep(1000);
                        Environment.Exit(0);
                        break;
                    }
                    else
                    {
                        ConsoleHelper.PrintError("Ungültige Eingabe! Bitte 1 oder 0 wählen.");
                    }
                }
            }

            Console.WriteLine();
            System.Threading.Thread.Sleep(1500);
        }
    }
}