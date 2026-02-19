using Inventarverwaltung.Manager.AI;
using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.Employee;
using Inventarverwaltung.Manager.UI;
using System;
using System.Globalization;
using System.Linq;

namespace Inventarverwaltung.Manager.Inventory
{
    /// <summary>
    /// Verwaltet alle Inventar-Operationen
    /// ERWEITERT: Vollständige Bestandsverwaltung mit allen neuen Features
    /// </summary>
    public static class InventoryManager
    {
        /// <summary>
        /// Erstellt einen neuen Inventarartikel mit ALLEN Feldern und KI-Unterstützung
        /// ERWEITERT: SNR, Preis, Datum, Hersteller, Kategorie, Anzahl, Mindestbestand
        /// </summary>
        public static void NeuenArtikelErstellen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Neues Gerät hinzufügen (Erweitert)", ConsoleColor.DarkGreen);

            // WICHTIG: Prüfe ob überhaupt Mitarbeiter vorhanden sind
            if (DataManager.Mitarbeiter.Count == 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║                                                                   ║");
                Console.WriteLine("  ║     ⚠️  KEINE MITARBEITER VORHANDEN                               ║");
                Console.WriteLine("  ║                                                                   ║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  📋 Sie müssen zuerst mindestens einen Mitarbeiter anlegen,");
                Console.WriteLine("     bevor Sie Inventar-Artikel hinzufügen können.");
                Console.ResetColor();
                Console.WriteLine();

                string antwort = ConsoleHelper.GetInput("Möchten Sie jetzt einen Mitarbeiter anlegen? (j/n)");

                if (antwort.ToLower() == "j" || antwort.ToLower() == "ja")
                {
                    EmployeeManager.NeuenMitarbeiterHinzufuegen();
                    if (DataManager.Mitarbeiter.Count == 0)
                    {
                        ConsoleHelper.PrintWarning("Kein Mitarbeiter angelegt. Artikel kann nicht hinzugefügt werden.");
                        ConsoleHelper.PressKeyToContinue();
                        return;
                    }
                    Console.Clear();
                    ConsoleHelper.PrintSectionHeader("Neues Gerät hinzufügen (Erweitert)", ConsoleColor.DarkGreen);
                }
                else
                {
                    ConsoleHelper.PrintInfo("Vorgang abgebrochen.");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }
            }

            // KI: System-Insights anzeigen
            IntelligentAssistant.ZeigeSystemInsights();

            // ═══════════════════════════════════════════════════════════════
            // 1. INVENTARNUMMER
            // ═══════════════════════════════════════════════════════════════
            string invNmr;
            while (true)
            {
                string vorschlag = IntelligentAssistant.SchlageInventarnummernVor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n   🤖 KI-Vorschlag: {vorschlag}");
                Console.ResetColor();

                invNmr = ConsoleHelper.GetInput("Inventar-Nummer (Enter für Vorschlag)");

                if (string.IsNullOrWhiteSpace(invNmr))
                {
                    invNmr = vorschlag;
                    ConsoleHelper.PrintSuccess($"✓ Vorschlag übernommen: {invNmr}");
                }

                if (DataManager.Inventar.Exists(i => i.InvNmr.Equals(invNmr, StringComparison.OrdinalIgnoreCase)))
                {
                    ConsoleHelper.PrintError($"Die Inventar-Nummer '{invNmr}' existiert bereits!");
                    LogManager.LogArtikelDuplikat(invNmr, "[noch nicht eingegeben]");
                    continue;
                }

                break;
            }

            // ═══════════════════════════════════════════════════════════════
            // 2. GERÄTENAME
            // ═══════════════════════════════════════════════════════════════
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

                // Prüfe auf ähnliche Namen
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

            // ═══════════════════════════════════════════════════════════════
            // 3. SERIENNUMMER (NEU!)
            // ═══════════════════════════════════════════════════════════════
            string serienNummer;
            while (true)
            {
                serienNummer = ConsoleHelper.GetInput("Seriennummer / SNR (oder 'N/A' wenn keine)");

                if (string.IsNullOrWhiteSpace(serienNummer))
                {
                    serienNummer = "N/A";
                    ConsoleHelper.PrintInfo("Keine Seriennummer angegeben → N/A");
                }

                break;
            }

