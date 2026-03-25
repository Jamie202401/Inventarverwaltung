using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Inventarverwaltung.Security
{
    // ══════════════════════════════════════════════════════════════════════════
    //
    //   DATEI-SICHERHEITS-SCANNER  —  v2.0  (Erweitert)
    //
    //   PRÜFSTUFEN ÜBERSICHT:
    //
    //   STUFE  1 — DATEIEIGENSCHAFTEN
    //              Leer, versteckt, System-Attribut, langer Name
    //
    //   STUFE  2 — DOPPELTE / VERSCHACHTELTE ENDUNGEN
    //              "Rechnung.pdf.exe" → klassischer Spoofing-Angriff
    //
    //   STUFE  3 — GEFÄHRLICHE DATEIENDUNG (Blacklist)
    //              .exe .dll .bat .ps1 .vbs .jar .lnk .iso u.v.m.
    //
    //   STUFE  4 — MAGIC BYTES (Datei-Signatur)
    //              Prüft ob Dateiinhalt zur Endung passt
    //              Eine .pdf die intern ein .exe ist → BLOCKIERT
    //
    //   STUFE  5 — NULL-BYTES IN TEXTDATEIEN
    //              Hinweis auf eingebetteten Binärcode / Exploits
    //
    //   STUFE  6 — INHALTS-SCAN (Script/Makro/Shell-Erkennung)
    //              PowerShell, VBA, eval(), CreateObject, EICAR,
    //              SQL-Injection, XSS, Credential-Hinweise usw.
    //
    //   STUFE  7 — ENTROPIE-ANALYSE
    //              Sehr hohe Entropie = verschlüsselt / gepackter Schadcode
    //              Ransomware & Packer haben fast immer Entropie > 7.5
    //
    //   STUFE  8 — DATEIGRÖSSEN-PLAUSIBILITÄT
    //              Winzige .docx oder .pdf deutet auf Korruption / Falle hin
    //
    //   STUFE  9 — UNICODE-TRICKS / HOMOGLYPHEN / RLO-ANGRIFF
    //              Right-to-Left-Override (U+202E): "Rechnung‮exe.pdf"
    //              erscheint als "Rechnung.pdf" ist aber eine .exe
    //
    //   STUFE 10 — ZEITSTEMPEL-ANOMALIE
    //              Zukunftsdatum oder vor 1980 → manipulierter Timestamp
    //
    //   STUFE 11 — PFAD-TRAVERSAL-SCHUTZ
    //              "../../../windows/system32" → Ausbruchsversuch
    //
    //   STUFE 12 — DATEIDUPLIKAT-ERKENNUNG
    //              Verhindert doppelte Ablage identischer Dateien
    //
    //   ERGEBNIS:
    //     SicherheitsStufe.Sicher      → Datei wird normal gespeichert
    //     SicherheitsStufe.Warnung     → Nutzer wird gewarnt, kann fortfahren
    //     SicherheitsStufe.Gefaehrlich → Datei wird BLOCKIERT, nicht gespeichert
    //
    // ══════════════════════════════════════════════════════════════════════════

    public enum SicherheitsStufe
    {
        Sicher,
        Warnung,
        Gefaehrlich
    }

    public class SicherheitsBefund
    {
        public SicherheitsStufe Stufe { get; set; } = SicherheitsStufe.Sicher;
        public List<string> Warnungen { get; set; } = new List<string>();
        public List<string> Gefahren { get; set; } = new List<string>();
        public List<string> Infos { get; set; } = new List<string>();
        public string DateiName { get; set; } = string.Empty;
        public long DateiGroesse { get; set; }
        public string EchterTyp { get; set; } = "Unbekannt";
        public double Entropie { get; set; }
        public int GepruefteStufen { get; set; }
        public TimeSpan ScanDauer { get; set; }

        public bool IstSicher => Stufe == SicherheitsStufe.Sicher;
        public bool IstGefaehrlich => Stufe == SicherheitsStufe.Gefaehrlich;
    }

    public static class DateiSicherheitsScanner
    {
        // ══════════════════════════════════════════════════════════════════
        // STUFE 3 — GEFÄHRLICHE ENDUNGEN: Blacklist
        // ══════════════════════════════════════════════════════════════════

        private static readonly HashSet<string> GefaehrlicheEndungen = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            // Windows Executables & Loader
            ".exe", ".dll", ".com", ".scr", ".pif", ".msi", ".msp", ".msc",
            ".cpl", ".ocx", ".sys", ".drv", ".vxd",
            // Scripts Windows
            ".bat", ".cmd", ".ps1", ".psm1", ".psd1", ".ps2", ".pssc",
            ".vbs", ".vbe", ".js",  ".jse",  ".wsf", ".wsh", ".hta", ".ws", ".wsc",
            // Registry / Konfiguration
            ".reg", ".inf",
            // Verknüpfungen
            ".lnk", ".url", ".scf",
            // Disk-Images
            ".iso", ".img", ".vhd", ".vhdx", ".vmdk", ".qcow2",
            // Web-Scripts
            ".html", ".htm", ".php", ".asp", ".aspx", ".jsp", ".cgi", ".shtml", ".xhtml",
            // Java / JVM
            ".jar", ".class", ".war", ".ear",
            // Skriptsprachen
            ".py", ".pyc", ".pyo", ".rb", ".pl", ".lua",
            ".sh", ".bash", ".zsh", ".fish", ".ksh", ".csh",
            // Datenbank
            ".sql",
            // .NET Quellcode
            ".cs", ".vb", ".fs",
            // Makro-fähige Office-Formate
            ".xlsm", ".docm", ".pptm", ".xlam", ".dotm", ".potm",
        };

        // ══════════════════════════════════════════════════════════════════
        // STUFE 4 — MAGIC BYTES: Datei-Signaturen
        // ══════════════════════════════════════════════════════════════════

        private static readonly Dictionary<string, (byte[] Bytes, int Offset, string Beschreibung)[]> MagicBytes
            = new Dictionary<string, (byte[], int, string)[]>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = new[] { (new byte[] { 0x25, 0x50, 0x44, 0x46 }, 0, "PDF") },
                [".png"] = new[] { (new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, "PNG") },
                [".jpg"] = new[] { (new byte[] { 0xFF, 0xD8, 0xFF }, 0, "JPEG") },
                [".jpeg"] = new[] { (new byte[] { 0xFF, 0xD8, 0xFF }, 0, "JPEG") },
                [".gif"] = new[]
            {
                (new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, 0, "GIF87a"),
                (new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, 0, "GIF89a")
            },
                [".bmp"] = new[] { (new byte[] { 0x42, 0x4D }, 0, "BMP") },
                [".tiff"] = new[]
            {
                (new byte[] { 0x49, 0x49, 0x2A, 0x00 }, 0, "TIFF (Little-Endian)"),
                (new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, 0, "TIFF (Big-Endian)")
            },
                [".docx"] = new[] { (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, "ZIP/DOCX") },
                [".xlsx"] = new[] { (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, "ZIP/XLSX") },
                [".pptx"] = new[] { (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, "ZIP/PPTX") },
                [".zip"] = new[]
            {
                (new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0, "ZIP"),
                (new byte[] { 0x50, 0x4B, 0x05, 0x06 }, 0, "ZIP (leer)"),
                (new byte[] { 0x50, 0x4B, 0x07, 0x08 }, 0, "ZIP (spanned)")
            },
                [".7z"] = new[] { (new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, 0, "7-ZIP") },
                [".rar"] = new[]
            {
                (new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 }, 0, "RAR v4"),
                (new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01 }, 0, "RAR v5")
            },
                [".doc"] = new[] { (new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, 0, "OLE2/DOC") },
                [".xls"] = new[] { (new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, 0, "OLE2/XLS") },
                [".msg"] = new[] { (new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, 0, "OLE2/MSG") },
                [".mp4"] = new[]
            {
                (new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 }, 0, "MP4"),
                (new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 }, 0, "MP4")
            },
                [".avi"] = new[] { (new byte[] { 0x52, 0x49, 0x46, 0x46 }, 0, "RIFF/AVI") },
                [".mkv"] = new[] { (new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, 0, "MKV/WebM") },
            };

        // ══════════════════════════════════════════════════════════════════
        // STUFE 6 — INHALTS-MUSTER: Script / Makro / Angriffe
        // ══════════════════════════════════════════════════════════════════

        private static readonly (string Muster, string Beschreibung, SicherheitsStufe Stufe)[] InhaltsScanner =
        {
            // ── MAKROS & VBA ─────────────────────────────────────────────
            ("AutoOpen",            "VBA AutoOpen-Makro erkannt",                   SicherheitsStufe.Gefaehrlich),
            ("Auto_Open",           "VBA Auto_Open-Makro erkannt",                  SicherheitsStufe.Gefaehrlich),
            ("Document_Open",       "VBA Document_Open-Makro erkannt",              SicherheitsStufe.Gefaehrlich),
            ("Workbook_Open",       "VBA Workbook_Open-Makro erkannt",              SicherheitsStufe.Gefaehrlich),
            ("Shell(",              "VBA Shell()-Funktion erkannt",                 SicherheitsStufe.Gefaehrlich),
            ("WScript.Shell",       "WScript Shell-Objekt erkannt",                 SicherheitsStufe.Gefaehrlich),
            ("CreateObject",        "COM-Objekt-Erstellung erkannt",                SicherheitsStufe.Gefaehrlich),
            ("GetObject(",          "VBA GetObject() erkannt",                      SicherheitsStufe.Gefaehrlich),
            ("Application.Run",     "VBA Application.Run erkannt",                  SicherheitsStufe.Gefaehrlich),
            ("environ(",            "VBA Environ() (Umgebungsvariablen) erkannt",   SicherheitsStufe.Gefaehrlich),

            // ── POWERSHELL ───────────────────────────────────────────────
            ("powershell",          "PowerShell-Befehl im Dokument",                SicherheitsStufe.Gefaehrlich),
            ("Invoke-Expression",   "PowerShell IEX erkannt",                       SicherheitsStufe.Gefaehrlich),
            ("IEX(",                "PowerShell IEX-Abkürzung erkannt",             SicherheitsStufe.Gefaehrlich),
            ("Invoke-Command",      "PowerShell Remote-Ausführung erkannt",         SicherheitsStufe.Gefaehrlich),
            ("Invoke-WebRequest",   "PowerShell Download erkannt",                  SicherheitsStufe.Gefaehrlich),
            ("DownloadString(",     "PowerShell DownloadString erkannt",            SicherheitsStufe.Gefaehrlich),
            ("DownloadFile(",       "PowerShell DownloadFile erkannt",              SicherheitsStufe.Gefaehrlich),
            ("Start-Process",       "PowerShell Prozessstart erkannt",              SicherheitsStufe.Gefaehrlich),
            ("Net.WebClient",       "PowerShell WebClient erkannt",                 SicherheitsStufe.Gefaehrlich),
            ("-EncodedCommand",     "PowerShell EncodedCommand (Verschleierung)",   SicherheitsStufe.Gefaehrlich),
            ("-enc ",               "PowerShell -enc Kurzform (Verschleierung)",    SicherheitsStufe.Gefaehrlich),
            ("bypass",              "PowerShell ExecutionPolicy Bypass",            SicherheitsStufe.Gefaehrlich),

            // ── SHELL / SYSTEM ───────────────────────────────────────────
            ("cmd.exe",             "CMD.EXE Referenz im Dokument",                 SicherheitsStufe.Gefaehrlich),
            ("/bin/sh",             "Unix Shell-Befehl erkannt",                    SicherheitsStufe.Gefaehrlich),
            ("/bin/bash",           "Bash-Befehl erkannt",                          SicherheitsStufe.Gefaehrlich),
            ("os.system(",          "Python os.system() erkannt",                   SicherheitsStufe.Gefaehrlich),
            ("subprocess.call(",    "Python subprocess erkannt",                    SicherheitsStufe.Gefaehrlich),
            ("exec(",               "Code-Ausführung exec() erkannt",               SicherheitsStufe.Gefaehrlich),
            ("system(",             "System()-Aufruf erkannt",                      SicherheitsStufe.Gefaehrlich),

            // ── WEB / JAVASCRIPT ─────────────────────────────────────────
            ("<script",             "Script-Tag im Dokument erkannt",               SicherheitsStufe.Gefaehrlich),
            ("eval(",               "JavaScript eval() erkannt",                    SicherheitsStufe.Gefaehrlich),
            ("document.write(",     "JavaScript document.write() erkannt",          SicherheitsStufe.Gefaehrlich),
            ("unescape(",           "JavaScript unescape() Verschleierung",         SicherheitsStufe.Gefaehrlich),
            ("fromCharCode(",       "JavaScript fromCharCode Verschleierung",       SicherheitsStufe.Gefaehrlich),
            ("XMLHttpRequest",      "AJAX/XMLHttpRequest erkannt",                  SicherheitsStufe.Gefaehrlich),
            ("ActiveXObject",       "ActiveX-Objekt erkannt",                       SicherheitsStufe.Gefaehrlich),
            ("javascript:",         "JavaScript-Protokoll erkannt",                 SicherheitsStufe.Gefaehrlich),
            ("vbscript:",           "VBScript-Protokoll erkannt",                   SicherheitsStufe.Gefaehrlich),
            ("data:text/html",      "Inline-HTML über Data-URL erkannt",            SicherheitsStufe.Gefaehrlich),
            ("<iframe",             "Versteckter iframe erkannt",                   SicherheitsStufe.Gefaehrlich),

            // ── SQL-INJECTION ─────────────────────────────────────────────
            ("DROP TABLE",          "SQL-Injection: DROP TABLE",                    SicherheitsStufe.Gefaehrlich),
            ("UNION SELECT",        "SQL-Injection: UNION SELECT",                  SicherheitsStufe.Gefaehrlich),
            ("1=1--",               "SQL-Injection: klassisches 1=1--",             SicherheitsStufe.Gefaehrlich),
            ("' OR '1'='1",         "SQL-Injection: OR 1=1 erkannt",               SicherheitsStufe.Gefaehrlich),
            ("xp_cmdshell",         "SQL-Injection: xp_cmdshell (Shell via SQL)",   SicherheitsStufe.Gefaehrlich),

            // ── BEKANNTE MALWARE-SIGNATUREN ──────────────────────────────
            ("EICAR-STANDARD-ANTIVIRUS-TEST-FILE", "EICAR Antivirus-Testmuster",   SicherheitsStufe.Gefaehrlich),
            ("X5O!P%@AP[4\\PZX",    "EICAR Signatur-Fragment erkannt",              SicherheitsStufe.Gefaehrlich),
            ("TVqQAAMAAAAEAAAA",     "Base64-kodierter PE-Header (EXE in Dokument)", SicherheitsStufe.Gefaehrlich),

            // ── WARNUNGEN ─────────────────────────────────────────────────
            ("base64",              "Base64-kodierter Inhalt (häufig bei Malware)", SicherheitsStufe.Warnung),
            ("http://",             "Unverschlüsselte HTTP-URL gefunden",           SicherheitsStufe.Warnung),
            ("ftp://",              "FTP-Link gefunden",                            SicherheitsStufe.Warnung),
            ("\\\\",                "UNC-Netzwerkpfad gefunden",                    SicherheitsStufe.Warnung),
            ("HKEY_",               "Windows-Registry Referenz gefunden",          SicherheitsStufe.Warnung),
            ("%appdata%",           "Windows AppData-Pfad gefunden",               SicherheitsStufe.Warnung),
            ("%temp%",              "Windows TEMP-Pfad gefunden",                  SicherheitsStufe.Warnung),
            ("%systemroot%",        "Windows Systemordner-Referenz gefunden",      SicherheitsStufe.Warnung),
            ("password",            "Klartextpasswort-Hinweis gefunden",           SicherheitsStufe.Warnung),
            ("passwd",              "Passwort-Hinweis gefunden",                   SicherheitsStufe.Warnung),
            ("credentials",         "Zugangsdaten-Hinweis gefunden",               SicherheitsStufe.Warnung),
            ("wget ",               "wget Download-Befehl gefunden",               SicherheitsStufe.Warnung),
            ("curl ",               "curl Download-Befehl gefunden",               SicherheitsStufe.Warnung),
            ("nc -",                "Netcat-Befehl gefunden",                      SicherheitsStufe.Warnung),
            ("nmap",                "Nmap-Netzwerkscanner Referenz gefunden",      SicherheitsStufe.Warnung),
        };

        // Minimale Dateigrößen pro Typ
        private static readonly Dictionary<string, long> MinGroessen =
            new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = 100,
                [".docx"] = 1000,
                [".xlsx"] = 1000,
                [".pptx"] = 1000,
                [".zip"] = 22,
                [".jpg"] = 100,
                [".png"] = 67,
            };

        // Textbasierte Typen für Inhalts-Scan
        private static readonly HashSet<string> ScannbareTextTypen =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".csv", ".xml", ".json", ".log", ".eml", ".ini",
            ".conf", ".yaml", ".yml", ".toml", ".md", ".rtf"
        };

        // Archivtypen mit naturgemäß hoher Entropie (nicht scannen)
        private static readonly HashSet<string> ArchivTypen =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".zip", ".7z", ".rar", ".gz", ".bz2", ".xz",
            ".mp4", ".avi", ".mkv", ".jpg", ".jpeg", ".png", ".gif", ".bmp"
        };

        // ══════════════════════════════════════════════════════════════════
        // HAUPT-SCAN-METHODE
        // ══════════════════════════════════════════════════════════════════

        public static SicherheitsBefund DateiScannen(string pfad,
                                                      string vorhandenerAnhangOrdner = null)
        {
            var startzeit = DateTime.Now;
            var befund = new SicherheitsBefund { DateiName = Path.GetFileName(pfad) };

            if (!File.Exists(pfad))
            {
                befund.Gefahren.Add("Datei existiert nicht oder ist nicht zugänglich.");
                befund.Stufe = SicherheitsStufe.Gefaehrlich;
                return befund;
            }

            var info = new FileInfo(pfad);
            befund.DateiGroesse = info.Length;

            ZeigeScanFortschritt(1, "Dateieigenschaften");
            PruefeEigenschaften(info, befund);

            ZeigeScanFortschritt(2, "Dateinamen-Struktur");
            PruefeDoppelteEndungen(info.Name, befund);

            ZeigeScanFortschritt(3, "Dateiendung-Blacklist");
            PruefeGefaehrlicheEndung(info.Extension, befund);

            ZeigeScanFortschritt(4, "Datei-Signatur (Magic Bytes)");
            PruefeMagicBytes(pfad, info.Extension, befund);

            ZeigeScanFortschritt(5, "Null-Byte Anomalien");
            PruefeNullBytes(pfad, info.Extension, befund);

            ZeigeScanFortschritt(6, "Script / Makro / Schadcode-Muster");
            PruefeInhalt(pfad, info.Extension, befund);

            ZeigeScanFortschritt(7, "Entropie (Packer/Verschlüsselung)");
            PruefeEntropie(pfad, info.Extension, info.Length, befund);

            ZeigeScanFortschritt(8, "Dateigröße-Plausibilität");
            PruefeDateigroesse(info, befund);

            ZeigeScanFortschritt(9, "Unicode-Tricks / RLO-Angriff");
            PruefeUnicodeTricks(info.Name, befund);

            ZeigeScanFortschritt(10, "Zeitstempel-Plausibilität");
            PruefeZeitstempel(info, befund);

            ZeigeScanFortschritt(11, "Pfad-Traversal-Schutz");
            PruefePfadTraversal(pfad, befund);

            ZeigeScanFortschritt(12, "Duplikat-Erkennung");
            if (!string.IsNullOrEmpty(vorhandenerAnhangOrdner))
                PruefeDuplikat(pfad, vorhandenerAnhangOrdner, befund);
            else
                befund.Infos.Add("Duplikat-Prüfung übersprungen.");

            // Fortschrittszeile löschen
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Math.Max(0, Console.WindowWidth - 1)));
            Console.CursorLeft = 0;

            // Gesamtergebnis
            if (befund.Gefahren.Count > 0)
                befund.Stufe = SicherheitsStufe.Gefaehrlich;
            else if (befund.Warnungen.Count > 0)
                befund.Stufe = SicherheitsStufe.Warnung;
            else
                befund.Stufe = SicherheitsStufe.Sicher;

            befund.GepruefteStufen = 12;
            befund.ScanDauer = DateTime.Now - startzeit;

            return befund;
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 1 — DATEIEIGENSCHAFTEN
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeEigenschaften(FileInfo info, SicherheitsBefund befund)
        {
            if (info.Length == 0)
                befund.Warnungen.Add("Datei ist 0 Byte groß (leer oder korrupt).");

            if (info.Name.Length > 200)
                befund.Warnungen.Add(
                    $"Sehr langer Dateiname ({info.Name.Length} Zeichen) — " +
                    "möglicher Verschleierungsversuch.");

            if ((info.Attributes & FileAttributes.Hidden) != 0)
                befund.Warnungen.Add("Datei hat das 'Versteckt'-Attribut gesetzt.");

            if ((info.Attributes & FileAttributes.System) != 0)
                befund.Gefahren.Add("Datei hat das 'System'-Attribut — für Anhänge unzulässig.");
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 2 — DOPPELTE ENDUNGEN
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeDoppelteEndungen(string dateiname, SicherheitsBefund befund)
        {
            var teile = dateiname.Split('.');
            if (teile.Length <= 2) return;

            for (int i = 1; i < teile.Length - 1; i++)
            {
                string versteckt = "." + teile[i];
                if (GefaehrlicheEndungen.Contains(versteckt))
                    befund.Gefahren.Add(
                        $"DOPPELTE ENDUNG: Versteckte gefährliche Endung '{versteckt}' " +
                        "erkannt — klassischer Datei-Spoofing-Angriff!");
            }

            if (teile.Length > 3)
                befund.Warnungen.Add(
                    $"Ungewöhnlich viele Datei-Endungen ({teile.Length - 1}) im Dateinamen.");
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 3 — GEFÄHRLICHE ENDUNG
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeGefaehrlicheEndung(string endung, SicherheitsBefund befund)
        {
            if (GefaehrlicheEndungen.Contains(endung))
                befund.Gefahren.Add(
                    $"Dateiendung '{endung}' ist als gefährlich klassifiziert " +
                    "(ausführbare Datei, Script oder Web-Datei).");
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 4 — MAGIC BYTES
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeMagicBytes(string pfad, string endung, SicherheitsBefund befund)
        {
            try
            {
                byte[] header = new byte[32];
                using var fs = File.OpenRead(pfad);
                int gelesen = fs.Read(header, 0, header.Length);

                if (gelesen < 2) { befund.Warnungen.Add("Datei zu klein für Signaturprüfung."); return; }

                // Sofort kritische Typen erkennen (EXE, ELF, Script)
                string kritisch = ErkennKritischenTyp(header, gelesen);
                if (kritisch != null)
                {
                    befund.EchterTyp = kritisch;
                    befund.Gefahren.Add(
                        $"EXTENSION-SPOOFING: Datei ist intern '{kritisch}' " +
                        $"aber hat Endung '{endung}' — höchstwahrscheinlich Malware!");
                    return;
                }

                if (!MagicBytes.TryGetValue(endung, out var signaturen))
                {
                    befund.EchterTyp = "Textdatei / kein Binär-Eintrag";
                    return;
                }

                bool gefunden = false;
                foreach (var (bytes, offset, beschreibung) in signaturen)
                {
                    if (offset + bytes.Length > gelesen) continue;
                    bool passt = true;
                    for (int i = 0; i < bytes.Length; i++)
                        if (header[offset + i] != bytes[i]) { passt = false; break; }
                    if (passt) { gefunden = true; befund.EchterTyp = beschreibung; break; }
                }

                if (!gefunden)
                {
                    string erkannt = ErkenneTypAusMagicBytes(header, gelesen);
                    befund.EchterTyp = erkannt;
                    befund.Gefahren.Add(
                        $"EXTENSION-SPOOFING: '{endung}' passt nicht zur internen Signatur. " +
                        $"Erkannter echter Typ: {erkannt}");
                }
            }
            catch (Exception ex) { befund.Warnungen.Add($"Magic-Byte-Prüfung fehlgeschlagen: {ex.Message}"); }
        }

        private static string ErkennKritischenTyp(byte[] h, int n)
        {
            if (n >= 2 && h[0] == 0x4D && h[1] == 0x5A)
                return "Windows EXE/DLL (MZ-Header!) — AUSFÜHRBARE DATEI";
            if (n >= 4 && h[0] == 0x7F && h[1] == 0x45 && h[2] == 0x4C && h[3] == 0x46)
                return "Linux ELF Executable — AUSFÜHRBARE DATEI";
            if (n >= 2 && h[0] == 0x23 && h[1] == 0x21)
                return "Script mit Shebang (#!) — SCRIPT-DATEI";
            if (n >= 4 && h[0] == 0xCA && h[1] == 0xFE && h[2] == 0xBA && h[3] == 0xBE)
                return "Java Class / Mach-O Binary";
            if (n >= 4 && h[0] == 0xCE && h[1] == 0xFA && h[2] == 0xED && h[3] == 0xFE)
                return "macOS Mach-O Executable";
            return null;
        }

        private static string ErkenneTypAusMagicBytes(byte[] h, int n)
        {
            if (n >= 4 && h[0] == 0x25 && h[1] == 0x50 && h[2] == 0x44 && h[3] == 0x46) return "PDF";
            if (n >= 4 && h[0] == 0x50 && h[1] == 0x4B && h[2] == 0x03 && h[3] == 0x04) return "ZIP-Archiv";
            if (n >= 8 && h[0] == 0xD0 && h[1] == 0xCF && h[2] == 0x11 && h[3] == 0xE0) return "OLE2 Office";
            if (n >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return "JPEG-Bild";
            if (n >= 4 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return "PNG-Bild";
            return "Unbekannte Signatur";
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 5 — NULL-BYTES
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeNullBytes(string pfad, string endung, SicherheitsBefund befund)
        {
            if (!ScannbareTextTypen.Contains(endung)) return;
            try
            {
                byte[] inhalt = File.ReadAllBytes(pfad);
                int nullCount = inhalt.Count(b => b == 0x00);
                if (nullCount > 0)
                    befund.Warnungen.Add(
                        $"Null-Bytes in Textdatei ({nullCount} Stück) — " +
                        "Hinweis auf eingebetteten Binärcode oder Exploits.");
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 6 — INHALTS-SCAN
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeInhalt(string pfad, string endung, SicherheitsBefund befund)
        {
            if (!ScannbareTextTypen.Contains(endung)) return;
            try
            {
                using var fs = new FileStream(pfad, FileMode.Open, FileAccess.Read);
                int max = (int)Math.Min(fs.Length, 2 * 1024 * 1024); // max 2 MB
                byte[] buffer = new byte[max];
                fs.Read(buffer, 0, max);
                string lower = Encoding.UTF8.GetString(buffer).ToLowerInvariant();

                foreach (var (muster, beschreibung, stufe) in InhaltsScanner)
                {
                    if (!lower.Contains(muster.ToLowerInvariant())) continue;
                    if (stufe == SicherheitsStufe.Gefaehrlich)
                        befund.Gefahren.Add($"Inhalt: {beschreibung}");
                    else
                        befund.Warnungen.Add($"Inhalt: {beschreibung}");
                }
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 7 — ENTROPIE-ANALYSE
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeEntropie(string pfad, string endung, long groesse,
                                            SicherheitsBefund befund)
        {
            if (ArchivTypen.Contains(endung)) return;
            if (groesse < 512) return;

            try
            {
                int sampleSize = (int)Math.Min(groesse, 512 * 1024);
                byte[] buf = new byte[sampleSize];
                using var fs = File.OpenRead(pfad);
                fs.Read(buf, 0, sampleSize);

                long[] freq = new long[256];
                foreach (byte b in buf) freq[b]++;

                double entropie = 0.0;
                double total = sampleSize;
                for (int i = 0; i < 256; i++)
                {
                    if (freq[i] == 0) continue;
                    double p = freq[i] / total;
                    entropie -= p * Math.Log(p, 2);
                }

                befund.Entropie = entropie;

                if (entropie > 7.8)
                    befund.Gefahren.Add(
                        $"ENTROPIE KRITISCH: {entropie:F2} bit/byte — " +
                        "Extrem zufällige Daten. Typisch für Ransomware-Payloads " +
                        "oder gepackten/verschlüsselten Schadcode.");
                else if (entropie > 7.2)
                    befund.Warnungen.Add(
                        $"ENTROPIE HOCH: {entropie:F2} bit/byte — " +
                        "Datei wirkt verschlüsselt oder komprimiert. " +
                        "Bei Textdateien/PDFs ungewöhnlich.");
                else
                    befund.Infos.Add($"Entropie: {entropie:F2} bit/byte (unauffällig)");
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 8 — DATEIGRÖSSEN-PLAUSIBILITÄT
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeDateigroesse(FileInfo info, SicherheitsBefund befund)
        {
            if (!MinGroessen.TryGetValue(info.Extension, out long minGroesse)) return;
            if (info.Length > 0 && info.Length < minGroesse)
                befund.Warnungen.Add(
                    $"Dateigröße ({info.Length} Byte) ungewöhnlich klein für " +
                    $"'{info.Extension}' (erwartet: ≥{minGroesse} Byte) — " +
                    "könnte korrupt oder eine Falle sein.");
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 9 — UNICODE-TRICKS / RLO-ANGRIFF
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeUnicodeTricks(string dateiname, SicherheitsBefund befund)
        {
            // Right-to-Left Override — gefährlichster Trick
            if (dateiname.Contains('\u202E'))
                befund.Gefahren.Add(
                    "RLO-ANGRIFF: Right-to-Left-Override (U+202E) im Dateinamen — " +
                    "der angezeigte Name ist irreführend, echter Typ verschleiert!");

            if (dateiname.Contains('\u202B'))
                befund.Gefahren.Add(
                    "Unicode-Trick: Right-to-Left-Embedding (U+202B) erkannt.");

            // Zero-Width Chars (unsichtbar)
            if (dateiname.Contains('\u200B'))
                befund.Warnungen.Add("Unsichtbares Zeichen: Zero-Width-Space (U+200B) im Namen.");

            if (dateiname.Contains('\u200C') || dateiname.Contains('\u200D'))
                befund.Warnungen.Add("Unsichtbares Zeichen: Zero-Width-Joiner im Namen.");

            if (dateiname.Contains('\u00AD'))
                befund.Warnungen.Add("Unsichtbares Zeichen: Soft-Hyphen (U+00AD) im Namen.");

            // Nicht-ASCII Buchstaben (mögliche Homoglyphen)
            bool homoglyphe = dateiname.Any(c =>
                c > 127 && !char.IsHighSurrogate(c) &&
                !char.IsLowSurrogate(c) && char.IsLetter(c));

            if (homoglyphe)
                befund.Warnungen.Add(
                    "Nicht-ASCII-Buchstaben im Dateinamen — mögliche Homoglyphen " +
                    "(fremdsprachige Zeichen die lateinischen ähneln).");
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 10 — ZEITSTEMPEL-ANOMALIE
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeZeitstempel(FileInfo info, SicherheitsBefund befund)
        {
            DateTime jetzt = DateTime.Now;
            DateTime zukunft = jetzt.AddHours(1);
            DateTime alt1990 = new DateTime(1990, 1, 1);
            DateTime alt1980 = new DateTime(1980, 1, 1);

            if (info.CreationTime > zukunft)
                befund.Warnungen.Add(
                    $"Erstellungsdatum in der Zukunft ({info.CreationTime:dd.MM.yyyy HH:mm}) " +
                    "— Timestamp könnte manipuliert sein.");

            if (info.LastWriteTime > zukunft)
                befund.Warnungen.Add(
                    $"Änderungsdatum in der Zukunft ({info.LastWriteTime:dd.MM.yyyy HH:mm}) " +
                    "— Timestamp könnte manipuliert sein.");

            if (info.CreationTime < alt1990 && info.CreationTime.Year > 1)
                befund.Warnungen.Add(
                    $"Erstellungsdatum sehr alt ({info.CreationTime:dd.MM.yyyy}) " +
                    "— erscheint manipuliert.");

            if (info.LastWriteTime < alt1980 && info.LastWriteTime.Year > 1)
                befund.Gefahren.Add(
                    $"ZEITSTEMPEL KRITISCH: Datum vor 1980 ({info.LastWriteTime:dd.MM.yyyy}) " +
                    "— bei Windows-Dateisystemen physisch unmöglich, Timestamp manipuliert!");
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 11 — PFAD-TRAVERSAL-SCHUTZ
        // ══════════════════════════════════════════════════════════════════

        private static void PruefePfadTraversal(string pfad, SicherheitsBefund befund)
        {
            string dateiname = Path.GetFileName(pfad);

            foreach (string muster in new[] { "..", "..\\", "../", "%2e%2e", "%2f", "%5c" })
            {
                if (dateiname.Contains(muster))
                {
                    befund.Gefahren.Add(
                        $"PFAD-TRAVERSAL: Verdächtiges Muster '{muster}' im Dateinamen — " +
                        "Versuch aus dem Zielordner auszubrechen!");
                    break;
                }
            }

            // Laufwerksbuchstabe im Dateinamen
            if (dateiname.Length >= 2 && dateiname[1] == ':' &&
                char.IsLetter(dateiname[0]))
                befund.Gefahren.Add(
                    "PFAD-TRAVERSAL: Laufwerksbuchstabe im Dateinamen erkannt.");

            // Aufgelösten Pfad prüfen
            try
            {
                string aufgeloest = Path.GetFullPath(pfad);
                string basis = Path.GetFullPath(
                    Path.GetDirectoryName(pfad) ?? Directory.GetCurrentDirectory());

                if (!aufgeloest.StartsWith(basis, StringComparison.OrdinalIgnoreCase))
                    befund.Gefahren.Add(
                        "PFAD-TRAVERSAL: Aufgelöster Pfad verlässt Ausgangsverzeichnis!");
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════
        // STUFE 12 — DUPLIKAT-ERKENNUNG
        // ══════════════════════════════════════════════════════════════════

        private static void PruefeDuplikat(string pfad, string zielOrdner,
                                            SicherheitsBefund befund)
        {
            if (!Directory.Exists(zielOrdner)) return;
            try
            {
                string neuerHash = BerechneHashSchnell(pfad);

                foreach (string vorhandene in Directory.GetFiles(zielOrdner))
                {
                    if (Path.GetFileName(vorhandene).StartsWith("_")) continue;
                    try
                    {
                        if (BerechneHashSchnell(vorhandene) == neuerHash)
                        {
                            befund.Warnungen.Add(
                                $"DUPLIKAT: Identisch mit '{Path.GetFileName(vorhandene)}' " +
                                "im Zielordner — Doppelablage verhindert.");
                            return;
                        }
                    }
                    catch { }
                }
                befund.Infos.Add("Kein Duplikat gefunden.");
            }
            catch { }
        }

        private static string BerechneHashSchnell(string pfad)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            using var fs = File.OpenRead(pfad);
            return BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "");
        }

        // ══════════════════════════════════════════════════════════════════
        // FORTSCHRITTSANZEIGE
        // ══════════════════════════════════════════════════════════════════

        private static void ZeigeScanFortschritt(int stufe, string beschreibung)
        {
            int breite = 20;
            int voll = (int)Math.Round((double)stufe / 12 * breite);
            string bar = "[" + new string('█', voll) + new string('░', breite - voll) + "]";
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"\r  🔍 Stufe {stufe,2}/12 {bar} {beschreibung,-42}");
            Console.ResetColor();
        }

        // ══════════════════════════════════════════════════════════════════
        // AUSGABE-METHODEN
        // ══════════════════════════════════════════════════════════════════

        public static bool BefundAnzeigenUndEntscheiden(SicherheitsBefund befund)
        {
            Console.WriteLine();
            Console.WriteLine();

            switch (befund.Stufe)
            {
                case SicherheitsStufe.Sicher:
                    ZeigeSicherheitsOK(befund);
                    return true;

                case SicherheitsStufe.Warnung:
                    ZeigeWarnung(befund);
                    return FrageNutzer();

                case SicherheitsStufe.Gefaehrlich:
                    ZeigeGefahr(befund);
                    LogManager.LogFehler("Sicherheitsscanner",
                        $"BLOCKIERT [{befund.DateiName}] | " +
                        string.Join(" | ", befund.Gefahren));
                    return false;

                default:
                    return false;
            }
        }

        private static void ZeigeSicherheitsOK(SicherheitsBefund befund)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║  🛡️  SICHERHEITSCHECK BESTANDEN — ALLE 12 STUFEN UNAUFFÄLLIG      ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  ✓ Datei:           {befund.DateiName}");
            Console.WriteLine($"  ✓ Erkannter Typ:   {befund.EchterTyp}");
            Console.WriteLine($"  ✓ Entropie:        {befund.Entropie:F2} bit/byte");
            Console.WriteLine($"  ✓ Stufen geprüft:  {befund.GepruefteStufen}/12");
            Console.WriteLine($"  ✓ Scandauer:       {befund.ScanDauer.TotalMilliseconds:F0} ms");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeWarnung(SicherheitsBefund befund)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║  ⚠️   SICHERHEITSWARNUNG                                           ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"  ║  Datei:    {PadRight(befund.DateiName, 56)} ║");
            Console.WriteLine($"  ║  Entropie: {befund.Entropie:F2}  |  Stufen: {befund.GepruefteStufen}/12  |  {befund.ScanDauer.TotalMilliseconds:F0} ms                ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < befund.Warnungen.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  {i + 1,2}. ⚠  {befund.Warnungen[i]}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  Die Datei kann potenziell Risiken enthalten.");
            Console.WriteLine("  Fortfahren auf eigene Verantwortung.");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeGefahr(SicherheitsBefund befund)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║   🚨  SICHERHEITSGEFAHR — DATEI WURDE BLOCKIERT                  ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"  ║  Datei:      {PadRight(befund.DateiName, 53)} ║");
            Console.WriteLine($"  ║  Echter Typ: {PadRight(befund.EchterTyp, 53)} ║");
            Console.WriteLine($"  ║  Entropie:   {befund.Entropie:F2}  |  Stufen: {befund.GepruefteStufen}/12  |  {befund.ScanDauer.TotalMilliseconds:F0} ms              ║");
            Console.WriteLine("  ╠═══ 🚨 KRITISCHE BEFUNDE ══════════════════════════════════════════╣");
            Console.ResetColor();

            for (int i = 0; i < befund.Gefahren.Count; i++)
            {
                string zeile = $"  ║  {i + 1}. {befund.Gefahren[i]}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(zeile.Length > 71 ? zeile.Substring(0, 70) + "…" : zeile);
                Console.ResetColor();
            }

            if (befund.Warnungen.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ╠═══ ⚠️  ZUSÄTZLICHE WARNUNGEN ════════════════════════════════════╣");
                foreach (var w in befund.Warnungen)
                {
                    string z = $"  ║     {w}";
                    Console.WriteLine(z.Length > 71 ? z.Substring(0, 70) + "…" : z);
                }
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ╠═══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("  ║  ✗  DATEI WURDE NICHT GESPEICHERT                                ║");
            Console.WriteLine("  ║  ✗  VORFALL WURDE IM SYSTEM-LOG PROTOKOLLIERT                    ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static bool FrageNutzer()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  Trotzdem fortfahren? (j = Ja, eigene Verantwortung / n = Nein): ");
            Console.ResetColor();
            string a = Console.ReadLine()?.Trim().ToLower() ?? "n";
            Console.WriteLine();
            return a == "j" || a == "ja";
        }

        private static string PadRight(string text, int len)
        {
            if (text == null) return new string(' ', len);
            if (text.Length >= len) return text.Substring(0, len);
            return text.PadRight(len);
        }
    }
}