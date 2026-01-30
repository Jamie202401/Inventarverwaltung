using System;
using System.IO;
using System.Text;

namespace Inventarverwaltung
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Einfacher, thread‑sicherer Logger für das Programm.
    /// Verwenden: Extrafunctions.InitializeLog("logs\\app.log"); dann Extrafunctions.Log(...).
    /// Beinhaltet Convenience‑Methoden wie LogLogin, LogAction, LogException.
    /// </summary>
    public static class Extrafunctions
    {
        private static readonly object s_lock = new();
        private static string s_logPath = "app.log";
        private static bool s_initialized = false;

        /// <summary>
        /// Initialisiert den Logger. Optionaler Pfad (relativ zum Arbeitsverzeichnis).
        /// Erstellt Verzeichnis bei Bedarf und schreibt Kopfzeile.
        /// </summary>
        public static void InitializeLog(string logPath = "app.log")
        {
            Console.WriteLine("halo");
            lock (s_lock)
            {
                s_logPath = logPath ?? "app.log";

                try
                {
                    string dir = Path.GetDirectoryName(Path.GetFullPath(s_logPath));
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    // Kopfzeile mit Zeitstempel
                    var header = new StringBuilder();
                    header.AppendLine("==================================================");
                    header.AppendLine($"Log gestartet: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    header.AppendLine("==================================================");
                    File.AppendAllText(s_logPath, header.ToString(), Encoding.UTF8);

                    s_initialized = true;
                }
                catch
                {
                    // Initialisierungsfehler still ignorieren; Logging weiterhin versuchen
                    s_initialized = false;
                }
            }
        }

        /// <summary>
        /// Schreibt eine generische Logzeile mit Timestamp und Level.
        /// </summary>
        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (message == null) return;

            try
            {
                if (!s_initialized)
                {
                    InitializeLog(s_logPath);
                }

                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";

                lock (s_lock)
                {
                    File.AppendAllText(s_logPath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Wenn Logging fehlschlägt, nicht das Programm stoppen.
            }
        }

        /// <summary>
        /// Loggt eine Anmeldung (Wer hat sich eingeloggt).
        /// </summary>
        public static void LogLogin(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return;
            Log($"Anmeldung: Benutzer '{username}' hat sich angemeldet.", LogLevel.Info);
        }

        /// <summary>
        /// Loggt das Anlegen eines Benutzers.
        /// </summary>
        public static void LogUserCreated(string username, LogLevel level = LogLevel.Info)
        {
            if (string.IsNullOrWhiteSpace(username)) return;
            Log($"Benutzer angelegt: '{username}'.", level);
        }

        /// <summary>
        /// Loggt generelle Aktionen (z.B. "Artikel hinzugefügt").
        /// </summary>
        public static void LogAction(string actionDescription, string performedBy = null)
        {
            if (string.IsNullOrWhiteSpace(actionDescription)) return;

            if (string.IsNullOrWhiteSpace(performedBy))
            {
                Log($"Aktion: {actionDescription}", LogLevel.Info);
            }
            else
            {
                Log($"Aktion: {actionDescription} (ausgeführt von: {performedBy})", LogLevel.Info);
            }
        }

        /// <summary>
        /// Loggt Ausnahmen mit optionalem Kontext.
        /// </summary>
        public static void LogException(Exception ex, string context = null)
        {
            if (ex == null) return;
            var msg = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(context))
            {
                msg.AppendLine($"Kontext: {context}");
            }
            msg.AppendLine($"Exception: {ex.GetType()}: {ex.Message}");
            msg.AppendLine(ex.StackTrace);
            Log(msg.ToString(), LogLevel.Error);
        }

        /// <summary>
        /// Liest das aktuelle Log (falls vorhanden) zurück.
        /// </summary>
        public static string ReadLog()
        {
            try
            {
                lock (s_lock)
                {
                    if (!File.Exists(s_logPath)) return string.Empty;
                    return File.ReadAllText(s_logPath, Encoding.UTF8);
                }
            }
            catch
            {
                return string.Empty;
            }
            
        }
        
    }
    
}
