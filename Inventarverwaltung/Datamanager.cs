using System;
using System.Collections.Generic;
using System.IO;

namespace Inventarverwaltung
{
    /// <summary>
    /// Zentrale Datenverwaltung für Laden und Speichern aller Daten
    /// </summary>
    public static class DataManager
    {
        // Öffentliche Listen für den Zugriff aus anderen Klassen
        public static List<InvId> Inventar = new List<InvId>();
        public static List<MID> Mitarbeiter = new List<MID>();
        public static List<Accounts> Benutzer = new List<Accounts>();
        public static List<Anmelder> Anmeldung = new List<Anmelder>();

        #region Anmeldung - Laden und Speichern

        /// <summary>
        /// Lädt alle Anmeldedaten aus der Datei
        /// </summary>
        public static void LoadAnmeldung()
        {
            Anmeldung.Clear();
            if (!File.Exists(FileManager.FilePath4)) return;

            string[] lines = File.ReadAllLines(FileManager.FilePath4);
            foreach (var line in lines)
            {
                string name = line.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    Anmeldung.Add(new Anmelder(name));
                }
            }
        }

        /// <summary>
        /// Speichert neue Anmeldedaten in die Datei
        /// </summary>
        public static void SaveIntoNewAccounts()
        {
            using (StreamWriter sw = new StreamWriter(FileManager.FilePath4, true))
            {
                foreach (var benutzer in Anmeldung)
                {
                    sw.WriteLine(benutzer.Anmeldename);
                }
            }
        }

        #endregion

        #region Mitarbeiter - Laden und Speichern

        /// <summary>
        /// Lädt alle Mitarbeiter aus der Datei
        /// </summary>
        public static void LoadMitarbeiter()
        {
            if (File.Exists(FileManager.FilePath2))
            {
                string[] mitarbeiter = File.ReadAllLines(FileManager.FilePath2);
                foreach (var line in mitarbeiter)
                {
                    string[] mitarbeiterData = line.Split(';');
                    if (mitarbeiterData.Length >= 3)
                    {
                        Mitarbeiter.Add(new MID(mitarbeiterData[0], mitarbeiterData[1], mitarbeiterData[2]));
                    }
                }
            }
        }

        /// <summary>
        /// Speichert alle Mitarbeiter in die Datei
        /// </summary>
        public static void SaveMitarbeiterToFile()
        {
            using (StreamWriter sw = new StreamWriter(FileManager.FilePath2, true))
            {
                foreach (MID mitarbeiter in Mitarbeiter)
                {
                    sw.WriteLine($"{mitarbeiter.VName};{mitarbeiter.NName};{mitarbeiter.Abteilung}");
                }
            }
        }

        #endregion

        #region Inventar - Laden und Speichern

        /// <summary>
        /// Lädt das gesamte Inventar aus der Datei
        /// </summary>
        public static void LoadInventar()
        {
            if (File.Exists(FileManager.FilePath))
            {
                string[] inventar = File.ReadAllLines(FileManager.FilePath);
                foreach (var line in inventar)
                {
                    string[] inventarData = line.Split(';');
                    if (inventarData.Length >= 3)
                    {
                        Inventar.Add(new InvId(inventarData[0], inventarData[1], inventarData[2]));
                    }
                }
            }
        }

        /// <summary>
        /// Speichert den neuesten Inventarartikel in die Datei
        /// </summary>
        public static void SaveInvToFile()
        {
            if (Inventar.Count == 0) return;

            InvId letzterInventar = Inventar[Inventar.Count - 1];
            using (StreamWriter sw = new StreamWriter(FileManager.FilePath, true))
            {
                sw.WriteLine($"{letzterInventar.InvNmr};{letzterInventar.GeraeteName};{letzterInventar.MitarbeiterBezeichnung}");
            }
        }

        #endregion

        #region Benutzer - Laden und Speichern

        /// <summary>
        /// Lädt alle Benutzer aus der Datei
        /// </summary>
        public static void LoadBenutzer()
        {
            if (!File.Exists(FileManager.FilePath3)) return;

            string[] lines = File.ReadAllLines(FileManager.FilePath3);
            Benutzer.Clear();

            foreach (var line in lines)
            {
                string[] data = line.Split(';');
                if (data.Length != 2) continue;

                string name = data[0].Trim();
                string rolleText = data[1].Trim();

                if (!Enum.TryParse(rolleText, out Berechtigungen rolle))
                {
                    rolle = Berechtigungen.Unbekannt;
                }

                Benutzer.Add(new Accounts(name, rolle));
            }
        }

        /// <summary>
        /// Speichert alle Benutzer in die Datei
        /// </summary>
        public static void SaveBenutzerToFile()
        {
            if (Benutzer.Count == 0) return;

            using (StreamWriter sw = new StreamWriter(FileManager.FilePath3, true))
            {
                foreach (Accounts acc in Benutzer)
                {
                    sw.WriteLine($"{acc.Benutzername};{acc.Berechtigung}");
                }
            }
        }

        #endregion
    }
}