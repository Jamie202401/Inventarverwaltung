using System;
using System.Text.RegularExpressions;

namespace Inventarverwaltung
{
    // ══════════════════════════════════════════════════════════════════════════
    //
    //   INPUT-VALIDATOR  —  Schutz gegen Injection & Datenkorrumption
    //
    //   PROBLEM (Punkt 8):
    //     Benutzereingaben wurden direkt in Dateien geschrieben ohne
    //     Prüfung auf Sonderzeichen. Das Trennzeichen ';' in den CSV-Dateien
    //     konnte durch absichtliche Eingaben die Datenstruktur korrumpieren.
    //     Beispiel: Benutzername "admin;Admin" → erzeugt zweiten Admin-Eintrag.
    //
    //   LÖSUNG:
    //     Alle Eingaben werden vor der Verwendung durch ValidiereXxx() geprüft.
    //     Folgende Zeichen werden in Benutzereingaben blockiert:
    //       ';'  → Trennzeichen der CSV-Dateien
    //       '\n' '\r'  → Zeilenumbrüche → neue Zeilen in Dateien
    //       '\0'  → Null-Byte → String-Termination-Angriff
    //       '|'  → Alternativ-Trennzeichen
    //       '#'  → Kommentar-Zeichen (würde Zeile als Header maskieren)
    //       '╔' '║' '╚' '═'  → Header-Rahmen-Zeichen
    //       '[' ']'  → Section-Marker wie [DATEN]
    //
    //   VERWENDUNG:
    //     string bereinigt = InputValidator.ValidiereBenutzername(eingabe);
    //     if (bereinigt == null) { /* Fehler anzeigen */ }
    //
    // ══════════════════════════════════════════════════════════════════════════

    public static class InputValidator
    {
        // ── Erlaubte Zeichen für verschiedene Felder ──────────────────────────

        // Benutzername: Nur Buchstaben, Zahlen, Unterstrich, Bindestrich, Punkt
        private static readonly Regex _regexBenutzername =
            new Regex(@"^[a-zA-ZäöüÄÖÜß0-9_\-\.]{3,50}$", RegexOptions.Compiled);

        // Inventarnummer: Buchstaben, Zahlen, Bindestrich
        private static readonly Regex _regexInvNr =
            new Regex(@"^[a-zA-Z0-9\-]{1,20}$", RegexOptions.Compiled);

        // Allgemeiner Text (Namen, Bezeichnungen): Alles außer Steuerzeichen
        private static readonly Regex _regexAllgemeinerText =
            new Regex(@"^[^;\n\r\0|#\[\]╔║╚═]{1,100}$", RegexOptions.Compiled);

        // Abteilung / Kategorie: Buchstaben, Zahlen, Leerzeichen, Bindestrich
        private static readonly Regex _regexKategorie =
            new Regex(@"^[a-zA-ZäöüÄÖÜß0-9\s\-\.\/]{1,50}$", RegexOptions.Compiled);

        // Seriennummer: Buchstaben, Zahlen, Bindestrich, Schrägstrich
        private static readonly Regex _regexSerienNr =
            new Regex(@"^[a-zA-Z0-9\-\/\.]{1,50}$", RegexOptions.Compiled);

        // Pfad für Export: Nur gültige Dateipfad-Zeichen
        private static readonly Regex _regexPfad =
            new Regex(@"^[a-zA-ZäöüÄÖÜß0-9\s\-_\.\\/:()\[\]]{1,260}$", RegexOptions.Compiled);

        // ── Gefährliche Zeichen die IMMER blockiert werden ────────────────────
        private static readonly char[] _geblockteFuerAlles = {
            ';', '\n', '\r', '\0', '|', '#'
        };

        private static readonly string[] _geblockteSektionsMarker = {
            "[DATEN]", "╔", "║", "╚", "═══", "───",
            "INVENTAR-DATENBANK", "BENUTZER-DATENBANK", "MITARBEITER-DATENBANK"
        };

        // ══════════════════════════════════════════════════════════════════
        // ÖFFENTLICHE VALIDIERUNGS-METHODEN
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Validiert einen Benutzernamen.
        /// Erlaubt: Buchstaben, Zahlen, _ - .  (3–50 Zeichen)
        /// Gibt null zurück wenn ungültig.
        /// </summary>
        public static string ValidiereBenutzername(string eingabe)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            string bereinigt = eingabe.Trim();

            if (!_regexBenutzername.IsMatch(bereinigt))
                return null;

            if (EnthältGefährlicheSequenz(bereinigt))
                return null;

            return bereinigt;
        }

        /// <summary>
        /// Validiert eine Inventarnummer.
        /// Erlaubt: Buchstaben, Zahlen, -  (1–20 Zeichen)
        /// </summary>
        public static string ValidiereInvNr(string eingabe)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            string bereinigt = eingabe.Trim().ToUpperInvariant();

