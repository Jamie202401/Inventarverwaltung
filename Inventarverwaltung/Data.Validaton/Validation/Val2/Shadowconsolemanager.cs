using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Inventarverwaltung.Data.Validation.Validation.Val2
{
    /// ╔══════════════════════════════════════════════════════════════════════╗
    /// ║              SHADOW CONSOLE MANAGER  ·  v3.0                        ║
    /// ║                                                                      ║
    /// ║  Zugang ausschließlich über TOTP-Authenticator App (RFC 6238).      ║
    /// ║  Kompatibel mit: Google Authenticator, Microsoft Authenticator,     ║
    /// ║                  Authy, und jeder anderen TOTP-App.                 ║
    /// ║                                                                      ║
    /// ║  ERSTEINRICHTUNG (einmalig):                                         ║
    /// ║    1. Programm starten                                               ║
    /// ║    2. Im Hauptmenü "SYS" eingeben                                   ║
    /// ║    3. Als Dev-ID "SETUP" eingeben                                    ║
    /// ║    4. Dev-Namen vergeben → Secret Key / QR-URL erscheint            ║
    /// ║    5. Secret in Authenticator-App eintragen                         ║
    /// ║    6. Testcode eingeben → fertig                                     ║
    /// ║                                                                      ║
    /// ║  NORMALER ZUGANG (nach Einrichtung):                                 ║
    /// ║    1. Im Hauptmenü "SYS" eingeben                                   ║
    /// ║    2. Dev-ID eingeben (unsichtbar)                                   ║
    /// ║    3. 6-stelligen Code aus Authenticator-App eingeben               ║
    /// ╚══════════════════════════════════════════════════════════════════════╝
    public static class ShadowConsoleManager
    {
        // ══════════════════════════════════════════════════════════════════
        // KONFIGURATION
        // ══════════════════════════════════════════════════════════════════

        private const string TriggerWort = "SYS";
        private const int MaxFehlversuche = 3;
        private const int TotpPeriode = 30;   // Sekunden (TOTP-Standard)
        private const int TotpToleranz = 1;    // ±1 Fenster (Uhr-Abweichung)
        private const string AppName = "Inventarverwaltung";
        private const string SetupTrigger = "SETUP";

        private static readonly string ShadowLogPfad = ".sys_trace.enc";
        private static readonly string DevCfgPfad = ".dev_cfg.enc";

        // ══════════════════════════════════════════════════════════════════
        // STATUS
        // ══════════════════════════════════════════════════════════════════

        private static bool _laeuft = false;
        private static readonly object _lock = new object();
        private static DateTime _startzeit = DateTime.Now;
        private static int _gesamtAktionen = 0;
        private static int _fehlversuche = 0;
        private static bool _gesperrt = false;
        private static string _aktuellerDevUser = null;
        private static readonly List<string> _liveLog = new List<string>();
        private static Dictionary<string, string> _devAccounts =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // ══════════════════════════════════════════════════════════════════
        // START
        // ══════════════════════════════════════════════════════════════════

        public static void Starten()
        {
            if (_laeuft) return;
            _laeuft = true;
            _startzeit = DateTime.Now;
            LadeDevAccounts();

            new Thread(UeberwachungsLoop)
            {
                IsBackground = true,
                Name = "SysMonitor",
                Priority = ThreadPriority.BelowNormal
            }.Start();

            SchreibeShadowLog("SYSTEM",
                $"ShadowConsoleManager v3.0 gestartet | " +
                $"{_devAccounts.Count} Dev-Account(s)");
        }

        private static void UeberwachungsLoop()
        {
            while (_laeuft)
            {
                try
                {
                    Thread.Sleep(30_000);
                    SchreibeShadowLog("HEARTBEAT",
                        $"Aktionen: {_gesamtAktionen} | Fehler: {_fehlversuche} | " +
                        $"Uptime: {(DateTime.Now - _startzeit).TotalMinutes:F1} min | " +
                        $"Sperre: {_gesperrt}");
                }
                catch { }
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // TRACKING
        // ══════════════════════════════════════════════════════════════════

        public static void TrackAktion(string typ, string details)
        {
            lock (_lock)
            {
                _gesamtAktionen++;
                string e = $"[{DateTime.Now:HH:mm:ss}] {typ}: {details}";
                _liveLog.Add(e);
                if (_liveLog.Count > 500) _liveLog.RemoveAt(0);
            }
        }

        public static void TrackFehlversuch(string kontext)
        {
            lock (_lock)
            {
                _fehlversuche++;
                TrackAktion("FEHLVERSUCH", kontext);
                SchreibeShadowLog("WARNUNG",
                    $"Fehlversuch #{_fehlversuche}: {kontext}");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // TRIGGER (AppRouter ruft dies bei jeder Hauptmenü-Eingabe auf)
        // ══════════════════════════════════════════════════════════════════

        public static bool VerarbeiteEingabe(string eingabe)
        {
            if (!eingabe.Equals(TriggerWort, StringComparison.OrdinalIgnoreCase)) return false;

            if (_gesperrt)
            {
                SchreibeShadowLog("VERWEIGERT", "Trigger — System gesperrt");
                return false;
            }

            StarteAuthentifizierung();
            return true;
        }

        // ══════════════════════════════════════════════════════════════════
        // AUTHENTIFIZIERUNG
        // ══════════════════════════════════════════════════════════════════

        private static void StarteAuthentifizierung()
        {
            Thread.Sleep(new Random().Next(150, 500));
            Console.Clear();

            // ── Schritt 1: Dev-ID ──────────────────────────────────────
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("\n  ID: ");
            Console.ForegroundColor = ConsoleColor.Green;
            string devId = (Console.ReadLine() ?? "").Trim();
            Console.ResetColor();
            Console.Clear();

            // Setup-Modus?
            if (devId.Equals(SetupTrigger, StringComparison.OrdinalIgnoreCase))
            {
                StarteEinrichtung();
                return;
            }

            // Dev-Account suchen (case-insensitive + Trim auf beiden Seiten)
            string secret = null;
            foreach (var kv in _devAccounts)
            {
                if (kv.Key.Trim().Equals(devId, StringComparison.OrdinalIgnoreCase))
                {
                    secret = kv.Value.Trim();
                    break;
                }
            }

            if (secret == null)
            {
                // Kein Account gefunden → Fehlschlag
                AuthFehlschlag($"Unbekannte Dev-ID '{devId}'");
                return;
            }

            // ── Schritt 2: TOTP-Code ───────────────────────────────────
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("\n  Authenticator-Code: ");
            Console.ForegroundColor = ConsoleColor.Green;
            string code = new string((Console.ReadLine() ?? "").Where(char.IsDigit).ToArray());
            Console.ResetColor();
            Console.Clear();

            if (string.IsNullOrWhiteSpace(code))
            {
                AuthFehlschlag("Kein Code eingegeben");
                return;
            }

            if (!ValidiereTOTP(secret, code))
            {
                AuthFehlschlag($"Falscher TOTP-Code für '{devId}'");
                return;
            }

            // ── Erfolg ─────────────────────────────────────────────────
            _aktuellerDevUser = devId;
            _fehlversuche = 0;
            SchreibeShadowLog("ZUGANG-OK",
                $"Dev '{devId}' via TOTP | App-User: {AuthManager.AktuellerBenutzer ?? "N/A"}");
            OeffneShadowFenster();
        }

        private static void AuthFehlschlag(string grund)
        {
            _fehlversuche++;
            SchreibeShadowLog("ZUGANG-FEHLER",
                $"Fehlversuch #{_fehlversuche}: {grund}");

            if (_fehlversuche >= MaxFehlversuche)
            {
                _gesperrt = true;
                SchreibeShadowLog("SPERRE",
                    $"Automatisch gesperrt nach {MaxFehlversuche} Fehlversuchen");
            }

            // Täuschungsreaktion — immer gleich, gibt nichts preis
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n  ...");
            Thread.Sleep(2500);
            Console.ResetColor();
            Console.Clear();
        }

        // ══════════════════════════════════════════════════════════════════
        // ERSTEINRICHTUNG — TOTP SECRET GENERIEREN & ANZEIGEN
        // ══════════════════════════════════════════════════════════════════

        private static void StarteEinrichtung()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine();
            Console.WriteLine("  ████████████████████████████████████████████████████████████████████");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  █         S H A D O W   C O N S O L E  ·  EINRICHTUNG             █");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("  ████████████████████████████████████████████████████████████████████");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Dev-Name (z.B. 'dev1'): ");
            Console.ResetColor();
            string devName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(devName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ✗ Abgebrochen.");
                Console.ResetColor();
                Thread.Sleep(1200);
                return;
            }

            // TOTP-Secret generieren (20 Bytes = 160 Bit, RFC 4226 Standard)
            string secret = GeneriereBase32Secret();
            _devAccounts[devName] = secret;
            SpeichereDevAccounts();
            SchreibeShadowLog("SETUP", $"Neuer Dev-Account: '{devName}'");

            // otpauth:// URI (Standard für alle TOTP-Apps)
            string uri = $"otpauth://totp/{Uri.EscapeDataString(AppName)}:{Uri.EscapeDataString(devName)}" +
                         $"?secret={secret}&issuer={Uri.EscapeDataString(AppName)}" +
                         $"&algorithm=SHA1&digits=6&period=30";

            string qrUrl = "https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=" +
                           Uri.EscapeDataString(uri);

            // Anzeige
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine();
            Console.WriteLine("  ████████████████████████████████████████████████████████████████████");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  █               AUTHENTICATOR APP EINRICHTEN                       █");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("  ████████████████████████████████████████████████████████████████████");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌──────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │                                                                  │");
            Console.WriteLine("  │  OPTION A — Manuell in App eintragen:                           │");
            Console.WriteLine("  │                                                                  │");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  │  Secret Key:   {FormatierSecret(secret),-50}│");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  │  Konto:        {(AppName + " (" + devName + ")"),-50}│");
            Console.WriteLine("  │  Typ:          Time-based (TOTP)                                │");
            Console.WriteLine("  │  Algorithmus:  SHA1   Stellen: 6   Zeitraum: 30 Sek            │");
            Console.WriteLine("  │                                                                  │");
            Console.WriteLine("  │  OPTION B — QR-Code (URL in Browser öffnen):                   │");
            Console.WriteLine("  │                                                                  │");
            Console.ResetColor();

            // QR-URL aufgeteilt auf mehrere Zeilen
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < qrUrl.Length; i += 64)
            {
                string teil = qrUrl.Substring(i, Math.Min(64, qrUrl.Length - i));
                Console.WriteLine($"  │  {teil,-66}│");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  │                                                                  │");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  │  ⚠ Secret NIEMALS teilen! Screenshot danach löschen!           │");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  │                                                                  │");
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Dev-Account '{devName}' gespeichert.");
            Console.ResetColor();
            Console.WriteLine();

            // Testcode zur Bestätigung
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Trage den Secret jetzt in deine Authenticator-App ein");
            Console.WriteLine("  und gib den ersten Code ein zur Bestätigung:");
            Console.ResetColor();
            Console.Write("\n  Code (6 Stellen): ");
            string testCode = new string(
                (Console.ReadLine() ?? "").Where(char.IsDigit).ToArray());

            if (!string.IsNullOrWhiteSpace(testCode))
            {
                if (ValidiereTOTP(secret, testCode))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n  ✓ Einrichtung erfolgreich! Du kannst dich jetzt einloggen.");
                    SchreibeShadowLog("SETUP-OK", $"TOTP-Test bestanden für '{devName}'");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n  ✗ Code falsch — Secret bitte nochmal in App prüfen.");
                    SchreibeShadowLog("SETUP-FEHLER", $"TOTP-Test fehlgeschlagen für '{devName}'");
                }
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [ENTER] Schließen");
            Console.ResetColor();
            Console.ReadLine();
        }

        // ══════════════════════════════════════════════════════════════════
        // TOTP — RFC 6238 / RFC 4226  (keine externe Bibliothek nötig)
        // ══════════════════════════════════════════════════════════════════

        private static bool ValidiereTOTP(string base32Secret, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 6) return false;
            if (!int.TryParse(code, out int codeInt)) return false;

            byte[] secretBytes = Base32Decode(base32Secret);
            long zeitCounter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpPeriode;

            for (int delta = -TotpToleranz; delta <= TotpToleranz; delta++)
                if (BerechneTOTP(secretBytes, zeitCounter + delta) == codeInt)
                    return true;

            return false;
        }

        private static int BerechneTOTP(byte[] secret, long counter)
        {
            // Counter → Big-Endian 8 Bytes
            byte[] msg = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(msg);

            // HMAC-SHA1
            using var hmac = new HMACSHA1(secret);
            byte[] h = hmac.ComputeHash(msg);

            // Dynamic Truncation
            int offset = h[h.Length - 1] & 0x0F;
            int binary = ((h[offset] & 0x7F) << 24)
                       | ((h[offset + 1] & 0xFF) << 16)
                       | ((h[offset + 2] & 0xFF) << 8)
                       | (h[offset + 3] & 0xFF);

            return binary % 1_000_000; // 6 Stellen
        }

        // ══════════════════════════════════════════════════════════════════
        // BASE32  (RFC 4648 — Standard-Encoding für TOTP-Secrets)
        // ══════════════════════════════════════════════════════════════════

        private const string B32 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        private static string GeneriereBase32Secret()
        {
            byte[] raw = new byte[20]; // 160 Bit
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(raw);
            return Base32Encode(raw);
        }

        private static string Base32Encode(byte[] data)
        {
            var sb = new StringBuilder();
            int buf = 0, bits = 0;
            foreach (byte b in data)
            {
                buf = (buf << 8) | b;
                bits += 8;
                while (bits >= 5) { bits -= 5; sb.Append(B32[(buf >> bits) & 0x1F]); }
            }
            if (bits > 0) sb.Append(B32[(buf << (5 - bits)) & 0x1F]);
            return sb.ToString();
        }

        private static byte[] Base32Decode(string s)
        {
            s = s.TrimEnd('=').ToUpperInvariant();
            var result = new List<byte>();
            int buf = 0, bits = 0;
            foreach (char c in s)
            {
                int v = B32.IndexOf(c);
                if (v < 0) continue;
                buf = (buf << 5) | v;
                bits += 5;
                if (bits >= 8) { bits -= 8; result.Add((byte)((buf >> bits) & 0xFF)); }
            }
            return result.ToArray();
        }

        private static string FormatierSecret(string s)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (i > 0 && i % 4 == 0) sb.Append(' ');
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════════════
        // DEV-ACCOUNTS SPEICHERN / LADEN (verschlüsselt in .dev_cfg.enc)
        // ══════════════════════════════════════════════════════════════════

        private static void SpeichereDevAccounts()
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var kv in _devAccounts)
                    sb.AppendLine($"{kv.Key}|{kv.Value}");

                File.WriteAllBytes(DevCfgPfad,
                    VerschluesseleBytes(Encoding.UTF8.GetBytes(sb.ToString())));

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    && File.Exists(DevCfgPfad))
                    File.SetAttributes(DevCfgPfad,
                        File.GetAttributes(DevCfgPfad) | FileAttributes.Hidden);
            }
            catch (Exception ex)
            {
                SchreibeShadowLog("FEHLER", $"Dev-Config nicht gespeichert: {ex.Message}");
            }
        }

        private static void LadeDevAccounts()
        {
            try
            {
                if (!File.Exists(DevCfgPfad)) return;
                string txt = Encoding.UTF8.GetString(
                    EntschlusseleBytes(File.ReadAllBytes(DevCfgPfad)));

                foreach (string z in txt.Split('\n'))
                {
                    int p = z.IndexOf('|');
                    if (p <= 0) continue;
                    _devAccounts[z.Substring(0, p).Trim()] = z.Substring(p + 1).Trim();
                }
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════
        // SHADOW-FENSTER ÖFFNEN
        // ══════════════════════════════════════════════════════════════════

        private static void OeffneShadowFenster()
        {
            try
            {
                string tmp = Path.Combine(Path.GetTempPath(), $".{Guid.NewGuid():N}.tmp");
                File.WriteAllBytes(tmp,
                    VerschluesseleBytes(Encoding.UTF8.GetBytes(ErstelleSnapshot())));

                string devEnc = Convert.ToBase64String(
                    VerschluesseleBytes(
                        Encoding.UTF8.GetBytes(_aktuellerDevUser ?? "dev")));

                string exe = Process.GetCurrentProcess().MainModule?.FileName
                             ?? "Inventarverwaltung.exe";

                var psi = new ProcessStartInfo { UseShellExecute = true };

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    psi.FileName = "cmd.exe";
                    psi.Arguments = $"/K title System Monitor&color 0A" +
                                    $"&\"{exe}\" --shadow \"{tmp}\" \"{devEnc}\"";
                }
                else
                {
                    psi.FileName = "bash";
                    psi.Arguments = $"-c \"{exe} --shadow '{tmp}' '{devEnc}'\"";
                }

                Process.Start(psi);
                SchreibeShadowLog("FENSTER",
                    $"Shadow-Fenster geöffnet — Dev '{_aktuellerDevUser}'");
            }
            catch (Exception ex)
            {
                SchreibeShadowLog("FEHLER", $"Fenster-Fehler: {ex.Message}");
                ZeigeShadowInterface(ErstelleSnapshot(), _aktuellerDevUser ?? "dev");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // SHADOW-FENSTER ENTRY POINT (wird von Program.cs aufgerufen)
        // ══════════════════════════════════════════════════════════════════

        public static bool IstShadowArgument(string[] args)
            => args?.Length >= 1 && args[0] == "--shadow";

        /// <summary>
        /// Parameterlose Überladung — wird vom AppRouter aufgerufen wenn
        /// "sys" im Hauptmenü eingegeben wird (direkter Aufruf im laufenden Prozess).
        /// </summary>
        public static void StarteAlsShadowFenster()
        {
            if (_gesperrt)
            {
                SchreibeShadowLog("VERWEIGERT", "StarteAlsShadowFenster() — System gesperrt");
                return;
            }
            StarteAuthentifizierung();
        }

        public static void StarteAlsShadowFenster(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "System Monitor";
            LadeDevAccounts();

            string snapshot = null, devName = "dev";

            if (args.Length >= 2 && File.Exists(args[1]))
            {
                try
                {
                    snapshot = Encoding.UTF8.GetString(
                        EntschlusseleBytes(File.ReadAllBytes(args[1])));
                    File.Delete(args[1]);
                }
                catch { snapshot = "[nicht verfügbar]"; }
            }

            if (args.Length >= 3)
            {
                try { devName = Encoding.UTF8.GetString(EntschlusseleBytes(Convert.FromBase64String(args[2]))); }
                catch { devName = "dev"; }
            }

            ZeigeShadowInterface(snapshot, devName);
        }

        // ══════════════════════════════════════════════════════════════════
        // SHADOW INTERFACE
        // ══════════════════════════════════════════════════════════════════

        private static void ZeigeShadowInterface(string snapshot, string devName)
        {
            while (true)
            {
                Console.Clear();
                Header(devName);
                Menu();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"\n  [{devName}@shadow]> ");
                Console.ResetColor();
                string cmd = Console.ReadLine()?.Trim().ToUpper() ?? "";

                switch (cmd)
                {
                    case "1": LiveLog(); break;
                    case "2": SystemStatus(devName); break;
                    case "3": ShadowLogAnzeigen(); break;
                    case "4": Snapshot(snapshot); break;
                    case "5": DevAccountsVerwalten(devName); break;
                    case "6": LogLeeren(devName); break;
                    case "7": SperreVerwalten(); break;
                    case "8": Notfall(devName); break;
                    case "0":
                    case "Q":
                    case "EXIT":
                        SchreibeShadowLog("SESSION", $"Beendet — '{devName}'");
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("\n  Session beendet.");
                        Console.ResetColor();
                        Thread.Sleep(700);
                        return;
                    default:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("  Unbekannt.");
                        Console.ResetColor();
                        Thread.Sleep(500);
                        break;
                }
            }
        }

        private static void Header(string devName)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine();
            Console.WriteLine("  ████████████████████████████████████████████████████████████████████");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  █    S H A D O W   C O N S O L E   M A N A G E R   v3.0          █");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("  █             ── DEV ACCESS · TOTP PROTECTED ──                   █");
            Console.WriteLine("  ████████████████████████████████████████████████████████████████████");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Dev: {devName}  │  App-User: {AuthManager.AktuellerBenutzer ?? "N/A"}  │  " +
                              $"{DateTime.Now:HH:mm:ss}  │  Uptime: {(DateTime.Now - _startzeit).TotalMinutes:F0} min  │  " +
                              $"Aktionen: {_gesamtAktionen}" + (_gesperrt ? "  │  ⚠ GESPERRT" : ""));
            Console.ResetColor();
        }

        private static void Menu()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("  ┌──────────────────────────────────────────────────────────────────┐");
            MP("1", "📡", "Live-Aktivitäten (RAM)", ConsoleColor.Green);
            MP("2", "💻", "System-Status", ConsoleColor.Cyan);
            MP("3", "📋", "Shadow-Log anzeigen", ConsoleColor.Yellow);
            MP("4", "📸", "Programm-Snapshot", ConsoleColor.White);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("  │  ──────────────────────────────────────────────────────────────  │");
            Console.ResetColor();
            MP("5", "🔑", "Dev-Accounts verwalten (TOTP)", ConsoleColor.DarkCyan);
            MP("6", "🗑 ", "Shadow-Log leeren", ConsoleColor.DarkYellow);
            MP("7", "🔓", "Sperre verwalten", ConsoleColor.DarkYellow);
            MP("8", "🔒", "NOTFALL-SPERRUNG", ConsoleColor.Red);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("  │  ──────────────────────────────────────────────────────────────  │");
            Console.ResetColor();
            MP("0", "🚪", "Session beenden", ConsoleColor.DarkGray);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static void MP(string t, string ic, string tx, ConsoleColor f)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("[");
            Console.ForegroundColor = f; Console.Write(t);
            Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("] ");
            Console.ForegroundColor = f;
            string s = $"{ic}  {tx}";
            Console.Write(s);
            int p = 58 - s.Length; if (p > 0) Console.Write(new string(' ', p));
            Console.ForegroundColor = ConsoleColor.DarkGreen; Console.WriteLine("│");
            Console.ResetColor();
        }

        // ── Menü-Aktionen ──────────────────────────────────────────────

        private static void LiveLog()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n  ╔══ LIVE-AKTIVITÄTEN (RAM) ══════════════════════════════════════╗");
            Console.ResetColor();
            List<string> k; lock (_lock) { k = new List<string>(_liveLog); }
            if (k.Count == 0) { Grau("  │  Noch keine Aktivitäten."); }
            else
            {
                foreach (string e in Enumerable.Reverse(k).Take(60))
                {
                    Console.ForegroundColor = e.Contains("FEHLER") || e.Contains("FEHLVERSUCH")
                        ? ConsoleColor.Red : e.Contains("WARNUNG") ? ConsoleColor.Yellow : ConsoleColor.Green;
                    Console.WriteLine($"  │  {e}"); Console.ResetColor();
                }
                Grau($"\n  │  {k.Count} / 500 im RAM");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor(); WarteTaste();
        }

        private static void SystemStatus(string devName)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  ╔══ SYSTEM-STATUS ═══════════════════════════════════════════════╗");
            Console.ResetColor();
            var p = Process.GetCurrentProcess();
            SZ("Dev-Session", devName);
            SZ("App-Benutzer", AuthManager.AktuellerBenutzer ?? "N/A");
            SZ("Prozess-ID", p.Id.ToString());
            SZ("Gestartet", _startzeit.ToString("dd.MM.yyyy HH:mm:ss"));
            SZ("Uptime", $"{(DateTime.Now - _startzeit).TotalMinutes:F1} min");
            SZ("RAM", $"{p.WorkingSet64 / 1024 / 1024} MB");
            SZ("Threads", p.Threads.Count.ToString());
            SZ("Aktionen", _gesamtAktionen.ToString());
            SZ("Auth-Fehlversuche", _fehlversuche.ToString(),
               _fehlversuche >= MaxFehlversuche ? ConsoleColor.Red : ConsoleColor.Green);
            SZ("Gesperrt", _gesperrt ? "⚠ JA" : "✓ Nein",
               _gesperrt ? ConsoleColor.Red : ConsoleColor.Green);
            SZ("Dev-Accounts", _devAccounts.Count.ToString());
            SZ("Artikel", DataManager.Inventar?.Count.ToString() ?? "N/A");
            SZ("Mitarbeiter", DataManager.Mitarbeiter?.Count.ToString() ?? "N/A");
            SZ("Benutzer", DataManager.Benutzer?.Count.ToString() ?? "N/A");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor(); WarteTaste();
        }

        private static void SZ(string l, string w, ConsoleColor f = ConsoleColor.White)
        {
            Console.ForegroundColor = ConsoleColor.Cyan; Console.Write($"  │  {l,-28}");
            Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write("→  ");
            Console.ForegroundColor = f; Console.WriteLine(w); Console.ResetColor();
        }

        private static void ShadowLogAnzeigen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  ╔══ SHADOW-LOG (AES-256) ═════════════════════════════════════════╗");
            Console.ResetColor();
            if (!File.Exists(ShadowLogPfad)) { Grau("  │  Kein Log vorhanden."); }
            else
            {
                try
                {
                    string txt = Encoding.UTF8.GetString(EntschlusseleBytes(File.ReadAllBytes(ShadowLogPfad)));
                    string[] zl = txt.Split('\n');
                    Grau($"  │  {new FileInfo(ShadowLogPfad).Length:N0} Bytes  │  {zl.Length} Zeilen");
                    Grau("  │  ──────────────────────────────────────────────────────────────");
                    foreach (string z in zl.Reverse().Take(80).Reverse())
                    {
                        if (string.IsNullOrWhiteSpace(z)) continue;
                        Console.ForegroundColor =
                            z.Contains("FEHLER") || z.Contains("SPERRE") ? ConsoleColor.Red :
                            z.Contains("WARNUNG") ? ConsoleColor.Yellow :
                            z.Contains("ZUGANG") ? ConsoleColor.Cyan :
                            z.Contains("HEARTBEAT") ? ConsoleColor.DarkGray : ConsoleColor.Gray;
                        Console.WriteLine($"  │  {z}"); Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  │  Fehler: {ex.Message}"); Console.ResetColor();
                }
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor(); WarteTaste();
        }

        private static void Snapshot(string snap)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n  ╔══ PROGRAMM-SNAPSHOT ═══════════════════════════════════════════╗");
            Console.ResetColor();
            foreach (string z in (snap ?? ErstelleSnapshot()).Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(z)) Grau($"  │  {z}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor(); WarteTaste();
        }

        private static void DevAccountsVerwalten(string devName)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\n  ╔══ DEV-ACCOUNTS (TOTP) ═════════════════════════════════════════╗");
            Console.ResetColor();
            Console.WriteLine($"\n  Vorhandene Accounts ({_devAccounts.Count}):");
            foreach (var kv in _devAccounts)
            {
                string mark = kv.Key.Equals(devName, StringComparison.OrdinalIgnoreCase) ? " ← aktiv" : "";
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"    • {kv.Key}{mark}");
            }
            Console.ResetColor();
            Console.WriteLine("\n  [1] Neuen Account hinzufügen\n  [2] Account entfernen\n  [0] Zurück");
            Console.Write("\n  Auswahl: ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    StarteEinrichtung();
                    break;
                case "2":
                    Console.Write("  Zu entfernender Dev-Name: ");
                    string rem = Console.ReadLine()?.Trim() ?? "";
                    if (_devAccounts.ContainsKey(rem) &&
                        !rem.Equals(devName, StringComparison.OrdinalIgnoreCase))
                    {
                        _devAccounts.Remove(rem);
                        SpeichereDevAccounts();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  ✓ '{rem}' entfernt.");
                        SchreibeShadowLog("DEV", $"Account '{rem}' entfernt von '{devName}'");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  ✗ Nicht möglich.");
                    }
                    Console.ResetColor();
                    Thread.Sleep(1200);
                    break;
            }
        }

        private static void LogLeeren(string devName)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("\n  Shadow-Log löschen? [J/N]: ");
            Console.ResetColor();
            if (Console.ReadLine()?.Trim().ToUpper() == "J")
            {
                try
                {
                    if (File.Exists(ShadowLogPfad))
                    {
                        File.WriteAllBytes(ShadowLogPfad, new byte[new FileInfo(ShadowLogPfad).Length]);
                        File.Delete(ShadowLogPfad);
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ✓ Gelöscht.");
                    SchreibeShadowLog("LOG-GELEERT", $"Von '{devName}'");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ {ex.Message}");
                }
                Console.ResetColor();
            }
            Thread.Sleep(900);
        }

        private static void SperreVerwalten()
        {
            if (_gesperrt)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\n  Sperre aufheben? [J/N]: ");
                Console.ResetColor();
                if (Console.ReadLine()?.Trim().ToUpper() == "J")
                {
                    _gesperrt = false; _fehlversuche = 0;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ✓ Aufgehoben.");
                    SchreibeShadowLog("SPERRE", "Manuell aufgehoben");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\n  System sperren? [J/N]: ");
                Console.ResetColor();
                if (Console.ReadLine()?.Trim().ToUpper() == "J")
                {
                    _gesperrt = true;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  🔒 Gesperrt.");
                    SchreibeShadowLog("SPERRE", "Manuell gesetzt");
                    Console.ResetColor();
                }
            }
            Thread.Sleep(900);
        }

        private static void Notfall(string devName)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║              ⚠  NOTFALL-SPERRUNG  ⚠                             ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n  Bestätigung — 'LOCK' eingeben: ");
            Console.ResetColor();
            if (Console.ReadLine()?.Trim() == "LOCK")
            {
                SchreibeShadowLog("NOTFALL", $"Ausgelöst von '{devName}'");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n  🔒 PROGRAMM WIRD BEENDET");
                Console.ResetColor();
                Thread.Sleep(1500);
                Environment.Exit(99);
            }
            else { Grau("  Abgebrochen."); Thread.Sleep(700); }
        }

        // ══════════════════════════════════════════════════════════════════
        // HILFSMETHODEN
        // ══════════════════════════════════════════════════════════════════

        private static string ErstelleSnapshot()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Snapshot:    {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"App-User:    {AuthManager.AktuellerBenutzer ?? "N/A"}");
            sb.AppendLine($"Artikel:     {DataManager.Inventar?.Count ?? 0}");
            sb.AppendLine($"Mitarbeiter: {DataManager.Mitarbeiter?.Count ?? 0}");
            sb.AppendLine($"Benutzer:    {DataManager.Benutzer?.Count ?? 0}");
            sb.AppendLine($"Aktionen:    {_gesamtAktionen}");
            sb.AppendLine($"Fehler:      {_fehlversuche}");
            sb.AppendLine($"Uptime:      {(DateTime.Now - _startzeit).TotalMinutes:F1} min");
            sb.AppendLine(); sb.AppendLine("── Letzte 20 Aktivitäten ──");
            lock (_lock) { foreach (string a in _liveLog.TakeLast(20)) sb.AppendLine(a); }
            return sb.ToString();
        }

        private static void SchreibeShadowLog(string typ, string msg)
        {
            try
            {
                string neu = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] [{typ,-22}] {msg}\n";
                string alt = "";
                if (File.Exists(ShadowLogPfad))
                    try { alt = Encoding.UTF8.GetString(EntschlusseleBytes(File.ReadAllBytes(ShadowLogPfad))); }
                    catch { alt = ""; }

                File.WriteAllBytes(ShadowLogPfad,
                    VerschluesseleBytes(Encoding.UTF8.GetBytes(alt + neu)));

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(ShadowLogPfad))
                    File.SetAttributes(ShadowLogPfad,
                        File.GetAttributes(ShadowLogPfad) | FileAttributes.Hidden);
            }
            catch { }
        }

        private static string LeseVerdeckt()
        {
            var sb = new StringBuilder();
            ConsoleKeyInfo k;
            do
            {
                k = Console.ReadKey(intercept: true);
                if (k.Key == ConsoleKey.Backspace && sb.Length > 0) sb.Remove(sb.Length - 1, 1);
                else if (k.Key != ConsoleKey.Enter) sb.Append(k.KeyChar);
            } while (k.Key != ConsoleKey.Enter);
            return sb.ToString();
        }

        private static void WarteTaste()
        { Console.ForegroundColor = ConsoleColor.DarkGray; Console.WriteLine("\n  [ENTER] Zurück"); Console.ResetColor(); Console.ReadLine(); }

        private static void Grau(string s)
        { Console.ForegroundColor = ConsoleColor.DarkGray; Console.WriteLine(s); Console.ResetColor(); }

        // AES-256 (eigener Schlüssel, vollständig getrennt vom EncryptionManager)
        private static readonly byte[] _k = AblKey("ShadowDev#2026!TOTP$Inventar_Private");
        private static readonly byte[] _v = { 0xD3, 0xC2, 0xB1, 0xA0, 0x9F, 0x8E, 0x7D, 0x6C, 0x5B, 0x4A, 0x39, 0x28, 0x17, 0x06, 0xF5, 0xE4 };

        private static byte[] AblKey(string pw)
        {
            using var k = new Rfc2898DeriveBytes(pw, Encoding.UTF8.GetBytes("ShadowTOTPSalt26"), 10_000, HashAlgorithmName.SHA256);
            return k.GetBytes(32);
        }

        private static byte[] VerschluesseleBytes(byte[] d)
        { using var a = Aes.Create(); a.Key = _k; a.IV = _v; using var e = a.CreateEncryptor(); return e.TransformFinalBlock(d, 0, d.Length); }

        private static byte[] EntschlusseleBytes(byte[] d)
        { using var a = Aes.Create(); a.Key = _k; a.IV = _v; using var e = a.CreateDecryptor(); return e.TransformFinalBlock(d, 0, d.Length); }
    }
}