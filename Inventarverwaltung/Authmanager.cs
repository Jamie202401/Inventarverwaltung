using System;
using System.Linq;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// Moderner Authentifizierungs-Manager mit erweiterten Sicherheitsfeatures
    /// Verwaltet Benutzeranmeldung, Session-Management und Sicherheitsprüfungen
    /// </summary>
    public static class AuthManager
    {
        #region Properties & Constants

        /// <summary>
        /// Aktuell angemeldeter Benutzer
        /// </summary>
        public static string AktuellerBenutzer { get; private set; }

        /// <summary>
        /// Berechtigung des aktuellen Benutzers
        /// </summary>
        public static Berechtigungen AktuelleBerechtigung { get; private set; }

        /// <summary>
        /// Zeitpunkt der Anmeldung
        /// </summary>
        public static DateTime AnmeldeZeitpunkt { get; private set; }

        /// <summary>
        /// Maximale Anzahl fehlgeschlagener Anmeldeversuche
        /// </summary>
        private const int MAX_FEHLVERSUCHE = 3;

        /// <summary>
        /// Sperrzeit nach zu vielen Fehlversuchen (in Sekunden)
        /// </summary>
        private const int SPERRZEIT_SEKUNDEN = 30;

        /// <summary>
        /// Zähler für fehlgeschlagene Anmeldeversuche
        /// </summary>
        private static int fehlversuche = 0;

        /// <summary>
        /// Zeitpunkt der Sperrung
        /// </summary>
        private static DateTime? sperrzeitEnde = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Führt den vollständigen Anmeldeprozess durch
        /// Inkl. Benutzerinitialisierung, Validierung und Session-Setup
        /// </summary>
        public static void Anmeldung()
        {
            ZeigeAnmeldeBildschirm();
            InitialisiereBenutzerSystem();
            FuehreAnmeldungDurch();
        }

        /// <summary>
        /// Meldet den aktuellen Benutzer ab
        /// </summary>
        public static void Abmeldung()
        {
            if (!string.IsNullOrEmpty(AktuellerBenutzer))
            {
                LogManager.LogAbmeldung(AktuellerBenutzer);
                ConsoleHelper.PrintSuccess($"Benutzer {AktuellerBenutzer} erfolgreich abgemeldet");

                // Session-Daten zurücksetzen
                AktuellerBenutzer = null;
                AktuelleBerechtigung = Berechtigungen.User;
                AnmeldeZeitpunkt = DateTime.MinValue;
            }
        }

        /// <summary>
        /// Prüft ob der aktuelle Benutzer Admin-Rechte hat
        /// </summary>
        public static bool IstAdmin()
        {
            return AktuelleBerechtigung == Berechtigungen.Admin;
        }

        /// <summary>
        /// Prüft ob eine aktive Benutzer-Session existiert
        /// </summary>
        public static bool IstAngemeldet()
        {
            return !string.IsNullOrEmpty(AktuellerBenutzer);
        }

        /// <summary>
        /// Gibt Informationen zur aktuellen Session zurück
        /// </summary>
        public static string GetSessionInfo()
        {
            if (!IstAngemeldet())
                return "Keine aktive Session";

            TimeSpan sessionDauer = DateTime.Now - AnmeldeZeitpunkt;
            string rollenIcon = IstAdmin() ? "👑" : "👤";

            return $"{rollenIcon} {AktuellerBenutzer} ({AktuelleBerechtigung}) • Session: {sessionDauer.Hours:D2}:{sessionDauer.Minutes:D2}:{sessionDauer.Seconds:D2}";
        }

        #endregion

        #region Private Methods - Anmeldung

        /// <summary>
        /// Zeigt den Anmeldebildschirm mit modernem Design
        /// </summary>
        private static void ZeigeAnmeldeBildschirm()
        {
            Console.Clear();

            // Gradient Header
            ConsoleHelper.PrintGradientHeader(
                "INVENTARVERWALTUNG SYSTEM",
                "Authentifizierung erforderlich"
            );

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╭─────────────────────────────────────────────────────────────╮");
            Console.WriteLine("  │                   🔐 BENUTZER-ANMELDUNG                     │");
            Console.WriteLine("  ╰─────────────────────────────────────────────────────────────╯");
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Initialisiert das Benutzersystem und erstellt Admin falls nötig
        /// </summary>
        private static void InitialisiereBenutzerSystem()
        {
            try
            {
                // Lade vorhandene Benutzer
                DataManager.LoadBenutzer();

                // Wenn keine Benutzer existieren, erstelle Standard-Admin
                if (!DataManager.Benutzer.Any())
                {
                    ErstelleStandardAdmin();
                }
                else
                {
                    ConsoleHelper.PrintInfo($"System geladen • {DataManager.Benutzer.Count} Benutzer registriert");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Laden der Benutzerdaten: {ex.Message}");
                ConsoleHelper.PrintWarning("Erstelle neues Benutzersystem...");
                ErstelleStandardAdmin();
            }
        }

        /// <summary>
        /// Erstellt einen Standard-Admin-Account für erste Anmeldung
        /// </summary>
        private static void ErstelleStandardAdmin()
        {
            Console.WriteLine();
            ConsoleHelper.PrintWarning("⚠️  Keine Benutzer-Accounts gefunden!");
            Console.WriteLine();

            ConsoleHelper.AnimatedText("     Initialisiere Standardkonfiguration", 30);
            Thread.Sleep(500);

            try
            {
                // Erstelle Admin-Account
                var adminAccount = new Accounts("admin", Berechtigungen.Admin);
                DataManager.Benutzer.Add(adminAccount);
                DataManager.SaveBenutzerToFile();

                Console.WriteLine();
                ConsoleHelper.PrintSuccess("✓ Standard-Admin erfolgreich erstellt!");
                Console.WriteLine();

                // Zeige Admin-Info in Box
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ╭─────────────────────────────────────────────────────────────╮");
                Console.WriteLine("  │                  STANDARD-ADMIN-ACCOUNT                     │");
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────┤");
                Console.WriteLine("  │  👤 Benutzername: admin                                     │");
                Console.WriteLine("  │  👑 Berechtigung: Administrator                             │");
                Console.WriteLine("  │  💡 Hinweis: Bitte weitere Accounts im System anlegen       │");
                Console.WriteLine("  ╰─────────────────────────────────────────────────────────────╯");
                Console.ResetColor();

                Thread.Sleep(2000);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Erstellen des Admin-Accounts: {ex.Message}");
                ConsoleHelper.PrintWarning("Bitte prüfen Sie die Dateiberechtigungen");
                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Führt die eigentliche Anmeldung mit Validierung durch
        /// </summary>
        private static void FuehreAnmeldungDurch()
        {
            while (true)
            {
                // Prüfe auf aktive Sperrzeit
                if (IstGesperrt())
                {
                    ZeigeSperre();
                    continue;
                }

                // Benutzername eingeben
                string benutzername = FrageNachBenutzername();

                if (string.IsNullOrWhiteSpace(benutzername))
                {
                    ConsoleHelper.PrintError("✗ Benutzername darf nicht leer sein!");
                    Console.WriteLine();
                    continue;
                }

                // Validiere Benutzername
                var benutzer = ValidiereBenutzer(benutzername);

                if (benutzer == null)
                {
                    BehandleFehlgeschlageneAnmeldung(benutzername);
                    continue;
                }

                // Erfolgreiche Anmeldung
                ErfolgreicheAnmeldung(benutzer);
                break;
            }
        }

        /// <summary>
        /// Fragt nach dem Benutzernamen mit verbessertem Input
        /// </summary>
        private static string FrageNachBenutzername()
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("👤");
            Console.ResetColor();
            Console.Write(" Benutzername: ");
            Console.ForegroundColor = ConsoleColor.White;

            string input = Console.ReadLine()?.Trim();

            Console.ResetColor();
            return input;
        }

        /// <summary>
        /// Validiert den eingegebenen Benutzernamen
        /// </summary>
        private static Accounts ValidiereBenutzer(string benutzername)
        {
            return DataManager.Benutzer.FirstOrDefault(b =>
                b.Benutzername.Equals(benutzername, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Behandelt eine fehlgeschlagene Anmeldung
        /// </summary>
        private static void BehandleFehlgeschlageneAnmeldung(string benutzername)
        {
            fehlversuche++;

            Console.WriteLine();
            ConsoleHelper.PrintError($"✗ Benutzer '{benutzername}' nicht gefunden!");

            // Zeige verbleibende Versuche
            int verbleibendeVersuche = MAX_FEHLVERSUCHE - fehlversuche;

            if (verbleibendeVersuche > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠️  Verbleibende Versuche: {verbleibendeVersuche}");
                Console.ResetColor();
            }
            else
            {
                // Sperre aktivieren
                sperrzeitEnde = DateTime.Now.AddSeconds(SPERRZEIT_SEKUNDEN);
                ConsoleHelper.PrintError($"🔒 Zu viele Fehlversuche! Gesperrt für {SPERRZEIT_SEKUNDEN} Sekunden.");
            }

            Console.WriteLine();

            // Hilfreiche Tipps
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  💡 Tipps:");
            Console.WriteLine("     • Lassen Sie einen Admin einen Account für Sie erstellen");
            Console.WriteLine("     • Standardaccount: 'admin' (falls verfügbar)");
            Console.WriteLine("     • Groß-/Kleinschreibung wird nicht beachtet");
            Console.ResetColor();
            Console.WriteLine();

            // Logging
            LogManager.LogFehlgeschlageneAnmeldung(benutzername);
        }

        /// <summary>
        /// Führt die erfolgreiche Anmeldung durch und setzt Session-Daten
        /// </summary>
        private static void ErfolgreicheAnmeldung(Accounts benutzer)
        {
            // Setze Session-Daten
            AktuellerBenutzer = benutzer.Benutzername;
            AktuelleBerechtigung = benutzer.Berechtigung;
            AnmeldeZeitpunkt = DateTime.Now;

            // Fehlversuche zurücksetzen
            fehlversuche = 0;
            sperrzeitEnde = null;

            // Animierter Erfolg
            Console.WriteLine();
            ConsoleHelper.AnimatedText("     Authentifizierung läuft", 50);
            Thread.Sleep(800);

            Console.WriteLine();
            Console.WriteLine();

            // Erfolgsbox
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╭─────────────────────────────────────────────────────────────╮");
            Console.WriteLine("  │               ✓ ANMELDUNG ERFOLGREICH                       │");
            Console.WriteLine("  ╰─────────────────────────────────────────────────────────────╯");
            Console.ResetColor();
            Console.WriteLine();

            // Benutzerdetails
            string rollenIcon = benutzer.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";
            string rollenBeschreibung = benutzer.Berechtigung == Berechtigungen.Admin
                ? "Administrator"
                : "Benutzer";

            ConsoleHelper.PrintInfo($"  → Willkommen, {AktuellerBenutzer}!");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  → Berechtigung: {rollenIcon} {rollenBeschreibung}");
            Console.WriteLine($"  → Anmeldung: {AnmeldeZeitpunkt:dd.MM.yyyy HH:mm:ss}");
            Console.ResetColor();

            // Logging
            LogManager.LogAnmeldungErfolgreich(AktuellerBenutzer);

            Console.WriteLine();
            Thread.Sleep(1500);
        }

        #endregion

        #region Private Methods - Sicherheit

        /// <summary>
        /// Prüft ob aktuell eine Sperrzeit aktiv ist
        /// </summary>
        private static bool IstGesperrt()
        {
            if (sperrzeitEnde.HasValue && DateTime.Now < sperrzeitEnde.Value)
            {
                return true;
            }

            // Sperre abgelaufen, zurücksetzen
            if (sperrzeitEnde.HasValue && DateTime.Now >= sperrzeitEnde.Value)
            {
                sperrzeitEnde = null;
                fehlversuche = 0;
            }

            return false;
        }

        /// <summary>
        /// Zeigt die aktive Sperrzeit an
        /// </summary>
        private static void ZeigeSperre()
        {
            if (!sperrzeitEnde.HasValue)
                return;

            TimeSpan verbleibend = sperrzeitEnde.Value - DateTime.Now;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("  ╭─────────────────────────────────────────────────────────────╮");
            Console.WriteLine("  │                    🔒 ACCOUNT GESPERRT                      │");
            Console.WriteLine("  ╰─────────────────────────────────────────────────────────────╯");
            Console.ResetColor();
            Console.WriteLine();

            ConsoleHelper.PrintError($"  Zu viele fehlgeschlagene Anmeldeversuche!");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⏳ Verbleibende Sperrzeit: {verbleibend.Seconds} Sekunden");
            Console.ResetColor();
            Console.WriteLine();

            Thread.Sleep(1000);
        }

        /// <summary>
        /// Prüft ob ein Benutzer Admin-Rechte für eine Aktion benötigt
        /// </summary>
        public static bool PruefeAdminBerechtigung(string aktion)
        {
            if (IstAdmin())
                return true;

            ConsoleHelper.PrintError($"⛔ Zugriff verweigert!");
            ConsoleHelper.PrintWarning($"Die Aktion '{aktion}' erfordert Administrator-Rechte.");
            Console.WriteLine();

            LogManager.LogZugriffVerweigert(AktuellerBenutzer, aktion);

            return false;
        }

        #endregion

        #region Statistiken

        /// <summary>
        /// Gibt Session-Statistiken zurück
        /// </summary>
        public static void ZeigeSessionStatistik()
        {
            if (!IstAngemeldet())
            {
                ConsoleHelper.PrintWarning("Keine aktive Session");
                return;
            }

            TimeSpan sessionDauer = DateTime.Now - AnmeldeZeitpunkt;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╭─────────────────────────────────────────────────────────────╮");
            Console.WriteLine("  │                    SESSION-INFORMATIONEN                    │");
            Console.WriteLine("  ╰─────────────────────────────────────────────────────────────╯");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine($"  👤 Benutzer: {AktuellerBenutzer}");
            Console.WriteLine($"  👑 Berechtigung: {AktuelleBerechtigung}");
            Console.WriteLine($"  🕐 Anmeldung: {AnmeldeZeitpunkt:dd.MM.yyyy HH:mm:ss}");
            Console.WriteLine($"  ⏱️  Session-Dauer: {sessionDauer.Hours:D2}:{sessionDauer.Minutes:D2}:{sessionDauer.Seconds:D2}");
            Console.WriteLine();
        }

        #endregion
    }
}