            if (!_regexInvNr.IsMatch(bereinigt))
                return null;

            return bereinigt;
        }

        /// <summary>
        /// Validiert allgemeinen Text (Gerätename, Hersteller, Mitarbeitername etc.)
        /// Blockiert: ; | # [ ] Zeilenumbrüche Null-Bytes Header-Rahmen
        /// </summary>
        public static string ValidiereText(string eingabe, int maxLaenge = 100)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            string bereinigt = eingabe.Trim();

            if (bereinigt.Length > maxLaenge) return null;

            // Auf einzelne gefährliche Zeichen prüfen
            foreach (char c in _geblockteFuerAlles)
            {
                if (bereinigt.Contains(c))
                    return null;
            }

            // Auf gefährliche Sequenzen prüfen
            if (EnthältGefährlicheSequenz(bereinigt))
                return null;

            return bereinigt;
        }

        /// <summary>
        /// Validiert eine Seriennummer.
        /// </summary>
        public static string ValidiereSerienNr(string eingabe)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            string bereinigt = eingabe.Trim();

            if (!_regexSerienNr.IsMatch(bereinigt))
                return null;

            return bereinigt;
        }

        /// <summary>
        /// Validiert eine Kategorie / Abteilung.
        /// </summary>
        public static string ValidiereKategorie(string eingabe)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            string bereinigt = eingabe.Trim();

            if (!_regexKategorie.IsMatch(bereinigt))
                return null;

            if (EnthältGefährlicheSequenz(bereinigt))
                return null;

            return bereinigt;
        }

        /// <summary>
        /// Validiert eine Dezimalzahl (z.B. Preis).
        /// Gibt null zurück wenn kein gültiger Preis.
        /// </summary>
        public static decimal? ValidiereDecimal(string eingabe, decimal min = 0m, decimal max = 9_999_999m)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            // Komma → Punkt normalisieren
            string normalisiert = eingabe.Trim()
                .Replace(',', '.')
                .Replace(" ", "");

            if (!decimal.TryParse(normalisiert,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal wert))
                return null;

            if (wert < min || wert > max) return null;

            return wert;
        }

        /// <summary>
        /// Validiert eine Ganzzahl.
        /// </summary>
        public static int? ValidiereInt(string eingabe, int min = 0, int max = 999_999)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            if (!int.TryParse(eingabe.Trim(), out int wert)) return null;

            if (wert < min || wert > max) return null;

            return wert;
        }

        /// <summary>
        /// Validiert ein Datum (Format: dd.MM.yyyy).
        /// </summary>
        public static DateTime? ValidiereDatum(string eingabe)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            if (DateTime.TryParseExact(eingabe.Trim(), "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime datum))
                return datum;

            return null;
        }

        /// <summary>
        /// Validiert einen Export-Dateipfad.
        /// Blockiert Pfad-Traversal-Angriffe (../).
        /// </summary>
        public static string ValidiereExportPfad(string eingabe)
        {
            if (string.IsNullOrWhiteSpace(eingabe)) return null;

            string bereinigt = eingabe.Trim();

            // Pfad-Traversal blockieren
            if (bereinigt.Contains("..") || bereinigt.Contains("~"))
                return null;

            // Null-Bytes blockieren
            if (bereinigt.Contains('\0'))
                return null;

            return bereinigt;
        }

        // ══════════════════════════════════════════════════════════════════
        // FEHLERMELDUNGEN — einheitlich für UI
        // ══════════════════════════════════════════════════════════════════

        public static string FehlermeldungBenutzername =>
            "Benutzername: 3-50 Zeichen, nur Buchstaben/Zahlen/_ - .  Keine Sonderzeichen.";

        public static string FehlermeldungText =>
            "Ungültige Zeichen. Nicht erlaubt: ; | # [ ] sowie Zeilenumbrüche.";

        public static string FehlermeldungInvNr =>
            "Inventarnummer: Nur Buchstaben, Zahlen und - (max. 20 Zeichen).";

        public static string FehlermeldungSerienNr =>
            "Seriennummer: Nur Buchstaben, Zahlen, - / . (max. 50 Zeichen).";

        public static string FehlermeldungDecimal =>
            "Ungültige Zahl. Beispiele: 1234.50 oder 1234,50";

        public static string FehlermeldungInt =>
            "Ungültige Ganzzahl oder Wert außerhalb des erlaubten Bereichs.";

        public static string FehlermeldungDatum =>
            "Ungültiges Datum. Bitte im Format TT.MM.JJJJ eingeben (z.B. 15.03.2024).";

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE HILFSMETHODEN
        // ══════════════════════════════════════════════════════════════════

        private static bool EnthältGefährlicheSequenz(string text)
        {
            foreach (string seq in _geblockteSektionsMarker)
            {
                if (text.IndexOf(seq, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}