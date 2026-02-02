using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Logging-Operationen des Systems
    /// Protokolliert alle wichtigen Aktionen mit Zeitstempel, Benutzer und Systeminfo
    /// </summary>
    public static class LogManager
    {
        private static string logFilePath = "System_Log.txt";
        private static string aktuellerBenutzer = "System";

        /// <summary>
        /// Setzt den aktuellen Benutzernamen für das Logging
        /// </summary>
        public static void SetAktuellerBenutzer(string benutzername)
        {
            aktuellerBenutzer = benutzername;
        }

        /// <summary>
        /// Initialisiert die Log-Datei beim Programmstart
        /// </summary>
        public static void InitializeLog()
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    using (StreamWriter sw = new StreamWriter(logFilePath))
                    {
                        sw.WriteLine("╔════════════════════════════════════════════════════════════════════════╗");
                        sw.WriteLine("║            INVENTARVERWALTUNG - SYSTEM LOG DATEI                      ║");
                        sw.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
                        sw.WriteLine($"Log-Datei erstellt am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                        sw.WriteLine(new string('═', 76));
                        sw.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Initialisieren der Log-Datei: {ex.Message}");
            }
        }

        /// <summary>
        /// Schreibt einen allgemeinen Log-Eintrag
        /// </summary>
        private static void SchreibeLog(string kategorie, string aktion, string details = "")
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(logFilePath, true))
                {
                    string zeitstempel = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    string computerName = Environment.MachineName;
                    string ip = GetLocalIPAddress();

                    sw.WriteLine($"[{zeitstempel}] [{kategorie}]");
                    sw.WriteLine($"  ├─ Benutzer: {aktuellerBenutzer}");
                    sw.WriteLine($"  ├─ Computer: {computerName}");
                    sw.WriteLine($"  ├─ IP-Adresse: {ip}");
                    sw.WriteLine($"  ├─ Aktion: {aktion}");

                    if (!string.IsNullOrWhiteSpace(details))
                    {
                        sw.WriteLine($"  └─ Details: {details}");
                    }
                    else
                    {
                        sw.WriteLine($"  └─ Status: Erfolgreich");
                    }

                    sw.WriteLine(new string('─', 76));
                    sw.WriteLine();
                }
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

        /// <summary>
        /// Protokolliert den Programmstart
        /// </summary>
        public static void LogProgrammStart()
        {
            SchreibeLog("SYSTEM", "Programm gestartet",
                $"Version: 1.0 | Betriebssystem: {Environment.OSVersion}");
        }

        /// <summary>
        /// Protokolliert das Programmende
        /// </summary>
        public static void LogProgrammEnde()
        {
            SchreibeLog("SYSTEM", "Programm beendet");
        }

        #endregion

        #region Anmelde-Logs

        /// <summary>
        /// Protokolliert eine erfolgreiche Anmeldung
        /// </summary>
        public static void LogAnmeldungErfolgreich(string benutzername)
        {
            aktuellerBenutzer = benutzername;
            SchreibeLog("ANMELDUNG", "Benutzer angemeldet",
                $"Benutzername: {benutzername} | Status: Erfolgreich");
        }

        /// <summary>
        /// Protokolliert die Erstellung eines neuen Anmelde-Kontos
        /// </summary>
        public static void LogNeuesKontoErstellt(string benutzername)
        {
            aktuellerBenutzer = benutzername;
            SchreibeLog("ANMELDUNG", "Neues Konto erstellt",
                $"Benutzername: {benutzername}");
        }

        /// <summary>
        /// Protokolliert einen fehlgeschlagenen Anmeldeversuch
        /// </summary>
        public static void LogAnmeldungFehlgeschlagen(string grund)
        {
            SchreibeLog("ANMELDUNG", "Anmeldung fehlgeschlagen", grund);
        }

        #endregion

        #region Inventar-Logs

        /// <summary>
        /// Protokolliert das Hinzufügen eines neuen Artikels
        /// </summary>
        public static void LogArtikelHinzugefuegt(string invNr, string geraeteName, string mitarbeiter)
        {
            SchreibeLog("INVENTAR", "Neuer Artikel hinzugefügt",
                $"Inv-Nr: {invNr} | Gerät: {geraeteName} | Mitarbeiter: {mitarbeiter}");
        }

        /// <summary>
        /// Protokolliert das Anzeigen des Inventars
        /// </summary>
        public static void LogInventarAngezeigt(int anzahl)
        {
            SchreibeLog("INVENTAR", "Inventar angezeigt",
                $"Anzahl Artikel: {anzahl}");
        }

        /// <summary>
        /// Protokolliert einen Versuch, einen doppelten Artikel hinzuzufügen
        /// </summary>
        public static void LogArtikelDuplikat(string invNr, string geraeteName)
        {
            SchreibeLog("INVENTAR", "Duplikat verhindert",
                $"Versuch einen bereits existierenden Artikel hinzuzufügen | Inv-Nr: {invNr} | Gerät: {geraeteName}");
        }

        #endregion

        #region Mitarbeiter-Logs

        /// <summary>
        /// Protokolliert das Hinzufügen eines neuen Mitarbeiters
        /// </summary>
        public static void LogMitarbeiterHinzugefuegt(string vorname, string nachname, string abteilung)
        {
            SchreibeLog("MITARBEITER", "Neuer Mitarbeiter hinzugefügt",
                $"Name: {vorname} {nachname} | Abteilung: {abteilung}");
        }

        /// <summary>
        /// Protokolliert das Anzeigen der Mitarbeiterliste
        /// </summary>
        public static void LogMitarbeiterAngezeigt(int anzahl)
        {
            SchreibeLog("MITARBEITER", "Mitarbeiterliste angezeigt",
                $"Anzahl Mitarbeiter: {anzahl}");
        }

        /// <summary>
        /// Protokolliert einen Versuch, einen doppelten Mitarbeiter hinzuzufügen
        /// </summary>
        public static void LogMitarbeiterDuplikat(string vorname, string nachname)
        {
            SchreibeLog("MITARBEITER", "Duplikat verhindert",
                $"Versuch einen bereits existierenden Mitarbeiter hinzuzufügen | Name: {vorname} {nachname}");
        }

        #endregion

        #region Benutzer-Logs

        /// <summary>
        /// Protokolliert das Anlegen eines neuen Benutzers
        /// </summary>
        public static void LogBenutzerAngelegt(string benutzername, Berechtigungen berechtigung)
        {
            SchreibeLog("BENUTZER", "Neuer Benutzer angelegt",
                $"Benutzername: {benutzername} | Berechtigung: {berechtigung}");
        }

        /// <summary>
        /// Protokolliert das Anzeigen der Benutzerliste
        /// </summary>
        public static void LogBenutzerAngezeigt(int anzahl)
        {
            SchreibeLog("BENUTZER", "Benutzerliste angezeigt",
                $"Anzahl Benutzer: {anzahl}");
        }

        /// <summary>
        /// Protokolliert einen Versuch, einen doppelten Benutzer anzulegen
        /// </summary>
        public static void LogBenutzerDuplikat(string benutzername)
        {
            SchreibeLog("BENUTZER", "Duplikat verhindert",
                $"Versuch einen bereits existierenden Benutzer anzulegen | Benutzername: {benutzername}");
        }

        #endregion

        #region Datei-Logs

        /// <summary>
        /// Protokolliert das Laden von Daten aus einer Datei
        /// </summary>
        public static void LogDatenGeladen(string dateityp, int anzahl)
        {
            SchreibeLog("DATEI", "Daten geladen",
                $"Typ: {dateityp} | Anzahl Datensätze: {anzahl}");
        }

        /// <summary>
        /// Protokolliert das Speichern von Daten in eine Datei
        /// </summary>
        public static void LogDatenGespeichert(string dateityp, string details)
        {
            SchreibeLog("DATEI", "Daten gespeichert",
                $"Typ: {dateityp} | {details}");
        }

        #endregion

        #region Fehler-Logs

        /// <summary>
        /// Protokolliert einen Fehler im System
        /// </summary>
        public static void LogFehler(string bereich, string fehlermeldung)
        {
            SchreibeLog("FEHLER", $"Fehler in {bereich}", fehlermeldung);
        }

        /// <summary>
        /// Protokolliert eine Warnung
        /// </summary>
        public static void LogWarnung(string bereich, string warnung)
        {
            SchreibeLog("WARNUNG", bereich, warnung);
        }

        #endregion

        #region Hilfs-Methoden

        /// <summary>
        /// Zeigt die Log-Datei an (für Admin-Zugriff)
        /// </summary>
        public static void ZeigeLogDatei()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("System-Log anzeigen", ConsoleColor.DarkCyan);

            if (!File.Exists(logFilePath))
            {
                ConsoleHelper.PrintWarning("Keine Log-Datei vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            try
            {
                string[] logLines = File.ReadAllLines(logFilePath);

                Console.WriteLine();
                ConsoleHelper.PrintInfo($"Log-Datei: {Path.GetFullPath(logFilePath)}");
                ConsoleHelper.PrintInfo($"Anzahl Zeilen: {logLines.Length}");
                Console.WriteLine();

                ConsoleHelper.PrintSeparator();

                // Die letzten 50 Zeilen anzeigen (oder weniger falls nicht so viele vorhanden)
                int startIndex = Math.Max(0, logLines.Length - 50);

                Console.ForegroundColor = ConsoleColor.Gray;
                for (int i = startIndex; i < logLines.Length; i++)
                {
                    Console.WriteLine(logLines[i]);
                }
                Console.ResetColor();

                Console.WriteLine();
                ConsoleHelper.PrintInfo($"Es werden die letzten {Math.Min(50, logLines.Length)} Einträge angezeigt.");
                ConsoleHelper.PressKeyToContinue();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Lesen der Log-Datei: {ex.Message}");
                ConsoleHelper.PressKeyToContinue();
            }
        }

        /// <summary>
        /// Erstellt einen täglichen Log-Report
        /// </summary>
        public static void ErstelleTagesReport()
        {
            string heute = DateTime.Now.ToString("dd.MM.yyyy");
            string reportPfad = $"Report_{DateTime.Now:yyyyMMdd}.txt";

            try
            {
                if (!File.Exists(logFilePath))
                {
                    ConsoleHelper.PrintWarning("Keine Log-Datei vorhanden!");
                    return;
                }

                string[] alleLogs = File.ReadAllLines(logFilePath);
                int eintraegeHeute = 0;

                using (StreamWriter sw = new StreamWriter(reportPfad))
                {
                    sw.WriteLine("╔════════════════════════════════════════════════════════════════════════╗");
                    sw.WriteLine("║                     TAGESREPORT - ZUSAMMENFASSUNG                      ║");
                    sw.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
                    sw.WriteLine($"Datum: {heute}");
                    sw.WriteLine($"Erstellt am: {DateTime.Now:HH:mm:ss}");
                    sw.WriteLine(new string('═', 76));
                    sw.WriteLine();

                    foreach (string zeile in alleLogs)
                    {
                        if (zeile.Contains(heute))
                        {
                            eintraegeHeute++;
                            sw.WriteLine(zeile);
                        }
                    }

                    sw.WriteLine();
                    sw.WriteLine(new string('═', 76));
                    sw.WriteLine($"Gesamt {eintraegeHeute} Einträge für den {heute}");
                }

                ConsoleHelper.PrintSuccess($"Tagesreport erstellt: {reportPfad}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Erstellen des Reports: {ex.Message}");
            }
        }

        #endregion
    }
}