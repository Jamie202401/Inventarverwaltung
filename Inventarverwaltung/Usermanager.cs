using System;
using System.Linq;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Benutzer-Operationen mit KI-Unterstützung
    /// </summary>
    public static class UserManager
    {
        /// <summary>
        /// Erstellt einen neuen Benutzer ODER aktualisiert Berechtigung wenn bereits vorhanden
        /// </summary>
        public static void NeuerBenutzer()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Neuen Benutzer anlegen", ConsoleColor.DarkMagenta);

            // Benutzername eingeben
            string benutzerName;
            Accounts existierenderBenutzer = null;

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
                existierenderBenutzer = DataManager.Benutzer.FirstOrDefault(b =>
                    b.Benutzername.Equals(benutzerName, StringComparison.OrdinalIgnoreCase));

                if (existierenderBenutzer != null)
                {
                    // Benutzer existiert bereits!
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
                    Console.WriteLine("  ║                                                                   ║");
                    Console.WriteLine("  ║     ⚠️  BENUTZER EXISTIERT BEREITS                                ║");
                    Console.WriteLine("  ║                                                                   ║");
                    Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
                    Console.ResetColor();
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"  👤 Benutzername: {existierenderBenutzer.Benutzername}");

                    string rollenIcon = existierenderBenutzer.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";
                    Console.WriteLine($"  🔑 Aktuelle Berechtigung: {rollenIcon} {existierenderBenutzer.Berechtigung}");
                    Console.ResetColor();
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("  💡 Möchten Sie die Berechtigung ändern?");
                    Console.ResetColor();

                    string aendern = ConsoleHelper.GetInput("Berechtigung ändern? (j/n)");

                    if (aendern.ToLower() == "j" || aendern.ToLower() == "ja")
                    {
                        // Springe zur Berechtigungs-Auswahl
                        break;
                    }
                    else
                    {
                        ConsoleHelper.PrintInfo("Vorgang abgebrochen.");
                        LogManager.LogBenutzerDuplikat(benutzerName);
                        ConsoleHelper.PressKeyToContinue();
                        return;
                    }
                }

                break;
            }

            // Berechtigung wählen (mit KI-Empfehlung)
            Berechtigungen berechtigung;
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  Verfügbare Berechtigungen:");
                Console.ResetColor();
                Console.WriteLine("  [1] 👤 User - Kann nur Daten ansehen und hinzufügen");
                Console.WriteLine("  [2] 👑 Admin - Hat volle Rechte (Löschen, Ändern, etc.)");

                // KI: Empfehlung basierend auf Kontext
                string aiEmpfehlung = IntelligentAssistant.SchlageBerechtigungVor(benutzerName);
                if (!string.IsNullOrWhiteSpace(aiEmpfehlung))
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   🤖 {aiEmpfehlung}");
                    Console.ResetColor();
                }

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

            // Benutzer erstellen ODER aktualisieren
            if (existierenderBenutzer != null)
            {
                // AKTUALISIERE bestehenden Benutzer
                Berechtigungen alteBerechtigung = existierenderBenutzer.Berechtigung;
                existierenderBenutzer.Berechtigung = berechtigung;

                // Speichere alle Benutzer komplett neu
                DataManager.SaveBenutzerToFile();

                // Erfolgsmeldung
                string rollenIcon = berechtigung == Berechtigungen.Admin ? "👑" : "👤";
                Console.WriteLine();
                ConsoleHelper.PrintSuccess($"Berechtigung von '{benutzerName}' wurde aktualisiert!");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  Alt: {alteBerechtigung}");
                Console.WriteLine($"  Neu: {rollenIcon} {berechtigung}");
                Console.ResetColor();

                // Logging
                LogManager.LogBenutzerAktualisiert(benutzerName, alteBerechtigung.ToString(), berechtigung.ToString());
            }
            else
            {
                // ERSTELLE neuen Benutzer
                Accounts neuerBenutzer = new Accounts(benutzerName, berechtigung);
                DataManager.Benutzer.Add(neuerBenutzer);
                DataManager.SaveBenutzerToFile();

                // Erfolgsmeldung mit Icon
                string rollenIcon = berechtigung == Berechtigungen.Admin ? "👑" : "👤";
                Console.WriteLine();
                ConsoleHelper.PrintSuccess($"Benutzer '{benutzerName}' wurde als {rollenIcon} '{berechtigung}' angelegt!");

                // Logging
                LogManager.LogBenutzerAngelegt(benutzerName, berechtigung);
            }

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt alle Benutzer in einer übersichtlichen Tabelle mit Spaltenüberschriften
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

            // Spaltenüberschriften exakt über den Daten
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {"Nr",-4} {"Benutzername",-20} {"Berechtigung",-20} {"Rolle"}");
            Console.WriteLine($"  {new string('─', 4)} {new string('─', 20)} {new string('─', 20)} {new string('─', 10)}");
            Console.ResetColor();

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