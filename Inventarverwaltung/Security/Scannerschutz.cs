using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.UI;
using Inventarverwaltung.Manager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Inventarverwaltung.Security
{
    // ══════════════════════════════════════════════════════════════════════════
    //
    //   SCANNER-SCHUTZ  —  Manipulationsschutz für den Sicherheitsscanner
    //
    //   SCHUTZSCHICHT 1 — ASSEMBLY-INTEGRITÄTSPRÜFUNG
    //   SCHUTZSCHICHT 2 — SCANNER-KLASSEN-VOLLSTÄNDIGKEIT
    //   SCHUTZSCHICHT 3 — VERSCHLÜSSELTE REGELBANK
    //   SCHUTZSCHICHT 4 — BYPASS / DEBUGGER-ERKENNUNG
    //   SCHUTZSCHICHT 5 — VERSCHLÜSSELTES AUDIT-LOG
    //   SCHUTZSCHICHT 6 — DEAD-MAN'S SWITCH
    //
    // ══════════════════════════════════════════════════════════════════════════

    public static class ScannerSchutz
    {
        // ── Pfade — versteckt über SecurePaths ───────────────────────────────
        // Keine hartcodierten Pfadnamen — SecurePaths berechnet den Ort
        // und verwendet unauffällige Windows-Systemdateinamen.
        private static string ReferenzHashDatei => SecurePaths.AssemblyHashDatei;
        private static string AuditLogDatei => SecurePaths.AuditLogDatei;
        private const int MaxPingPause = 300;

        // ── Schlüssel ─────────────────────────────────────────────────────────
        // FIX CS0103: _schluessel kann nicht als Feld-Initializer aufgerufen werden
        // wenn DeriveKey() selbst ein static Feld initialisiert. → Lazy-Initialisierung.
        private static byte[] _schluessel;
        private static byte[] Schluessel
            => _schluessel ??= DeriveKey("ScannerSchutz_Regelbank_2026_#!Xq7@mP9");

        // ── Pflichttypen / -methoden ───────────────────────────────────────────
        private static readonly string[] _pflichtTypen = {
            "DateiSicherheitsScanner", "SicherheitsBefund", "SicherheitsStufe"
        };
        private static readonly string[] _pflichtMethoden = {
            "DateiScannen", "BefundAnzeigenUndEntscheiden"
        };
        private static readonly string[] _pflichtEnumWerte = {
            "Sicher", "Warnung", "Gefaehrlich"
        };

        // ── Dead-Man's Switch ─────────────────────────────────────────────────
        private static DateTime _letzterPing = DateTime.Now;
        private static bool _initialisiert = false;
        private static int _scanZaehler = 0;

        // ══════════════════════════════════════════════════════════════════
        // SCHUTZSCHICHT 3 — VERSCHLÜSSELTE REGELBANK
        // FIX CS0103: RegelVerschluesseln() greift auf Schluessel zu (Lazy-Property),
        // kein statisches Feld nötig. Array wird beim ersten Zugriff aufgebaut.
        // ══════════════════════════════════════════════════════════════════

        private static string[] _verschluesselteMuster;

        private static string[] VerschluesselteMuster
        {
            get
            {
                if (_verschluesselteMuster != null) return _verschluesselteMuster;

                _verschluesselteMuster = new[]
                {
                    RegelVerschluesseln("4D5A"),
                    RegelVerschluesseln("7F454C46"),
                    RegelVerschluesseln("AutoOpen"),
                    RegelVerschluesseln("Auto_Open"),
                    RegelVerschluesseln("Document_Open"),
                    RegelVerschluesseln("powershell"),
                    RegelVerschluesseln("Invoke-Expression"),
                    RegelVerschluesseln("WScript.Shell"),
                    RegelVerschluesseln("CreateObject"),
                    RegelVerschluesseln("Shell("),
                    RegelVerschluesseln("cmd.exe"),
                    RegelVerschluesseln("/bin/bash"),
                    RegelVerschluesseln("eval("),
                    RegelVerschluesseln("<script"),
                    RegelVerschluesseln("DROP TABLE"),
                    RegelVerschluesseln("UNION SELECT"),
                    RegelVerschluesseln("xp_cmdshell"),
                    RegelVerschluesseln("EICAR-STANDARD"),
                    RegelVerschluesseln("TVqQAAMAAAAEAAAA"),
                    RegelVerschluesseln("-EncodedCommand"),
                };
                return _verschluesselteMuster;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // PROGRAMMSTART — ALLE SCHICHTEN PRÜFEN
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// In Program.cs als allererste Zeile aufrufen — noch vor allem anderen.
        /// </summary>
        public static void ProgrammstartPruefen()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  🔒 Systemintegrität wird geprüft ...");
            Console.ResetColor();

#if DEBUG
            // ── DEBUG-MODUS (Visual Studio) ──────────────────────────────
            // Schicht 1 (Assembly-Hash) und Schicht 4 (Debugger-Erkennung)
            // werden übersprungen, weil:
            //   • Jede Kompilierung ändert die .exe → Hash ändert sich immer
            //   • Visual Studio startet immer mit Debugger → wäre immer FEHLER
            // Im Release-Build (echtes Produktivsystem) sind beide voll aktiv.

            Console.Write("  ├─ [1/6] Assembly-Integrität     ... ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ÜBERSPRUNGEN (Debug-Modus)");
            Console.ResetColor();
            bool s1 = true;

            Console.Write("  ├─ [2/6] Scanner-Vollständigkeit ... ");
            bool s2 = PruefeScannerKlassen();
            Status(s2);

            Console.Write("  ├─ [3/6] Regelbank               ... ");
            bool s3 = PruefeRegelbank();
            Status(s3);

            Console.Write("  ├─ [4/6] Bypass-Erkennung        ... ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("ÜBERSPRUNGEN (Debug-Modus)");
            Console.ResetColor();
            bool s4 = true;

#else
            // ── RELEASE-MODUS (Produktivsystem) ─────────────────────────
            // Alle 6 Schichten laufen vollständig durch.

            Console.Write("  ├─ [1/6] Assembly-Integrität     ... ");
            bool s1 = PruefeAssemblyIntegritaet();
            Status(s1);

            Console.Write("  ├─ [2/6] Scanner-Vollständigkeit ... ");
            bool s2 = PruefeScannerKlassen();
            Status(s2);

            Console.Write("  ├─ [3/6] Regelbank               ... ");
            bool s3 = PruefeRegelbank();
            Status(s3);

            Console.Write("  ├─ [4/6] Bypass-Erkennung        ... ");
            bool s4 = PruefeBypass();
            Status(s4);

#endif

            Console.Write("  ├─ [5/6] Audit-System            ... ");
            bool s5 = AuditInit();
            Status(s5);

            Console.Write("  └─ [6/6] Dead-Man's Switch       ... ");
            _letzterPing = DateTime.Now;
            Status(true);

            Console.WriteLine();

            bool ok = s1 && s2 && s3 && s4;
            if (!ok)
                TamperAlarm("Integritätsprüfung beim Start fehlgeschlagen.");

            _initialisiert = true;
            Console.ForegroundColor = ConsoleColor.Green;

#if DEBUG
            Console.WriteLine("  ✓ Sicherheitsprüfung bestanden (Debug-Modus).");
#else
            Console.WriteLine("  ✓ Alle Sicherheitsprüfungen bestanden.");
#endif
            Console.ResetColor();
            Console.WriteLine();
        }

        // ══════════════════════════════════════════════════════════════════
        // SCHICHT 1 — ASSEMBLY-INTEGRITÄT
        // ══════════════════════════════════════════════════════════════════

        private static bool PruefeAssemblyIntegritaet()
        {
            try
            {
                // FIX CS0117: Assembly.GetExecutingAssembly() statt
                // Assembly.GetExecutingAssembly (ohne Klammern war kein Fehler,
                // aber .Location ist eine Property, kein Feld)
                string pfad = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(pfad) || !File.Exists(pfad))
                    return true; // Single-File-Publish: kein separater Pfad

                string aktuellerHash = BerechneHash(pfad);

                if (!File.Exists(ReferenzHashDatei))
                {
                    SpeichereReferenzHash(aktuellerHash);
                    return true;
                }

                string referenzHash = LeseReferenzHash();
                if (referenzHash == null) return true;

                if (!aktuellerHash.Equals(referenzHash, StringComparison.OrdinalIgnoreCase))
                {
                    AuditSchreiben("KRITISCH",
                        $"Assembly-Hash abweichend! Erwartet: {referenzHash.Substring(0, 12)}  " +
                        $"Aktuell: {aktuellerHash.Substring(0, 12)}");
                    return false;
                }

                return true;
            }
            catch { return true; }
        }

        private static void SpeichereReferenzHash(string hash)
        {
            try
            {
                byte[] enc = AesVerschluesseln(
                    $"HASH|{hash}|{DateTime.Now:yyyyMMdd}", Schluessel);
                File.WriteAllBytes(ReferenzHashDatei, enc);
                File.SetAttributes(ReferenzHashDatei,
                    FileAttributes.Hidden | FileAttributes.ReadOnly);
            }
            catch { }
        }

        private static string LeseReferenzHash()
        {
            try
            {
                File.SetAttributes(ReferenzHashDatei,
                    File.GetAttributes(ReferenzHashDatei) & ~FileAttributes.ReadOnly);
                byte[] enc = File.ReadAllBytes(ReferenzHashDatei);
                File.SetAttributes(ReferenzHashDatei,
                    FileAttributes.Hidden | FileAttributes.ReadOnly);

                string klar = AesEntschluesseln(enc, Schluessel);
                if (klar == null) return null;

                var teile = klar.Split('|');
                return teile.Length >= 2 ? teile[1] : null;
            }
            catch { return null; }
        }

        // ══════════════════════════════════════════════════════════════════
        // SCHICHT 2 — SCANNER-KLASSEN-VOLLSTÄNDIGKEIT
        // FIX CS1061: .GetTypes() existiert auf Assembly — korrekte Nutzung
        // ══════════════════════════════════════════════════════════════════

        private static bool PruefeScannerKlassen()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();

                // FIX CS1061: GetTypes() ist eine Methode von Assembly, kein Problem —
                // aber wir sichern mit try/catch gegen ReflectionTypeLoadException ab
                Type[] alleTypen;
                try
                {
                    alleTypen = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Teilweise geladene Typen trotzdem verwenden
                    alleTypen = ex.Types.Where(t => t != null).ToArray();
                }

                var typNamen = alleTypen.Select(t => t.Name).ToHashSet();

                foreach (string name in _pflichtTypen)
                {
                    if (!typNamen.Contains(name))
                    {
                        AuditSchreiben("KRITISCH", $"Pflichttyp '{name}' fehlt!");
                        return false;
                    }
                }

                var scannerTyp = alleTypen.FirstOrDefault(
                    t => t.Name == "DateiSicherheitsScanner");
                if (scannerTyp == null) return false;

                var methodenNamen = scannerTyp
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Select(m => m.Name)
                    .ToHashSet();

                foreach (string m in _pflichtMethoden)
                {
                    if (!methodenNamen.Contains(m))
                    {
                        AuditSchreiben("KRITISCH", $"Pflichtmethode '{m}' fehlt!");
                        return false;
                    }
                }

                // SicherheitsStufe-Enum prüfen
                var stufeTyp = alleTypen.FirstOrDefault(
                    t => t.Name == "SicherheitsStufe");

                if (stufeTyp != null && stufeTyp.IsEnum)
                {
                    var werte = Enum.GetNames(stufeTyp);
                    foreach (string pw in _pflichtEnumWerte)
                    {
                        if (!werte.Contains(pw))
                        {
                            AuditSchreiben("KRITISCH",
                                $"SicherheitsStufe.{pw} fehlt!");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch { return false; }
        }

        // ══════════════════════════════════════════════════════════════════
        // SCHICHT 3 — REGELBANK-INTEGRITÄT
        // ══════════════════════════════════════════════════════════════════

        private static bool PruefeRegelbank()
        {
            int fehler = 0;
            foreach (string blob in VerschluesselteMuster)
            {
                if (string.IsNullOrEmpty(blob)) { fehler++; continue; }
                try
                {
                    byte[] bytes = Convert.FromBase64String(blob);
                    if (string.IsNullOrEmpty(AesEntschluesseln(bytes, Schluessel)))
                        fehler++;
                }
                catch { fehler++; }
            }

            if (fehler > 0)
            {
                AuditSchreiben("KRITISCH",
                    $"Regelbank: {fehler} Muster beschädigt!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gibt alle entschlüsselten Scan-Muster zurück.
        /// Wird vom DateiSicherheitsScanner zur Laufzeit aufgerufen.
        /// </summary>
        public static List<string> HoleEntschluesselteMuster()
        {
            var liste = new List<string>();
            foreach (string blob in VerschluesselteMuster)
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(blob);
                    string klar = AesEntschluesseln(bytes, Schluessel);
                    if (!string.IsNullOrEmpty(klar)) liste.Add(klar);
                }
                catch { }
            }
            return liste;
        }

        // ══════════════════════════════════════════════════════════════════
        // SCHICHT 4 — BYPASS / DEBUGGER-ERKENNUNG
        // ══════════════════════════════════════════════════════════════════

        private static bool PruefeBypass()
        {
#if DEBUG
            // Im Debug-Modus (Visual Studio) ist Debugger.IsAttached immer true
            // und VS setzt selbst Profiler-Umgebungsvariablen → immer false-positive.
            // Diese Prüfung läuft deshalb nur im Release-Build.
            return true;
#else
            var probleme = new List<string>();

            if (System.Diagnostics.Debugger.IsAttached)
                probleme.Add("Debugger ist angehängt");

            foreach (string ev in new[] {
                "CORECLR_ENABLE_PROFILING", "COR_ENABLE_PROFILING",
                "DOTNET_STARTUP_HOOKS",     "_DOTNET_CLR_PROFILER" })
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(ev)))
                    probleme.Add($"Verdächtige Umgebungsvariable: {ev}");
            }

            var reTools = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "dnspy", "ilspy", "dotpeek", "de4dot", "reflexil",
                "x64dbg", "x32dbg", "ollydbg", "windbg",
                "wireshark", "fiddler", "processhacker", "pestudio"
            };

            try
            {
                foreach (var proc in System.Diagnostics.Process.GetProcesses())
                {
                    try
                    {
                        if (reTools.Contains(proc.ProcessName.ToLowerInvariant()))
                            probleme.Add($"RE-Tool aktiv: {proc.ProcessName}");
                    }
                    catch { }
                }
            }
            catch { }

            foreach (string p in probleme)
                AuditSchreiben("WARNUNG", p);

            if (System.Diagnostics.Debugger.IsAttached)       return false;
            if (probleme.Any(p => p.StartsWith("RE-Tool")))    return false;

            return true;
#endif
        }

        // ══════════════════════════════════════════════════════════════════
        // SCHICHT 5 — AUDIT-LOG
        // FIX CS0117: LogManager.LogAktion → durch eigenes AuditSchreiben ersetzt
        // (LogAktion existiert im LogManager, aber wir nutzen das eigene
        //  verschlüsselte Audit-Log damit Scanner-Events nicht im normalen
        //  System-Log landen und separat geschützt sind)
        // ══════════════════════════════════════════════════════════════════

        private static bool AuditInit()
        {
            // FIX CS0117: Assembly.GetName().Version statt
            // Assembly.GetExecutingAssembly ohne ()
            string version = Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version?.ToString() ?? "unbekannt";

            AuditSchreiben("START", $"Programmstart | v{version}");
            return true;
        }

        public static void ScanBeginn(string dateiname)
        {
            _scanZaehler++;
            SchutzPing();
            AuditSchreiben("SCAN_START", $"#{_scanZaehler} | {dateiname}");
        }

        public static void ScanEnde(string dateiname, string ergebnis)
        {
            AuditSchreiben("SCAN_ENDE",
                $"#{_scanZaehler} | {dateiname} | {ergebnis}");
        }

        private static void AuditSchreiben(string typ, string nachricht)
        {
            try
            {
                string zeile = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{typ}|{nachricht}\n";
                byte[] enc = AesVerschluesseln(zeile, Schluessel);
                string b64 = Convert.ToBase64String(enc) + "\n";

                if (File.Exists(AuditLogDatei))
                {
                    try
                    {
                        File.SetAttributes(AuditLogDatei,
                            File.GetAttributes(AuditLogDatei) & ~FileAttributes.ReadOnly);
                    }
                    catch { }
                }

                File.AppendAllText(AuditLogDatei, b64, Encoding.ASCII);

                try
                {
                    File.SetAttributes(AuditLogDatei,
                        FileAttributes.Hidden | FileAttributes.ReadOnly);
                }
                catch { }
            }
            catch { }
        }

        public static void ZeigeAuditLog()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🔒 Scanner-Audit-Log", ConsoleColor.DarkCyan);

            if (!File.Exists(AuditLogDatei))
            {
                ConsoleHelper.PrintInfo("Kein Audit-Log vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            try
            {
                File.SetAttributes(AuditLogDatei,
                    File.GetAttributes(AuditLogDatei) & ~FileAttributes.ReadOnly);
                var zeilen = File.ReadAllLines(AuditLogDatei, Encoding.ASCII);
                File.SetAttributes(AuditLogDatei,
                    FileAttributes.Hidden | FileAttributes.ReadOnly);

                int n = 0;
                foreach (string zeile in zeilen.Reverse().Take(150))
                {
                    try
                    {
                        byte[] bytes = Convert.FromBase64String(zeile.Trim());
                        string klar = AesEntschluesseln(bytes, Schluessel);
                        if (klar == null) continue;

                        var t = klar.Split('|');
                        if (t.Length < 3) continue;

                        string ts = t[0];
                        string typ = t[1];
                        string msg = string.Join("|", t.Skip(2)).Trim();

                        Console.ForegroundColor = typ switch
                        {
                            "KRITISCH" => ConsoleColor.Red,
                            "TAMPER_ALARM" => ConsoleColor.Red,
                            "WARNUNG" => ConsoleColor.Yellow,
                            "SCAN_START" => ConsoleColor.DarkCyan,
                            "SCAN_ENDE" => ConsoleColor.Cyan,
                            _ => ConsoleColor.Gray
                        };
                        Console.WriteLine($"  [{ts}] [{typ,-14}] {msg}");
                        Console.ResetColor();
                        n++;
                    }
                    catch { }
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"  {n} Einträge (letzte 150, neueste zuerst)");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Lesefehler: {ex.Message}");
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════════
        // SCHICHT 6 — DEAD-MAN'S SWITCH
        // ══════════════════════════════════════════════════════════════════

        public static void SchutzPing() => _letzterPing = DateTime.Now;

        public static bool IstSystemIntakt()
        {
            if (!_initialisiert) return false;

            double sek = (DateTime.Now - _letzterPing).TotalSeconds;
            if (sek > MaxPingPause)
            {
                TamperAlarm(
                    $"Dead-Man's Switch: kein Ping seit {sek:F0}s — " +
                    "Scanner-Code möglicherweise deaktiviert.");
                return false;
            }
            return true;
        }

        // ══════════════════════════════════════════════════════════════════
        // REFERENZ-HASH NACH UPDATE AKTUALISIEREN (Admin-Funktion)
        // FIX CS0117: GuardZuruecksetzen → ReferenzHashAktualisieren
        // (war in SystemCommands.cs als "GuardZuruecksetzen" referenziert —
        //  neue korrekte Methode heißt ReferenzHashAktualisieren)
        // ══════════════════════════════════════════════════════════════════

        public static void ReferenzHashAktualisieren()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader(
                "🔑 Referenz-Hash aktualisieren", ConsoleColor.DarkCyan);
            Console.WriteLine();
            ConsoleHelper.PrintWarning("Nur nach legitimen Programm-Updates verwenden!");
            Console.WriteLine();

            // Nur eingeloggter Admin darf das
            var aktuellerBenutzer = DataManager.Benutzer.FirstOrDefault(b =>
                b.Benutzername.Equals(
                    AuthManager.AktuellerBenutzer,
                    StringComparison.OrdinalIgnoreCase));

            if (aktuellerBenutzer == null ||
                aktuellerBenutzer.Berechtigung != Berechtigungen.Admin)
            {
                ConsoleHelper.PrintError(
                    "Nur Administratoren können den Referenz-Hash aktualisieren.");
                AuditSchreiben("WARNUNG",
                    $"Versuchter Guard-Reset ohne Admin-Rechte " +
                    $"durch '{AuthManager.AktuellerBenutzer}'.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            string best = ConsoleHelper.GetInput(
                "Bestätigung: 'UPDATE BESTAETIGEN' eingeben");

            if (!string.Equals(best, "UPDATE BESTAETIGEN", StringComparison.Ordinal))
            {
                ConsoleHelper.PrintInfo("Abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            try
            {
                string pfad = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(pfad) || !File.Exists(pfad))
                {
                    ConsoleHelper.PrintError("Assembly-Pfad nicht gefunden.");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }

                if (File.Exists(ReferenzHashDatei))
                {
                    File.SetAttributes(ReferenzHashDatei, FileAttributes.Normal);
                    File.Delete(ReferenzHashDatei);
                }

                string neuerHash = BerechneHash(pfad);
                SpeichereReferenzHash(neuerHash);
                AuditSchreiben("UPDATE",
                    $"Hash aktualisiert durch '{AuthManager.AktuellerBenutzer}': " +
                    $"{neuerHash.Substring(0, 16)}...");

                ConsoleHelper.PrintSuccess("✓ Referenz-Hash erfolgreich aktualisiert.");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(
                    $"  Neuer Fingerabdruck: {neuerHash.Substring(0, 32)}...");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler: {ex.Message}");
                AuditSchreiben("FEHLER", ex.Message);
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════════
        // TAMPER-ALARM
        // ══════════════════════════════════════════════════════════════════

        private static void TamperAlarm(string grund)
        {
            AuditSchreiben("TAMPER_ALARM", grund);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║   🚨  SICHERHEITSSYSTEM-VERLETZUNG ERKANNT                       ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");

            string gz = $"  ║  {grund}";
            string gzOut = gz.Length > 70
                ? gz.Substring(0, 70) + "… ║"
                : gz.PadRight(70) + " ║";
            Console.WriteLine(gzOut);

            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("  ║  Das Programm wurde möglicherweise manipuliert.                  ║");
            Console.WriteLine("  ║  Alle Operationen wurden gesperrt.                               ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // ── NOTFALL-ZUGANG anbieten ──────────────────────────────────
            // Admins und der Dev (jah) können trotzdem ins System
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Notfall-Zugang verfügbar für Administratoren und Developer.");
            Console.Write("  Notfall-Login versuchen? (j = Ja / beliebige Taste = Beenden): ");
            Console.ResetColor();

            var taste = Console.ReadKey(true);
            Console.WriteLine();

            if (taste.KeyChar == 'j' || taste.KeyChar == 'J')
            {
                bool zugang = NotfallZugang.NotfallLoginAnbieten(grund);
                if (zugang)
                {
                    // Programm läuft weiter — kein Exit
                    _initialisiert = true;
                    return;
                }
            }

            // Kein Zugang gewährt → Programm beenden
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  Programm wird beendet.");
            Console.ResetColor();
            Console.WriteLine("  Drücken Sie eine Taste ...");
            Console.ReadKey(true);
            Environment.Exit(-2);
        }

        // ══════════════════════════════════════════════════════════════════
        // KRYPTOGRAPHIE-HILFSMETHODEN
        // ══════════════════════════════════════════════════════════════════

        private static byte[] DeriveKey(string pw)
        {
            byte[] salt = Encoding.UTF8.GetBytes("ScannerSalt_2026_#Inv!");
            using var k = new Rfc2898DeriveBytes(
                pw, salt, 100_000, HashAlgorithmName.SHA256);
            return k.GetBytes(32);
        }

        // Format: [16 Byte Zufalls-IV][Ciphertext]
        private static byte[] AesVerschluesseln(string text, byte[] key)
        {
            byte[] iv = new byte[16];
            RandomNumberGenerator.Fill(iv);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            ms.Write(iv, 0, 16);
            using (var cs = new CryptoStream(
                ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
                sw.Write(text);

            return ms.ToArray();
        }

        private static string AesEntschluesseln(byte[] daten, byte[] key)
        {
            if (daten == null || daten.Length < 17) return null;
            try
            {
                // FIX CS0029: Range-Operator [..16] / [16..] durch Substring/Array.Copy ersetzen
                byte[] iv = new byte[16];
                byte[] enc = new byte[daten.Length - 16];
                Array.Copy(daten, 0, iv, 0, 16);
                Array.Copy(daten, 16, enc, 0, daten.Length - 16);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var ms = new MemoryStream(enc);
                using var cs = new CryptoStream(
                    ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch { return null; }
        }

        private static string BerechneHash(string pfad)
        {
            using var sha = SHA256.Create();
            using var fs = File.OpenRead(pfad);
            return BitConverter.ToString(sha.ComputeHash(fs))
                .Replace("-", "")
                .ToLowerInvariant();
        }

        public static string RegelVerschluesseln(string muster)
        {
            try
            {
                return Convert.ToBase64String(
                    AesVerschluesseln(muster, Schluessel));
            }
            catch { return string.Empty; }
        }

        private static void Status(bool ok)
        {
            Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(ok ? "OK ✓" : "FEHLGESCHLAGEN ✗");
            Console.ResetColor();
        }
    }
}