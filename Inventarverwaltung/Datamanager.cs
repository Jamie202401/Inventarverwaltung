using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Inventarverwaltung
{
    /// <summary>
    /// Zentrale Datenverwaltung für Laden und Speichern aller Daten
    /// ERWEITERT: Schön formatierte, strukturierte Text-Dateien
    /// </summary>
    public static class DataManager
    {
        // Öffentliche Listen für den Zugriff aus anderen Klassen
        public static List<InvId> Inventar = new List<InvId>();
        public static List<MID> Mitarbeiter = new List<MID>();
        public static List<Accounts> Benutzer = new List<Accounts>();
        public static List<Anmelder> Anmeldung = new List<Anmelder>();
        public static List<Lieferant> Lieferanten = new List<Lieferant>(); // ← NEU

        #region Inventar - Laden und Speichern (mit schönem Format)

        /// <summary>
        /// Lädt das gesamte Inventar aus der Datei
        /// Unterstützt sowohl das neue schöne Format als auch alte Formate
        /// </summary>
        public static void LoadInventar()
        {
            Inventar.Clear();

            if (!File.Exists(FileManager.FilePath))
            {
                LogManager.LogDatenGeladen("Inventar", 0);
                return;
            }

            string[] lines = File.ReadAllLines(FileManager.FilePath);
            bool inDataSection = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Überspringe Kommentar- und Header-Zeilen
                if (line.StartsWith("#") || line.StartsWith("=") || line.StartsWith("-") ||
                    line.StartsWith("╔") || line.StartsWith("║") || line.StartsWith("╚") ||
                    line.Contains("INVENTAR-DATENBANK") || line.Contains("Erstellt am:") ||
                    line.Contains("Letzte Änderung:"))
                {
                    continue;
                }

                // Markiere Beginn der Datensektion
                if (line.Contains("[DATEN]"))
                {
                    inDataSection = true;
                    continue;
                }

                // Nur Datenzeilen verarbeiten
                if (!inDataSection && !line.Contains(";")) continue;

                string[] data = line.Split(';');

                // Rückwärtskompatibilität: Alte Dateien (nur 3 Felder)
                if (data.Length == 3)
                {
                    Inventar.Add(new InvId(data[0], data[1], data[2]));
                }
                // Mittlere Version (10 Felder ohne Tracking)
                else if (data.Length == 10)
                {
                    try
                    {
                        Inventar.Add(new InvId(
                            data[0], data[1], data[2], data[3],
                            decimal.Parse(data[4], CultureInfo.InvariantCulture),
                            DateTime.ParseExact(data[5], "dd.MM.yyyy", CultureInfo.InvariantCulture),
                            data[6], data[7],
                            int.Parse(data[8]), int.Parse(data[9])
                        ));
                    }
                    catch { if (data.Length >= 3) Inventar.Add(new InvId(data[0], data[1], data[2])); }
                }
                // Neue Version (12 Felder mit Tracking)
                else if (data.Length >= 12)
                {
                    try
                    {
                        Inventar.Add(new InvId(
                            data[0], data[1], data[2], data[3],
                            decimal.Parse(data[4], CultureInfo.InvariantCulture),
                            DateTime.ParseExact(data[5], "dd.MM.yyyy", CultureInfo.InvariantCulture),
                            data[6], data[7],
                            int.Parse(data[8]), int.Parse(data[9]),
                            data[10],
                            DateTime.ParseExact(data[11], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)
                        ));
                    }
                    catch { if (data.Length >= 3) Inventar.Add(new InvId(data[0], data[1], data[2])); }
                }
            }

            LogManager.LogDatenGeladen("Inventar", Inventar.Count);
        }

        /// <summary>
        /// Speichert den neuesten Inventarartikel in die Datei (APPEND)
        /// Verwendet schönes strukturiertes Format
        /// </summary>
        public static void SaveInvToFile()
        {
            if (Inventar.Count == 0) return;

            // Prüfe ob Datei leer ist oder nicht existiert
            bool dateiIstNeu = !File.Exists(FileManager.FilePath) || new FileInfo(FileManager.FilePath).Length == 0;

            if (dateiIstNeu)
            {
                // Erstelle neue Datei mit Header
                SaveKomplettesInventar();
            }
            else
            {
                // Füge nur den neuen Artikel hinzu
                InvId letzterArtikel = Inventar[Inventar.Count - 1];
                using (StreamWriter sw = new StreamWriter(FileManager.FilePath, true))
                {
                    sw.WriteLine(FormatInventarZeile(letzterArtikel));
                }
            }

            LogManager.LogDatenGespeichert("Inventar", $"Artikel {Inventar[Inventar.Count - 1].InvNmr} hinzugefügt");
        }

        /// <summary>
        /// Speichert das KOMPLETTE Inventar neu mit schönem Format
        /// </summary>
        public static void SaveKomplettesInventar()
        {
            StringBuilder sb = new StringBuilder();

            // Header
            sb.AppendLine("╔════════════════════════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                                                                                    ║");
            sb.AppendLine("║                          INVENTAR-DATENBANK                                        ║");
            sb.AppendLine("║                    🤖 KI-gestützte Inventarverwaltung                              ║");
            sb.AppendLine("║                                                                                    ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"# Erstellt am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"# Letzte Änderung: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"# Anzahl Artikel: {Inventar.Count}");
            sb.AppendLine($"# Verschlüsselung: AES-256 (für Logs)");
            sb.AppendLine();
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine("  STRUKTUR DER DATEN:");
            sb.AppendLine("  InvNr;Gerätename;Mitarbeiter;SNR;Preis;Datum;Hersteller;Kategorie;");
            sb.AppendLine("  Anzahl;Mindestbestand;ErstelltVon;ErstelltAm");
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine("[DATEN]");
            sb.AppendLine();

            // Daten
            foreach (var artikel in Inventar)
            {
                sb.AppendLine(FormatInventarZeile(artikel));
            }

            // Footer
            sb.AppendLine();
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine($"# Ende der Datei - {Inventar.Count} Artikel gespeichert");
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");

            try
            {
                // Entferne "Hidden" und "ReadOnly" Attribute falls gesetzt
                if (File.Exists(FileManager.FilePath))
                {
                    FileAttributes attributes = File.GetAttributes(FileManager.FilePath);
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        File.SetAttributes(FileManager.FilePath, attributes & ~FileAttributes.Hidden);
                    }
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(FileManager.FilePath, attributes & ~FileAttributes.ReadOnly);
                    }
                }

                // Schreibe Datei
                File.WriteAllText(FileManager.FilePath, sb.ToString(), Encoding.UTF8);

                // Setze "Hidden" Attribut wieder zurück falls gewünscht
                // (Kommentieren Sie diese Zeile aus, wenn Dateien NICHT versteckt werden sollen)
                // File.SetAttributes(FileManager.FilePath, File.GetAttributes(FileManager.FilePath) | FileAttributes.Hidden);
            }
            catch (UnauthorizedAccessException)
            {
                // Versuche mit temporärer Datei
                string tempFile = FileManager.FilePath + ".tmp";
                File.WriteAllText(tempFile, sb.ToString(), Encoding.UTF8);

                // Lösche alte Datei und benenne temp um
                if (File.Exists(FileManager.FilePath))
                {
                    File.Delete(FileManager.FilePath);
                }
                File.Move(tempFile, FileManager.FilePath);
            }

            LogManager.LogDatenGespeichert("Inventar", $"Komplettes Inventar ({Inventar.Count} Artikel) gespeichert");
        }

        /// <summary>
        /// Formatiert eine Inventar-Zeile
        /// </summary>
        private static string FormatInventarZeile(InvId artikel)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11}",
                artikel.InvNmr.PadRight(10),
                artikel.GeraeteName.PadRight(30),
                artikel.MitarbeiterBezeichnung.PadRight(25),
                artikel.SerienNummer.PadRight(20),
                artikel.Preis.ToString("F2").PadRight(10),
                artikel.Anschaffungsdatum.ToString("dd.MM.yyyy").PadRight(12),
                artikel.Hersteller.PadRight(15),
                artikel.Kategorie.PadRight(20),
                artikel.Anzahl.ToString().PadRight(5),
                artikel.Mindestbestand.ToString().PadRight(5),
                artikel.ErstelltVon.PadRight(20),
                artikel.ErstelltAm.ToString("dd.MM.yyyy HH:mm:ss")
            );
        }

        #endregion

        #region Mitarbeiter - Laden und Speichern (mit schönem Format)

        /// <summary>
        /// Lädt alle Mitarbeiter aus der Datei
        /// </summary>
        public static void LoadMitarbeiter()
        {
            Mitarbeiter.Clear();

            if (!File.Exists(FileManager.FilePath2))
            {
                LogManager.LogDatenGeladen("Mitarbeiter", 0);
                return;
            }

            string[] lines = File.ReadAllLines(FileManager.FilePath2);
            bool inDataSection = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Überspringe Header-Zeilen
                if (line.StartsWith("#") || line.StartsWith("=") || line.StartsWith("-") ||
                    line.StartsWith("╔") || line.StartsWith("║") || line.StartsWith("╚") ||
                    line.Contains("MITARBEITER-DATENBANK"))
                {
                    continue;
                }

                if (line.Contains("[DATEN]"))
                {
                    inDataSection = true;
                    continue;
                }

                if (!inDataSection && !line.Contains(";")) continue;

                string[] data = line.Split(';');
                if (data.Length >= 3)
                {
                    Mitarbeiter.Add(new MID(data[0].Trim(), data[1].Trim(), data[2].Trim()));
                }
            }

            LogManager.LogDatenGeladen("Mitarbeiter", Mitarbeiter.Count);
        }

        /// <summary>
        /// Speichert alle Mitarbeiter mit schönem Format
        /// </summary>
        public static void SaveMitarbeiterToFile()
        {
            // Prüfe ob Datei leer ist
            bool dateiIstNeu = !File.Exists(FileManager.FilePath2) || new FileInfo(FileManager.FilePath2).Length == 0;

            if (dateiIstNeu)
            {
                SaveKompletteMitarbeiter();
            }
            else
            {
                // Füge nur neuen Mitarbeiter hinzu
                if (Mitarbeiter.Count > 0)
                {
                    MID letzterMitarbeiter = Mitarbeiter[Mitarbeiter.Count - 1];
                    using (StreamWriter sw = new StreamWriter(FileManager.FilePath2, true))
                    {
                        sw.WriteLine(FormatMitarbeiterZeile(letzterMitarbeiter));
                    }
                }
            }
        }

        /// <summary>
        /// Speichert alle Mitarbeiter komplett neu
        /// </summary>
        public static void SaveKompletteMitarbeiter()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("╔════════════════════════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                                                                                    ║");
            sb.AppendLine("║                        MITARBEITER-DATENBANK                                       ║");
            sb.AppendLine("║                    🤖 KI-gestützte Inventarverwaltung                              ║");
            sb.AppendLine("║                                                                                    ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"# Erstellt am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"# Letzte Änderung: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"# Anzahl Mitarbeiter: {Mitarbeiter.Count}");
            sb.AppendLine();
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine("  STRUKTUR DER DATEN:");
            sb.AppendLine("  Vorname;Nachname;Abteilung");
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine("[DATEN]");
            sb.AppendLine();

            foreach (var mitarbeiter in Mitarbeiter)
            {
                sb.AppendLine(FormatMitarbeiterZeile(mitarbeiter));
            }

            sb.AppendLine();
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine($"# Ende der Datei - {Mitarbeiter.Count} Mitarbeiter gespeichert");
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");

            try
            {
                // Entferne "Hidden" und "ReadOnly" Attribute falls gesetzt
                if (File.Exists(FileManager.FilePath2))
                {
                    FileAttributes attributes = File.GetAttributes(FileManager.FilePath2);
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        File.SetAttributes(FileManager.FilePath2, attributes & ~FileAttributes.Hidden);
                    }
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(FileManager.FilePath2, attributes & ~FileAttributes.ReadOnly);
                    }
                }

                // Schreibe Datei
                File.WriteAllText(FileManager.FilePath2, sb.ToString(), Encoding.UTF8);
            }
            catch (UnauthorizedAccessException)
            {
                // Versuche mit temporärer Datei
                string tempFile = FileManager.FilePath2 + ".tmp";
                File.WriteAllText(tempFile, sb.ToString(), Encoding.UTF8);

                if (File.Exists(FileManager.FilePath2))
                {
                    File.Delete(FileManager.FilePath2);
                }
                File.Move(tempFile, FileManager.FilePath2);
            }
        }

        private static string FormatMitarbeiterZeile(MID mitarbeiter)
        {
            return $"{mitarbeiter.VName.PadRight(20)};{mitarbeiter.NName.PadRight(25)};{mitarbeiter.Abteilung.PadRight(25)}";
        }

        #endregion

        #region Benutzer - Laden und Speichern (mit schönem Format)

        /// <summary>
        /// Lädt alle Benutzer aus der Datei
        /// </summary>
        public static void LoadBenutzer()
        {
            if (!File.Exists(FileManager.FilePath3))
            {
                LogManager.LogDatenGeladen("Benutzer", 0);
                return;
            }

            string[] lines = File.ReadAllLines(FileManager.FilePath3);
            Benutzer.Clear();
            bool inDataSection = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Überspringe Header-Zeilen
                if (line.StartsWith("#") || line.StartsWith("=") || line.StartsWith("-") ||
                    line.StartsWith("╔") || line.StartsWith("║") || line.StartsWith("╚") ||
                    line.Contains("BENUTZER-DATENBANK"))
                {
                    continue;
                }

                if (line.Contains("[DATEN]")) { inDataSection = true; continue; }
                if (!inDataSection && !line.Contains(";")) continue;

                // Kommentar-Teil entfernen (alles ab "  #")
                string bereinigt = line;
                int kommentarPos = line.IndexOf("  #");
                if (kommentarPos >= 0)
                    bereinigt = line.Substring(0, kommentarPos);

                // Format: Benutzername;Berechtigung;PasswortHash
                // Rückwärtskompatibel: Benutzername;Berechtigung (kein Hash)
                string[] data = bereinigt.Split(';');
                if (data.Length < 2) continue;

                string name = data[0].Trim();
                string rolleText = data[1].Trim();
                string hash = data.Length >= 3 ? data[2].Trim() : string.Empty;

                if (!Enum.TryParse(rolleText, out Berechtigungen rolle))
                    rolle = Berechtigungen.User;

                Benutzer.Add(new Accounts(name, rolle, hash));
            }

            LogManager.LogDatenGeladen("Benutzer", Benutzer.Count);
        }

        /// <summary>
        /// Speichert alle Benutzer mit schönem Format
        /// </summary>
        public static void SaveBenutzerToFile()
        {
            if (Benutzer.Count == 0) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("╔════════════════════════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                                                                                    ║");
            sb.AppendLine("║                         BENUTZER-DATENBANK                                         ║");
            sb.AppendLine("║                    🤖 KI-gestützte Inventarverwaltung                              ║");
            sb.AppendLine("║                          🔐 AES-256 verschlüsselt                                  ║");
            sb.AppendLine("║                                                                                    ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"# Erstellt am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"# Letzte Änderung: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"# Anzahl Benutzer: {Benutzer.Count}");
            sb.AppendLine();
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine("  STRUKTUR DER DATEN:");
            sb.AppendLine("  Benutzername;Berechtigung");
            sb.AppendLine("  ");
            sb.AppendLine("  Berechtigungen: User | Admin");
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine("[DATEN]");
            sb.AppendLine();

            foreach (var acc in Benutzer)
            {
                string icon = acc.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";
                string hash = acc.PasswortHash ?? string.Empty;

                // WICHTIG: PasswortHash muss mitgespeichert werden!
                // Format: Benutzername;Berechtigung;PasswortHash  # Icon
                sb.AppendLine($"{acc.Benutzername.PadRight(25)};{acc.Berechtigung.ToString().PadRight(10)};{hash}  # {icon}");
            }

            sb.AppendLine();
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");
            sb.AppendLine($"# Ende der Datei - {Benutzer.Count} Benutzer gespeichert");
            sb.AppendLine("════════════════════════════════════════════════════════════════════════════════════");

            try
            {
                // Entferne "Hidden" und "ReadOnly" Attribute falls gesetzt
                if (File.Exists(FileManager.FilePath3))
                {
                    FileAttributes attributes = File.GetAttributes(FileManager.FilePath3);
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        File.SetAttributes(FileManager.FilePath3, attributes & ~FileAttributes.Hidden);
                    }
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(FileManager.FilePath3, attributes & ~FileAttributes.ReadOnly);
                    }
                }

                // Schreibe Datei
                File.WriteAllText(FileManager.FilePath3, sb.ToString(), Encoding.UTF8);
            }
            catch (UnauthorizedAccessException)
            {
                // Versuche mit temporärer Datei
                string tempFile = FileManager.FilePath3 + ".tmp";
                File.WriteAllText(tempFile, sb.ToString(), Encoding.UTF8);

                if (File.Exists(FileManager.FilePath3))
                {
                    File.Delete(FileManager.FilePath3);
                }
                File.Move(tempFile, FileManager.FilePath3);
            }
        }

        #endregion

        #region Bestandsverwaltung

        public static bool BestandErhoehen(string invNr, int menge)
        {
            var artikel = Inventar.FirstOrDefault(a => a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase));
            if (artikel == null) return false;

            artikel.Anzahl += menge;
            SaveKomplettesInventar();

            LogManager.LogDatenGespeichert("Bestand", $"{invNr}: +{menge} → Neu: {artikel.Anzahl}");
            return true;
        }

        public static bool BestandVerringern(string invNr, int menge)
        {
            var artikel = Inventar.FirstOrDefault(a => a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase));
            if (artikel == null) return false;

            if (artikel.Anzahl < menge)
            {
                LogManager.LogWarnung("Bestand", $"{invNr}: Nicht genug Bestand ({artikel.Anzahl} < {menge})");
                return false;
            }

            artikel.Anzahl -= menge;
            SaveKomplettesInventar();

            LogManager.LogDatenGespeichert("Bestand", $"{invNr}: -{menge} → Neu: {artikel.Anzahl}");
            return true;
        }

        public static bool MindestbestandAendern(string invNr, int neuerMindestbestand)
        {
            var artikel = Inventar.FirstOrDefault(a => a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase));
            if (artikel == null) return false;

            int alterWert = artikel.Mindestbestand;
            artikel.Mindestbestand = neuerMindestbestand;
            SaveKomplettesInventar();

            LogManager.LogDatenGespeichert("Mindestbestand", $"{invNr}: {alterWert} → {neuerMindestbestand}");
            return true;
        }

        public static List<InvId> GetArtikelUnterMindestbestand()
        {
            return Inventar.Where(a => a.Anzahl <= a.Mindestbestand).ToList();
        }

        private static readonly string LIEFERANTEN_PFAD = "lieferanten.json";

        /// <summary>
        /// Lädt alle Lieferanten aus lieferanten.json
        /// Wird beim Programmstart in LoadingScreen.cs aufgerufen
        /// </summary>
        public static void LoadLieferanten()
        {
            try
            {
                if (!File.Exists(LIEFERANTEN_PFAD))
                {
                    Lieferanten = new List<Lieferant>();
                    LogManager.LogDatenGeladen("Lieferanten", 0);
                    return;
                }

                string json = File.ReadAllText(LIEFERANTEN_PFAD, Encoding.UTF8);

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Lieferanten = System.Text.Json.JsonSerializer.Deserialize<List<Lieferant>>(json, options)
                              ?? new List<Lieferant>();

                LogManager.LogDatenGeladen("Lieferanten", Lieferanten.Count);
            }
            catch (Exception ex)
            {
                LogManager.LogFehler("LoadLieferanten", ex.Message);
                Lieferanten = new List<Lieferant>();
            }
        }

        /// <summary>
        /// Speichert alle Lieferanten in lieferanten.json
        /// Wird nach jeder Änderung (Anlegen / Bearbeiten / Löschen) aufgerufen
        /// </summary>
        public static void SaveLieferanten()
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                string json = System.Text.Json.JsonSerializer.Serialize(Lieferanten, options);
                File.WriteAllText(LIEFERANTEN_PFAD, json, Encoding.UTF8);

                LogManager.LogDatenGespeichert("Lieferanten", $"{Lieferanten.Count} Lieferant(en) gespeichert");
            }
            catch (Exception ex)
            {
                LogManager.LogFehler("SaveLieferanten", ex.Message);
            }
        }

        public static (int gesamt, int leer, int niedrig, int ok) GetBestandsStatistik()
        {
            int gesamt = Inventar.Count;
            int leer = Inventar.Count(a => a.Anzahl == 0);
            int niedrig = Inventar.Count(a => a.Anzahl > 0 && a.Anzahl <= a.Mindestbestand);
            int ok = Inventar.Count(a => a.Anzahl > a.Mindestbestand);

            return (gesamt, leer, niedrig, ok);
        }

        #endregion
    }
}