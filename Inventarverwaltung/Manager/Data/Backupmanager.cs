using Inventarverwaltung.Manager.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Inventarverwaltung.Manager.UI
{
    /// <summary>
    /// Erstellt automatisch alle 2 Wochen verschlüsselte Backups aller Datendateien.
    ///
    /// Ablauf:
    ///   1. Beim Programmstart prüfen ob ein Backup fällig ist (letzte Sicherung > 14 Tage)
    ///   2. Alle relevanten Dateien in ein ZIP-Archiv bündeln (im Speicher)
    ///   3. ZIP-Inhalt mit AES-256 verschlüsseln
    ///   4. Als .bkp-Datei im Backup-Ordner speichern
    ///   5. Alte Backups bereinigen (max. 5 behalten)
    ///
    /// Die .bkp-Dateien sind binär verschlüsselt und für Dritte nicht lesbar.
    /// </summary>
    public static class BackupManager
    {
        // ─────────────────────────────────────────────────────────────────────
        // KONSTANTEN
        // ─────────────────────────────────────────────────────────────────────

        private const int BackupIntervallTage = 14;
        private const int MaxBackups = 5;
        private const string BackupDateiendung = ".bkp";
        private const string ZeitmarkenDatei = "backup_meta.dat";
        private const string BackupPrefix = "INV_Backup_";
        private const string MasterHashDatei = "backup_master.key";
        private const int MaxMasterVersuche = 3;

        // AES-256 Schlüssel-Ableitung — eigener Salt für Backups
        private static readonly byte[] BackupKey = LeiteSchluesselAb(
            "InvBackup#AES256!Secure2026$Key", "InvBackupSalt!2026#");


        // ─────────────────────────────────────────────────────────────────────
        // MASTER-PASSWORT SCHUTZ
        // ─────────────────────────────────────────────────────────────────────

        private static string HashMasterPasswort(string passwort)
        {
            const string masterSalt = "InvMasterBackup!Salt#2026$Secure";
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(masterSalt + passwort));
                return Convert.ToBase64String(bytes);
            }
        }

        private static bool MasterPasswortPruefung()
        {
            string hashPfad = Path.Combine(FileManager.ConfigVerzeichnis, MasterHashDatei);

            if (!File.Exists(hashPfad) || new FileInfo(hashPfad).Length == 0)
                return MasterPasswortEinrichten(hashPfad);

            ZeigeMasterPasswortHeader();

            for (int versuch = 1; versuch <= MaxMasterVersuche; versuch++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  🔐  Master-Passwort [{versuch}/{MaxMasterVersuche}]: ");
                Console.ResetColor();

                string eingabe = AuthManager.PasswortEingabe();
                Console.WriteLine();

                string eingegebenerHash = HashMasterPasswort(eingabe);
                string gespeicherterHash = File.ReadAllText(hashPfad).Trim();

                if (eingegebenerHash.Equals(gespeicherterHash, StringComparison.Ordinal))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ✅  Zugang gewährt.");
                    Console.ResetColor();
                    Thread.Sleep(700);
                    return true;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ✗  Falsches Master-Passwort.");
                Console.ResetColor();

                if (versuch < MaxMasterVersuche)
                    Thread.Sleep(800);
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║   🔒  ZUGANG VERWEIGERT — Zu viele Fehlversuche             ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Thread.Sleep(1500);
            return false;
        }

        private static bool MasterPasswortEinrichten(string hashPfad)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║   🔐  BACKUP — MASTER-PASSWORT EINRICHTEN                   ║");
            Console.WriteLine("  ║                                                              ║");
            Console.WriteLine("  ║   Dieses Passwort schützt ausschließlich den Backup-Bereich.║");
            Console.WriteLine("  ║   Es ist getrennt vom normalen Login-Passwort.               ║");
            Console.WriteLine("  ║   ⚠️  Merken Sie es sich gut — kein Reset möglich!           ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  🔐  Neues Master-Passwort    : ");
                Console.ResetColor();
                string pw1 = AuthManager.PasswortEingabe();
                Console.WriteLine();

                if (string.IsNullOrEmpty(pw1) || pw1.Length < 8)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ✗  Mindestens 8 Zeichen erforderlich.");
                    Console.ResetColor();
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  🔐  Passwort bestätigen      : ");
                Console.ResetColor();
                string pw2 = AuthManager.PasswortEingabe();
                Console.WriteLine();

                if (pw1 != pw2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ✗  Passwörter stimmen nicht überein.");
                    Console.ResetColor();
                    continue;
                }

                FileManager.ErstelleOrdner(FileManager.ConfigVerzeichnis);
                File.WriteAllText(hashPfad, HashMasterPasswort(pw1), Encoding.UTF8);
                FileManager.BereinigeAttribute(hashPfad);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✅  Master-Passwort gesetzt (SHA-256 gespeichert).");
                Console.ResetColor();
                Thread.Sleep(1000);
                return true;
            }
        }

        public static void MasterPasswortAendern()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🔐 Master-Passwort ändern", ConsoleColor.Yellow);
            Console.WriteLine();

            string hashPfad = Path.Combine(FileManager.ConfigVerzeichnis, MasterHashDatei);

            if (File.Exists(hashPfad) && new FileInfo(hashPfad).Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  🔐  Aktuelles Master-Passwort: ");
                Console.ResetColor();
                string alt = AuthManager.PasswortEingabe();
                Console.WriteLine();

                if (!HashMasterPasswort(alt).Equals(File.ReadAllText(hashPfad).Trim(), StringComparison.Ordinal))
                {
                    ConsoleHelper.PrintError("Falsches Master-Passwort. Abbruch.");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }
            }

            if (File.Exists(hashPfad)) File.Delete(hashPfad);
            MasterPasswortEinrichten(hashPfad);
            LogManager.LogDatenGespeichert("Backup", "Master-Passwort geaendert");
            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeMasterPasswortHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║   🔐  BACKUP-BEREICH — MASTER-PASSWORT ERFORDERLICH         ║");
            Console.WriteLine("  ║                                                              ║");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ║   Dieser Bereich ist durch ein separates Master-Passwort    ║");
            Console.WriteLine("  ║   gesichert. Unabhängig vom normalen Login-Passwort.         ║");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        // ─────────────────────────────────────────────────────────────────────
        // AUTOMATISCHE PRÜFUNG BEIM START
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Wird beim Programmstart aufgerufen.
        /// Erstellt ein Backup wenn das letzte länger als 14 Tage zurückliegt.
        /// Läuft im Hintergrund — keine Blockierung des Starts.
        /// </summary>
        public static void PruefeUndErstelleBackup()
        {
            try
            {
                if (!IstBackupFaellig()) return;

                // Backup im Hintergrund erstellen (kein UI-Block)
                var thread = new Thread(() =>
                {
                    try
                    {
                        ErstelleBackupIntern(zeigeFortschritt: false);
                    }
                    catch { /* stiller Hintergrundfehler */ }
                });
                thread.IsBackground = true;
                thread.Start();
            }
            catch { }
        }

        // ─────────────────────────────────────────────────────────────────────
        // MANUELLES BACKUP (mit UI-Feedback)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Manuelles Backup mit vollem Konsolen-Feedback.
        /// Wird aus dem Menü aufgerufen.
        /// </summary>
        public static void ManuellesBackup()
        {
            if (!MasterPasswortPruefung()) return;

            Console.Clear();
            ConsoleHelper.PrintSectionHeader("💾 Backup erstellen", ConsoleColor.Yellow);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Erstellt ein verschlüsseltes Backup aller Datendateien.");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Die Backup-Datei ist AES-256-verschlüsselt und für");
            Console.WriteLine("  Dritte ohne den Programmschlüssel nicht lesbar.");
            Console.ResetColor();
            Console.WriteLine();

            string backupPfad = ErstelleBackupIntern(zeigeFortschritt: true);

            if (!string.IsNullOrEmpty(backupPfad))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║   ✅ BACKUP ERFOLGREICH ERSTELLT                             ║");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  ║   📁 {Path.GetFileName(backupPfad),-56}║");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                var info = new FileInfo(backupPfad);
                Console.WriteLine($"  ║   📦 Größe: {FormatGroesse(info.Length),-54}║");
                Console.WriteLine($"  ║   🔐 Verschlüsselung: AES-256                               ║");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ╚══════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
            }
            else
            {
                ConsoleHelper.PrintError("Backup konnte nicht erstellt werden.");
            }

            Console.WriteLine();
            ZeigeBackupListe();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt alle vorhandenen Backups in einer Übersicht an.
        /// </summary>
        public static void ZeigeBackupUebersicht()
        {
            if (!MasterPasswortPruefung()) return;

            Console.Clear();
            ConsoleHelper.PrintSectionHeader("💾 Backup-Übersicht", ConsoleColor.Cyan);

            Console.WriteLine();

            DateTime? letztes = LadeLetzteBackupZeit();
            if (letztes.HasValue)
            {
                var diff = DateTime.Now - letztes.Value;
                string wann = diff.TotalDays < 1
                    ? "heute"
                    : diff.TotalDays < 2 ? "gestern"
                    : $"vor {(int)diff.TotalDays} Tagen";

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  Letztes Backup: {letztes.Value:dd.MM.yyyy HH:mm}  ({wann})");

                int restTage = BackupIntervallTage - (int)diff.TotalDays;
                if (restTage > 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  Nächstes automatisches Backup: in {restTage} Tag(en)");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  Nächstes automatisches Backup: beim nächsten Programmstart");
                }
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Noch kein Backup vorhanden.");
                Console.ResetColor();
            }

            Console.WriteLine();
            ZeigeBackupListe();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [1] Backup jetzt erstellen");
            Console.WriteLine("  [2] Backup wiederherstellen");
            Console.WriteLine("  [0] Zurück");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ Auswahl: ");
            Console.ResetColor();

            string eingabe = Console.ReadLine()?.Trim();
            if (eingabe == "1") ManuellesBackup();
            else if (eingabe == "2") BackupWiederherstellen();
        }

        // ─────────────────────────────────────────────────────────────────────
        // BACKUP WIEDERHERSTELLEN
        // ─────────────────────────────────────────────────────────────────────

        public static void BackupWiederherstellen()
        {
            if (!MasterPasswortPruefung()) return;

            Console.Clear();
            ConsoleHelper.PrintSectionHeader("♻️  Backup wiederherstellen", ConsoleColor.Magenta);

            var backups = LadeBackupListe();
            if (backups.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Backups vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            for (int i = 0; i < backups.Count; i++)
            {
                var info = new FileInfo(backups[i]);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  [{i + 1}] {info.LastWriteTime:dd.MM.yyyy HH:mm}   ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"{FormatGroesse(info.Length),10}   {Path.GetFileName(backups[i])}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ⚠️  ACHTUNG: Die aktuellen Daten werden überschrieben!");
            Console.ResetColor();
            Console.WriteLine();

            string numEingabe = ConsoleHelper.GetInput("Backup-Nummer wählen (0 = Abbrechen)");
            if (numEingabe == "0" || string.IsNullOrEmpty(numEingabe)) return;

            if (!int.TryParse(numEingabe, out int nr) || nr < 1 || nr > backups.Count)
            {
                ConsoleHelper.PrintError("Ungültige Auswahl.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            string bestaetigung = ConsoleHelper.GetInput("Wirklich wiederherstellen? Eingabe 'ja' zur Bestätigung");
            if (!bestaetigung.Equals("ja", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelper.PrintInfo("Abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            bool ok = WiederherstellenIntern(backups[nr - 1]);

            if (ok)
            {
                ConsoleHelper.PrintSuccess("Backup erfolgreich wiederhergestellt!");
                ConsoleHelper.PrintInfo("Bitte das Programm neu starten damit alle Daten neu geladen werden.");
            }
            else
            {
                ConsoleHelper.PrintError("Fehler beim Wiederherstellen. Backup möglicherweise beschädigt.");
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ─────────────────────────────────────────────────────────────────────
        // KERN-LOGIK: BACKUP ERSTELLEN
        // ─────────────────────────────────────────────────────────────────────

        private static string ErstelleBackupIntern(bool zeigeFortschritt)
        {
            try
            {
                FileManager.ErstelleOrdner(FileManager.BackupVerzeichnis);

                string zeitstempel = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string dateiName = $"{BackupPrefix}{zeitstempel}{BackupDateiendung}";
                string zielPfad = Path.Combine(FileManager.BackupVerzeichnis, dateiName);

                // ── Dateien die gesichert werden ──────────────────────────────
                var zuSichern = new List<(string Pfad, string ArchivName)>
                {
                    (FileManager.FilePath,              "Data/Inventar.txt"),
                    (FileManager.FilePath2,             "Data/Mitarbeiter.txt"),
                    (FileManager.FilePath3,             "Data/Accounts.txt"),
                    (FileManager.RollenPfad,            "Data/Rollen.txt"),
                    (FileManager.DruckEinstellungenPfad,"Config/DruckEinstellungen.txt"),
                    (FileManager.KIConfigPfad,          "Config/KI_Config.dat"),
                    (FileManager.KIStatsPfad,           "Config/KI_Stats.dat"),
                    (FileManager.ArtikelTemplatesPfad,  "Config/artikel_templates.txt"),
                    (FileManager.DruckHistoriePfad,     "Exports/DruckHistorie.json"),
                };

                // ── ZIP im Speicher erstellen ─────────────────────────────────
                if (zeigeFortschritt)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("  Sammle Dateien...");
                    Console.ResetColor();
                }

                byte[] zipDaten;
                using (var ms = new MemoryStream())
                {
                    using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        int gesamt = zuSichern.Count;
                        int aktuell = 0;

                        foreach (var (pfad, archivName) in zuSichern)
                        {
                            aktuell++;

                            if (!File.Exists(pfad))
                            {
                                if (zeigeFortschritt)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGray;
                                    Console.WriteLine($"  [{aktuell}/{gesamt}] {archivName,-40} — übersprungen (nicht vorhanden)");
                                    Console.ResetColor();
                                }
                                continue;
                            }

                            // Datei einlesen (auch wenn gerade geöffnet)
                            byte[] inhalt = LeseDateiSicher(pfad);
                            if (inhalt == null) continue;

                            var eintrag = zip.CreateEntry(archivName, CompressionLevel.Optimal);
                            using (var stream = eintrag.Open())
                                stream.Write(inhalt, 0, inhalt.Length);

                            if (zeigeFortschritt)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write($"  [{aktuell}/{gesamt}] ");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write($"{archivName,-40}");
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine($" {FormatGroesse(inhalt.Length),8}");
                                Console.ResetColor();
                            }
                        }

                        // Metadaten-Eintrag im ZIP
                        var meta = zip.CreateEntry("backup_info.txt");
                        using (var sw = new StreamWriter(meta.Open(), Encoding.UTF8))
                        {
                            sw.WriteLine($"Backup erstellt: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                            sw.WriteLine($"Programm: Inventarverwaltung");
                            sw.WriteLine($"Version:  2.0");
                            sw.WriteLine($"Dateien:  {zuSichern.Count(x => File.Exists(x.Pfad))}");
                        }
                    }

                    zipDaten = ms.ToArray();
                }

                if (zeigeFortschritt)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  ZIP erstellt: {FormatGroesse(zipDaten.Length)}");
                    Console.WriteLine("  Verschlüssele mit AES-256...");
                    Console.ResetColor();
                }

                // ── AES-256 Verschlüsselung ───────────────────────────────────
                byte[] verschluesselt = VerschluesseleBytes(zipDaten);
                File.WriteAllBytes(zielPfad, verschluesselt);
                FileManager.BereinigeAttribute(zielPfad);

                // ── Zeitmarke speichern ───────────────────────────────────────
                SpeichereBackupZeit(DateTime.Now);

                // ── Alte Backups bereinigen ───────────────────────────────────
                BereinigeAlteBackups();

                return zielPfad;
            }
            catch (Exception ex)
            {
                if (zeigeFortschritt)
                    ConsoleHelper.PrintError($"Backup-Fehler: {ex.Message}");
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // KERN-LOGIK: WIEDERHERSTELLEN
        // ─────────────────────────────────────────────────────────────────────

        private static bool WiederherstellenIntern(string backupPfad)
        {
            try
            {
                // Entschlüsseln
                byte[] verschluesselt = File.ReadAllBytes(backupPfad);
                byte[] zipDaten = EntschluesseleBytes(verschluesselt);

                if (zipDaten == null || zipDaten.Length == 0)
                    return false;

                // ZIP auspacken
                using (var ms = new MemoryStream(zipDaten))
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Read))
                {
                    foreach (var eintrag in zip.Entries)
                    {
                        if (eintrag.Name == "backup_info.txt") continue;

                        // Zielpfad bestimmen
                        string ziel = ErmittleZielpfad(eintrag.FullName);
                        if (string.IsNullOrEmpty(ziel)) continue;

                        FileManager.ErstelleOrdner(Path.GetDirectoryName(ziel));

                        using (var src = eintrag.Open())
                        using (var dest = File.Create(ziel))
                            src.CopyTo(dest);

                        FileManager.BereinigeAttribute(ziel);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // VERSCHLÜSSELUNG — AES-256 mit zufälligem IV pro Backup
        // ─────────────────────────────────────────────────────────────────────

        private static byte[] VerschluesseleBytes(byte[] daten)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = BackupKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV(); // zufälliger IV pro Backup → erhöht Sicherheit

                using (var ms = new MemoryStream())
                {
                    // IV (16 Byte) voranstellen damit er beim Entschlüsseln bekannt ist
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(daten, 0, daten.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        private static byte[] EntschluesseleBytes(byte[] daten)
        {
            if (daten == null || daten.Length < 17) return null;

            using (var aes = Aes.Create())
            {
                aes.Key = BackupKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Ersten 16 Byte = IV
                byte[] iv = new byte[16];
                Array.Copy(daten, 0, iv, 0, 16);
                aes.IV = iv;

                using (var ms = new MemoryStream(daten, 16, daten.Length - 16))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var out_ms = new MemoryStream())
                {
                    cs.CopyTo(out_ms);
                    return out_ms.ToArray();
                }
            }
        }

        private static byte[] LeiteSchluesselAb(string passwort, string salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                passwort,
                Encoding.UTF8.GetBytes(salt),
                iterations: 50_000,
                HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32); // 256 Bit
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ZEITMARKE
        // ─────────────────────────────────────────────────────────────────────

        private static bool IstBackupFaellig()
        {
            DateTime? letztes = LadeLetzteBackupZeit();
            if (!letztes.HasValue) return true;
            return (DateTime.Now - letztes.Value).TotalDays >= BackupIntervallTage;
        }

        private static void SpeichereBackupZeit(DateTime zeit)
        {
            try
            {
                string pfad = Path.Combine(FileManager.BackupVerzeichnis, ZeitmarkenDatei);
                File.WriteAllText(pfad, zeit.ToString("o"), Encoding.UTF8); // ISO 8601
            }
            catch { }
        }

        private static DateTime? LadeLetzteBackupZeit()
        {
            try
            {
                string pfad = Path.Combine(FileManager.BackupVerzeichnis, ZeitmarkenDatei);
                if (!File.Exists(pfad)) return null;

                string inhalt = File.ReadAllText(pfad).Trim();
                if (DateTime.TryParse(inhalt, out DateTime dt))
                    return dt;
            }
            catch { }
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // BACKUP-LISTE & BEREINIGUNG
        // ─────────────────────────────────────────────────────────────────────

        private static List<string> LadeBackupListe()
        {
            try
            {
                if (!Directory.Exists(FileManager.BackupVerzeichnis))
                    return new List<string>();

                return Directory
                    .GetFiles(FileManager.BackupVerzeichnis, $"{BackupPrefix}*{BackupDateiendung}")
                    .OrderByDescending(f => f)
                    .ToList();
            }
            catch { return new List<string>(); }
        }

        private static void BereinigeAlteBackups()
        {
            try
            {
                var alle = LadeBackupListe();
                // Die neuesten MaxBackups behalten, Rest löschen
                foreach (string alt in alle.Skip(MaxBackups))
                {
                    try { File.Delete(alt); } catch { }
                }
            }
            catch { }
        }

        private static void ZeigeBackupListe()
        {
            var backups = LadeBackupListe();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ║   VORHANDENE BACKUPS                                             ║");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            if (backups.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Noch keine Backups vorhanden.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  {"Nr",-4} {"Datum",-18} {"Größe",-12} {"Dateiname"}");
            Console.WriteLine("  " + new string('─', 65));
            Console.ResetColor();

            for (int i = 0; i < backups.Count; i++)
            {
                var info = new FileInfo(backups[i]);
                bool neustes = i == 0;

                Console.ForegroundColor = neustes ? ConsoleColor.Green : ConsoleColor.White;
                Console.Write($"  [{i + 1}]  {info.LastWriteTime:dd.MM.yyyy HH:mm}   ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{FormatGroesse(info.Length),-12}");
                Console.ForegroundColor = neustes ? ConsoleColor.Green : ConsoleColor.DarkGray;
                Console.Write(Path.GetFileName(backups[i]));
                if (neustes) { Console.ForegroundColor = ConsoleColor.Green; Console.Write("  ← aktuell"); }
                Console.WriteLine();
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Max. {MaxBackups} Backups · Intervall: {BackupIntervallTage} Tage · Verschlüsselung: AES-256");
            Console.ResetColor();
        }

        // ─────────────────────────────────────────────────────────────────────
        // HILFSMETHODEN
        // ─────────────────────────────────────────────────────────────────────

        private static byte[] LeseDateiSicher(string pfad)
        {
            try
            {
                // FileShare.ReadWrite damit Dateien die evtl. offen sind trotzdem lesbar
                using (var fs = new FileStream(pfad, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            catch { return null; }
        }

        private static string ErmittleZielpfad(string archivName)
        {
            // archivName z.B. "Data/Inventar.txt" → FileManager.FilePath
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Data/Inventar.txt",               FileManager.FilePath },
                { "Data/Mitarbeiter.txt",             FileManager.FilePath2 },
                { "Data/Accounts.txt",               FileManager.FilePath3 },
                { "Data/Rollen.txt",                 FileManager.RollenPfad },
                { "Config/DruckEinstellungen.txt",   FileManager.DruckEinstellungenPfad },
                { "Config/KI_Config.dat",            FileManager.KIConfigPfad },
                { "Config/KI_Stats.dat",             FileManager.KIStatsPfad },
                { "Config/artikel_templates.txt",    FileManager.ArtikelTemplatesPfad },
                { "Exports/DruckHistorie.json",      FileManager.DruckHistoriePfad },
            };

            string normiert = archivName.Replace('\\', '/');
            return map.TryGetValue(normiert, out string ziel) ? ziel : null;
        }

        private static string FormatGroesse(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024):F1} MB";
        }
    }
}