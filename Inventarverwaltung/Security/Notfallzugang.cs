using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.UI;
using Inventarverwaltung.Manager.Data;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Inventarverwaltung.Security
{
    // ══════════════════════════════════════════════════════════════════════════
    //
    //   NOTFALL-ZUGANG
    //   ──────────────
    //   Ermöglicht Admin und Developer den Systemzugang auch wenn der
    //   Sicherheitscheck beim Start fehlschlägt.
    //
    //   WER DARF REIN?
    //   ┌─────────────────────────────────────────────────────────────────┐
    //   │  🛠️  Developer  │ Benutzername: "jah"  │ PW: einmalig einrichten │
    //   │  👑 Admin       │ Jeder Admin-Account  │ PW: das Dev-Notfall-PW  │
    //   └─────────────────────────────────────────────────────────────────┘
    //
    //   ABLAUF BEIM ERSTEN START (EINMALIG):
    //     Program.cs ruft NotfallZugang.SicherstellenDassEingerichtet() auf.
    //     → Notfall-Datei fehlt → Einrichtungsmaske erscheint
    //     → Dev gibt sein Passwort ein + bestätigt
    //     → Passwort wird PBKDF2-gehasht + AES-256-verschlüsselt gespeichert
    //     → Gilt ab sofort auf diesem System für alle zukünftigen Starts
    //
    //   ABLAUF WENN SICHERHEITSCHECK FEHLSCHLÄGT:
    //     ScannerSchutz.TamperAlarm() fragt: "Notfall-Login versuchen?"
    //     → Benutzer drückt J
    //     → NotfallZugang.NotfallLoginAnbieten() zeigt Login-Maske
    //     → Dev oder Admin gibt Benutzername + Passwort ein
    //     → Bei Erfolg: Programm startet normal weiter, Vorfall wird geloggt
    //     → Bei 3 Fehlversuchen: Sperre für 30 Sekunden
    //
    //   SICHERHEIT:
    //     • PBKDF2-SHA256, 150.000 Iterationen, 32-Byte-Zufalls-Salt
    //     • Datei ".notfall": AES-256-verschlüsselt, Hidden + ReadOnly
    //     • Constant-Time-Vergleich gegen Timing-Angriffe
    //     • Alle Versuche werden im verschlüsselten Audit-Log protokolliert
    //
    // ══════════════════════════════════════════════════════════════════════════

    public static class NotfallZugang
    {
        // ── Konfiguration ─────────────────────────────────────────────────────
        // Pfad über SecurePaths — unauffälliger Name im System-Ordner
        private static string NotfallDateiPfad => SecurePaths.NotfallDatei;
        private const string DevBenutzername = "jah";
        private const int MaxFehlversuche = 3;
        private const int SperreSekunden = 30;
        private const int Pbkdf2Iterationen = 150_000;

        // ── Laufzeit-Zustand ──────────────────────────────────────────────────
        private static int _fehlversuche = 0;
        private static DateTime _gesperrtBis = DateTime.MinValue;

        // ══════════════════════════════════════════════════════════════════════
        //  SCHRITT 0 — SICHERSTELLEN DASS EINGERICHTET
        //  Wird in Program.cs VOR dem Sicherheitscheck aufgerufen.
        //  Läuft nur durch wenn die .notfall-Datei noch nicht existiert.
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Stellt sicher dass das Notfall-Passwort eingerichtet ist.
        /// Beim ersten Start auf einem neuen System → Einrichtungsmaske.
        /// Bei jedem weiteren Start → sofortiger Return ohne Ausgabe.
        /// </summary>
        public static void SicherstellenDassEingerichtet()
        {
            if (File.Exists(NotfallDateiPfad))
                return; // Bereits eingerichtet → nichts tun

            // Erster Start: Einrichtung ist Pflicht
            ErsteinrichtungDurchfuehren();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SCHRITT 1 — EINMALIGE ERSTEINRICHTUNG
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Führt die einmalige Einrichtung des Notfall-Passworts durch.
        /// Kann auch manuell aus dem Admin-Menü aufgerufen werden.
        /// </summary>
        public static void ErsteinrichtungDurchfuehren()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║   🔐  NOTFALL-PASSWORT EINRICHTEN  (Einmalig)                    ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║   Dieses Passwort ermöglicht dem Developer (jah) und allen       ║");
            Console.WriteLine("  ║   Administratoren den Systemzugang, auch wenn der                ║");
            Console.WriteLine("  ║   Sicherheitscheck beim Start fehlschlägt.                       ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║   ⚠️  Mindestens 8 Zeichen.                                      ║");
            Console.WriteLine("  ║   ⚠️  Wird verschlüsselt gespeichert – nicht wiederherstellbar.  ║");
            Console.WriteLine("  ║   ⚠️  Gut merken oder sicher aufbewahren!                        ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            string passwort = string.Empty;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  🔑 Notfall-Passwort festlegen: ");
                Console.ResetColor();
                string pw1 = PasswortVerdecktLesen();
                Console.WriteLine();

                if (string.IsNullOrEmpty(pw1) || pw1.Length < 8)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ✗ Passwort muss mindestens 8 Zeichen lang sein!");
                    Console.ResetColor();
                    Console.WriteLine();
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  🔑 Passwort bestätigen:        ");
                Console.ResetColor();
                string pw2 = PasswortVerdecktLesen();
                Console.WriteLine();

                if (pw1 != pw2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ✗ Passwörter stimmen nicht überein! Bitte erneut versuchen.");
                    Console.ResetColor();
                    Console.WriteLine();
                    continue;
                }

                passwort = pw1;
                break;
            }

            // Hash berechnen + verschlüsselt speichern
            byte[] salt = GeneriereSalt();
            byte[] hash = HashPasswort(passwort, salt);
            DateiSpeichern(salt, hash);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Notfall-Passwort wurde erfolgreich eingerichtet!");
            Console.WriteLine($"  ✓ Gilt ab sofort auf diesem System für '{DevBenutzername}' und alle Admins.");
            Console.ResetColor();
            Console.WriteLine();

            AuditSchreiben("EINGERICHTET",
                $"Notfall-Passwort für '{DevBenutzername}' erstmalig eingerichtet.");

            System.Threading.Thread.Sleep(2000);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SCHRITT 2 — NOTFALL-LOGIN ANBIETEN
        //  Wird aus ScannerSchutz.TamperAlarm() aufgerufen.
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Zeigt den Notfall-Login-Dialog.
        /// Gibt TRUE zurück wenn Zugang gewährt wurde → Programm läuft weiter.
        /// Gibt FALSE zurück → Programm wird von TamperAlarm beendet.
        /// </summary>
        public static bool NotfallLoginAnbieten(string fehlerGrund)
        {
            // Falls noch nicht eingerichtet (sollte nicht vorkommen, aber sicher ist sicher)
            if (!File.Exists(NotfallDateiPfad))
                ErsteinrichtungDurchfuehren();

            // DataManager laden falls noch nicht geschehen
            // (Sicherheitscheck läuft vor LoadingScreen)
            if (DataManager.Benutzer.Count == 0)
            {
                try { DataManager.LoadBenutzer(); } catch { }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║   🔐  NOTFALL-ZUGANG                                              ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ║   Grund:  {PadRight(fehlerGrund, 57)} ║");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("  ║   Zugang für:  🛠️  Developer (jah)  |  👑 Administratoren        ║");
            Console.WriteLine("  ║   Passwort:    Das einmalig eingerichtete Notfall-Passwort        ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("  ║   [ENTER ohne Eingabe beim Benutzernamen] = Programm beenden      ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            while (true)
            {
                // Brute-Force-Sperre prüfen
                if (DateTime.Now < _gesperrtBis)
                {
                    int restSek = (int)(_gesperrtBis - DateTime.Now).TotalSeconds + 1;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"\r  ⏳ Gesperrt – noch {restSek,3} Sekunden ...   ");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }

                // Leerzeile nach Sperre-Countdown
                if (_gesperrtBis != DateTime.MinValue)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    _gesperrtBis = DateTime.MinValue;
                    _fehlversuche = 0;
                }

                // ── Eingabe ──────────────────────────────────────────────
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  👤 Benutzername: ");
                Console.ResetColor();
                string benutzer = Console.ReadLine()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(benutzer))
                {
                    AuditSchreiben("ABBRUCH",
                        "Benutzer hat den Notfallzugang abgebrochen.");
                    return false;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  🔑 Passwort:     ");
                Console.ResetColor();
                string passwort = PasswortVerdecktLesen();
                Console.WriteLine();
                Console.WriteLine();

                // ── Prüfung ──────────────────────────────────────────────
                bool zugang = PruefeZugang(benutzer, passwort, out string rolle);

                if (zugang)
                {
                    _fehlversuche = 0;
                    ZugangGewaehren(benutzer, rolle, fehlerGrund);
                    return true;
                }

                // ── Fehlversuch ──────────────────────────────────────────
                _fehlversuche++;
                int verbleibend = MaxFehlversuche - _fehlversuche;

                AuditSchreiben("FEHLVERSUCH",
                    $"Falscher Login für '{benutzer}' " +
                    $"({_fehlversuche}/{MaxFehlversuche})");

                Console.ForegroundColor = ConsoleColor.Red;
                if (verbleibend > 0)
                    Console.WriteLine(
                        $"  ✗ Falscher Benutzername oder Passwort. " +
                        $"Noch {verbleibend} Versuch(e).");
                else
                    Console.WriteLine(
                        "  ✗ Letzter Versuch fehlgeschlagen. Zugang gesperrt.");
                Console.ResetColor();
                Console.WriteLine();

                if (_fehlversuche >= MaxFehlversuche)
                {
                    _gesperrtBis = DateTime.Now.AddSeconds(SperreSekunden);
                    _fehlversuche = 0;

                    AuditSchreiben("GESPERRT",
                        $"Notfallzugang für {SperreSekunden}s gesperrt.");

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(
                        $"  🔒 Zu viele Fehlversuche — " +
                        $"Sperre für {SperreSekunden} Sekunden.");
                    Console.ResetColor();
                    Console.WriteLine();
                }
                else
                {
                    // Kurze Verzögerung gegen schnelles Durchprobieren
                    System.Threading.Thread.Sleep(1500);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PASSWORT ÄNDERN — aus dem Admin/Dev-Menü
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Ändert das Notfall-Passwort. Nur für eingeloggte Admins oder den Dev.
        /// </summary>
        public static void PasswortAendern()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader(
                "🔑 Notfall-Passwort ändern", ConsoleColor.DarkCyan);
            Console.WriteLine();

            // Berechtigung prüfen
            bool istDev = AuthManager.AktuellerBenutzer?.Equals(
                DevBenutzername, StringComparison.OrdinalIgnoreCase) ?? false;

            bool istAdmin = DataManager.Benutzer.Any(b =>
                b.Benutzername.Equals(
                    AuthManager.AktuellerBenutzer,
                    StringComparison.OrdinalIgnoreCase) &&
                b.Berechtigung == Berechtigungen.Admin);

            if (!istDev && !istAdmin)
            {
                ConsoleHelper.PrintError(
                    "Nur der Developer (jah) oder Administratoren " +
                    "dürfen das Notfall-Passwort ändern.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Aktuelles Passwort bestätigen
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  🔑 Aktuelles Notfall-Passwort: ");
            Console.ResetColor();
            string aktuell = PasswortVerdecktLesen();
            Console.WriteLine();

            if (!PruefeDevPasswort(aktuell))
            {
                ConsoleHelper.PrintError(
                    "Aktuelles Passwort falsch. Abgebrochen.");
                AuditSchreiben("PW_AENDERUNG_FEHLGESCHLAGEN",
                    $"Falsches aktuelles PW bei Änderung durch " +
                    $"'{AuthManager.AktuellerBenutzer}'.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Neues Passwort
            string neuesPw = string.Empty;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  🔑 Neues Passwort (min. 8 Zeichen): ");
                Console.ResetColor();
                string pw1 = PasswortVerdecktLesen();
                Console.WriteLine();

                if (string.IsNullOrEmpty(pw1) || pw1.Length < 8)
                {
                    ConsoleHelper.PrintError(
                        "Passwort muss mindestens 8 Zeichen lang sein!");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  🔑 Neues Passwort bestätigen:       ");
                Console.ResetColor();
                string pw2 = PasswortVerdecktLesen();
                Console.WriteLine();

                if (pw1 != pw2)
                {
                    ConsoleHelper.PrintError("Passwörter stimmen nicht überein!");
                    continue;
                }

                neuesPw = pw1;
                break;
            }

            byte[] salt = GeneriereSalt();
            byte[] hash = HashPasswort(neuesPw, salt);
            DateiSpeichern(salt, hash);

            ConsoleHelper.PrintSuccess(
                "✓ Notfall-Passwort wurde erfolgreich geändert.");
            AuditSchreiben("PW_GEAENDERT",
                $"Notfall-Passwort geändert von " +
                $"'{AuthManager.AktuellerBenutzer}'.");

            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRIVATE — ZUGANGS-PRÜFUNG
        // ══════════════════════════════════════════════════════════════════════

        private static bool PruefeZugang(
            string benutzer, string passwort, out string rolle)
        {
            rolle = string.Empty;

            if (string.IsNullOrWhiteSpace(benutzer) ||
                string.IsNullOrEmpty(passwort))
                return false;

            // ── Dev-Zugang ───────────────────────────────────────────────────
            if (benutzer.Equals(DevBenutzername,
                    StringComparison.OrdinalIgnoreCase))
            {
                if (PruefeDevPasswort(passwort))
                {
                    rolle = "DEV";
                    return true;
                }
                return false;
            }

            // ── Admin-Zugang ─────────────────────────────────────────────────
            // Admin-Account muss in Accounts.txt existieren
            var admin = DataManager.Benutzer.FirstOrDefault(b =>
                b.Benutzername.Equals(benutzer,
                    StringComparison.OrdinalIgnoreCase) &&
                b.Berechtigung == Berechtigungen.Admin);

            if (admin == null)
                return false;

            // Admin benutzt das gleiche Notfall-Passwort wie der Dev
            if (PruefeDevPasswort(passwort))
            {
                rolle = "ADMIN";
                return true;
            }

            return false;
        }

        private static bool PruefeDevPasswort(string passwort)
        {
            try
            {
                var (salt, hash) = DateiLesen();
                if (salt == null || hash == null)
                    return false;

                byte[] eingabeHash = HashPasswort(passwort, salt);
                return KryptographischGleich(eingabeHash, hash);
            }
            catch { return false; }
        }

        // ── Constant-Time-Vergleich (verhindert Timing-Angriffe) ─────────────
        private static bool KryptographischGleich(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRIVATE — ZUGANG GEWÄHREN
        // ══════════════════════════════════════════════════════════════════════

        private static void ZugangGewaehren(
            string benutzer, string rolle, string fehlerGrund)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║   ✓  NOTFALL-ZUGANG GEWÄHRT                                      ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.ResetColor();

            string rolleAnzeige = rolle == "DEV"
                ? "🛠️  Developer" : "👑 Administrator";

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(
                $"  ║   Benutzer:  " +
                $"{PadRight(benutzer + "  (" + rolleAnzeige + ")", 53)} ║");
            Console.WriteLine(
                $"  ║   Zeit:      " +
                $"{PadRight(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), 53)} ║");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("  ║   ⚠️  Bitte nach dem Login den Sicherheitscheck beheben:          ║");
            Console.WriteLine("  ║      System  →  Scanner-Guard aktualisieren                      ║");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Benutzer als eingeloggt setzen
           // AuthManager.SetAktuellerBenutzer(benutzer);
            LogManager.SetAktuellerBenutzer(benutzer);

            AuditSchreiben("ZUGANG_GEWAEHRT",
                $"'{benutzer}' ({rolle}) — Grund: {fehlerGrund}");

            System.Threading.Thread.Sleep(2500);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRIVATE — DATEI-OPERATIONEN
        // ══════════════════════════════════════════════════════════════════════

        private static void DateiSpeichern(byte[] salt, byte[] hash)
        {
            try
            {
                string inhalt =
                    $"DEV={DevBenutzername}\n" +
                    $"SALT={Convert.ToBase64String(salt)}\n" +
                    $"HASH={Convert.ToBase64String(hash)}\n" +
                    $"TS={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}";

                byte[] enc = EncryptionManager.EncryptString(inhalt);
                if (enc == null)
                    throw new InvalidOperationException(
                        "AES-Verschlüsselung fehlgeschlagen.");

                // Schreibschutz aufheben falls vorhanden
                if (File.Exists(NotfallDateiPfad))
                    File.SetAttributes(NotfallDateiPfad, FileAttributes.Normal);

                File.WriteAllBytes(NotfallDateiPfad, enc);

                // Hidden + ReadOnly
                File.SetAttributes(NotfallDateiPfad,
                    FileAttributes.Hidden | FileAttributes.ReadOnly);
            }
            catch (Exception ex)
            {
                AuditSchreiben("SPEICHER_FEHLER", ex.Message);
            }
        }

        private static (byte[] Salt, byte[] Hash) DateiLesen()
        {
            try
            {
                // ReadOnly temporär aufheben
                File.SetAttributes(NotfallDateiPfad,
                    File.GetAttributes(NotfallDateiPfad) & ~FileAttributes.ReadOnly);

                byte[] enc = File.ReadAllBytes(NotfallDateiPfad);

                // Wieder schützen
                File.SetAttributes(NotfallDateiPfad,
                    FileAttributes.Hidden | FileAttributes.ReadOnly);

                string inhalt = EncryptionManager.DecryptBytes(enc);
                if (string.IsNullOrWhiteSpace(inhalt))
                    return (null, null);

                byte[] salt = null;
                byte[] hash = null;

                foreach (string zeile in inhalt.Split('\n'))
                {
                    if (zeile.StartsWith("SALT="))
                        salt = Convert.FromBase64String(zeile.Substring(5).Trim());
                    else if (zeile.StartsWith("HASH="))
                        hash = Convert.FromBase64String(zeile.Substring(5).Trim());
                }

                return (salt, hash);
            }
            catch
            {
                return (null, null);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRIVATE — KRYPTOGRAPHIE
        // ══════════════════════════════════════════════════════════════════════

        private static byte[] GeneriereSalt()
        {
            byte[] salt = new byte[32];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        private static byte[] HashPasswort(string passwort, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                passwort, salt, Pbkdf2Iterationen, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRIVATE — PASSWORT VERDECKT LESEN
        // ══════════════════════════════════════════════════════════════════════

        private static string PasswortVerdecktLesen()
        {
            var sb = new StringBuilder();

            while (true)
            {
                var taste = Console.ReadKey(intercept: true);

                if (taste.Key == ConsoleKey.Enter)
                    break;

                if (taste.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    continue;
                }

                if (taste.Key == ConsoleKey.Escape)
                    break;

                if (taste.KeyChar >= 32)
                {
                    sb.Append(taste.KeyChar);
                    Console.Write("*");
                }
            }

            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRIVATE — AUDIT-LOG
        // ══════════════════════════════════════════════════════════════════════

        private static void AuditSchreiben(string typ, string nachricht)
        {
            try
            {
                LogManager.LogFehler(
                    $"NotfallZugang.{typ}",
                    $"[NOTFALL] {nachricht}");
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRIVATE — HILFSMETHODEN
        // ══════════════════════════════════════════════════════════════════════

        private static string PadRight(string text, int len)
        {
            if (text == null) return new string(' ', len);
            if (text.Length >= len) return text.Substring(0, len);
            return text.PadRight(len);
        }
    }
}