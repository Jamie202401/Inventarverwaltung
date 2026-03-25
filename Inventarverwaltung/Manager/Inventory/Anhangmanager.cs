using Inventarverwaltung.Manager.UI;
using Inventarverwaltung.Manager.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Inventarverwaltung.Security;

namespace Inventarverwaltung
{
    /// <summary>
    /// ╔══════════════════════════════════════════════════════════════════════╗
    /// ║                       ANHANG-MANAGER                                ║
    /// ╠══════════════════════════════════════════════════════════════════════╣
    /// ║  Verwaltet Datei-Anhänge für Inventar-Artikel und Mitarbeiter.      ║
    /// ║                                                                      ║
    /// ║  ORDNER-STRUKTUR:                                                    ║
    /// ║    Anhänge/                                                          ║
    /// ║    ├── Artikel/                                                      ║
    /// ║    │   ├── INV001/    ← Inventarnummer als Ordnername               ║
    /// ║    │   │   ├── Rechnung.pdf                                          ║
    /// ║    │   │   └── Foto.jpg                                              ║
    /// ║    │   └── INV002/                                                   ║
    /// ║    └── Mitarbeiter/                                                  ║
    /// ║        ├── Mueller_Max/   ← NName_VName als Ordnername              ║
    /// ║        │   └── Vertrag.pdf                                           ║
    /// ║        └── Schmidt_Anna/                                             ║
    /// ║                                                                      ║
    /// ║  SICHERHEIT:                                                         ║
    /// ║    • Dateien werden mit SHA-256 gehasht (Manipulationserkennung)    ║
    /// ║    • Hash-Manifest wird separat gespeichert                          ║
    /// ║    • Dateien werden als ReadOnly gesetzt nach dem Kopieren           ║
    /// ║    • Originalquelle wird NICHT verändert oder gelöscht               ║
    /// ╚══════════════════════════════════════════════════════════════════════╝
    /// </summary>
    public static class AnhangManager
    {
        // ══════════════════════════════════════════════════════════════════
        // KONFIGURATION & PFADE
        // ══════════════════════════════════════════════════════════════════

        private const string BasisOrdner = "Anhänge";
        private const string ArtikelUnterOrdner = "Artikel";
        private const string MaUnterOrdner = "Mitarbeiter";
        private const string ManifestDateiname = "_manifest.sha256";

        // Erlaubte Dateitypen (Whitelist für Sicherheit)
        private static readonly HashSet<string> ErlaubteEndungen = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".csv", ".txt",
            ".png",  ".jpg", ".jpeg", ".gif", ".bmp", ".tiff",
            ".zip",  ".7z",  ".rar",
            ".mp4",  ".avi", ".mkv",
            ".msg",  ".eml"
        };

        // Maximale Dateigröße: 500 MB
        private const long MaxDateigroesseBytes = 500L * 1024 * 1024;

