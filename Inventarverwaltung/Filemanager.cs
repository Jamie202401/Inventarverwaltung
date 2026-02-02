using System.Collections.Generic;
using System.IO;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Dateipfade und Dateioperationen
    /// </summary>
    public static class FileManager
    {
        // Dateipfade für die verschiedenen Datentypen
        public static string FilePath = "Inventar.txt";      // Inventardaten
        public static string FilePath2 = "Mitarbeiter.txt";  // Mitarbeiterdaten
        public static string FilePath3 = "Accounts.txt";     // Benutzerdaten
        public static string FilePath4 = "Anmelder.txt";     // Anmeldedaten

        /// <summary>
        /// Erstellt eine Datei (falls nicht vorhanden) und versteckt sie
        /// </summary>
        static void CreateAndHideFile(string path)
        {
            // Datei erstellen, falls sie noch nicht existiert
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }

            // Datei als versteckt markieren
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
        }

        /// <summary>
        /// Versteckt alle Programmdateien
        /// </summary>
        public static void HideAllFiles()
        {
            CreateAndHideFile(FilePath);
            CreateAndHideFile(FilePath2);
            CreateAndHideFile(FilePath3);
            CreateAndHideFile(FilePath4);
        }
    }
}