using System.Collections.Generic;
using System.IO;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet alle Dateipfade und Dateioperationen
	/// AKTUALISIERT: Dateien werden NICHT mehr versteckt um "Access Denied" zu vermeiden
    /// </summary>
    public static class FileManager
    {
        // Dateipfade für die verschiedenen Datentypen
        public static string FilePath = "Inventar.txt";      // Inventardaten
        public static string FilePath2 = "Mitarbeiter.txt";  // Mitarbeiterdaten
        public static string FilePath3 = "Accounts.txt";     // Benutzerdaten
     
        /// <summary>
        /// Erstellt eine Datei (falls nicht vorhanden) OHNE sie zu verstecken
        /// </summary>
        static void CreateFile(string path)
        {
                // Datei erstellen, falls sie noch nicht existiert
                if (!File.Exists(path))
                {
                        File.Create(path).Close();
				}

					// Entferne Hidden/ReadOnly Attribute falls vorhanden
				if (File.Exists(path))
				{
                FileAttributes attributes = File.GetAttributes(path);

				// Entferne Hidden
				if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
				{
					File.SetAttributes(path, attributes & ~FileAttributes.Hidden);
				}
				
				// Entferne ReadOnly
				if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
				}
			}
		}

        /// <summary>
        /// Erstellt alle Programmdateien (OHNE sie zu verstecken)
		/// </summary>
		public static void InitializeFiles()
		{
			CreateFile(FilePath);
			CreateFile(FilePath2);
			CreateFile(FilePath3);
		}
		
		/// <summary>
		/// Legacy-Funktion: HideAllFiles wird nicht mehr verwendet
		/// Stattdessen: InitializeFiles
		/// </summary>
		public static void HideAllFiles()
        {
            // NICHT MEHR VERSTECKEN - nur erstellen
			InitializeFiles();
		}

		/// <summary>
		/// Entfernt alle Datei-Attribute die Probleme verursachen könnten
		/// </summary>
		public static void FixFileAttributes()
		{
			FixFileAttribute(FilePath);
			FixFileAttribute(FilePath2);
			FixFileAttribute(FilePath3);
		}

		/// <summary>
		/// Repariert Datei-Attribute einer einzelnen Datei
		/// </summary>
		private static void FixFileAttribute(string path)
		{
			if (!File.Exists(path)) return;

			try
			{
				FileAttributes attributes = File.GetAttributes(path);

				// Entferne problematische Attribute
				attributes &= ~FileAttributes.Hidden;
				attributes &= ~FileAttributes.ReadOnly;
				attributes &= ~FileAttributes.System;

				File.SetAttributes(path, attributes);
			}
			catch
			{
				// Ignoriere Fehler - kann passieren wenn Datei gerade in Benutzung ist
			}
        }
    }
}