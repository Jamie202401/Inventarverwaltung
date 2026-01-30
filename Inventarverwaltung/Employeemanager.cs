using System;
using System.Linq;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Mitarbeiter-Operationen
    /// </summary>
    public static class EmployeeManager
    {
        /// <summary>
        /// Fügt einen neuen Mitarbeiter hinzu mit intelligenter Fehlerbehandlung
        /// </summary>
        public static void NeuenMitarbeiterHinzufuegen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Neuen Mitarbeiter hinzufügen", ConsoleColor.Magenta);

            // Vorname eingeben (mit Wiederholung bei Fehler)
            string vName;
            while (true)
            {
                vName = ConsoleHelper.GetInput("Vorname des Mitarbeiters");

                if (string.IsNullOrWhiteSpace(vName))
                {
                    ConsoleHelper.PrintError("Vorname darf nicht leer sein!");
                    continue;
                }

                if (vName.Length < 2)
                {
                    ConsoleHelper.PrintError("Vorname muss mindestens 2 Zeichen lang sein!");
                    continue;
                }

                break; // Eingabe ist gültig
            }

            // Nachname eingeben (mit Wiederholung bei Fehler)
            string nName;
            while (true)
            {
                nName = ConsoleHelper.GetInput("Nachname des Mitarbeiters");

                if (string.IsNullOrWhiteSpace(nName))
                {
                    ConsoleHelper.PrintError("Nachname darf nicht leer sein!");
                    continue;
                }

                if (nName.Length < 2)
                {
                    ConsoleHelper.PrintError("Nachname muss mindestens 2 Zeichen lang sein!");
                    continue;
                }

                // Prüfen ob Mitarbeiter bereits existiert
                bool existiert = DataManager.Mitarbeiter.Exists(m =>
                    m.VName.Equals(vName, StringComparison.OrdinalIgnoreCase) &&
                    m.NName.Equals(nName, StringComparison.OrdinalIgnoreCase));

                if (existiert)
                {
                    ConsoleHelper.PrintError($"Ein Mitarbeiter mit dem Namen '{vName} {nName}' existiert bereits!");
                    ConsoleHelper.PrintWarning("Bitte geben Sie einen anderen Nachnamen ein.");
                    continue;
                }

                break; // Eingabe ist gültig
            }

            // Abteilung eingeben (mit Wiederholung bei Fehler)
            string abteilung;
            while (true)
            {
                ConsoleHelper.PrintInfo("Beispiele: IT, Vertrieb, Buchhaltung, Verwaltung");
                abteilung = ConsoleHelper.GetInput("Abteilung des Mitarbeiters");

                if (string.IsNullOrWhiteSpace(abteilung))
                {
                    ConsoleHelper.PrintError("Abteilung darf nicht leer sein!");
                    continue;
                }

                break; // Eingabe ist gültig
            }

            // Mitarbeiter erstellen und speichern
            MID neuerMitarbeiter = new MID(vName, nName, abteilung);
            DataManager.Mitarbeiter.Add(neuerMitarbeiter);
            DataManager.SaveMitarbeiterToFile();

            // Erfolgsmeldung
            ConsoleHelper.PrintSuccess($"Mitarbeiter '{vName} {nName}' aus der Abteilung '{abteilung}' wurde erfolgreich hinzugefügt!");

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt alle Mitarbeiter in einer übersichtlichen Tabelle
        /// </summary>
        public static void ZeigeMitarbeiter()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Mitarbeiter-Übersicht", ConsoleColor.Blue);

            if (DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Mitarbeiter vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            ConsoleHelper.PrintTableHeader("Nr", "Vorname", "Nachname", "Abteilung");

            for (int i = 0; i < DataManager.Mitarbeiter.Count; i++)
            {
                MID m = DataManager.Mitarbeiter[i];
                Console.WriteLine($"  {i + 1,-4} {m.VName,-20} {m.NName,-20} {m.Abteilung,-20}");
            }

            Console.WriteLine();
            ConsoleHelper.PrintInfo($"Gesamt: {DataManager.Mitarbeiter.Count} Mitarbeiter");
            ConsoleHelper.PressKeyToContinue();
        }
    }
}