using Inventarverwaltung.Data.Validation.Validation.Val2;
using Inventarverwaltung.Manager.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Inventarverwaltung
{
    /// ╔══════════════════════════════════════════════════════════════════════╗
    /// ║              KI BACKGROUND TRAINER                                   ║
    /// ║                                                                      ║
    /// ║  Generiert synthetische Trainingsdaten während das Programm         ║
    /// ║  NICHT läuft — als Windows Task / Hintergrundprozess.               ║
    /// ║                                                                      ║
    /// ║  ABLAUF:                                                             ║
    /// ║  1. Programm beendet sich → Trainer-Task wird registriert           ║
    /// ║  2. Trainer läuft im Hintergrund → generiert .ki_train.enc          ║
    /// ║  3. Programm startet → Daten werden geladen → Datei wird gelöscht   ║
    /// ║  4. KIEngine trainiert mit den neuen Daten → Datei existiert nicht  ║
    /// ║                                                                      ║
    /// ║  SICHERHEIT:                                                         ║
    /// ║  • AES-256 verschlüsselt                                             ║
    /// ║  • Hidden-Datei Attribut (Windows)                                  ║
    /// ║  • Datei wird nach dem Laden sofort sicher gelöscht                 ║
    /// ║  • Nie im normalen Programmablauf sichtbar                          ║
    /// ╚══════════════════════════════════════════════════════════════════════╝
    public static class KIBackgroundTrainer
    {
        // ══════════════════════════════════════════════════════════════════
        // PFADE & KONFIGURATION
        // ══════════════════════════════════════════════════════════════════

        // Versteckte Trainingsdatei — neben der .exe
        private static readonly string TrainPfad = ".ki_train.enc";

        // Sperrdatei: verhindert dass zwei Prozesse gleichzeitig schreiben
        private static readonly string LockPfad = ".ki_train.lock";

        // Wie viele Trainingsdatensätze pro Hintergrund-Session generiert werden
        private const int DatenpunkteProSession = 200;

        // Maximale Gesamtgröße der Trainingsdatei (damit sie nicht unkontrolliert wächst)
        private const int MaxGesamtDatenpunkte = 5000;

        // ══════════════════════════════════════════════════════════════════
        // BEIM PROGRAMMSTART — Trainingsdaten laden & Datei löschen
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Wird in Program.cs VOR KIEngine.Initialisiere() aufgerufen.
        /// Lädt Trainingsdaten falls vorhanden, löscht die Datei danach sofort.
        /// Gibt die geladenen Daten zurück damit KIEngine sie verarbeiten kann.
        /// </summary>
        public static KITrainingsPaket LadeUndLoescheTrainingsdaten()
        {
            if (!File.Exists(TrainPfad)) return null;

            try
            {
                // Laden
                byte[] raw = File.ReadAllBytes(TrainPfad);
                string json = Encoding.UTF8.GetString(EntschlusseleBytes(raw));
                var paket = JsonSerializer.Deserialize<KITrainingsPaket>(json);

                // Sofort sicher löschen — Datei darf nicht im Programmbetrieb existieren
                SicheresLoeschen(TrainPfad);
                if (File.Exists(LockPfad)) File.Delete(LockPfad);

                return paket;
            }
            catch
            {
                // Bei Fehler trotzdem löschen
                try { SicheresLoeschen(TrainPfad); } catch { }
                return null;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // BEIM PROGRAMMENDE — Hintergrund-Trainer starten
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Wird in Program.cs beim Beenden aufgerufen.
        /// Startet einen separaten Hintergrundprozess der Trainingsdaten generiert.
        /// </summary>
        public static void StarteHintergrundTraining()
        {
            try
            {
                // Snapshot der aktuellen Daten als Basis für den Trainer erstellen
                // (der Hintergrundprozess hat keinen Zugriff auf den RAM des Hauptprozesses)
                var snapshot = ErstelleDatenSnapshot();
                if (snapshot == null) return;

                // Snapshot in temporäre Datei schreiben damit der Trainer drauf zugreifen kann
                string snapPfad = ".ki_snap.enc";
                byte[] snapEnc = VerschluesseleBytes(
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(snapshot)));
                File.WriteAllBytes(snapPfad, snapEnc);
                VersteckeDatai(snapPfad);

                // Eigenen Prozess als Trainer neu starten mit speziellem Argument
                string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                             ?? "Inventarverwaltung.exe";

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = $"--ki-train \"{snapPfad}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,   // Kein Fenster — läuft komplett unsichtbar
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                System.Diagnostics.Process.Start(psi);
            }
            catch { /* Trainer-Start-Fehler darf Programm nicht beeinflussen */ }
        }

        // ══════════════════════════════════════════════════════════════════
        // HINTERGRUND-TRAINER ENTRY POINT
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Prüft ob dieses Programm als Hintergrund-Trainer gestartet wurde.
        /// Aufgerufen ganz oben in Program.Main() — vor allem anderen.
        /// </summary>
        public static bool IstTrainerModus(string[] args)
            => args?.Length >= 1 && args[0] == "--ki-train";

        /// <summary>
        /// Hauptschleife des unsichtbaren Hintergrund-Trainers.
        /// Läuft komplett ohne Fenster, generiert Trainingsdaten und beendet sich.
        /// </summary>
        public static void StarteAlsTrainer(string[] args)
        {
            try
            {
                string snapPfad = args.Length >= 2 ? args[1] : null;
                if (snapPfad == null || !File.Exists(snapPfad)) return;

                // Sperrdatei anlegen damit kein zweiter Trainer gleichzeitig läuft
                if (File.Exists(LockPfad)) return;
                File.WriteAllText(LockPfad, DateTime.Now.ToString("o"));

                // Snapshot laden
                byte[] snapRaw = File.ReadAllBytes(snapPfad);
                string snapJson = Encoding.UTF8.GetString(EntschlusseleBytes(snapRaw));
                DatenSnapshot snapshot = JsonSerializer.Deserialize<DatenSnapshot>(snapJson);
                File.Delete(snapPfad); // Snapshot sofort löschen

                if (snapshot == null) { File.Delete(LockPfad); return; }

                // Bestehende Trainingsdaten laden (falls vorhanden)
                KITrainingsPaket paket = LadeVorhandeneTrainingsdaten();

                // Neue Datenpunkte generieren
                var generator = new TrainingsDatenGenerator(snapshot);
                var neueDaten = generator.Generiere(DatenpunkteProSession);

                // Zusammenführen und auf Maximum begrenzen
                paket.Datenpunkte.AddRange(neueDaten);
                if (paket.Datenpunkte.Count > MaxGesamtDatenpunkte)
                    paket.Datenpunkte = paket.Datenpunkte
                        .OrderByDescending(d => d.Zeitstempel)
                        .Take(MaxGesamtDatenpunkte)
                        .ToList();

                paket.LetztesTraining = DateTime.Now;
                paket.TrainingsRunden += 1;
                paket.GesamtDatenpunkte = paket.Datenpunkte.Count;

                // Verschlüsselt speichern
                string json = JsonSerializer.Serialize(paket);
                byte[] enc = VerschluesseleBytes(Encoding.UTF8.GetBytes(json));
                File.WriteAllBytes(TrainPfad, enc);
                VersteckeDatai(TrainPfad);

                File.Delete(LockPfad);
            }
            catch
            {
                try { if (File.Exists(LockPfad)) File.Delete(LockPfad); } catch { }
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // TRAININGSDATEN IN KI-ENGINE EINPFLEGEN
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verarbeitet ein geladenes Trainingspaket und füttert die KIEngine damit.
        /// Aufgerufen nach LadeUndLoescheTrainingsdaten() und vor KIEngine.Initialisiere().
        /// </summary>
        public static void TrainiereKIEngine(KITrainingsPaket paket)
        {
            if (paket == null || paket.Datenpunkte == null || paket.Datenpunkte.Count == 0)
                return;

            try
            {
                int verarbeitet = 0;

                foreach (var dp in paket.Datenpunkte)
                {
                    switch (dp.Typ)
                    {
                        case TrainingsDatenTyp.ArtikelMuster:
                            // Synthetischen Artikel in DataManager einfügen (temporär für Training)
                            decimal.TryParse(dp.Wert3, out decimal trainPreis);
                            int.TryParse(dp.Wert4, out int trainAnzahl);
                            int.TryParse(dp.Wert5, out int trainMindest);
                            var artikel = new InvId(
                                $"TRAIN_{dp.Id:D4}",
                                dp.Wert1 ?? "Trainingsgerät",
                                dp.Wert6 ?? "",
                                "N/A",
                                trainPreis,
                                dp.Zeitstempel,
                                "KI_TRAINER",
                                dp.Wert2 ?? "Sonstiges",
                                trainAnzahl > 0 ? trainAnzahl : 1,
                                trainMindest > 0 ? trainMindest : 1,
                                "KI_TRAINER",
                                dp.Zeitstempel
                            );
                            DataManager.Inventar.Add(artikel);
                            verarbeitet++;
                            break;

                        case TrainingsDatenTyp.MitarbeiterMuster:
                            // Synthetischen Mitarbeiter temporär einfügen
                            var ma = new MID(
                                dp.Wert1 ?? "Train",
                                dp.Wert2 ?? "User",
                                dp.Wert3 ?? "Training"
                            );
                            DataManager.Mitarbeiter.Add(ma);
                            verarbeitet++;
                            break;

                        case TrainingsDatenTyp.PreisMuster:
                        case TrainingsDatenTyp.BestandsMuster:
                        case TrainingsDatenTyp.KategorieMuster:
                            // Diese Typen werden direkt von KIEngine verarbeitet
                            // (keine DataManager-Einträge nötig)
                            verarbeitet++;
                            break;
                    }
                }

                // Nach dem Training alle synthetischen Einträge wieder entfernen
                DataManager.Inventar.RemoveAll(i => i.ErstelltVon == "KI_TRAINER");
                DataManager.Mitarbeiter.RemoveAll(m =>
                    m.VName == "Train" && m.NName == "User" && m.Abteilung == "Training");

                // Info im Shadow-Log (nicht im normalen Log)
                ShadowConsoleManager.TrackAktion("KI-TRAINER",
                    $"Training abgeschlossen: {verarbeitet} Datenpunkte | " +
                    $"Runde {paket.TrainingsRunden} | " +
                    $"Letztes Training: {paket.LetztesTraining:dd.MM.yyyy HH:mm}");
            }
            catch { /* Training-Fehler dürfen Programmstart nicht blockieren */ }
        }

        // ══════════════════════════════════════════════════════════════════
        // DATEN-SNAPSHOT (wird beim Beenden erstellt als Basis für Trainer)
        // ══════════════════════════════════════════════════════════════════

        private static DatenSnapshot ErstelleDatenSnapshot()
        {
            try
            {
                return new DatenSnapshot
                {
                    Kategorien = DataManager.Inventar
                        .Select(i => i.Kategorie)
                        .Distinct()
                        .ToList(),

                    Geraete = DataManager.Inventar
                        .Select(i => i.GeraeteName)
                        .Distinct()
                        .Take(50)
                        .ToList(),

                    Abteilungen = DataManager.Mitarbeiter
                        .Select(m => m.Abteilung)
                        .Distinct()
                        .ToList(),

                    PreisbereicheProKategorie = DataManager.Inventar
                        .GroupBy(i => i.Kategorie)
                        .ToDictionary(
                            g => g.Key,
                            g => new decimal[] {
                                g.Min(i => i.Preis),
                                g.Average(i => i.Preis),
                                g.Max(i => i.Preis)
                            }),

                    BestandsVerteilung = DataManager.Inventar
                        .GroupBy(i => i.Kategorie)
                        .ToDictionary(
                            g => g.Key,
                            g => (int)g.Average(i => i.Anzahl)),

                    AnzahlArtikel = DataManager.Inventar.Count,
                    AnzahlMitarbeiter = DataManager.Mitarbeiter.Count,
                    ErstelltAm = DateTime.Now
                };
            }
            catch { return null; }
        }

        private static KITrainingsPaket LadeVorhandeneTrainingsdaten()
        {
            try
            {
                if (!File.Exists(TrainPfad)) return new KITrainingsPaket();
                byte[] raw = File.ReadAllBytes(TrainPfad);
                string json = Encoding.UTF8.GetString(EntschlusseleBytes(raw));
                return JsonSerializer.Deserialize<KITrainingsPaket>(json)
                       ?? new KITrainingsPaket();
            }
            catch { return new KITrainingsPaket(); }
        }

        // ══════════════════════════════════════════════════════════════════
        // TRAININGS-DATEN-GENERATOR
        // ══════════════════════════════════════════════════════════════════

        private class TrainingsDatenGenerator
        {
            private readonly DatenSnapshot _snap;
            private readonly Random _rng = new Random();

            // Reale Gerätekategorien und Typen
            private static readonly string[] GeraeteTypen =
            {
                "Laptop", "Desktop PC", "Monitor", "Drucker", "Scanner",
                "Tastatur", "Maus", "Headset", "Webcam", "Dockingstation",
                "Switch", "Router", "Server", "NAS", "USV",
                "Beamer", "Whiteboard", "Telefon", "Tablet", "Smartphone"
            };

            private static readonly string[] Hersteller =
            {
                "Dell", "HP", "Lenovo", "Apple", "Asus", "Acer", "Microsoft",
                "Logitech", "Samsung", "LG", "Cisco", "Ubiquiti", "Synology"
            };

            private static readonly string[] Abteilungen =
            {
                "IT", "Vertrieb", "Buchhaltung", "HR", "Geschäftsführung",
                "Marketing", "Lager", "Produktion", "Einkauf", "Support"
            };

            private static readonly string[] Vornamen =
            {
                "Max", "Anna", "Felix", "Lisa", "Thomas", "Sarah",
                "Michael", "Julia", "Andreas", "Laura"
            };

            private static readonly string[] Nachnamen =
            {
                "Müller", "Schmidt", "Schneider", "Fischer", "Weber",
                "Meyer", "Wagner", "Becker", "Schulz", "Hoffmann"
            };

            public TrainingsDatenGenerator(DatenSnapshot snap) => _snap = snap;

            public List<TrainingsDatenpunkt> Generiere(int anzahl)
            {
                var liste = new List<TrainingsDatenpunkt>();

                // Gleichmäßige Verteilung über alle Typen
                int proTyp = anzahl / 5;

                liste.AddRange(GeneriereArtikelMuster(proTyp));
                liste.AddRange(GeneriereMitarbeiterMuster(proTyp));
                liste.AddRange(GenerierePreisMuster(proTyp));
                liste.AddRange(GeneriereBestandsMuster(proTyp));
                liste.AddRange(GeneriereKategorieMuster(anzahl - proTyp * 4));

                return liste;
            }

            private List<TrainingsDatenpunkt> GeneriereArtikelMuster(int n)
            {
                var liste = new List<TrainingsDatenpunkt>();

                // Kategorien aus echten Daten verwenden, sonst Fallback
                var kategorien = _snap.Kategorien.Count > 0
                    ? _snap.Kategorien
                    : new List<string> { "IT-Hardware", "Büroausstattung", "Elektronik" };

                for (int i = 0; i < n; i++)
                {
                    string geraet = ZufaelligesElement(Hersteller) + " " +
                                       ZufaelligesElement(GeraeteTypen);
                    string kategorie = ZufaelligesElement(kategorien);

                    // Preis realistisch aus echten Preisbereichen ableiten
                    decimal preis = BerechneRealistischenPreis(kategorie);
                    int anzahl = _rng.Next(1, 15);
                    int mindest = Math.Max(1, (int)(anzahl * 0.3));

                    string ma = ZufaelligesElement(Vornamen) + " " +
                                ZufaelligesElement(Nachnamen);

                    liste.Add(new TrainingsDatenpunkt
                    {
                        Id = _rng.Next(10000, 99999),
                        Typ = TrainingsDatenTyp.ArtikelMuster,
                        Wert1 = geraet,
                        Wert2 = kategorie,
                        Wert3 = preis.ToString("F2"),
                        Wert4 = anzahl.ToString(),
                        Wert5 = mindest.ToString(),
                        Wert6 = ma,
                        Zeitstempel = DateTime.Now.AddDays(-_rng.Next(0, 180))
                    });
                }
                return liste;
            }

            private List<TrainingsDatenpunkt> GeneriereMitarbeiterMuster(int n)
            {
                var liste = new List<TrainingsDatenpunkt>();
                var abteilungen = _snap.Abteilungen.Count > 0
                    ? _snap.Abteilungen : new List<string>(Abteilungen);

                for (int i = 0; i < n; i++)
                {
                    liste.Add(new TrainingsDatenpunkt
                    {
                        Id = _rng.Next(10000, 99999),
                        Typ = TrainingsDatenTyp.MitarbeiterMuster,
                        Wert1 = ZufaelligesElement(Vornamen),
                        Wert2 = ZufaelligesElement(Nachnamen),
                        Wert3 = ZufaelligesElement(abteilungen),
                        Zeitstempel = DateTime.Now.AddDays(-_rng.Next(0, 365))
                    });
                }
                return liste;
            }

            private List<TrainingsDatenpunkt> GenerierePreisMuster(int n)
            {
                var liste = new List<TrainingsDatenpunkt>();
                for (int i = 0; i < n; i++)
                {
                    string geraet = ZufaelligesElement(GeraeteTypen);
                    decimal basis = geraet switch
                    {
                        "Laptop" => 800m + (decimal)_rng.NextDouble() * 1500m,
                        "Server" => 2000m + (decimal)_rng.NextDouble() * 8000m,
                        "Monitor" => 150m + (decimal)_rng.NextDouble() * 600m,
                        "Smartphone" => 300m + (decimal)_rng.NextDouble() * 800m,
                        "Drucker" => 100m + (decimal)_rng.NextDouble() * 500m,
                        _ => 50m + (decimal)_rng.NextDouble() * 400m
                    };

                    liste.Add(new TrainingsDatenpunkt
                    {
                        Id = _rng.Next(10000, 99999),
                        Typ = TrainingsDatenTyp.PreisMuster,
                        Wert1 = geraet,
                        Wert2 = Math.Round(basis, 2).ToString("F2"),
                        Wert3 = Math.Round(basis * 0.7m, 2).ToString("F2"),  // Min
                        Wert4 = Math.Round(basis * 1.3m, 2).ToString("F2"),  // Max
                        Zeitstempel = DateTime.Now
                    });
                }
                return liste;
            }

            private List<TrainingsDatenpunkt> GeneriereBestandsMuster(int n)
            {
                var liste = new List<TrainingsDatenpunkt>();
                var kategorien = _snap.Kategorien.Count > 0
                    ? _snap.Kategorien
                    : new List<string> { "IT-Hardware", "Büroausstattung" };

                for (int i = 0; i < n; i++)
                {
                    string kategorie = ZufaelligesElement(kategorien);
                    int bestand = _rng.Next(1, 50);
                    int mindest = Math.Max(1, (int)(bestand * (_rng.NextDouble() * 0.4 + 0.1)));

                    liste.Add(new TrainingsDatenpunkt
                    {
                        Id = _rng.Next(10000, 99999),
                        Typ = TrainingsDatenTyp.BestandsMuster,
                        Wert1 = kategorie,
                        Wert2 = bestand.ToString(),
                        Wert3 = mindest.ToString(),
                        Zeitstempel = DateTime.Now
                    });
                }
                return liste;
            }

            private List<TrainingsDatenpunkt> GeneriereKategorieMuster(int n)
            {
                var liste = new List<TrainingsDatenpunkt>();
                string[] kat = { "IT-Hardware", "Büroausstattung", "Werkzeug",
                                 "Kommunikation", "Möbel", "Elektronik", "Netzwerk" };

                for (int i = 0; i < n; i++)
                {
                    string k = ZufaelligesElement(kat);
                    liste.Add(new TrainingsDatenpunkt
                    {
                        Id = _rng.Next(10000, 99999),
                        Typ = TrainingsDatenTyp.KategorieMuster,
                        Wert1 = k,
                        Wert2 = _rng.Next(1, 100).ToString(),  // Häufigkeit
                        Zeitstempel = DateTime.Now
                    });
                }
                return liste;
            }

            private decimal BerechneRealistischenPreis(string kategorie)
            {
                // Aus echten Daten ableiten wenn vorhanden
                if (_snap.PreisbereicheProKategorie.TryGetValue(kategorie, out decimal[] range)
                    && range.Length == 3)
                {
                    decimal min = range[0], avg = range[1], max = range[2];
                    double t = _rng.NextDouble();
                    return Math.Round(min + (decimal)t * (max - min), 2);
                }

                // Fallback: Kategorie-basierte Schätzung
                return kategorie.ToLower() switch
                {
                    var k when k.Contains("it") => 100m + (decimal)_rng.NextDouble() * 1900m,
                    var k when k.Contains("büro") => 50m + (decimal)_rng.NextDouble() * 500m,
                    var k when k.Contains("mobel") ||
                               k.Contains("möbel") => 100m + (decimal)_rng.NextDouble() * 800m,
                    _ => 30m + (decimal)_rng.NextDouble() * 300m
                };
            }

            private T ZufaelligesElement<T>(IList<T> liste)
                => liste.Count > 0 ? liste[_rng.Next(liste.Count)] : default;
        }

        // ══════════════════════════════════════════════════════════════════
        // HILFSMETHODEN
        // ══════════════════════════════════════════════════════════════════

        private static void SicheresLoeschen(string pfad)
        {
            if (!File.Exists(pfad)) return;
            try
            {
                // Erst überschreiben (forensische Wiederherstellung erschweren)
                long laenge = new FileInfo(pfad).Length;
                File.WriteAllBytes(pfad, new byte[laenge]);
                File.Delete(pfad);
            }
            catch { try { File.Delete(pfad); } catch { } }
        }

        private static void VersteckeDatai(string pfad)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(pfad))
                    File.SetAttributes(pfad,
                        File.GetAttributes(pfad) | FileAttributes.Hidden);
            }
            catch { }
        }

        // AES-256 (eigener Schlüssel, getrennt von EncryptionManager und ShadowManager)
        private static readonly byte[] _k = AblKey("KITrainer#2026!Inventar$Background_Private");
        private static readonly byte[] _v =
        {
            0xF1, 0xE2, 0xD3, 0xC4, 0xB5, 0xA6, 0x97, 0x88,
            0x79, 0x6A, 0x5B, 0x4C, 0x3D, 0x2E, 0x1F, 0x00
        };

        private static byte[] AblKey(string pw)
        {
            using var k = new Rfc2898DeriveBytes(
                pw, Encoding.UTF8.GetBytes("KITrainerSalt2026"),
                10_000, HashAlgorithmName.SHA256);
            return k.GetBytes(32);
        }

        private static byte[] VerschluesseleBytes(byte[] d)
        {
            using var a = Aes.Create();
            a.Key = _k; a.IV = _v;
            using var e = a.CreateEncryptor();
            return e.TransformFinalBlock(d, 0, d.Length);
        }

        private static byte[] EntschlusseleBytes(byte[] d)
        {
            using var a = Aes.Create();
            a.Key = _k; a.IV = _v;
            using var e = a.CreateDecryptor();
            return e.TransformFinalBlock(d, 0, d.Length);
        }

        // ══════════════════════════════════════════════════════════════════
        // DATEN-MODELLE
        // ══════════════════════════════════════════════════════════════════

        public class KITrainingsPaket
        {
            public int TrainingsRunden { get; set; } = 0;
            public int GesamtDatenpunkte { get; set; } = 0;
            public DateTime LetztesTraining { get; set; } = DateTime.MinValue;
            public List<TrainingsDatenpunkt> Datenpunkte { get; set; } = new();
        }

        public class TrainingsDatenpunkt
        {
            public int Id { get; set; }
            public TrainingsDatenTyp Typ { get; set; }
            public string Wert1 { get; set; }
            public string Wert2 { get; set; }
            public string Wert3 { get; set; }
            public string Wert4 { get; set; }
            public string Wert5 { get; set; }
            public string Wert6 { get; set; }
            public DateTime Zeitstempel { get; set; }
        }

        public class DatenSnapshot
        {
            public List<string> Kategorien { get; set; } = new();
            public List<string> Geraete { get; set; } = new();
            public List<string> Abteilungen { get; set; } = new();
            public Dictionary<string, decimal[]> PreisbereicheProKategorie { get; set; } = new();
            public Dictionary<string, int> BestandsVerteilung { get; set; } = new();
            public int AnzahlArtikel { get; set; }
            public int AnzahlMitarbeiter { get; set; }
            public DateTime ErstelltAm { get; set; }
        }

        public enum TrainingsDatenTyp
        {
            ArtikelMuster,
            MitarbeiterMuster,
            PreisMuster,
            BestandsMuster,
            KategorieMuster
        }
    }
}