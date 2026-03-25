using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.Data;
using Inventarverwaltung.Manager.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventarverwaltung
{
    // ══════════════════════════════════════════════════════════════════════════
    //  BERECHTIGUNGS-GUARD — Punkte 4 & 13
    //
    //  PROBLEM: AppRouter ruft command.Execute() ohne Berechtigungsprüfung.
    //  Normaler User kann löschen, exportieren, Benutzer anlegen.
    //
    //  LÖSUNG: AppRouter ruft vor Execute() PruefeUndFuehreAus() auf.
    //  Admin-only Keys werden für normale User komplett ausgeblendet.
    // ══════════════════════════════════════════════════════════════════════════

    public static class BerechtigungsGuard
    {
        // Keys die Admin erfordern. Alles was NICHT hier steht → für alle offen.
        private static readonly HashSet<string> _adminOnly =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Benutzerverwaltung
            "USER_NEU",
            "USER_ZEIGE",
            // Löschen + Bearbeiten
            "TOOL_DEL",
            "TOOL_EDIT",
            // Import
            "TOOL_IMPORT",
            // Export: Benutzer & Vollexport (Punkt 13)
            "EXPORT_BENUTZER",
            "EXPORT_ALLES",
            // System
            "SYS_LOG",
            "SYS_REP",
            "SYS_GUARD",
            "SYS_AUDIT",
            "SYS_ENC",
            "SYS_NOTFALL_PW",
            // Mindestbestand
            "BEST_MIN",
            // Mitarbeiter anlegen
            "MA_NEU",
        };

        /// <summary>
        /// Prüft Berechtigung und führt Execute() aus wenn erlaubt.
        /// Vom AppRouter aufzurufen statt direktem command.Execute().
        /// </summary>
        public static bool PruefeUndFuehreAus(Core.ICommand command)
        {
            if (IstAdminOnly(command.Key) && !IstAdmin())
            {
                ZeigeVerweigert(command);
                return false;
            }
            command.Execute();
            return true;
        }

        public static bool IstAdminOnly(string commandKey)
            => _adminOnly.Contains(commandKey);

        public static bool IstAdmin()
        {
            if (string.IsNullOrEmpty(AuthManager.AktuellerBenutzer))
                return false;
            var b = DataManager.Benutzer.FirstOrDefault(u =>
                u.Benutzername.Equals(AuthManager.AktuellerBenutzer,
                    StringComparison.OrdinalIgnoreCase));
            return b?.Berechtigung == Berechtigungen.Admin;
        }

        private static void ZeigeVerweigert(Core.ICommand command)
        {
            Console.Clear();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║   🚫  ZUGRIFF VERWEIGERT                                          ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            string lbl = Kuerze(command.Label ?? "", 53);
            string usr = Kuerze((AuthManager.AktuellerBenutzer ?? "?") + "  (👤 User)", 53);
            Console.WriteLine($"  ║   Aktion:   {lbl} ║");
            Console.WriteLine($"  ║   Account:  {usr} ║");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("  ║   Diese Aktion ist nur für Administratoren freigegeben.          ║");
            Console.WriteLine("  ║   Wenden Sie sich an einen Administrator.                        ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            LogManager.LogFehler("BERECHTIGUNG.VERWEIGERT",
                $"'{AuthManager.AktuellerBenutzer}' → unerlaubt: " +
                $"[{command.Key}] '{command.Label}'");

            ConsoleHelper.PressKeyToContinue();
        }

        private static string Kuerze(string s, int len)
        {
            if (s.Length >= len) return s.Substring(0, len);
            return s.PadRight(len);
        }
    }
}