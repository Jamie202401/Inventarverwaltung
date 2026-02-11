using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// ⚡ PREMIUM SCHNELLERFASSUNGS-MANAGER ⚡
    /// 
    /// Ultra-Schnell-Modus | CSV-Massen-Import | Template-System
    /// Blitzschnelle Artikel-Anlage mit modernem Design
    /// 
    /// Features:
    /// • Ein-Zeilen-Eingabe (InvNr;Gerät;Mitarbeiter;Anzahl)
    /// • CSV-Import für Massen-Anlage
    /// • Template-System für häufige Artikel
    /// • Intelligente Kategorie-Ableitung
    /// • Auto-Vervollständigung
    /// • Batch-Verarbeitung
    /// </summary>
    public static class SchnellerfassungsManager
    {
        // ═══════════════════════════════════════════════════════════════
        // KONSTANTEN & KONFIGURATION
        // ═══════════════════════════════════════════════════════════════

        private static readonly string TemplateVerzeichnis = Path.Combine(Environment.CurrentDirectory, "Templates");
        private static readonly string TemplateListeDatei = Path.Combine(TemplateVerzeichnis, "artikel_templates.txt");

        // Design-Farben
        private static readonly ConsoleColor HeaderFarbe = ConsoleColor.Yellow;
        private static readonly ConsoleColor ErfolgFarbe = ConsoleColor.Green;
        private static readonly ConsoleColor FehlerFarbe = ConsoleColor.Red;
        private static readonly ConsoleColor InfoFarbe = ConsoleColor.Cyan;
        private static readonly ConsoleColor TippFarbe = ConsoleColor.DarkGray;

        // ═══════════════════════════════════════════════════════════════
        // HAUPTMENÜ
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Zeigt das Premium Schnellerfassungs-Hauptmenü
        /// </summary>
        public static void ZeigeSchnellerfassungsMenu()
        {
            bool menuAktiv = true;

            while (menuAktiv)
            {
                ZeigePremiumHeader();
                ZeigeHauptmenuOptionen();

                string auswahl = LeseEingabe("Ihre Auswahl");

                switch (auswahl)
                {
                    case "1":
                        UltraSchnellModus();
                        break;
                    case "2":
                        CSVMassenImport();
                        break;
                    case "3":
                        ArtikelAusTemplate();
                        break;
                    case "4":
                        TemplateVerwaltung();
                        break;
                    case "5":
                        ZeigeHilfe();
                        break;
                    case "0":
                        menuAktiv = false;
                        break;
                    default:
                        ZeigeFehler("Ungültige Auswahl!");
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        /// <summary>
        /// Zeigt den Premium-Header mit Animation
        /// </summary>
        private static void ZeigePremiumHeader()
        {
            Console.Clear();
            Console.WriteLine();

            // Gradient-Header
            string[] headerZeilen = {
                "╔═══════════════════════════════════════════════════════════════════════════════════════╗",
                "║                                                                                       ║",
                "║                          ⚡ SCHNELLERFASSUNGS-CENTER ⚡                               ║",
                "║                                                                                       ║",
                "║              Ultra-Schnell • CSV-Import • Templates • Batch-Modus                    ║",
                "║                                                                                       ║",
                "╚═══════════════════════════════════════════════════════════════════════════════════════╝"
            };

            foreach (var zeile in headerZeilen)
            {
                Console.ForegroundColor = HeaderFarbe;
                Console.WriteLine($"  {zeile}");
                Thread.Sleep(30);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Zeigt die Hauptmenü-Optionen mit Premium-Design
        /// </summary>
        private static void ZeigeHauptmenuOptionen()
        {
            // Statistik-Box
            ZeigeStatistikBox();

            Console.WriteLine();

            // Optionen mit Icons und Beschreibungen
            var optionen = new[]
            {
                (Icon: "⚡", Nummer: "1", Titel: "Ultra-Schnell-Modus", Beschreibung: "Ein-Zeilen-Eingabe für blitzschnelle Artikel-Anlage", Farbe: ConsoleColor.Yellow),
                (Icon: "📥", Nummer: "2", Titel: "CSV-Massen-Import", Beschreibung: "Hunderte Artikel auf einmal aus CSV-Datei importieren", Farbe: ConsoleColor.Green),
                (Icon: "📋", Nummer: "3", Titel: "Template-System", Beschreibung: "Artikel aus gespeicherten Vorlagen erstellen", Farbe: ConsoleColor.Magenta),
                (Icon: "🔧", Nummer: "4", Titel: "Template-Verwaltung", Beschreibung: "Templates erstellen, bearbeiten und löschen", Farbe: ConsoleColor.Blue),
                (Icon: "❓", Nummer: "5", Titel: "Hilfe & Beispiele", Beschreibung: "Ausführliche Anleitung und Beispiele", Farbe: ConsoleColor.Cyan)
            };

            foreach (var option in optionen)
            {
                ZeigePremiumOption(option.Icon, option.Nummer, option.Titel, option.Beschreibung, option.Farbe);
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ┌─────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │  [0] ← Zurück zum Hauptmenü                                                    │");
            Console.WriteLine("  └─────────────────────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Zeigt eine Premium-Option im Menü
        /// </summary>
        private static void ZeigePremiumOption(string icon, string nummer, string titel, string beschreibung, ConsoleColor farbe)
        {
            Console.ForegroundColor = farbe;
            Console.WriteLine($"  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{nummer}] {icon} ");
            Console.ForegroundColor = farbe;
            Console.Write($"{titel,-70}");
            Console.WriteLine("  ║");
            Console.ForegroundColor = TippFarbe;
            Console.WriteLine($"  ║      {beschreibung,-75}║");
            Console.ForegroundColor = farbe;
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Zeigt eine kompakte Statistik-Box
        /// </summary>
        private static void ZeigeStatistikBox()
        {
            int artikelGesamt = DataManager.Inventar.Count;
            int templates = LadeTemplates().Count;

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ┌─────────────────────────────────────────────────────────────────────────────────┐");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  │  📊 ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Gesamt: {artikelGesamt,3} Artikel");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  •  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Templates: {templates,2}");
            Console.SetCursorPosition(92, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("│");
            Console.WriteLine("  └─────────────────────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        // ═══════════════════════════════════════════════════════════════
        // 1. ULTRA-SCHNELL-MODUS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Ultra-Schnell-Modus: Blitzschnelle Ein-Zeilen-Eingabe
        /// </summary>
        private static void UltraSchnellModus()
        {
            Console.Clear();
            ZeigeSektionsHeader("⚡ ULTRA-SCHNELL-MODUS", HeaderFarbe);

            ZeigeSchnellModusAnleitung();

            bool weiterEingeben = true;
            int erfolgreiche = 0;
            int fehler = 0;

            while (weiterEingeben)
            {
                Console.WriteLine();
                Console.ForegroundColor = InfoFarbe;
                Console.Write("  ⚡ ");
                Console.ForegroundColor = ConsoleColor.White;

                string eingabe = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(eingabe))
                {
                    continue;
                }

                // Befehle
                if (eingabe.Equals("exit", StringComparison.OrdinalIgnoreCase) || eingabe == "0")
                {
                    weiterEingeben = false;
                    continue;
                }

                if (eingabe.Equals("batch", StringComparison.OrdinalIgnoreCase) || eingabe.Equals("liste", StringComparison.OrdinalIgnoreCase))
                {
                    int batchErfolg = BatchEingabeModus();
                    erfolgreiche += batchErfolg;
                    continue;
                }

                if (eingabe.Equals("help", StringComparison.OrdinalIgnoreCase) || eingabe == "?")
                {
                    ZeigeSchnellModusAnleitung();
                    continue;
                }

                // Artikel erstellen
                if (ParseUndErstelleArtikel(eingabe))
                {
                    erfolgreiche++;
                    ZeigeErfolg($"✓ Artikel #{erfolgreiche} erfolgreich angelegt!", false);
                }
                else
                {
                    fehler++;
                    ZeigeFehler("✗ Fehler bei der Eingabe! Format: InvNr;Gerät;Mitarbeiter;Anzahl");
                }
            }

            // Zusammenfassung
            ZeigeAbschlussSummary(erfolgreiche, fehler);
        }

        /// <summary>
        /// Zeigt die Anleitung für den Schnell-Modus
        /// </summary>
        private static void ZeigeSchnellModusAnleitung()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                              📝 EINGABE-FORMAT                                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = InfoFarbe;
            Console.Write("  Format: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("InvNr;Gerät;Mitarbeiter;Anzahl");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = TippFarbe;
            Console.WriteLine("  📌 Beispiele:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("     IT-001;Lenovo ThinkPad T14;Max Müller;5");
            Console.WriteLine("     BÜ-042;Bürostuhl ErgoMax Pro;Anna Schmidt;10");
            Console.WriteLine("     WZ-007;Akkuschrauber Bosch;Tom Weber;3");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = TippFarbe;
            Console.WriteLine("  💡 Auto-Werte:");
            Console.WriteLine("     • Kategorie → Aus Inv-Nr-Präfix (IT→IT-Hardware, BÜ→Büroausstattung)");
            Console.WriteLine("     • Seriennummer → Auto-generiert");
            Console.WriteLine("     • Mindestbestand → Anzahl ÷ 2");
            Console.WriteLine("     • Preis → 0.00€");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⌨️  Befehle:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("     batch / liste  → Mehrere Artikel auf einmal");
            Console.WriteLine("     help / ?       → Diese Hilfe anzeigen");
            Console.WriteLine("     exit / 0       → Beenden");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 85));
            Console.ResetColor();
        }

        /// <summary>
        /// Batch-Eingabe-Modus (mehrere Artikel)
        /// </summary>
        private static int BatchEingabeModus()
        {
            Console.WriteLine();
            ZeigeSektionsHeader("📝 BATCH-EINGABE-MODUS", ConsoleColor.Magenta);

            Console.ForegroundColor = TippFarbe;
            Console.WriteLine("  Geben Sie mehrere Artikel ein (eine Zeile pro Artikel).");
            Console.WriteLine("  Beenden mit leerer Zeile oder 'done'.");
            Console.ResetColor();
            Console.WriteLine();

            List<string> zeilen = new List<string>();
            int zeilenNummer = 1;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  [{zeilenNummer,3}] ");
                Console.ResetColor();

                string zeile = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(zeile) || zeile.Equals("done", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                zeilen.Add(zeile);
                zeilenNummer++;
            }

            if (zeilen.Count == 0)
            {
                ZeigeWarnung("Keine Eingaben erfasst.");
                return 0;
            }

            // Verarbeitung mit Progress-Bar
            return VerarbeiteBatch(zeilen);
        }

        /// <summary>
        /// Verarbeitet Batch-Eingaben mit Fortschrittsanzeige
        /// </summary>
        private static int VerarbeiteBatch(List<string> zeilen)
        {
            Console.WriteLine();
            Console.ForegroundColor = InfoFarbe;
            Console.WriteLine($"  ⏳ Verarbeite {zeilen.Count} Artikel...");
            Console.ResetColor();
            Console.WriteLine();

            int erfolg = 0;
            int fehler = 0;

            for (int i = 0; i < zeilen.Count; i++)
            {
                // Progress Bar
                ZeigeProgressBar(i + 1, zeilen.Count);

                if (ParseUndErstelleArtikel(zeilen[i]))
                {
                    erfolg++;
                    Console.ForegroundColor = ErfolgFarbe;
                    Console.WriteLine($"  ✓ [{i + 1}/{zeilen.Count}] {TruncateString(zeilen[i], 70)}");
                    Console.ResetColor();
                }
                else
                {
                    fehler++;
                    Console.ForegroundColor = FehlerFarbe;
                    Console.WriteLine($"  ✗ [{i + 1}/{zeilen.Count}] {TruncateString(zeilen[i], 70)}");
                    Console.ResetColor();
                }

                Thread.Sleep(100); // Animation
            }

            Console.WriteLine();
            ZeigeErfolg($"Batch abgeschlossen: {erfolg} erfolgreich, {fehler} Fehler", true);

            return erfolg;
        }

        // ═══════════════════════════════════════════════════════════════
        // 2. CSV-MASSEN-IMPORT
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// CSV-Import für Massen-Anlage mit Premium-Design
        /// </summary>
        private static void CSVMassenImport()
        {
            Console.Clear();
            ZeigeSektionsHeader("📥 CSV-MASSEN-IMPORT", ConsoleColor.Green);

            ZeigeCSVFormatInfo();

            string csvPfad = LeseEingabe("📁 Pfad zur CSV-Datei");

            if (string.IsNullOrEmpty(csvPfad))
            {
                ZeigeFehler("Kein Pfad angegeben!");
                Thread.Sleep(1500);
                return;
            }

            if (!File.Exists(csvPfad))
            {
                ZeigeFehler($"Datei nicht gefunden: {csvPfad}");
                Thread.Sleep(1500);
                return;
            }

            VerarbeiteCSVDatei(csvPfad);
        }

        /// <summary>
        /// Zeigt CSV-Format-Informationen
        /// </summary>
        private static void ZeigeCSVFormatInfo()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                              📋 CSV-FORMAT                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = InfoFarbe;
            Console.WriteLine("  Erforderliche Spalten:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  InvNr;Gerät;Mitarbeiter;Anzahl;Preis;Kategorie");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = TippFarbe;
            Console.WriteLine("  📄 Beispiel CSV-Datei:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  IT-001;Laptop Dell XPS 15;Max Müller;5;1299.99;IT-Hardware");
            Console.WriteLine("  IT-002;Monitor LG 27\";Anna Schmidt;10;299.50;IT-Hardware");
            Console.WriteLine("  BÜ-001;Bürostuhl Herman Miller;Tom Weber;15;849.00;Büroausstattung");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = TippFarbe;
            Console.WriteLine("  💡 Hinweise:");
            Console.WriteLine("     • Erste Zeile kann Header sein (wird automatisch erkannt)");
            Console.WriteLine("     • Felder Preis und Kategorie sind optional");
            Console.WriteLine("     • Trennzeichen: Semikolon (;)");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 85));
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Verarbeitet eine CSV-Datei mit Fortschrittsanzeige
        /// </summary>
        private static void VerarbeiteCSVDatei(string csvPfad)
        {
            try
            {
                string[] zeilen = File.ReadAllLines(csvPfad, Encoding.UTF8);

                Console.WriteLine();
                Console.ForegroundColor = InfoFarbe;
                Console.WriteLine($"  📊 {zeilen.Length} Zeilen in CSV-Datei gefunden");
                Console.ResetColor();

                // Header-Erkennung
                int startIndex = zeilen[0].Contains("InvNr") || zeilen[0].Contains("Inventar") ? 1 : 0;

                if (startIndex == 1)
                {
                    Console.ForegroundColor = TippFarbe;
                    Console.WriteLine("  ℹ️  Header-Zeile erkannt und übersprungen");
                    Console.ResetColor();
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ⏳ Import läuft...");
                Console.ResetColor();
                Console.WriteLine();

                int erfolg = 0;
                int fehler = 0;

                for (int i = startIndex; i < zeilen.Length; i++)
                {
                    string zeile = zeilen[i].Trim();

                    if (string.IsNullOrEmpty(zeile))
                    {
                        continue;
                    }

                    // Progress
                    ZeigeProgressBar(i - startIndex + 1, zeilen.Length - startIndex);

                    if (ParseUndErstelleArtikelCSV(zeile))
                    {
                        erfolg++;
                        Console.ForegroundColor = ErfolgFarbe;
                        Console.WriteLine($"  ✓ Zeile {i + 1,4}: {TruncateString(zeile, 65)}");
                        Console.ResetColor();
                    }
                    else
                    {
                        fehler++;
                        Console.ForegroundColor = FehlerFarbe;
                        Console.WriteLine($"  ✗ Zeile {i + 1,4}: Fehler beim Import");
                        Console.ResetColor();
                    }

                    Thread.Sleep(50); // Animation
                }

                // Zusammenfassung
                ZeigeImportSummary(erfolg, fehler);
               // LogManager.LogImport(erfolg);
            }
            catch (Exception ex)
            {
                ZeigeFehler($"CSV-Import-Fehler: {ex.Message}");
                Thread.Sleep(2000);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 3. TEMPLATE-SYSTEM
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Artikel aus Template erstellen
        /// </summary>
        private static void ArtikelAusTemplate()
        {
            Console.Clear();
            ZeigeSektionsHeader("📋 ARTIKEL AUS TEMPLATE ERSTELLEN", ConsoleColor.Magenta);

            var templates = LadeTemplates();

            if (templates.Count == 0)
            {
                Console.WriteLine();
                ZeigeWarnung("Noch keine Templates vorhanden!");
                Console.WriteLine();
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine("  💡 Erstellen Sie zuerst Templates über Option [4]");
                Console.ResetColor();
                Thread.Sleep(2000);
                return;
            }

            ZeigeTemplateListe(templates);

            string auswahlText = LeseEingabe("Template wählen");

            if (!int.TryParse(auswahlText, out int auswahl) || auswahl < 1 || auswahl > templates.Count)
            {
                ZeigeFehler("Ungültige Auswahl!");
                Thread.Sleep(1000);
                return;
            }

            var gewaehltes = templates[auswahl - 1];

            // Template-Info anzeigen
            ZeigeTemplateInfo(gewaehltes);

            // Artikel-Details
            string invNr = LeseEingabe("📦 Inventar-Nr");
            string mitarbeiter = LeseEingabe("👤 Mitarbeiter");
            string anzahlText = LeseEingabe("🔢 Anzahl");

            if (!int.TryParse(anzahlText, out int anzahl) || anzahl < 0)
            {
                ZeigeFehler("Ungültige Anzahl!");
                Thread.Sleep(1000);
                return;
            }

            // Artikel erstellen
            if (ErstelleArtikelAusTemplate(gewaehltes, invNr, mitarbeiter, anzahl))
            {
                Console.WriteLine();
                ZeigeErfolg($"✓ Artikel '{invNr}' erfolgreich aus Template '{gewaehltes.Name}' erstellt!", true);
            }
        }

        /// <summary>
        /// Template-Verwaltung mit Premium-Design
        /// </summary>
        private static void TemplateVerwaltung()
        {
            bool verwaltungAktiv = true;

            while (verwaltungAktiv)
            {
                Console.Clear();
                ZeigeSektionsHeader("🔧 TEMPLATE-VERWALTUNG", ConsoleColor.Blue);

                var templates = LadeTemplates();

                Console.WriteLine();
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine($"  📊 Gespeicherte Templates: {templates.Count}");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  ┌─────────────────────────────────────────────────────────────────────────────┐");
                Console.WriteLine("  │  [1] ➕ Neues Template erstellen                                           │");
                Console.WriteLine("  │  [2] 📋 Templates anzeigen                                                 │");
                Console.WriteLine("  │  [3] ✏️  Template bearbeiten                                               │");
                Console.WriteLine("  │  [4] 🗑️  Template löschen                                                  │");
                Console.WriteLine("  │  [0] ← Zurück                                                              │");
                Console.WriteLine("  └─────────────────────────────────────────────────────────────────────────────┘");
                Console.ResetColor();

                Console.WriteLine();
                string auswahl = LeseEingabe("Auswahl");

                switch (auswahl)
                {
                    case "1":
                        NeuesTemplateErstellen();
                        break;
                    case "2":
                        TemplatesAnzeigen();
                        break;
                    case "3":
                        TemplateBearbeiten();
                        break;
                    case "4":
                        TemplateLöschen();
                        break;
                    case "0":
                        verwaltungAktiv = false;
                        break;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 4. HILFE-SYSTEM
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Zeigt ausführliche Hilfe und Beispiele mit Premium-Design
        /// </summary>
        private static void ZeigeHilfe()
        {
            bool hilfeAktiv = true;

            while (hilfeAktiv)
            {
                Console.Clear();
                ZeigeSektionsHeader("❓ HILFE & DOKUMENTATION", ConsoleColor.Cyan);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║                          📚 HILFE-BEREICHE                                       ║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [1] ⚡ Ultra-Schnell-Modus");
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine("      Ein-Zeilen-Eingabe, Batch-Modus, Befehle");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [2] 📥 CSV-Massen-Import");
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine("      Dateiformat, Excel-Export, Tipps & Tricks");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("  [3] 📋 Template-System");
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine("      Templates erstellen, verwenden und verwalten");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  [4] 🎓 Kategorie-Präfixe");
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine("      Automatische Kategorie-Zuordnung verstehen");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("  [5] 💡 Tipps & Best Practices");
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine("      Profi-Tipps für maximale Effizienz");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [6] 📖 Vollständige Dokumentation");
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine("      Alle Themen im Überblick");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [0] ← Zurück");
                Console.ResetColor();
                Console.WriteLine();

                string auswahl = LeseEingabe("Hilfe-Bereich wählen");

                switch (auswahl)
                {
                    case "1":
                        ZeigeHilfeSchnellModus();
                        break;
                    case "2":
                        ZeigeHilfeCSVImport();
                        break;
                    case "3":
                        ZeigeHilfeTemplates();
                        break;
                    case "4":
                        ZeigeHilfeKategorien();
                        break;
                    case "5":
                        ZeigeHilfeTipps();
                        break;
                    case "6":
                        ZeigeVollstaendigeDokumentation();
                        break;
                    case "0":
                        hilfeAktiv = false;
                        break;
                    default:
                        ZeigeFehler("Ungültige Auswahl!");
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        /// <summary>
        /// Hilfe: Ultra-Schnell-Modus
        /// </summary>
        private static void ZeigeHilfeSchnellModus()
        {
            Console.Clear();
            ZeigeSektionsHeader("⚡ ULTRA-SCHNELL-MODUS", ConsoleColor.Yellow);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                         📝 EINGABE-FORMAT                                        ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Format:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │  InvNr ; Gerät ; Mitarbeiter ; Anzahl                                          │");
            Console.WriteLine("  └─────────────────────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                         💡 BEISPIELE                                             ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            var beispiele = new[]
            {
                new { Inv = "IT-001", Geraet = "Lenovo ThinkPad T14", Ma = "Max Müller", Anz = "5" },
                new { Inv = "BÜ-042", Geraet = "Bürostuhl ErgoMax Pro", Ma = "Anna Schmidt", Anz = "10" },
                new { Inv = "WZ-007", Geraet = "Akkuschrauber Bosch", Ma = "Tom Weber", Anz = "3" },
                new { Inv = "KOM-123", Geraet = "iPhone 15 Pro", Ma = "Sarah Klein", Anz = "2" }
            };

            foreach (var bsp in beispiele)
            {
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{bsp.Inv,-8}");
                Console.ResetColor();
                Console.Write(";");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{bsp.Geraet,-25}");
                Console.ResetColor();
                Console.Write(";");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{bsp.Ma,-15}");
                Console.ResetColor();
                Console.Write(";");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{bsp.Anz}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                         ⌨️  BEFEHLE                                              ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            ZeigeBefehlsZeile("batch / liste", "Mehrfach-Eingabe-Modus aktivieren");
            ZeigeBefehlsZeile("help / ?", "Diese Hilfe anzeigen");
            ZeigeBefehlsZeile("exit / 0", "Ultra-Schnell-Modus beenden");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      ✨ AUTO-WERTE                                               ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            ZeigeAutoWertZeile("Kategorie", "Aus Inv-Nr-Präfix (IT → IT-Hardware, BÜ → Büroausstattung)");
            ZeigeAutoWertZeile("Seriennummer", "Auto-generiert (SN-20260209153045)");
            ZeigeAutoWertZeile("Mindestbestand", "Automatisch: Anzahl ÷ 2");
            ZeigeAutoWertZeile("Preis", "Standard: 0.00€");
            ZeigeAutoWertZeile("Erstellt von", "Aktuell angemeldeter Benutzer");
            ZeigeAutoWertZeile("Erstellt am", "Aktueller Zeitstempel");

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Hilfe: CSV-Import
        /// </summary>
        private static void ZeigeHilfeCSVImport()
        {
            Console.Clear();
            ZeigeSektionsHeader("📥 CSV-MASSEN-IMPORT", ConsoleColor.Green);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                         📋 CSV-FORMAT                                            ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  Erforderliches Format:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │  InvNr ; Gerät ; Mitarbeiter ; Anzahl ; Preis ; Kategorie                      │");
            Console.WriteLine("  └─────────────────────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = TippFarbe;
            Console.WriteLine("  💡 Preis und Kategorie sind optional");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      📄 BEISPIEL CSV-DATEI                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  # Optional: Header-Zeile (wird automatisch erkannt)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  InvNr;Gerät;Mitarbeiter;Anzahl;Preis;Kategorie");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  IT-001;Laptop Dell XPS 15;Max Müller;5;1299.99;IT-Hardware");
            Console.WriteLine("  IT-002;Monitor LG 27\";Anna Schmidt;10;299.50;IT-Hardware");
            Console.WriteLine("  BÜ-001;Bürostuhl Herman Miller;Tom Weber;15;849.00;Büroausstattung");
            Console.WriteLine("  WZ-042;Akkuschrauber Set;Sarah Klein;8;159.90;Werkzeug");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      📊 EXCEL → CSV EXPORT                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Schritt-für-Schritt:");
            Console.ResetColor();
            Console.WriteLine();

            var schritte = new[]
            {
                new { Nr = "1", Text = "Excel-Tabelle vorbereiten mit Spalten: InvNr, Gerät, Mitarbeiter, Anzahl, ..." },
                new { Nr = "2", Text = "Datei → Speichern unter" },
                new { Nr = "3", Text = "Dateityp wählen: CSV (Trennzeichen-getrennt) (*.csv)" },
                new { Nr = "4", Text = "Speichern → Ja (nur aktives Blatt)" },
                new { Nr = "5", Text = "Datei in Schnellerfassung importieren [2]" }
            };

            foreach (var schritt in schritte)
            {
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[{schritt.Nr}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(schritt.Text);
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      ⚠️  WICHTIGE HINWEISE                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            ZeigeTippZeile("✓", "Trennzeichen: Semikolon (;)");
            ZeigeTippZeile("✓", "Encoding: UTF-8 (für Umlaute)");
            ZeigeTippZeile("✓", "Leere Zeilen werden übersprungen");
            ZeigeTippZeile("✓", "Duplikate (gleiche Inv-Nr) werden ignoriert");
            ZeigeTippZeile("✓", "Progress-Bar zeigt Fortschritt in Echtzeit");

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Hilfe: Templates
        /// </summary>
        private static void ZeigeHilfeTemplates()
        {
            Console.Clear();
            ZeigeSektionsHeader("📋 TEMPLATE-SYSTEM", ConsoleColor.Magenta);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      🎯 WAS SIND TEMPLATES?                                      ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Templates sind Vorlagen für häufig verwendete Artikel.");
            Console.WriteLine("  Einmal erstellt, können Sie Artikel mit nur 3 Eingaben anlegen:");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ➡️  Inventar-Nummer");
            Console.WriteLine("  ➡️  Mitarbeiter");
            Console.WriteLine("  ➡️  Anzahl");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                   📝 TEMPLATE ERSTELLEN                                          ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Beispiel: Lenovo ThinkPad T14");
            Console.ResetColor();
            Console.WriteLine();

            var templateFelder = new[]
            {
                new { Feld = "Template-Name", Wert = "Lenovo ThinkPad T14", Farbe = ConsoleColor.Cyan },
                new { Feld = "Kategorie", Wert = "IT-Hardware", Farbe = ConsoleColor.Yellow },
                new { Feld = "Hersteller", Wert = "Lenovo", Farbe = ConsoleColor.Green },
                new { Feld = "Standard-Preis", Wert = "899.99€", Farbe = ConsoleColor.Magenta }
            };

            foreach (var feld in templateFelder)
            {
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{feld.Feld,-20} ");
                Console.ForegroundColor = feld.Farbe;
                Console.WriteLine($"→ {feld.Wert}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                   🚀 ARTIKEL AUS TEMPLATE                                        ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Menü [3] → Template wählen → Nur noch 3 Felder ausfüllen:");
            Console.ResetColor();
            Console.WriteLine();

            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Inv-Nr:    ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("IT-042");

            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Mitarbeiter: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Sarah Klein");

            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Anzahl:    ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("3");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Fertig! Alle anderen Werte kommen aus dem Template.");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                   💡 VERWENDUNGSZWECKE                                           ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            var zwecke = new[]
            {
                "Standard-Laptops (MacBook Pro, ThinkPad, etc.)",
                "Büromöbel (Schreibtische, Stühle)",
                "Werkzeug-Sets",
                "Standard-Peripherie (Maus, Tastatur, Monitor)",
                "Firmen-Smartphones"
            };

            foreach (var zweck in zwecke)
            {
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("✓ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(zweck);
                Console.ResetColor();
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Hilfe: Kategorien
        /// </summary>
        private static void ZeigeHilfeKategorien()
        {
            Console.Clear();
            ZeigeSektionsHeader("🎓 KATEGORIE-PRÄFIXE", ConsoleColor.Cyan);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                   🤖 AUTOMATISCHE KATEGORISIERUNG                                ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Das System erkennt die Kategorie automatisch aus dem Inv-Nr-Präfix:");
            Console.ResetColor();
            Console.WriteLine();

            var kategorien = new[]
            {
                new { Praefix = "IT", Kategorie = "IT-Hardware", Beispiele = "Laptops, PCs, Server, Netzwerk", Farbe = ConsoleColor.Cyan },
                new { Praefix = "BÜ / BU", Kategorie = "Büroausstattung", Beispiele = "Möbel, Schreibtische, Stühle", Farbe = ConsoleColor.Yellow },
                new { Praefix = "WZ", Kategorie = "Werkzeug", Beispiele = "Bohrmaschinen, Schrauber, Sets", Farbe = ConsoleColor.Green },
                new { Praefix = "KOM", Kategorie = "Kommunikation", Beispiele = "Smartphones, Tablets, Headsets", Farbe = ConsoleColor.Magenta },
                new { Praefix = "MÖ / MO", Kategorie = "Möbel", Beispiele = "Schränke, Regale, Tische", Farbe = ConsoleColor.Blue },
                new { Praefix = "EL", Kategorie = "Elektronik", Beispiele = "Kabel, Adapter, Ladegeräte", Farbe = ConsoleColor.DarkCyan },
                new { Praefix = "???", Kategorie = "Sonstiges", Beispiele = "Alles andere", Farbe = ConsoleColor.DarkGray }
            };

            foreach (var kat in kategorien)
            {
                Console.ForegroundColor = kat.Farbe;
                Console.WriteLine($"  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
                Console.Write("  ║  ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{kat.Praefix,-12}");
                Console.ForegroundColor = kat.Farbe;
                Console.Write("→ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{kat.Kategorie,-66}");
                Console.ForegroundColor = kat.Farbe;
                Console.WriteLine("║");
                Console.ForegroundColor = TippFarbe;
                Console.WriteLine($"  ║  {kat.Beispiele,-80}║");
                Console.ForegroundColor = kat.Farbe;
                Console.WriteLine($"  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                         💡 BEISPIELE                                             ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            var beispiele = new[]
            {
                new { Inv = "IT-001", Kat = "→ IT-Hardware" },
                new { Inv = "BÜ-042", Kat = "→ Büroausstattung" },
                new { Inv = "WZ-123", Kat = "→ Werkzeug" },
                new { Inv = "KOM-007", Kat = "→ Kommunikation" },
                new { Inv = "XYZ-999", Kat = "→ Sonstiges (unbekanntes Präfix)" }
            };

            foreach (var bsp in beispiele)
            {
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{bsp.Inv,-12}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(bsp.Kat);
                Console.ResetColor();
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Hilfe: Tipps & Best Practices
        /// </summary>
        private static void ZeigeHilfeTipps()
        {
            Console.Clear();
            ZeigeSektionsHeader("💡 TIPPS & BEST PRACTICES", ConsoleColor.Blue);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      ⚡ GESCHWINDIGKEIT                                           ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            ZeigeTippMitIcon("🚀", "Ultra-Schnell-Modus", "Für 1-10 Artikel am schnellsten");
            ZeigeTippMitIcon("📥", "CSV-Import", "Ab 10+ Artikel → Excel vorbereiten, dann importieren");
            ZeigeTippMitIcon("📋", "Templates", "Für wiederkehrende Artikel → Einmal erstellen, immer nutzen");
            ZeigeTippMitIcon("⚡", "Batch-Modus", "Mehrere Artikel vorbereiten, dann alle auf einmal");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      📝 NAMENSKONVENTIONEN                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            ZeigeTippMitIcon("✓", "Inv-Nr", "Präfix + Nummer → IT-001, BÜ-042, WZ-123");
            ZeigeTippMitIcon("✓", "Gerätename", "Kurz & präzise → \"Lenovo ThinkPad T14\" statt \"Laptop\"");
            ZeigeTippMitIcon("✓", "Mitarbeiter", "Voller Name → \"Max Müller\" (für bessere Suche)");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      🎯 WORKFLOW-EMPFEHLUNGEN                                    ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Neues Equipment angekommen?");
            Console.ResetColor();
            Console.WriteLine();

            var workflow = new[]
            {
                new { Nr = "1", Typ = "Standard-Artikel", Methode = "→ Template erstellen/nutzen" },
                new { Nr = "2", Typ = "Einzelstücke", Methode = "→ Ultra-Schnell-Modus" },
                new { Nr = "3", Typ = "Große Lieferung", Methode = "→ Excel-Liste → CSV-Import" }
            };

            foreach (var wf in workflow)
            {
                Console.Write("  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[{wf.Nr}] ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{wf.Typ,-20}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(wf.Methode);
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      ⚠️  FEHLER VERMEIDEN                                        ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            ZeigeTippMitIcon("✗", "Duplikate", "Inv-Nr muss eindeutig sein → System prüft automatisch");
            ZeigeTippMitIcon("✗", "Anzahl", "Muss positive Zahl sein → Keine Buchstaben");
            ZeigeTippMitIcon("✗", "Trennzeichen", "Immer Semikolon (;) verwenden → Kein Komma!");

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Vollständige Dokumentation (alle Themen)
        /// </summary>
        private static void ZeigeVollstaendigeDokumentation()
        {
            Console.Clear();
            ZeigeSektionsHeader("📖 VOLLSTÄNDIGE DOKUMENTATION", ConsoleColor.White);

            // Alle Hilfe-Themen hintereinander anzeigen
            Console.WriteLine();
            Console.ForegroundColor = TippFarbe;
            Console.WriteLine("  Alle Hilfe-Themen werden nacheinander angezeigt.");
            Console.WriteLine("  Drücken Sie nach jedem Abschnitt eine beliebige Taste...");
            Console.ResetColor();
            Console.WriteLine();
            Thread.Sleep(2000);

            ZeigeHilfeSchnellModus();
            ZeigeHilfeCSVImport();
            ZeigeHilfeTemplates();
            ZeigeHilfeKategorien();
            ZeigeHilfeTipps();

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                  ✓ DOKUMENTATION ABGESCHLOSSEN                                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Thread.Sleep(1500);
        }

        // Hilfs-Funktionen für schöne Hilfe-Darstellung
        private static void ZeigeBefehlsZeile(string befehl, string beschreibung)
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{befehl,-15}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" → ");
            Console.ForegroundColor = TippFarbe;
            Console.WriteLine(beschreibung);
            Console.ResetColor();
        }

        private static void ZeigeAutoWertZeile(string feld, string beschreibung)
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"• {feld,-18}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" → ");
            Console.ForegroundColor = TippFarbe;
            Console.WriteLine(beschreibung);
            Console.ResetColor();
        }

        private static void ZeigeTippZeile(string icon, string text)
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{icon} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void ZeigeTippMitIcon(string icon, string titel, string beschreibung)
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{icon} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{titel,-20}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" → {beschreibung}");
            Console.ResetColor();
        }

        // ═══════════════════════════════════════════════════════════════
        // KERN-FUNKTIONEN (PARSING & ERSTELLUNG)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Parst Schnell-Eingabe und erstellt Artikel
        /// </summary>
        private static bool ParseUndErstelleArtikel(string eingabe)
        {
            try
            {
                string[] teile = eingabe.Split(';');

                if (teile.Length < 4)
                {
                    return false;
                }

                string invNr = teile[0].Trim();
                string geraet = teile[1].Trim();
                string mitarbeiter = teile[2].Trim();

                if (!int.TryParse(teile[3].Trim(), out int anzahl) || anzahl < 0)
                {
                    return false;
                }

                // Prüfe Duplikat
                if (DataManager.Inventar.Any(a => a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                // Auto-Werte
                string kategorie = LeitePraeFixKategorieAb(invNr);
                string serienNummer = $"SN-{DateTime.Now:yyyyMMddHHmmss}";
                decimal preis = 0.00m;
                string hersteller = "Unbekannt";
                DateTime anschaffungsdatum = DateTime.Now;
                int mindestbestand = Math.Max(1, anzahl / 2);
                string erstelltVon = AuthManager.AktuellerBenutzer ?? "System";
                DateTime erstelltAm = DateTime.Now;

                // Artikel erstellen
                var neuerArtikel = new InvId(
                    invNr, geraet, mitarbeiter, serienNummer,
                    preis, anschaffungsdatum, hersteller, kategorie,
                    anzahl, mindestbestand, erstelltVon, erstelltAm
                );

                DataManager.Inventar.Add(neuerArtikel);
                DataManager.SaveKomplettesInventar();
               // LogManager.LogArtikelHinzugefuegt(invNr, geraet);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parst CSV-Zeile und erstellt Artikel
        /// </summary>
        private static bool ParseUndErstelleArtikelCSV(string zeile)
        {
            try
            {
                string[] teile = zeile.Split(';');

                if (teile.Length < 4)
                {
                    return false;
                }

                string invNr = teile[0].Trim();
                string geraet = teile[1].Trim();
                string mitarbeiter = teile[2].Trim();

                if (!int.TryParse(teile[3].Trim(), out int anzahl) || anzahl < 0)
                {
                    return false;
                }

                // Optionale Felder
                decimal preis = teile.Length > 4 && decimal.TryParse(teile[4].Trim(), out decimal p) ? p : 0.00m;
                string kategorie = teile.Length > 5 ? teile[5].Trim() : LeitePraeFixKategorieAb(invNr);

                // Duplikat-Check
                if (DataManager.Inventar.Any(a => a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                // Auto-Werte
                string serienNummer = $"SN-{DateTime.Now:yyyyMMddHHmmss}-{invNr}";
                string hersteller = "Unbekannt";
                DateTime anschaffungsdatum = DateTime.Now;
                int mindestbestand = Math.Max(1, anzahl / 2);
                string erstelltVon = AuthManager.AktuellerBenutzer ?? "System";
                DateTime erstelltAm = DateTime.Now;

                var neuerArtikel = new InvId(
                    invNr, geraet, mitarbeiter, serienNummer,
                    preis, anschaffungsdatum, hersteller, kategorie,
                    anzahl, mindestbestand, erstelltVon, erstelltAm
                );

                DataManager.Inventar.Add(neuerArtikel);
                DataManager.SaveKomplettesInventar();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Erstellt Artikel aus Template
        /// </summary>
        private static bool ErstelleArtikelAusTemplate(ArtikelTemplate template, string invNr, string mitarbeiter, int anzahl)
        {
            try
            {
                if (DataManager.Inventar.Any(a => a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase)))
                {
                    ZeigeFehler($"Inv-Nr '{invNr}' existiert bereits!");
                    return false;
                }

                string serienNummer = $"SN-{DateTime.Now:yyyyMMddHHmmss}";
                int mindestbestand = Math.Max(1, anzahl / 2);

                var neuerArtikel = new InvId(
                    invNr, template.Name, mitarbeiter, serienNummer,
                    template.Preis, DateTime.Now, template.Hersteller, template.Kategorie,
                    anzahl, mindestbestand, AuthManager.AktuellerBenutzer ?? "System", DateTime.Now
                );

                DataManager.Inventar.Add(neuerArtikel);
                DataManager.SaveKomplettesInventar();
               // LogManager.LogArtikelHinzugefuegt(invNr, template.Name);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Leitet Kategorie aus Inv-Nr-Präfix ab
        /// </summary>
        private static string LeitePraeFixKategorieAb(string invNr)
        {
            string praefix = invNr.Split('-', '_')[0].ToUpper();

            return praefix switch
            {
                "IT" => "IT-Hardware",
                "BÜ" or "BU" or "BÜRO" => "Büroausstattung",
                "WZ" or "WERK" => "Werkzeug",
                "KOM" or "KOMM" => "Kommunikation",
                "MÖ" or "MO" or "MÖBEL" => "Möbel",
                "EL" or "ELEK" => "Elektronik",
                _ => "Sonstiges"
            };
        }

        // ═══════════════════════════════════════════════════════════════
        // TEMPLATE-VERWALTUNG
        // ═══════════════════════════════════════════════════════════════

        private static void NeuesTemplateErstellen()
        {
            Console.Clear();
            ZeigeSektionsHeader("➕ NEUES TEMPLATE ERSTELLEN", ErfolgFarbe);

            Console.WriteLine();
            string name = LeseEingabe("📝 Template-Name (z.B. 'Lenovo ThinkPad T14')");
            string kategorie = LeseEingabe("📂 Kategorie");
            string hersteller = LeseEingabe("🏭 Hersteller");
            string preisText = LeseEingabe("💰 Standard-Preis");

            if (!decimal.TryParse(preisText, out decimal preis))
            {
                preis = 0.00m;
            }

            var template = new ArtikelTemplate
            {
                Name = name,
                Kategorie = kategorie,
                Hersteller = hersteller,
                Preis = preis
            };

            StelleTemplateVerzeichnisSicher();
            var templates = LadeTemplates();
            templates.Add(template);
            SpeichereTemplates(templates);

            Console.WriteLine();
            ZeigeErfolg($"✓ Template '{name}' erfolgreich erstellt!", true);
        }

        private static void TemplatesAnzeigen()
        {
            Console.Clear();
            ZeigeSektionsHeader("📋 GESPEICHERTE TEMPLATES", InfoFarbe);

            var templates = LadeTemplates();

            if (templates.Count == 0)
            {
                Console.WriteLine();
                ZeigeWarnung("Noch keine Templates vorhanden!");
                Thread.Sleep(1500);
                return;
            }

            ZeigeTemplateListe(templates);
            ConsoleHelper.PressKeyToContinue();
        }

        private static void TemplateBearbeiten()
        {
            Console.Clear();
            ZeigeSektionsHeader("✏️ TEMPLATE BEARBEITEN", ConsoleColor.Blue);

            var templates = LadeTemplates();

            if (templates.Count == 0)
            {
                ZeigeWarnung("Keine Templates vorhanden!");
                Thread.Sleep(1500);
                return;
            }

            ZeigeTemplateListe(templates);

            string auswahlText = LeseEingabe("Template zum Bearbeiten");

            if (!int.TryParse(auswahlText, out int auswahl) || auswahl < 1 || auswahl > templates.Count)
            {
                ZeigeFehler("Ungültige Auswahl!");
                Thread.Sleep(1000);
                return;
            }

            var template = templates[auswahl - 1];

            Console.WriteLine();
            Console.ForegroundColor = TippFarbe;
            Console.WriteLine("  💡 Leer lassen = keine Änderung");
            Console.ResetColor();
            Console.WriteLine();

            string neuerName = LeseEingabe($"Name [{template.Name}]");
            string neueKategorie = LeseEingabe($"Kategorie [{template.Kategorie}]");
            string neuerHersteller = LeseEingabe($"Hersteller [{template.Hersteller}]");
            string neuerPreis = LeseEingabe($"Preis [{template.Preis:F2}€]");

            if (!string.IsNullOrWhiteSpace(neuerName)) template.Name = neuerName;
            if (!string.IsNullOrWhiteSpace(neueKategorie)) template.Kategorie = neueKategorie;
            if (!string.IsNullOrWhiteSpace(neuerHersteller)) template.Hersteller = neuerHersteller;
            if (decimal.TryParse(neuerPreis, out decimal p)) template.Preis = p;

            SpeichereTemplates(templates);

            Console.WriteLine();
            ZeigeErfolg("✓ Template aktualisiert!", true);
        }

        private static void TemplateLöschen()
        {
            Console.Clear();
            ZeigeSektionsHeader("🗑️ TEMPLATE LÖSCHEN", FehlerFarbe);

            var templates = LadeTemplates();

            if (templates.Count == 0)
            {
                ZeigeWarnung("Keine Templates vorhanden!");
                Thread.Sleep(1500);
                return;
            }

            ZeigeTemplateListe(templates);

            string auswahlText = LeseEingabe("Template zum Löschen");

            if (!int.TryParse(auswahlText, out int auswahl) || auswahl < 1 || auswahl > templates.Count)
            {
                ZeigeFehler("Ungültige Auswahl!");
                Thread.Sleep(1000);
                return;
            }

            string name = templates[auswahl - 1].Name;
            templates.RemoveAt(auswahl - 1);
            SpeichereTemplates(templates);

            Console.WriteLine();
            ZeigeErfolg($"✓ Template '{name}' gelöscht!", true);
        }

        // ═══════════════════════════════════════════════════════════════
        // UI-HILFSFUNKTIONEN
        // ═══════════════════════════════════════════════════════════════

        private static void ZeigeSektionsHeader(string titel, ConsoleColor farbe)
        {
            Console.WriteLine();
            Console.ForegroundColor = farbe;
            Console.WriteLine($"  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"  ║  {titel,-83}║");
            Console.WriteLine($"  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private static void ZeigeTemplateListe(List<ArtikelTemplate> templates)
        {
            Console.WriteLine();
            for (int i = 0; i < templates.Count; i++)
            {
                var t = templates[i];
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  ┌─ [{i + 1}] {t.Name}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  │   📂 {t.Kategorie,-30} 🏭 {t.Hersteller,-20} 💰 {t.Preis:F2}€");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  └─" + new string('─', 80));
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        private static void ZeigeTemplateInfo(ArtikelTemplate template)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"  ║  📋 Template: {template.Name,-68}║");
            Console.WriteLine($"  ╠═══════════════════════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  ║  📂 Kategorie:  {template.Kategorie,-65}║");
            Console.WriteLine($"  ║  🏭 Hersteller: {template.Hersteller,-65}║");
            Console.WriteLine($"  ║  💰 Preis:      {template.Preis,-65:F2}€║");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeProgressBar(int aktuell, int gesamt)
        {
            int prozent = (aktuell * 100) / gesamt;
            int balkenLaenge = 50;
            int gefuellt = (prozent * balkenLaenge) / 100;

            Console.Write("  [");
            Console.ForegroundColor = ErfolgFarbe;
            Console.Write(new string('█', gefuellt));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('░', balkenLaenge - gefuellt));
            Console.ResetColor();
            Console.Write($"] {prozent,3}% ({aktuell}/{gesamt})");
            Console.WriteLine();
        }

        private static void ZeigeAbschlussSummary(int erfolg, int fehler)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                              📊 ZUSAMMENFASSUNG                                  ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ErfolgFarbe;
            Console.WriteLine($"  ║  ✓ Erfolgreich:  {erfolg,-65}║");
            Console.ForegroundColor = FehlerFarbe;
            Console.WriteLine($"  ║  ✗ Fehler:       {fehler,-65}║");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  ║  📦 Gesamt:      {erfolg + fehler,-65}║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeImportSummary(int erfolg, int fehler)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                          ✓ IMPORT ABGESCHLOSSEN                                  ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  ║  ✓ Erfolgreich importiert: {erfolg,-57}║");
            Console.WriteLine($"  ║  ✗ Fehler:                 {fehler,-57}║");
            Console.WriteLine($"  ║  📦 Gesamt im Inventar:    {DataManager.Inventar.Count,-57}║");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        private static string LeseEingabe(string prompt)
        {
            Console.ForegroundColor = InfoFarbe;
            Console.Write($"  ▶ {prompt}: ");
            Console.ForegroundColor = ConsoleColor.White;
            string eingabe = Console.ReadLine()?.Trim();
            Console.ResetColor();
            return eingabe;
        }

        private static void ZeigeErfolg(string nachricht, bool mitPause)
        {
            Console.ForegroundColor = ErfolgFarbe;
            Console.WriteLine($"  {nachricht}");
            Console.ResetColor();
            if (mitPause) Thread.Sleep(1500);
        }

        private static void ZeigeFehler(string nachricht)
        {
            Console.ForegroundColor = FehlerFarbe;
            Console.WriteLine($"  {nachricht}");
            Console.ResetColor();
        }

        private static void ZeigeWarnung(string nachricht)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠️  {nachricht}");
            Console.ResetColor();
        }

        private static string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        // ═══════════════════════════════════════════════════════════════
        // TEMPLATE-DATEI-VERWALTUNG
        // ═══════════════════════════════════════════════════════════════

        private static void StelleTemplateVerzeichnisSicher()
        {
            if (!Directory.Exists(TemplateVerzeichnis))
            {
                Directory.CreateDirectory(TemplateVerzeichnis);
            }
        }

        private static List<ArtikelTemplate> LadeTemplates()
        {
            var templates = new List<ArtikelTemplate>();

            if (!File.Exists(TemplateListeDatei))
            {
                return templates;
            }

            try
            {
                string[] zeilen = File.ReadAllLines(TemplateListeDatei, Encoding.UTF8);

                foreach (var zeile in zeilen)
                {
                    if (string.IsNullOrWhiteSpace(zeile) || zeile.StartsWith("#"))
                    {
                        continue;
                    }

                    string[] teile = zeile.Split(';');

                    if (teile.Length >= 4)
                    {
                        templates.Add(new ArtikelTemplate
                        {
                            Name = teile[0],
                            Kategorie = teile[1],
                            Hersteller = teile[2],
                            Preis = decimal.TryParse(teile[3], out decimal p) ? p : 0m
                        });
                    }
                }
            }
            catch { }

            return templates;
        }

        private static void SpeichereTemplates(List<ArtikelTemplate> templates)
        {
            StelleTemplateVerzeichnisSicher();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# ════════════════════════════════════════════════════════════");
            sb.AppendLine("# ARTIKEL-TEMPLATES");
            sb.AppendLine("# Schnellerfassungs-Manager");
            sb.AppendLine("# ════════════════════════════════════════════════════════════");
            sb.AppendLine("# Format: Name;Kategorie;Hersteller;Preis");
            sb.AppendLine("#");
            sb.AppendLine();

            foreach (var template in templates)
            {
                sb.AppendLine($"{template.Name};{template.Kategorie};{template.Hersteller};{template.Preis}");
            }

            File.WriteAllText(TemplateListeDatei, sb.ToString(), Encoding.UTF8);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ARTIKEL-TEMPLATE MODEL
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Repräsentiert ein Artikel-Template für Schnellerfassung
    /// </summary>
    public class ArtikelTemplate
    {
        public string Name { get; set; }
        public string Kategorie { get; set; }
        public string Hersteller { get; set; }
        public decimal Preis { get; set; }
    }
}