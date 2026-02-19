using System;
using System.Collections.Generic;
using System.IO;

namespace Inventarverwaltung.Manager.UI
{
    /// <summary>
    /// Verwaltet alle Dateipfade mit strukturierter Ordnerorganisation.
    ///
    /// Ordnerstruktur:
    ///   Data\        → Inventar.txt, Mitarbeiter.txt, Accounts.txt, Rollen.txt
    ///   Logs\        → System_Log.enc, Tagesreport*.enc
    ///   Config\      → DruckEinstellungen.txt, KI_Config.dat, KI_Stats.dat, artikel_templates.txt
    ///   Exports\     → *.csv, *.txt (Exporte)
    ///   Backup\      → *.bak, *.backup
    ///
    /// Alle Pfade laufen zentral über diese Klasse.
    /// Beim Start werden bestehende Dateien automatisch in die richtigen Ordner verschoben.
    /// </summary>
    /// 

    public static class FileManager
    {
        // ─────────────────────────────────────────────────────────────────────
        // ORDNER
        // ─────────────────────────────────────────────────────────────────────

        public static readonly string BasisVerzeichnis = Environment.CurrentDirectory;
        public static readonly string DataVerzeichnis = Path.Combine(BasisVerzeichnis, "Data");
        public static readonly string LogsVerzeichnis = Path.Combine(BasisVerzeichnis, "Logs");
        public static readonly string ConfigVerzeichnis = Path.Combine(BasisVerzeichnis, "Config");
        public static readonly string ExportsVerzeichnis = Path.Combine(BasisVerzeichnis, "Exports");
        public static readonly string BackupVerzeichnis = Path.Combine(BasisVerzeichnis, "Backup");

        // ─────────────────────────────────────────────────────────────────────
        // DATEIPFADE — DATA
        // ─────────────────────────────────────────────────────────────────────

        public static string FilePath => Path.Combine(DataVerzeichnis, "Inventar.txt");      // Inventardaten
        public static string FilePath2 => Path.Combine(DataVerzeichnis, "Mitarbeiter.txt");   // Mitarbeiterdaten
        public static string FilePath3 => Path.Combine(DataVerzeichnis, "Accounts.txt");      // Benutzerdaten
        public static string RollenPfad => Path.Combine(DataVerzeichnis, "Rollen.txt");       // Rollendaten

        // ─────────────────────────────────────────────────────────────────────
        // DATEIPFADE — LOGS
        // ─────────────────────────────────────────────────────────────────────

        public static string LogPfad => Path.Combine(LogsVerzeichnis, "System_Log.enc");
        public static string ReportVerzeichnis => LogsVerzeichnis;  // Reports landen ebenfalls in Logs\

        // ─────────────────────────────────────────────────────────────────────
        // DATEIPFADE — CONFIG
        // ─────────────────────────────────────────────────────────────────────

        public static string DruckEinstellungenPfad => Path.Combine(ConfigVerzeichnis, "DruckEinstellungen.txt");
        public static string KIConfigPfad => Path.Combine(ConfigVerzeichnis, "KI_Config.dat");
        public static string KIStatsPfad => Path.Combine(ConfigVerzeichnis, "KI_Stats.dat");
        public static string ArtikelTemplatesPfad => Path.Combine(ConfigVerzeichnis, "artikel_templates.txt");

        // ─────────────────────────────────────────────────────────────────────
        // DATEIPFADE — EXPORTS & DRUCKHISTORIE
        // ─────────────────────────────────────────────────────────────────────

        public static string DruckHistoriePfad => Path.Combine(ExportsVerzeichnis, "DruckHistorie.json");

        // ─────────────────────────────────────────────────────────────────────
        // INIT — Ordner anlegen + Dateien sortieren
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Erstellt alle Ordner und verschiebt vorhandene Dateien in die richtigen Ordner.
        /// Wird einmalig beim Programmstart aufgerufen.
        /// </summary>
        public static void InitializeFiles()
        {
            // 1. Alle Ordner anlegen
            ErstelleOrdner(DataVerzeichnis);
            ErstelleOrdner(LogsVerzeichnis);
            ErstelleOrdner(ConfigVerzeichnis);
            ErstelleOrdner(ExportsVerzeichnis);
            ErstelleOrdner(BackupVerzeichnis);

            // 2. Bestehende Dateien sortieren
            SortiereDateien();

            // 3. Pflichtdateien anlegen
            SichereDatei(FilePath);
            SichereDatei(FilePath2);
            SichereDatei(FilePath3);
        }

