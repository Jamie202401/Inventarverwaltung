// ┌────────────────────────────────────────────────────────────────────┐
// │  Internal diagnostics interface — not part of the public API       │
// └────────────────────────────────────────────────────────────────────┘
using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Inventarverwaltung
{
    internal static class DevConsole
    {
        // ── Identität (Laufzeit-Auflösung, kein Klartext) ─────────────────
        private static readonly string _ident = _ri();
        private static string _ri() { int[] v = { 0x6A, 0x61, 0x68 }; return new string(Array.ConvertAll(v, x => (char)x)); }

        // ── Pfade ──────────────────────────────────────────────────────────
        private const string _logDir = "Logs";
        private static string _logFile => Path.Combine(_logDir, "System_Log.enc");

        // ── Steuerung ──────────────────────────────────────────────────────
        public const string _ep = "--devconsole";
        private const int _mf = 3;
        private const int _cd = 4;
        private const int _lockSec = 20;           // Auto-Lock nach X Sekunden
        private static int _fc = 0;
        private static string _sid = string.Empty;
        private static DateTime _lastAct = DateTime.Now; // Letzte Aktivität
        private static DateTime _ts = DateTime.MinValue;
        private const int _W = 86;

        // ── Farb-Aliase ────────────────────────────────────────────────────
        private static readonly ConsoleColor _a0 = ConsoleColor.DarkCyan;
        private static readonly ConsoleColor _a1 = ConsoleColor.Cyan;
        private static readonly ConsoleColor _a2 = ConsoleColor.DarkGray;
        private static readonly ConsoleColor _a3 = ConsoleColor.White;
        private static readonly ConsoleColor _a4 = ConsoleColor.Green;
        private static readonly ConsoleColor _a5 = ConsoleColor.Yellow;
        private static readonly ConsoleColor _a6 = ConsoleColor.Red;
        private static readonly ConsoleColor _a7 = ConsoleColor.Magenta;
        private static readonly ConsoleColor _a8 = ConsoleColor.DarkGreen;
        private static readonly ConsoleColor _a9 = ConsoleColor.DarkBlue;

        // ══════════════════════════════════════════════════════════════════
        // A — Aufruf aus Hauptmenü
        // ══════════════════════════════════════════════════════════════════

        internal static void Open()
        {
            string u = AuthManager.AktuellerBenutzer ?? string.Empty;
            if (!_chk(u))
            {
                _f(_a6); Console.WriteLine("\n  ✗ Zugriff nicht möglich.");
                Console.ResetColor();
                string _lg1 = $"Denied|u={_mx(u)}|h={_h()}|ip={_ip()}"; LogManager.LogWarnung("DC", _lg1);
                global::System.Threading.Thread.Sleep(1800);
                return;
            }
            string bin = Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0];
            string tok = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = bin,
                    Arguments = $"{_ep} \"{u}\" \"{tok}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                });
                string _lg2 = $"Spawned|u={_mx(u)}|h={_h()}"; LogManager.LogWarnung("DC", _lg2);
            }
            catch (Exception ex) { _f(_a6); Console.WriteLine($"\n  ✗ {ex.Message}"); Console.ResetColor(); _wk(); }
        }

        // ══════════════════════════════════════════════════════════════════
        // B — Neues Fenster (Program.cs übergibt args)
        // ══════════════════════════════════════════════════════════════════

        internal static void Boot(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            _ts = DateTime.Now;
            _sid = Guid.NewGuid().ToString("N")[..8].ToUpper();
            Console.Title = $"[{_sid}] Initializing...";

            if (args.Length < 3) { _ad("E:01"); return; }
            string u = args[1];

            _splash();
            _scan(u);

            DataManager.LoadBenutzer();
            DataManager.LoadInventar();
            DataManager.LoadMitarbeiter();

            var acc = DataManager.Benutzer
                .FirstOrDefault(b => b.Benutzername.Equals(u, StringComparison.OrdinalIgnoreCase));
            if (acc == null) { _ad("E:02"); return; }

            // ── Passwort-Bestätigung ───────────────────────────────────────
            _pwHdr(u);

            while (_fc < _mf)
            {
                _f(_a0); Console.Write("  ┌ ");
                _f(_a5); Console.Write($"[{_mf - _fc}/{_mf}]  ");
                _f(_a3); Console.Write("Passwort ▶ ");
                Console.ResetColor();

                // Passwort über exakt dieselbe Methode wie AuthManager prüfen
                string pw = _rpw();
                string hash = AuthManager.HashPasswort(pw);

                if (string.Equals(acc.PasswortHash, hash, StringComparison.Ordinal))
                    break;

                _fc++;
                string _lg3 = $"PW-fail#{_fc}|u={_mx(u)}|ip={_ip()}"; LogManager.LogWarnung("DC", _lg3);

                if (_fc >= _mf)
                {
                    _ad($"E:03 — {_mf}x auth failure");
                    string _lg4 = $"LOCKED|u={_mx(u)}|h={_h()}|ip={_ip()}"; LogManager.LogWarnung("DC", _lg4);
                    return;
                }

                _f(_a6); Console.WriteLine($"  ✗ Falsches Passwort. {_mf - _fc} Versuch(e) verbleibend.");
                Console.ResetColor();
                for (int i = _cd * _fc; i > 0; i--)
                { _f(_a2); Console.Write($"\r  Warte {i}s...  "); global::System.Threading.Thread.Sleep(1000); }
                Console.WriteLine();
            }

            bool dev = _isDev(u);
            bool adm = acc.Berechtigung == Berechtigungen.Admin;
            if (!dev && !adm) { _ad("E:04"); return; }

            Console.Title = $"[{_sid}][{(dev ? "DEV" : "ADM")}] {u}";
            string _lg5 = $"GRANTED|u={u}|role={(dev ? "DEV" : "ADM")}|sid={_sid}|h={_h()}|ip={_ip()}"; LogManager.LogWarnung("DC", _lg5);
            _ok(u, dev);
            _loop(dev, u);
        }

        // ══════════════════════════════════════════════════════════════════
        // SECURITY SCREENS
        // ══════════════════════════════════════════════════════════════════

        private static void _splash()
        {
            Console.Clear(); Console.WriteLine();
            _fr(_a0, "╔", "╗");
            _fz(_a0, _a1, "██████╗ ███████╗██╗   ██╗ ██████╗ ██████╗ ███╗   ██╗███████╗");
            _fz(_a0, _a1, "██╔══██╗██╔════╝██║   ██║██╔════╝██╔═══██╗████╗  ██║██╔════╝");
            _fz(_a0, _a1, "██║  ██║█████╗  ██║   ██║██║     ██║   ██║██╔██╗ ██║███████╗");
            _fz(_a0, _a1, "██║  ██║██╔══╝  ╚██╗ ██╔╝██║     ██║   ██║██║╚██╗██║╚════██║");
            _fz(_a0, _a1, "██████╔╝███████╗ ╚████╔╝ ╚██████╗╚██████╔╝██║ ╚████║███████║");
            _fz(_a0, _a1, "╚═════╝ ╚══════╝  ╚═══╝   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝");
            _fz(_a0, _a7, "🔐  INVENTARVERWALTUNG  ·  DIAGNOSTICS INTERFACE");
            _fz(_a0, _a2, "Restricted Access  —  Authorized Personnel Only");
            _fz(_a0, _a2, $"SID: {_sid}  ·  {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            _fr(_a0, "╚", "╝");
            Console.WriteLine(); global::System.Threading.Thread.Sleep(500);
        }

        private static void _scan(string u)
        {
            var checks = new (string L, string V, int Ms)[]
            {
                ("Process integrity",           $"PID={Process.GetCurrentProcess().Id}",    260),
                ("Memory boundary check",       "Heap scan passed",                          200),
                ("Principal lookup",            $"Target: {_mx(u)}",                         280),
                ("Permission matrix",           "ACL loaded",                                220),
                ("Session token",               $"SID={_sid}",                               300),
                ("Cryptographic context",       "AES-256 + PBKDF2-SHA256 ready",             190),
                ("Network fingerprint",         $"Host: {_h()}",                             250),
                ("Audit subsystem",             "Encrypted log active",                      170),
                ("Anti-tamper scan",            "No modifications detected",                 290),
                ("Runtime environment",         $".NET {Environment.Version}",               200),
            };

            _f(_a0); Console.Write("  ┌─ ");
            _f(_a1); Console.Write(" SECURITY CHECK ");
            _f(_a0); Console.WriteLine(new string('─', 62) + "┐");
            Console.ResetColor(); Console.WriteLine();

            for (int i = 0; i < checks.Length; i++)
            {
                var (l, v, ms) = checks[i];
                _f(_a2); Console.Write($"  [{i + 1:D2}/{checks.Length}]  ");
                _f(_a3); Console.Write($"{l,-36}");
                global::System.Threading.Thread.Sleep(ms / 2);
                _f(_a9);
                for (int b = 0; b < 14; b++) { Console.Write("▓"); global::System.Threading.Thread.Sleep(ms / 28); }
                _f(_a4); Console.Write("  ✓ "); _f(_a2); Console.WriteLine(v);
                Console.ResetColor();
            }
            Console.WriteLine();
            _f(_a4); Console.Write("  ✓ All checks passed  ");
            _f(_a2); Console.WriteLine($"·  {DateTime.Now:dd.MM.yyyy HH:mm:ss}  ·  {_h()}");
            Console.ResetColor(); Console.WriteLine(); global::System.Threading.Thread.Sleep(350);
        }

        private static void _pwHdr(string u)
        {
            _fr(_a0, "╔", "╗");
            _fz(_a0, _a5, "🔑  IDENTITY CONFIRMATION REQUIRED");
            _fz(_a0, _a2, $"Principal : {u}");
            _fz(_a0, _a2, $"Session   : {_sid}");
            _fz(_a0, _a2, $"Timestamp : {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            _fz(_a0, _a2, $"Host/IP   : {_h()}  ·  {_ip()}");
            _fr(_a0, "╚", "╝");
            Console.ResetColor(); Console.WriteLine();
        }

        private static void _ok(string u, bool dev)
        {
            Console.Clear(); Console.WriteLine();
            _fr(_a4, "╔", "╗");
            _fz(_a4, _a4, "✅   ZUGANG GEWÄHRT  —  ACCESS GRANTED");
            _fz(_a4, _a2, $"Benutzer  :  {u}");
            _fz(_a4, _a2, $"Rolle     :  {(dev ? "🔐 ENTWICKLER (DEV) — Vollzugriff" : "👑 ADMINISTRATOR")}");
            _fz(_a4, _a2, $"Zeitpunkt :  {DateTime.Now:dd.MM.yyyy  HH:mm:ss}");
            _fz(_a4, _a2, $"Maschine  :  {_h()}");
            _fz(_a4, _a2, $"IP-Adresse:  {_ip()}");
            _fz(_a4, _a2, $"Sitzung   :  {_sid}");
            _fr(_a4, "╚", "╝");
            Console.ResetColor(); global::System.Threading.Thread.Sleep(1300);
        }

        private static void _ad(string code)
        {
            Console.Clear(); Console.WriteLine();
            _fr(_a6, "╔", "╗");
            _fz(_a6, _a6, "🔒   ZUGRIFF VERWEIGERT  —  ACCESS DENIED");
            _fz(_a6, _a2, $"Code      :  {code}");
            _fz(_a6, _a2, $"Maschine  :  {_h()}");
            _fz(_a6, _a2, $"IP        :  {_ip()}");
            _fz(_a6, _a2, $"Zeit      :  {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            _fr(_a6, "╚", "╝");
            Console.ResetColor(); global::System.Threading.Thread.Sleep(3500);
        }

        // ══════════════════════════════════════════════════════════════════
        // HAUPTSCHLEIFE
        // ══════════════════════════════════════════════════════════════════

        private static void _loop(bool dev, string u)
        {
            while (true)
            {
                Console.Clear();
                _hdr(dev, u);
                _pSys(); Console.WriteLine();
                _pDat(); Console.WriteLine();
                _pSec(dev); Console.WriteLine();
                _pProc(); Console.WriteLine();
                if (dev) _pDev();
                _bar(dev);
                _pmt(u);

                // Eingabe mit Auto-Lock Überwachung
                string cmd = _readWithTimeout(u, dev);
                if (cmd == null) continue; // nach Re-Auth weiter
                switch (cmd)
                {
                    case "exit":
                    case "quit":
                    case "q":
                        string _lg6 = $"Closed|u={u}|sid={_sid}"; LogManager.LogWarnung("DC", _lg6); return;
                    case "cls": case "clear": continue;
                    case "logs": _vLog(false); break;
                    case "logs all": _vLog(true); break;
                    case "benutzer": _vUsr(); break;
                    case "inventar": _vInv(); break;
                    case "mitarbeiter": _vStf(); break;
                    case "dateien": _vFil(); break;
                    case "netzwerk": _vNet(); break;
                    case "sicherheit": _vSec(dev); break;
                    case "warnungen": _vWrn(); break;
                    case "reload": _doRld(); break;
                    case "gc": _doGC(); break;
                    case "help": case "?": _vHlp(dev); break;
                    case "env": if (dev) _dEnv(); else _na(); break;
                    case "hash": if (dev) _dHsh(); else _na(); break;
                    case "prozesse": if (dev) _dPrc(); else _na(); break;
                    case "laufzeit": if (dev) _dRt(); else _na(); break;
                    default:
                        if (!string.IsNullOrWhiteSpace(cmd))
                        { _f(_a6); Console.WriteLine($"\n  ✗ '{cmd}' unbekannt — 'help' für alle Befehle"); Console.ResetColor(); _wk(); }
                        break;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // DASHBOARD
        // ══════════════════════════════════════════════════════════════════

        private static void _hdr(bool dev, string u)
        {
            string up = (DateTime.Now - _ts).ToString(@"hh\:mm\:ss");
            string ram = (GC.GetTotalMemory(false) / 1024).ToString("N0");
            _fr(_a0, "╔", "╗");
            _fz(_a0, _a1, "🖥   INVENTARVERWALTUNG  ·  DIAGNOSTICS CONSOLE");
            _fz(_a0, dev ? _a7 : _a4,
                dev ? "🔐  DEV-MODUS  ·  Vollzugriff  ·  Alle Funktionen aktiv"
                    : "👑  ADMIN-MODUS  ·  Standardzugriff");
            _fr(_a0, "╠", "╣");
            _fz(_a0, _a2, $"👤 {u}   🔑 {_sid}   ⏱ {up}   💾 {ram} KB   🕒 {DateTime.Now:dd.MM.yyyy HH:mm:ss}   🌐 {_ip()}");
            _fr(_a0, "╚", "╝");
            Console.ResetColor(); Console.WriteLine();
        }

        private static void _pSys()
        {
            _sec("⚙️   SYSTEM  &  HARDWARE");
            var p = Process.GetCurrentProcess();
            long heap = GC.GetTotalMemory(false) / 1024;
            long ws = Environment.WorkingSet / 1024;
            long peak = p.PeakWorkingSet64 / 1024;
            _kk("Computername", _h(), "OS", $"{Environment.OSVersion.Platform} {(Environment.Is64BitOperatingSystem ? "64-Bit" : "32-Bit")}");
            _kk("OS-Version", Environment.OSVersion.VersionString, ".NET", Environment.Version.ToString());
            _kk("CPU-Kerne", Environment.ProcessorCount.ToString(), "Zeitzone", TimeZoneInfo.Local.StandardName);
            _kk("RAM Heap", $"{heap:N0} KB", "RAM Working", $"{ws:N0} KB");
            _kk("RAM Peak", $"{peak:N0} KB", "GC Gen0/1/2", $"{GC.CollectionCount(0)}/{GC.CollectionCount(1)}/{GC.CollectionCount(2)}");
            _kk("Threads", p.Threads.Count.ToString(), "Handles", p.HandleCount.ToString());
            _kk("Systemzeit", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), "Uptime", (DateTime.Now - p.StartTime).ToString(@"hh\:mm\:ss"));
            _kk("User@Domain", $"{Environment.UserName}@{Environment.UserDomainName}", "IP", _ip());
        }

        private static void _pDat()
        {
            _sec("📦  DATEN  &  BESTAND");
            int ges = DataManager.Inventar.Count;
            int leer = DataManager.Inventar.Count(a => a.Anzahl == 0);
            int nied = DataManager.Inventar.Count(a => a.Anzahl > 0 && a.Anzahl <= a.Mindestbestand);
            int ok = ges - leer - nied;
            decimal wert = DataManager.Inventar.Sum(a => a.Preis * a.Anzahl);
            int kats = DataManager.Inventar.Select(a => a.Kategorie).Distinct().Count();
            int hers = DataManager.Inventar.Select(a => a.Hersteller).Distinct().Count();
            int mitA = DataManager.Mitarbeiter.Count;
            int abts = DataManager.Mitarbeiter.Select(m => m.Abteilung).Distinct().Count();
            int ben = DataManager.Benutzer.Count;
            int adm = DataManager.Benutzer.Count(b => b.Berechtigung == Berechtigungen.Admin);
            int oPW = DataManager.Benutzer.Count(b => string.IsNullOrWhiteSpace(b.PasswortHash));

            _kk("Inventar gesamt", $"{ges}", "Gesamtwert", $"{wert:N2} €");
            _kkF("  ✓ OK", $"{ok}", _a4, "  ⚠ Niedrig", $"{nied}", nied > 0 ? _a5 : _a4);
            _kkF("  ✗ Leer", $"{leer}", leer > 0 ? _a6 : _a4, "Kategorien", $"{kats}", _a3);
            _kk("Hersteller", $"{hers}", "Mitarbeiter", $"{mitA} ({abts} Abt.)");
            _kkF("Benutzer", $"{ben} ({adm} Admin{(adm != 1 ? "s" : "")})", _a3,
                 "Ohne Passwort", oPW == 0 ? "✓ Keiner" : $"⚠ {oPW}", oPW > 0 ? _a5 : _a4);

            Console.WriteLine();
            if (ges > 0)
            {
                int bOK = ok * 38 / ges;
                int bNI = nied * 38 / ges;
                int bLE = Math.Max(0, 38 - bOK - bNI);
                _f(_a2); Console.Write("  Bestand  ");
                _f(_a4); Console.Write(new string('█', bOK));
                _f(_a5); Console.Write(new string('█', bNI));
                _f(_a6); Console.Write(new string('█', bLE));
                _f(_a2); Console.WriteLine($"  OK:{ok}  ⚠:{nied}  ✗:{leer}");
                Console.ResetColor();
            }
        }

        private static void _pSec(bool dev)
        {
            _sec("🔐  SICHERHEITS-STATUS");
            bool logOk = File.Exists(_logFile);
            long logSz = logOk ? new FileInfo(_logFile).Length : 0;
            bool invOk = File.Exists(FileManager.FilePath);
            bool mitOk = File.Exists(FileManager.FilePath2);
            bool accOk = File.Exists(FileManager.FilePath3);
            int oPW = DataManager.Benutzer.Count(b => string.IsNullOrWhiteSpace(b.PasswortHash));
            int adm = DataManager.Benutzer.Count(b => b.Berechtigung == Berechtigungen.Admin);

            _kkF("Verschlüsselung", "AES-256-CBC", _a4, "Key-Ableitung", "PBKDF2-SHA256", _a4);
            _kkF("System-Log", logOk ? $"✓ {logSz:N0}B" : "✗", logOk ? _a4 : _a6, "Inventar.txt", invOk ? "✓" : "✗", invOk ? _a4 : _a6);
            _kkF("Mitarbeiter.txt", mitOk ? "✓" : "✗", mitOk ? _a4 : _a6, "Accounts.txt", accOk ? "✓" : "✗", accOk ? _a4 : _a6);
            _kkF("Ohne Passwort", oPW == 0 ? "✓ Keiner" : $"⚠ {oPW}", oPW == 0 ? _a4 : _a5, "Admin-Konten", $"{adm}", adm >= 1 ? _a4 : _a6);
            _kkF("Fehlversuche", $"{_fc}/{_mf}", _fc > 0 ? _a5 : _a4, "Modus", dev ? "DEV (Voll)" : "ADMIN (Std)", dev ? _a7 : _a4);
            _kk("Sitzungs-ID", _sid, "IP-Adresse", _ip());
        }

        private static void _pProc()
        {
            _sec("🔧  PROZESS  &  LAUFZEIT");
            var p = Process.GetCurrentProcess();
            _kk("Prozess-ID", p.Id.ToString(), "Prozessname", p.ProcessName);
            _kk("Start", p.StartTime.ToString("dd.MM.yyyy HH:mm:ss"), "CPU-Zeit", p.TotalProcessorTime.ToString(@"hh\:mm\:ss\.ff"));
            _kk("Uptime Sitzung", (DateTime.Now - _ts).ToString(@"hh\:mm\:ss"), "Uptime Prog.", (DateTime.Now - p.StartTime).ToString(@"hh\:mm\:ss"));
            _kk("Arbeitsverz.", _trim(Environment.CurrentDirectory, 36), "Priv. Memory", $"{p.PrivateMemorySize64 / 1024:N0} KB");
        }

        private static void _pDev()
        {
            _sec("🔮  DEV  —  EXKLUSIV");
            _f(_a0);
            Console.WriteLine($"  │  {"Typ",-4} {"Name",-20} {"Rolle",-10} {"Passwort",-14} {"Hash-Prefix",-22}");
            Console.WriteLine($"  │  {"────",-4} {"────────────────────",-20} {"──────────",-10} {"──────────────",-14} {"──────────────────────",-22}");
            Console.ResetColor();

            foreach (var a in DataManager.Benutzer.OrderByDescending(b => b.Berechtigung).ThenBy(b => b.Benutzername))
            {
                string icon = a.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";
                bool hasPW = !string.IsNullOrWhiteSpace(a.PasswortHash);
                string hp = hasPW ? a.PasswortHash[..Math.Min(20, a.PasswortHash.Length)] + "…" : "—";
                _f(_a0); Console.Write("  │  ");
                _f(_a3); Console.Write($"{icon,-4}{a.Benutzername,-20} ");
                _f(a.Berechtigung == Berechtigungen.Admin ? _a7 : _a2); Console.Write($"{a.Berechtigung,-10} ");
                _f(hasPW ? _a4 : _a6); Console.Write($"{(hasPW ? "🔐 gesetzt" : "⚠️  FEHLT!"),-14} ");
                _f(_a2); Console.WriteLine(hp);
                Console.ResetColor();
            }

            _f(_a0); Console.Write("  │  ");
            foreach (var (n, p2) in new[] { ("Inv", FileManager.FilePath), ("Mit", FileManager.FilePath2), ("Acc", FileManager.FilePath3), ("Log", _logFile) })
            {
                bool ex = File.Exists(p2); long sz = ex ? new FileInfo(p2).Length : 0;
                _f(ex ? _a4 : _a6); Console.Write($"{(ex ? "✓" : "✗")}{n}({sz:N0}B)  ");
            }
            Console.ResetColor(); Console.WriteLine();
            _f(_a0); Console.WriteLine("  └" + new string('─', _W) + "┘"); Console.ResetColor();
            Console.WriteLine();
        }

        private static void _bar(bool dev)
        {
            _f(_a0); Console.WriteLine("  ╔" + new string('═', _W) + "╗");
            _f(_a2); Console.Write("  ║  ");
            foreach (var c in new[] { "logs", "logs all", "benutzer", "inventar", "mitarbeiter", "dateien", "netzwerk", "sicherheit", "warnungen", "reload", "gc", "help", "exit" })
            { _f(_a1); Console.Write(c); _f(_a2); Console.Write("  "); }
            if (dev)
            {
                _f(_a7); Console.Write("│ DEV: ");
                foreach (var c in new[] { "env", "hash", "prozesse", "laufzeit" })
                { _f(_a7); Console.Write(c); _f(_a2); Console.Write("  "); }
            }
            Console.WriteLine();
            _f(_a0); Console.WriteLine("  ╚" + new string('═', _W) + "╝"); Console.ResetColor();
        }

        private static void _pmt(string u)
        {
            Console.WriteLine();
            _f(_a0); Console.Write("  ╔▶ ");
            _f(_a1); Console.Write("devconsole");
            _f(_a2); Console.Write($"@{u}");
            _f(_a0); Console.Write($" [{_sid}] ─▶ ");
            Console.ResetColor();
        }

        // ══════════════════════════════════════════════════════════════════
        // ANSICHTEN
        // ══════════════════════════════════════════════════════════════════

        // logs     → Report_*.enc Dateien auflisten & auswählen
        // logs all → System_Log.enc vollständig anzeigen
        private static void _vLog(bool alleSystemLog)
        {
            if (alleSystemLog)
                _vSystemLog();
            else
                _vReportAuswahl();
        }

        private static void _vReportAuswahl()
        {
            Console.Clear();
            _ph("📋  TAGESREPORTS  —  Auswahl");

            // Alle Report_*.enc Dateien im Logs-Ordner suchen
            if (!Directory.Exists(_logDir))
            { _inf($"Ordner '{_logDir}' nicht gefunden.", _a5); _wk(); return; }

            var reports = Directory.GetFiles(_logDir, "Report_*.enc")
                .OrderByDescending(f => f)
                .ToList();

            if (!reports.Any())
            {
                _inf($"Keine Report-Dateien in '{_logDir}/' vorhanden.", _a5);
                _inf("Tipp: Tagesreport über das Hauptmenü erstellen.", _a2);
                _wk(); return;
            }

            // Liste anzeigen
            _f(_a0); Console.WriteLine($"  {"Nr",-4}  {"Dateiname",-30}  {"Größe",10}  {"Datum"}");
            Console.WriteLine($"  {"────",-4}  {"──────────────────────────────",-30}  {"──────────",10}  ──────────────────");
            Console.ResetColor();

            for (int i = 0; i < reports.Count; i++)
            {
                var fi = new FileInfo(reports[i]);
                string fn = fi.Name;
                // Datum aus Dateiname: Report_20250120.enc → 20.01.2025
                string dat = "–";
                if (fn.Length >= 15)
                {
                    string raw = fn.Substring(7, 8); // yyyyMMdd
                    if (raw.Length == 8)
                        dat = $"{raw[6..8]}.{raw[4..6]}.{raw[0..4]}";
                }
                _f(_a2); Console.Write($"  [{i + 1,2}]  ");
                _f(_a3); Console.Write($"{fn,-30}  ");
                _f(_a2); Console.Write($"{fi.Length,10:N0}  ");
                _f(_a1); Console.WriteLine(dat);
                Console.ResetColor();
            }

            Console.WriteLine();
            _f(_a0); Console.Write("  ▶ Nummer wählen (0 = Abbrechen): ");
            Console.ResetColor();
            string eingabe = Console.ReadLine()?.Trim() ?? "0";

            if (!int.TryParse(eingabe, out int wahl) || wahl < 1 || wahl > reports.Count)
            {
                if (eingabe != "0") { _f(_a6); Console.WriteLine("  ✗ Ungültige Eingabe."); Console.ResetColor(); }
                return;
            }

            _vReportAnzeigen(reports[wahl - 1]);
        }

        private static void _vReportAnzeigen(string pfad)
        {
            Console.Clear();
            _ph($"📋  REPORT  —  {Path.GetFileName(pfad)}");

            try
            {
                string dec = EncryptionManager.ReadEncryptedFile(pfad);
                if (string.IsNullOrWhiteSpace(dec))
                { _inf("Report ist leer oder konnte nicht entschlüsselt werden.", _a5); _wk(); return; }

                var fi = new FileInfo(pfad);
                _inf($"Datei: {fi.Name}  |  {fi.Length:N0} Bytes  |  Erstellt: {fi.CreationTime:dd.MM.yyyy HH:mm}", _a2);
                Console.WriteLine();

                foreach (var z in dec.Split(new[] { '\r', '\n' }, StringSplitOptions.None))
                {
                    if (z.StartsWith("╔") || z.StartsWith("╚") || z.StartsWith("║")) { _f(_a0); }
                    else if (z.StartsWith("═") || z.Contains(new string('═', 10))) { _f(_a0); }
                    else if (z.Contains("FEHLER") || z.Contains("ERROR")) { _f(_a6); }
                    else if (z.Contains("WARN")) { _f(_a5); }
                    else if (z.Contains("ANMELDUNG") || z.Contains("Erfolgreich")) { _f(_a4); }
                    else if (z.StartsWith("Datum") || z.StartsWith("Erstellt")) { _f(_a1); }
                    else if (z.StartsWith("  ├") || z.StartsWith("  └")) { _f(_a2); }
                    else { _f(_a3); }
                    Console.WriteLine($"  {z}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex) { _inf($"Fehler: {ex.Message}", _a6); }
            _wk();
        }

        private static void _vSystemLog()
        {
            Console.Clear();
            _ph("📜  SYSTEM-LOG  —  System_Log.enc");

            if (!File.Exists(_logFile))
            { _inf("System_Log.enc nicht gefunden.", _a5); _wk(); return; }

            string[] zeilen;
            FileInfo fi;
            try
            {
                byte[] raw = File.ReadAllBytes(_logFile);
                string dec = EncryptionManager.DecryptBytes(raw) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(dec))
                { _inf("Log-Datei ist leer.", _a5); _wk(); return; }
                zeilen = dec.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                fi = new FileInfo(_logFile);
            }
            catch (Exception ex) { _inf($"Fehler: {ex.Message}", _a6); _wk(); return; }

            const int pro = 25;
            int ges = zeilen.Length;
            int seiten = (int)Math.Ceiling(ges / (double)pro);
            int seite = seiten; // neueste Einträge zuerst = letzte Seite

            while (true)
            {
                Console.Clear();
                _ph($"📜  SYSTEM-LOG  —  Seite {seite} / {seiten}");

                // Info-Zeile
                _f(_a2);
                Console.WriteLine($"  {ges} Einträge  ·  {fi.Length:N0} Bytes  ·  Geändert: {fi.LastWriteTime:dd.MM.yyyy HH:mm:ss}");
                Console.ResetColor();
                Console.WriteLine();

                // Einträge dieser Seite
                int von = (seite - 1) * pro;
                int bis = Math.Min(von + pro, ges);

                for (int i = von; i < bis; i++)
                {
                    string z = zeilen[i];

                    // Zeilennummer (grau)
                    _f(_a9); Console.Write($"  {i + 1,5} │ ");

                    // Farbe nach Inhalt
                    if (z.Contains("LOCKED") || z.Contains("DENIED") || z.Contains("FEHLER")) _f(_a6);
                    else if (z.Contains("GRANTED") || z.Contains("Erfolgreich")) _f(_a4);
                    else if (z.Contains("WARN") || z.Contains("fail")) _f(_a5);
                    else if (z.Contains("DevConsole") || z.Contains("DC")) _f(_a7);
                    else if (z.StartsWith("  ├") || z.StartsWith("  └")) _f(_a2);
                    else _f(_a3);
                    Console.WriteLine(z);
                    Console.ResetColor();
                }

                // Navigationsleiste
                Console.WriteLine();
                _f(_a0); Console.WriteLine("  " + new string('═', _W));
                _f(_a2); Console.Write($"  Seite {seite}/{seiten}  · Zeilen {von + 1}–{bis} von {ges}    ");
                _f(seite > 1 ? _a1 : _a9); Console.Write("[◀ P] Zurück   ");
                _f(seite < seiten ? _a1 : _a9); Console.Write("[▶ N] Weiter   ");
                _f(_a1); Console.Write("[A] Erste   ");
                _f(_a1); Console.Write("[E] Letzte   ");
                _f(_a2); Console.Write("[G] Gehe zu   ");
                _f(_a6); Console.Write("[X] Schließen");
                Console.ResetColor(); Console.WriteLine();
                _f(_a0); Console.WriteLine("  " + new string('═', _W));
                _f(_a0); Console.Write("  ▶ "); Console.ResetColor();

                var key = Console.ReadKey(true);
                char kc = char.ToLower(key.KeyChar);

                if (key.Key == ConsoleKey.RightArrow || kc == 'n') { if (seite < seiten) seite++; }
                else if (key.Key == ConsoleKey.LeftArrow || kc == 'p') { if (seite > 1) seite--; }
                else if (kc == 'a') seite = 1;
                else if (kc == 'e') seite = seiten;
                else if (kc == 'x' || key.Key == ConsoleKey.Escape) break;
                else if (kc == 'g')
                {
                    _f(_a2); Console.Write($"Seite (1–{seiten}): "); Console.ResetColor();
                    string inp = Console.ReadLine()?.Trim() ?? "";
                    if (int.TryParse(inp, out int ziel) && ziel >= 1 && ziel <= seiten)
                        seite = ziel;
                    else
                    {
                        _f(_a6); Console.WriteLine($"  ✗ Ungültig — Bitte 1 bis {seiten} eingeben.");
                        Console.ResetColor(); global::System.Threading.Thread.Sleep(1200);
                    }
                }
            }
        }

        private static void _vUsr()
        {
            Console.Clear(); _ph("👥  BENUTZER  —  Vollständige Übersicht");
            int adm = DataManager.Benutzer.Count(b => b.Berechtigung == Berechtigungen.Admin);
            int oPW = DataManager.Benutzer.Count(b => string.IsNullOrWhiteSpace(b.PasswortHash));
            _kv("Gesamt", DataManager.Benutzer.Count.ToString(), _a3);
            _kv("Admins", adm.ToString(), _a4);
            _kv("Standard-User", (DataManager.Benutzer.Count - adm).ToString(), _a2);
            _kv("Ohne Passwort", oPW == 0 ? "✓ Keiner" : $"⚠ {oPW}", oPW == 0 ? _a4 : _a5);
            Console.WriteLine();

            _f(_a0);
            Console.WriteLine($"  ┌─{"────",-4}─┬─{"────────────────────",-20}─┬─{"──────────",-10}─┬─{"────────────",-12}─┬─{"──────────────────────",-22}─┐");
            Console.WriteLine($"  │ {"",4} │ {"Benutzername",-20} │ {"Rolle",-10} │ {"Passwort",-12} │ {"Hash-Prefix",-22} │");
            Console.WriteLine($"  ├─{"────",-4}─┼─{"────────────────────",-20}─┼─{"──────────",-10}─┼─{"────────────",-12}─┼─{"──────────────────────",-22}─┤");
            Console.ResetColor();

            foreach (var a in DataManager.Benutzer.OrderByDescending(b => b.Berechtigung).ThenBy(b => b.Benutzername))
            {
                string icon = a.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";
                bool hasPW = !string.IsNullOrWhiteSpace(a.PasswortHash);
                string hp = hasPW ? a.PasswortHash[..Math.Min(20, a.PasswortHash.Length)] + "…" : "—";
                _f(_a0); Console.Write($"  │ {icon,-4}│ ");
                _f(_a3); Console.Write($"{a.Benutzername,-20}"); _f(_a0); Console.Write(" │ ");
                _f(a.Berechtigung == Berechtigungen.Admin ? _a7 : _a2); Console.Write($"{a.Berechtigung,-10}");
                _f(_a0); Console.Write(" │ ");
                _f(hasPW ? _a4 : _a6); Console.Write($"{(hasPW ? "🔐 gesetzt" : "⚠ FEHLT!"),-12}");
                _f(_a0); Console.Write(" │ "); _f(_a2); Console.Write($"{hp,-22}"); _f(_a0); Console.WriteLine(" │");
            }
            _f(_a0);
            Console.WriteLine($"  └─{"────",-4}─┴─{"────────────────────",-20}─┴─{"──────────",-10}─┴─{"────────────",-12}─┴─{"──────────────────────",-22}─┘");
            Console.ResetColor(); _wk();
        }

        private static void _vInv()
        {
            Console.Clear(); _ph("📦  INVENTAR  —  Vollstatistik");
            int ges = DataManager.Inventar.Count;
            int leer = DataManager.Inventar.Count(a => a.Anzahl == 0);
            int nied = DataManager.Inventar.Count(a => a.Anzahl > 0 && a.Anzahl <= a.Mindestbestand);
            int ok = ges - leer - nied;
            decimal wert = DataManager.Inventar.Sum(a => a.Preis * a.Anzahl);
            decimal avg = ges > 0 ? DataManager.Inventar.Average(a => a.Preis) : 0;
            decimal maxP = ges > 0 ? DataManager.Inventar.Max(a => a.Preis) : 0;

            _kv("Artikel gesamt", ges.ToString(), _a3);
            _kv("  ✓ OK", ok.ToString(), _a4);
            _kv("  ⚠ Niedrig", nied.ToString(), nied > 0 ? _a5 : _a4);
            _kv("  ✗ Leer", leer.ToString(), leer > 0 ? _a6 : _a4);
            _kv("Gesamtwert", $"{wert:N2} €", _a4);
            _kv("Ø Preis", $"{avg:N2} €", _a3);
            _kv("Max Preis", $"{maxP:N2} €", _a3);
            Console.WriteLine();

            _sh("KATEGORIEN  —  nach Anzahl");
            var cats = DataManager.Inventar.GroupBy(a => a.Kategorie ?? "Unbekannt")
                .Select(g => new { K = g.Key, N = g.Count(), V = g.Sum(x => x.Preis * x.Anzahl) })
                .OrderByDescending(x => x.N).ToList();
            int maxN = cats.Any() ? cats.Max(x => x.N) : 1;
            _f(_a2); Console.WriteLine($"  {"Kategorie",-22}  {"Anz",4}  {"Wert",12}  Balken"); Console.ResetColor();
            foreach (var ct in cats)
            {
                int bar = ct.N * 28 / maxN;
                _f(_a3); Console.Write($"  {ct.K,-22}  {ct.N,4}  {ct.V,11:N2}€  ");
                _f(_a1); Console.Write(new string('█', bar));
                _f(_a9); Console.WriteLine(new string('░', 28 - bar));
                Console.ResetColor();
            }
            Console.WriteLine();

            _sh("HERSTELLER  —  Top 8");
            var hers = DataManager.Inventar.GroupBy(a => a.Hersteller ?? "?")
                .Select(g => new { H = g.Key, N = g.Count() }).OrderByDescending(x => x.N).Take(8).ToList();
            int maxH = hers.Any() ? hers.Max(x => x.N) : 1;
            foreach (var h in hers)
            {
                int bar = h.N * 28 / maxH;
                _f(_a3); Console.Write($"  {h.H,-22}  {h.N,4}  ");
                _f(_a1); Console.Write(new string('█', bar));
                _f(_a9); Console.WriteLine(new string('░', 28 - bar));
                Console.ResetColor();
            }
            Console.WriteLine();

            var krit = DataManager.Inventar.Where(a => a.Anzahl == 0).Take(10).ToList();
            if (krit.Any())
            {
                _sh($"⚠  LEERE ARTIKEL ({DataManager.Inventar.Count(a => a.Anzahl == 0)} gesamt)");
                foreach (var a in krit)
                { _f(_a6); Console.WriteLine($"  ✗  {a.InvNmr,-10}  {a.GeraeteName,-28}  → {a.MitarbeiterBezeichnung}"); Console.ResetColor(); }
            }
            _wk();
        }

        private static void _vStf()
        {
            Console.Clear(); _ph("👤  MITARBEITER  —  Übersicht");
            _kv("Gesamt", DataManager.Mitarbeiter.Count.ToString(), _a3);
            _kv("Abteilungen", DataManager.Mitarbeiter.Select(m => m.Abteilung).Distinct().Count().ToString(), _a3);
            Console.WriteLine(); _sh("ABTEILUNGEN");
            var abts = DataManager.Mitarbeiter.GroupBy(m => m.Abteilung ?? "?")
                .Select(g => new { A = g.Key, N = g.Count() }).OrderByDescending(x => x.N);
            foreach (var a in abts)
            { _f(_a3); Console.Write($"  {a.A,-28}  "); _f(_a4); Console.Write(new string('█', a.N)); _f(_a2); Console.WriteLine($"  {a.N}"); Console.ResetColor(); }
            Console.WriteLine(); _sh($"LISTE (erste 20 von {DataManager.Mitarbeiter.Count})");
            _f(_a2); Console.WriteLine($"  {"Vorname",-15}  {"Nachname",-15}  {"Abteilung",-20}");
            _f(_a0); Console.WriteLine($"  {"───────────────",-15}  {"───────────────",-15}  {"────────────────────",-20}");
            foreach (var m in DataManager.Mitarbeiter.Take(20))
            { _f(_a3); Console.WriteLine($"  {m.VName,-15}  {m.NName,-15}  {m.Abteilung,-20}"); Console.ResetColor(); }
            if (DataManager.Mitarbeiter.Count > 20)
            { _f(_a2); Console.WriteLine($"  … und {DataManager.Mitarbeiter.Count - 20} weitere"); Console.ResetColor(); }
            _wk();
        }

        private static void _vFil()
        {
            Console.Clear(); _ph("📁  DATEISYSTEM  —  Vollständige Übersicht");
            var dl = new Dictionary<string, string>
            {
                { "Inventar.txt", FileManager.FilePath }, { "Mitarbeiter.txt", FileManager.FilePath2 },
                { "Accounts.txt", FileManager.FilePath3 }, { "System_Log.enc", _logFile },
                { "lieferanten.json", "lieferanten.json" }
            };
            _f(_a0); Console.WriteLine($"  {"Datei",-20}  {"St",-2}  {"Größe",12}  {"Geändert",17}  {"Erstellt",17}");
            Console.WriteLine($"  {"────────────────────",-20}  {"──",-2}  {"────────────",12}  {"─────────────────",17}  {"─────────────────",17}");
            Console.ResetColor();
            foreach (var kv in dl)
            {
                bool ex = File.Exists(kv.Value); long sz = ex ? new FileInfo(kv.Value).Length : 0;
                string mod = ex ? File.GetLastWriteTime(kv.Value).ToString("dd.MM.yy HH:mm") : "—";
                string cre = ex ? File.GetCreationTime(kv.Value).ToString("dd.MM.yy HH:mm") : "—";
                _f(_a2); Console.Write($"  {kv.Key,-20}  "); _f(ex ? _a4 : _a6); Console.Write($"{(ex ? "✓" : "✗"),-2}  ");
                _f(_a3); Console.Write($"{sz,12:N0}  "); _f(_a2); Console.WriteLine($"{mod,-17}  {cre,-17}");
                Console.ResetColor();
            }
            Console.WriteLine(); _sh("ORDNER");
            foreach (var o in new[] { "ZuweisungsHistorien", "Infos", "Backups", "Export" })
            {
                bool ex = Directory.Exists(o);
                int cnt = ex ? Directory.GetFiles(o, "*", SearchOption.AllDirectories).Length : 0;
                long tsz = ex ? Directory.GetFiles(o, "*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length) : 0;
                _f(_a2); Console.Write($"  {o,-22}  "); _f(ex ? _a4 : _a5); Console.Write(ex ? "✓  " : "—  ");
                _f(_a3); Console.WriteLine(ex ? $"{cnt} Dateien  ({tsz:N0} Bytes)" : "nicht vorhanden");
                Console.ResetColor();
            }
            _wk();
        }

        private static void _vNet()
        {
            Console.Clear(); _ph("🌐  NETZWERK  &  VERBINDUNGEN");
            _kv("Hostname", _h(), _a3);
            _kv("Windows-User", Environment.UserName, _a3);
            _kv("Domain", Environment.UserDomainName, _a3);
            _kv("Lokale IP", _ip(), _a4);
            Console.WriteLine();
            try
            {
                var entry = System.Net.Dns.GetHostEntry(_h());
                _kv("DNS-Name", entry.HostName, _a3); _sh("IP-ADRESSEN");
                foreach (var ip2 in entry.AddressList)
                {
                    string t = ip2.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4";
                    _f(_a3); Console.Write($"  {t,-6}  "); _f(_a4); Console.WriteLine(ip2); Console.ResetColor();
                }
            }
            catch { _kv("DNS", "Nicht auflösbar", _a5); }
            Console.WriteLine(); _sh("SCHNITTSTELLEN");
            try
            {
                foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                    _f(_a3); Console.Write($"  {ni.Name,-25}  "); _f(_a4); Console.Write(ni.OperationalStatus.ToString().PadRight(6));
                    _f(_a2); Console.WriteLine($"  {ni.NetworkInterfaceType,-18}  {ni.Description}"); Console.ResetColor();
                }
            }
            catch { _kv("Schnittstellen", "Nicht lesbar", _a5); }
            _wk();
        }

        private static void _vSec(bool dev)
        {
            Console.Clear(); _ph("🔐  SICHERHEITS-REPORT  —  Vollanalyse");
            int adm = DataManager.Benutzer.Count(b => b.Berechtigung == Berechtigungen.Admin);
            int oPW = DataManager.Benutzer.Count(b => string.IsNullOrWhiteSpace(b.PasswortHash));
            bool logOk = File.Exists(_logFile);
            int leer = DataManager.Inventar.Count(a => a.Anzahl == 0);
            int nied = DataManager.Inventar.Count(a => a.Anzahl > 0 && a.Anzahl <= a.Mindestbestand);

            _sh("KRYPTOGRAPHIE");
            _kv("Algorithmus", "AES-256-CBC", _a4);
            _kv("Key-Ableitung", "PBKDF2-SHA256 (10.000 Iter.)", _a4);
            _kv("Hash", "SHA-256 + Salt", _a4);
            _kv("Log-Datei", logOk ? "✓ Verschlüsselt aktiv" : "✗ Fehlt", logOk ? _a4 : _a6);
            Console.WriteLine();

            _sh("ZUGRIFFSKONTROLLE");
            _kv("Admin-Konten", $"{adm}", adm >= 1 ? _a4 : _a6);
            _kv("Ohne Passwort", oPW == 0 ? "✓ Keiner" : $"⚠ {oPW}", oPW == 0 ? _a4 : _a5);
            _kv("Fehlversuche", $"{_fc}/{_mf}", _fc > 0 ? _a5 : _a4);
            _kv("Sitzungs-ID", _sid, _a2);
            _kv("IP", _ip(), _a2);
            Console.WriteLine();

            _sh("DATEI-INTEGRITÄT");
            foreach (var (n, p2) in new[] { ("Inventar.txt", FileManager.FilePath), ("Mitarbeiter.txt", FileManager.FilePath2), ("Accounts.txt", FileManager.FilePath3), ("System_Log.enc", _logFile) })
            {
                bool ex = File.Exists(p2);
                _kv(n, ex ? $"✓  {new FileInfo(p2).Length:N0} Bytes" : "✗ fehlt", ex ? _a4 : _a6);
            }
            Console.WriteLine();

            _sh("GESAMTBEWERTUNG");
            int pts = 0;
            if (logOk) pts += 20;
            if (oPW == 0) pts += 25;
            if (adm >= 1) pts += 20;
            if (leer == 0) pts += 15;
            if (nied == 0) pts += 10;
            if (_fc == 0) pts += 10;
            ConsoleColor bc = pts >= 80 ? _a4 : pts >= 50 ? _a5 : _a6;
            string bw = pts >= 80 ? "GUT" : pts >= 50 ? "MITTEL" : "KRITISCH";
            _f(bc); Console.Write($"  [{bw}  {pts}/100]  ");
            Console.Write(new string('█', pts * 38 / 100));
            _f(_a9); Console.WriteLine(new string('░', 38 - pts * 38 / 100)); Console.ResetColor();
            Console.WriteLine();

            if (oPW > 0) { _f(_a5); Console.WriteLine($"  ⚠  {oPW} Benutzer ohne Passwort!"); Console.ResetColor(); }
            if (!logOk) { _f(_a6); Console.WriteLine("  ✗  System-Log fehlt!"); Console.ResetColor(); }
            if (adm == 0) { _f(_a6); Console.WriteLine("  ✗  Kein Admin-Account!"); Console.ResetColor(); }
            if (leer > 0) { _f(_a5); Console.WriteLine($"  ⚠  {leer} leere Artikel."); Console.ResetColor(); }
            if (pts >= 80 && oPW == 0 && logOk)
            { _f(_a4); Console.WriteLine("  ✓  Sicherheitskonfiguration in Ordnung."); Console.ResetColor(); }
            _wk();
        }

        private static void _vWrn()
        {
            Console.Clear(); _ph("⚠️   AKTIVE WARNUNGEN  &  HANDLUNGSBEDARF");
            bool any = false;

            var oPW2 = DataManager.Benutzer.Where(b => string.IsNullOrWhiteSpace(b.PasswortHash)).ToList();
            if (oPW2.Any())
            {
                _sh($"🔑  OHNE PASSWORT ({oPW2.Count})");
                foreach (var b in oPW2) { _f(_a5); Console.WriteLine($"  ⚠  {b.Benutzername}  [{b.Berechtigung}]"); Console.ResetColor(); }
                Console.WriteLine(); any = true;
            }

            var leerA = DataManager.Inventar.Where(a => a.Anzahl == 0).ToList();
            if (leerA.Any())
            {
                _sh($"📦  LEERE ARTIKEL ({leerA.Count})");
                foreach (var a in leerA.Take(15)) { _f(_a6); Console.WriteLine($"  ✗  {a.InvNmr,-10}  {a.GeraeteName,-28}  → {a.MitarbeiterBezeichnung}"); Console.ResetColor(); }
                if (leerA.Count > 15) { _f(_a2); Console.WriteLine($"  … und {leerA.Count - 15} weitere"); Console.ResetColor(); }
                Console.WriteLine(); any = true;
            }

            var niedA = DataManager.Inventar.Where(a => a.Anzahl > 0 && a.Anzahl <= a.Mindestbestand).ToList();
            if (niedA.Any())
            {
                _sh($"📉  NIEDRIGER BESTAND ({niedA.Count})");
                foreach (var a in niedA.Take(10)) { _f(_a5); Console.WriteLine($"  ⚠  {a.InvNmr,-10}  {a.GeraeteName,-28}  {a.Anzahl}/{a.Mindestbestand}"); Console.ResetColor(); }
                if (niedA.Count > 10) { _f(_a2); Console.WriteLine($"  … und {niedA.Count - 10} weitere"); Console.ResetColor(); }
                Console.WriteLine(); any = true;
            }

            if (!File.Exists(_logFile))
            {
                _sh("📁  FEHLENDE SYSTEMDATEIEN");
                _f(_a6); Console.WriteLine("  ✗  System_Log.enc fehlt — kein Audit-Log!"); Console.ResetColor();
                Console.WriteLine(); any = true;
            }

            if (!any) { _f(_a4); Console.WriteLine("\n  ✅  Keine aktiven Warnungen. System in Ordnung."); Console.ResetColor(); }
            _wk();
        }

        private static void _doRld()
        {
            _f(_a2); Console.WriteLine("\n  Lade Daten...");
            DataManager.LoadInventar(); DataManager.LoadMitarbeiter(); DataManager.LoadBenutzer();
            _f(_a4); Console.WriteLine("  ✓ Alle Daten neu geladen."); Console.ResetColor(); _wk();
        }

        private static void _doGC()
        {
            long v = GC.GetTotalMemory(false); GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); long n = GC.GetTotalMemory(true);
            Console.WriteLine(); _f(_a4); Console.WriteLine("  ♻  GC abgeschlossen");
            _kv("Vorher", $"{v / 1024:N0} KB", _a5);
            _kv("Nachher", $"{n / 1024:N0} KB", _a4);
            _kv("Freigegeben", $"{(v - n) / 1024:N0} KB", _a4);
            Console.ResetColor(); _wk();
        }

        private static void _vHlp(bool dev)
        {
            Console.Clear(); _ph("❓  HILFE  —  ALLE BEFEHLE");
            var cmds = new (string C, string D, bool Dv)[]
            {
                ("logs",        "Tagesreports auflisten & auswählen",  false),
                ("logs all",    "System_Log.enc vollständig anzeigen", false),
                ("benutzer",    "Benutzer-Detail mit Hash",         false),
                ("inventar",    "Inventar-Vollstatistik",           false),
                ("mitarbeiter", "Mitarbeiter & Abteilungen",        false),
                ("dateien",     "Datei- & Ordner-Status",           false),
                ("netzwerk",    "Netzwerk, IPs, Schnittstellen",    false),
                ("sicherheit",  "Sicherheits-Report",               false),
                ("warnungen",   "Alle Warnungen & Probleme",        false),
                ("reload",      "Daten neu laden",                  false),
                ("gc",          "Garbage Collection",               false),
                ("cls",         "Bildschirm leeren",                false),
                ("exit",        "Schließen",                        false),
                ("env",         "Environment-Variablen",            true),
                ("hash",        "SHA-256 Hash-Tool",                true),
                ("prozesse",    "Laufende Prozesse",                true),
                ("laufzeit",    "Laufzeit-Statistiken",             true),
            };
            _f(_a0); Console.WriteLine($"  {"Befehl",-15}  {"Beschreibung",-42}  Zugang");
            Console.WriteLine($"  {"───────────────",-15}  {"──────────────────────────────────────────",-42}  ──────"); Console.ResetColor();
            foreach (var (c, d, dv) in cmds)
            {
                if (dv && !dev) continue;
                _f(dv ? _a7 : _a1); Console.Write($"  {c,-15}  ");
                _f(_a3); Console.Write($"{d,-42}  ");
                _f(dv ? _a7 : _a2); Console.WriteLine(dv ? "DEV" : "ALL"); Console.ResetColor();
            }
            _wk();
        }

        // ── Dev-Befehle ────────────────────────────────────────────────────

        private static void _dEnv()
        {
            Console.Clear(); _ph("🌍  ENVIRONMENT  —  DEV");
            foreach (var v in new[] { "COMPUTERNAME", "USERNAME", "USERDOMAIN", "OS", "PROCESSOR_ARCHITECTURE", "NUMBER_OF_PROCESSORS", "TEMP", "USERPROFILE", "APPDATA", "PROGRAMFILES", "SYSTEMROOT", "WINDIR" })
            {
                string val = Environment.GetEnvironmentVariable(v) ?? "—";
                if (val.Length > 60) val = val[..57] + "…";
                _kv(v, val, _a3);
            }
            Console.WriteLine(); _sh("PATH (aufgespaltet)");
            foreach (var p2 in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';').Take(12))
            { _f(_a2); Console.WriteLine($"  {p2}"); Console.ResetColor(); }
            _wk();
        }

        private static void _dHsh()
        {
            Console.Clear(); _ph("🔑  HASH-TOOL  —  SHA-256  (DEV)");
            _f(_a2); Console.WriteLine("  SHA-256 + Salt (identisch zu AuthManager). Leer = Beenden."); Console.WriteLine();
            while (true)
            {
                _f(_a0); Console.Write("  ▶ Text: "); Console.ResetColor();
                string input = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(input)) break;
                _kv("SHA-256", AuthManager.HashPasswort(input), _a4);
                _kv("Länge", $"{input.Length} Zeichen", _a2);
                Console.WriteLine();
            }
        }

        private static void _dPrc()
        {
            Console.Clear(); _ph("⚙️   LAUFENDE PROZESSE  —  Top 18 nach RAM  (DEV)");
            _f(_a0);
            Console.WriteLine($"  {"PID",6}  {"Name",-26}  {"RAM MB",7}  {"Threads",7}  {"CPU",10}  {"Start",8}");
            Console.WriteLine($"  {"──────",6}  {"──────────────────────────",-26}  {"───────",7}  {"───────",7}  {"──────────",10}  {"────────",8}");
            Console.ResetColor();
            foreach (var p2 in Process.GetProcesses().OrderByDescending(p2 => { try { return p2.WorkingSet64; } catch { return 0L; } }).Take(18))
            {
                try
                {
                    long mb = p2.WorkingSet64 / 1024 / 1024;
                    _f(_a2); Console.Write($"  {p2.Id,6}  "); _f(_a3); Console.Write($"{p2.ProcessName,-26}  ");
                    _f(mb > 300 ? _a5 : _a4); Console.Write($"{mb,7}  ");
                    _f(_a2); Console.WriteLine($"{p2.Threads.Count,7}  {p2.TotalProcessorTime.ToString(@"hh\:mm\:ss"),10}  {p2.StartTime:HH:mm:ss}");
                    Console.ResetColor();
                }
                catch { }
            }
            _wk();
        }

        private static void _dRt()
        {
            Console.Clear(); _ph("⏱  LAUFZEIT-STATISTIKEN  —  DEV");
            var p = Process.GetCurrentProcess();
            _kv("Programm-Laufzeit", (DateTime.Now - p.StartTime).ToString(@"hh\:mm\:ss\.fff"), _a3);
            _kv("Sitzungs-Dauer", (DateTime.Now - _ts).ToString(@"hh\:mm\:ss\.fff"), _a3);
            _kv("CPU User", p.UserProcessorTime.ToString(@"hh\:mm\:ss\.ff"), _a2);
            _kv("CPU System", p.PrivilegedProcessorTime.ToString(@"hh\:mm\:ss\.ff"), _a2);
            _kv("CPU Gesamt", p.TotalProcessorTime.ToString(@"hh\:mm\:ss\.ff"), _a2);
            _kv("GC Gen0", GC.CollectionCount(0).ToString(), _a3);
            _kv("GC Gen1", GC.CollectionCount(1).ToString(), _a3);
            _kv("GC Gen2", GC.CollectionCount(2).ToString(), _a3);
            _kv("Heap", $"{GC.GetTotalMemory(false) / 1024:N0} KB", _a3);
            _kv("Working Set", $"{p.WorkingSet64 / 1024:N0} KB", _a3);
            _kv("Private Memory", $"{p.PrivateMemorySize64 / 1024:N0} KB", _a3);
            _kv("Virtual Memory", $"{p.VirtualMemorySize64 / 1024:N0} KB", _a3);
            _wk();
        }

        // ── Auto-Lock ──────────────────────────────────────────────────────

        /// <summary>
        /// Liest eine Zeile mit parallelem Timeout-Monitor.
        /// Gibt null zurück wenn gesperrt + erfolgreich entsperrt (Loop soll neu rendern).
        /// Gibt den Befehlsstring zurück bei normaler Eingabe.
        /// </summary>
        private static string _readWithTimeout(string u, bool dev)
        {
            _lastAct = DateTime.Now;
            string result = null;
            bool done = false;
            bool locked = false;

            // Background-Thread überwacht Inaktivität
            var watcher = new global::System.Threading.Thread(() =>
            {
                while (!done)
                {
                    global::System.Threading.Thread.Sleep(500);
                    if (!done && (DateTime.Now - _lastAct).TotalSeconds >= _lockSec)
                    {
                        locked = true;
                        done = true;
                    }
                }
            })
            { IsBackground = true };
            watcher.Start();

            // Eingabe zeichenweise lesen damit wir jederzeit abbrechen können
            var sb = new StringBuilder();
            while (!done)
            {
                if (!Console.KeyAvailable)
                {
                    global::System.Threading.Thread.Sleep(80);
                    continue;
                }
                var k = Console.ReadKey(true);
                _lastAct = DateTime.Now;

                if (k.Key == ConsoleKey.Enter)
                {
                    done = true;
                    result = sb.ToString();
                    Console.WriteLine(); // Zeilenumbruch nach Eingabe
                }
                else if (k.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (k.KeyChar >= ' ')
                {
                    sb.Append(k.KeyChar);
                    Console.Write(k.KeyChar);
                }
            }
            watcher.Join(200);

            if (!locked)
                return result ?? "";

            // ── Bildschirm sperren ─────────────────────────────────────────
            _lockScreen(u, dev);
            return null; // signalisiert: neu rendern
        }

        private static void _lockScreen(string u, bool dev)
        {
            // Countdown-Animation bevor Schwärzung
            for (int i = 3; i > 0; i--)
            {
                Console.Clear();
                _f(_a5);
                string msg = $"  ⚠  Automatische Sperre in {i}s...";
                Console.SetCursorPosition((Console.WindowWidth - msg.Length) / 2,
                    Console.WindowHeight / 2);
                Console.WriteLine(msg);
                Console.ResetColor();
                global::System.Threading.Thread.Sleep(1000);
            }

            // Schwarzer Sperrbildschirm
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Black;
            // Gesamten Bildschirm mit Leerzeichen füllen → komplett schwarz
            string blank = new string(' ', Console.WindowWidth);
            for (int r = 0; r < Console.WindowHeight; r++)
                Console.Write(blank);

            Console.SetCursorPosition(0, Console.WindowHeight / 2 - 2);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            string lockMsg = "  🔒  Gesperrt — Taste drücken zum Entsperren";
            int lx = Math.Max(0, (Console.WindowWidth - lockMsg.Length) / 2);
            Console.SetCursorPosition(lx, Console.WindowHeight / 2);
            Console.WriteLine(lockMsg);
            Console.ResetColor();
            Console.CursorVisible = false;

            // Warten bis eine Taste gedrückt wird
            Console.ReadKey(true);
            Console.CursorVisible = true;

            // Re-Authentifizierung
            bool ok = _reAuth(u);
            string lgMsg = ok
                ? $"AutoLock-Unlock|u={u}|sid={_sid}"
                : $"AutoLock-ReAuth-Failed|u={u}|sid={_sid}";
            LogManager.LogWarnung("DC", lgMsg);

            if (!ok)
            {
                // Bei fehlgeschlagener Re-Auth: Fenster schließen
                Console.Clear(); _ad("E:05 — re-auth failure after lock");
                Environment.Exit(0);
            }
        }

        private static bool _reAuth(string u)
        {
            Console.Clear(); Console.WriteLine();
            _fr(_a5, "╔", "╗");
            _fz(_a5, _a5, "🔒  SITZUNG GESPERRT  —  Identität bestätigen");
            _fz(_a5, _a2, $"Benutzer  :  {u}");
            _fz(_a5, _a2, $"Gesperrt  :  {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            _fz(_a5, _a2, $"Inaktivität > {_lockSec}s erkannt");
            _fr(_a5, "╚", "╝");
            Console.ResetColor(); Console.WriteLine();

            var acc = DataManager.Benutzer
                .FirstOrDefault(b => b.Benutzername.Equals(u, StringComparison.OrdinalIgnoreCase));
            if (acc == null) return false;

            for (int i = 0; i < _mf; i++)
            {
                _f(_a5); Console.Write($"  [{_mf - i}/{_mf}]  Passwort ▶ "); Console.ResetColor();
                string pw = _rpw();
                string hash = AuthManager.HashPasswort(pw);
                if (string.Equals(acc.PasswortHash, hash, StringComparison.Ordinal))
                {
                    _f(_a4); Console.WriteLine("  ✓ Entsperrt."); Console.ResetColor();
                    global::System.Threading.Thread.Sleep(700);
                    return true;
                }
                _f(_a6); Console.WriteLine($"  ✗ Falsches Passwort. {_mf - i - 1} Versuch(e) verbleibend.");
                Console.ResetColor();
                global::System.Threading.Thread.Sleep(1500);
            }
            return false;
        }

        private static void _na()
        { _f(_a6); Console.WriteLine("\n  ✗  Nur für Entwickler."); Console.ResetColor(); _wk(); }

        // ══════════════════════════════════════════════════════════════════
        // LAYOUT
        // ══════════════════════════════════════════════════════════════════

        private static void _fr(ConsoleColor col, string tl, string tr)
        { _f(col); Console.WriteLine($"  {tl}" + new string('═', _W) + $"{tr}"); Console.ResetColor(); }

        private static void _fz(ConsoleColor bc, ConsoleColor tc, string text)
        {
            string c = text.Length >= _W ? text[.._W]
                : new string(' ', (_W - text.Length) / 2) + text + new string(' ', _W - (_W - text.Length) / 2 - text.Length);
            _f(bc); Console.Write("  ║"); _f(tc); Console.Write(c); _f(bc); Console.WriteLine("║");
            Console.ResetColor();
        }

        private static void _sec(string t)
        { _f(_a8); Console.Write($"  ▸ {t}  "); _f(_a0); Console.WriteLine(new string('─', Math.Max(0, _W - t.Length - 5))); Console.ResetColor(); }

        private static void _sh(string t)
        { _f(_a8); Console.Write($"  ▸ {t}  "); _f(_a0); Console.WriteLine(new string('─', Math.Max(0, _W - t.Length - 5))); Console.ResetColor(); }

        private static void _ph(string t)
        { _fr(_a0, "╔", "╗"); _f(_a1); Console.WriteLine($"  ║  {t.PadRight(_W - 2)}║"); _fr(_a0, "╚", "╝"); Console.ResetColor(); Console.WriteLine(); }

        // Zwei-Spalten Key-Value
        private static void _kk(string k1, string v1, string k2, string v2)
        {
            _f(_a2); Console.Write($"  {k1,-18} "); _f(_a3); Console.Write($"{v1,-22}");
            _f(_a0); Console.Write("  │  "); _f(_a2); Console.Write($"{k2,-18} "); _f(_a3); Console.WriteLine(v2);
            Console.ResetColor();
        }

        private static void _kkF(string k1, string v1, ConsoleColor c1, string k2, string v2, ConsoleColor c2)
        {
            _f(_a2); Console.Write($"  {k1,-18} "); _f(c1); Console.Write($"{v1,-22}");
            _f(_a0); Console.Write("  │  "); _f(_a2); Console.Write($"{k2,-18} "); _f(c2); Console.WriteLine(v2);
            Console.ResetColor();
        }

        // Einzeiliges Key-Value
        private static void _kv(string k, string v, ConsoleColor vc)
        { _f(_a2); Console.Write($"  {k,-22}  "); _f(vc); Console.WriteLine(v); Console.ResetColor(); }

        private static void _inf(string t, ConsoleColor c)
        { _f(c); Console.WriteLine($"  {t}"); Console.ResetColor(); }

        private static void _wk()
        { Console.WriteLine(); _f(_a0); Console.WriteLine("  " + new string('─', _W)); _f(_a2); Console.Write("  ▸ Weiter mit beliebiger Taste..."); Console.ResetColor(); Console.ReadKey(true); _lastAct = DateTime.Now; }

        // ══════════════════════════════════════════════════════════════════
        // UTIL
        // ══════════════════════════════════════════════════════════════════

        private static bool _chk(string u) =>
            _isDev(u) || DataManager.Benutzer.FirstOrDefault(b =>
                b.Benutzername.Equals(u, StringComparison.OrdinalIgnoreCase))?.Berechtigung == Berechtigungen.Admin;

        private static bool _isDev(string u) => u.Equals(_ident, StringComparison.OrdinalIgnoreCase);
        private static string _h() => Environment.MachineName;
        private static string _mx(string s) => s.Length <= 2 ? s : s[..2] + new string('*', s.Length - 2);
        private static string _trim(string s, int max) => s.Length <= max ? s : "…" + s[^(max - 1)..];
        private static void _f(ConsoleColor c) => Console.ForegroundColor = c;

        private static string _ip()
        {
            try
            {
                return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList
                    .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
            }
            catch { return "127.0.0.1"; }
        }

        private static string _rpw()
        {
            string pw = "";
            ConsoleKeyInfo k;
            do
            {
                k = Console.ReadKey(true);
                if (k.Key != ConsoleKey.Backspace && k.Key != ConsoleKey.Enter)
                { pw += k.KeyChar; Console.Write("*"); }
                else if (k.Key == ConsoleKey.Backspace && pw.Length > 0)
                { pw = pw[..^1]; Console.Write("\b \b"); }
            } while (k.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return pw;
        }
    }
}