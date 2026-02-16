using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;


namespace Inventarverwaltung
{
    // ═══════════════════════════════════════════════════════════════
    // MODELLE FÜR DRUCKHISTORIE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Repräsentiert einen gedruckten Hardware-Ausgabe-Beleg
    /// </summary>
    public class DruckHistorieEintrag
    {
        public string Id { get; set; }                          // Eindeutige ID
        public string MitarbeiterKuerzel { get; set; }          // Kürzel des Mitarbeiters
        public string MitarbeiterName { get; set; }             // Vollständiger Name
        public DateTime Datum { get; set; }                     // Datum der Ausgabe
        public string GedrucktVon { get; set; }                 // Wer hat gedruckt
        public string DruckerName { get; set; }                 // Welcher Drucker
        public List<DruckArtikelPosition> Artikel { get; set; } // Artikel-Liste
        public string Status { get; set; }                      // Gedruckt / Vorschau / Storniert
        public string Notizen { get; set; }                     // Optionale Notizen

        public DruckHistorieEintrag()
        {
            Artikel = new List<DruckArtikelPosition>();
            Status = "Gedruckt";
        }
    }

    /// <summary>
    /// Einzelne Artikel-Position im Druckbeleg
    /// </summary>
    public class DruckArtikelPosition
    {
        public string InvNmr { get; set; }
        public string GeraeteName { get; set; }
        public string SerienNummer { get; set; }
        public string Kategorie { get; set; }
        public int Anzahl { get; set; }
        public string Bemerkung { get; set; }   // Optionale Bemerkung
    }

    // ═══════════════════════════════════════════════════════════════
    // HAUPT-MANAGER KLASSE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Verwaltet den Hardware-Ausgabe-Druck mit Vorschau, Druckerauswahl und Historie
    /// </summary>
    public static partial class HardwarePrintManager
    {
        // Dateipfad für die Druckhistorie (JSON)
        private static readonly string HistorieDatei = "DruckHistorie.json";

        // Aktuelle Druckliste (wird vor dem Drucken bearbeitet)
        private static List<DruckArtikelPosition> _aktuelleArtikelListe = new List<DruckArtikelPosition>();
        private static string _aktuelleMitarbeiterKuerzel = "";
        private static string _aktuellerMitarbeiterName = "";
        private static string _ausgewaehlterDrucker = "";

