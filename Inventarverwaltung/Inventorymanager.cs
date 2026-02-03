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
        /// Erstellt einen neuen Inventarartikel mit intelligenter KI-Unterstützung
        /// </summary>
        public static void NeuenArtikelErstellen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Neues Gerät hinzufügen", ConsoleColor.DarkGreen);

            // KI: System-Insights anzeigen
            IntelligentAssistant.ZeigeSystemInsights();

            // Inventarnummer eingeben (mit KI-Vorschlag)
            string invNmr;
            while (true)
            {
                // KI: Intelligenter Vorschlag für nächste Nummer
                string vorschlag = IntelligentAssistant.SchlageInventarnummernVor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n   🤖 KI-Vorschlag: {vorschlag}");
                Console.ResetColor();

                invNmr = ConsoleHelper.GetInput("Inventar-Nummer (Enter für Vorschlag)");

                // Wenn leer, Vorschlag übernehmen
                if (string.IsNullOrWhiteSpace(invNmr))
                {
                    invNmr = vorschlag;
                    ConsoleHelper.PrintSuccess($"✓ Vorschlag übernommen: {invNmr}");
                }

                // Prüfen ob bereits vorhanden
                if (DataManager.Inventar.Exists(i => i.InvNmr.Equals(invNmr, StringComparison.OrdinalIgnoreCase)))
                {
                    ConsoleHelper.PrintError($"Die Inventar-Nummer '{invNmr}' existiert bereits!");
                    LogManager.LogArtikelDuplikat(invNmr, "[noch nicht eingegeben]");
                    continue;
                }

                break;
            }

            // Gerätename eingeben (mit KI-Analyse)
            string geraeteName;
            while (true)
            {
                geraeteName = ConsoleHelper.GetInput("Gerätename (z.B. Laptop Dell XPS)");

                if (string.IsNullOrWhiteSpace(geraeteName))
                {
                    ConsoleHelper.PrintError("Gerätename darf nicht leer sein!");
                    continue;
                }

                // KI: Analysiere Gerät
                string analyse = IntelligentAssistant.Analysieregeraete(geraeteName);
                if (!string.IsNullOrWhiteSpace(analyse))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n   🤖 KI-Analyse:");
                    Console.WriteLine(analyse);
                    Console.ResetColor();
                }

                // Prüfen ob bereits vorhanden
                if (DataManager.Inventar.Exists(i => i.GeraeteName.Equals(geraeteName, StringComparison.OrdinalIgnoreCase)))
                {
                    ConsoleHelper.PrintError($"Ein Gerät mit dem Namen '{geraeteName}' existiert bereits!");
                    LogManager.LogArtikelDuplikat("[bereits vergeben]", geraeteName);
                    continue;
                }

                // KI: Prüfe auf ähnliche Namen (Tippfehler?)
                var vorhandeneGeraete = DataManager.Inventar.Select(i => i.GeraeteName).ToList();
                var aehnliche = IntelligentAssistant.FindePotentielleDuplikate(geraeteName, vorhandeneGeraete);

                if (aehnliche.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n   ⚠️ KI-Warnung: Ähnliche Geräte gefunden:");
                    foreach (var item in aehnliche)
                    {
                        Console.WriteLine($"      • {item}");
                    }
                    Console.ResetColor();

                    string bestaetigung = ConsoleHelper.GetInput("Trotzdem fortfahren? (j/n)");
                    if (bestaetigung.ToLower() != "j" && bestaetigung.ToLower() != "ja")
                    {
                        continue;
                    }
                }

                break;
            }

            // Mitarbeiter zuweisen (mit KI-Vorschlägen)
            string mitarbeiterBezeichnung;
            while (true)
            {
                // KI: Intelligente Mitarbeiter-Vorschläge
                var aiVorschlaege = IntelligentAssistant.SchlageMitarbeiterVor(geraeteName, 3);

                if (aiVorschlaege.Count > 0)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("   🤖 KI empfiehlt folgende Mitarbeiter:");
                    Console.ResetColor();
                    for (int i = 0; i < aiVorschlaege.Count; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"      [{i + 1}] {aiVorschlaege[i]}");
                        Console.ResetColor();
                    }
                    Console.WriteLine();
                }

                ConsoleHelper.PrintInfo("Alle verfügbaren Mitarbeiter:");
                ZeigeMitarbeiterListe();

                mitarbeiterBezeichnung = ConsoleHelper.GetInput("Mitarbeiter (Vorname Nachname oder Nummer)");

                if (string.IsNullOrWhiteSpace(mitarbeiterBezeichnung))
                {
                    ConsoleHelper.PrintError("Mitarbeitername darf nicht leer sein!");
                    continue;
                }

                // Prüfe ob Nummer eingegeben wurde
                if (int.TryParse(mitarbeiterBezeichnung, out int nummer) && nummer > 0 && nummer <= aiVorschlaege.Count)
                {
                    mitarbeiterBezeichnung = aiVorschlaege[nummer - 1];
                    ConsoleHelper.PrintSuccess($"✓ KI-Vorschlag übernommen: {mitarbeiterBezeichnung}");
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

                break;
            }

            // Artikel erstellen und speichern
            InvId neuerArtikel = new InvId(invNmr, geraeteName, mitarbeiterBezeichnung);
            DataManager.Inventar.Add(neuerArtikel);
            DataManager.SaveInvToFile();

            // KI neu initialisieren (lernt aus neuem Eintrag)
            IntelligentAssistant.IniializeAI();

            // Erfolgsmeldung
            Console.WriteLine();
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