        // ══════════════════════════════════════════════════════════════════
        // INITIALISIERUNG
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Erstellt die Ordnerstruktur beim Programmstart (falls nicht vorhanden).
        /// </summary>
        public static void Initialisieren()
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(BasisOrdner, ArtikelUnterOrdner));
                Directory.CreateDirectory(Path.Combine(BasisOrdner, MaUnterOrdner));
            }
            catch (Exception ex)
            {
                LogManager.LogFehler("AnhangManager.Initialisieren", ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // STARTMENÜ
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Zeigt das Hauptmenü des AnhangManagers an.
        /// </summary>
        public static void StarteAnhangMenu()
        {
            while (true)
            {
                Console.Clear();
                ZeigeAnhangStartMenu();

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\n  ▶ Ihre Auswahl: ");
                Console.ResetColor();
                string eingabe = Console.ReadLine()?.Trim().ToUpper() ?? string.Empty;

                switch (eingabe)
                {
                    case "1":
                        AnhangZuArtikelHinzufuegen();
                        break;
                    case "2":
                        AnhangZuMitarbeiterHinzufuegen();
                        break;
                    case "3":
                        AnhaengeVonArtikelAnzeigen();
                        break;
                    case "4":
                        AnhaengeVonMitarbeiterAnzeigen();
                        break;
                    case "5":
                        IntegritaetPruefen();
                        break;
                    case "0":
                    case "Q":
                        return;
                    default:
                        ConsoleHelper.PrintWarning("Ungültige Eingabe. Bitte wählen Sie 1–5 oder 0.");
                        ConsoleHelper.PressKeyToContinue();
                        break;
                }
            }
        }

        /// <summary>
        /// Rendert das schön gestaltete Startmenü des AnhangManagers.
        /// </summary>
        private static void ZeigeAnhangStartMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                      ║");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ║          📎   A N H A N G - V E R W A L T U N G   📎               ║");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ║                                                                      ║");
            Console.WriteLine("  ╠══════════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("  ║                                                                      ║");
            Console.ResetColor();

            // Menüpunkte
            DruckeMenuPunkt("1", "📥", "Anhang zu Artikel hinzufügen", ConsoleColor.Green);
            DruckeMenuPunkt("2", "📥", "Anhang zu Mitarbeiter hinzufügen", ConsoleColor.Magenta);
            DruckeMenuPunkt("3", "📂", "Anhänge eines Artikels anzeigen", ConsoleColor.Green);
            DruckeMenuPunkt("4", "📂", "Anhänge eines Mitarbeiters anzeigen", ConsoleColor.Magenta);

            // Trennlinie
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ║  ─────────────────────────────────────────────────────────────────  ║");
            Console.ResetColor();

            DruckeMenuPunkt("5", "🔐", "Integrität aller Anhänge prüfen", ConsoleColor.Yellow);

            // Trennlinie
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ║  ─────────────────────────────────────────────────────────────────  ║");
            Console.ResetColor();

            DruckeMenuPunkt("0", "🚪", "Zurück zum Hauptmenü", ConsoleColor.DarkGray);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ║                                                                      ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            // Status-Zeile
            int artikelAnhaenge = ZaehleAnhaenge(Path.Combine(BasisOrdner, ArtikelUnterOrdner));
            int maAnhaenge = ZaehleAnhaenge(Path.Combine(BasisOrdner, MaUnterOrdner));

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n  📊 Gespeicherte Anhänge:  " +
                              $"Artikel: {artikelAnhaenge}  |  Mitarbeiter: {maAnhaenge}");
            Console.ResetColor();
        }

        /// <summary>
        /// Gibt eine einzelne Menüzeile im einheitlichen Box-Stil aus.
        /// </summary>
        private static void DruckeMenuPunkt(string taste, string icon, string text, ConsoleColor farbe)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("  ║   ");

            // Taste
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = farbe;
            Console.Write(taste);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]");

            // Icon + Text
            Console.ForegroundColor = farbe;
            Console.Write($"  {icon}  {text}");

            // Auffüllen bis Rahmenende
            int genutzt = 3 + 3 + 2 + 1 + text.Length + 5 + icon.Length;
            // Feste Breite: 72 Zeichen innen (74 - 2 für ║ und Leerzeichen am Rand)
            int padding = 68 - text.Length - icon.Length;
            if (padding > 0)
                Console.Write(new string(' ', padding));

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("║");
            Console.ResetColor();
        }

        /// <summary>
        /// Zählt alle Anhang-Dateien in einem Unterordner (ohne Manifeste).
        /// </summary>
        private static int ZaehleAnhaenge(string ordner)
        {
            if (!Directory.Exists(ordner)) return 0;
            return Directory.GetFiles(ordner, "*", SearchOption.AllDirectories)
                .Count(f => !Path.GetFileName(f).StartsWith("_"));
        }

        // ══════════════════════════════════════════════════════════════════
        // ANHÄNGE HINZUFÜGEN
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Fügt einer Inventar-Artikel-Akte eine oder mehrere Dateien hinzu.
        /// Artikel werden nummeriert angezeigt – keine manuelle INV-Nr. nötig.
        /// </summary>
        public static void AnhangZuArtikelHinzufuegen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("📥 Anhang zu Artikel hinzufügen", ConsoleColor.Green);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Artikel im Inventar vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            var artikel = ArtikelPerNummerAuswaehlen();
            if (artikel == null) return;

            string zielOrdner = HoleArtikelOrdner(artikel.InvNmr);
            DateiHinzufuegenDialog(zielOrdner, $"Artikel [{artikel.InvNmr}] {artikel.GeraeteName}");
        }

        /// <summary>
        /// Fügt einem Mitarbeiter-Profil eine oder mehrere Dateien hinzu.
        /// </summary>
        public static void AnhangZuMitarbeiterHinzufuegen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("📥 Anhang zu Mitarbeiter hinzufügen", ConsoleColor.Magenta);

            if (DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Mitarbeiter vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Mitarbeiter nummeriert auswählen
            MID mitarbeiter = MitarbeiterPerNummerAuswaehlen();
            if (mitarbeiter == null) return;

            string zielOrdner = HoleMitarbeiterOrdner(mitarbeiter);
            DateiHinzufuegenDialog(zielOrdner,
                $"Mitarbeiter {mitarbeiter.VName} {mitarbeiter.NName}");
        }

        // ══════════════════════════════════════════════════════════════════
        // ANHÄNGE ANZEIGEN & ÖFFNEN
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Zeigt alle Anhänge eines Inventar-Artikels und ermöglicht das Öffnen.
        /// </summary>
        public static void AnhaengeVonArtikelAnzeigen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("📂 Anhänge eines Artikels anzeigen", ConsoleColor.Green);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Artikel im Inventar vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            var artikel = ArtikelPerNummerAuswaehlen();
            if (artikel == null) return;

            string ordner = HoleArtikelOrdner(artikel.InvNmr);
            AnhaengeAnzeigenUndOeffnen(ordner,
                $"Artikel [{artikel.InvNmr}] {artikel.GeraeteName}");
        }

        /// <summary>
        /// Zeigt alle Anhänge eines Mitarbeiters und ermöglicht das Öffnen.
        /// </summary>
        public static void AnhaengeVonMitarbeiterAnzeigen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("📂 Anhänge eines Mitarbeiters anzeigen", ConsoleColor.Magenta);

            if (DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Mitarbeiter vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            MID mitarbeiter = MitarbeiterPerNummerAuswaehlen();
            if (mitarbeiter == null) return;

            string ordner = HoleMitarbeiterOrdner(mitarbeiter);
            AnhaengeAnzeigenUndOeffnen(ordner,
                $"Mitarbeiter {mitarbeiter.VName} {mitarbeiter.NName}");
        }

        // ══════════════════════════════════════════════════════════════════
        // INTEGRITÄT PRÜFEN
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Prüft alle gespeicherten Anhänge auf Manipulation anhand der SHA-256-Hashes.
        /// </summary>
        public static void IntegritaetPruefen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🔐 Anhang-Integrität prüfen", ConsoleColor.Yellow);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  Scanne alle Anhänge auf Manipulation ...");
            Console.ResetColor();
            Console.WriteLine();

            int geprueft = 0;
            int bestanden = 0;
            int fehlerhaft = 0;

            string basisPfad = BasisOrdner;
            if (!Directory.Exists(basisPfad))
            {
                ConsoleHelper.PrintInfo("Noch keine Anhänge vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            foreach (string unterOrdner in Directory.GetDirectories(basisPfad, "*",
                         SearchOption.AllDirectories))
            {
                string manifestPfad = Path.Combine(unterOrdner, ManifestDateiname);
                if (!File.Exists(manifestPfad)) continue;

                var hashes = LadeManifest(manifestPfad);

                foreach (var eintrag in hashes)
                {
                    string dateiPfad = Path.Combine(unterOrdner, eintrag.Key);
                    geprueft++;

                    if (!File.Exists(dateiPfad))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  ✗ FEHLT:       {eintrag.Key}");
                        Console.ResetColor();
                        fehlerhaft++;
                        continue;
                    }

                    string aktuellerHash = BerechneHash(dateiPfad);
                    if (aktuellerHash == eintrag.Value)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ OK:          {eintrag.Key}");
                        Console.ResetColor();
                        bestanden++;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  ✗ MANIPULIERT: {eintrag.Key}");
                        Console.ResetColor();
                        fehlerhaft++;
                        LogManager.LogFehler("Integrität", $"MANIPULATION ERKANNT: {dateiPfad}");
                    }
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ─── Ergebnis ───────────────────────────────────────────");
            Console.ResetColor();

            if (geprueft == 0)
            {
                ConsoleHelper.PrintInfo("Keine Anhänge mit Integritätsprüfung gefunden.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  ✓ Bestanden:   {bestanden} von {geprueft} Dateien");
                if (fehlerhaft > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ Fehlerhaft:  {fehlerhaft} Dateien (manipuliert oder gelöscht!)");
                }
                Console.ResetColor();
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════════
        // NUMMERIERTE AUSWAHLMENÜS
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Zeigt alle Artikel nummeriert an und gibt den ausgewählten zurück.
        /// Gibt null zurück, wenn der Benutzer abbricht.
        /// </summary>
        private static InvId ArtikelPerNummerAuswaehlen()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ┌────┬────────────────┬──────────────────────────────────────┬──────────────────┐");
            Console.WriteLine("  │ Nr │  Inv.-Nummer   │  Gerätename                          │  Kategorie       │");
            Console.WriteLine("  ├────┼────────────────┼──────────────────────────────────────┼──────────────────┤");
            Console.ResetColor();

            for (int i = 0; i < DataManager.Inventar.Count; i++)
            {
                var a = DataManager.Inventar[i];

                // Abwechselnde Zeilenfarbe für bessere Lesbarkeit
                Console.ForegroundColor = (i % 2 == 0) ? ConsoleColor.White : ConsoleColor.Gray;

                string nr = (i + 1).ToString().PadLeft(2);
                string invNr = TruncateOrPad(a.InvNmr, 14);
                string name = TruncateOrPad(a.GeraeteName, 38);
                string kategorie = TruncateOrPad(a.Kategorie ?? "-", 16);

                Console.WriteLine($"  │ {nr} │ {invNr} │ {name} │ {kategorie} │");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  └────┴────────────────┴──────────────────────────────────────┴──────────────────┘");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [0] = Abbrechen");
            Console.ResetColor();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\n  ▶ Nummer des Artikels: ");
                Console.ResetColor();

                string eingabe = Console.ReadLine()?.Trim() ?? string.Empty;

                if (eingabe == "0" || string.IsNullOrWhiteSpace(eingabe))
                {
                    ConsoleHelper.PrintWarning("Abgebrochen.");
                    ConsoleHelper.PressKeyToContinue();
                    return null;
                }

                if (int.TryParse(eingabe, out int nr) && nr >= 1 && nr <= DataManager.Inventar.Count)
                {
                    var gewählt = DataManager.Inventar[nr - 1];
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n  ✔ Ausgewählt: [{gewählt.InvNmr}] {gewählt.GeraeteName}");
                    Console.ResetColor();
                    return gewählt;
                }

                ConsoleHelper.PrintError($"Ungültige Nummer. Bitte 1–{DataManager.Inventar.Count} eingeben.");
            }
        }

        /// <summary>
        /// Zeigt alle Mitarbeiter nummeriert an und gibt den ausgewählten zurück.
        /// Gibt null zurück, wenn der Benutzer abbricht.
        /// </summary>
        private static MID MitarbeiterPerNummerAuswaehlen()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ┌────┬──────────────────────────┬──────────────────────────────────────┐");
            Console.WriteLine("  │ Nr │  Name                    │  Abteilung                           │");
            Console.WriteLine("  ├────┼──────────────────────────┼──────────────────────────────────────┤");
            Console.ResetColor();

            for (int i = 0; i < DataManager.Mitarbeiter.Count; i++)
            {
                var ma = DataManager.Mitarbeiter[i];

                Console.ForegroundColor = (i % 2 == 0) ? ConsoleColor.White : ConsoleColor.Gray;

                string nr = (i + 1).ToString().PadLeft(2);
                string name = TruncateOrPad($"{ma.VName} {ma.NName}", 26);
                string abteilung = TruncateOrPad(ma.Abteilung ?? "-", 38);

                Console.WriteLine($"  │ {nr} │ {name} │ {abteilung} │");
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  └────┴──────────────────────────┴──────────────────────────────────────┘");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [0] = Abbrechen");
            Console.ResetColor();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\n  ▶ Nummer des Mitarbeiters: ");
                Console.ResetColor();

                string eingabe = Console.ReadLine()?.Trim() ?? string.Empty;

                if (eingabe == "0" || string.IsNullOrWhiteSpace(eingabe))
                {
                    ConsoleHelper.PrintWarning("Abgebrochen.");
                    ConsoleHelper.PressKeyToContinue();
                    return null;
                }

                if (int.TryParse(eingabe, out int nr) && nr >= 1 && nr <= DataManager.Mitarbeiter.Count)
                {
                    var gewählt = DataManager.Mitarbeiter[nr - 1];
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n  ✔ Ausgewählt: {gewählt.VName} {gewählt.NName}");
                    Console.ResetColor();
                    return gewählt;
                }

                ConsoleHelper.PrintError($"Ungültige Nummer. Bitte 1–{DataManager.Mitarbeiter.Count} eingeben.");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE HILFSMETHODEN — DIALOG & UI
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Interaktiver Dialog zum Hinzufügen von Dateien per Drag & Drop oder Pfadeingabe.
        /// </summary>
        private static void DateiHinzufuegenDialog(string zielOrdner, string kontextName)
        {
            Directory.CreateDirectory(zielOrdner);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Ziel: {kontextName}");
            Console.WriteLine($"  Ordner: {Path.GetFullPath(zielOrdner)}");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║  💡 Drag & Drop:                                              ║");
            Console.WriteLine("  ║     Ziehen Sie eine Datei direkt in dieses Fenster           ║");
            Console.WriteLine("  ║     und drücken Sie ENTER. Der Pfad wird automatisch         ║");
            Console.WriteLine("  ║     eingefügt.                                               ║");
            Console.WriteLine("  ║                                                               ║");
            Console.WriteLine("  ║  Alternativ: Dateipfad manuell eingeben.                     ║");
            Console.WriteLine("  ║  Mehrere Dateien: Jeden Pfad einzeln bestätigen.              ║");
            Console.WriteLine("  ║  Fertig: Leere Eingabe + ENTER                               ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            int hinzugefuegt = 0;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  Datei #{hinzugefuegt + 1} (leer = fertig): ");
                Console.ResetColor();

                string eingabe = Console.ReadLine()?.Trim()?.Trim('"') ?? string.Empty;

                if (string.IsNullOrWhiteSpace(eingabe))
                {
                    if (hinzugefuegt == 0)
                        ConsoleHelper.PrintWarning("Keine Datei hinzugefügt.");
                    else
                        ConsoleHelper.PrintSuccess($"✓ {hinzugefuegt} Datei(en) erfolgreich hinzugefügt.");
                    break;
                }

                var pfade = eingabe.Split(new[] { '\n', '\r' },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (string rohPfad in pfade)
                {
                    string pfad = rohPfad.Trim().Trim('"');
                    if (string.IsNullOrWhiteSpace(pfad)) continue;

                    bool erfolg = DateiKopieren(pfad, zielOrdner);
                    if (erfolg) hinzugefuegt++;
                }
            }

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Kopiert eine einzelne Datei in den Zielordner mit Validierung, Sicherheitsscan
        /// und Hash-Sicherung.
        /// </summary>
        private static bool DateiKopieren(string quellPfad, string zielOrdner)
        {
            if (!File.Exists(quellPfad))
            {
                ConsoleHelper.PrintError($"Datei nicht gefunden: {quellPfad}");
                return false;
            }

            var info = new FileInfo(quellPfad);

            if (!ErlaubteEndungen.Contains(info.Extension))
            {
                ConsoleHelper.PrintError(
                    $"Dateityp '{info.Extension}' ist nicht erlaubt. " +
                    $"Erlaubt: {string.Join(", ", ErlaubteEndungen)}");
                return false;
            }

            if (info.Length > MaxDateigroesseBytes)
            {
                ConsoleHelper.PrintError(
                    $"Datei zu groß: {info.Length / 1024 / 1024} MB " +
                    $"(Maximum: {MaxDateigroesseBytes / 1024 / 1024} MB)");
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  🔍 Scanne Datei auf Sicherheitsrisiken: {info.Name} ...");
            Console.ResetColor();

            SicherheitsBefund befund = DateiSicherheitsScanner.DateiScannen(quellPfad);
            bool darfFortfahren = DateiSicherheitsScanner.BefundAnzeigenUndEntscheiden(befund);

            if (!darfFortfahren)
                return false;

            string zielName = SichereDateiname(info.Name);
            string zielPfad = Path.Combine(zielOrdner, zielName);

            if (File.Exists(zielPfad))
            {
                string basisName = Path.GetFileNameWithoutExtension(zielName);
                string endung = Path.GetExtension(zielName);
                int zaehler = 1;

                do
                {
                    zielName = $"{basisName}_{zaehler}{endung}";
                    zielPfad = Path.Combine(zielOrdner, zielName);
                    zaehler++;
                }
                while (File.Exists(zielPfad));
            }

            string hash;
            try
            {
                hash = BerechneHash(quellPfad);
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Hash-Berechnung fehlgeschlagen: {ex.Message}");
                return false;
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"  Kopiere: {info.Name} ...");
                Console.ResetColor();

                File.Copy(quellPfad, zielPfad, overwrite: false);

                File.SetAttributes(zielPfad,
                    File.GetAttributes(zielPfad) | FileAttributes.ReadOnly);

                HashInManifestSpeichern(zielOrdner, zielName, hash);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($" ✓  →  {zielName}");
                Console.ResetColor();

                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" ✗");
                Console.ResetColor();
                ConsoleHelper.PrintError($"Kopieren fehlgeschlagen: {ex.Message}");
                LogManager.LogFehler("AnhangManager.DateiKopieren", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Zeigt alle Anhänge eines Ordners mit Integritätsstatus und ermöglicht das Öffnen.
        /// </summary>
        private static void AnhaengeAnzeigenUndOeffnen(string ordner, string kontextName)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  📂 Anhänge von: {kontextName}");
            Console.ResetColor();
            Console.WriteLine();

            if (!Directory.Exists(ordner))
            {
                ConsoleHelper.PrintInfo("Noch keine Anhänge vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            var dateien = Directory.GetFiles(ordner)
                .Where(f => !Path.GetFileName(f).StartsWith("_"))
                .OrderBy(f => f)
                .ToList();

            if (dateien.Count == 0)
            {
                ConsoleHelper.PrintInfo("Noch keine Anhänge vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            string manifestPfad = Path.Combine(ordner, ManifestDateiname);
            var gespeicherteHashes = File.Exists(manifestPfad)
                ? LadeManifest(manifestPfad)
                : new Dictionary<string, string>();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {"#",-4} {"Dateiname",-35} {"Größe",-12} {"Status",-14} {"Datum"}");
            Console.WriteLine($"  {new string('─', 4)} {new string('─', 35)} {new string('─', 12)} {new string('─', 14)} {new string('─', 20)}");
            Console.ResetColor();

            for (int i = 0; i < dateien.Count; i++)
            {
                var di = new FileInfo(dateien[i]);
                string dateiname = di.Name;
                string groesse = FormatiereDateigroesse(di.Length);
                string datum = di.LastWriteTime.ToString("dd.MM.yyyy HH:mm");

                string status;
                ConsoleColor statusFarbe;

                if (gespeicherteHashes.TryGetValue(dateiname, out string gespeicherterHash))
                {
                    try
                    {
                        var attr = di.Attributes;
                        di.Attributes = attr & ~FileAttributes.ReadOnly;
                        string aktuellerHash = BerechneHash(dateien[i]);
                        di.Attributes = attr;

                        if (aktuellerHash == gespeicherterHash)
                        {
                            status = "✓ Unverändert";
                            statusFarbe = ConsoleColor.Green;
                        }
                        else
                        {
                            status = "⚠ VERÄNDERT!";
                            statusFarbe = ConsoleColor.Red;
                        }
                    }
                    catch
                    {
                        status = "? Prüffehler";
                        statusFarbe = ConsoleColor.Yellow;
                    }
                }
                else
                {
                    status = "– Kein Hash";
                    statusFarbe = ConsoleColor.Gray;
                }

                Console.ForegroundColor = statusFarbe;
                Console.WriteLine($"  {i + 1,-4} {dateiname,-35} {groesse,-12} {status,-14} {datum}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  Ordner: {Path.GetFullPath(ordner)}");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Optionen:");
            Console.ResetColor();
            Console.WriteLine("  [Nummer] → Datei öffnen");
            Console.WriteLine("  [O]      → Ordner im Explorer öffnen");
            Console.WriteLine("  [ENTER]  → Zurück");
            Console.WriteLine();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  Auswahl: ");
                Console.ResetColor();
                string auswahl = Console.ReadLine()?.Trim().ToUpper() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(auswahl)) break;

                if (auswahl == "O")
                {
                    OrdnerOeffnen(ordner);
                    break;
                }

                if (int.TryParse(auswahl, out int nr) && nr >= 1 && nr <= dateien.Count)
                {
                    DateiOeffnen(dateien[nr - 1]);
                    break;
                }

                ConsoleHelper.PrintError("Ungültige Eingabe.");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE HILFSMETHODEN — PFADE
        // ══════════════════════════════════════════════════════════════════

        private static string HoleArtikelOrdner(string invNr)
        {
            string sichererName = SichererOrdnername(invNr);
            return Path.Combine(BasisOrdner, ArtikelUnterOrdner, sichererName);
        }

        private static string HoleMitarbeiterOrdner(MID ma)
        {
            string name = $"{ma.NName}_{ma.VName}";
            string sichererName = SichererOrdnername(name);
            return Path.Combine(BasisOrdner, MaUnterOrdner, sichererName);
        }

        private static string SichererOrdnername(string name)
        {
            char[] ungueltig = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (char c in name)
                sb.Append(ungueltig.Contains(c) ? '_' : c);
            return sb.ToString().Trim('_');
        }

        private static string SichereDateiname(string dateiname)
        {
            char[] ungueltig = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (char c in dateiname)
                sb.Append(ungueltig.Contains(c) ? '_' : c);
            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE HILFSMETHODEN — SICHERHEIT & HASHING
        // ══════════════════════════════════════════════════════════════════

        private static string BerechneHash(string pfad)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(pfad);
            byte[] hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static void HashInManifestSpeichern(string ordner, string dateiname, string hash)
        {
            string manifestPfad = Path.Combine(ordner, ManifestDateiname);

            if (File.Exists(manifestPfad))
            {
                File.SetAttributes(manifestPfad,
                    File.GetAttributes(manifestPfad) & ~FileAttributes.ReadOnly);
            }

            var vorhandene = LadeManifest(manifestPfad);
            vorhandene[dateiname] = hash;

            var zeilen = vorhandene
                .Select(kv => $"{kv.Key}|{kv.Value}")
                .ToList();

            File.WriteAllLines(manifestPfad, zeilen, Encoding.UTF8);

            File.SetAttributes(manifestPfad,
                File.GetAttributes(manifestPfad) | FileAttributes.ReadOnly);
        }

        private static Dictionary<string, string> LadeManifest(string manifestPfad)
        {
            var ergebnis = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(manifestPfad)) return ergebnis;

            try
            {
                foreach (string zeile in File.ReadAllLines(manifestPfad, Encoding.UTF8))
                {
                    int trennPos = zeile.IndexOf('|');
                    if (trennPos <= 0) continue;

                    string name = zeile.Substring(0, trennPos);
                    string hash = zeile.Substring(trennPos + 1);
                    ergebnis[name] = hash;
                }
            }
            catch
            {
                // Manifest beschädigt → leer zurückgeben
            }

            return ergebnis;
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE HILFSMETHODEN — DATEI ÖFFNEN
        // ══════════════════════════════════════════════════════════════════

        private static void DateiOeffnen(string pfad)
        {
            bool warReadOnly = false;
            try
            {
                var attr = File.GetAttributes(pfad);
                if ((attr & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(pfad, attr & ~FileAttributes.ReadOnly);
                    warReadOnly = true;
                }
            }
            catch { }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  🔍 Sicherheitsprüfung vor dem Öffnen ...");
            Console.ResetColor();

            SicherheitsBefund befund = DateiSicherheitsScanner.DateiScannen(pfad);
            bool darfOeffnen = DateiSicherheitsScanner.BefundAnzeigenUndEntscheiden(befund);

            if (warReadOnly)
            {
                try { File.SetAttributes(pfad, File.GetAttributes(pfad) | FileAttributes.ReadOnly); }
                catch { }
            }

            if (!darfOeffnen)
            {
                ConsoleHelper.PrintWarning("Datei wurde nicht geöffnet.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"  Öffne: {Path.GetFileName(pfad)} ...");
                Console.ResetColor();

                var psi = new ProcessStartInfo
                {
                    FileName = pfad,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Datei konnte nicht geöffnet werden: {ex.Message}");
                LogManager.LogFehler("AnhangManager.DateiOeffnen", ex.Message);
            }
        }

        private static void OrdnerOeffnen(string pfad)
        {
            try
            {
                string vollPfad = Path.GetFullPath(pfad);

                var psi = new ProcessStartInfo { UseShellExecute = true };

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    psi.FileName = "explorer.exe";
                    psi.Arguments = $"\"{vollPfad}\"";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    psi.FileName = "open";
                    psi.Arguments = $"\"{vollPfad}\"";
                }
                else
                {
                    psi.FileName = "xdg-open";
                    psi.Arguments = $"\"{vollPfad}\"";
                }

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Ordner konnte nicht geöffnet werden: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE HILFSMETHODEN — FORMATIERUNG
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Kürzt einen Text auf maxLen Zeichen und füllt ihn mit Leerzeichen auf.
        /// </summary>
        private static string TruncateOrPad(string text, int maxLen)
        {
            if (text == null) text = "-";
            if (text.Length > maxLen)
                return text.Substring(0, maxLen - 1) + "…";
            return text.PadRight(maxLen);
        }

        private static string FormatiereDateigroesse(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024.0 / 1024:F1} MB";
            return $"{bytes / 1024.0 / 1024 / 1024:F2} GB";
        }
    }
}