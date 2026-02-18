using System;
using System.Collections.Generic;

namespace Inventarverwaltung
{
    /// <summary>
    /// Repräsentiert einen einzelnen Eintrag in der Zuweisungshistorie eines Artikels
    /// </summary>
    public class ZuweisungsEintrag
    {
        public string Aktion { get; set; }              // z.B. "Erstellt", "Zugewiesen", "Entfernt"
        public string MitarbeiterAlt { get; set; }      // Vorherige Zuweisung
        public string MitarbeiterNeu { get; set; }      // Neue Zuweisung
        public DateTime ZugewisenAm { get; set; }       // Wann zugewiesen
        public DateTime? ZugewiesenBis { get; set; }    // Bis wann (null = noch aktiv)
        public string AbteilungAlt { get; set; }        // Vorherige Abteilung
        public string AbteilungNeu { get; set; }        // Neue Abteilung
        public string Notizen { get; set; }             // Weitere Notizen
        public string ErfasstVon { get; set; }          // Wer hat den Eintrag gemacht

        public ZuweisungsEintrag(string aktion, string mitarbeiterAlt, string mitarbeiterNeu,
                                  DateTime zugewiesenAm, DateTime? zugewiesenBis,
                                  string abteilungAlt, string abteilungNeu,
                                  string notizen, string erfasstVon)
        {
            Aktion = aktion;
            MitarbeiterAlt = mitarbeiterAlt ?? "";
            MitarbeiterNeu = mitarbeiterNeu ?? "";
            ZugewisenAm = zugewiesenAm;
            ZugewiesenBis = zugewiesenBis;
            AbteilungAlt = abteilungAlt ?? "";
            AbteilungNeu = abteilungNeu ?? "";
            Notizen = notizen ?? "";
            ErfasstVon = erfasstVon ?? "System";
        }

        /// <summary>
        /// Serialisiert den Eintrag in eine Zeile für die Protokoll-Datei
        /// Format: AKTION|MitAlt|MitNeu|ZugewisenAm|ZugewiesenBis|AbtAlt|AbtNeu|Notizen|ErfasstVon
        /// </summary>
        public string ToFileString()
        {
            string bis = ZugewiesenBis.HasValue ? ZugewiesenBis.Value.ToString("dd.MM.yyyy") : "aktiv";
            return $"{Aktion}|{MitarbeiterAlt}|{MitarbeiterNeu}|{ZugewisenAm:dd.MM.yyyy HH:mm}|{bis}|{AbteilungAlt}|{AbteilungNeu}|{Notizen}|{ErfasstVon}";
        }

