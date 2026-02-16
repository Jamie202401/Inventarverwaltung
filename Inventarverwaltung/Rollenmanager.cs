using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Inventarverwaltung.Rolle;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Rollen, Berechtigungen und Benutzerzuweisungen
    /// </summary>
    public static class RollenManager
    {
        // ─────────────────────────────────────────────────────────────────────
        // INTERNE DATEN
        // ─────────────────────────────────────────────────────────────────────

        private static List<Rolle> _rollen = new List<Rolle>();
        private const string RollenDatei = "Rollen.txt";

        // ─────────────────────────────────────────────────────────────────────
        // INIT
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Lädt Rollen aus Datei oder erstellt Standard-Rollen beim ersten Start
        /// </summary>
        public static void Initialisieren()
        {
            LadeRollen();

            if (_rollen.Count == 0)
            {
                ErstelleStandardRollen();
                SpeichereRollen();
            }
        }

        private static void ErstelleStandardRollen()
        {
            // Admin-Rolle: alle Rechte
            var adminRolle = new Rolle("Admin", "👑", ConsoleColor.Red)
            {
                Rechte = new RollenRechte
                {
                    RollenVerwalten = true,
                    BenutzerVerwalten = true,
                    MitarbeiterBearbeiten = true,
                    MitarbeiterLoeschen = true,
                    InventarBearbeiten = true,
                    InventarLoeschen = true,
                    BestandAendern = true,
                    DruckVerwalten = true,
                    ImportExport = true,
                    SystemLog = true,
                    Verschluesselung = true,
                    KIDashboard = true,
                    Schnellerfassung = true
                }
            };

            // User-Rolle: nur Lesen & Hinzufügen
            var userRolle = new Rolle("User", "👤", ConsoleColor.Cyan)
            {
                Rechte = new RollenRechte
                {
                    RollenVerwalten = false,
                    BenutzerVerwalten = false,
                    MitarbeiterBearbeiten = false,
                    MitarbeiterLoeschen = false,
                    InventarBearbeiten = false,
                    InventarLoeschen = false,
                    BestandAendern = true,
                    DruckVerwalten = false,
                    ImportExport = false,
                    SystemLog = false,
                    Verschluesselung = false,
                    KIDashboard = true,
                    Schnellerfassung = true
                }
            };

            // Lager-Rolle: Bestand & Inventar
            var lagerRolle = new Rolle("Lager", "📦", ConsoleColor.Green)
            {
                Rechte = new RollenRechte
                {
                    RollenVerwalten = false,
                    BenutzerVerwalten = false,
                    MitarbeiterBearbeiten = false,
                    MitarbeiterLoeschen = false,
                    InventarBearbeiten = true,
                    InventarLoeschen = false,
                    BestandAendern = true,
                    DruckVerwalten = true,
                    ImportExport = true,
                    SystemLog = false,
                    Verschluesselung = false,
                    KIDashboard = true,
                    Schnellerfassung = true
                }
            };

            _rollen.Add(adminRolle);
            _rollen.Add(userRolle);
            _rollen.Add(lagerRolle);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ROLLEN ANZEIGEN
        // ─────────────────────────────────────────────────────────────────────

        public static void ZeigeRollenUebersicht()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🔲 Rollen Details — Übersicht", ConsoleColor.Magenta);

            if (_rollen.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Rollen vorhanden. Initialisierung...");
                Initialisieren();
            }

            Console.WriteLine();

            // Tabellenkopf
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ║   ALLE ROLLEN & IHRE BERECHTIGUNGEN                                       ║");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var rolle in _rollen)
            {
                ZeigeRolleDetail(rolle);
                Console.WriteLine();
            }

            // Benutzer-Zuweisung anzeigen
            ZeigeBenutzerZuweisungsTabelle();

            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeRolleDetail(Rolle rolle)
        {
            Console.ForegroundColor = rolle.Farbe;
            Console.WriteLine($"  ┌─ {rolle.Icon} {rolle.Name.ToUpper()} ───────────────────────────────────────────");
            Console.ResetColor();

            var rechte = rolle.Rechte;
            var alleRechte = GetAlleRechteDefinitionen();

            int col = 0;
            foreach (var r in alleRechte)
            {
                bool wert = r.Getter(rechte);
                string haken = wert ? "✅" : "❌";

                Console.ForegroundColor = wert ? ConsoleColor.Green : ConsoleColor.DarkGray;
                string zeile = $"  │  {haken} {r.Anzeigename,-30}";

                if (col == 0)
                {
                    Console.Write(zeile);
                    col = 1;
                }
                else
                {
                    Console.WriteLine(zeile);
                    col = 0;
                }
            }

            if (col == 1) Console.WriteLine();
            Console.ResetColor();

            // Zähle Benutzer dieser Rolle
            int anzahl = DataManager.Benutzer.Count(b =>
                b.Berechtigung.ToString().Equals(rolle.Name, StringComparison.OrdinalIgnoreCase));

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  └─ Zugewiesene Benutzer: {anzahl}");
            Console.ResetColor();
        }

        private static void ZeigeBenutzerZuweisungsTabelle()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ║   BENUTZER-ZUWEISUNGEN                                                    ║");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            if (DataManager.Benutzer.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Benutzer vorhanden.");
                return;
            }

            // Tabellenkopf
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  {"Benutzername",-22} {"Rolle",-15} {"Rechte-Kurzinfo",-35}");
            Console.WriteLine("  " + new string('─', 72));
            Console.ResetColor();

            foreach (var benutzer in DataManager.Benutzer)
            {
                var rolle = _rollen.FirstOrDefault(r =>
                    r.Name.Equals(benutzer.Berechtigung.ToString(), StringComparison.OrdinalIgnoreCase));

                string rollenIcon = rolle?.Icon ?? "❓";
                string rollenName = benutzer.Berechtigung.ToString();
                int rechteAnzahl = rolle != null ? ZaehleAktiveRechte(rolle.Rechte) : 0;

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  {benutzer.Benutzername,-22}");

                Console.ForegroundColor = rolle?.Farbe ?? ConsoleColor.Gray;
                Console.Write($" {rollenIcon} {rollenName,-13}");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($" {rechteAnzahl} von 13 Rechten aktiv");
                Console.ResetColor();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // NEUE ROLLE ERSTELLEN
        // ─────────────────────────────────────────────────────────────────────

        public static void NeueRolleErstellen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("➕ Neue Rolle erstellen", ConsoleColor.Green);

            // Name eingeben
            string rollenName;
            while (true)
            {
                rollenName = ConsoleHelper.GetInput("Rollenname");

                if (string.IsNullOrWhiteSpace(rollenName))
                {
                    ConsoleHelper.PrintError("Rollenname darf nicht leer sein!");
                    continue;
                }
                if (rollenName.Length < 2)
                {
                    ConsoleHelper.PrintError("Rollenname muss mindestens 2 Zeichen haben!");
                    continue;
                }
                if (_rollen.Any(r => r.Name.Equals(rollenName, StringComparison.OrdinalIgnoreCase)))
                {
                    ConsoleHelper.PrintError($"Rolle '{rollenName}' existiert bereits!");
                    continue;
                }
                break;
            }

            // Icon wählen
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Icons:");
            Console.ResetColor();
            string[] icons = { "🔵", "🟢", "🟡", "🟠", "🔴", "🟣", "⚫", "⚪", "🌟", "⚡", "🔧", "📋" };
            for (int i = 0; i < icons.Length; i++)
                Console.WriteLine($"  [{i + 1}] {icons[i]}");

            string iconEingabe = ConsoleHelper.GetInput("Icon wählen (1-12, Enter für Standard)");
            string gewaehltesIcon = "🔲";
            if (int.TryParse(iconEingabe, out int iconIdx) && iconIdx >= 1 && iconIdx <= icons.Length)
                gewaehltesIcon = icons[iconIdx - 1];

            // Farbe wählen
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Farbe wählen:");
            Console.ResetColor();
            var farben = new (string Name, ConsoleColor Farbe)[]
            {
                ("Cyan",    ConsoleColor.Cyan),
                ("Grün",    ConsoleColor.Green),
                ("Gelb",    ConsoleColor.Yellow),
                ("Magenta", ConsoleColor.Magenta),
                ("Blau",    ConsoleColor.Blue),
                ("Weiß",    ConsoleColor.White),
            };
            for (int i = 0; i < farben.Length; i++)
            {
                Console.ForegroundColor = farben[i].Farbe;
                Console.WriteLine($"  [{i + 1}] {farben[i].Name}");
            }
            Console.ResetColor();

            string farbEingabe = ConsoleHelper.GetInput("Farbe wählen (1-6, Enter für Cyan)");
            ConsoleColor gewaehlteFarbe = ConsoleColor.Cyan;
            if (int.TryParse(farbEingabe, out int farbIdx) && farbIdx >= 1 && farbIdx <= farben.Length)
                gewaehlteFarbe = farben[farbIdx - 1].Farbe;

            // Rechte konfigurieren
            Console.WriteLine();
            ConsoleHelper.PrintInfo("Jetzt die Berechtigungen für diese Rolle festlegen:");
            Console.WriteLine();

            var rechte = KonfigurierereRechte(new RollenRechte());

            // Rolle erstellen und speichern
            var neueRolle = new Rolle(rollenName, gewaehltesIcon, gewaehlteFarbe)
            {
                Rechte = rechte
            };

            _rollen.Add(neueRolle);
            SpeichereRollen();

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"Rolle '{gewaehltesIcon} {rollenName}' wurde erfolgreich erstellt!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  → Aktive Rechte: {ZaehleAktiveRechte(rechte)} von 13");
            Console.ResetColor();

            LogManager.LogDatenGespeichert("Rollen", $"Neue Rolle erstellt: {rollenName}");
            ConsoleHelper.PressKeyToContinue();
        }

        // ─────────────────────────────────────────────────────────────────────
        // ROLLE BEARBEITEN
        // ─────────────────────────────────────────────────────────────────────

        public static void RolleBearbeiten()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("✏️  Rolle bearbeiten", ConsoleColor.Yellow);

            var rolle = WaehlRolle("bearbeiten");
            if (rolle == null) return;

            Console.WriteLine();
            Console.ForegroundColor = rolle.Farbe;
            Console.WriteLine($"  {rolle.Icon} Berechtigungen für Rolle: {rolle.Name}");
            Console.ResetColor();
            Console.WriteLine();
            ConsoleHelper.PrintInfo("Aktuelle Einstellungen werden angezeigt. Neue Werte festlegen:");
            Console.WriteLine();

            rolle.Rechte = KonfigurierereRechte(rolle.Rechte);
            SpeichereRollen();

            ConsoleHelper.PrintSuccess($"Rolle '{rolle.Name}' wurde aktualisiert!");
            LogManager.LogDatenGespeichert("Rollen", $"Rolle bearbeitet: {rolle.Name}");
            ConsoleHelper.PressKeyToContinue();
        }

        // ─────────────────────────────────────────────────────────────────────
        // BENUTZER-BERECHTIGUNGEN VERWALTEN (Tabellen-Übersicht mit Haken)
        // ─────────────────────────────────────────────────────────────────────

        public static void BenutzerBerechtigungenVerwalten()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("👤 Benutzer-Berechtigungen verwalten", ConsoleColor.Cyan);

            if (DataManager.Benutzer.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Benutzer vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("👤 Benutzer-Berechtigungen verwalten", ConsoleColor.Cyan);

                // Übersichtstabelle mit allen Benutzern und ihren Rechten
                ZeigeBenutzerRechteTabelle();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  Was möchten Sie tun?");
                Console.ResetColor();
                Console.WriteLine("  [1] Benutzer wählen und Rolle ändern");
                Console.WriteLine("  [0] Zurück");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  ▶ Auswahl: ");
                Console.ResetColor();

                string eingabe = Console.ReadLine()?.Trim() ?? "";

                if (eingabe == "0") return;

                if (eingabe == "1")
                {
                    BenutzerRolleAendern();
                }
                else
                {
                    ConsoleHelper.PrintError("Ungültige Eingabe!");
                    System.Threading.Thread.Sleep(800);
                }
            }
        }

        private static void ZeigeBenutzerRechteTabelle()
        {
            var alleRechte = GetAlleRechteDefinitionen();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ║   BENUTZER · ROLLEN · BERECHTIGUNGEN                                                ║");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Spaltenbreite Benutzer
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"  {"Nr",-4} {"Benutzername",-20} {"Rolle",-12}");

            // Kurzname jedes Rechts als Spalte
            foreach (var r in alleRechte)
            {
                Console.Write($" {TruncateRight(r.Kurzname, 5),5}");
            }
            Console.WriteLine();
            Console.WriteLine("  " + new string('─', 4 + 20 + 12 + (alleRechte.Count * 6) + 4));
            Console.ResetColor();

            // Jeder Benutzer = eine Zeile
            for (int i = 0; i < DataManager.Benutzer.Count; i++)
            {
                var benutzer = DataManager.Benutzer[i];
                var rolle = _rollen.FirstOrDefault(r =>
                    r.Name.Equals(benutzer.Berechtigung.ToString(), StringComparison.OrdinalIgnoreCase));

                string rollenIcon = rolle?.Icon ?? "❓";
                string rollenName = benutzer.Berechtigung.ToString();

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  [{i + 1}]  {benutzer.Benutzername,-20}");

                Console.ForegroundColor = rolle?.Farbe ?? ConsoleColor.Gray;
                Console.Write($" {rollenIcon}{rollenName,-11}");

                // Rechte-Haken
                foreach (var r in alleRechte)
                {
                    bool hatRecht = rolle != null && r.Getter(rolle.Rechte);
                    Console.ForegroundColor = hatRecht ? ConsoleColor.Green : ConsoleColor.DarkGray;
                    Console.Write(hatRecht ? "   ✅ " : "   ❌ ");
                }

                Console.ResetColor();
                Console.WriteLine();
            }

            Console.WriteLine();

            // Legende
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Legende der Spalten:");
            Console.ResetColor();
            for (int i = 0; i < alleRechte.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                string legende = $"  {TruncateRight(alleRechte[i].Kurzname, 5)} = {alleRechte[i].Anzeigename}";
                Console.Write(legende.PadRight(40));
                if ((i + 1) % 2 == 0) Console.WriteLine();
                Console.ResetColor();
            }
            if (alleRechte.Count % 2 != 0) Console.WriteLine();
        }

        private static void BenutzerRolleAendern()
        {
            Console.WriteLine();
            string nummerEingabe = ConsoleHelper.GetInput("Benutzer-Nummer eingeben");

            if (!int.TryParse(nummerEingabe, out int idx) || idx < 1 || idx > DataManager.Benutzer.Count)
            {
                ConsoleHelper.PrintError("Ungültige Nummer!");
                System.Threading.Thread.Sleep(1000);
                return;
            }

            var benutzer = DataManager.Benutzer[idx - 1];

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  Benutzer: {benutzer.Benutzername}");
            Console.WriteLine($"  Aktuelle Rolle: {benutzer.Berechtigung}");
            Console.ResetColor();
            Console.WriteLine();

            // Verfügbare Rollen anzeigen
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Rollen:");
            Console.ResetColor();

            for (int i = 0; i < _rollen.Count; i++)
            {
                Console.ForegroundColor = _rollen[i].Farbe;
                Console.WriteLine($"  [{i + 1}] {_rollen[i].Icon} {_rollen[i].Name}");
                Console.ResetColor();
            }

            string rolleEingabe = ConsoleHelper.GetInput("\nNeue Rolle wählen (Nummer)");

            if (!int.TryParse(rolleEingabe, out int rolleIdx) || rolleIdx < 1 || rolleIdx > _rollen.Count)
            {
                ConsoleHelper.PrintError("Ungültige Auswahl!");
                System.Threading.Thread.Sleep(1000);
                return;
            }

            var neueRolle = _rollen[rolleIdx - 1];
            string alteRolle = benutzer.Berechtigung.ToString();

            // Berechtigung aktualisieren
            if (Enum.TryParse(neueRolle.Name, out Berechtigungen neuerWert))
            {
                benutzer.Berechtigung = neuerWert;
            }
            else
            {
                // Wenn die Rolle kein Standard-Enum ist, auf Admin/User mappen
                benutzer.Berechtigung = neueRolle.Name.ToLower().Contains("admin")
                    ? Berechtigungen.Admin
                    : Berechtigungen.User;
            }

            // Benutzerdaten speichern
            DataManager.SaveBenutzerToFile();

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"Rolle von '{benutzer.Benutzername}' geändert: {alteRolle} → {neueRolle.Icon} {neueRolle.Name}");
            LogManager.LogDatenGespeichert("Benutzer", $"'{benutzer.Benutzername}' Rolle geändert: {alteRolle} → {neueRolle.Name}");
            System.Threading.Thread.Sleep(1500);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ROLLE LÖSCHEN
        // ─────────────────────────────────────────────────────────────────────

        public static void RolleLoeschen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🗑️  Rolle löschen", ConsoleColor.Red);

            var rolle = WaehlRolle("löschen");
            if (rolle == null) return;

            // Standard-Rollen schützen
            if (rolle.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                rolle.Name.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelper.PrintError($"Die Standard-Rolle '{rolle.Name}' kann nicht gelöscht werden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Prüfen ob Benutzer diese Rolle haben
            int anzahlBenutzer = DataManager.Benutzer.Count(b =>
                b.Berechtigung.ToString().Equals(rolle.Name, StringComparison.OrdinalIgnoreCase));

            if (anzahlBenutzer > 0)
            {
                ConsoleHelper.PrintWarning($"Diese Rolle ist {anzahlBenutzer} Benutzer(n) zugewiesen!");
                ConsoleHelper.PrintInfo("Diese Benutzer werden auf 'User' zurückgesetzt.");
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ⚠️  Rolle '{rolle.Icon} {rolle.Name}' wirklich löschen?");
            Console.ResetColor();

            string bestaetigung = ConsoleHelper.GetInput("Bestätigen mit 'ja'");
            if (!bestaetigung.Equals("ja", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelper.PrintInfo("Löschung abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Benutzer zurücksetzen
            if (anzahlBenutzer > 0)
            {
                foreach (var benutzer in DataManager.Benutzer.Where(b =>
                    b.Berechtigung.ToString().Equals(rolle.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    benutzer.Berechtigung = Berechtigungen.User;
                }
                DataManager.SaveBenutzerToFile();
            }

            _rollen.Remove(rolle);
            SpeichereRollen();

            ConsoleHelper.PrintSuccess($"Rolle '{rolle.Name}' wurde gelöscht!");
            LogManager.LogDatenGespeichert("Rollen", $"Rolle gelöscht: {rolle.Name}");
            ConsoleHelper.PressKeyToContinue();
        }

        // ─────────────────────────────────────────────────────────────────────
        // HILFSMETHODEN
        // ─────────────────────────────────────────────────────────────────────

        private static Rolle WaehlRolle(string aktion)
        {
            if (_rollen.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Rollen vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return null;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Verfügbare Rollen zum {aktion}:");
            Console.ResetColor();

            for (int i = 0; i < _rollen.Count; i++)
            {
                Console.ForegroundColor = _rollen[i].Farbe;
                Console.WriteLine($"  [{i + 1}] {_rollen[i].Icon} {_rollen[i].Name}");
                Console.ResetColor();
            }

            Console.WriteLine("  [0] Abbrechen");
            Console.WriteLine();

            string eingabe = ConsoleHelper.GetInput("Rolle wählen");
            if (eingabe == "0" || string.IsNullOrWhiteSpace(eingabe)) return null;

            if (int.TryParse(eingabe, out int idx) && idx >= 1 && idx <= _rollen.Count)
                return _rollen[idx - 1];

            ConsoleHelper.PrintError("Ungültige Auswahl!");
            System.Threading.Thread.Sleep(800);
            return null;
        }

        /// <summary>
        /// Interaktive Rechte-Konfiguration als Übersichtstabelle.
        /// Alle Rechte auf einen Blick — Nummer eingeben zum Umschalten.
        /// </summary>
        private static RollenRechte KonfigurierereRechte(RollenRechte aktuelleRechte)
        {
            var neueRechte = new RollenRechte
            {
                RollenVerwalten = aktuelleRechte.RollenVerwalten,
                BenutzerVerwalten = aktuelleRechte.BenutzerVerwalten,
                MitarbeiterBearbeiten = aktuelleRechte.MitarbeiterBearbeiten,
                MitarbeiterLoeschen = aktuelleRechte.MitarbeiterLoeschen,
                InventarBearbeiten = aktuelleRechte.InventarBearbeiten,
                InventarLoeschen = aktuelleRechte.InventarLoeschen,
                BestandAendern = aktuelleRechte.BestandAendern,
                DruckVerwalten = aktuelleRechte.DruckVerwalten,
                ImportExport = aktuelleRechte.ImportExport,
                SystemLog = aktuelleRechte.SystemLog,
                Verschluesselung = aktuelleRechte.Verschluesselung,
                KIDashboard = aktuelleRechte.KIDashboard,
                Schnellerfassung = aktuelleRechte.Schnellerfassung
            };

            var definitionen = GetAlleRechteDefinitionen();

            while (true)
            {
                Console.Clear();

                // ── Übersichtstabelle ─────────────────────────────────────────
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ╔══════════════════════════════════════════════════════════╗");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  ║       RECHTE-ÜBERSICHT  —  Nummer zum Umschalten        ║");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ╠══════╦══════════════════════════════════╦═══════════════╣");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("  ║  Nr  ║ Berechtigung                     ║ Status        ║");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ╠══════╬══════════════════════════════════╬═══════════════╣");
                Console.ResetColor();

                for (int i = 0; i < definitionen.Count; i++)
                {
                    var def = definitionen[i];
                    bool an = def.Getter(neueRechte);

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  ║ ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($" {(i + 1),2} ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" ║ ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{def.Anzeigename,-32}");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" ║ ");

                    if (an)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("✅  AN        ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("❌  AUS       ");
                    }

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" ║");
                }

                int aktiv = ZaehleAktiveRechte(neueRechte);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ╠══════╩══════════════════════════════════╩═══════════════╣");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  ║   Aktive Rechte: {aktiv,2} von {definitionen.Count}                              ║");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ╠══════════════════════════════════════════════════════════╣");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ║  [1-13] Recht umschalten                                ║");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  ║  [A] Alle Rechte AN    [K] Alle Rechte AUS              ║");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ║  [S] Speichern & Fertig                                 ║");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ╚══════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  ▶ Auswahl: ");
                Console.ResetColor();

                string eingabe = Console.ReadLine()?.Trim().ToUpper() ?? "";

                if (eingabe == "S")
                {
                    Console.WriteLine();
                    ConsoleHelper.PrintInfo($"Gespeichert: {aktiv} von {definitionen.Count} Rechten aktiv.");
                    Console.WriteLine();
                    break;
                }
                else if (eingabe == "A")
                {
                    foreach (var def in definitionen)
                        def.Setter(neueRechte, true);
                }
                else if (eingabe == "K")
                {
                    foreach (var def in definitionen)
                        def.Setter(neueRechte, false);
                }
                else if (int.TryParse(eingabe, out int nr) && nr >= 1 && nr <= definitionen.Count)
                {
                    var def = definitionen[nr - 1];
                    bool jetzt = def.Getter(neueRechte);
                    def.Setter(neueRechte, !jetzt);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ✗ Ungültige Eingabe!");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(600);
                }
            }

            return neueRechte;
        }

        private static int ZaehleAktiveRechte(RollenRechte rechte)
        {
            return GetAlleRechteDefinitionen().Count(r => r.Getter(rechte));
        }

        private static string TruncateRight(string text, int maxLen)
        {
            if (text.Length <= maxLen) return text;
            return text.Substring(0, maxLen);
        }

        /// <summary>
        /// Zentrale Definition aller Rechte mit Getter/Setter für generische Verarbeitung
        /// </summary>
        private static List<RechtDefinition> GetAlleRechteDefinitionen()
        {
            return new List<RechtDefinition>
            {
                new RechtDefinition("Rollen verwalten",   "Rollen",  r => r.RollenVerwalten,       (r, v) => r.RollenVerwalten = v),
                new RechtDefinition("Benutzer verwalten", "Benutzer",r => r.BenutzerVerwalten,     (r, v) => r.BenutzerVerwalten = v),
                new RechtDefinition("Mitarb. bearbeiten", "MitBea",  r => r.MitarbeiterBearbeiten, (r, v) => r.MitarbeiterBearbeiten = v),
                new RechtDefinition("Mitarb. löschen",    "MitLösch",r => r.MitarbeiterLoeschen,   (r, v) => r.MitarbeiterLoeschen = v),
                new RechtDefinition("Inventar bearbeiten","InvBea",  r => r.InventarBearbeiten,    (r, v) => r.InventarBearbeiten = v),
                new RechtDefinition("Inventar löschen",   "InvLösch",r => r.InventarLoeschen,      (r, v) => r.InventarLoeschen = v),
                new RechtDefinition("Bestand ändern",     "Bestand", r => r.BestandAendern,        (r, v) => r.BestandAendern = v),
                new RechtDefinition("Druck verwalten",    "Druck",   r => r.DruckVerwalten,        (r, v) => r.DruckVerwalten = v),
                new RechtDefinition("Import / Export",    "ImpExp",  r => r.ImportExport,          (r, v) => r.ImportExport = v),
                new RechtDefinition("System-Log",         "Log",     r => r.SystemLog,             (r, v) => r.SystemLog = v),
                new RechtDefinition("Verschlüsselung",    "Krypt",   r => r.Verschluesselung,      (r, v) => r.Verschluesselung = v),
                new RechtDefinition("KI-Dashboard",       "KI",      r => r.KIDashboard,           (r, v) => r.KIDashboard = v),
                new RechtDefinition("Schnellerfassung",   "Schnell", r => r.Schnellerfassung,      (r, v) => r.Schnellerfassung = v),
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // PERSISTENZ
        // ─────────────────────────────────────────────────────────────────────

        private static void SpeichereRollen()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("╔══════════════════════════════════════════════════════════════════╗");
                sb.AppendLine("║                    ROLLEN-DATENBANK                              ║");
                sb.AppendLine("║          🤖 KI-gestützte Inventarverwaltung                     ║");
                sb.AppendLine("╚══════════════════════════════════════════════════════════════════╝");
                sb.AppendLine();
                sb.AppendLine($"# Letzte Änderung: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                sb.AppendLine($"# Anzahl Rollen: {_rollen.Count}");
                sb.AppendLine();
                sb.AppendLine("[DATEN]");
                sb.AppendLine();

                foreach (var rolle in _rollen)
                {
                    sb.AppendLine($"NAME={rolle.Name}");
                    sb.AppendLine($"ICON={rolle.Icon}");
                    sb.AppendLine($"FARBE={(int)rolle.Farbe}");
                    var r = rolle.Rechte;
                    sb.AppendLine($"RECHTE={BoolToInt(r.RollenVerwalten)},{BoolToInt(r.BenutzerVerwalten)},{BoolToInt(r.MitarbeiterBearbeiten)},{BoolToInt(r.MitarbeiterLoeschen)},{BoolToInt(r.InventarBearbeiten)},{BoolToInt(r.InventarLoeschen)},{BoolToInt(r.BestandAendern)},{BoolToInt(r.DruckVerwalten)},{BoolToInt(r.ImportExport)},{BoolToInt(r.SystemLog)},{BoolToInt(r.Verschluesselung)},{BoolToInt(r.KIDashboard)},{BoolToInt(r.Schnellerfassung)}");
                    sb.AppendLine("---");
                }

                File.WriteAllText(RollenDatei, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Rollen konnten nicht gespeichert werden: {ex.Message}");
            }
        }

        private static void LadeRollen()
        {
            try
            {
                if (!File.Exists(RollenDatei)) return;

                string[] zeilen = File.ReadAllLines(RollenDatei, Encoding.UTF8);
                _rollen.Clear();

                string name = null;
                string icon = "🔲";
                ConsoleColor farbe = ConsoleColor.White;
                RollenRechte rechte = null;

                foreach (string zeile in zeilen)
                {
                    if (zeile.StartsWith("NAME="))
                    {
                        name = zeile.Substring(5).Trim();
                        rechte = new RollenRechte();
                    }
                    else if (zeile.StartsWith("ICON="))
                    {
                        icon = zeile.Substring(5).Trim();
                    }
                    else if (zeile.StartsWith("FARBE="))
                    {
                        if (int.TryParse(zeile.Substring(6).Trim(), out int f))
                            farbe = (ConsoleColor)f;
                    }
                    else if (zeile.StartsWith("RECHTE=") && rechte != null)
                    {
                        string[] bits = zeile.Substring(7).Split(',');
                        if (bits.Length >= 13)
                        {
                            rechte.RollenVerwalten = bits[0] == "1";
                            rechte.BenutzerVerwalten = bits[1] == "1";
                            rechte.MitarbeiterBearbeiten = bits[2] == "1";
                            rechte.MitarbeiterLoeschen = bits[3] == "1";
                            rechte.InventarBearbeiten = bits[4] == "1";
                            rechte.InventarLoeschen = bits[5] == "1";
                            rechte.BestandAendern = bits[6] == "1";
                            rechte.DruckVerwalten = bits[7] == "1";
                            rechte.ImportExport = bits[8] == "1";
                            rechte.SystemLog = bits[9] == "1";
                            rechte.Verschluesselung = bits[10] == "1";
                            rechte.KIDashboard = bits[11] == "1";
                            rechte.Schnellerfassung = bits[12] == "1";
                        }
                    }
                    else if (zeile.Trim() == "---" && name != null && rechte != null)
                    {
                        _rollen.Add(new Rolle(name, icon, farbe) { Rechte = rechte });
                        name = null;
                        icon = "🔲";
                        farbe = ConsoleColor.White;
                        rechte = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Rollen konnten nicht geladen werden: {ex.Message}");
                _rollen.Clear();
            }
        }

        private static string BoolToInt(bool b) => b ? "1" : "0";

        // ─────────────────────────────────────────────────────────────────────
        // PUBLIC HELPERS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gibt die Rollen-Rechte für den aktuell angemeldeten Benutzer zurück.
        /// Wird für zukünftige Rechtsprüfungen verwendet.
        /// </summary>
        public static RollenRechte GetRechteVonBenutzer(string benutzername)
        {
            var benutzer = DataManager.Benutzer.FirstOrDefault(b =>
                b.Benutzername.Equals(benutzername, StringComparison.OrdinalIgnoreCase));

            if (benutzer == null) return new RollenRechte();

            var rolle = _rollen.FirstOrDefault(r =>
                r.Name.Equals(benutzer.Berechtigung.ToString(), StringComparison.OrdinalIgnoreCase));

            return rolle?.Rechte ?? new RollenRechte();
        }

        public static List<Rolle> GetAlleRollen() => _rollen;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // INTERNE HILFSKLASSE für Getter/Setter-Abstraktion
    // ─────────────────────────────────────────────────────────────────────────

    internal class RechtDefinition
    {
        public string Anzeigename { get; }
        public string Kurzname { get; }
        public Func<RollenRechte, bool> Getter { get; }
        public Action<RollenRechte, bool> Setter { get; }

        public RechtDefinition(string anzeigename, string kurzname,
            Func<RollenRechte, bool> getter, Action<RollenRechte, bool> setter)
        {
            Anzeigename = anzeigename;
            Kurzname = kurzname;
            Getter = getter;
            Setter = setter;
        }
    }
}