using System;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Benutzer-Operationen
    /// </summary>
    public static class UserManager
    {
        /// <summary>
        /// Erstellt einen neuen Benutzer mit Berechtigungen
        /// </summary>
        public static void NeuerBenutzer()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Neuen Benutzer anlegen", ConsoleColor.DarkMagenta);

            // Benutzername eingeben (mit Wiederholung bei Fehler)
            string benutzerName;
            while (true)
            {
                benutzerName = ConsoleHelper.GetInput("Benutzername");

                if (string.IsNullOrWhiteSpace(benutzerName))
                {
                    ConsoleHelper.PrintError("Benutzername darf nicht leer sein!");
                    continue;
                }

                if (benutzerName.Length < 3)
                {
                    ConsoleHelper.PrintError("Benutzername muss mindestens 3 Zeichen lang sein!");
                    continue;
                }

                // Prüfen ob Benutzer bereits existiert
                bool existiert = DataManager.Benutzer.Exists(b =>
                    b.Benutzername.Equals(benutzerName, StringComparison.OrdinalIgnoreCase));

                if (existiert)
                {
                    ConsoleHelper.PrintError($"Ein Benutzer mit dem Namen '{benutzerName}' existiert bereits!");
                    LogManager.LogBenutzerDuplikat(benutzerName);
                    continue;
                }

                break; // Eingabe ist gültig
            }

            // Berechtigung wählen (mit Wiederholung bei Fehler)
            Berechtigungen berechtigung;
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  Verfügbare Berechtigungen:");
                Console.ResetColor();
                Console.WriteLine("  [1] 👤 User - Kann nur Daten ansehen und hinzufügen");
                Console.WriteLine("  [2] 👑 Admin - Hat volle Rechte (Löschen, Ändern, etc.)");

                string eingabe = ConsoleHelper.GetInput("\nBerechtigungsstufe wählen (1 oder 2)");

                if (eingabe == "1")
                {
                    berechtigung = Berechtigungen.User;
                    break;
                }
                else if (eingabe == "2")
                {
                    berechtigung = Berechtigungen.Admin;
                    break;
                }
                else
                {
                    ConsoleHelper.PrintError("Ungültige Auswahl! Bitte nur 1 oder 2 eingeben.");
                }
            }

            // Benutzer erstellen und speichern
            Accounts neuerBenutzer = new Accounts(benutzerName, berechtigung);
            DataManager.Benutzer.Add(neuerBenutzer);
            DataManager.SaveBenutzerToFile();

            // Erfolgsmeldung mit Icon
            string rollenIcon = berechtigung == Berechtigungen.Admin ? "👑" : "👤";
            ConsoleHelper.PrintSuccess($"Benutzer '{benutzerName}' wurde als {rollenIcon} '{berechtigung}' angelegt!");

            // Logging
            LogManager.LogBenutzerAngelegt(benutzerName, berechtigung);

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt alle Benutzer in einer übersichtlichen Tabelle
        /// </summary>
        public static void ZeigeBenutzer()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Benutzer-Übersicht", ConsoleColor.Blue);

            if (DataManager.Benutzer.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Benutzer vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            ConsoleHelper.PrintTableHeader("Nr", "Benutzername", "Berechtigung", "Rolle");

            for (int i = 0; i < DataManager.Benutzer.Count; i++)
            {
                Accounts a = DataManager.Benutzer[i];
                string icon = a.Berechtigung == Berechtigungen.Admin ? "👑 Admin" : "👤 User";
                Console.WriteLine($"  {i + 1,-4} {a.Benutzername,-20} {a.Berechtigung,-20} {icon}");
            }

            Console.WriteLine();
            ConsoleHelper.PrintInfo($"Gesamt: {DataManager.Benutzer.Count} Benutzer");

            // Logging
            LogManager.LogBenutzerAngezeigt(DataManager.Benutzer.Count);

            ConsoleHelper.PressKeyToContinue();
        }
    }
}