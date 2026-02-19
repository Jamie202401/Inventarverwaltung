using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.UI;
using System;
using System.Collections.Generic;
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
        private static string HistorieDatei => FileManager.DruckHistoriePfad;

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
            string standardDrucker = "";
            try
            {
                // System.Drawing.Common per Reflection laden (optional, Windows-only)
                var settingsType = Type.GetType("System.Drawing.Printing.PrinterSettings, System.Drawing.Common");
                if (settingsType == null)
                    settingsType = Type.GetType("System.Drawing.Printing.PrinterSettings, System.Drawing");

                if (settingsType != null)
                {
                    var installedProp = settingsType.GetProperty("InstalledPrinters", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (installedProp != null)
                    {
                        var printerList = installedProp.GetValue(null) as System.Collections.IEnumerable;
                        if (printerList != null)
                            foreach (var p in printerList)
                                drucker.Add(p.ToString());
                    }

                    var inst = Activator.CreateInstance(settingsType);
                    var nameProp = settingsType.GetProperty("PrinterName");
                    if (nameProp != null)
                        standardDrucker = nameProp.GetValue(inst)?.ToString() ?? "";
                }
            }
            catch { }

            if (drucker.Count == 0)
                drucker.Add("Standard-Drucker");

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
                    File.WriteAllText(FileManager.DruckEinstellungenPfad, _ausgewaehlterDrucker);
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
                if (File.Exists(FileManager.DruckEinstellungenPfad))
                    _ausgewaehlterDrucker = File.ReadAllText(FileManager.DruckEinstellungenPfad).Trim();
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
                DruckePerReflection(historieEintrag);
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
        /// Führt den Druckvorgang per Reflection aus (keine direkte System.Drawing-Abhängigkeit)
        /// </summary>
        private static void DruckePerReflection(DruckHistorieEintrag eintrag)
        {
            // Versuche System.Drawing.Common zu laden (Windows / NuGet-Paket)
            Type printDocType = Type.GetType("System.Drawing.Printing.PrintDocument, System.Drawing.Common")
                             ?? Type.GetType("System.Drawing.Printing.PrintDocument, System.Drawing");

            if (printDocType == null)
                throw new InvalidOperationException(
                    "System.Drawing ist nicht verfügbar. Bitte NuGet-Paket 'System.Drawing.Common' installieren.");

            dynamic printDoc = Activator.CreateInstance(printDocType);

            if (!string.IsNullOrEmpty(_ausgewaehlterDrucker))
            {
                dynamic ps = printDoc.PrinterSettings;
                ps.PrinterName = _ausgewaehlterDrucker;
            }

            printDoc.DocumentName =
                $"HW-Ausgabe_{_aktuelleMitarbeiterKuerzel}_{DateTime.Now:yyyyMMdd_HHmm}";

            printDoc.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(
                (sender, e) => PrintPage(e, eintrag));

            printDoc.Print();
        }

        /// <summary>
        /// Erzeugt professionellen DIN-A4-Druckinhalt (vollständig neu gestaltet)
        /// </summary>
        private static void PrintPage(System.Drawing.Printing.PrintPageEventArgs e,
                                      DruckHistorieEintrag eintrag)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // ── Seitenmaße (DIN A4 bei 100 dpi ≈ 827 × 1169) ─────────
            float margin = 50f;
            float pageW = e.MarginBounds.Width;   // nutzbarer Bereich
            float pageLeft = e.MarginBounds.Left;
            float pageTop = e.MarginBounds.Top;
            float yPos = pageTop;

            // ── Farben ─────────────────────────────────────────────────
            var colDark = System.Drawing.Color.FromArgb(30, 30, 45);   // Fast-Schwarz
            var colAccent = System.Drawing.Color.FromArgb(25, 90, 160);   // Dunkelblau
            var colAccentLt = System.Drawing.Color.FromArgb(210, 225, 245);  // Hellblau
            var colGray = System.Drawing.Color.FromArgb(110, 110, 110);
            var colLightGray = System.Drawing.Color.FromArgb(245, 246, 248);
            var colWhite = System.Drawing.Color.White;
            var colBorder = System.Drawing.Color.FromArgb(180, 190, 205);

            var brushDark = new System.Drawing.SolidBrush(colDark);
            var brushAccent = new System.Drawing.SolidBrush(colAccent);
            var brushAccentLt = new System.Drawing.SolidBrush(colAccentLt);
            var brushGray = new System.Drawing.SolidBrush(colGray);
            var brushWhite = new System.Drawing.SolidBrush(colWhite);
            var brushLight = new System.Drawing.SolidBrush(colLightGray);
            var penAccent = new System.Drawing.Pen(colAccent, 1.5f);
            var penBorder = new System.Drawing.Pen(colBorder, 0.8f);
            var penDark = new System.Drawing.Pen(colDark, 1.0f);
            var penThin = new System.Drawing.Pen(colBorder, 0.5f);

            // ── Fonts ──────────────────────────────────────────────────
            var fntKopf = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
            var fntSubKopf = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Regular);
            var fntSektTitel = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold);
            var fntLabel = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold);
            var fntWert = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular);
            var fntTblHead = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold);
            var fntTbl = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Regular);
            var fntSmall = new System.Drawing.Font("Arial", 7, System.Drawing.FontStyle.Regular);
            var fntSmallBold = new System.Drawing.Font("Arial", 7, System.Drawing.FontStyle.Bold);

            var sfLeft = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Near, LineAlignment = System.Drawing.StringAlignment.Center };
            var sfCenter = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center, LineAlignment = System.Drawing.StringAlignment.Center };
            var sfRight = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Far, LineAlignment = System.Drawing.StringAlignment.Center };

            // ════════════════════════════════════════════════════════════
            // [1] KOPFZEILE — blauer Balken mit Titel
            // ════════════════════════════════════════════════════════════
            float headerH = 70f;
            var headerRect = new System.Drawing.RectangleF(pageLeft, yPos, pageW, headerH);
            g.FillRectangle(brushAccent, headerRect);

            // Titel linksbündig
            var titleRect = new System.Drawing.RectangleF(pageLeft + 16, yPos + 8, pageW * 0.65f, headerH - 10);
            g.DrawString("HARDWARE-AUSGABE-BELEG", fntKopf, brushWhite, titleRect, sfLeft);

            // Rechts: Dok-ID + Datum
            var idRect = new System.Drawing.RectangleF(pageLeft + pageW * 0.65f, yPos + 10, pageW * 0.35f - 10, 22);
            g.DrawString($"Dok.-ID:  {eintrag.Id}", fntSubKopf, brushAccentLt, idRect, sfRight);
            var datRect = new System.Drawing.RectangleF(pageLeft + pageW * 0.65f, yPos + 30, pageW * 0.35f - 10, 20);
            g.DrawString($"Datum:    {eintrag.Datum:dd.MM.yyyy  HH:mm}", fntSubKopf, brushAccentLt, datRect, sfRight);
            var statRect = new System.Drawing.RectangleF(pageLeft + pageW * 0.65f, yPos + 48, pageW * 0.35f - 10, 18);
            g.DrawString($"Status:   {eintrag.Status}", fntSubKopf, brushAccentLt, statRect, sfRight);

            // Unterrand-Akzentlinie
            g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 190, 30)),
                pageLeft, yPos + headerH - 4, pageW, 4);
            yPos += headerH + 14;

            // ════════════════════════════════════════════════════════════
            // [2] INFO-BLOCK — zwei Spalten in abgerundetem Rahmen
            // ════════════════════════════════════════════════════════════
            float infoH = 90f;
            float colHalf = (pageW - 12) / 2f;

            // Linke Info-Box
            var boxL = new System.Drawing.RectangleF(pageLeft, yPos, colHalf, infoH);
            g.FillRectangle(brushLight, boxL);
            g.DrawRectangle(penBorder, boxL.X, boxL.Y, boxL.Width, boxL.Height);

            // Linker Box-Header
            g.FillRectangle(brushAccentLt, pageLeft, yPos, colHalf, 20);
            g.DrawString("MITARBEITER", fntSektTitel, brushAccent,
                new System.Drawing.RectangleF(pageLeft + 8, yPos + 1, colHalf - 10, 18), sfLeft);

            float infoY = yPos + 26;
            float lbl1 = pageLeft + 8;
            float val1 = pageLeft + 115;

            void InfoZeile(float x, float xv, float y, string lbl, string val, System.Drawing.Font fLbl, System.Drawing.Brush bVal)
            {
                g.DrawString(lbl, fLbl, brushGray,
                    new System.Drawing.RectangleF(x, y, xv - x - 4, 18), sfLeft);
                g.DrawString(val, fntWert, bVal,
                    new System.Drawing.RectangleF(xv, y, colHalf - (xv - pageLeft) - 8, 18), sfLeft);
            }

            InfoZeile(lbl1, val1, infoY, "Mitarbeiter:", eintrag.MitarbeiterName, fntLabel, brushDark);
            InfoZeile(lbl1, val1, infoY + 20, "Kürzel:", eintrag.MitarbeiterKuerzel, fntLabel, brushDark);
            InfoZeile(lbl1, val1, infoY + 40, "Ausgestellt von:", eintrag.GedrucktVon, fntLabel, brushGray);
            InfoZeile(lbl1, val1, infoY + 58, "Drucker:", eintrag.DruckerName, fntLabel, brushGray);

            // Rechte Info-Box
            float rx = pageLeft + colHalf + 12;
            var boxR = new System.Drawing.RectangleF(rx, yPos, colHalf, infoH);
            g.FillRectangle(brushLight, boxR);
            g.DrawRectangle(penBorder, boxR.X, boxR.Y, boxR.Width, boxR.Height);

            g.FillRectangle(brushAccentLt, rx, yPos, colHalf, 20);
            g.DrawString("AUSGABE-DETAILS", fntSektTitel, brushAccent,
                new System.Drawing.RectangleF(rx + 8, yPos + 1, colHalf - 10, 18), sfLeft);

            float lbl2 = rx + 8;
            float val2 = rx + 120;

            InfoZeile(lbl2, val2, infoY, "Datum:", eintrag.Datum.ToString("dd.MM.yyyy"), fntLabel, brushDark);
            InfoZeile(lbl2, val2, infoY + 20, "Uhrzeit:", eintrag.Datum.ToString("HH:mm") + " Uhr", fntLabel, brushDark);
            InfoZeile(lbl2, val2, infoY + 40, "Status:", eintrag.Status, fntLabel, brushDark);
            InfoZeile(lbl2, val2, infoY + 58, "Artikel gesamt:", eintrag.Artikel.Count.ToString() + " Stk.", fntLabel, brushDark);

            yPos += infoH + 18;

            // ════════════════════════════════════════════════════════════
            // [3] ARTIKEL-TABELLE
            // ════════════════════════════════════════════════════════════

            // Sektion-Überschrift
            g.FillRectangle(brushAccent,
                new System.Drawing.RectangleF(pageLeft, yPos, pageW, 22));
            g.DrawString("AUSGEGEBENE ARTIKEL", fntSektTitel, brushWhite,
                new System.Drawing.RectangleF(pageLeft + 8, yPos + 2, pageW - 16, 18), sfLeft);
            yPos += 22;

            // Spalten-Definitionen (Breiten relativ zu pageW)
            float cNr = 28f;
            float cInv = 90f;
            float cName = pageW - cNr - cInv - 130f - 40f - 110f;  // flexibel
            float cSerie = 130f;
            float cAnz = 40f;
            float cBem = 110f;
            float rowH = 22f;

            float xNr = pageLeft;
            float xInv = xNr + cNr;
            float xName = xInv + cInv;
            float xSerie = xName + cName;
            float xAnz = xSerie + cSerie;
            float xBem = xAnz + cAnz;

            // Tabellenkopf
            float thY = yPos;
            g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(50, 65, 90)),
                pageLeft, thY, pageW, rowH);

            void TblHead(float x, float w, string txt)
                => g.DrawString(txt, fntTblHead, brushWhite,
                    new System.Drawing.RectangleF(x + 4, thY + 2, w - 6, rowH - 4), sfLeft);

            TblHead(xNr, cNr, "#");
            TblHead(xInv, cInv, "Inventar-Nr.");
            TblHead(xName, cName, "Gerätename");
            TblHead(xSerie, cSerie, "Seriennummer");
            TblHead(xAnz, cAnz, "Anz.");
            TblHead(xBem, cBem, "Bemerkung");

            // Vertikale Spaltenlinien im Header
            var penWhiteThin = new System.Drawing.Pen(System.Drawing.Color.FromArgb(80, 255, 255, 255), 0.5f);
            foreach (float cx in new[] { xInv, xName, xSerie, xAnz, xBem })
                g.DrawLine(penWhiteThin, cx, thY, cx, thY + rowH);

            yPos += rowH;

            // Zeilen
            for (int i = 0; i < eintrag.Artikel.Count; i++)
            {
                var a = eintrag.Artikel[i];
                var rowBg = (i % 2 == 0)
                    ? brushWhite
                    : brushLight;

                var rowRect = new System.Drawing.RectangleF(pageLeft, yPos, pageW, rowH);
                g.FillRectangle(rowBg, rowRect);
                g.DrawRectangle(penThin, rowRect.X, rowRect.Y, rowRect.Width, rowRect.Height);

                // Vertikale Spaltenlinien
                foreach (float cx in new[] { xInv, xName, xSerie, xAnz, xBem })
                    g.DrawLine(penBorder, cx, yPos, cx, yPos + rowH);

                // Nr. (zentriert)
                g.DrawString($"{i + 1}", fntTbl, brushGray,
                    new System.Drawing.RectangleF(xNr, yPos, cNr, rowH), sfCenter);

                // Inventar-Nr.
                g.DrawString(a.InvNmr ?? "-", fntTbl, brushDark,
                    new System.Drawing.RectangleF(xInv + 4, yPos, cInv - 6, rowH), sfLeft);

                // Gerätename — ggf. kürzen
                string gname = a.GeraeteName ?? "-";
                var measRect = new System.Drawing.RectangleF(xName + 4, yPos, cName - 8, rowH);
                while (gname.Length > 1 && g.MeasureString(gname + "…", fntTbl).Width > cName - 10)
                    gname = gname.Substring(0, gname.Length - 1);
                if (gname != (a.GeraeteName ?? "-")) gname += "…";
                g.DrawString(gname, fntTbl, brushDark, measRect, sfLeft);

                // Seriennummer
                g.DrawString(a.SerienNummer ?? "—", fntTbl, brushGray,
                    new System.Drawing.RectangleF(xSerie + 4, yPos, cSerie - 6, rowH), sfLeft);

                // Anzahl (zentriert)
                g.DrawString(a.Anzahl.ToString(), fntTbl, brushDark,
                    new System.Drawing.RectangleF(xAnz, yPos, cAnz, rowH), sfCenter);

                // Bemerkung
                string bem = a.Bemerkung ?? "—";
                while (bem.Length > 1 && g.MeasureString(bem + "…", fntTbl).Width > cBem - 10)
                    bem = bem.Substring(0, bem.Length - 1);
                if (bem != (a.Bemerkung ?? "—")) bem += "…";
                g.DrawString(bem, fntTbl, brushGray,
                    new System.Drawing.RectangleF(xBem + 4, yPos, cBem - 6, rowH), sfLeft);

                yPos += rowH;
            }

            // Abschluss-Zeile Tabelle
            g.FillRectangle(brushAccentLt,
                new System.Drawing.RectangleF(pageLeft, yPos, pageW, 20));
            g.DrawRectangle(penAccent, pageLeft, yPos, pageW, 20);
            g.DrawString($"Gesamt: {eintrag.Artikel.Count} Artikel", fntSmallBold, brushAccent,
                new System.Drawing.RectangleF(pageLeft + 8, yPos + 2, pageW - 16, 16), sfLeft);
            yPos += 30;

            // ════════════════════════════════════════════════════════════
            // [4] UNTERSCHRIFTEN — nebeneinander in Boxen
            // ════════════════════════════════════════════════════════════
            yPos += 10;
            float sigH = 90f;
            float sigW = (pageW - 16) / 2f;
            float sigGap = 16f;

            void SignaturBox(float sx, string titel, string name, string datum)
            {
                var bx = new System.Drawing.RectangleF(sx, yPos, sigW, sigH);
                g.FillRectangle(brushLight, bx);
                g.DrawRectangle(penBorder, bx.X, bx.Y, bx.Width, bx.Height);

                // Box-Titel
                g.FillRectangle(brushAccentLt, sx, yPos, sigW, 20);
                g.DrawString(titel, fntSektTitel, brushAccent,
                    new System.Drawing.RectangleF(sx + 8, yPos + 2, sigW - 12, 16), sfLeft);

                // Unterschrift-Linie
                float lineY = yPos + 65;
                g.DrawLine(new System.Drawing.Pen(colDark, 1f), sx + 14, lineY, sx + sigW - 14, lineY);

                // Name + Datum darunter
                g.DrawString(name, fntSmall, brushGray,
                    new System.Drawing.RectangleF(sx + 14, lineY + 3, sigW * 0.55f, 16), sfLeft);
                g.DrawString(datum, fntSmall, brushGray,
                    new System.Drawing.RectangleF(sx + 14, lineY + 3, sigW - 20, 16), sfRight);
            }

            SignaturBox(pageLeft, "UNTERSCHRIFT — MITARBEITER", eintrag.MitarbeiterName, eintrag.Datum.ToString("dd.MM.yyyy"));
            SignaturBox(pageLeft + sigW + sigGap, "UNTERSCHRIFT — AUSGABE-PERSON", eintrag.GedrucktVon, eintrag.Datum.ToString("dd.MM.yyyy"));

            yPos += sigH;

            // ════════════════════════════════════════════════════════════
            // [5] FUSSZEILE
            // ════════════════════════════════════════════════════════════
            float footY = e.PageBounds.Height - e.MarginBounds.Bottom + e.MarginBounds.Top - 28;
            // Sichere Fußzeile am unteren Rand
            footY = e.PageBounds.Height - 40;

            g.FillRectangle(brushAccent,
                new System.Drawing.RectangleF(pageLeft, footY, pageW, 24));
            g.DrawString(
                $"Inventarverwaltung  |  Dok.-ID: {eintrag.Id}  |  Gedruckt am: {eintrag.Datum:dd.MM.yyyy HH:mm}  |  Drucker: {eintrag.DruckerName}",
                fntSmall, brushAccentLt,
                new System.Drawing.RectangleF(pageLeft + 8, footY + 4, pageW - 16, 16), sfLeft);

            // ── Ressourcen freigeben ───────────────────────────────────
            foreach (var obj in new System.IDisposable[] {
                brushDark, brushAccent, brushAccentLt, brushGray, brushWhite, brushLight,
                penAccent, penBorder, penDark, penThin, penWhiteThin,
                fntKopf, fntSubKopf, fntSektTitel, fntLabel, fntWert,
                fntTblHead, fntTbl, fntSmall, fntSmallBold, sfLeft, sfCenter, sfRight })
                obj.Dispose();
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