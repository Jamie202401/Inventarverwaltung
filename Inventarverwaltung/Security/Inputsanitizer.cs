using Inventarverwaltung.Manager.Auth;
using System;
using System.Text;

namespace Inventarverwaltung
{
    // ══════════════════════════════════════════════════════════════════════════
    //  INPUT-SANITIZER — Punkt 8: Injection-Schutz
    //
    //  PROBLEM: Eingabe "admin;Admin" → direkt in Accounts.txt →
    //  Parser liest zwei Felder → eigener Admin-Account ohne Programm-Logik.
    //
    //  LÖSUNG: Alle Benutzereingaben laufen durch diese Klasse.
    //  Trennzeichen werden ersetzt, Steuerzeichen entfernt, Längen erzwungen,
    //  verdächtige Patterns im Log erfasst.
    // ══════════════════════════════════════════════════════════════════════════

    public static class InputSanitizer
    {
        public const int MaxBenutzername = 30;
        public const int MaxInvNummer = 15;
        public const int MaxGeraeteName = 60;
        public const int MaxFreitext = 100;
        public const int MaxPfad = 260;

        private const char Trennzeichen = ';';

        // ── Allgemeiner Freitext ──────────────────────────────────────────────
        public static string Bereinige(string eingabe, int maxLen = MaxFreitext)
        {
            if (string.IsNullOrEmpty(eingabe)) return string.Empty;
            if (IstVerdaechtig(eingabe)) LogVerdacht(eingabe);

            var sb = new StringBuilder(eingabe.Length);
            foreach (char c in eingabe)
            {
                if (c == Trennzeichen) sb.Append('-');
                else if (c < 32 && c != 9) continue;
                else sb.Append(c);
            }

            string r = sb.ToString().Trim();
            return r.Length > maxLen ? r.Substring(0, maxLen).TrimEnd() : r;
        }

        // ── Benutzername: nur [a-zA-Z0-9._-] ─────────────────────────────────
        public static string BereinigeBenutzername(string eingabe)
        {
            if (string.IsNullOrEmpty(eingabe)) return string.Empty;
            if (IstVerdaechtig(eingabe)) LogVerdacht(eingabe);

            var sb = new StringBuilder();
            foreach (char c in eingabe)
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.')
                    sb.Append(c);

            string r = sb.ToString().Trim();
            return r.Length > MaxBenutzername ? r.Substring(0, MaxBenutzername) : r;
        }

        // ── Inventarnummer: nur [A-Z0-9-] ────────────────────────────────────
        public static string BereinigeInvNummer(string eingabe)
        {
            if (string.IsNullOrEmpty(eingabe)) return string.Empty;
            if (IstVerdaechtig(eingabe)) LogVerdacht(eingabe);

            var sb = new StringBuilder();
            foreach (char c in eingabe)
                if (char.IsLetterOrDigit(c) || c == '-')
                    sb.Append(char.ToUpperInvariant(c));

            string r = sb.ToString().Trim();
            return r.Length > MaxInvNummer ? r.Substring(0, MaxInvNummer) : r;
        }

        // ── Alias für Freitext mit angepasster Länge ─────────────────────────
        public static string BereinigeFreitext(string eingabe, int maxLen = MaxGeraeteName)
            => Bereinige(eingabe, maxLen);

        // ── Dezimalzahl ───────────────────────────────────────────────────────
        public static string BereinigeDecimal(string eingabe)
        {
            if (string.IsNullOrEmpty(eingabe)) return "0";
            var sb = new StringBuilder();
            bool hatPunkt = false;
            foreach (char c in eingabe)
            {
                if (char.IsDigit(c))
                    sb.Append(c);
                else if ((c == ',' || c == '.') && !hatPunkt)
                {
                    sb.Append('.');
                    hatPunkt = true;
                }
            }
            return sb.Length > 0 ? sb.ToString() : "0";
        }

        // ── Pfad: kein Path-Traversal ─────────────────────────────────────────
        public static string BereinigePfad(string eingabe)
        {
            if (string.IsNullOrEmpty(eingabe)) return string.Empty;
            if (eingabe.Contains("..") || eingabe.Contains("//") ||
                eingabe.Contains("\\\\"))
                LogVerdacht($"Path-Traversal: {eingabe}");
            return Bereinige(System.IO.Path.GetFileName(eingabe), MaxPfad);
        }

        // ── Interne Helfer ────────────────────────────────────────────────────
        private static bool IstVerdaechtig(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            if (s.Split(';').Length > 2) return true;
            string l = s.ToLowerInvariant();
            if (l.Contains("[daten]") || l.Contains("[header]")) return true;
            if (s.Contains('\0')) return true;
            if (s.Length > 500) return true;
            return false;
        }

        private static void LogVerdacht(string eingabe)
        {
            try
            {
                string kurz = eingabe.Length > 60
                    ? eingabe.Substring(0, 60) + "…" : eingabe;
                LogManager.LogFehler("InputSanitizer.VERDACHT",
                    $"Verdächtige Eingabe: '{kurz}' | " +
                    $"Benutzer: {AuthManager.AktuellerBenutzer ?? "?"}");
            }
            catch { }
        }
    }
}