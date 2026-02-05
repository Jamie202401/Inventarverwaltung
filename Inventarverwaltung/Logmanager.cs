using System;
using System.IO;
using System.Net;
using System.Text;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Logging-Operationen des Systems mit AES-256 Verschlüsselung
    /// Alle Log-Dateien werden automatisch verschlüsselt gespeichert
    /// </summary>
    public static class LogManager
    {
        private static string logFilePath = "System_Log.enc";  // .enc für encrypted
        private static string aktuellerBenutzer = "System";

        /// <summary>
        /// Setzt den aktuellen Benutzernamen für das Logging
        /// </summary>
        public static void SetAktuellerBenutzer(string benutzername)
        {
            aktuellerBenutzer = benutzername;
        }

        /// <summary>
        /// Initialisiert die verschlüsselte Log-Datei beim Programmstart
        /// </summary>
        public static void InitializeLog()
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("╔════════════════════════════════════════════════════════════════════════╗");
                    sb.AppendLine("║         INVENTARVERWALTUNG - VERSCHLÜSSELTE LOG DATEI                 ║");
                    sb.AppendLine("║              🔐 AES-256 ENCRYPTED LOG FILE 🔐                         ║");
                    sb.AppendLine("╚════════════════════════════════════════════════════════════════════════╝");
                    sb.AppendLine($"Log-Datei erstellt am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                    sb.AppendLine($"Verschlüsselung: AES-256-CBC mit PBKDF2-SHA256");
                    sb.AppendLine(new string('═', 76));
                    sb.AppendLine();

                    // Verschlüsselt speichern
                    byte[] encrypted = EncryptionManager.EncryptString(sb.ToString());
                    if (encrypted != null)
                    {
                        File.WriteAllBytes(logFilePath, encrypted);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Initialisieren der Log-Datei: {ex.Message}");
            }
        }

        /// <summary>
        /// Schreibt einen verschlüsselten Log-Eintrag
        /// </summary>
        private static void SchreibeLog(string kategorie, string aktion, string details = "")
        {
            try
            {
                StringBuilder logEntry = new StringBuilder();

                string zeitstempel = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                string computerName = Environment.MachineName;
                string ip = GetLocalIPAddress();

                logEntry.AppendLine($"[{zeitstempel}] [{kategorie}]");
                logEntry.AppendLine($"  ├─ Benutzer: {aktuellerBenutzer}");
                logEntry.AppendLine($"  ├─ Computer: {computerName}");
                logEntry.AppendLine($"  ├─ IP-Adresse: {ip}");
                logEntry.AppendLine($"  ├─ Aktion: {aktion}");

                if (!string.IsNullOrWhiteSpace(details))
                {
                    logEntry.AppendLine($"  └─ Details: {details}");
                }
                else
                {
                    logEntry.AppendLine($"  └─ Status: Erfolgreich");
                }

                logEntry.AppendLine(new string('─', 76));
                logEntry.AppendLine();

                // Verschlüsselt anhängen
                EncryptionManager.AppendEncrypted(logFilePath, logEntry.ToString());
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Schreiben ins Log: {ex.Message}");
            }
        }

        /// <summary>
        /// Ermittelt die lokale IP-Adresse des Computers
        /// </summary>
        private static string GetLocalIPAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                foreach (IPAddress address in addresses)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }
                }

                return "Unbekannt";
            }
            catch
            {
                return "Nicht verfügbar";
            }
        }

        #region System-Logs

        public static void LogProgrammStart()
        {
            SchreibeLog("SYSTEM", "Programm gestartet",
                $"Version: 1.0 | Betriebssystem: {Environment.OSVersion} | Verschlüsselung: AES-256");
        }

        public static void LogProgrammEnde()
        {
            SchreibeLog("SYSTEM", "Programm beendet");
        }

        #endregion

        #region Anmelde-Logs

        public static void LogAnmeldungErfolgreich(string benutzername)
        {
            aktuellerBenutzer = benutzername;
            SchreibeLog("ANMELDUNG", "Benutzer angemeldet",
                $"Benutzername: {benutzername} | Status: Erfolgreich");
        }

        public static void LogNeuesKontoErstellt(string benutzername)
        {
            aktuellerBenutzer = benutzername;
            SchreibeLog("ANMELDUNG", "Neues Konto erstellt",
                $"Benutzername: {benutzername}");
        }

        public static void LogAnmeldungFehlgeschlagen(string grund)
        {
            SchreibeLog("ANMELDUNG", "Anmeldung fehlgeschlagen", grund);
        }

        #endregion

        #region Inventar-Logs

        public static void LogArtikelHinzugefuegt(string invNr, string geraeteName, string mitarbeiter)
        {
            SchreibeLog("INVENTAR", "Neuer Artikel hinzugefügt",
                $"Inv-Nr: {invNr} | Gerät: {geraeteName} | Mitarbeiter: {mitarbeiter}");
        }

        public static void LogInventarAngezeigt(int anzahl)
        {
            SchreibeLog("INVENTAR", "Inventar angezeigt",
                $"Anzahl Artikel: {anzahl}");
        }

        public static void LogArtikelDuplikat(string invNr, string geraeteName)
        {
            SchreibeLog("INVENTAR", "Duplikat verhindert",
                $"Versuch einen bereits existierenden Artikel hinzuzufügen | Inv-Nr: {invNr} | Gerät: {geraeteName}");
        }

        #endregion

        #region Mitarbeiter-Logs

        public static void LogMitarbeiterHinzugefuegt(string vorname, string nachname, string abteilung)
        {
            SchreibeLog("MITARBEITER", "Neuer Mitarbeiter hinzugefügt",
                $"Name: {vorname} {nachname} | Abteilung: {abteilung}");
        }

        public static void LogMitarbeiterAngezeigt(int anzahl)
        {
            SchreibeLog("MITARBEITER", "Mitarbeiterliste angezeigt",
                $"Anzahl Mitarbeiter: {anzahl}");
        }

        public static void LogMitarbeiterDuplikat(string vorname, string nachname)
        {
            SchreibeLog("MITARBEITER", "Duplikat verhindert",
                $"Versuch einen bereits existierenden Mitarbeiter hinzuzufügen | Name: {vorname} {nachname}");
        }

        #endregion

        #region Benutzer-Logs

        public static void LogBenutzerAngelegt(string benutzername, Berechtigungen berechtigung)
        {
            SchreibeLog("BENUTZER", "Neuer Benutzer angelegt",
                $"Benutzername: {benutzername} | Berechtigung: {berechtigung}");
        }

        public static void LogBenutzerAngezeigt(int anzahl)
        {
            SchreibeLog("BENUTZER", "Benutzerliste angezeigt",
                $"Anzahl Benutzer: {anzahl}");
        }

        public static void LogBenutzerDuplikat(string benutzername)
        {
            SchreibeLog("BENUTZER", "Duplikat verhindert",
                $"Versuch einen bereits existierenden Benutzer anzulegen | Benutzername: {benutzername}");
        }

        public static void LogBenutzerAktualisiert(string benutzername, string alteBerechtigung, string neueBerechtigung)
        {
            SchreibeLog("BENUTZER", "Berechtigung aktualisiert",
                $"Benutzername: {benutzername} | Alt: {alteBerechtigung} → Neu: {neueBerechtigung}");
        }

        #endregion

        #region Datei-Logs

        public static void LogDatenGeladen(string dateityp, int anzahl)
        {
            SchreibeLog("DATEI", "Daten geladen",
                $"Typ: {dateityp} | Anzahl Datensätze: {anzahl}");
        }

        public static void LogDatenGespeichert(string dateityp, string details)
        {
            SchreibeLog("DATEI", "Daten gespeichert",
                $"Typ: {dateityp} | {details}");
        }

        #endregion

        #region Zuweisung-Logs

        /// <summary>
        /// Protokolliert die Neuzuweisung eines Artikels
        /// </summary>
        public static void LogArtikelNeuzugewiesen(string invNr, string geraeteName, string alterMitarbeiter, string neuerMitarbeiter)
        {
            SchreibeLog("ZUWEISUNG", "Artikel neu zugewiesen",
                $"Inv-Nr: {invNr} | Gerät: {geraeteName} | Von: {alterMitarbeiter} → Zu: {neuerMitarbeiter}");
        }

        #endregion

        #region Fehler-Logs

        public static void LogFehler(string bereich, string fehlermeldung)
        {
            SchreibeLog("FEHLER", $"Fehler in {bereich}", fehlermeldung);
        }

        public static void LogWarnung(string bereich, string warnung)
        {
            SchreibeLog("WARNUNG", bereich, warnung);
        }

        #endregion

        #region Log-Anzeige

        /// <summary>
        /// Zeigt die entschlüsselte Log-Datei an
        /// </summary>
        public static void ZeigeLogDatei()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🔐 Verschlüsseltes System-Log anzeigen", ConsoleColor.DarkCyan);

            if (!File.Exists(logFilePath))
            {
                ConsoleHelper.PrintWarning("Keine Log-Datei vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            try
            {
                Console.WriteLine();
                ConsoleHelper.PrintInfo("🔓 Entschlüssele Log-Datei...");
                System.Threading.Thread.Sleep(500); // Simuliert Entschlüsselung

                // Log entschlüsseln und lesen
                string decryptedLog = EncryptionManager.ReadEncryptedFile(logFilePath);

                if (decryptedLog == null)
                {
                    ConsoleHelper.PrintError("Fehler beim Entschlüsseln der Log-Datei!");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }

                string[] logLines = decryptedLog.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                Console.WriteLine();
                ConsoleHelper.PrintSuccess("✓ Log-Datei erfolgreich entschlüsselt!");
                ConsoleHelper.PrintInfo($"Log-Datei: {Path.GetFullPath(logFilePath)}");
                ConsoleHelper.PrintInfo($"Dateigröße: {new FileInfo(logFilePath).Length} Bytes (verschlüsselt)");
                ConsoleHelper.PrintInfo($"Anzahl Zeilen: {logLines.Length}");
                Console.WriteLine();

                ConsoleHelper.PrintSeparator();

                // Die letzten 50 Zeilen anzeigen
                int startIndex = Math.Max(0, logLines.Length - 50);

                Console.ForegroundColor = ConsoleColor.Gray;
                for (int i = startIndex; i < logLines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(logLines[i]))
                    {
                        Console.WriteLine(logLines[i]);
                    }
                }
                Console.ResetColor();

                Console.WriteLine();
                ConsoleHelper.PrintInfo($"Es werden die letzten {Math.Min(50, logLines.Length)} Einträge angezeigt.");

                // Hash zur Integritätsprüfung
                string hash = EncryptionManager.GetFileHash(logFilePath);
                if (hash != null)
                {
                    Console.WriteLine();
                    ConsoleHelper.PrintInfo($"Datei-Hash (SHA-256): {hash.Substring(0, 32)}...");
                }

                ConsoleHelper.PressKeyToContinue();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Lesen der Log-Datei: {ex.Message}");
                ConsoleHelper.PressKeyToContinue();
            }
        }

        /// <summary>
        /// Erstellt einen verschlüsselten Tagesreport
        /// </summary>
        public static void ErstelleTagesReport()
        {
            string heute = DateTime.Now.ToString("dd.MM.yyyy");
            string reportPfad = $"Report_{DateTime.Now:yyyyMMdd}.enc";

            try
            {
                if (!File.Exists(logFilePath))
                {
                    ConsoleHelper.PrintWarning("Keine Log-Datei vorhanden!");
                    return;
                }

                Console.WriteLine();
                ConsoleHelper.PrintInfo("🔓 Entschlüssele Log-Datei für Report...");
                System.Threading.Thread.Sleep(500);

                // Log entschlüsseln
                string decryptedLog = EncryptionManager.ReadEncryptedFile(logFilePath);

                if (decryptedLog == null)
                {
                    ConsoleHelper.PrintError("Fehler beim Entschlüsseln!");
                    return;
                }

                string[] alleLogs = decryptedLog.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                int eintraegeHeute = 0;

                StringBuilder report = new StringBuilder();
                report.AppendLine("╔════════════════════════════════════════════════════════════════════════╗");
                report.AppendLine("║              TAGESREPORT - ZUSAMMENFASSUNG (VERSCHLÜSSELT)            ║");
                report.AppendLine("╚════════════════════════════════════════════════════════════════════════╝");
                report.AppendLine($"Datum: {heute}");
                report.AppendLine($"Erstellt am: {DateTime.Now:HH:mm:ss}");
                report.AppendLine($"Verschlüsselung: AES-256-CBC");
                report.AppendLine(new string('═', 76));
                report.AppendLine();

                foreach (string zeile in alleLogs)
                {
                    if (zeile.Contains(heute))
                    {
                        eintraegeHeute++;
                        report.AppendLine(zeile);
                    }
                }

                report.AppendLine();
                report.AppendLine(new string('═', 76));
                report.AppendLine($"Gesamt {eintraegeHeute} Einträge für den {heute}");

                ConsoleHelper.PrintInfo("🔐 Verschlüssele Report...");
                System.Threading.Thread.Sleep(500);

                // Report verschlüsseln und speichern
                byte[] encrypted = EncryptionManager.EncryptString(report.ToString());

                if (encrypted == null)
                {
                    ConsoleHelper.PrintError("Fehler beim Verschlüsseln des Reports!");
                    return;
                }

                File.WriteAllBytes(reportPfad, encrypted);

                ConsoleHelper.PrintSuccess($"✓ Verschlüsselter Tagesreport erstellt: {reportPfad}");
                ConsoleHelper.PrintInfo($"Einträge heute: {eintraegeHeute}");
                ConsoleHelper.PrintInfo($"Dateigröße: {new FileInfo(reportPfad).Length} Bytes");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Erstellen des Reports: {ex.Message}");
            }
        }

        #endregion
    }
}