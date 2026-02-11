using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet den Import von CSV-Dateien in das Inventarsystem
    /// Unterstützt Import von Inventar, Mitarbeitern und Benutzern
    /// VERBESSERT: Bessere Datei-Zugriffskontrolle und Fehlerbehandlung
    /// </summary>
    public static class CSVImportManager
    {
        /// <summary>
        /// Hauptmenü für CSV-Import
        /// </summary>
        public static void ZeigeImportMenu()
        {
            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("CSV-Import", ConsoleColor.DarkCyan);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  Was möchten Sie importieren?");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1] 📦 Inventar-Artikel importieren");
                Console.WriteLine("  [2] 👥 Mitarbeiter importieren");
                Console.WriteLine("  [3] 👨‍💼 Benutzer importieren");
                Console.WriteLine();
                Console.WriteLine("  [0] ↩️  Zurück zum Hauptmenü");
                Console.ResetColor();

                Console.WriteLine();
                string auswahl = ConsoleHelper.GetInput("Ihre Auswahl");

                switch (auswahl)
                {
                    case "1":
                        ImportiereInventar();
                        break;
                    case "2":
                        ImportiereMitarbeiter();
                        break;
                    case "3":
                        ImportiereBenutzer();
                        break;
                    case "0":
                        return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        ConsoleHelper.PressKeyToContinue();
                        break;
                }
            }
        }

        #region Inventar-Import

        /// <summary>
        /// Importiert Inventar-Artikel aus CSV
        /// Erwartetes Format: InvNr,Gerätename,Mitarbeiter,SNR,Preis,Datum,Hersteller,Kategorie,Anzahl,Mindestbestand
        /// </summary>
        public static void ImportiereInventar()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Inventar-Import aus CSV", ConsoleColor.Green);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📋 Erwartetes CSV-Format:");
            Console.ResetColor();
            Console.WriteLine("  InvNr,Gerätename,Mitarbeiter,SNR,Preis,Datum,Hersteller,Kategorie,Anzahl,Mindestbestand");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Beispiel:");
            Console.WriteLine("  INV001,Laptop Dell,Max Müller,SN123,1299.99,15.01.2025,Dell,IT-Hardware,5,2");
            Console.ResetColor();
            Console.WriteLine();

            // Zeige aktuelles Verzeichnis
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  📂 Aktuelles Verzeichnis: {Directory.GetCurrentDirectory()}");
            Console.ResetColor();
            Console.WriteLine();

            // Zeige CSV-Dateien im aktuellen Verzeichnis
            string[] csvDateien = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            if (csvDateien.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  📄 Gefundene CSV-Dateien:");
                Console.ResetColor();
                for (int i = 0; i < csvDateien.Length; i++)
                {
                    Console.WriteLine($"     [{i + 1}] {Path.GetFileName(csvDateien[i])}");
                }
                Console.WriteLine();
            }

            string csvPfad = ConsoleHelper.GetInput("Pfad zur CSV-Datei, Nummer oder Dateiname (oder 'x' zum Abbrechen)");

            if (csvPfad.ToLower() == "x")
            {
                ConsoleHelper.PrintInfo("Import abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Prüfe ob Nummer eingegeben wurde
            if (int.TryParse(csvPfad, out int nummer) && nummer > 0 && nummer <= csvDateien.Length)
            {
                csvPfad = csvDateien[nummer - 1];
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  ✓ Datei ausgewählt: {Path.GetFileName(csvPfad)}");
                Console.ResetColor();
            }
            // Prüfe ob nur Dateiname (ohne Pfad) eingegeben wurde
            else if (!Path.IsPathRooted(csvPfad) && !csvPfad.Contains("\\") && !csvPfad.Contains("/"))
            {
                string vollerPfad = Path.Combine(Directory.GetCurrentDirectory(), csvPfad);
                if (File.Exists(vollerPfad))
                {
                    csvPfad = vollerPfad;
                }
                else
                {
                    vollerPfad = Path.Combine(Directory.GetCurrentDirectory(), csvPfad + ".csv");
                    if (File.Exists(vollerPfad))
                    {
                        csvPfad = vollerPfad;
                    }
                }
            }

            if (!File.Exists(csvPfad))
            {
                ConsoleHelper.PrintError($"Datei nicht gefunden: {csvPfad}");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  💡 Tipps:");
                Console.WriteLine("     • Geben Sie den vollständigen Pfad an: C:\\Ordner\\datei.csv");
                Console.WriteLine("     • Oder legen Sie die CSV-Datei ins Programmverzeichnis");
                Console.WriteLine("     • Oder wählen Sie die Nummer aus der Liste");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            try
            {
                // Prüfe Dateizugriff mit besserer Fehlerbehandlung
                Console.WriteLine();
                ConsoleHelper.PrintInfo("Prüfe Dateizugriff...");

                string[] lines;
                try
                {
                    // Versuche mit verschiedenen Encodings zu lesen
                    lines = File.ReadAllLines(csvPfad, Encoding.UTF8);
                }
                catch (IOException)
                {
                    // Wenn UTF8 fehlschlägt, versuche Default Encoding
                    try
                    {
                        lines = File.ReadAllLines(csvPfad, Encoding.Default);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        ConsoleHelper.PrintError("Keine Zugriffsberechtigung für die Datei!");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  💡 Lösungsvorschläge:");
                        Console.WriteLine("     • Schließen Sie die Datei in Excel/anderen Programmen");
                        Console.WriteLine("     • Starten Sie das Programm als Administrator");
                        Console.WriteLine("     • Prüfen Sie die Datei-Berechtigungen");
                        Console.WriteLine("     • Kopieren Sie die Datei in einen anderen Ordner");
                        Console.ResetColor();
                        ConsoleHelper.PressKeyToContinue();
                        return;
                    }
                    catch (IOException ex)
                    {
                        ConsoleHelper.PrintError($"Datei ist gesperrt: {ex.Message}");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  💡 Die Datei ist möglicherweise:");
                        Console.WriteLine("     • In Excel oder einem anderen Programm geöffnet");
                        Console.WriteLine("     • Von einem anderen Prozess verwendet");
                        Console.WriteLine("     → Schließen Sie die Datei und versuchen Sie es erneut");
                        Console.ResetColor();
                        ConsoleHelper.PressKeyToContinue();
                        return;
                    }
                }

                if (lines.Length == 0)
                {
                    ConsoleHelper.PrintWarning("CSV-Datei ist leer!");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }

                ConsoleHelper.PrintSuccess($"✓ Datei geladen: {lines.Length} Zeilen gefunden");

                // Prüfe ob erste Zeile Header ist
                bool hatHeader = lines[0].ToLower().Contains("invnr") || lines[0].ToLower().Contains("inventar");
                int startZeile = hatHeader ? 1 : 0;

                if (hatHeader)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  ✓ Header erkannt: {lines[0]}");
                    Console.ResetColor();
                }

                // Statistiken
                int erfolgreich = 0;
                int fehler = 0;
                int duplikate = 0;
                List<string> fehlerListe = new List<string>();

                // Importiere Zeile für Zeile
                Console.WriteLine();
                ConsoleHelper.PrintInfo("Importiere Daten...");
                Console.WriteLine();

                for (int i = startZeile; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        string[] daten = ParseCSVLine(line);

                        if (daten.Length < 10)
                        {
                            fehlerListe.Add($"Zeile {i + 1}: Zu wenige Felder ({daten.Length}/10)");
                            fehler++;
                            continue;
                        }

                        // Prüfe ob bereits vorhanden
                        bool existiert = DataManager.Inventar.Any(a =>
                            a.InvNmr.Equals(daten[0].Trim(), StringComparison.OrdinalIgnoreCase));

                        if (existiert)
                        {
                            fehlerListe.Add($"Zeile {i + 1}: Artikel '{daten[0]}' existiert bereits");
                            duplikate++;
                            continue;
                        }

                        // Parse Daten
                        string invNr = daten[0].Trim();
                        string geraeteName = daten[1].Trim();
                        string mitarbeiter = daten[2].Trim();
                        string snr = daten[3].Trim();
                        decimal preis = ParseDecimal(daten[4].Trim());
                        DateTime datum = ParseDatum(daten[5].Trim());
                        string hersteller = daten[6].Trim();
                        string kategorie = daten[7].Trim();
                        int anzahl = int.Parse(daten[8].Trim());
                        int mindestbestand = int.Parse(daten[9].Trim());

                        // Erstelle Artikel mit Tracking
                        string aktuellerBenutzer = AuthManager.AktuellerBenutzer ?? "CSV-Import";
                        DateTime importZeitpunkt = DateTime.Now;

                        InvId neuerArtikel = new InvId(
                            invNr, geraeteName, mitarbeiter, snr, preis, datum,
                            hersteller, kategorie, anzahl, mindestbestand,
                            aktuellerBenutzer, importZeitpunkt
                        );

                        DataManager.Inventar.Add(neuerArtikel);
                        erfolgreich++;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(".");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        fehlerListe.Add($"Zeile {i + 1}: {ex.Message}");
                        fehler++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("x");
                        Console.ResetColor();
                    }
                }

                // Speichere alle importierten Artikel
                if (erfolgreich > 0)
                {
                    DataManager.SaveKomplettesInventar();
                    IntelligentAssistant.IniializeAI();
                }

                ZeigeImportZusammenfassung(erfolgreich, duplikate, fehler, fehlerListe);

                // Logging
                LogManager.LogDatenGespeichert("CSV-Import", $"Inventar: {erfolgreich} importiert, {duplikate} Duplikate, {fehler} Fehler");

                ConsoleHelper.PressKeyToContinue();
            }
            catch (UnauthorizedAccessException)
            {
                ConsoleHelper.PrintError("Keine Zugriffsberechtigung!");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  💡 Starten Sie das Programm als Administrator");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Importieren: {ex.Message}");
                ConsoleHelper.PressKeyToContinue();
            }
        }

        #endregion

        #region Mitarbeiter-Import

        public static void ImportiereMitarbeiter()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Mitarbeiter-Import aus CSV", ConsoleColor.Blue);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📋 Erwartetes CSV-Format:");
            Console.ResetColor();
            Console.WriteLine("  Vorname,Nachname,Abteilung");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Beispiel:");
            Console.WriteLine("  Max,Mustermann,IT");
            Console.WriteLine("  Anna,Schmidt,Verwaltung");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  📂 Aktuelles Verzeichnis: {Directory.GetCurrentDirectory()}");
            Console.ResetColor();
            Console.WriteLine();

            string[] csvDateien = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            if (csvDateien.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  📄 Gefundene CSV-Dateien:");
                Console.ResetColor();
                for (int i = 0; i < csvDateien.Length; i++)
                {
                    Console.WriteLine($"     [{i + 1}] {Path.GetFileName(csvDateien[i])}");
                }
                Console.WriteLine();
            }

            string csvPfad = ConsoleHelper.GetInput("Pfad zur CSV-Datei, Nummer oder Dateiname (oder 'x' zum Abbrechen)");

            if (csvPfad.ToLower() == "x")
            {
                ConsoleHelper.PrintInfo("Import abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            csvPfad = BehandleDateiPfad(csvPfad, csvDateien);

            if (!File.Exists(csvPfad))
            {
                ZeigeDateiNichtGefundenFehler(csvPfad);
                return;
            }

            try
            {
                string[] lines = LeseDateiMitFehlerbehandlung(csvPfad);
                if (lines == null) return;

                if (lines.Length == 0)
                {
                    ConsoleHelper.PrintWarning("CSV-Datei ist leer!");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }

                ConsoleHelper.PrintSuccess($"✓ Datei geladen: {lines.Length} Zeilen gefunden");

                bool hatHeader = lines[0].ToLower().Contains("vorname") || lines[0].ToLower().Contains("name");
                int startZeile = hatHeader ? 1 : 0;

                if (hatHeader)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  ✓ Header erkannt: {lines[0]}");
                    Console.ResetColor();
                }

                int erfolgreich = 0;
                int fehler = 0;
                int duplikate = 0;
                List<string> fehlerListe = new List<string>();

                Console.WriteLine();
                ConsoleHelper.PrintInfo("Importiere Daten...");
                Console.WriteLine();

                for (int i = startZeile; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        string[] daten = ParseCSVLine(line);

                        if (daten.Length < 3)
                        {
                            fehlerListe.Add($"Zeile {i + 1}: Zu wenige Felder ({daten.Length}/3)");
                            fehler++;
                            continue;
                        }

                        string vorname = daten[0].Trim();
                        string nachname = daten[1].Trim();
                        string abteilung = daten[2].Trim();

                        bool existiert = DataManager.Mitarbeiter.Any(m =>
                            m.VName.Equals(vorname, StringComparison.OrdinalIgnoreCase) &&
                            m.NName.Equals(nachname, StringComparison.OrdinalIgnoreCase));

                        if (existiert)
                        {
                            fehlerListe.Add($"Zeile {i + 1}: {vorname} {nachname} existiert bereits");
                            duplikate++;
                            continue;
                        }

                        MID neuerMitarbeiter = new MID(vorname, nachname, abteilung);
                        DataManager.Mitarbeiter.Add(neuerMitarbeiter);
                        erfolgreich++;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(".");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        fehlerListe.Add($"Zeile {i + 1}: {ex.Message}");
                        fehler++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("x");
                        Console.ResetColor();
                    }
                }

                if (erfolgreich > 0)
                {
                    DataManager.SaveKompletteMitarbeiter();
                    IntelligentAssistant.IniializeAI();
                }

                ZeigeImportZusammenfassung(erfolgreich, duplikate, fehler, fehlerListe);
                LogManager.LogDatenGespeichert("CSV-Import", $"Mitarbeiter: {erfolgreich} importiert, {duplikate} Duplikate, {fehler} Fehler");

                ConsoleHelper.PressKeyToContinue();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Importieren: {ex.Message}");
                ConsoleHelper.PressKeyToContinue();
            }
        }

        #endregion

        #region Benutzer-Import

        public static void ImportiereBenutzer()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Benutzer-Import aus CSV", ConsoleColor.Magenta);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📋 Erwartetes CSV-Format:");
            Console.ResetColor();
            Console.WriteLine("  Benutzername,Berechtigung");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Beispiel:");
            Console.WriteLine("  admin,Admin");
            Console.WriteLine("  user01,User");
            Console.WriteLine();
            Console.WriteLine("  Erlaubte Berechtigungen: User, Admin");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  📂 Aktuelles Verzeichnis: {Directory.GetCurrentDirectory()}");
            Console.ResetColor();
            Console.WriteLine();

            string[] csvDateien = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            if (csvDateien.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  📄 Gefundene CSV-Dateien:");
                Console.ResetColor();
                for (int i = 0; i < csvDateien.Length; i++)
                {
                    Console.WriteLine($"     [{i + 1}] {Path.GetFileName(csvDateien[i])}");
                }
                Console.WriteLine();
            }

            string csvPfad = ConsoleHelper.GetInput("Pfad zur CSV-Datei, Nummer oder Dateiname (oder 'x' zum Abbrechen)");

            if (csvPfad.ToLower() == "x")
            {
                ConsoleHelper.PrintInfo("Import abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            csvPfad = BehandleDateiPfad(csvPfad, csvDateien);

            if (!File.Exists(csvPfad))
            {
                ZeigeDateiNichtGefundenFehler(csvPfad);
                return;
            }

            try
            {
                string[] lines = LeseDateiMitFehlerbehandlung(csvPfad);
                if (lines == null) return;

                if (lines.Length == 0)
                {
                    ConsoleHelper.PrintWarning("CSV-Datei ist leer!");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }

                ConsoleHelper.PrintSuccess($"✓ Datei geladen: {lines.Length} Zeilen gefunden");

                bool hatHeader = lines[0].ToLower().Contains("benutzer") || lines[0].ToLower().Contains("user");
                int startZeile = hatHeader ? 1 : 0;

                if (hatHeader)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"  ✓ Header erkannt: {lines[0]}");
                    Console.ResetColor();
                }

                int erfolgreich = 0;
                int fehler = 0;
                int duplikate = 0;
                List<string> fehlerListe = new List<string>();

                Console.WriteLine();
                ConsoleHelper.PrintInfo("Importiere Daten...");
                Console.WriteLine();

                for (int i = startZeile; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        string[] daten = ParseCSVLine(line);

                        if (daten.Length < 2)
                        {
                            fehlerListe.Add($"Zeile {i + 1}: Zu wenige Felder ({daten.Length}/2)");
                            fehler++;
                            continue;
                        }

                        string benutzername = daten[0].Trim();
                        string berechtigungText = daten[1].Trim();

                        bool existiert = DataManager.Benutzer.Any(b =>
                            b.Benutzername.Equals(benutzername, StringComparison.OrdinalIgnoreCase));

                        if (existiert)
                        {
                            fehlerListe.Add($"Zeile {i + 1}: Benutzer '{benutzername}' existiert bereits");
                            duplikate++;
                            continue;
                        }

                        if (!Enum.TryParse(berechtigungText, true, out Berechtigungen berechtigung))
                        {
                            fehlerListe.Add($"Zeile {i + 1}: Ungültige Berechtigung '{berechtigungText}' (erlaubt: User, Admin)");
                            fehler++;
                            continue;
                        }

                        Accounts neuerBenutzer = new Accounts(benutzername, berechtigung);
                        DataManager.Benutzer.Add(neuerBenutzer);
                        erfolgreich++;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(".");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        fehlerListe.Add($"Zeile {i + 1}: {ex.Message}");
                        fehler++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("x");
                        Console.ResetColor();
                    }
                }

                if (erfolgreich > 0)
                {
                    DataManager.SaveBenutzerToFile();
                }

                ZeigeImportZusammenfassung(erfolgreich, duplikate, fehler, fehlerListe);
                LogManager.LogDatenGespeichert("CSV-Import", $"Benutzer: {erfolgreich} importiert, {duplikate} Duplikate, {fehler} Fehler");

                ConsoleHelper.PressKeyToContinue();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Importieren: {ex.Message}");
                ConsoleHelper.PressKeyToContinue();
            }
        }

        #endregion

        #region Hilfsfunktionen

        /// <summary>
        /// Liest Datei mit umfassender Fehlerbehandlung
        /// </summary>
        private static string[] LeseDateiMitFehlerbehandlung(string csvPfad)
        {
            Console.WriteLine();
            ConsoleHelper.PrintInfo("Prüfe Dateizugriff...");

            try
            {
                return File.ReadAllLines(csvPfad, Encoding.UTF8);
            }
            catch (IOException)
            {
                try
                {
                    return File.ReadAllLines(csvPfad, Encoding.Default);
                }
                catch (UnauthorizedAccessException)
                {
                    ConsoleHelper.PrintError("Keine Zugriffsberechtigung für die Datei!");
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  💡 Lösungsvorschläge:");
                    Console.WriteLine("     • Schließen Sie die Datei in Excel/anderen Programmen");
                    Console.WriteLine("     • Starten Sie das Programm als Administrator");
                    Console.WriteLine("     • Prüfen Sie die Datei-Berechtigungen (Rechtsklick → Eigenschaften)");
                    Console.WriteLine("     • Kopieren Sie die Datei in einen anderen Ordner");
                    Console.ResetColor();
                    ConsoleHelper.PressKeyToContinue();
                    return null;
                }
                catch (IOException ex)
                {
                    ConsoleHelper.PrintError($"Datei ist gesperrt: {ex.Message}");
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  💡 Die Datei ist möglicherweise:");
                    Console.WriteLine("     • In Excel oder einem anderen Programm geöffnet");
                    Console.WriteLine("     • Von einem anderen Prozess verwendet");
                    Console.WriteLine("     → Schließen Sie die Datei und versuchen Sie es erneut");
                    Console.ResetColor();
                    ConsoleHelper.PressKeyToContinue();
                    return null;
                }
            }
        }

        /// <summary>
        /// Behandelt verschiedene Dateipfad-Formate
        /// </summary>
        private static string BehandleDateiPfad(string csvPfad, string[] verfuegbareDateien)
        {
            if (int.TryParse(csvPfad, out int nummer) && nummer > 0 && nummer <= verfuegbareDateien.Length)
            {
                csvPfad = verfuegbareDateien[nummer - 1];
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  ✓ Datei ausgewählt: {Path.GetFileName(csvPfad)}");
                Console.ResetColor();
            }
            else if (!Path.IsPathRooted(csvPfad) && !csvPfad.Contains("\\") && !csvPfad.Contains("/"))
            {
                string vollerPfad = Path.Combine(Directory.GetCurrentDirectory(), csvPfad);
                if (File.Exists(vollerPfad))
                {
                    csvPfad = vollerPfad;
                }
                else
                {
                    vollerPfad = Path.Combine(Directory.GetCurrentDirectory(), csvPfad + ".csv");
                    if (File.Exists(vollerPfad))
                    {
                        csvPfad = vollerPfad;
                    }
                }
            }
            return csvPfad;
        }

        /// <summary>
        /// Zeigt Fehler wenn Datei nicht gefunden wurde
        /// </summary>
        private static void ZeigeDateiNichtGefundenFehler(string csvPfad)
        {
            ConsoleHelper.PrintError($"Datei nicht gefunden: {csvPfad}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  💡 Tipps:");
            Console.WriteLine("     • Geben Sie den vollständigen Pfad an: C:\\Ordner\\datei.csv");
            Console.WriteLine("     • Oder legen Sie die CSV-Datei ins Programmverzeichnis");
            Console.WriteLine("     • Oder wählen Sie die Nummer aus der Liste");
            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt Import-Zusammenfassung
        /// </summary>
        private static void ZeigeImportZusammenfassung(int erfolgreich, int duplikate, int fehler, List<string> fehlerListe)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                    IMPORT ABGESCHLOSSEN                           ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Erfolgreich importiert: {erfolgreich}");
            Console.ResetColor();

            if (duplikate > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠️  Duplikate übersprungen: {duplikate}");
                Console.ResetColor();
            }

            if (fehler > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Fehler: {fehler}");
                Console.ResetColor();
            }

            if (fehlerListe.Count > 0 && fehlerListe.Count <= 10)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  📋 Fehlerdetails:");
                Console.ResetColor();
                foreach (var fehlerMsg in fehlerListe)
                {
                    Console.WriteLine($"     • {fehlerMsg}");
                }
            }
            else if (fehlerListe.Count > 10)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  📋 {fehlerListe.Count} Fehler aufgetreten (zu viele zum Anzeigen)");
                Console.ResetColor();
            }
        }

        private static string[] ParseCSVLine(string line)
        {
            List<string> felder = new List<string>();
            bool inQuotes = false;
            string aktuellesFeld = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    felder.Add(aktuellesFeld.Trim());
                    aktuellesFeld = "";
                }
                else
                {
                    aktuellesFeld += c;
                }
            }

            felder.Add(aktuellesFeld.Trim());
            return felder.ToArray();
        }

        private static decimal ParseDecimal(string wert)
        {
            wert = wert.Replace(",", ".");

            if (decimal.TryParse(wert, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal ergebnis))
            {
                return ergebnis;
            }

            throw new FormatException($"Ungültiger Dezimalwert: {wert}");
        }

        private static DateTime ParseDatum(string datum)
        {
            string[] formate = {
                "dd.MM.yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d.M.yyyy", "dd.MM.yy"
            };

            foreach (var format in formate)
            {
                if (DateTime.TryParseExact(datum, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ergebnis))
                {
                    return ergebnis;
                }
            }

            if (DateTime.TryParse(datum, out DateTime fallback))
            {
                return fallback;
            }

            throw new FormatException($"Ungültiges Datumsformat: {datum}");
        }

        #endregion
    }
}