        // ═══════════════════════════════════════════════════════════════
        // HAUPTMENÜ
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Hauptmenü für den Hardware-Ausgabe-Druck
        /// </summary>
        public static void ZeigeDruckMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║           🖨️  HARDWARE AUSGABE - DRUCKMANAGER                    ║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  ┌─ 🖨️  DRUCKFUNKTIONEN " + new string('─', 46) + "┐");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  │ [1] 📄 Neues Hardware-Ausgabe-Dokument erstellen & drucken      │");
                Console.WriteLine("  │ [2] 📚 Druckhistorie anzeigen                                   │");
                Console.WriteLine("  │ [3] 🔍 Druckhistorie durchsuchen                                │");
                Console.WriteLine("  │ [4] ✏️  Historien-Eintrag bearbeiten                             │");
                Console.WriteLine("  │ [5] 🖨️  Standarddrucker festlegen                               │");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  └" + new string('─', 68) + "┘");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [0] ← Zurück zum Hauptmenü");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  ▶ Ihre Auswahl: ");
                Console.ResetColor();
                string auswahl = Console.ReadLine()?.Trim();

                switch (auswahl)
                {
                    case "1":
                        NeuesDokumentErstellen();
                        break;
                    case "2":
                        ZeigeDruckHistorie();
                        break;
                    case "3":
                        SucheDruckHistorie();
                        break;
                    case "4":
                        BearbeiteHistorienEintrag();
                        break;
                    case "5":
                        DruckerAuswaehlen(true);
                        break;
                    case "0":
                        return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        System.Threading.Thread.Sleep(1000);
                        break;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // NEUES DOKUMENT ERSTELLEN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Erstellt ein neues Hardware-Ausgabe-Dokument (Schritt für Schritt)
        /// </summary>
        private static void NeuesDokumentErstellen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║           📄 NEUES HARDWARE-AUSGABE DOKUMENT                      ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            // ─── Schritt 1: Mitarbeiter auswählen ─────────────────────────────
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ══ SCHRITT 1: Mitarbeiter auswählen ══");
            Console.ResetColor();

            string mitarbeiterKuerzel = MitarbeiterAuswaehlen();
            if (string.IsNullOrEmpty(mitarbeiterKuerzel)) return;

            // ─── Schritt 2: Artikel laden ──────────────────────────────────────
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ══ SCHRITT 2: Artikel laden & bearbeiten ══");
            Console.ResetColor();

            _aktuelleMitarbeiterKuerzel = mitarbeiterKuerzel;
            var mitarbeiter = DataManager.Mitarbeiter.FirstOrDefault(m =>
                GetMitarbeiterKuerzel(m).Equals(mitarbeiterKuerzel, StringComparison.OrdinalIgnoreCase));
            _aktuellerMitarbeiterName = mitarbeiter != null
                ? $"{mitarbeiter.VName} {mitarbeiter.NName}"
                : mitarbeiterKuerzel;

            // Lade zugewiesene Artikel
            _aktuelleArtikelListe = LadeArtikelFuerMitarbeiter(mitarbeiterKuerzel);

            if (_aktuelleArtikelListe.Count == 0)
            {
                Console.WriteLine();
                ConsoleHelper.PrintWarning($"Keine Artikel für '{mitarbeiterKuerzel}' gefunden.");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Möchten Sie Artikel manuell hinzufügen?");
                Console.ResetColor();
                string antwort = ConsoleHelper.GetInput("Artikel manuell hinzufügen? (j/n)");
                if (antwort?.ToLower() != "j" && antwort?.ToLower() != "ja")
                {
                    ConsoleHelper.PrintInfo("Vorgang abgebrochen.");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }
            }

            // ─── Schritt 3: Artikel-Liste bearbeiten ──────────────────────────
            ArtikelListeBearbeiten();

            if (_aktuelleArtikelListe.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Artikel in der Liste. Druckvorgang abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // ─── Schritt 4: Vorschau anzeigen ─────────────────────────────────
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ══ SCHRITT 3: Dokument-Vorschau ══");
            Console.ResetColor();

            ZeigeDokumentVorschau();

            // ─── Schritt 5: Druckbestätigung ───────────────────────────────────
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ DRUCKOPTIONEN " + new string('─', 52) + "┐");
            Console.WriteLine("  │ [1] ✅ Dokument drucken                                         │");
            Console.WriteLine("  │ [2] ✏️  Weiter bearbeiten                                        │");
            Console.WriteLine("  │ [0] ❌ Abbrechen                                                 │");
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\n  ▶ Ihre Auswahl: ");
            Console.ResetColor();
            string entscheidung = Console.ReadLine()?.Trim();

            switch (entscheidung)
            {
                case "1":
                    DokumentDrucken();
                    break;
                case "2":
                    ArtikelListeBearbeiten();
                    ZeigeDokumentVorschau();
                    Console.WriteLine();
                    string bestaetigung = ConsoleHelper.GetInput("Jetzt drucken? (j/n)");
                    if (bestaetigung?.ToLower() == "j" || bestaetigung?.ToLower() == "ja")
                        DokumentDrucken();
                    break;
                case "0":
                    ConsoleHelper.PrintInfo("Druckvorgang abgebrochen.");
                    ConsoleHelper.PressKeyToContinue();
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // MITARBEITER AUSWÄHLEN
        // ═══════════════════════════════════════════════════════════════

        private static string MitarbeiterAuswaehlen()
        {
            if (DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintError("Keine Mitarbeiter vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return null;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Verfügbare Mitarbeiter:");
            Console.WriteLine("  " + new string('─', 65));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {"Nr.",-5} {"Kürzel",-10} {"Name",-25} {"Abteilung",-20}");
            Console.WriteLine("  " + new string('─', 65));
            Console.ResetColor();

            for (int i = 0; i < DataManager.Mitarbeiter.Count; i++)
            {
                var m = DataManager.Mitarbeiter[i];
                string kuerzel = GetMitarbeiterKuerzel(m);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {i + 1,-5} {kuerzel,-10} {m.VName + " " + m.NName,-25} {m.Abteilung,-20}");
            }

            Console.ResetColor();
            Console.WriteLine("  " + new string('─', 65));
            Console.WriteLine();

            while (true)
            {
                string eingabe = ConsoleHelper.GetInput("Mitarbeiter-Kürzel oder Nummer eingeben");
                if (string.IsNullOrWhiteSpace(eingabe))
                {
                    ConsoleHelper.PrintError("Eingabe darf nicht leer sein!");
                    continue;
                }

                // Versuche als Nummer
                if (int.TryParse(eingabe, out int nr) && nr >= 1 && nr <= DataManager.Mitarbeiter.Count)
                {
                    return GetMitarbeiterKuerzel(DataManager.Mitarbeiter[nr - 1]);
                }

                // Versuche als Kürzel
                var gefunden = DataManager.Mitarbeiter.FirstOrDefault(m =>
                    GetMitarbeiterKuerzel(m).Equals(eingabe, StringComparison.OrdinalIgnoreCase));

                if (gefunden != null)
                    return GetMitarbeiterKuerzel(gefunden);

                ConsoleHelper.PrintError($"Mitarbeiter '{eingabe}' nicht gefunden!");
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ARTIKEL-LISTE BEARBEITEN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Interaktive Bearbeitung der Artikel-Liste
        /// </summary>
        private static void ArtikelListeBearbeiten()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
                Console.WriteLine($"  ║  ✏️  ARTIKEL-LISTE BEARBEITEN  │  Mitarbeiter: {_aktuelleMitarbeiterKuerzel,-18}║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();

                ZeigeArtikelListeTabelle();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  ┌─ AKTIONEN " + new string('─', 57) + "┐");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  │ [1] ➕ Artikel hinzufügen                                       │");
                Console.WriteLine("  │ [2] ✏️  Artikel bearbeiten (Bemerkung / Anzahl)                  │");
                Console.WriteLine("  │ [3] ❌ Artikel entfernen                                         │");
                Console.WriteLine("  │ [4] 🔄 Alle Artikel neu laden                                   │");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  │ [0] ✅ Fertig - zur Vorschau                                     │");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  └" + new string('─', 68) + "┘");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\n  ▶ Ihre Auswahl: ");
                Console.ResetColor();
                string auswahl = Console.ReadLine()?.Trim();

                switch (auswahl)
                {
                    case "1":
                        ArtikelHinzufuegen();
                        break;
                    case "2":
                        ArtikelBearbeiten();
                        break;
                    case "3":
                        ArtikelEntfernen();
                        break;
                    case "4":
                        _aktuelleArtikelListe = LadeArtikelFuerMitarbeiter(_aktuelleMitarbeiterKuerzel);
                        ConsoleHelper.PrintSuccess("Liste wurde neu geladen!");
                        System.Threading.Thread.Sleep(1000);
                        break;
                    case "0":
                        return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        System.Threading.Thread.Sleep(800);
                        break;
                }
            }
        }

        private static void ZeigeArtikelListeTabelle()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {"Nr.",-5} {"Inv.-Nr.",-12} {"Gerätename",-28} {"SNR",-16} {"Anz.",-6} {"Bemerkung",-15}");
            Console.WriteLine("  " + new string('─', 85));
            Console.ResetColor();

            if (_aktuelleArtikelListe.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  (Keine Artikel in der Liste)");
                Console.ResetColor();
            }
            else
            {
                for (int i = 0; i < _aktuelleArtikelListe.Count; i++)
                {
                    var a = _aktuelleArtikelListe[i];
                    string name = a.GeraeteName.Length > 26 ? a.GeraeteName.Substring(0, 26) + ".." : a.GeraeteName;
                    string snr = (a.SerienNummer ?? "").Length > 14 ? a.SerienNummer.Substring(0, 14) + ".." : (a.SerienNummer ?? "-");
                    string bem = (a.Bemerkung ?? "").Length > 13 ? a.Bemerkung.Substring(0, 13) + ".." : (a.Bemerkung ?? "-");

                    Console.ForegroundColor = i % 2 == 0 ? ConsoleColor.White : ConsoleColor.Gray;
                    Console.WriteLine($"  {i + 1,-5} {a.InvNmr,-12} {name,-28} {snr,-16} {a.Anzahl,-6} {bem,-15}");
                }
            }

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 85));
            Console.WriteLine($"  Gesamt: {_aktuelleArtikelListe.Count} Artikel");
            Console.ResetColor();
        }

        private static void ArtikelHinzufuegen()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ── Artikel aus Inventar suchen ──");
            Console.ResetColor();

            string suchbegriff = ConsoleHelper.GetInput("Inventar-Nr. oder Gerätename (oder * für alle)");
            if (string.IsNullOrWhiteSpace(suchbegriff)) return;

            List<InvId> treffer;
            if (suchbegriff == "*")
                treffer = DataManager.Inventar.ToList();
            else
                treffer = DataManager.Inventar.Where(a =>
                    a.InvNmr.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                    a.GeraeteName.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase)).ToList();

            if (treffer.Count == 0)
            {
                ConsoleHelper.PrintError("Keine Artikel gefunden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {"Nr.",-5} {"Inv.-Nr.",-12} {"Gerätename",-30} {"SNR",-16} {"Anzahl",-8}");
            Console.WriteLine("  " + new string('─', 75));
            Console.ResetColor();

            for (int i = 0; i < treffer.Count; i++)
            {
                var a = treffer[i];
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {i + 1,-5} {a.InvNmr,-12} {a.GeraeteName,-30} {a.SerienNummer,-16} {a.Anzahl,-8}");
            }

            Console.ResetColor();
            Console.WriteLine();

            string numEingabe = ConsoleHelper.GetInput("Nummer des hinzuzufügenden Artikels (0 = abbrechen)");
            if (!int.TryParse(numEingabe, out int num) || num < 1 || num > treffer.Count) return;

            var ausgewaehlt = treffer[num - 1];

            // Prüfe ob bereits in Liste
            if (_aktuelleArtikelListe.Any(a => a.InvNmr == ausgewaehlt.InvNmr))
            {
                ConsoleHelper.PrintWarning("Dieser Artikel ist bereits in der Liste!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            string bemerkung = ConsoleHelper.GetInput("Bemerkung (optional, Enter überspringen)");

            _aktuelleArtikelListe.Add(new DruckArtikelPosition
            {
                InvNmr = ausgewaehlt.InvNmr,
                GeraeteName = ausgewaehlt.GeraeteName,
                SerienNummer = ausgewaehlt.SerienNummer,
                Kategorie = ausgewaehlt.Kategorie,
                Anzahl = 1,
                Bemerkung = string.IsNullOrWhiteSpace(bemerkung) ? null : bemerkung
            });

            ConsoleHelper.PrintSuccess($"Artikel '{ausgewaehlt.GeraeteName}' hinzugefügt!");
            System.Threading.Thread.Sleep(900);
        }

        private static void ArtikelBearbeiten()
        {
            if (_aktuelleArtikelListe.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Artikel in der Liste!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            ZeigeArtikelListeTabelle();
            string numEingabe = ConsoleHelper.GetInput("Nummer des zu bearbeitenden Artikels (0 = abbrechen)");
            if (!int.TryParse(numEingabe, out int num) || num < 1 || num > _aktuelleArtikelListe.Count) return;

            var artikel = _aktuelleArtikelListe[num - 1];

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  Bearbeite: [{artikel.InvNmr}] {artikel.GeraeteName}");
            Console.ResetColor();

            string neueBemerkung = ConsoleHelper.GetInput($"Bemerkung [aktuell: {artikel.Bemerkung ?? "-"}] (Enter = beibehalten)");
            if (!string.IsNullOrWhiteSpace(neueBemerkung))
                artikel.Bemerkung = neueBemerkung;

            string neueAnzahl = ConsoleHelper.GetInput($"Anzahl [aktuell: {artikel.Anzahl}] (Enter = beibehalten)");
            if (int.TryParse(neueAnzahl, out int anzahl) && anzahl > 0)
                artikel.Anzahl = anzahl;

            ConsoleHelper.PrintSuccess("Artikel aktualisiert!");
            System.Threading.Thread.Sleep(900);
        }

        private static void ArtikelEntfernen()
        {
            if (_aktuelleArtikelListe.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Artikel in der Liste!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            ZeigeArtikelListeTabelle();
            string numEingabe = ConsoleHelper.GetInput("Nummer des zu entfernenden Artikels (0 = abbrechen)");
            if (!int.TryParse(numEingabe, out int num) || num < 1 || num > _aktuelleArtikelListe.Count) return;

            string name = _aktuelleArtikelListe[num - 1].GeraeteName;
            _aktuelleArtikelListe.RemoveAt(num - 1);
            ConsoleHelper.PrintSuccess($"'{name}' aus der Liste entfernt!");
            System.Threading.Thread.Sleep(900);
        }

        // ═══════════════════════════════════════════════════════════════
        // DOKUMENT-VORSCHAU
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Zeigt eine formatierte Vorschau des Druckdokuments in der Konsole
        /// </summary>
        private static void ZeigeDokumentVorschau()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine();
            Console.WriteLine("  " + new string('═', 70));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  " + CenterText("AUSGABE HARDWARE", 68));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 70));
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {"Datum:",-25} {DateTime.Now:dd.MM.yyyy}");
            Console.WriteLine($"  {"Mitarbeiter-Kürzel:",-25} {_aktuelleMitarbeiterKuerzel}");
            Console.WriteLine($"  {"Mitarbeiter:",-25} {_aktuellerMitarbeiterName}");
            Console.WriteLine($"  {"Ausgestellt von:",-25} {AuthManager.AktuellerBenutzer}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 70));
            Console.ResetColor();

            // Artikel-Tabelle
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine($"  {"Nr.",-5} {"Inv.-Nr.",-12} {"Gerätename",-28} {"Seriennummer",-16} {"Anz.",-6}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 70));
            Console.ResetColor();

            for (int i = 0; i < _aktuelleArtikelListe.Count; i++)
            {
                var a = _aktuelleArtikelListe[i];
                string name = a.GeraeteName.Length > 26 ? a.GeraeteName.Substring(0, 26) + ".." : a.GeraeteName;
                string snr = (a.SerienNummer ?? "").Length > 14 ? a.SerienNummer.Substring(0, 14) + ".." : (a.SerienNummer ?? "-");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {i + 1,-5} {a.InvNmr,-12} {name,-28} {snr,-16} {a.Anzahl,-6}");

                if (!string.IsNullOrWhiteSpace(a.Bemerkung))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"       └─ Bemerkung: {a.Bemerkung}");
                }
            }

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 70));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  Gesamt: {_aktuelleArtikelListe.Count} Artikel");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine();
            Console.WriteLine("  " + new string('─', 70));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine("  Unterschrift Mitarbeiter: ____________________________________");
            Console.WriteLine();
            Console.WriteLine("  Unterschrift Ausgabe-Person: ________________________________");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('═', 70));
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [ Dies ist eine Vorschau - das gedruckte Dokument kann abweichen ]");
            Console.ResetColor();
        }

        // ═══════════════════════════════════════════════════════════════
        // DRUCKER AUSWÄHLEN & DRUCKEN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Drucker-Auswahl-Dialog
        /// </summary>
        private static void DruckerAuswaehlen(bool alsStandard = false)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║           🖨️  DRUCKER AUSWÄHLEN                                  ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Verfügbare Drucker:");
            Console.WriteLine("  " + new string('─', 60));
            Console.ResetColor();

            var drucker = new List<string>();
            try
            {
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    drucker.Add(printer);
                }
            }
            catch
            {
                // Fallback wenn System.Drawing.Common nicht verfügbar
                drucker.Add("Standard-Drucker");
            }

            // Standard-Drucker markieren
            string standardDrucker = "";
            try
            {
                var defaultSettings = new PrinterSettings();
                standardDrucker = defaultSettings.PrinterName;
            }
            catch { }

            if (drucker.Count == 0)
            {
                ConsoleHelper.PrintError("Keine Drucker gefunden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            for (int i = 0; i < drucker.Count; i++)
            {
                bool istStandard = drucker[i] == standardDrucker;
                Console.ForegroundColor = istStandard ? ConsoleColor.Green : ConsoleColor.White;
                string marker = istStandard ? " ⭐ [Standard]" : "";
                Console.WriteLine($"  [{i + 1}] {drucker[i]}{marker}");
            }

            Console.ResetColor();
            Console.WriteLine("  " + new string('─', 60));
            Console.WriteLine("  [0] Abbrechen");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ Drucker wählen: ");
            Console.ResetColor();
            string eingabe = Console.ReadLine()?.Trim();

            if (eingabe == "0" || string.IsNullOrWhiteSpace(eingabe)) return;

            if (int.TryParse(eingabe, out int nr) && nr >= 1 && nr <= drucker.Count)
            {
                _ausgewaehlterDrucker = drucker[nr - 1];
                ConsoleHelper.PrintSuccess($"Drucker gewählt: {_ausgewaehlterDrucker}");

                if (alsStandard)
                {
                    // Drucker in Einstellungsdatei speichern
                    File.WriteAllText("DruckEinstellungen.txt", _ausgewaehlterDrucker);
                    ConsoleHelper.PrintInfo("Als Standarddrucker gespeichert.");
                }
                System.Threading.Thread.Sleep(1200);
            }
            else
            {
                ConsoleHelper.PrintError("Ungültige Auswahl.");
                ConsoleHelper.PressKeyToContinue();
            }
        }

        /// <summary>
        /// Führt den eigentlichen Druckvorgang durch
        /// </summary>
        private static void DokumentDrucken()
        {
            // Drucker-Auswahl prüfen
            if (string.IsNullOrEmpty(_ausgewaehlterDrucker))
            {
                // Versuche gespeicherten Standarddrucker zu laden
                if (File.Exists("DruckEinstellungen.txt"))
                    _ausgewaehlterDrucker = File.ReadAllText("DruckEinstellungen.txt").Trim();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Aktueller Drucker: {(string.IsNullOrEmpty(_ausgewaehlterDrucker) ? "(Systemstandard)" : _ausgewaehlterDrucker)}");
            Console.ResetColor();

            string druckerWechseln = ConsoleHelper.GetInput("Drucker wechseln? (j/n)");
            if (druckerWechseln?.ToLower() == "j" || druckerWechseln?.ToLower() == "ja")
                DruckerAuswaehlen();

            // Druck-Snapshot für Historie anlegen
            var historieEintrag = new DruckHistorieEintrag
            {
                Id = GenerateId(),
                MitarbeiterKuerzel = _aktuelleMitarbeiterKuerzel,
                MitarbeiterName = _aktuellerMitarbeiterName,
                Datum = DateTime.Now,
                GedrucktVon = AuthManager.AktuellerBenutzer,
                DruckerName = string.IsNullOrEmpty(_ausgewaehlterDrucker) ? "Systemstandard" : _ausgewaehlterDrucker,
                Artikel = new List<DruckArtikelPosition>(_aktuelleArtikelListe.Select(a => new DruckArtikelPosition
                {
                    InvNmr = a.InvNmr,
                    GeraeteName = a.GeraeteName,
                    SerienNummer = a.SerienNummer,
                    Kategorie = a.Kategorie,
                    Anzahl = a.Anzahl,
                    Bemerkung = a.Bemerkung
                })),
                Status = "Gedruckt"
            };

            // Druckvorgang ausführen
            bool druckErfolgreich = false;
            try
            {
                var printDoc = new PrintDocument();

                if (!string.IsNullOrEmpty(_ausgewaehlterDrucker))
                    printDoc.PrinterSettings.PrinterName = _ausgewaehlterDrucker;

                printDoc.DocumentName = $"HW-Ausgabe_{_aktuelleMitarbeiterKuerzel}_{DateTime.Now:yyyyMMdd_HHmm}";
                printDoc.PrintPage += (sender, e) => PrintPage(e, historieEintrag);
                printDoc.Print();
                druckErfolgreich = true;
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Druckfehler: {ex.Message}");
                historieEintrag.Status = "Druckfehler";
                historieEintrag.Notizen = ex.Message;
            }

            // Historie speichern (immer, auch bei Fehler)
            SpeichereHistorieEintrag(historieEintrag);

            if (druckErfolgreich)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║                                                                   ║");
                Console.WriteLine("  ║     ✅ DOKUMENT WURDE ERFOLGREICH GEDRUCKT!                       ║");
                Console.WriteLine("  ║                                                                   ║");
                Console.WriteLine($"  ║     🖨️  Drucker: {historieEintrag.DruckerName,-48}║");
                Console.WriteLine($"  ║     📋 ID: {historieEintrag.Id,-55}║");
                Console.WriteLine("  ║                                                                   ║");
                Console.WriteLine("  ║     📚 Eintrag wurde in der Druckhistorie gespeichert.            ║");
                Console.WriteLine("  ║                                                                   ║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
            }
            else
            {
                ConsoleHelper.PrintWarning("Dokument wurde in der Historie als 'Druckfehler' gespeichert.");
            }

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Erzeugt den Druckinhalt für eine Seite
        /// </summary>
        private static void PrintPage(PrintPageEventArgs e, DruckHistorieEintrag eintrag)
        {
            Graphics g = e.Graphics;
            float yPos = 40;
            float margin = 60;
            float pageWidth = e.PageBounds.Width - margin * 2;

            // ── Titel ──────────────────────────────────────────────────
            Font titelFont = new Font("Arial", 18, FontStyle.Bold);
            Font subFont = new Font("Arial", 10, FontStyle.Regular);
            Font boldFont = new Font("Arial", 10, FontStyle.Bold);
            Font tableFont = new Font("Arial", 9, FontStyle.Regular);
            Font smallFont = new Font("Arial", 8, FontStyle.Regular);
            Font underlineFont = new Font("Arial", 10, FontStyle.Underline);

            Brush schwarz = Brushes.Black;
            Brush grau = Brushes.Gray;

            // Doppelter Rahmen oben
            g.DrawRectangle(Pens.Black, margin, yPos, pageWidth, 60);
            g.DrawRectangle(Pens.Black, margin + 2, yPos + 2, pageWidth - 4, 56);

            // Titel
            SizeF titelSize = g.MeasureString("AUSGABE HARDWARE", titelFont);
            g.DrawString("AUSGABE HARDWARE", titelFont, schwarz, margin + (pageWidth - titelSize.Width) / 2, yPos + 12);
            yPos += 75;

            // ── Info-Block ─────────────────────────────────────────────
            g.DrawString($"Datum:", boldFont, schwarz, margin, yPos);
            g.DrawString(eintrag.Datum.ToString("dd.MM.yyyy"), subFont, schwarz, margin + 120, yPos);

            g.DrawString($"Mitarbeiter-Kürzel:", boldFont, schwarz, margin + 300, yPos);
            g.DrawString(eintrag.MitarbeiterKuerzel, subFont, schwarz, margin + 460, yPos);
            yPos += 20;

            g.DrawString($"Mitarbeiter:", boldFont, schwarz, margin, yPos);
            g.DrawString(eintrag.MitarbeiterName, subFont, schwarz, margin + 120, yPos);

            g.DrawString($"Ausgestellt von:", boldFont, schwarz, margin + 300, yPos);
            g.DrawString(eintrag.GedrucktVon, subFont, schwarz, margin + 460, yPos);
            yPos += 10;

            // Trennlinie
            g.DrawLine(Pens.Black, margin, yPos, margin + pageWidth, yPos);
            yPos += 15;

            // ── Artikel-Tabelle ────────────────────────────────────────
            float col0 = margin;           // Nr.
            float col1 = margin + 35;      // Inv-Nr
            float col2 = margin + 115;     // Gerätename
            float col3 = margin + 340;     // Seriennummer
            float col4 = margin + 490;     // Anzahl
            float col5 = margin + 540;     // Bemerkung

            // Tabellenkopf
            g.FillRectangle(Brushes.LightGray, margin, yPos, pageWidth, 18);
            g.DrawRectangle(Pens.Black, margin, yPos, pageWidth, 18);
            g.DrawString("Nr.", boldFont, schwarz, col0 + 2, yPos + 3);
            g.DrawString("Inventar-Nr.", boldFont, schwarz, col1, yPos + 3);
            g.DrawString("Gerätename", boldFont, schwarz, col2, yPos + 3);
            g.DrawString("Seriennummer", boldFont, schwarz, col3, yPos + 3);
            g.DrawString("Anz.", boldFont, schwarz, col4, yPos + 3);
            g.DrawString("Bemerkung", boldFont, schwarz, col5, yPos + 3);
            yPos += 20;

            // Artikel-Zeilen
            for (int i = 0; i < eintrag.Artikel.Count; i++)
            {
                var a = eintrag.Artikel[i];

                if (i % 2 == 0)
                    g.FillRectangle(new SolidBrush(Color.FromArgb(248, 248, 248)), margin, yPos, pageWidth, 16);

                g.DrawRectangle(Pens.LightGray, margin, yPos, pageWidth, 16);
                g.DrawString($"{i + 1}", tableFont, schwarz, col0 + 2, yPos + 2);
                g.DrawString(a.InvNmr, tableFont, schwarz, col1, yPos + 2);

                // Name ggf. kürzen
                string name = a.GeraeteName;
                while (g.MeasureString(name, tableFont).Width > (col3 - col2 - 5) && name.Length > 0)
                    name = name.Substring(0, name.Length - 1);
                if (name != a.GeraeteName) name += "…";
                g.DrawString(name, tableFont, schwarz, col2, yPos + 2);

                g.DrawString(a.SerienNummer ?? "-", tableFont, schwarz, col3, yPos + 2);
                g.DrawString(a.Anzahl.ToString(), tableFont, schwarz, col4, yPos + 2);
                g.DrawString(a.Bemerkung ?? "-", tableFont, grau, col5, yPos + 2);
                yPos += 17;
            }

            // Tabellen-Abschluss
            g.DrawLine(Pens.Black, margin, yPos, margin + pageWidth, yPos);
            yPos += 5;
            g.DrawString($"Gesamt: {eintrag.Artikel.Count} Artikel", smallFont, grau, margin, yPos);
            yPos += 30;

            // ── Unterschriftsfeld ─────────────────────────────────────
            g.DrawLine(Pens.Black, margin, yPos, margin + pageWidth, yPos);
            yPos += 5;

            // Mitarbeiter-Unterschrift
            float sigBoxWidth = (pageWidth - 40) / 2;
            yPos += 10;
            g.DrawString("Unterschrift Mitarbeiter:", boldFont, schwarz, margin, yPos);
            yPos += 35;
            g.DrawLine(Pens.Black, margin, yPos, margin + sigBoxWidth, yPos);
            g.DrawString(eintrag.MitarbeiterName, smallFont, grau, margin, yPos + 3);
            g.DrawString(eintrag.Datum.ToString("dd.MM.yyyy"), smallFont, grau, margin + sigBoxWidth - 70, yPos + 3);
            yPos += 25;

            // Ausgabe-Person-Unterschrift
            g.DrawString("Unterschrift Ausgabe-Person:", boldFont, schwarz, margin, yPos);
            yPos += 35;
            g.DrawLine(Pens.Black, margin, yPos, margin + sigBoxWidth, yPos);
            g.DrawString(eintrag.GedrucktVon, smallFont, grau, margin, yPos + 3);
            g.DrawString(eintrag.Datum.ToString("dd.MM.yyyy"), smallFont, grau, margin + sigBoxWidth - 70, yPos + 3);
            yPos += 30;

            // ── Fußzeile ───────────────────────────────────────────────
            g.DrawLine(Pens.LightGray, margin, e.PageBounds.Height - 50, margin + pageWidth, e.PageBounds.Height - 50);
            g.DrawString($"Inventarverwaltung | Dok.-ID: {eintrag.Id} | Gedruckt: {eintrag.Datum:dd.MM.yyyy HH:mm}",
                smallFont, grau, margin, e.PageBounds.Height - 42);

            // Ressourcen freigeben
            titelFont.Dispose();
            subFont.Dispose();
            boldFont.Dispose();
            tableFont.Dispose();
            smallFont.Dispose();
            underlineFont.Dispose();
        }

        // ═══════════════════════════════════════════════════════════════
        // DRUCKHISTORIE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Zeigt alle gespeicherten Druckvorgänge in der Historie an
        /// </summary>
        private static void ZeigeDruckHistorie()
        {
            var historie = LadeHistorie();
            ZeigeHistorieTabelle(historie, "DRUCKHISTORIE - ALLE EINTRÄGE");
        }

        private static void ZeigeHistorieTabelle(List<DruckHistorieEintrag> liste, string titel)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"  ║  📚 {titel,-62}║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            if (liste.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Einträge in der Druckhistorie vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {"Nr.",-5} {"Datum",-17} {"Mitarbeiter",-12} {"Name",-22} {"Artikel",-8} {"Drucker",-20} {"Status",-12}");
            Console.WriteLine("  " + new string('─', 98));
            Console.ResetColor();

            var sortiert = liste.OrderByDescending(e => e.Datum).ToList();

            for (int i = 0; i < sortiert.Count; i++)
            {
                var e = sortiert[i];
                ConsoleColor farbe = e.Status == "Gedruckt" ? ConsoleColor.White
                    : e.Status == "Druckfehler" ? ConsoleColor.Red
                    : ConsoleColor.Yellow;

                string druckerKurz = e.DruckerName.Length > 18 ? e.DruckerName.Substring(0, 18) + ".." : e.DruckerName;
                string nameKurz = e.MitarbeiterName.Length > 20 ? e.MitarbeiterName.Substring(0, 20) + ".." : e.MitarbeiterName;

                Console.ForegroundColor = farbe;
                Console.WriteLine($"  {i + 1,-5} {e.Datum:dd.MM.yyyy HH:mm}  {e.MitarbeiterKuerzel,-12} {nameKurz,-22} {e.Artikel.Count,-8} {druckerKurz,-20} {e.Status,-12}");
            }

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 98));
            Console.WriteLine($"  Gesamt: {liste.Count} Einträge");
            Console.ResetColor();
            Console.WriteLine();

            // Optionen
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  [Nr.] Detail-Ansicht eines Eintrags     [0] Zurück");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\n  ▶ Eingabe: ");
            Console.ResetColor();

            string eingabe = Console.ReadLine()?.Trim();
            if (eingabe == "0" || string.IsNullOrWhiteSpace(eingabe)) return;

            if (int.TryParse(eingabe, out int nr) && nr >= 1 && nr <= sortiert.Count)
                ZeigeHistorieDetail(sortiert[nr - 1]);
        }

        private static void ZeigeHistorieDetail(DruckHistorieEintrag eintrag)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║           🔍 DETAIL-ANSICHT DRUCKHISTORIE                         ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Dokument-ID:       {eintrag.Id}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  Datum:             {eintrag.Datum:dd.MM.yyyy HH:mm:ss}");
            Console.WriteLine($"  Mitarbeiter:       {eintrag.MitarbeiterKuerzel} ({eintrag.MitarbeiterName})");
            Console.WriteLine($"  Gedruckt von:      {eintrag.GedrucktVon}");
            Console.WriteLine($"  Drucker:           {eintrag.DruckerName}");
            ConsoleColor statusFarbe = eintrag.Status == "Gedruckt" ? ConsoleColor.Green : ConsoleColor.Red;
            Console.ForegroundColor = statusFarbe;
            Console.WriteLine($"  Status:            {eintrag.Status}");
            Console.ResetColor();

            if (!string.IsNullOrWhiteSpace(eintrag.Notizen))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"  Notizen:           {eintrag.Notizen}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {"Nr.",-5} {"Inv.-Nr.",-12} {"Gerätename",-28} {"Seriennummer",-16} {"Anz.",-6} {"Bemerkung"}");
            Console.WriteLine("  " + new string('─', 85));
            Console.ResetColor();

            for (int i = 0; i < eintrag.Artikel.Count; i++)
            {
                var a = eintrag.Artikel[i];
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {i + 1,-5} {a.InvNmr,-12} {a.GeraeteName,-28} {a.SerienNummer ?? "-",-16} {a.Anzahl,-6} {a.Bemerkung ?? "-"}");
            }

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 85));
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  [1] Erneut drucken   [2] Notiz bearbeiten   [0] Zurück");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\n  ▶ Eingabe: ");
            Console.ResetColor();

            string wahl = Console.ReadLine()?.Trim();
            switch (wahl)
            {
                case "1":
                    ErneutDrucken(eintrag);
                    break;
                case "2":
                    NotizBearbeiten(eintrag);
                    break;
            }
        }

        private static void ErneutDrucken(DruckHistorieEintrag original)
        {
            // Lade Daten in aktuelle Sitzung
            _aktuelleMitarbeiterKuerzel = original.MitarbeiterKuerzel;
            _aktuellerMitarbeiterName = original.MitarbeiterName;
            _aktuelleArtikelListe = new List<DruckArtikelPosition>(original.Artikel);

            string wahl = ConsoleHelper.GetInput("Dokument erneut drucken? (j/n)");
            if (wahl?.ToLower() == "j" || wahl?.ToLower() == "ja")
                DokumentDrucken();
        }

        private static void NotizBearbeiten(DruckHistorieEintrag eintrag)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Aktuelle Notiz: {eintrag.Notizen ?? "(keine)"}");
            Console.ResetColor();

            string neueNotiz = ConsoleHelper.GetInput("Neue Notiz eingeben");
            eintrag.Notizen = string.IsNullOrWhiteSpace(neueNotiz) ? eintrag.Notizen : neueNotiz;

            // Speichern
            var alle = LadeHistorie();
            int idx = alle.FindIndex(e => e.Id == eintrag.Id);
            if (idx >= 0) alle[idx] = eintrag;
            else alle.Add(eintrag);
            SpeichereHistorie(alle);

            ConsoleHelper.PrintSuccess("Notiz gespeichert!");
            System.Threading.Thread.Sleep(1000);
        }

        /// <summary>
        /// Suche in der Druckhistorie
        /// </summary>
        private static void SucheDruckHistorie()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║           🔍 DRUCKHISTORIE DURCHSUCHEN                            ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Suchoptionen:");
            Console.WriteLine("  [1] Nach Mitarbeiter-Kürzel");
            Console.WriteLine("  [2] Nach Datum (TT.MM.JJJJ)");
            Console.WriteLine("  [3] Nach Status (Gedruckt / Druckfehler)");
            Console.WriteLine("  [4] Nach Inventar-Nr.");
            Console.ResetColor();
            Console.WriteLine();

            string wahl = ConsoleHelper.GetInput("Suchoption wählen");
            var alle = LadeHistorie();
            List<DruckHistorieEintrag> treffer = new List<DruckHistorieEintrag>();

            switch (wahl)
            {
                case "1":
                    string kuerzel = ConsoleHelper.GetInput("Mitarbeiter-Kürzel");
                    treffer = alle.Where(e => e.MitarbeiterKuerzel.Contains(kuerzel ?? "",
                        StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "2":
                    string datumStr = ConsoleHelper.GetInput("Datum (TT.MM.JJJJ)");
                    if (DateTime.TryParse(datumStr, out DateTime datum))
                        treffer = alle.Where(e => e.Datum.Date == datum.Date).ToList();
                    break;
                case "3":
                    string status = ConsoleHelper.GetInput("Status (Gedruckt/Druckfehler)");
                    treffer = alle.Where(e => e.Status.Contains(status ?? "",
                        StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "4":
                    string invNr = ConsoleHelper.GetInput("Inventar-Nr.");
                    treffer = alle.Where(e => e.Artikel.Any(a => a.InvNmr.Contains(invNr ?? "",
                        StringComparison.OrdinalIgnoreCase))).ToList();
                    break;
                default:
                    return;
            }

            ZeigeHistorieTabelle(treffer, $"SUCHERGEBNISSE ({treffer.Count} Treffer)");
        }

        /// <summary>
        /// Bearbeitet einen bestehenden Historien-Eintrag
        /// </summary>
        private static void BearbeiteHistorienEintrag()
        {
            var alle = LadeHistorie();
            if (alle.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Einträge in der Druckhistorie.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            ZeigeDruckHistorie();
        }

        // ═══════════════════════════════════════════════════════════════
        // HILFSMETHODEN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Lädt alle dem Mitarbeiter zugewiesenen Artikel
        /// </summary>
        private static List<DruckArtikelPosition> LadeArtikelFuerMitarbeiter(string kuerzel)
        {
            var list = new List<DruckArtikelPosition>();

            // Suche nach Artikeln, die dem Mitarbeiter über Name oder Kürzel zugewiesen sind
            var mitarbeiter = DataManager.Mitarbeiter.FirstOrDefault(m =>
                GetMitarbeiterKuerzel(m).Equals(kuerzel, StringComparison.OrdinalIgnoreCase));

            string suchName = mitarbeiter != null
                ? $"{mitarbeiter.VName} {mitarbeiter.NName}"
                : kuerzel;

            foreach (var inv in DataManager.Inventar)
            {
                bool match = inv.MitarbeiterBezeichnung != null &&
                    (inv.MitarbeiterBezeichnung.Contains(suchName, StringComparison.OrdinalIgnoreCase) ||
                     inv.MitarbeiterBezeichnung.Contains(kuerzel, StringComparison.OrdinalIgnoreCase));

                if (match)
                {
                    list.Add(new DruckArtikelPosition
                    {
                        InvNmr = inv.InvNmr,
                        GeraeteName = inv.GeraeteName,
                        SerienNummer = inv.SerienNummer,
                        Kategorie = inv.Kategorie,
                        Anzahl = 1,
                        Bemerkung = null
                    });
                }
            }

            return list;
        }

        /// <summary>
        /// Erzeugt ein Mitarbeiter-Kürzel aus Vor- und Nachname
        /// </summary>
        private static string GetMitarbeiterKuerzel(MID m)
        {
            if (m == null) return "";
            string vInit = m.VName?.Length > 0 ? m.VName.Substring(0, 1).ToUpper() : "";
            string nShort = m.NName?.Length > 3 ? m.NName.Substring(0, 3).ToUpper() : m.NName?.ToUpper() ?? "";
            return vInit + nShort;
        }

        private static string CenterText(string text, int width)
        {
            if (text.Length >= width) return text;
            int padding = (width - text.Length) / 2;
            return new string(' ', padding) + text;
        }

        private static string GenerateId()
        {
            return $"HW{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(100, 999)}";
        }

        // ═══════════════════════════════════════════════════════════════
        // PERSISTENZ - DRUCKHISTORIE (JSON)
        // ═══════════════════════════════════════════════════════════════

        private static List<DruckHistorieEintrag> LadeHistorie()
        {
            try
            {
                if (!File.Exists(HistorieDatei)) return new List<DruckHistorieEintrag>();
                string json = File.ReadAllText(HistorieDatei, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json)) return new List<DruckHistorieEintrag>();
                return JsonSerializer.Deserialize<List<DruckHistorieEintrag>>(json)
                       ?? new List<DruckHistorieEintrag>();
            }
            catch
            {
                return new List<DruckHistorieEintrag>();
            }
        }

        private static void SpeichereHistorieEintrag(DruckHistorieEintrag eintrag)
        {
            var alle = LadeHistorie();
            alle.Add(eintrag);
            SpeichereHistorie(alle);
        }

        private static void SpeichereHistorie(List<DruckHistorieEintrag> liste)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(liste, options);
                File.WriteAllText(HistorieDatei, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Speichern der Historie: {ex.Message}");
            }
        }
    }
}