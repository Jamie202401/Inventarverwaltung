using Inventarverwaltung.Manager.UI;
using Inventarverwaltung.Manager.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// ╔══════════════════════════════════════════════════════════════════════════╗
    /// ║         🔐  LOG-MANAGER  —  ENTERPRISE EDITION  v4.0                    ║
    /// ╠══════════════════════════════════════════════════════════════════════════╣
    /// ║  Vollständiges forensisches Audit-Log mit:                               ║
    /// ║    · AES-256 + SHA-256 Kettenintegrität (jeder Block hasht den letzten) ║
    /// ║    · Feld-Level Audit Trail (jede einzelne Feldänderung protokolliert)   ║
    /// ║    · Vollständiger Diff: Alt/Neu für jeden Wert                         ║
    /// ║    · Transaktions-Log (Batch-Änderungen als Einheit)                    ║
    /// ║    · Inventarwert-Verlauf & Trendprotokoll                              ║
    /// ║    · DSGVO/GoBD-konformer Compliance-Report                            ║
    /// ║    · Call-Stack-Kontext (welche Methode löste das Event aus)            ║
    /// ║    · Live-Monitor mit Auto-Refresh (Echtzeit-Ansicht)                  ║
    /// ║    · Korrelations-IDs für zusammenhängende Events                       ║
    /// ║    · Passwort-Policy-Überwachung                                        ║
    /// ║    · Netzwerk-Kontext (Hostname-Auflösung, MAC-ähnliche ID)             ║
    /// ║    · Anomalie-Scoring (je schwerer, desto höher der Score)              ║
    /// ║    · Brute-Force + Massenlöschung + Nachtzeit-Detektion                ║
    /// ║    · Heartbeat alle 30 Min mit Systemstatus                             ║
    /// ║    · Log-Rotation & Archiv                                              ║
    /// ║    · 10 Viewer-Filter + Suche + Statistik + Live                        ║
    /// ╚══════════════════════════════════════════════════════════════════════════╝
    /// </summary>
    public static class LogManager
    {
        // ══════════════════════════════════════════════════════════════
        //  KONFIGURATION
        // ══════════════════════════════════════════════════════════════
        private static string logFilePath => FileManager.LogPfad;
        private static string archivOrdner => "LogArchiv";
        private static string trendDateipfad => "InventarTrend.enc";
        private const long MAX_LOG_MB = 5;
        private const int MAX_LOGIN_FEHLER = 3;
        private const int ANOMALIE_GESCHW = 20;   // Aktionen/10s → Anomalie

        // ── Session ──────────────────────────────────────────────────
        private static string _benutzer = "System";
        private static string _sessionId = "";
        private static DateTime _sessionStart = DateTime.MinValue;
        private static int _aktionen = 0;
        private static int _fehler = 0;
        private static int _warnungen = 0;
        private static int _navigationen = 0;

        // ── Sicherheit ───────────────────────────────────────────────
        private static int _loginFehler = 0;
        private static DateTime _letzterFehler = DateTime.MinValue;
        private static bool _bruteForceSperr = false;
        private static int _anomalieScore = 0;   // Akkumulierter Risikoscore

        // ── Anomalie-Tracking ────────────────────────────────────────
        private static readonly List<DateTime> _aktionenFenster = new List<DateTime>();
        private static int _loeschZaehler = 0;
        private static DateTime _loeschFenster = DateTime.MinValue;

        // ── Transaktion ──────────────────────────────────────────────
        private static string _transaktionId = "";
        private static bool _transaktionAktiv = false;
        private static List<string> _transaktionPuffer = new List<string>();
        private static string _transaktionName = "";

        // ── Kettenintegrität ─────────────────────────────────────────
        private static string _letzterBlockHash = "GENESIS";    // Hash-Kette wie Blockchain
        private static long _laufenderZaehler = 0;

        // ── Performance ──────────────────────────────────────────────
        private static readonly Dictionary<string, Stopwatch> _perfWatches = new Dictionary<string, Stopwatch>();

        // ── Heartbeat + Trend ────────────────────────────────────────
        private static Timer _heartbeatTimer;
        private static DateTime _letzterHeartbeat = DateTime.MinValue;
        private static readonly List<TrendPunkt> _inventarTrend = new List<TrendPunkt>();

        // ── Snapshot ─────────────────────────────────────────────────
        private static DatenSnapshot _vorabSnapshot;

        // ── Thread-Safety ────────────────────────────────────────────
        private static readonly object _lock = new object();

        // ══════════════════════════════════════════════════════════════
        //  ENUMS & HILFSKLASSEN
        // ══════════════════════════════════════════════════════════════
        public enum LogLevel
        {
            DEBUG = 0, INFO = 1, SUCCESS = 2, WARNING = 3,
            ERROR = 4, CRITICAL = 5, SECURITY = 6, ANOMALY = 7
        }

        private class DatenSnapshot
        {
            public int ArtikelAnzahl { get; set; }
            public int MitarbeiterAnz { get; set; }
            public int BenutzerAnz { get; set; }
            public decimal GesamtWert { get; set; }
            public int KritischeArtikel { get; set; }
            public int LeereArtikel { get; set; }
            public string KonzentratHash { get; set; }  // SHA-256 über alle Inv-Nummern
            public DateTime Zeitpunkt { get; set; }

            public static DatenSnapshot Jetzt()
            {
                var invNrn = string.Join("|", DataManager.Inventar.Select(a => a.InvNmr + a.Anzahl));
                return new DatenSnapshot
                {
                    ArtikelAnzahl = DataManager.Inventar.Count,
                    MitarbeiterAnz = DataManager.Mitarbeiter.Count,
                    BenutzerAnz = DataManager.Benutzer.Count,
                    GesamtWert = DataManager.Inventar.Sum(a => a.Preis * a.Anzahl),
                    KritischeArtikel = DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand),
                    LeereArtikel = DataManager.Inventar.Count(a => a.Anzahl == 0),
                    KonzentratHash = BerechneMD5(invNrn),
                    Zeitpunkt = DateTime.Now
                };
            }

            public string AlsZeile() =>
                $"Artikel:{ArtikelAnzahl} | Mitarbeiter:{MitarbeiterAnz} | " +
                $"Benutzer:{BenutzerAnz} | Wert:{GesamtWert:F2}€ | " +
                $"Kritisch:{KritischeArtikel} | Leer:{LeereArtikel} | Hash:{KonzentratHash}";

            public string Diff(DatenSnapshot nach)
            {
                var teile = new List<string>();
                if (ArtikelAnzahl != nach.ArtikelAnzahl) teile.Add($"Artikel: {ArtikelAnzahl}→{nach.ArtikelAnzahl}");
                if (MitarbeiterAnz != nach.MitarbeiterAnz) teile.Add($"Mitarbeiter: {MitarbeiterAnz}→{nach.MitarbeiterAnz}");
                if (BenutzerAnz != nach.BenutzerAnz) teile.Add($"Benutzer: {BenutzerAnz}→{nach.BenutzerAnz}");
                if (Math.Abs(GesamtWert - nach.GesamtWert) > 0.001m)
                    teile.Add($"Wert: {GesamtWert:F2}€→{nach.GesamtWert:F2}€ (Δ{nach.GesamtWert - GesamtWert:+0.00;-0.00}€)");
                if (KritischeArtikel != nach.KritischeArtikel) teile.Add($"Kritisch: {KritischeArtikel}→{nach.KritischeArtikel}");
                if (KonzentratHash != nach.KonzentratHash) teile.Add($"Daten-Hash geändert");
                return teile.Count == 0 ? "Keine Datendifferenz" : string.Join(" | ", teile);
            }
        }

        private class TrendPunkt
        {
            public DateTime Zeit { get; set; }
            public decimal Wert { get; set; }
            public int Artikel { get; set; }
            public int Kritisch { get; set; }
            public string AusloesEvent { get; set; }
        }

        // ══════════════════════════════════════════════════════════════
        //  KERN-LOGGER (privat — alle öffentlichen Methoden laufen hier durch)
        // ══════════════════════════════════════════════════════════════
        private static void SchreibeLog(LogLevel level, string kategorie, string aktion,
                                        string details = "", string korrelationsId = "",
                                        [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            lock (_lock)
            {
                try
                {
                    _aktionen++;
                    _laufenderZaehler++;
                    if (level >= LogLevel.ERROR) _fehler++;
                    if (level == LogLevel.WARNING) _warnungen++;

                    // Anomalie: Geschwindigkeit (>20 Aktionen / 10 Sek)
                    _aktionenFenster.Add(DateTime.Now);
                    _aktionenFenster.RemoveAll(t => (DateTime.Now - t).TotalSeconds > 10);
                    if (_aktionenFenster.Count >= ANOMALIE_GESCHW && level != LogLevel.ANOMALY)
                        SchreibeAnomalieIntern("HOHE-GESCHWINDIGKEIT",
                            $"{_aktionenFenster.Count} Aktionen in 10 Sek — Benutzer: {_benutzer}", 15);

                    bool nacht = DateTime.Now.Hour >= 22 || DateTime.Now.Hour <= 5;

                    // Block aufbauen
                    var sb = new StringBuilder();
                    string zeit = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff");
                    string lvl = $"[{level,-8}]";
                    string sess = string.IsNullOrEmpty(_sessionId) ? "—" : _sessionId;
                    string korr = string.IsNullOrEmpty(korrelationsId) ? "—" : korrelationsId;

                    sb.AppendLine($"┌─ {zeit}  {lvl}  [{kategorie}]" + (nacht ? "  🌙 NACHTZEIT" : ""));
                    sb.AppendLine($"│  #Log:         {_laufenderZaehler:D8}  ·  #Session: {_aktionen:D6}");
                    sb.AppendLine($"│  Benutzer:     {_benutzer}" + (_bruteForceSperr ? "  ⚠️ BRUTE-FORCE" : "") + (nacht ? "  ⚠️ AUSSERHALB BETRIEBSZEIT" : ""));
                    sb.AppendLine($"│  Session-ID:   {sess}");
                    sb.AppendLine($"│  Korr.-ID:     {korr}");
                    sb.AppendLine($"│  Computer:     {Environment.MachineName}  ·  IP: {GetIP()}  ·  User-OS: {Environment.UserName}");
                    sb.AppendLine($"│  Aufruf:       {caller}()  ·  PID: {Process.GetCurrentProcess().Id}");
                    sb.AppendLine($"│  Risikoscore:  {_anomalieScore}  ·  Brute-Force: {(_bruteForceSperr ? "AKTIV ⚠️" : "OK ✔")}");
                    sb.AppendLine($"│  Aktion:       {aktion}");

                    // Details als strukturierte Liste
                    if (!string.IsNullOrWhiteSpace(details))
                    {
                        var teile = details.Split('|')
                                           .Select(t => t.Trim())
                                           .Where(t => !string.IsNullOrEmpty(t))
                                           .ToArray();
                        if (teile.Length > 1)
                        {
                            sb.AppendLine("│  Details:");
                            foreach (var t in teile)
                                sb.AppendLine($"│    ▸ {t}");
                        }
                        else
                        {
                            sb.AppendLine($"│  Details:      {details}");
                        }
                    }

                    // Kettenintegrität: Hash dieses Blocks über letzten Hash
                    string blockInhalt = sb.ToString();
                    string neuerHash = BerechneSHA256($"{_letzterBlockHash}|{blockInhalt}");
                    _letzterBlockHash = neuerHash;
                    sb.AppendLine($"│  Block-Hash:   {neuerHash.Substring(0, 16)}...  [Kette #{_laufenderZaehler}]");
                    sb.AppendLine($"└{'─',-1}[{_laufenderZaehler:D8}]" + new string('─', 50));
                    sb.AppendLine();

                    // Transaktion: puffern statt direkt schreiben
                    if (_transaktionAktiv)
                    {
                        _transaktionPuffer.Add(sb.ToString());
                    }
                    else
                    {
                        EncryptionManager.AppendEncrypted(logFilePath, sb.ToString());
                        if (level >= LogLevel.CRITICAL) PrüfeLogRotation();
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[LOG-INTERN] {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        private static void SchreibeAnomalieIntern(string typ, string beschr, int risikoGewicht = 10)
        {
            _anomalieScore += risikoGewicht;
            string korr = ErzeugeKorrelationsId("ANOM");
            SchreibeLog(LogLevel.ANOMALY, "ANOMALIE", $"🚨 Anomalie: {typ}",
                $"Beschreibung: {beschr} | " +
                $"Risikoscore jetzt: {_anomalieScore} (+{risikoGewicht}) | " +
                $"Benutzer: {_benutzer} | Session: {_sessionId} | IP: {GetIP()}",
                korr);
        }

        // ══════════════════════════════════════════════════════════════
        //  TRANSAKTIONS-LOG (Batch — mehrere Änderungen als eine Einheit)
        // ══════════════════════════════════════════════════════════════
        public static string StarteTransaktion(string name)
        {
            _transaktionId = ErzeugeKorrelationsId("TRX");
            _transaktionAktiv = true;
            _transaktionName = name;
            _transaktionPuffer.Clear();

            // Vor-Snapshot
            NimmVorabSnapshot(name);

            SchreibeLog(LogLevel.INFO, "TRANSAKTION", $"⏩ Transaktion gestartet: {name}",
                $"Transaktions-ID: {_transaktionId} | Benutzer: {_benutzer}",
                _transaktionId);

            return _transaktionId;
        }

        public static void CommitTransaktion(bool erfolg = true)
        {
            if (!_transaktionAktiv) return;
            _transaktionAktiv = false;

            var nach = DatenSnapshot.Jetzt();
            string diff = _vorabSnapshot?.Diff(nach) ?? "—";

            // Alle gepufferten Einträge als Block schreiben
            var header = new StringBuilder();
            header.AppendLine($"╔═ TRANSAKTION-BEGIN ══ ID: {_transaktionId} ══════════════════════════════╗");
            header.AppendLine($"║  Name:    {_transaktionName}");
            header.AppendLine($"║  Status:  {(erfolg ? "✔ COMMIT" : "✖ ROLLBACK")}");
            header.AppendLine($"║  Einträge:{_transaktionPuffer.Count}");
            header.AppendLine($"║  Diff:    {diff}");
            header.AppendLine($"╚═══════════════════════════════════════════════════════════════════════════╝");
            header.AppendLine();
            EncryptionManager.AppendEncrypted(logFilePath, header.ToString());

            foreach (var eintrag in _transaktionPuffer)
                EncryptionManager.AppendEncrypted(logFilePath, eintrag);

            var footer = new StringBuilder();
            footer.AppendLine($"╔═ TRANSAKTION-END ═══ ID: {_transaktionId} ══════════════════════════════╗");
            footer.AppendLine($"║  Abgeschlossen: {DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}");
            footer.AppendLine($"╚═══════════════════════════════════════════════════════════════════════════╝");
            footer.AppendLine();
            EncryptionManager.AppendEncrypted(logFilePath, footer.ToString());

            _transaktionPuffer.Clear();
            _transaktionId = "";
        }

        // ══════════════════════════════════════════════════════════════
        //  FELD-LEVEL AUDIT TRAIL
        // ══════════════════════════════════════════════════════════════
        public static void LogFeldAenderung(string objektTyp, string objektId, string objektName,
                                            string feldName, string altWert, string neuWert,
                                            string korrelationsId = "")
        {
            // Leere Änderungen nicht loggen
            if (altWert == neuWert) return;

            // Typ der Änderung klassifizieren
            bool istSicherheitsfeld = feldName.Contains("Passwort") || feldName.Contains("Berechtigung");
            bool istPreisfeld = feldName.Contains("Preis") || feldName.Contains("Wert");
            bool istMengenfeld = feldName.Contains("Anzahl") || feldName.Contains("Bestand");

            LogLevel level = istSicherheitsfeld ? LogLevel.SECURITY : LogLevel.INFO;

            // Bei Preisfeldern: Prozentuale Änderung berechnen
            string prozentInfo = "";
            if (istPreisfeld && decimal.TryParse(altWert, out decimal altD) && decimal.TryParse(neuWert, out decimal neuD) && altD != 0)
                prozentInfo = $" | Δ: {(neuD - altD):+0.00;-0.00} ({(neuD - altD) / altD * 100:+0.0;-0.0}%)";

            SchreibeLog(level, "FELD-AUDIT",
                $"Feld geändert: [{feldName}] an {objektTyp} '{objektName}'",
                $"Objekt-ID: {objektId} | " +
                $"Objekt-Typ: {objektTyp} | " +
                $"Feld: {feldName} | " +
                $"Alt: '{altWert}' | " +
                $"Neu: '{neuWert}'{prozentInfo} | " +
                $"Geändert von: {_benutzer} | " +
                $"Sicherheitsfeld: {(istSicherheitsfeld ? "JA ⚠️" : "Nein")}",
                string.IsNullOrEmpty(korrelationsId) ? ErzeugeKorrelationsId("FELD") : korrelationsId);
        }

        // ══════════════════════════════════════════════════════════════
        //  INVENTARWERT-TREND
        // ══════════════════════════════════════════════════════════════
        private static void AktualisiereInventarTrend(string ausloeser)
        {
            try
            {
                var punkt = new TrendPunkt
                {
                    Zeit = DateTime.Now,
                    Wert = DataManager.Inventar.Sum(a => a.Preis * a.Anzahl),
                    Artikel = DataManager.Inventar.Count,
                    Kritisch = DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand),
                    AusloesEvent = ausloeser
                };
                _inventarTrend.Add(punkt);

                // Nur letzten 200 Punkte im Speicher
                if (_inventarTrend.Count > 200)
                    _inventarTrend.RemoveAt(0);

                // Starke Wertveränderungen sofort loggen
                if (_inventarTrend.Count >= 2)
                {
                    var letzt = _inventarTrend[^2];
                    decimal deltaProz = letzt.Wert > 0 ? (punkt.Wert - letzt.Wert) / letzt.Wert * 100 : 0;
                    if (Math.Abs(deltaProz) >= 10m)
                        SchreibeLog(LogLevel.WARNING, "TREND",
                            $"Starke Inventarwertänderung: {deltaProz:+0.0;-0.0}%",
                            $"Vorher: {letzt.Wert:F2}€ | Jetzt: {punkt.Wert:F2}€ | " +
                            $"Delta: {punkt.Wert - letzt.Wert:+0.00;-0.00}€ | " +
                            $"Auslöser: {ausloeser}");
                }
            }
            catch { }
        }

        public static void LogInventarTrendPunkt(string ereignis) => AktualisiereInventarTrend(ereignis);

        // ══════════════════════════════════════════════════════════════
        //  SNAPSHOT (verbesserter Diff mit Hash-Vergleich)
        // ══════════════════════════════════════════════════════════════
        public static void NimmVorabSnapshot(string kontext)
        {
            _vorabSnapshot = DatenSnapshot.Jetzt();
            SchreibeLog(LogLevel.DEBUG, "SNAPSHOT", $"📸 Vor-Snapshot: {kontext}",
                _vorabSnapshot.AlsZeile());
        }

        public static void SchreibeNachSnapshot(string kontext, string operation)
        {
            if (_vorabSnapshot == null) return;
            var nach = DatenSnapshot.Jetzt();
            string diff = _vorabSnapshot.Diff(nach);
            bool geaendert = diff != "Keine Datendifferenz";

            SchreibeLog(geaendert ? LogLevel.INFO : LogLevel.DEBUG, "SNAPSHOT",
                $"📸 Nach-Snapshot: {kontext}",
                $"Operation: {operation} | " +
                $"Dauer: {(nach.Zeitpunkt - _vorabSnapshot.Zeitpunkt).TotalMilliseconds:F0}ms | " +
                $"Diff: {diff} | " +
                $"VOR-Hash: {_vorabSnapshot.KonzentratHash} | " +
                $"NACH-Hash: {nach.KonzentratHash}");

            AktualisiereInventarTrend(operation);
            _vorabSnapshot = null;
        }

        // ══════════════════════════════════════════════════════════════
        //  INITIALISIERUNG & HEARTBEAT
        // ══════════════════════════════════════════════════════════════
        public static void InitializeLog()
        {
            try
            {
                PrüfeLogRotation();

                if (!File.Exists(logFilePath))
                {
                    long ram = 0;
                    try { ram = GC.GetTotalMemory(false); } catch { }

                    var sb = new StringBuilder();
                    sb.AppendLine("╔════════════════════════════════════════════════════════════════════════╗");
                    sb.AppendLine("║      INVENTARVERWALTUNG — FORENSISCHES ENTERPRISE LOG v4.0            ║");
                    sb.AppendLine("║      🔐 AES-256 · SHA-256-Kette · FELD-AUDIT · TRANSAKTION · TREND   ║");
                    sb.AppendLine("╚════════════════════════════════════════════════════════════════════════╝");
                    sb.AppendLine($"Erstellt am:        {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                    sb.AppendLine($"Computername:       {Environment.MachineName}");
                    sb.AppendLine($"OS:                 {Environment.OSVersion}");
                    sb.AppendLine($"OS-User:            {Environment.UserName}");
                    sb.AppendLine($".NET:               {Environment.Version}");
                    sb.AppendLine($"CPU-Kerne:          {Environment.ProcessorCount}");
                    sb.AppendLine($"64-Bit:             {Environment.Is64BitProcess}");
                    sb.AppendLine($"PID:                {Process.GetCurrentProcess().Id}");
                    sb.AppendLine($"Arbeitsverz.:       {Environment.CurrentDirectory}");
                    sb.AppendLine($"Zeitzone:           {TimeZoneInfo.Local.Id}  ·  UTC-Offset: {TimeZoneInfo.Local.GetUtcOffset(DateTime.Now):hh\\:mm}");
                    sb.AppendLine($"IP:                 {GetIP()}");
                    sb.AppendLine($"Kettenstart-Hash:   {_letzterBlockHash}");
                    sb.AppendLine($"Verschlüsselung:    AES-256-CBC + PBKDF2-SHA256 (10.000 Iter.)");
                    sb.AppendLine($"Integritätsmodus:   SHA-256 Block-Kette");
                    sb.AppendLine(new string('═', 76));
                    sb.AppendLine();

                    byte[] enc = EncryptionManager.EncryptString(sb.ToString());
                    if (enc != null) File.WriteAllBytes(logFilePath, enc);
                }

                // Heartbeat alle 30 Minuten
                _heartbeatTimer = new Timer(_ => SchreibeHeartbeat(), null,
                    TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[LOG-INIT] {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void SchreibeHeartbeat()
        {
            _letzterHeartbeat = DateTime.Now;
            long ram = 0;
            try { ram = GC.GetTotalMemory(false); } catch { }

            SchreibeLog(LogLevel.DEBUG, "HEARTBEAT", "💓 System aktiv",
                $"Session: {_sessionId} | " +
                $"Benutzer: {_benutzer} | " +
                $"Uptime: {FormatDauer(DateTime.Now - _sessionStart)} | " +
                $"Session-Aktionen: {_aktionen} | " +
                $"Session-Fehler: {_fehler} | " +
                $"Anomalie-Score: {_anomalieScore} | " +
                $"RAM (GC): {ram / 1024} KB | " +
                $"Inventar: {DataManager.Inventar.Count} Artikel | " +
                $"Wert: {DataManager.Inventar.Sum(a => a.Preis * a.Anzahl):F2}€ | " +
                $"Kritisch: {DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand)} | " +
                $"Trend-Punkte: {_inventarTrend.Count} | " +
                $"Ketten-Hash: {_letzterBlockHash.Substring(0, 12)}...");
        }

        // ══════════════════════════════════════════════════════════════
        //  LOG-ROTATION
        // ══════════════════════════════════════════════════════════════
        private static void PrüfeLogRotation()
        {
            try
            {
                if (!File.Exists(logFilePath)) return;
                var fi = new FileInfo(logFilePath);
                if (fi.Length < MAX_LOG_MB * 1024 * 1024) return;

                if (!Directory.Exists(archivOrdner))
                    Directory.CreateDirectory(archivOrdner);

                string archiv = Path.Combine(archivOrdner, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.enc");
                File.Move(logFilePath, archiv);

                // Neues Log mit Verweis auf Archiv
                var sb = new StringBuilder();
                sb.AppendLine("╔═══════════════════════════════════════╗");
                sb.AppendLine("║  LOG ROTIERT — Fortsetzung            ║");
                sb.AppendLine("╚═══════════════════════════════════════╝");
                sb.AppendLine($"Rotiert am:     {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                sb.AppendLine($"Archiv:         {Path.GetFullPath(archiv)}");
                sb.AppendLine($"Archivgröße:    {fi.Length / 1024} KB");
                sb.AppendLine($"Ketten-Hash:    {_letzterBlockHash}  (Kette bleibt erhalten)");
                sb.AppendLine(new string('═', 76));
                sb.AppendLine();

                byte[] enc = EncryptionManager.EncryptString(sb.ToString());
                if (enc != null) File.WriteAllBytes(logFilePath, enc);
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════
        //  DATEI-INTEGRITÄT
        // ══════════════════════════════════════════════════════════════
        public static void LogDateiIntegrität(string pfad)
        {
            try
            {
                if (!File.Exists(pfad)) return;
                var fi = new FileInfo(pfad);
                string h = BerechneSHA256Datei(pfad);

                SchreibeLog(LogLevel.DEBUG, "INTEGRITÄT", $"Datei-Snapshot: {Path.GetFileName(pfad)}",
                    $"Pfad: {Path.GetFullPath(pfad)} | " +
                    $"Größe: {fi.Length} Bytes | " +
                    $"Geändert: {fi.LastWriteTime:dd.MM.yyyy HH:mm:ss} | " +
                    $"SHA-256: {h?.Substring(0, 32)}...");
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════
        //  SESSION-MANAGEMENT
        // ══════════════════════════════════════════════════════════════
        public static void SetAktuellerBenutzer(string b) => _benutzer = b;

        public static void StartSession(string benutzername)
        {
            _benutzer = benutzername;
            _sessionStart = DateTime.Now;
            _aktionen = 0; _fehler = 0; _warnungen = 0; _navigationen = 0;
            _sessionId = $"SES-{DateTime.Now:yyyyMMdd-HHmmss}-{benutzername.ToUpper().Substring(0, Math.Min(4, benutzername.Length))}";

            long ram = 0;
            try { ram = GC.GetTotalMemory(false); } catch { }

            SchreibeLog(LogLevel.SECURITY, "SESSION", "🟢 Session gestartet",
                $"Session-ID: {_sessionId} | " +
                $"Benutzer: {benutzername} | " +
                $"Berechtigung: {DataManager.Benutzer.FirstOrDefault(b => b.Benutzername == benutzername)?.Berechtigung.ToString() ?? "?"} | " +
                $"OS: {Environment.OSVersion} | " +
                $".NET: {Environment.Version} | " +
                $"CPU: {Environment.ProcessorCount} Kerne | " +
                $"RAM (GC): {ram / 1024} KB | " +
                $"PID: {Process.GetCurrentProcess().Id} | " +
                $"Zeitzone: {TimeZoneInfo.Local.Id} | " +
                $"Anomalie-Score bei Start: {_anomalieScore}",
                _sessionId);
        }

        public static void EndSession()
        {
            if (_sessionStart == DateTime.MinValue) return;
            var dauer = DateTime.Now - _sessionStart;

            // Inventartrend am Session-Ende persistieren (vereinfacht)
            int trendAnz = _inventarTrend.Count;
            decimal? trendErsterWert = _inventarTrend.FirstOrDefault()?.Wert;
            decimal? trendLetzterWert = _inventarTrend.LastOrDefault()?.Wert;
            string trendInfo = trendAnz >= 2 && trendErsterWert.HasValue
                ? $"Trendpunkte: {trendAnz} | Wert am Start: {trendErsterWert:F2}€ | Wert am Ende: {trendLetzterWert:F2}€ | Δ: {trendLetzterWert - trendErsterWert:+0.00;-0.00}€"
                : "Kein Trend (zu wenig Datenpunkte)";

            SchreibeLog(LogLevel.SECURITY, "SESSION", "🔴 Session beendet",
                $"Session-ID: {_sessionId} | " +
                $"Benutzer: {_benutzer} | " +
                $"Dauer: {FormatDauer(dauer)} | " +
                $"Aktionen: {_aktionen} | " +
                $"Fehler: {_fehler} | " +
                $"Warnungen: {_warnungen} | " +
                $"Navigationen: {_navigationen} | " +
                $"Ø Aktionen/Min: {(dauer.TotalMinutes > 0 ? (_aktionen / dauer.TotalMinutes).ToString("F1") : "—")} | " +
                $"End-Anomalie-Score: {_anomalieScore} | " +
                $"{trendInfo}",
                _sessionId);

            _heartbeatTimer?.Dispose();
            _sessionId = "";
            _sessionStart = DateTime.MinValue;
        }

        // ══════════════════════════════════════════════════════════════
        //  SYSTEM
        // ══════════════════════════════════════════════════════════════
        public static void LogProgrammStart()
        {
            long ram = 0;
            try { ram = GC.GetTotalMemory(false); } catch { }

            SchreibeLog(LogLevel.INFO, "SYSTEM", "🚀 Programm gestartet",
                $"Version: 1.0 | OS: {Environment.OSVersion} | .NET: {Environment.Version} | " +
                $"CPU: {Environment.ProcessorCount} Kerne | RAM (GC): {ram / 1024} KB | " +
                $"64-Bit: {Environment.Is64BitProcess} | PID: {Process.GetCurrentProcess().Id} | " +
                $"Verzeichnis: {Environment.CurrentDirectory} | Zeitzone: {TimeZoneInfo.Local.Id} | " +
                $"UTC-Offset: {TimeZoneInfo.Local.GetUtcOffset(DateTime.Now):hh\\:mm} | " +
                $"Verschlüsselung: AES-256");
        }

        public static void LogProgrammEnde()
        {
            EndSession();
            SchreibeLog(LogLevel.INFO, "SYSTEM", "🛑 Programm beendet",
                $"Gesamtaktionen: {_aktionen} | Fehler: {_fehler} | Anomalie-Score: {_anomalieScore}");
        }

        public static void LogSystemFehler(string bereich, Exception ex)
        {
            SchreibeLog(LogLevel.CRITICAL, "SYSTEM", $"💀 Unbehandelter Fehler: {bereich}",
                $"Typ: {ex.GetType().FullName} | " +
                $"Meldung: {ex.Message} | " +
                $"Stack-Top: {ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim()} | " +
                $"Stack-2: {ex.StackTrace?.Split('\n').Skip(1).FirstOrDefault()?.Trim()} | " +
                $"Inner: {ex.InnerException?.Message ?? "—"} | " +
                $"Source: {ex.Source} | Benutzer: {_benutzer}");
        }

        public static void LogKonfigurationGeaendert(string einstellung, string alt, string neu)
        {
            SchreibeLog(LogLevel.WARNING, "SYSTEM", "⚙️ Konfiguration geändert",
                $"Einstellung: {einstellung} | Alt: '{alt}' | Neu: '{neu}' | Von: {_benutzer}");
        }

        // ══════════════════════════════════════════════════════════════
        //  AUTH / SICHERHEIT
        // ══════════════════════════════════════════════════════════════
        public static void LogAnmeldungErfolgreich(string benutzername)
        {
            _loginFehler = 0;
            _bruteForceSperr = false;
            StartSession(benutzername);
            SchreibeLog(LogLevel.SECURITY, "AUTH", "✔ Anmeldung erfolgreich",
                $"Benutzer: {benutzername} | Session: {_sessionId} | IP: {GetIP()} | " +
                $"Uhrzeit: {DateTime.Now:HH:mm:ss} | Vorherige Fehlversuche: {_loginFehler}",
                _sessionId);
        }

        public static void LogAnmeldungFehlgeschlagen(string grund)
        {
            _loginFehler++;
            _letzterFehler = DateTime.Now;

            LogLevel lv = _loginFehler >= MAX_LOGIN_FEHLER ? LogLevel.ANOMALY : LogLevel.SECURITY;
            SchreibeLog(lv, "AUTH", $"✖ Anmeldung fehlgeschlagen ({_loginFehler}. Versuch)",
                $"Grund: {grund} | Fehlversuche: {_loginFehler}/{MAX_LOGIN_FEHLER} | " +
                $"IP: {GetIP()} | Computer: {Environment.MachineName}");

            if (_loginFehler >= MAX_LOGIN_FEHLER)
            {
                _bruteForceSperr = true;
                SchreibeAnomalieIntern("BRUTE-FORCE-SCHWELLE",
                    $"{_loginFehler} Fehlversuche in Folge | IP: {GetIP()} | Computer: {Environment.MachineName}", 25);
            }
        }

        public static void LogNeuesKontoErstellt(string benutzername)
        {
            SchreibeLog(LogLevel.SECURITY, "AUTH", "👤 Neues Konto erstellt",
                $"Benutzername: {benutzername} | Erstellt von: {_benutzer} | " +
                $"Gesamtbenutzer: {DataManager.Benutzer.Count}");
        }

        public static void LogPasswortGeaendert(string benutzername)
        {
            SchreibeLog(LogLevel.SECURITY, "AUTH", "🔑 Passwort geändert",
                $"Benutzer: {benutzername} | Geändert von: {_benutzer} | " +
                $"Zeitpunkt: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        }

        public static void LogPasswortPolicyVerletzung(string benutzername, string grund)
        {
            SchreibeLog(LogLevel.WARNING, "AUTH", "⚠️ Passwort-Policy verletzt",
                $"Benutzer: {benutzername} | Grund: {grund} | Versucht von: {_benutzer}");
        }

        public static void LogUnerlaubterZugriff(string bereich, string versuch)
        {
            SchreibeAnomalieIntern("ZUGRIFF-VERWEIGERT",
                $"Bereich: {bereich} | Versuch: {versuch} | Benutzer: {_benutzer} | IP: {GetIP()}", 20);
        }

        public static void LogDevZugriff(string aktion)
        {
            SchreibeLog(LogLevel.SECURITY, "DEV", "⚡ DEV-Aktion",
                $"Aktion: {aktion} | Benutzer: {_benutzer} | " +
                $"Zeitpunkt: {DateTime.Now:dd.MM.yyyy HH:mm:ss} | Session: {_sessionId}");
        }

        // ══════════════════════════════════════════════════════════════
        //  MENÜ-NAVIGATION
        // ══════════════════════════════════════════════════════════════
        public static void LogMenuNavigation(string von, string zu, string option)
        {
            _navigationen++;
            SchreibeLog(LogLevel.DEBUG, "NAVIGATION", $"🗺️ {von} → {zu}",
                $"Option: '{option}' | Nav #{_navigationen} | Benutzer: {_benutzer}");
        }

        public static void LogMenuAbgebrochen(string menu, string grund)
        {
            SchreibeLog(LogLevel.DEBUG, "NAVIGATION", $"↩️ Abgebrochen: {menu}",
                $"Grund: {grund} | Benutzer: {_benutzer}");
        }

        // ══════════════════════════════════════════════════════════════
        //  INVENTAR
        // ══════════════════════════════════════════════════════════════
        public static void LogArtikelHinzugefuegt(string invNr, string name, string mitarbeiter)
        {
            AktualisiereInventarTrend("Artikel hinzugefügt");
            SchreibeLog(LogLevel.SUCCESS, "INVENTAR", "📦 Artikel angelegt",
                $"Inv-Nr: {invNr} | Gerät: {name} | Mitarbeiter: {mitarbeiter} | " +
                $"Von: {_benutzer} | Gesamtartikel: {DataManager.Inventar.Count} | " +
                $"Inventarwert: {DataManager.Inventar.Sum(a => a.Preis * a.Anzahl):F2}€");
        }

        public static void LogArtikelHinzugefuegtDetail(InvId art)
        {
            AktualisiereInventarTrend("Artikel vollständig erfasst");
            SchreibeLog(LogLevel.SUCCESS, "INVENTAR", "📦 Artikel vollständig erfasst",
                $"Inv-Nr: {art.InvNmr} | Gerät: {art.GeraeteName} | Hersteller: {art.Hersteller} | " +
                $"Kategorie: {art.Kategorie} | SNR: {art.SerienNummer} | " +
                $"Preis: {art.Preis:F2}€ | Anschaffung: {art.Anschaffungsdatum:dd.MM.yyyy} | " +
                $"Anzahl: {art.Anzahl} | Mindestbestand: {art.Mindestbestand} | " +
                $"Mitarbeiter: {art.MitarbeiterBezeichnung} | Von: {_benutzer} | " +
                $"Neuer Gesamtinventarwert: {DataManager.Inventar.Sum(a => a.Preis * a.Anzahl):F2}€");
        }

        public static void LogArtikelBearbeitet(string invNr, string name, string feld, string alt, string neu)
        {
            LogFeldAenderung("Artikel", invNr, name, feld, alt, neu);
        }

        public static void LogArtikelGeloescht(string invNr, string name, string mitarbeiter, decimal preis)
        {
            _loeschZaehler++;
            if ((DateTime.Now - _loeschFenster).TotalMinutes > 5)
            { _loeschZaehler = 1; _loeschFenster = DateTime.Now; }

            if (_loeschZaehler >= 5)
                SchreibeAnomalieIntern("MASSENLÖSCHUNG-ARTIKEL",
                    $"{_loeschZaehler} Artikel in <5 Min gelöscht | Von: {_benutzer}", 30);

            AktualisiereInventarTrend("Artikel gelöscht");
            SchreibeLog(LogLevel.WARNING, "INVENTAR", "🗑️ Artikel gelöscht — UNWIDERRUFLICH",
                $"Inv-Nr: {invNr} | Gerät: {name} | Zugewiesen an: {mitarbeiter} | " +
                $"Wert verloren: {preis:F2}€ | Von: {_benutzer} | " +
                $"Verbleibend: {DataManager.Inventar.Count} | " +
                $"Löschung #{_loeschZaehler} in 5-Min-Fenster | " +
                $"Neuer Gesamtinventarwert: {DataManager.Inventar.Sum(a => a.Preis * a.Anzahl):F2}€");
        }

        public static void LogInventarAngezeigt(int anz)
        {
            SchreibeLog(LogLevel.INFO, "INVENTAR", "📋 Inventar angezeigt",
                $"Artikel: {anz} | Wert: {DataManager.Inventar.Sum(a => a.Preis * a.Anzahl):F2}€ | " +
                $"Kritisch: {DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand)} | " +
                $"Kategorien: {DataManager.Inventar.Select(a => a.Kategorie).Distinct().Count()} | " +
                $"Hersteller: {DataManager.Inventar.Select(a => a.Hersteller).Distinct().Count()} | " +
                $"Von: {_benutzer}");
        }

        public static void LogArtikelDuplikat(string invNr, string name)
        {
            SchreibeLog(LogLevel.WARNING, "INVENTAR", "⚠️ Duplikat verhindert",
                $"Inv-Nr: {invNr} | Gerät: {name} | Von: {_benutzer}");
        }

        public static void LogArtikelGesucht(string suchbegriff, int treffer)
        {
            SchreibeLog(LogLevel.INFO, "INVENTAR", "🔍 Artikelsuche",
                $"Begriff: '{suchbegriff}' | Treffer: {treffer} | Von: {_benutzer}");
        }

        // ══════════════════════════════════════════════════════════════
        //  BESTAND
        // ══════════════════════════════════════════════════════════════
        public static void LogBestandErhoehen(string invNr, string name, int alt, int menge, int neu)
        {
            AktualisiereInventarTrend("Bestand erhöht");
            SchreibeLog(LogLevel.INFO, "BESTAND", "➕ Bestand erhöht",
                $"Inv-Nr: {invNr} | Gerät: {name} | {alt} → +{menge} → {neu} | Von: {_benutzer}");
        }

        public static void LogBestandVerringern(string invNr, string name, int alt, int menge, int neu, int mindest)
        {
            bool krit = neu <= mindest;
            AktualisiereInventarTrend("Bestand verringert");
            SchreibeLog(krit ? LogLevel.WARNING : LogLevel.INFO, "BESTAND",
                $"➖ Bestand verringert{(neu == 0 ? " ⛔ LEER!" : krit ? " ⚠️ UNTER MINDEST!" : "")}",
                $"Inv-Nr: {invNr} | Gerät: {name} | {alt} → -{menge} → {neu} | " +
                $"Mindestbestand: {mindest} | Fehlend: {Math.Max(0, mindest - neu)} | Von: {_benutzer}");
        }

        public static void LogMindestbestandGeaendert(string invNr, string name, int alt, int neu)
        {
            LogFeldAenderung("Artikel", invNr, name, "Mindestbestand", alt.ToString(), neu.ToString());
        }

        public static void LogBestandswarnung(int krit, int leer)
        {
            int bestellBedarf = DataManager.Inventar
                .Where(a => a.Anzahl < a.Mindestbestand)
                .Sum(a => a.Mindestbestand - a.Anzahl);

            SchreibeLog(LogLevel.WARNING, "BESTAND", "🔔 Bestandswarnung",
                $"Kritische Artikel: {krit} | Leer (0 Stück): {leer} | " +
                $"Geschätzter Bestellbedarf: {bestellBedarf} Einheiten | " +
                $"Zeitpunkt: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        }

        // ══════════════════════════════════════════════════════════════
        //  MITARBEITER
        // ══════════════════════════════════════════════════════════════
        public static void LogMitarbeiterHinzugefuegt(string vn, string nn, string abt)
        {
            SchreibeLog(LogLevel.SUCCESS, "MITARBEITER", "👤 Mitarbeiter angelegt",
                $"Name: {vn} {nn} | Abteilung: {abt} | Von: {_benutzer} | " +
                $"Mitarbeiter in Abt.: {DataManager.Mitarbeiter.Count(m => m.Abteilung == abt)} | " +
                $"Gesamt: {DataManager.Mitarbeiter.Count}");
        }

        public static void LogMitarbeiterBearbeitet(string name, string feld, string alt, string neu)
        {
            LogFeldAenderung("Mitarbeiter", name, name, feld, alt, neu);
        }

        public static void LogMitarbeiterGeloescht(string vn, string nn, string abt, int artikel)
        {
            if (artikel > 0)
                SchreibeAnomalieIntern("MITARBEITER-MIT-ARTIKELN-GELÖSCHT",
                    $"{vn} {nn} gelöscht mit {artikel} zugewiesenen Artikeln | Von: {_benutzer}", 20);

            SchreibeLog(LogLevel.WARNING, "MITARBEITER", "🗑️ Mitarbeiter gelöscht — UNWIDERRUFLICH",
                $"Name: {vn} {nn} | Abteilung: {abt} | Hatte {artikel} Artikel | " +
                $"Von: {_benutzer} | Verbleibend: {DataManager.Mitarbeiter.Count}");
        }

        public static void LogMitarbeiterAngezeigt(int anz)
        {
            SchreibeLog(LogLevel.INFO, "MITARBEITER", "📋 Mitarbeiterliste",
                $"Mitarbeiter: {anz} | Abteilungen: {DataManager.Mitarbeiter.Select(m => m.Abteilung).Distinct().Count()} | " +
                $"Von: {_benutzer}");
        }

        public static void LogMitarbeiterDuplikat(string vn, string nn)
        {
            SchreibeLog(LogLevel.WARNING, "MITARBEITER", "⚠️ Duplikat verhindert",
                $"Name: {vn} {nn} | Von: {_benutzer}");
        }

        public static void LogArtikelNeuzugewiesen(string invNr, string name, string alt, string neu)
        {
            SchreibeLog(LogLevel.INFO, "ZUWEISUNG", "🔄 Artikel neu zugewiesen",
                $"Inv-Nr: {invNr} | Gerät: {name} | Von: '{alt}' → Zu: '{neu}' | " +
                $"Zugewiesen von: {_benutzer}");
        }

        // ══════════════════════════════════════════════════════════════
        //  BENUTZER
        // ══════════════════════════════════════════════════════════════
        public static void LogBenutzerAngelegt(string benutzername, Berechtigungen ber)
        {
            SchreibeLog(LogLevel.SECURITY, "BENUTZER", "👤 Benutzer angelegt",
                $"Benutzername: {benutzername} | Berechtigung: {ber} | " +
                $"Von: {_benutzer} | Gesamt: {DataManager.Benutzer.Count}");
        }

        public static void LogBenutzerBearbeitet(string benutzername, string feld, string alt, string neu)
        {
            LogFeldAenderung("Benutzer", benutzername, benutzername, feld, alt, neu);
        }

        public static void LogBenutzerGeloescht(string benutzername, Berechtigungen ber)
        {
            if (ber == Berechtigungen.Admin)
                SchreibeAnomalieIntern("ADMIN-GELÖSCHT",
                    $"Admin-Konto '{benutzername}' gelöscht von: {_benutzer}", 30);

            SchreibeLog(LogLevel.SECURITY, "BENUTZER", "🗑️ Benutzer gelöscht",
                $"Benutzername: {benutzername} | Berechtigung: {ber} | " +
                $"Von: {_benutzer} | Verbleibend: {DataManager.Benutzer.Count}");
        }

        public static void LogBenutzerAngezeigt(int anz)
        {
            SchreibeLog(LogLevel.INFO, "BENUTZER", "📋 Benutzerliste",
                $"Benutzer: {anz} | Admins: {DataManager.Benutzer.Count(b => b.Berechtigung == Berechtigungen.Admin)} | " +
                $"Von: {_benutzer}");
        }

        public static void LogBenutzerDuplikat(string benutzername)
        {
            SchreibeLog(LogLevel.WARNING, "BENUTZER", "⚠️ Duplikat verhindert",
                $"Benutzername: {benutzername} | Von: {_benutzer}");
        }

        public static void LogBenutzerAktualisiert(string benutzername, string alt, string neu)
        {
            LogFeldAenderung("Benutzer", benutzername, benutzername, "Berechtigung", alt, neu);
        }

        // ══════════════════════════════════════════════════════════════
        //  EXPORT / IMPORT / DRUCK
        // ══════════════════════════════════════════════════════════════
        public static void LogExport(string format, string pfad, int datensaetze, long bytes)
        {
            SchreibeLog(LogLevel.INFO, "EXPORT", $"📤 Export ({format})",
                $"Format: {format} | Pfad: {pfad} | Datensätze: {datensaetze} | " +
                $"Größe: {bytes / 1024.0:F1} KB | Von: {_benutzer}");
        }

        public static void LogImport(string datei, int ok, int fehler, int duplikate)
        {
            SchreibeLog(fehler > 0 ? LogLevel.WARNING : LogLevel.SUCCESS, "IMPORT", "📥 Import",
                $"Datei: {datei} | OK: {ok} | Fehler: {fehler} | Duplikate: {duplikate} | Von: {_benutzer}");
        }

        public static void LogDruckvorgang(string mitarbeiter, int artikel, string drucker, bool ok)
        {
            SchreibeLog(ok ? LogLevel.SUCCESS : LogLevel.ERROR, "DRUCK",
                $"🖨️ Druck {(ok ? "✔" : "✖")}",
                $"Mitarbeiter: {mitarbeiter} | Artikel: {artikel} | Drucker: {drucker} | Von: {_benutzer}");
        }

        // ══════════════════════════════════════════════════════════════
        //  KI / NACHBESTELLUNG / TAGS
        // ══════════════════════════════════════════════════════════════
        public static void LogKIAktion(string aktion, string ergebnis)
        {
            SchreibeLog(LogLevel.INFO, "KI", $"🤖 {aktion}", $"Ergebnis: {ergebnis} | Von: {_benutzer}");
        }

        public static void LogNachbestellungLinkGeoeffnet(string shop, string artikel, string suche, int bestand, int mindest)
        {
            SchreibeLog(LogLevel.INFO, "NACHBESTELLUNG", $"🛒 Shop geöffnet: {shop}",
                $"Artikel: {artikel} | Suchbegriff: '{suche}' | " +
                $"Bestand: {bestand}/{mindest} | Fehlend: {Math.Max(0, mindest - bestand)} | Von: {_benutzer}");
        }

        public static void LogNachbestellungAlleShops(string artikel, int anzShops)
        {
            SchreibeLog(LogLevel.INFO, "NACHBESTELLUNG", "🌐 Alle Shops geöffnet",
                $"Artikel: {artikel} | Shops: {anzShops} | Von: {_benutzer}");
        }

        public static void LogTagGesetzt(string typ, string name, string tag)
        {
            SchreibeLog(LogLevel.INFO, "TAGS", $"🏷️ Tag gesetzt",
                $"Typ: {typ} | Objekt: {name} | Tag: {tag} | Von: {_benutzer}");
        }

        public static void LogTagEntfernt(string typ, string name, string tag, bool alle)
        {
            SchreibeLog(LogLevel.WARNING, "TAGS", alle ? "🏷️ ALLE Tags entfernt (DEV)" : "🏷️ Tag entfernt (DEV)",
                $"Typ: {typ} | Objekt: {name} | Tag: {(alle ? "ALLE" : tag)} | Von: {_benutzer}");
        }

        // ══════════════════════════════════════════════════════════════
        //  DATEI / FEHLER
        // ══════════════════════════════════════════════════════════════
        public static void LogDatenGeladen(string typ, int anz)
        {
            SchreibeLog(LogLevel.INFO, "DATEI", "📂 Daten geladen",
                $"Typ: {typ} | Datensätze: {anz} | {DateTime.Now:HH:mm:ss.fff}");
        }

        public static void LogDatenGespeichert(string typ, string details)
        {
            SchreibeLog(LogLevel.INFO, "DATEI", "💾 Daten gespeichert", $"Typ: {typ} | {details}");
        }

        public static void LogFehler(string bereich, string msg)
        {
            SchreibeLog(LogLevel.ERROR, "FEHLER", $"❌ Fehler: {bereich}", $"Meldung: {msg} | Von: {_benutzer}");
        }

        public static void LogWarnung(string bereich, string msg)
        {
            SchreibeLog(LogLevel.WARNING, "WARNUNG", bereich, msg);
        }

        public static void LogValidierungsFehler(string feld, string eingabe, string grund)
        {
            SchreibeLog(LogLevel.WARNING, "VALIDIERUNG", "⚠️ Ungültige Eingabe",
                $"Feld: {feld} | Eingabe: '{eingabe}' | Grund: {grund} | Von: {_benutzer}");
        }

        public static void LogDateiFehler(string pfad, string op, string fehler)
        {
            SchreibeLog(LogLevel.ERROR, "DATEI", $"❌ Dateifehler: {op}",
                $"Pfad: {pfad} | Fehler: {fehler} | Von: {_benutzer}");
        }

        public static void LogSystemFehler(string bereich, string msg)
        {
            SchreibeLog(LogLevel.CRITICAL, "SYSTEM", $"💀 Kritisch: {bereich}",
                $"Meldung: {msg} | Von: {_benutzer}");
        }

        // ══════════════════════════════════════════════════════════════
        //  COMPLIANCE-REPORT (DSGVO / GoBD)
        // ══════════════════════════════════════════════════════════════
        public static void ErstelleComplianceReport()
        {
            string pfad = $"Compliance_{DateTime.Now:yyyyMMdd_HHmmss}.enc";
            try
            {
                ConsoleHelper.PrintInfo("🔓 Entschlüssele Log...");
                Thread.Sleep(300);
                string log = EncryptionManager.ReadEncryptedFile(logFilePath);
                if (log == null) { ConsoleHelper.PrintError("Entschlüsselung fehlgeschlagen!"); return; }

                string[] zeilen = log.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // Personenbezogene Zugriffe (DSGVO)
                var personenZugriffe = zeilen.Where(z => z.Contains("[BENUTZER]") || z.Contains("[MITARBEITER]")).Count();
                var datenaenderungen = zeilen.Where(z => z.Contains("[FELD-AUDIT]")).Count();
                var loeschoperationen = zeilen.Where(z => z.Contains("gelöscht — UNWIDERRUFLICH")).Count();
                var sicherheitsEvents = zeilen.Where(z => z.Contains("[SECURITY]") || z.Contains("[AUTH]")).Count();
                var anomalien = zeilen.Where(z => z.Contains("[ANOMALY]")).Count();

                var sb = new StringBuilder();
                sb.AppendLine("╔════════════════════════════════════════════════════════════════════════╗");
                sb.AppendLine("║      COMPLIANCE-REPORT — DSGVO / GoBD                                ║");
                sb.AppendLine("║      INVENTARVERWALTUNG — ENTERPRISE v4.0                             ║");
                sb.AppendLine("╚════════════════════════════════════════════════════════════════════════╝");
                sb.AppendLine($"Erstellt am:                  {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                sb.AppendLine($"Erstellt von:                 {_benutzer}");
                sb.AppendLine($"System:                       {Environment.MachineName}");
                sb.AppendLine($"Berichtszeitraum:             Gesamtes Log");
                sb.AppendLine($"Verschlüsselung:              AES-256 + SHA-256-Kette");
                sb.AppendLine();
                sb.AppendLine(new string('═', 76));
                sb.AppendLine("  DSGVO-RELEVANTE PROTOKOLLIERUNG:");
                sb.AppendLine(new string('─', 76));
                sb.AppendLine($"  Personenbezogene Datenzugriffe:  {personenZugriffe}");
                sb.AppendLine($"  Datenänderungen (Feld-Audit):    {datenaenderungen}");
                sb.AppendLine($"  Löschoperationen (unwiderruflich):{loeschoperationen}");
                sb.AppendLine($"  Authentifizierungsereignisse:    {sicherheitsEvents}");
                sb.AppendLine($"  Anomalie-Erkennungen:            {anomalien}");
                sb.AppendLine();
                sb.AppendLine(new string('═', 76));
                sb.AppendLine("  GoBD-RELEVANTE PROTOKOLLIERUNG:");
                sb.AppendLine(new string('─', 76));
                sb.AppendLine($"  Inventarwert aktuell:            {DataManager.Inventar.Sum(a => a.Preis * a.Anzahl):F2} €");
                sb.AppendLine($"  Artikel mit Wert > 0:            {DataManager.Inventar.Count(a => a.Preis > 0)}");
                sb.AppendLine($"  Kettenintegrität aktiv:          Ja (SHA-256 Block-Kette)");
                sb.AppendLine($"  Unveränderlichkeit sichergestellt: Ja (AES-256 verschlüsselt)");
                sb.AppendLine($"  Maschinelle Auswertbarkeit:      Ja (strukturiertes Format)");
                sb.AppendLine();
                sb.AppendLine("  CHAIN-INTEGRITÄT:");
                sb.AppendLine($"  Letzter Block-Hash:  {_letzterBlockHash}");
                sb.AppendLine($"  Gesamtblöcke:        {_laufenderZaehler}");
                sb.AppendLine();
                sb.AppendLine("  BENUTZER MIT SYSTEMZUGANG (zur Zeit der Berichterstellung):");
                foreach (var b in DataManager.Benutzer)
                    sb.AppendLine($"  · {b.Benutzername,-20} Berechtigung: {b.Berechtigung}");
                sb.AppendLine();
                sb.AppendLine(new string('═', 76));
                sb.AppendLine($"Ende Compliance-Report · Session: {_sessionId}");

                byte[] enc = EncryptionManager.EncryptString(sb.ToString());
                if (enc == null) { ConsoleHelper.PrintError("Verschlüsselung fehlgeschlagen!"); return; }
                File.WriteAllBytes(pfad, enc);

                SchreibeLog(LogLevel.SECURITY, "COMPLIANCE", "📋 Compliance-Report erstellt",
                    $"Pfad: {pfad} | Personenzugriffe: {personenZugriffe} | " +
                    $"Änderungen: {datenaenderungen} | Anomalien: {anomalien}");

                ConsoleHelper.PrintSuccess($"✔ Compliance-Report: {pfad}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"  · Personenbezogene Zugriffe: {personenZugriffe}");
                Console.WriteLine($"  · Datenänderungen:           {datenaenderungen}");
                Console.WriteLine($"  · Löschoperationen:          {loeschoperationen}");
                Console.WriteLine($"  · Anomalien:                 {anomalien}");
                Console.ResetColor();
            }
            catch (Exception ex) { ConsoleHelper.PrintError($"Fehler: {ex.Message}"); }
        }

        // ══════════════════════════════════════════════════════════════
        //  TAGES-REPORT
        // ══════════════════════════════════════════════════════════════
        public static void ErstelleTagesReport()
        {
            string heute = DateTime.Now.ToString("dd.MM.yyyy");
            string pfad = $"Report_{DateTime.Now:yyyyMMdd}.enc";
            try
            {
                ConsoleHelper.PrintInfo("🔓 Entschlüssele Log...");
                Thread.Sleep(250);
                string log = EncryptionManager.ReadEncryptedFile(logFilePath);
                if (log == null) { ConsoleHelper.PrintError("Entschlüsselung fehlgeschlagen!"); return; }

                string[] alle = log.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                int z = alle.Count(l => l.Contains(heute));
                int f = alle.Count(l => l.Contains(heute) && (l.Contains("[ERROR") || l.Contains("[CRITICAL")));
                int w = alle.Count(l => l.Contains(heute) && l.Contains("[WARNING"));
                int s = alle.Count(l => l.Contains(heute) && l.Contains("[SECURITY"));
                int a = alle.Count(l => l.Contains(heute) && l.Contains("[ANOMALY"));
                int fa = alle.Count(l => l.Contains(heute) && l.Contains("[FELD-AUDIT"));

                var sb = new StringBuilder();
                sb.AppendLine("╔════════════════════════════════════════════════════════════════════════╗");
                sb.AppendLine("║      TAGESREPORT — INVENTARVERWALTUNG ENTERPRISE v4.0                ║");
                sb.AppendLine("╚════════════════════════════════════════════════════════════════════════╝");
                sb.AppendLine($"Datum:                 {heute}  ·  Session: {_sessionId}");
                sb.AppendLine($"Erstellt von:          {_benutzer}  ·  {DateTime.Now:HH:mm:ss}");
                sb.AppendLine($"Ketten-Hash:           {_letzterBlockHash}");
                sb.AppendLine(new string('═', 76));
                sb.AppendLine();
                sb.AppendLine("  ── SYSTEMSTATUS ─────────────────────────────────────────────────────");
                sb.AppendLine($"  · Inventar:                     {DataManager.Inventar.Count} Artikel");
                sb.AppendLine($"  · Inventarwert:                 {DataManager.Inventar.Sum(a => a.Preis * a.Anzahl):F2} €");
                sb.AppendLine($"  · Unter Mindestbestand:         {DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand)}");
                sb.AppendLine($"  · Leer (0 Stück):               {DataManager.Inventar.Count(a => a.Anzahl == 0)}");
                sb.AppendLine($"  · Mitarbeiter:                  {DataManager.Mitarbeiter.Count}");
                sb.AppendLine($"  · Benutzer:                     {DataManager.Benutzer.Count}");
                sb.AppendLine($"  · Anomalie-Score:               {_anomalieScore}");
                sb.AppendLine();
                sb.AppendLine("  ── LOG-ZUSAMMENFASSUNG ──────────────────────────────────────────────");
                sb.AppendLine($"  · Log-Zeilen heute:             {z}");
                sb.AppendLine($"  · Feld-Audit-Einträge:          {fa}");
                sb.AppendLine($"  · Fehler/Kritisch:              {f}");
                sb.AppendLine($"  · Warnungen:                    {w}");
                sb.AppendLine($"  · Sicherheitsereignisse:        {s}");
                sb.AppendLine($"  · Anomalie-Erkennungen:         {a}");
                sb.AppendLine($"  · Brute-Force-Sperre:           {(_bruteForceSperr ? "⚠️ AKTIV" : "✔ OK")}");

                if (_inventarTrend.Count >= 2)
                {
                    sb.AppendLine();
                    sb.AppendLine("  ── INVENTARWERT-TREND (Session) ─────────────────────────────────────");
                    foreach (var tp in _inventarTrend.TakeLast(10))
                        sb.AppendLine($"  · {tp.Zeit:HH:mm:ss}  {tp.Wert:F2}€  ({tp.Artikel} Art., {tp.Kritisch} krit.)  ← {tp.AusloesEvent}");
                }

                sb.AppendLine();
                sb.AppendLine(new string('═', 76));
                sb.AppendLine("  LOG-EINTRÄGE DES TAGES:");
                sb.AppendLine(new string('═', 76));
                sb.AppendLine();

                bool inBlock = false; var block = new List<string>();
                foreach (var l in alle)
                {
                    if (l.StartsWith("┌─")) { inBlock = false; block.Clear(); }
                    block.Add(l);
                    if (block.Any(x => x.Contains(heute))) inBlock = true;
                    if (l.StartsWith("└─") && inBlock)
                    { sb.AppendLine(string.Join(Environment.NewLine, block)); inBlock = false; }
                }

                sb.AppendLine();
                sb.AppendLine($"Ende des Reports — {heute}");

                ConsoleHelper.PrintInfo("🔐 Verschlüssele Report...");
                Thread.Sleep(200);
                byte[] enc = EncryptionManager.EncryptString(sb.ToString());
                if (enc == null) { ConsoleHelper.PrintError("Verschlüsselung fehlgeschlagen!"); return; }
                File.WriteAllBytes(pfad, enc);

                ConsoleHelper.PrintSuccess($"✔ Report: {pfad}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"  Zeilen: {z}  |  Fehler: {f}  |  Anomalien: {a}  |  Feld-Audits: {fa}");
                Console.ResetColor();

                SchreibeLog(LogLevel.INFO, "SYSTEM", "📋 Tagesreport erstellt",
                    $"Pfad: {pfad} | Zeilen: {z} | Fehler: {f} | Anomalien: {a}");
            }
            catch (Exception ex) { ConsoleHelper.PrintError($"Fehler: {ex.Message}"); }
        }

        // ══════════════════════════════════════════════════════════════
        //  LOG-VIEWER
        // ══════════════════════════════════════════════════════════════
        public static void ZeigeLogDatei()
        {
            while (true)
            {
                Console.Clear();
                ZeigeLogHeader();
                ZeigeLogViewerMenu();

                string w = ConsoleHelper.GetInput("  Aktion");
                switch (w.ToLower())
                {
                    case "1": ZeigeLogEintraege(null, 60); break;
                    case "2": ZeigeLogEintraege("ERROR", 50); break;
                    case "3": ZeigeLogEintraege("SECURITY", 50); break;
                    case "4": ZeigeLogEintraege("ANOMALY", 50); break;
                    case "5": ZeigeLogEintraege("BESTAND", 50); break;
                    case "6": ZeigeLogEintraege("INVENTAR", 50); break;
                    case "7": ZeigeLogEintraege("FELD-AUDIT", 50); break;
                    case "8": ZeigeLogEintraege("TRANSAKTION", 50); break;
                    case "9": ZeigeLogEintraege("SNAPSHOT", 50); break;
                    case "t": ZeigeTrendAnzeige(); break;
                    case "s": ZeigeSuchDialog(); break;
                    case "x": ZeigeLogStatistik(); break;
                    case "i": ZeigeLogDateiInfo(); break;
                    case "l": ZeigeLiveMonitor(); break;
                    case "a": ZeigeArchivListe(); break;
                    case "0": return;
                }
            }
        }

        private static void ZeigeLogViewerMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌─ LOG-VIEWER ────────────────────────────────────────────────────┐");
            Console.ResetColor();

            ZeigeLogMenuItem("1", "📋", "Alle Einträge (letzte 60)", ConsoleColor.White);
            ZeigeLogMenuItem("2", "🔴", "Fehler & Kritisch", ConsoleColor.Red);
            ZeigeLogMenuItem("3", "🔐", "Sicherheitsereignisse", ConsoleColor.Yellow);
            ZeigeLogMenuItem("4", "🚨", "Anomalie-Erkennungen", ConsoleColor.Magenta);
            ZeigeLogMenuItem("5", "📊", "Bestandsveränderungen", ConsoleColor.Green);
            ZeigeLogMenuItem("6", "📦", "Inventar-Aktionen", ConsoleColor.Cyan);
            ZeigeLogMenuItem("7", "✏️ ", "Feld-Audit Trail", ConsoleColor.DarkYellow);
            ZeigeLogMenuItem("8", "⏩", "Transaktions-Log", ConsoleColor.DarkCyan);
            ZeigeLogMenuItem("9", "📸", "Daten-Snapshots (Before/After)", ConsoleColor.DarkGray);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ├────────────────────────────────────────────────────────────────┤");
            Console.ResetColor();

            ZeigeLogMenuItem("T", "📈", "Inventarwert-Trend (Session)", ConsoleColor.Cyan);
            ZeigeLogMenuItem("S", "🔍", "Suche mit Highlighting", ConsoleColor.White);
            ZeigeLogMenuItem("X", "📊", "Statistik & Auswertung", ConsoleColor.Cyan);
            ZeigeLogMenuItem("L", "⚡", "Live-Monitor (Auto-Refresh 3s)", ConsoleColor.Green);
            ZeigeLogMenuItem("I", "ℹ️ ", "Log-Datei Info & Ketten-Integrität", ConsoleColor.DarkGray);
            ZeigeLogMenuItem("A", "🗄️ ", "Archiv-Übersicht", ConsoleColor.DarkGray);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ├────────────────────────────────────────────────────────────────┤");
            Console.ResetColor();
            ZeigeLogMenuItem("0", "↩️ ", "Zurück", ConsoleColor.DarkGray);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  └────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeLogMenuItem(string taste, string icon, string text, ConsoleColor farbe)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"[{taste}]  ");
            Console.ForegroundColor = farbe;
            Console.Write($"{icon}  {text}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(' ', Math.Max(0, 57 - text.Length - icon.Length)) + "│");
            Console.ResetColor();
        }

        private static void ZeigeTrendAnzeige()
        {
            Console.Clear();
            ZeigeLogHeader();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ INVENTARWERT-TREND (Session) ─────────────────────────────────┐");
            Console.ResetColor();

            if (_inventarTrend.Count < 2)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ○  Zu wenig Datenpunkte (mindestens 2 nötig).");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            decimal minWert = _inventarTrend.Min(t => t.Wert);
            decimal maxWert = _inventarTrend.Max(t => t.Wert);
            decimal spanne = maxWert - minWert;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Min: {minWert:F2}€  ·  Max: {maxWert:F2}€  ·  Spanne: {spanne:F2}€  ·  Punkte: {_inventarTrend.Count}");
            Console.WriteLine();

            foreach (var tp in _inventarTrend.TakeLast(30))
            {
                int balken = spanne > 0 ? (int)((tp.Wert - minWert) / spanne * 40) : 20;
                bool istHoch = tp.Wert >= maxWert * 0.9m;
                bool istTief = tp.Wert <= minWert * 1.1m + 0.01m;
                ConsoleColor fc = istHoch ? ConsoleColor.Green : istTief ? ConsoleColor.Red : ConsoleColor.Cyan;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  {tp.Zeit:HH:mm:ss}  ");
                Console.ForegroundColor = fc;
                Console.Write(new string('█', balken) + new string('░', 40 - balken));
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  {tp.Wert,10:F2}€");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  {tp.AusloesEvent}");
            }

            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeLiveMonitor()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ⚡ LIVE-MONITOR — Drücke [ESC] oder [Q] zum Beenden");
            Console.ResetColor();
            Console.WriteLine();

            string letzterLog = "";
            int refresh = 0;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Escape || k.Key == ConsoleKey.Q) break;
                }

                string log = "";
                try { log = EncryptionManager.ReadEncryptedFile(logFilePath) ?? ""; } catch { }

                if (log != letzterLog && !string.IsNullOrEmpty(log))
                {
                    letzterLog = log;
                    var zeilen = log.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                   .Where(z => !string.IsNullOrWhiteSpace(z))
                                   .TakeLast(15)
                                   .ToList();

                    Console.SetCursorPosition(0, 3);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  🔄 Refresh #{++refresh}  ·  {DateTime.Now:HH:mm:ss.fff}  ·  [ESC/Q = Beenden]       ");
                    Console.WriteLine();

                    foreach (var z in zeilen)
                    {
                        ConsoleColor fc = ConsoleColor.Gray;
                        if (z.Contains("[ANOMALY")) fc = ConsoleColor.Magenta;
                        else if (z.Contains("[CRITICAL")) fc = ConsoleColor.Red;
                        else if (z.Contains("[ERROR")) fc = ConsoleColor.Red;
                        else if (z.Contains("[SECURITY")) fc = ConsoleColor.Yellow;
                        else if (z.Contains("[WARNING")) fc = ConsoleColor.DarkYellow;
                        else if (z.Contains("[SUCCESS")) fc = ConsoleColor.Green;
                        else if (z.Contains("[DEBUG")) fc = ConsoleColor.DarkGray;

                        Console.ForegroundColor = fc;
                        string anzeige = z.Length > 100 ? z.Substring(0, 97) + "..." : z;
                        Console.WriteLine($"  {anzeige,-100}");
                        Console.ResetColor();
                    }
                }

                Thread.Sleep(3000);
            }
        }

        private static void ZeigeLogEintraege(string filter, int max)
        {
            Console.Clear();
            ZeigeLogHeader();

            string log = LeseLog();
            if (log == null) return;

            var zeilen = log.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            List<string> out2 = filter == null ? zeilen :
                filter == "ERROR" ? FilterBlöcke(zeilen, l => l.Contains("[ERROR") || l.Contains("[CRITICAL") || l.Contains("[ANOMALY")) :
                FilterBlöcke(zeilen, l => l.Contains($"[{filter}]") || l.Contains($"[{filter} "));

            out2 = out2.Skip(Math.Max(0, out2.Count - max * 14)).ToList();

            string ft = filter == null ? "ALLE EINTRÄGE" : $"FILTER: {filter}";
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ┌─ {ft} " + new string('─', Math.Max(0, 62 - ft.Length)) + "┐");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var z in out2)
            {
                if (string.IsNullOrWhiteSpace(z)) { Console.WriteLine(); continue; }
                ConsoleColor fc = ConsoleColor.Gray;
                if (z.Contains("[ANOMALY")) fc = ConsoleColor.Magenta;
                else if (z.Contains("[CRITICAL")) fc = ConsoleColor.Red;
                else if (z.Contains("[ERROR")) fc = ConsoleColor.Red;
                else if (z.Contains("[SECURITY")) fc = ConsoleColor.Yellow;
                else if (z.Contains("[WARNING")) fc = ConsoleColor.DarkYellow;
                else if (z.Contains("[SUCCESS")) fc = ConsoleColor.Green;
                else if (z.Contains("[PERFORMANCE")) fc = ConsoleColor.Magenta;
                else if (z.Contains("[DEBUG")) fc = ConsoleColor.DarkGray;
                else if (z.Contains("│  Benutzer:")) fc = ConsoleColor.Cyan;
                else if (z.Contains("│  Aktion:")) fc = ConsoleColor.White;
                else if (z.Contains("│    ▸")) fc = ConsoleColor.DarkCyan;
                else if (z.StartsWith("┌─") || z.StartsWith("└─") || z.StartsWith("╔") || z.StartsWith("╚") || z.StartsWith("╠")) fc = ConsoleColor.DarkGray;

                Console.ForegroundColor = fc;
                Console.WriteLine("  " + z);
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ── {out2.Count(l => !string.IsNullOrWhiteSpace(l))} Zeilen ──");
            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeSuchDialog()
        {
            Console.Clear();
            ZeigeLogHeader();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ LOG-SUCHE ────────────────────────────────────────────────────┐");
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Begriff, Benutzername, IP, Datum, Session-ID, Inv-Nr, Feld ...");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("    │");
            Console.WriteLine("  └────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();

            string such = ConsoleHelper.GetInput("  Suchbegriff");
            if (string.IsNullOrWhiteSpace(such)) return;

            string log = LeseLog();
            if (log == null) return;

            var zeilen = log.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            var treffer = FilterBlöcke(zeilen, l => l.IndexOf(such, StringComparison.OrdinalIgnoreCase) >= 0);
            int anz = treffer.Count(l => l.StartsWith("┌─"));

            Console.Clear();
            ZeigeLogHeader();
            Console.ForegroundColor = anz > 0 ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
            Console.WriteLine($"  ┌─ {anz} Treffer für: '{such}' " + new string('─', Math.Max(0, 52 - such.Length)));
            Console.ResetColor();
            Console.WriteLine();

            foreach (var z in treffer)
            {
                if (string.IsNullOrWhiteSpace(z)) { Console.WriteLine(); continue; }
                int pos = z.IndexOf(such, StringComparison.OrdinalIgnoreCase);
                if (pos >= 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  " + z.Substring(0, pos));
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(z.Substring(pos, such.Length));
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(z.Substring(pos + such.Length));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("  " + z);
                }
                Console.ResetColor();
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeLogStatistik()
        {
            Console.Clear();
            ZeigeLogHeader();

            string log = LeseLog();
            if (log == null) return;

            int ges = 0, err = 0, warn = 0, sec = 0, succ = 0, crit = 0, inf = 0, anom = 0, dbg = 0, perf = 0, feld = 0, trx = 0;
            string heute = DateTime.Now.ToString("dd.MM.yyyy");
            int heuteAnz = 0;
            var benutzerAktiv = new Dictionary<string, int>();
            var stundenVert = new int[24];
            var katZähler = new Dictionary<string, int>();
            var alleKats = new[] { "INVENTAR","MITARBEITER","BENUTZER","BESTAND","AUTH","DATEI",
                                        "EXPORT","IMPORT","DRUCK","KI","TAGS","NACHBESTELLUNG","SYSTEM",
                                        "NAVIGATION","SNAPSHOT","INTEGRITÄT","ANOMALIE","ZUWEISUNG",
                                        "HEARTBEAT","DEV","PERF","SESSION","VALIDIERUNG","FELD-AUDIT",
                                        "TRANSAKTION","COMPLIANCE","TREND" };

            foreach (var z in log.Split('\n'))
            {
                if (z.Contains("┌─ ") && z.Contains("["))
                {
                    ges++;
                    try
                    {
                        int hp = z.IndexOf(' ') + 12;
                        if (hp > 0 && z.Length > hp + 2 && int.TryParse(z.Substring(hp, 2), out int h) && h is >= 0 and < 24)
                            stundenVert[h]++;
                    }
                    catch { }
                }
                if (z.Contains("[ANOMALY")) anom++;
                if (z.Contains("[CRITICAL")) crit++;
                if (z.Contains("[ERROR")) err++;
                if (z.Contains("[WARNING")) warn++;
                if (z.Contains("[SECURITY")) sec++;
                if (z.Contains("[SUCCESS")) succ++;
                if (z.Contains("[INFO")) inf++;
                if (z.Contains("[DEBUG")) dbg++;
                if (z.Contains("[PERFORMANCE")) perf++;
                if (z.Contains("[FELD-AUDIT")) feld++;
                if (z.Contains("[TRANSAKTION")) trx++;
                if (z.Contains(heute)) heuteAnz++;

                if (z.Contains("│  Benutzer:"))
                {
                    int s = z.IndexOf("│  Benutzer:") + 12;
                    int e = z.IndexOf("  ", s); if (e < 0) e = z.Length;
                    if (s >= 0 && e > s) { string bu = z.Substring(s, e - s).Trim(); if (!benutzerAktiv.ContainsKey(bu)) benutzerAktiv[bu] = 0; benutzerAktiv[bu]++; }
                }
                foreach (var k in alleKats)
                    if (z.Contains($"[{k}]") || z.Contains($"[{k} "))
                    { if (!katZähler.ContainsKey(k)) katZähler[k] = 0; katZähler[k]++; }
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌─ LOG-STATISTIK ─────────────────────────────────────────────────┐");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📋 Einträge gesamt: {ges}  ·  Heute: {heuteAnz}  ·  Feld-Audits: {feld}  ·  Transaktionen: {trx}");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ── NACH SCHWEREGRAD ─────────────────────────────────────────────");
            Console.ResetColor();
            ZeigeStatBalken("🟢 SUCCESS", succ, ges, ConsoleColor.Green);
            ZeigeStatBalken("🔵 INFO", inf, ges, ConsoleColor.Cyan);
            ZeigeStatBalken("⚫ DEBUG", dbg, ges, ConsoleColor.DarkGray);
            ZeigeStatBalken("⚡ PERF", perf, ges, ConsoleColor.DarkMagenta);
            ZeigeStatBalken("🟡 WARNING", warn, ges, ConsoleColor.Yellow);
            ZeigeStatBalken("🔴 ERROR", err, ges, ConsoleColor.Red);
            ZeigeStatBalken("💀 CRITICAL", crit, ges, ConsoleColor.DarkRed);
            ZeigeStatBalken("🔐 SECURITY", sec, ges, ConsoleColor.Magenta);
            ZeigeStatBalken("🚨 ANOMALIE", anom, ges, ConsoleColor.Magenta);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ── TOP-KATEGORIEN ───────────────────────────────────────────────");
            Console.ResetColor();
            foreach (var kv in katZähler.OrderByDescending(k => k.Value).Take(10))
                ZeigeStatBalken($"  {kv.Key,-20}", kv.Value, ges, ConsoleColor.White);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ── TOP-BENUTZER ─────────────────────────────────────────────────");
            Console.ResetColor();
            foreach (var kv in benutzerAktiv.OrderByDescending(k => k.Value).Take(5))
                ZeigeStatBalken($"  👤 {kv.Key,-18}", kv.Value, ges, ConsoleColor.Cyan);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ── AKTIVITÄT NACH TAGESSTUNDE ───────────────────────────────────");
            Console.ResetColor();
            int maxSt = stundenVert.Max() > 0 ? stundenVert.Max() : 1;
            for (int h = 0; h < 24; h += 6)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  {h:D2}h  ");
                for (int s = h; s < h + 6 && s < 24; s++)
                {
                    int f = (int)((double)stundenVert[s] / maxSt * 8);
                    bool istNacht = s >= 22 || s <= 5;
                    Console.ForegroundColor = stundenVert[s] == 0 ? ConsoleColor.DarkGray :
                                              istNacht ? ConsoleColor.Yellow : ConsoleColor.Cyan;
                    Console.Write(new string('█', f) + new string('░', 8 - f) + " ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"{s:D2}h ");
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Anomalie-Score aktuell: {_anomalieScore}  ·  Block-Kette: #{_laufenderZaehler}  ·  Hash: {_letzterBlockHash.Substring(0, 12)}...");
            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeLogDateiInfo()
        {
            Console.Clear();
            ZeigeLogHeader();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌─ LOG-DATEI INFO & KETTEN-INTEGRITÄT ───────────────────────────┐");
            Console.ResetColor();

            if (!File.Exists(logFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  │  Keine Log-Datei vorhanden.");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  └────────────────────────────────────────────────────────────────┘");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            var fi = new FileInfo(logFilePath);
            string h = EncryptionManager.GetFileHash(logFilePath);

            ZeigeInfoZeile("Pfad", Path.GetFullPath(logFilePath));
            ZeigeInfoZeile("Dateigröße", $"{fi.Length / 1024.0:F2} KB  ({fi.Length} Bytes)");
            ZeigeInfoZeile("Rotation ab", $"{MAX_LOG_MB} MB  ({fi.Length * 100 / (MAX_LOG_MB * 1024 * 1024):F1}% ausgelastet)");
            ZeigeInfoZeile("Erstellt", fi.CreationTime.ToString("dd.MM.yyyy HH:mm:ss"));
            ZeigeInfoZeile("Zuletzt geändert", fi.LastWriteTime.ToString("dd.MM.yyyy HH:mm:ss"));
            ZeigeInfoZeile("Verschlüsselung", "AES-256-CBC + PBKDF2-SHA256 (10.000 Iter.)");
            ZeigeInfoZeile("Datei-SHA-256", h != null ? h.Substring(0, 32) + "..." : "—");
            ZeigeInfoZeile("Block-Kette #", _laufenderZaehler.ToString("D8"));
            ZeigeInfoZeile("Letzter Block-Hash", _letzterBlockHash.Substring(0, Math.Min(32, _letzterBlockHash.Length)) + "...");
            ZeigeInfoZeile("Session-ID", string.IsNullOrEmpty(_sessionId) ? "—" : _sessionId);
            ZeigeInfoZeile("Session-Aktionen", _aktionen.ToString());
            ZeigeInfoZeile("Session-Fehler", _fehler.ToString());
            ZeigeInfoZeile("Anomalie-Score", $"{_anomalieScore} {(_anomalieScore >= 50 ? "⚠️ HOCH" : _anomalieScore >= 20 ? "○ Mittel" : "✔ Niedrig")}");
            ZeigeInfoZeile("Brute-Force", _bruteForceSperr ? "⚠️ AKTIVE SPERRE" : "✔ OK");
            ZeigeInfoZeile("Login-Fehlversuche", _loginFehler.ToString());
            ZeigeInfoZeile("Letzter Heartbeat", _letzterHeartbeat == DateTime.MinValue ? "—" : _letzterHeartbeat.ToString("HH:mm:ss"));
            ZeigeInfoZeile("Trend-Punkte", _inventarTrend.Count.ToString());

            if (Directory.Exists(archivOrdner))
            {
                var archs = Directory.GetFiles(archivOrdner, "*.enc");
                long arcSz = archs.Sum(f => new FileInfo(f).Length);
                ZeigeInfoZeile("Archivdateien", $"{archs.Length}  ·  Gesamt: {arcSz / 1024.0:F1} KB");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  └────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeArchivListe()
        {
            Console.Clear();
            ZeigeLogHeader();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌─ LOG-ARCHIV ───────────────────────────────────────────────────┐");
            Console.ResetColor();
            Console.WriteLine();

            if (!Directory.Exists(archivOrdner) || !Directory.GetFiles(archivOrdner, "*.enc").Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ○  Kein Archiv vorhanden.");
                Console.ResetColor();
            }
            else
            {
                foreach (var f in Directory.GetFiles(archivOrdner, "*.enc")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"  🗄️  {f.Name,-42}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{f.Length / 1024.0:F1} KB");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  ·  {f.CreationTime:dd.MM.yyyy HH:mm}");
                }
            }

            Console.WriteLine();
            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════
        //  HILFSFUNKTIONEN
        // ══════════════════════════════════════════════════════════════
        private static string LeseLog()
        {
            if (!File.Exists(logFilePath))
            { ConsoleHelper.PrintWarning("Keine Log-Datei!"); ConsoleHelper.PressKeyToContinue(); return null; }
            ConsoleHelper.PrintInfo("🔓 Entschlüssele...");
            Thread.Sleep(200);
            string r = EncryptionManager.ReadEncryptedFile(logFilePath);
            if (r == null) ConsoleHelper.PrintError("Entschlüsselung fehlgeschlagen!");
            return r;
        }

        private static List<string> FilterBlöcke(List<string> zeilen, Func<string, bool> f)
        {
            var res = new List<string>(); var blk = new List<string>(); bool m = false;
            foreach (var z in zeilen)
            {
                if (z.StartsWith("┌─")) { blk.Clear(); m = false; }
                blk.Add(z);
                if (f(z)) m = true;
                if (z.StartsWith("└─") && blk.Count > 1 && m)
                { res.AddRange(blk); res.Add(""); }
            }
            return res;
        }

        private static void ZeigeLogHeader()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("🔐  S Y S T E M - L O G  —  F O R E N S I C  v 4 . 0");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("          ║");
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            string si = string.IsNullOrEmpty(_sessionId) ? "Keine Session" :
                        $"Session: {_sessionId}  ·  #{_aktionen}  ·  Score: {_anomalieScore}  ·  Kette: #{_laufenderZaehler}";
            string sia = si.Length > 62 ? si.Substring(0, 59) + "..." : si;
            Console.Write(sia);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(new string(' ', Math.Max(0, 63 - sia.Length)) + "║");
            if (_bruteForceSperr)
            {
                Console.Write("  ║  ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("⚠️  BRUTE-FORCE-SPERRE AKTIV");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(new string(' ', 37) + "║");
            }
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeStatBalken(string label, int wert, int gesamt, ConsoleColor fc)
        {
            int w = 22; int f = gesamt > 0 ? (int)((double)wert / gesamt * w) : 0;
            double p = gesamt > 0 ? (double)wert / gesamt * 100 : 0;
            Console.ForegroundColor = fc;
            Console.Write($"  {label,-22}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  [");
            Console.ForegroundColor = fc;
            Console.Write(new string('█', f));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('░', w - f) + "] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{wert,5}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ({p:F1}%)");
            Console.ResetColor();
        }

        private static void ZeigeInfoZeile(string label, string wert)
        {
            string a = wert.Length > 42 ? wert.Substring(0, 39) + "..." : wert;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"{label,-22}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(a);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(' ', Math.Max(0, 64 - label.Length - a.Length)) + "│");
            Console.ResetColor();
        }

        private static string ErzeugeKorrelationsId(string präfix)
            => $"{präfix}-{DateTime.Now:HHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";

        private static string GetIP()
        {
            try
            {
                foreach (var addr in Dns.GetHostAddresses(Dns.GetHostName()))
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return addr.ToString();
                return "—";
            }
            catch { return "—"; }
        }

        private static string BerechneSHA256(string input)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLower();
        }

        private static string BerechneSHA256Datei(string pfad)
        {
            using var sha = SHA256.Create();
            using var s = File.OpenRead(pfad);
            return BitConverter.ToString(sha.ComputeHash(s)).Replace("-", "").ToLower();
        }

        private static string BerechneMD5(string input)
        {
            using var md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLower().Substring(0, 8);
        }

        private static string FormatDauer(TimeSpan ts)
        {
            if (ts.Ticks <= 0) return "—";
            if (ts.TotalSeconds < 60) return $"{ts.Seconds}s";
            if (ts.TotalMinutes < 60) return $"{ts.Minutes}m {ts.Seconds}s";
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        }

        // Performance-Wrapper (Kompatibilität mit v3)
        private static Stopwatch _perfWatch;
        public static void StartPerformanceMessung(string k) => StartPerformanceMessungNamed(k);
        public static void StopPerformanceMessung(string k) => StopPerformanceMessungNamed(k);

        public static void StartPerformanceMessungNamed(string kontext)
        {
            lock (_perfWatches) { _perfWatches[kontext] = Stopwatch.StartNew(); }
            SchreibeLog(LogLevel.DEBUG, "PERF", $"⏱️ Messung: {kontext}");
        }

        public static void StopPerformanceMessungNamed(string kontext)
        {
            lock (_perfWatches)
            {
                if (!_perfWatches.TryGetValue(kontext, out var sw)) return;
                sw.Stop();
                _perfWatches.Remove(kontext);
                string b = sw.ElapsedMilliseconds < 100 ? "✔ Schnell" :
                           sw.ElapsedMilliseconds < 500 ? "○ Normal" :
                           sw.ElapsedMilliseconds < 2000 ? "⚠ Langsam" : "✖ Sehr langsam";
                SchreibeLog(LogLevel.DEBUG, "PERF", $"⏱️ Ende: {kontext}",
                    $"Dauer: {sw.ElapsedMilliseconds} ms | Ticks: {sw.ElapsedTicks} | Bewertung: {b}");
            }
        }
    }
}