        /// <summary>
        /// Verschiebt alle Dateien im Basisverzeichnis in ihre korrekten Unterordner.
        /// Dateien die bereits im richtigen Ordner sind werden nicht bewegt.
        /// </summary>
        public static void SortiereDateien()
        {
            // Mapping: Dateiname (oder Muster) → Zielordner
            var regeln = new List<(Func<string, bool> Passt, string Ziel)>
            {
                // DATA
                (f => f.Equals("Inventar.txt",    StringComparison.OrdinalIgnoreCase), DataVerzeichnis),
                (f => f.Equals("Mitarbeiter.txt", StringComparison.OrdinalIgnoreCase), DataVerzeichnis),
                (f => f.Equals("Accounts.txt",    StringComparison.OrdinalIgnoreCase), DataVerzeichnis),
                (f => f.Equals("Rollen.txt",      StringComparison.OrdinalIgnoreCase), DataVerzeichnis),

                // LOGS
                (f => f.Equals("System_Log.enc",  StringComparison.OrdinalIgnoreCase), LogsVerzeichnis),
                (f => f.EndsWith(".enc",           StringComparison.OrdinalIgnoreCase), LogsVerzeichnis),

                // CONFIG
                (f => f.Equals("DruckEinstellungen.txt", StringComparison.OrdinalIgnoreCase), ConfigVerzeichnis),
                (f => f.Equals("KI_Config.dat",          StringComparison.OrdinalIgnoreCase), ConfigVerzeichnis),
                (f => f.Equals("KI_Stats.dat",           StringComparison.OrdinalIgnoreCase), ConfigVerzeichnis),
                (f => f.Equals("artikel_templates.txt",  StringComparison.OrdinalIgnoreCase), ConfigVerzeichnis),
                (f => f.EndsWith(".dat",                 StringComparison.OrdinalIgnoreCase), ConfigVerzeichnis),

                // EXPORTS & DRUCKHISTORIE
                (f => f.Equals("DruckHistorie.json", StringComparison.OrdinalIgnoreCase), ExportsVerzeichnis),
                (f => f.EndsWith(".json",            StringComparison.OrdinalIgnoreCase), ExportsVerzeichnis),
                (f => f.EndsWith(".csv",             StringComparison.OrdinalIgnoreCase), ExportsVerzeichnis),

                // BACKUP
                (f => f.EndsWith(".bak",    StringComparison.OrdinalIgnoreCase), BackupVerzeichnis),
                (f => f.EndsWith(".backup", StringComparison.OrdinalIgnoreCase), BackupVerzeichnis),
            };

            // Alle Dateien direkt im Basisverzeichnis prüfen
            try
            {
                foreach (string dateiPfad in Directory.GetFiles(BasisVerzeichnis, "*", SearchOption.TopDirectoryOnly))
                {
                    string dateiName = Path.GetFileName(dateiPfad);

                    // Eigene Exe/dll/config nicht anfassen
                    string ext = Path.GetExtension(dateiName).ToLower();
                    if (ext == ".exe" || ext == ".dll" || ext == ".config" ||
                        ext == ".pdb" || ext == ".runtimeconfig")
                        dateiName.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase);
                            dateiName.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase);
                        continue;

                    // Passendes Ziel suchen
                    foreach (var (passt, ziel) in regeln)
                    {
                        if (!passt(dateiName)) continue;

                        string zielPfad = Path.Combine(ziel, dateiName);

                        // Schon im richtigen Ordner?
                        if (string.Equals(Path.GetFullPath(dateiPfad),
                                          Path.GetFullPath(zielPfad),
                                          StringComparison.OrdinalIgnoreCase))
                            break;

                        VerschiebeDate(dateiPfad, zielPfad);
                        break;
                    }
                }
            }
            catch
            {
                // Stiller Fehler — Sortierung ist optional
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // HILFSMETHODEN
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Legt einen Ordner an (inkl. alle Elternordner), falls nicht vorhanden.
        /// </summary>
        public static void ErstelleOrdner(string pfad)
        {
            try
            {
                if (!Directory.Exists(pfad))
                    Directory.CreateDirectory(pfad);
            }
            catch { /* ignorieren */ }
        }

        /// <summary>
        /// Erstellt eine leere Datei falls sie noch nicht existiert.
        /// Entfernt Hidden/ReadOnly-Attribute.
        /// </summary>
        public static void SichereDatei(string pfad)
        {
            try
            {
                ErstelleOrdner(Path.GetDirectoryName(pfad));

                if (!File.Exists(pfad))
                    File.Create(pfad).Close();

                BereinigeAttribute(pfad);
            }
            catch { }
        }

        /// <summary>
        /// Verschiebt eine Datei. Bei Konflikt (Ziel existiert) wird zusammengeführt/überschrieben.
        /// </summary>
        private static void VerschiebeDate(string quelle, string ziel)
        {
            try
            {
                if (File.Exists(ziel))
                {
                    // Konflikt: Quelldatei ist neuer → überschreiben
                    var qInfo = new FileInfo(quelle);
                    var zInfo = new FileInfo(ziel);

                    if (qInfo.LastWriteTime > zInfo.LastWriteTime)
                    {
                        BereinigeAttribute(ziel);
                        File.Delete(ziel);
                        File.Move(quelle, ziel);
                    }
                    else
                    {
                        // Ziel ist aktueller → Quelle als Backup oder löschen
                        File.Delete(quelle);
                    }
                }
                else
                {
                    File.Move(quelle, ziel);
                }

                BereinigeAttribute(ziel);
            }
            catch { }
        }

        /// <summary>
        /// Entfernt Hidden/ReadOnly/System-Attribute von einer Datei.
        /// </summary>
        public static void BereinigeAttribute(string pfad)
        {
            try
            {
                if (!File.Exists(pfad)) return;
                var attr = File.GetAttributes(pfad);
                attr &= ~FileAttributes.Hidden;
                attr &= ~FileAttributes.ReadOnly;
                attr &= ~FileAttributes.System;
                File.SetAttributes(pfad, attr);
            }
            catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // LEGACY-KOMPATIBILITÄT
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Legacy: wird von altem Code aufgerufen → leitet auf InitializeFiles weiter</summary>
        public static void HideAllFiles() => InitializeFiles();

        /// <summary>Legacy: repariert Attribute aller bekannten Dateien</summary>
        public static void FixFileAttributes()
        {
            BereinigeAttribute(FilePath);
            BereinigeAttribute(FilePath2);
            BereinigeAttribute(FilePath3);
            BereinigeAttribute(LogPfad);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ORDNER-ÜBERSICHT (für Debug/System-Anzeige)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gibt eine strukturierte Übersicht aller Ordner und deren Dateianzahl zurück.
        /// </summary>
        public static void ZeigeOrdnerStruktur()
        {
            var ordner = new[]
            {
                ("📂 Data",    DataVerzeichnis,    "Inventar, Mitarbeiter, Accounts, Rollen"),
                ("📋 Logs",    LogsVerzeichnis,    "System-Log, Tagesreporte"),
                ("⚙️  Config",  ConfigVerzeichnis,  "Drucker, KI-Config, Templates"),
                ("📤 Exports", ExportsVerzeichnis, "CSV-Exporte, Druckhistorie (JSON)"),
                ("💾 Backup",  BackupVerzeichnis,  "Sicherungen (.bak)"),
            };

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ║   DATEI-STRUKTUR                                                  ║");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            foreach (var (name, pfad, beschreibung) in ordner)
            {
                int anzahl = Directory.Exists(pfad) ? Directory.GetFiles(pfad).Length : 0;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"\n  {name}\\");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"   ({anzahl} Datei{(anzahl != 1 ? "en" : "")})");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  │  {beschreibung}");

                if (Directory.Exists(pfad))
                {
                    foreach (string f in Directory.GetFiles(pfad))
                    {
                        var info = new FileInfo(f);
                        string groesse = info.Length < 1024
                            ? $"{info.Length} B"
                            : $"{info.Length / 1024.0:F1} KB";

                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("  │  ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"  {Path.GetFileName(f),-35}");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"{groesse,8}   {info.LastWriteTime:dd.MM.yyyy HH:mm}");
                    }
                }
                Console.ResetColor();
            }

            Console.WriteLine();
        }
    }
}