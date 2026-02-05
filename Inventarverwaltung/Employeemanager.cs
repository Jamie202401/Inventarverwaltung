using System;
using System.Linq;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Mitarbeiter-Operationen mit KI-Unterstützung
    /// </summary>
    public static class EmployeeManager
    {
        /// <summary>
        /// Fügt einen neuen Mitarbeiter hinzu mit intelligenter KI-Unterstützung
        /// </summary>
        public static void NeuenMitarbeiterHinzufuegen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Neuen Mitarbeiter hinzufügen", ConsoleColor.Magenta);

            // KI: Zeige Abteilungsverteilung
            string verteilung = IntelligentAssistant.AnalysiereAbteilungsverteilung();
            if (!string.IsNullOrWhiteSpace(verteilung))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n   🤖 KI-Analyse:");
                Console.WriteLine(verteilung);
                Console.ResetColor();
            }

            // Vorname eingeben
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

                break;
            }

            // Nachname eingeben (mit KI-Prüfung)
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

                // KI: Prüfe Plausibilität und ähnliche Namen
                string feedback = IntelligentAssistant.PruefeNamePlausibilitaet(vName, nName);
                if (!string.IsNullOrWhiteSpace(feedback))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n   🤖 KI-Feedback:");
                    Console.WriteLine(feedback);
                    Console.ResetColor();

                    string weiter = ConsoleHelper.GetInput("Trotzdem fortfahren? (j/n)");
                    if (weiter.ToLower() != "j" && weiter.ToLower() != "ja")
                    {
                        continue;
                    }
                }

                // Prüfen ob Mitarbeiter bereits existiert
                bool existiert = DataManager.Mitarbeiter.Exists(m =>
                    m.VName.Equals(vName, StringComparison.OrdinalIgnoreCase) &&
                    m.NName.Equals(nName, StringComparison.OrdinalIgnoreCase));

                if (existiert)
                {
                    ConsoleHelper.PrintError($"Ein Mitarbeiter mit dem Namen '{vName} {nName}' existiert bereits!");
                    ConsoleHelper.PrintWarning("Bitte geben Sie einen anderen Nachnamen ein.");
                    LogManager.LogMitarbeiterDuplikat(vName, nName);
                    continue;
                }

                break;
            }

            // Abteilung eingeben (mit KI-Vorschlägen)
            string abteilung;
            while (true)
            {
                // KI: Zeige häufigste Abteilungen
                var vorschlaege = IntelligentAssistant.SchlageAbteilungenVor();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   🤖 KI schlägt folgende Abteilungen vor:");
                Console.ResetColor();

                for (int i = 0; i < vorschlaege.Count && i < 5; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"      [{i + 1}] {vorschlaege[i]}");
                    Console.ResetColor();
                }
                Console.WriteLine();

                abteilung = ConsoleHelper.GetInput("Abteilung (oder Nummer für Vorschlag)");

                if (string.IsNullOrWhiteSpace(abteilung))
                {
                    ConsoleHelper.PrintError("Abteilung darf nicht leer sein!");
                    continue;
                }

                // Prüfe ob Nummer eingegeben wurde
                if (int.TryParse(abteilung, out int nummer) && nummer > 0 && nummer <= vorschlaege.Count)
                {
                    abteilung = vorschlaege[nummer - 1];
                    ConsoleHelper.PrintSuccess($"✓ KI-Vorschlag übernommen: {abteilung}");
                }

                break;
            }

            // Mitarbeiter erstellen und speichern
            MID neuerMitarbeiter = new MID(vName, nName, abteilung);
            DataManager.Mitarbeiter.Add(neuerMitarbeiter);
            DataManager.SaveMitarbeiterToFile();

            // KI neu initialisieren
            IntelligentAssistant.IniializeAI();

            // Erfolgsmeldung
            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"Mitarbeiter '{vName} {nName}' aus der Abteilung '{abteilung}' wurde erfolgreich hinzugefügt!");

            // Logging
            LogManager.LogMitarbeiterHinzugefuegt(vName, nName, abteilung);

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt alle Mitarbeiter in einer übersichtlichen Tabelle mit Spaltenüberschriften
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

            // Spaltenüberschriften exakt über den Daten
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {"Nr",-4} {"Vorname",-20} {"Nachname",-20} {"Abteilung",-20}");
            Console.WriteLine($"  {new string('─', 4)} {new string('─', 20)} {new string('─', 20)} {new string('─', 20)}");
            Console.ResetColor();

            for (int i = 0; i < DataManager.Mitarbeiter.Count; i++)
            {
                MID m = DataManager.Mitarbeiter[i];
                Console.WriteLine($"  {i + 1,-4} {m.VName,-20} {m.NName,-20} {m.Abteilung,-20}");
            }

            Console.WriteLine();
            ConsoleHelper.PrintInfo($"Gesamt: {DataManager.Mitarbeiter.Count} Mitarbeiter");

            // Logging
            LogManager.LogMitarbeiterAngezeigt(DataManager.Mitarbeiter.Count);

            ConsoleHelper.PressKeyToContinue();
        }
    }
}