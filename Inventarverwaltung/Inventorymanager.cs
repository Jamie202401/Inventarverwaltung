using System;
using System.Linq;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Inventar-Operationen
    /// </summary>
    public static class InventoryManager
    {
        /// <summary>
        /// Erstellt einen neuen Inventarartikel mit intelligenter Fehlerbehandlung
        /// </summary>
        public static void NeuenArtikelErstellen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Neues Gerät hinzufügen", ConsoleColor.DarkGreen);

            // Inventarnummer eingeben (mit Wiederholung bei Fehler)
            string invNmr;
            while (true)
            {
                invNmr = ConsoleHelper.GetInput("Inventar-Nummer (z.B. INV001)");

                if (string.IsNullOrWhiteSpace(invNmr))
                {
                    ConsoleHelper.PrintError("Inventar-Nummer darf nicht leer sein!");
                    continue;
                }

                // Prüfen ob bereits vorhanden
                if (DataManager.Inventar.Exists(i => i.InvNmr.Equals(invNmr, StringComparison.OrdinalIgnoreCase)))
                {
                    ConsoleHelper.PrintError($"Die Inventar-Nummer '{invNmr}' existiert bereits!");
                    LogManager.LogArtikelDuplikat(invNmr, "[noch nicht eingegeben]");
                    continue;
                }

                break; // Eingabe ist gültig
            }

            // Gerätename eingeben (mit Wiederholung bei Fehler)
            string geraeteName;
            while (true)
            {
                geraeteName = ConsoleHelper.GetInput("Gerätename (z.B. Laptop Dell XPS)");

                if (string.IsNullOrWhiteSpace(geraeteName))
                {
                    ConsoleHelper.PrintError("Gerätename darf nicht leer sein!");
                    continue;
                }

                // Prüfen ob bereits vorhanden
                if (DataManager.Inventar.Exists(i => i.GeraeteName.Equals(geraeteName, StringComparison.OrdinalIgnoreCase)))
                {
                    ConsoleHelper.PrintError($"Ein Gerät mit dem Namen '{geraeteName}' existiert bereits!");
                    LogManager.LogArtikelDuplikat("[bereits vergeben]", geraeteName);
                    continue;
                }

                break; // Eingabe ist gültig
            }

            // Mitarbeiter zuweisen (mit Wiederholung bei Fehler)
            string mitarbeiterBezeichnung;
            while (true)
            {
                ConsoleHelper.PrintInfo("Verfügbare Mitarbeiter:");
                ZeigeMitarbeiterListe();

                mitarbeiterBezeichnung = ConsoleHelper.GetInput("Mitarbeiter (Vorname Nachname)");

                if (string.IsNullOrWhiteSpace(mitarbeiterBezeichnung))
                {
                    ConsoleHelper.PrintError("Mitarbeitername darf nicht leer sein!");
                    continue;
                }

                // Prüfen ob Mitarbeiter existiert
                bool existiert = DataManager.Mitarbeiter.Any(m =>
                    $"{m.VName} {m.NName}".Equals(mitarbeiterBezeichnung, StringComparison.OrdinalIgnoreCase)
                );

                if (!existiert)
                {
                    ConsoleHelper.PrintError($"Der Mitarbeiter '{mitarbeiterBezeichnung}' existiert nicht!");
                    ConsoleHelper.PrintWarning("Bitte wählen Sie einen existierenden Mitarbeiter oder legen Sie zuerst einen neuen an.");
                    continue;
                }

                break; // Eingabe ist gültig
            }

            // Artikel erstellen und speichern
            InvId neuerArtikel = new InvId(invNmr, geraeteName, mitarbeiterBezeichnung);
            DataManager.Inventar.Add(neuerArtikel);
            DataManager.SaveInvToFile();

            // Erfolgsmeldung
            ConsoleHelper.PrintSuccess($"Gerät '{geraeteName}' (ID: {invNmr}) wurde erfolgreich dem Mitarbeiter '{mitarbeiterBezeichnung}' zugeordnet!");

            // Logging
            LogManager.LogArtikelHinzugefuegt(invNmr, geraeteName, mitarbeiterBezeichnung);

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt eine kompakte Liste aller Mitarbeiter für die Auswahl
        /// </summary>
        private static void ZeigeMitarbeiterListe()
        {
            if (DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Mitarbeiter vorhanden!");
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var m in DataManager.Mitarbeiter)
            {
                Console.WriteLine($"   • {m.VName} {m.NName} ({m.Abteilung})");
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Zeigt das komplette Inventar in einer übersichtlichen Tabelle
        /// </summary>
        public static void ZeigeInventar()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Inventar-Übersicht", ConsoleColor.Blue);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Artikel im Inventar vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            ConsoleHelper.PrintTableHeader("Nr", "Inventar-Nr", "Gerätename", "Mitarbeiter");

            for (int i = 0; i < DataManager.Inventar.Count; i++)
            {
                InvId artikel = DataManager.Inventar[i];
                Console.WriteLine($"  {i + 1,-4} {artikel.InvNmr,-20} {artikel.GeraeteName,-20} {artikel.MitarbeiterBezeichnung,-20}");
            }

            Console.WriteLine();
            ConsoleHelper.PrintInfo($"Gesamt: {DataManager.Inventar.Count} Artikel im Inventar");

            // Logging
            LogManager.LogInventarAngezeigt(DataManager.Inventar.Count);

            ConsoleHelper.PressKeyToContinue();
        }
    }
}