        /// <summary>
        /// Lädt einen Eintrag aus einer Datei-Zeile
        /// </summary>
        public static ZuweisungsEintrag FromFileString(string line)
        {
            string[] parts = line.Split('|');
            if (parts.Length < 9) return null;

            DateTime.TryParseExact(parts[3].Trim(), "dd.MM.yyyy HH:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime zugewiesenAm);

            DateTime? zugewiesenBis = null;
            if (parts[4].Trim() != "aktiv" &&
                DateTime.TryParseExact(parts[4].Trim(), "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime bis))
            {
                zugewiesenBis = bis;
            }

            return new ZuweisungsEintrag(
                parts[0].Trim(), parts[1].Trim(), parts[2].Trim(),
                zugewiesenAm, zugewiesenBis,
                parts[5].Trim(), parts[6].Trim(),
                parts[7].Trim(), parts[8].Trim()
            );
        }
    }

    /// <summary>
    /// Repräsentiert einen Inventar-Artikel mit vollständiger Bestandsführung
    /// ERWEITERT: Tracking, Rechnungsdatum, Garantie, Zuweisungshistorie
    /// </summary>
    public class InvId
    {
        // Basis-Informationen
        public string InvNmr { get; set; }
        public string GeraeteName { get; set; }
        public string MitarbeiterBezeichnung { get; set; }

        // Erweiterte Informationen
        public string SerienNummer { get; set; }
        public decimal Preis { get; set; }
        public DateTime Anschaffungsdatum { get; set; }
        public string Hersteller { get; set; }
        public string Kategorie { get; set; }

        // Bestandsführung
        public int Anzahl { get; set; }
        public int Mindestbestand { get; set; }

        // Tracking-Informationen
        public string ErstelltVon { get; set; }
        public DateTime ErstelltAm { get; set; }

        // Rechnungs- und Garantieinformationen
        public DateTime Rechnungsdatum { get; set; }
        public DateTime GarantieBis { get; set; }

        // Zuweisungshistorie (NEU!)
        public List<ZuweisungsEintrag> ZuweisungsHistorie { get; set; } = new List<ZuweisungsEintrag>();

        // Konstruktor: alle Felder inkl. Tracking + Rechnung/Garantie (14 Parameter)
        public InvId(string invNmr, string geraeteName, string mitarbeiterBezeichnung,
                     string serienNummer, decimal preis, DateTime anschaffungsdatum,
                     string hersteller, string kategorie, int anzahl, int mindestbestand,
                     string erstelltVon, DateTime erstelltAm,
                     DateTime rechnungsdatum, DateTime garantieBis)
        {
            InvNmr = invNmr; GeraeteName = geraeteName; MitarbeiterBezeichnung = mitarbeiterBezeichnung;
            SerienNummer = serienNummer; Preis = preis; Anschaffungsdatum = anschaffungsdatum;
            Hersteller = hersteller; Kategorie = kategorie; Anzahl = anzahl; Mindestbestand = mindestbestand;
            ErstelltVon = erstelltVon; ErstelltAm = erstelltAm;
            Rechnungsdatum = rechnungsdatum; GarantieBis = garantieBis;
        }

        // Konstruktor: mit Tracking, OHNE Rechnung/Garantie (Rückwärtskompatibilität)
        public InvId(string invNmr, string geraeteName, string mitarbeiterBezeichnung,
                     string serienNummer, decimal preis, DateTime anschaffungsdatum,
                     string hersteller, string kategorie, int anzahl, int mindestbestand,
                     string erstelltVon, DateTime erstelltAm)
        {
            InvNmr = invNmr; GeraeteName = geraeteName; MitarbeiterBezeichnung = mitarbeiterBezeichnung;
            SerienNummer = serienNummer; Preis = preis; Anschaffungsdatum = anschaffungsdatum;
            Hersteller = hersteller; Kategorie = kategorie; Anzahl = anzahl; Mindestbestand = mindestbestand;
            ErstelltVon = erstelltVon; ErstelltAm = erstelltAm;
            Rechnungsdatum = anschaffungsdatum; GarantieBis = anschaffungsdatum.AddYears(2);
        }

        // Konstruktor: OHNE Tracking (Rückwärtskompatibilität)
        public InvId(string invNmr, string geraeteName, string mitarbeiterBezeichnung,
                     string serienNummer, decimal preis, DateTime anschaffungsdatum,
                     string hersteller, string kategorie, int anzahl, int mindestbestand)
        {
            InvNmr = invNmr; GeraeteName = geraeteName; MitarbeiterBezeichnung = mitarbeiterBezeichnung;
            SerienNummer = serienNummer; Preis = preis; Anschaffungsdatum = anschaffungsdatum;
            Hersteller = hersteller; Kategorie = kategorie; Anzahl = anzahl; Mindestbestand = mindestbestand;
            ErstelltVon = "System"; ErstelltAm = DateTime.Now;
            Rechnungsdatum = anschaffungsdatum; GarantieBis = anschaffungsdatum.AddYears(2);
        }

        // Konstruktor: nur Basis-Felder (Rückwärtskompatibilität alte Dateien)
        public InvId(string invNmr, string geraeteName, string mitarbeiterBezeichnung)
        {
            InvNmr = invNmr; GeraeteName = geraeteName; MitarbeiterBezeichnung = mitarbeiterBezeichnung;
            SerienNummer = "N/A"; Preis = 0.00m; Anschaffungsdatum = DateTime.Now;
            Hersteller = "Unbekannt"; Kategorie = "Sonstiges"; Anzahl = 1; Mindestbestand = 1;
            ErstelltVon = "System (Migriert)"; ErstelltAm = DateTime.Now;
            Rechnungsdatum = DateTime.Now; GarantieBis = DateTime.Now.AddYears(2);
        }

        public BestandsStatus GetBestandsStatus()
        {
            if (Anzahl == 0) return BestandsStatus.Leer;
            else if (Anzahl <= Mindestbestand) return BestandsStatus.Niedrig;
            else if (Anzahl <= Mindestbestand * 2) return BestandsStatus.Mittel;
            else return BestandsStatus.Gut;
        }

        public string GetBestandsStatusText()
        {
            switch (GetBestandsStatus())
            {
                case BestandsStatus.Leer: return "🔴 LEER";
                case BestandsStatus.Niedrig: return "🟡 NIEDRIG";
                case BestandsStatus.Mittel: return "🟢 OK";
                case BestandsStatus.Gut: return "🟢 GUT";
                default: return "⚪ UNBEKANNT";
            }
        }
    }

    public enum BestandsStatus { Leer, Niedrig, Mittel, Gut }

    public class MID
    {
        public string VName { get; set; }
        public string NName { get; set; }
        public string Abteilung { get; set; }

        public MID(string vName, string nName, string abteilung)
        {
            VName = vName; NName = nName; Abteilung = abteilung;
        }
    }

    public enum Berechtigungen { User, Admin }

    /// <summary>
    /// Repräsentiert einen Benutzer-Account
    /// ERWEITERT: PasswortHash und HatKeinPasswort
    /// </summary>
    public class Accounts
    {
        public string Benutzername { get; set; }
        public Berechtigungen Berechtigung { get; set; }
        public string PasswortHash { get; set; }
        public bool HatKeinPasswort { get; set; }

        public Accounts(string benutzername, Berechtigungen berechtigung)
        {
            Benutzername = benutzername;
            Berechtigung = berechtigung;
            PasswortHash = string.Empty;
            HatKeinPasswort = true;
        }

        public Accounts(string benutzername, Berechtigungen berechtigung, string passwortHash)
        {
            Benutzername = benutzername;
            Berechtigung = berechtigung;
            PasswortHash = passwortHash ?? string.Empty;
            HatKeinPasswort = string.IsNullOrEmpty(PasswortHash);
        }
    }

    public class Anmelder
    {
        public string Anmeldename { get; set; }
        public Anmelder(string anmeldename) 
        { 
            Anmeldename = anmeldename; 
        }
    }
}