            // ═══════════════════════════════════════════════════════════════
            // 4. PREIS (NEU!)
            // ═══════════════════════════════════════════════════════════════
            decimal preis;
            while (true)
            {
                string preisEingabe = ConsoleHelper.GetInput("Anschaffungspreis in € (z.B. 1299.99)");

                if (string.IsNullOrWhiteSpace(preisEingabe))
                {
                    preis = 0.00m;
                    ConsoleHelper.PrintInfo("Kein Preis angegeben → 0.00€");
                    break;
                }

                if (decimal.TryParse(preisEingabe.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out preis))
                {
                    if (preis < 0)
                    {
                        ConsoleHelper.PrintError("Preis darf nicht negativ sein!");
                        continue;
                    }
                    ConsoleHelper.PrintSuccess($"✓ Preis: {preis:F2}€");
                    break;
                }
                else
                {
                    ConsoleHelper.PrintError("Ungültiges Preisformat! Verwenden Sie z.B. 1299.99");
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 5. ANSCHAFFUNGSDATUM (NEU!)
            // ═══════════════════════════════════════════════════════════════
            DateTime anschaffungsdatum = DateTime.Now;
            while (true)
            {
                string datumEingabe = ConsoleHelper.GetInput("Anschaffungsdatum (TT.MM.JJJJ oder Enter für heute)");

                if (string.IsNullOrWhiteSpace(datumEingabe))
                {
                    anschaffungsdatum = DateTime.Now;
                    ConsoleHelper.PrintSuccess($"✓ Datum: {anschaffungsdatum:dd.MM.yyyy} (heute)");
                    break;
                }

                if (DateTime.TryParseExact(datumEingabe, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out anschaffungsdatum))
                {
                    ConsoleHelper.PrintSuccess($"✓ Datum: {anschaffungsdatum:dd.MM.yyyy}");
                    break;
                }
                else
                {
                    ConsoleHelper.PrintError("Ungültiges Datumsformat! Verwenden Sie TT.MM.JJJJ (z.B. 15.01.2025)");
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 6. HERSTELLER (NEU!)
            // ═══════════════════════════════════════════════════════════════
            string hersteller;
            while (true)
            {
                // Zeige häufige Hersteller
                var haeufigsteHersteller = DataManager.Inventar
                    .Where(a => a.Hersteller != "Unbekannt")
                    .GroupBy(a => a.Hersteller)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList();

                if (haeufigsteHersteller.Any())
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("   🤖 Häufige Hersteller:");
                    Console.ResetColor();
                    for (int i = 0; i < haeufigsteHersteller.Count; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"      [{i + 1}] {haeufigsteHersteller[i]}");
                        Console.ResetColor();
                    }
                    Console.WriteLine();
                }

                hersteller = ConsoleHelper.GetInput("Hersteller (z.B. Dell, HP, Lenovo)");

                if (string.IsNullOrWhiteSpace(hersteller))
                {
                    hersteller = "Unbekannt";
                    ConsoleHelper.PrintInfo("Kein Hersteller angegeben → Unbekannt");
                    break;
                }

                // Prüfe ob Nummer eingegeben wurde
                if (int.TryParse(hersteller, out int nummer) && nummer > 0 && nummer <= haeufigsteHersteller.Count)
                {
                    hersteller = haeufigsteHersteller[nummer - 1];
                    ConsoleHelper.PrintSuccess($"✓ Hersteller: {hersteller}");
                }

                break;
            }

            // ═══════════════════════════════════════════════════════════════
            // 7. KATEGORIE (NEU!)
            // ═══════════════════════════════════════════════════════════════
            string kategorie;
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("   📋 Verfügbare Kategorien:");
                Console.ResetColor();
                Console.WriteLine("      [1] IT-Hardware");
                Console.WriteLine("      [2] Büroausstattung");
                Console.WriteLine("      [3] Werkzeug");
                Console.WriteLine("      [4] Kommunikation");
                Console.WriteLine("      [5] Sonstiges");
                Console.WriteLine();

                string eingabe = ConsoleHelper.GetInput("Kategorie (1-5 oder eigene)");

                if (string.IsNullOrWhiteSpace(eingabe))
                {
                    kategorie = "Sonstiges";
                    ConsoleHelper.PrintInfo("Keine Kategorie → Sonstiges");
                    break;
                }

                switch (eingabe)
                {
                    case "1": kategorie = "IT-Hardware"; break;
                    case "2": kategorie = "Büroausstattung"; break;
                    case "3": kategorie = "Werkzeug"; break;
                    case "4": kategorie = "Kommunikation"; break;
                    case "5": kategorie = "Sonstiges"; break;
                    default: kategorie = eingabe; break;
                }

                ConsoleHelper.PrintSuccess($"✓ Kategorie: {kategorie}");
                break;
            }

            // ═══════════════════════════════════════════════════════════════
            // 8. ANZAHL / MENGE (NEU!)
            // ═══════════════════════════════════════════════════════════════
            int anzahl;
            while (true)
            {
                string anzahlEingabe = ConsoleHelper.GetInput("Anzahl / Menge (Standard: 1)");

                if (string.IsNullOrWhiteSpace(anzahlEingabe))
                {
                    anzahl = 1;
                    ConsoleHelper.PrintInfo("Keine Anzahl → 1 Stück");
                    break;
                }

                if (int.TryParse(anzahlEingabe, out anzahl))
                {
                    if (anzahl < 0)
                    {
                        ConsoleHelper.PrintError("Anzahl darf nicht negativ sein!");
                        continue;
                    }

                    if (anzahl == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("   ⚠️ Warnung: Anzahl = 0 bedeutet 'Nicht auf Lager'");
                        Console.ResetColor();
                    }

                    ConsoleHelper.PrintSuccess($"✓ Anzahl: {anzahl} Stück");
                    break;
                }
                else
                {
                    ConsoleHelper.PrintError("Ungültige Anzahl! Bitte eine Zahl eingeben.");
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 9. MINDESTBESTAND (NEU!)
            // ═══════════════════════════════════════════════════════════════
            int mindestbestand;
            while (true)
            {
                string mindestEingabe = ConsoleHelper.GetInput($"Mindestbestand (Warnschwelle, Standard: {Math.Max(1, anzahl / 4)})");

                if (string.IsNullOrWhiteSpace(mindestEingabe))
                {
                    mindestbestand = Math.Max(1, anzahl / 4);
                    ConsoleHelper.PrintInfo($"Kein Mindestbestand → {mindestbestand} Stück");
                    break;
                }

                if (int.TryParse(mindestEingabe, out mindestbestand))
                {
                    if (mindestbestand < 0)
                    {
                        ConsoleHelper.PrintError("Mindestbestand darf nicht negativ sein!");
                        continue;
                    }

                    if (mindestbestand >= anzahl && anzahl > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"   ⚠️ Warnung: Mindestbestand ({mindestbestand}) >= Aktuelle Anzahl ({anzahl})");
                        Console.WriteLine("   → Artikel wird sofort als 'NIEDRIG' markiert!");
                        Console.ResetColor();
                    }

                    ConsoleHelper.PrintSuccess($"✓ Mindestbestand: {mindestbestand} Stück");
                    break;
                }
                else
                {
                    ConsoleHelper.PrintError("Ungültiger Mindestbestand! Bitte eine Zahl eingeben.");
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 10. RECHNUNGSDATUM (NEU!)
            // ═══════════════════════════════════════════════════════════════
            DateTime rechnungsdatum = anschaffungsdatum;
            while (true)
            {
                string eingabe = ConsoleHelper.GetInput("Rechnungsdatum (TT.MM.JJJJ oder Enter für Anschaffungsdatum)");

                if (string.IsNullOrWhiteSpace(eingabe))
                {
                    rechnungsdatum = anschaffungsdatum;
                    ConsoleHelper.PrintSuccess($"✓ Rechnungsdatum: {rechnungsdatum:dd.MM.yyyy} (= Anschaffungsdatum)");
                    break;
                }

                if (DateTime.TryParseExact(eingabe, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out rechnungsdatum))
                {
                    ConsoleHelper.PrintSuccess($"✓ Rechnungsdatum: {rechnungsdatum:dd.MM.yyyy}");
                    break;
                }
                else
                {
                    ConsoleHelper.PrintError("Ungültiges Datumsformat! Verwenden Sie TT.MM.JJJJ (z.B. 15.01.2025)");
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 11. GARANTIE BIS (NEU!)
            // ═══════════════════════════════════════════════════════════════
            DateTime garantieBis = rechnungsdatum.AddYears(2);
            while (true)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("   💡 Tipp: Garantie läuft oft 2-3 Jahre ab Rechnungsdatum.");
                Console.ResetColor();
                string vorschlagGarantie = rechnungsdatum.AddYears(2).ToString("dd.MM.yyyy");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   🤖 KI-Vorschlag: {vorschlagGarantie} (+2 Jahre ab Rechnungsdatum)");
                Console.ResetColor();

                string eingabe = ConsoleHelper.GetInput("Garantie gültig bis (TT.MM.JJJJ oder Enter für Vorschlag)");

                if (string.IsNullOrWhiteSpace(eingabe))
                {
                    garantieBis = rechnungsdatum.AddYears(2);
                    ConsoleHelper.PrintSuccess($"✓ Garantie bis: {garantieBis:dd.MM.yyyy}");
                    break;
                }

                if (DateTime.TryParseExact(eingabe, "dd.MM.yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out garantieBis))
                {
                    if (garantieBis < rechnungsdatum)
                    {
                        ConsoleHelper.PrintError("Garantieende kann nicht vor dem Rechnungsdatum liegen!");
                        continue;
                    }
                    ConsoleHelper.PrintSuccess($"✓ Garantie bis: {garantieBis:dd.MM.yyyy}");
                    break;
                }
                else
                {
                    ConsoleHelper.PrintError("Ungültiges Datumsformat! Verwenden Sie TT.MM.JJJJ (z.B. 15.01.2027)");
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // 12. MITARBEITER ZUWEISEN
            // ═══════════════════════════════════════════════════════════════
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

            // ═══════════════════════════════════════════════════════════════
            // ARTIKEL ERSTELLEN UND SPEICHERN (MIT TRACKING)
            // ═══════════════════════════════════════════════════════════════
            string aktuellerBenutzer = AuthManager.AktuellerBenutzer ?? "System";
            DateTime erstellZeitpunkt = DateTime.Now;

            InvId neuerArtikel = new InvId(
                invNmr, geraeteName, mitarbeiterBezeichnung,
                serienNummer, preis, anschaffungsdatum,
                hersteller, kategorie, anzahl, mindestbestand,
                aktuellerBenutzer, erstellZeitpunkt,
                rechnungsdatum, garantieBis
            );

            DataManager.Inventar.Add(neuerArtikel);
            DataManager.SaveInvToFile();

            // KI neu initialisieren
            IntelligentAssistant.IniializeAI();

            // ═══════════════════════════════════════════════════════════════
            // ZUSAMMENFASSUNG UND ERFOLGSMELDUNG
            // ═══════════════════════════════════════════════════════════════
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║     ✓ ARTIKEL ERFOLGREICH HINZUGEFÜGT                             ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  📋 Zusammenfassung:");
            Console.ResetColor();
            Console.WriteLine($"     • Inventar-Nr:    {invNmr}");
            Console.WriteLine($"     • Gerät:          {geraeteName}");
            Console.WriteLine($"     • Seriennummer:   {serienNummer}");
            Console.WriteLine($"     • Preis:          {preis:F2}€");
            Console.WriteLine($"     • Datum:          {anschaffungsdatum:dd.MM.yyyy}");
            Console.WriteLine($"     • Rechnungsdatum: {rechnungsdatum:dd.MM.yyyy}");
            Console.WriteLine($"     • Garantie bis:   {garantieBis:dd.MM.yyyy}");
            Console.WriteLine($"     • Hersteller:     {hersteller}");
            Console.WriteLine($"     • Kategorie:      {kategorie}");
            Console.WriteLine($"     • Anzahl:         {anzahl} Stück");
            Console.WriteLine($"     • Mindestbestand: {mindestbestand} Stück");
            Console.WriteLine($"     • Mitarbeiter:    {mitarbeiterBezeichnung}");

            // Bestandsstatus anzeigen
            string statusText = neuerArtikel.GetBestandsStatusText();
            Console.WriteLine($"     • Status:         {statusText}");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📝 Tracking-Informationen:");
            Console.ResetColor();
            Console.WriteLine($"     • Erstellt von:   {aktuellerBenutzer}");
            Console.WriteLine($"     • Erstellt am:    {erstellZeitpunkt:dd.MM.yyyy HH:mm:ss}");

            Console.WriteLine();

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
        /// ERWEITERT: Mit Bestandsstatus und Farbcodierung
        /// </summary>
        public static void ZeigeInventar()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Inventar-Übersicht (Erweitert)", ConsoleColor.Blue);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Artikel im Inventar vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();

            // Spaltenüberschriften
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {"Nr",-4} {"Inv-Nr",-10} {"Gerät",-20} {"Anzahl",-8} {"Status",-15} {"Preis",-12} {"Mitarbeiter",-20}");
            Console.WriteLine($"  {new string('─', 4)} {new string('─', 10)} {new string('─', 20)} {new string('─', 8)} {new string('─', 15)} {new string('─', 12)} {new string('─', 20)}");
            Console.ResetColor();

            for (int i = 0; i < DataManager.Inventar.Count; i++)
            {
                InvId artikel = DataManager.Inventar[i];

                // Zeile mit Farbe basierend auf Bestandsstatus
                var status = artikel.GetBestandsStatus();
                ConsoleColor zeilenFarbe = status switch
                {
                    BestandsStatus.Leer => ConsoleColor.Red,
                    BestandsStatus.Niedrig => ConsoleColor.Yellow,
                    _ => ConsoleColor.White
                };

                Console.ForegroundColor = zeilenFarbe;

                string anzahlText = $"{artikel.Anzahl}/{artikel.Mindestbestand}";
                string statusText = artikel.GetBestandsStatusText();
                string preisText = $"{artikel.Preis:F2}€";

                Console.WriteLine($"  {i + 1,-4} {artikel.InvNmr,-10} {artikel.GeraeteName,-20} {anzahlText,-8} {statusText,-15} {preisText,-12} {artikel.MitarbeiterBezeichnung,-20}");
                Console.ResetColor();
            }

            Console.WriteLine();

            // Statistiken
            (int gesamt, int leer, int niedrig, int ok) stats = DataManager.GetBestandsStatistik();
            ConsoleHelper.PrintInfo($"Gesamt: {stats.gesamt} Artikel");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  🔴 Leer: {stats.leer}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  🟡 Niedrig: {stats.niedrig}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  🟢 OK: {stats.ok}");
            Console.ResetColor();

            // Logging
            LogManager.LogInventarAngezeigt(DataManager.Inventar.Count);

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// NEU: Bestand erhöhen
        /// </summary>
        public static void BestandErhoehen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Bestand erhöhen", ConsoleColor.Green);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Artikel im Inventar vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Zeige Inventar
            ZeigeInventarKompakt();

            string invNr = ConsoleHelper.GetInput("\nInventar-Nr des Artikels");

            var artikel = DataManager.Inventar.FirstOrDefault(a =>
                a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase));

            if (artikel == null)
            {
                ConsoleHelper.PrintError($"Artikel '{invNr}' nicht gefunden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  📦 Artikel: {artikel.GeraeteName}");
            Console.WriteLine($"  📊 Aktueller Bestand: {artikel.Anzahl} Stück");
            Console.ResetColor();

            string mengeText = ConsoleHelper.GetInput("\nWie viele Stück hinzufügen?");

            if (!int.TryParse(mengeText, out int menge) || menge <= 0)
            {
                ConsoleHelper.PrintError("Ungültige Menge!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            int alterBestand = artikel.Anzahl;
            DataManager.BestandErhoehen(invNr, menge);

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"✓ Bestand erhöht: {alterBestand} → {artikel.Anzahl} (+{menge})");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  Status: {artikel.GetBestandsStatusText()}");
            Console.ResetColor();

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// NEU: Bestand verringern
        /// </summary>
        public static void BestandVerringern()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Bestand verringern", ConsoleColor.Red);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Artikel im Inventar vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Zeige Inventar
            ZeigeInventarKompakt();

            string invNr = ConsoleHelper.GetInput("\nInventar-Nr des Artikels");

            var artikel = DataManager.Inventar.FirstOrDefault(a =>
                a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase));

            if (artikel == null)
            {
                ConsoleHelper.PrintError($"Artikel '{invNr}' nicht gefunden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  📦 Artikel: {artikel.GeraeteName}");
            Console.WriteLine($"  📊 Aktueller Bestand: {artikel.Anzahl} Stück");
            Console.ResetColor();

            string mengeText = ConsoleHelper.GetInput("\nWie viele Stück entnehmen?");

            if (!int.TryParse(mengeText, out int menge) || menge <= 0)
            {
                ConsoleHelper.PrintError("Ungültige Menge!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            int alterBestand = artikel.Anzahl;
            bool erfolg = DataManager.BestandVerringern(invNr, menge);

            if (!erfolg)
            {
                ConsoleHelper.PrintError($"Nicht genug Bestand! Verfügbar: {alterBestand}, Benötigt: {menge}");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"✓ Bestand verringert: {alterBestand} → {artikel.Anzahl} (-{menge})");

            if (artikel.Anzahl <= artikel.Mindestbestand)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠️ WARNUNG: Mindestbestand erreicht oder unterschritten!");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  Status: {artikel.GetBestandsStatusText()}");
            Console.ResetColor();

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// NEU: Mindestbestand ändern
        /// </summary>
        public static void MindestbestandAendern()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Mindestbestand konfigurieren", ConsoleColor.Magenta);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Artikel im Inventar vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Zeige Inventar
            ZeigeInventarKompakt();

            string invNr = ConsoleHelper.GetInput("\nInventar-Nr des Artikels");

            var artikel = DataManager.Inventar.FirstOrDefault(a =>
                a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase));

            if (artikel == null)
            {
                ConsoleHelper.PrintError($"Artikel '{invNr}' nicht gefunden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  📦 Artikel: {artikel.GeraeteName}");
            Console.WriteLine($"  📊 Aktueller Bestand: {artikel.Anzahl} Stück");
            Console.WriteLine($"  ⚠️  Aktueller Mindestbestand: {artikel.Mindestbestand} Stück");
            Console.ResetColor();

            string neuerWertText = ConsoleHelper.GetInput("\nNeuer Mindestbestand");

            if (!int.TryParse(neuerWertText, out int neuerWert) || neuerWert < 0)
            {
                ConsoleHelper.PrintError("Ungültiger Wert!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            int alterWert = artikel.Mindestbestand;
            DataManager.MindestbestandAendern(invNr, neuerWert);

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"✓ Mindestbestand geändert: {alterWert} → {neuerWert}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  Neuer Status: {artikel.GetBestandsStatusText()}");
            Console.ResetColor();

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// NEU: Zeigt alle Artikel an, die unter dem Mindestbestand sind
        /// </summary>
        public static void ZeigeArtikelUnterMindestbestand()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🔴 Artikel unter Mindestbestand", ConsoleColor.Red);

            var artikelUnterMindest = DataManager.GetArtikelUnterMindestbestand();

            if (artikelUnterMindest.Count == 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✓ Alle Artikel haben ausreichend Bestand!");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠️  {artikelUnterMindest.Count} Artikel benötigen Nachbestellung:");
            Console.ResetColor();
            Console.WriteLine();

            // Spaltenüberschriften
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {"Inv-Nr",-10} {"Gerät",-25} {"Bestand",-10} {"Mindest",-10} {"Status"}");
            Console.WriteLine($"  {new string('─', 10)} {new string('─', 25)} {new string('─', 10)} {new string('─', 10)} {new string('─', 15)}");
            Console.ResetColor();

            foreach (var artikel in artikelUnterMindest)
            {
                var status = artikel.GetBestandsStatus();
                ConsoleColor farbe = status == BestandsStatus.Leer ? ConsoleColor.Red : ConsoleColor.Yellow;

                Console.ForegroundColor = farbe;
                Console.WriteLine($"  {artikel.InvNmr,-10} {artikel.GeraeteName,-25} {artikel.Anzahl,-10} {artikel.Mindestbestand,-10} {artikel.GetBestandsStatusText()}");
                Console.ResetColor();
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// NEU: Zeigt detaillierte Informationen zu einem Artikel
        /// </summary>
        public static void ZeigeArtikelDetails()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Artikel-Details anzeigen", ConsoleColor.Cyan);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Artikel im Inventar vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            ZeigeInventarKompakt();

            string invNr = ConsoleHelper.GetInput("\nInventar-Nr des Artikels");

            var artikel = DataManager.Inventar.FirstOrDefault(a =>
                a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase));

            if (artikel == null)
            {
                ConsoleHelper.PrintError($"Artikel '{invNr}' nicht gefunden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Detailansicht
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                    ARTIKEL-DETAILS                                ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📌 Inventar-Nr:      {artikel.InvNmr}");
            Console.WriteLine($"  📦 Gerätename:       {artikel.GeraeteName}");
            Console.WriteLine($"  🔢 Seriennummer:     {artikel.SerienNummer}");
            Console.WriteLine($"  🏭 Hersteller:       {artikel.Hersteller}");
            Console.WriteLine($"  📂 Kategorie:        {artikel.Kategorie}");
            Console.WriteLine($"  💰 Anschaffungspreis: {artikel.Preis:F2}€");
            Console.WriteLine($"  📅 Anschaffungsdatum: {artikel.Anschaffungsdatum:dd.MM.yyyy}");
            Console.WriteLine($"  🧾 Rechnungsdatum:   {artikel.Rechnungsdatum:dd.MM.yyyy}");

            // Garantie mit Farbcodierung
            bool garantieAbgelaufen = artikel.GarantieBis < DateTime.Now;
            bool garantieBaldig = !garantieAbgelaufen && artikel.GarantieBis < DateTime.Now.AddMonths(3);
            Console.ForegroundColor = garantieAbgelaufen ? ConsoleColor.Red : garantieBaldig ? ConsoleColor.Yellow : ConsoleColor.Green;
            string garantieStatus = garantieAbgelaufen ? "❌ ABGELAUFEN" : garantieBaldig ? "⚠️  LÄUFT BALD AB" : "✓ AKTIV";
            Console.WriteLine($"  🛡️  Garantie bis:    {artikel.GarantieBis:dd.MM.yyyy}  [{garantieStatus}]");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  👤 Zugewiesen an:    {artikel.MitarbeiterBezeichnung}");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ─── BESTANDSINFORMATIONEN ───");
            Console.ResetColor();

            var status = artikel.GetBestandsStatus();
            ConsoleColor statusFarbe = status switch
            {
                BestandsStatus.Leer => ConsoleColor.Red,
                BestandsStatus.Niedrig => ConsoleColor.Yellow,
                _ => ConsoleColor.Green
            };

            Console.ForegroundColor = statusFarbe;
            Console.WriteLine($"  📊 Aktueller Bestand: {artikel.Anzahl} Stück");
            Console.WriteLine($"  ⚠️  Mindestbestand:   {artikel.Mindestbestand} Stück");
            Console.WriteLine($"  🚦 Status:           {artikel.GetBestandsStatusText()}");
            Console.ResetColor();

            // Tracking-Informationen
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  ─── TRACKING-INFORMATIONEN ───");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  👨‍💼 Erstellt von:     {artikel.ErstelltVon}");
            Console.WriteLine($"  🕐 Erstellt am:       {artikel.ErstelltAm:dd.MM.yyyy HH:mm:ss}");
            Console.ResetColor();

            // Zusätzliche Infos
            Console.WriteLine();
            int alter = (DateTime.Now - artikel.Anschaffungsdatum).Days;
            int alterSeitErstellung = (DateTime.Now - artikel.ErstelltAm).Days;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  📆 Alter des Geräts: {alter} Tage ({alter / 365} Jahre, {alter % 365 / 30} Monate)");
            Console.WriteLine($"  📆 Im System seit: {alterSeitErstellung} Tage(n)");
            Console.ResetColor();

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Hilfsmethode: Zeigt kompakte Inventarliste
        /// </summary>
        private static void ZeigeInventarKompakt()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Verfügbare Artikel:");
            Console.ResetColor();

            foreach (var artikel in DataManager.Inventar)
            {
                var status = artikel.GetBestandsStatus();
                ConsoleColor farbe = status switch
                {
                    BestandsStatus.Leer => ConsoleColor.Red,
                    BestandsStatus.Niedrig => ConsoleColor.Yellow,
                    _ => ConsoleColor.DarkGray
                };

                Console.ForegroundColor = farbe;
                Console.WriteLine($"     [{artikel.InvNmr}] {artikel.GeraeteName} - {artikel.Anzahl}/{artikel.Mindestbestand} Stück");
                Console.ResetColor();
            }
        }
    }
}